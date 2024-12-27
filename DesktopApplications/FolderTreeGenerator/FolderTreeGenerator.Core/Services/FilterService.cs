using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace FolderTreeGenerator.Core.Services;

public class FilterService : IFilterService
{
    private readonly GitignoreParser _gitignoreParser = new();
    private readonly IFileSystemService _fileSystem;

    public FilterService(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task LoadGitignoreAsync(string path)
    {
        _gitignoreParser.Clear();
        var content = await _fileSystem.ReadAllTextAsync(path);
        foreach (var line in content.Split('\n'))
        {
            _gitignoreParser.AddPattern(line);
        }
    }

    public void ClearGitignoreRules()
    {
        _gitignoreParser.Clear();
    }

    public async Task<bool> ShouldInclude(string path, FilterOptions options)
    {
        if (options == null) return true;

        if (!options.ShowHiddenFiles && await _fileSystem.IsHiddenAsync(path))
            return false;

        if (options.UseGitignore && await IsExcludedByGitignoreAsync(path))
            return false;

        if (Directory.Exists(path))
        {
            var dirName = Path.GetFileName(path);
            if (options.ExcludedFolders.Contains(dirName))
                return false;
        }
        else
        {
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            if (extension != null)
            {
                if (options.IncludeExtensions.Any() && !options.IncludeExtensions.Contains(extension))
                    return false;

                if (options.ExcludeExtensions.Contains(extension))
                    return false;
            }
        }

        return true;
    }

    public async Task<bool> IsExcludedByGitignoreAsync(string path)
    {
        return _gitignoreParser.IsIgnored(path);
    }

    public async Task<bool> MatchesExtensionFilters(string path, FilterOptions options)
    {
        var extension = Path.GetExtension(path)?.ToLowerInvariant();
        if (extension == null) return false;

        if (options.IncludeExtensions.Any())
            return options.IncludeExtensions.Contains(extension);

        return !options.ExcludeExtensions.Contains(extension);
    }

    public async Task<bool> MatchesDepthFilter(string path, string rootPath, FilterOptions options)
    {
        if (!options.MaxDepth.HasValue) return true;

        var relativePath = Path.GetRelativePath(rootPath, path);
        var depth = relativePath.Count(c => c == Path.DirectorySeparatorChar);
        return depth <= options.MaxDepth.Value;
    }
}