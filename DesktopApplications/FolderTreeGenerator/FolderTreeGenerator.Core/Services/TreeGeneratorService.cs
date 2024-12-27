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
        _fileSystem = fileSystem;
        _filter = filter;
    }

    public async Task<TreeNode> GenerateTreeAsync(
        string rootPath,
        FilterOptions filterOptions,
        GeneratorOptions generatorOptions,
        IProgress<(int current, int total, string currentItem)>? progress = null,
        CancellationToken cancellationToken = default)
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

        await PopulateTreeAsync(root, filterOptions, progress, cancellationToken);
        return root;
    }

    private async Task PopulateTreeAsync(
        TreeNode node,
        FilterOptions options,
        IProgress<(int current, int total, string currentItem)>? progress,
        CancellationToken cancellationToken)
    {
        if (options.MaxDepth.HasValue && node.Depth >= options.MaxDepth.Value)
            return;

        var files = await _fileSystem.GetFilesAsync(node.FullPath, cancellationToken);
        var directories = await _fileSystem.GetDirectoriesAsync(node.FullPath, cancellationToken);

        var total = files.Length + directories.Length;
        var current = 0;

        // Process files
        foreach (var file in files.OrderBy(f => f))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((++current, total, file));

            if (!await _filter.ShouldInclude(file, options))
                continue;

            var fileInfo = await _fileSystem.GetFileInfoAsync(file);
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
        foreach (var dir in directories.OrderBy(d => d))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((++current, total, dir));

            if (!await _filter.ShouldInclude(dir, options))
                continue;

            var dirInfo = await _fileSystem.GetDirectoryInfoAsync(dir);
            var childNode = new TreeNode
            {
                Name = dirInfo.Name,
                FullPath = dirInfo.FullName,
                IsFile = false,
                Depth = node.Depth + 1,
                LastModified = dirInfo.LastWriteTime
            };

            await PopulateTreeAsync(childNode, options, progress, cancellationToken);

            if (options.IncludeEmpty || childNode.Children.Any())
                node.AddChild(childNode);
        }
    }

    public async Task<string> GenerateOutputAsync(
        TreeNode tree,
        GeneratorOptions options,
        CancellationToken cancellationToken = default)
    {
        return options.Format switch
        {
            OutputFormat.PlainText => GeneratePlainText(tree, options),
            OutputFormat.Markdown => GenerateMarkdown(tree, options),
            OutputFormat.Json => GenerateJson(tree, options),
            OutputFormat.Xml => GenerateXml(tree, options),
            OutputFormat.Html => await GenerateHtmlAsync(tree, options),
            _ => throw new ArgumentException($"Unsupported format: {options.Format}")
        };
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

    private string GeneratePlainText(TreeNode node, GeneratorOptions options, string prefix = "")
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

            sb.Append(prefix).Append(currentPrefix).Append(child.Name);

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

    private string GenerateMarkdown(TreeNode node, GeneratorOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {node.Name}");
        GenerateMarkdownLines(node, "", sb, options);
        return sb.ToString();
    }

    private void GenerateMarkdownLines(TreeNode node, string indent, StringBuilder sb, GeneratorOptions options)
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

    private string GenerateJson(TreeNode tree, GeneratorOptions options)
    {
        return JsonSerializer.Serialize(tree, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private string GenerateXml(TreeNode tree, GeneratorOptions options)
    {
        // Implementation omitted for brevity
        throw new NotImplementedException();
    }

    private async Task<string> GenerateHtmlAsync(TreeNode tree, GeneratorOptions options)
    {
        // Implementation omitted for brevity
        throw new NotImplementedException();
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