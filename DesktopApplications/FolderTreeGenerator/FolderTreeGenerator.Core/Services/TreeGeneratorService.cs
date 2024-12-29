using System.IO;
using System.Text;
using System.Text.Json;
using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.Core.Services;

public class TreeGeneratorService : ITreeGeneratorService
{
    private readonly IFileSystemService _fileSystem;
    private readonly IFilterService _filter;

    public TreeGeneratorService(IFileSystemService fileSystem, IFilterService filter)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public async Task<TreeNode> GenerateTreeAsync(
        string rootPath,
        FilterOptions filterOptions,
        GeneratorOptions generatorOptions,
        IProgress<(int current, int total, string currentItem)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rootPath);
        ArgumentNullException.ThrowIfNull(filterOptions);

        // Check if directory exists
        if (!await _fileSystem.DirectoryExistsAsync(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
        }

        // Load gitignore if enabled
        if (filterOptions.UseGitignore)
        {
            var gitignorePath = await _fileSystem.FindGitignoreAsync(rootPath);
            if (gitignorePath != null)
            {
                await _filter.LoadGitignoreAsync(gitignorePath);
            }
        }

        try
        {
            var rootInfo = await _fileSystem.GetDirectoryInfoAsync(rootPath);
            var root = new TreeNode
            {
                Name = rootInfo.Name,
                FullPath = rootInfo.FullName,
                IsFile = false,
                Depth = 0,
                LastModified = rootInfo.LastWriteTime
            };

            await PopulateTreeAsync(root, rootPath, filterOptions, progress, cancellationToken);
            return root;
        }
        finally
        {
            if (filterOptions.UseGitignore)
            {
                _filter.ClearGitignoreRules();
            }
        }
    }

    private async Task PopulateTreeAsync(
        TreeNode node,
        string rootPath,
        FilterOptions options,
        IProgress<(int current, int total, string currentItem)>? progress,
        CancellationToken cancellationToken)
    {
        // Check depth limit
        if (!await _filter.MatchesDepthFilterAsync(node.FullPath, rootPath, options))
            return;

        var files = await _fileSystem.GetFilesAsync(node.FullPath, cancellationToken);
        var directories = await _fileSystem.GetDirectoriesAsync(node.FullPath, cancellationToken);

        var total = files.Length + directories.Length;
        var current = 0;

        // Process files
        foreach (var filePath in files.OrderBy(f => f))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((++current, total, filePath));

            if (!await _filter.ShouldIncludeFileAsync(filePath, options))
                continue;

            var fileInfo = await _fileSystem.GetFileInfoAsync(filePath);
            node.AddChild(new TreeNode
            {
                Name = fileInfo.Name,
                FullPath = fileInfo.FullName,
                IsFile = true,
                Depth = node.Depth + 1,
                LastModified = fileInfo.LastWriteTime,
                Size = fileInfo.Length
            });
        }

        // Process directories
        foreach (var dirPath in directories.OrderBy(d => d))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((++current, total, dirPath));

            if (!await _filter.ShouldIncludeFolderAsync(dirPath, options))
                continue;

            var dirInfo = await _fileSystem.GetDirectoryInfoAsync(dirPath);
            var childNode = new TreeNode
            {
                Name = dirInfo.Name,
                FullPath = dirInfo.FullName,
                IsFile = false,
                Depth = node.Depth + 1,
                LastModified = dirInfo.LastWriteTime
            };

            await PopulateTreeAsync(childNode, rootPath, options, progress, cancellationToken);

            if (options.IncludeEmpty || childNode.Children.Any())
                node.AddChild(childNode);
        }
    }

    public Task<string> GenerateOutputAsync(
        TreeNode tree,
        GeneratorOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(options);

        var output = options.Format switch
        {
            OutputFormat.PlainText => GeneratePlainText(tree, options),
            OutputFormat.Markdown => GenerateMarkdown(tree, options),
            OutputFormat.Json => GenerateJson(tree),
            OutputFormat.Xml => GenerateXml(tree),
            _ => throw new ArgumentException($"Unsupported format: {options.Format}")
        };

        return Task.FromResult(output);
    }

    public async Task ExportAsync(
        TreeNode tree,
        string outputPath,
        GeneratorOptions options,
        CancellationToken cancellationToken = default)
    {
        var content = await GenerateOutputAsync(tree, options, cancellationToken);
        await _fileSystem.WriteAllTextAsync(outputPath, content);
    }

    private static string GeneratePlainText(TreeNode node, GeneratorOptions options, string prefix = "")
    {
        var sb = new StringBuilder();
        if (string.IsNullOrEmpty(prefix))
        {
            sb.AppendLine(node.Name);
            sb.AppendLine("│");
        }

        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var isLast = i == node.Children.Count - 1;
            var currentPrefix = isLast ? "└── " : "├── ";

            sb.Append(prefix)
              .Append(currentPrefix)
              .Append(child.Name);

            if (options.IncludeMetadata)
            {
                if (child.IsFile && child.Size.HasValue)
                    sb.Append($" ({FormatSize(child.Size.Value)})");
                sb.Append($" [{child.LastModified.ToString(options.DateFormat)}]");
            }

            sb.AppendLine();

            if (!child.IsFile)
            {
                var newPrefix = isLast ? prefix + "    " : prefix + "│   ";
                sb.Append(GeneratePlainText(child, options, newPrefix));
            }
        }

        return sb.ToString();
    }

    private static string GenerateMarkdown(TreeNode node, GeneratorOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {node.Name}");
        GenerateMarkdownLines(node, "", sb, options);
        return sb.ToString();
    }

    private static void GenerateMarkdownLines(TreeNode node, string indent, StringBuilder sb, GeneratorOptions options)
    {
        foreach (var child in node.Children)
        {
            var bullet = child.IsFile ? "-" : "*";
            sb.Append($"{indent}{bullet} {child.Name}");

            if (options.IncludeMetadata)
            {
                if (child.IsFile && child.Size.HasValue)
                    sb.Append($" ({FormatSize(child.Size.Value)})");
                sb.Append($" *{child.LastModified.ToString(options.DateFormat)}*");
            }

            sb.AppendLine();

            if (!child.IsFile)
                GenerateMarkdownLines(child, indent + "  ", sb, options);
        }
    }

    private static string GenerateJson(TreeNode tree)
    {
        return JsonSerializer.Serialize(tree, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string GenerateXml(TreeNode tree)
    {
        throw new NotImplementedException("XML output format is not yet implemented.");
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}