// FolderTreeGenerator.Core/Services/FilterService.cs
// purpose: Provide services for filtering files and folders based on various criteria

using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;
using System.IO;
using System.Linq;

namespace FolderTreeGenerator.Core.Services
{

    public class FilterService : IFilterService
    {
        private readonly GitignoreParser _gitignoreParser = new();
        private readonly IFileSystemService _fileSystem;

        public FilterService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Loads gitignore rules from the specified path.
        /// </summary>
        public async Task LoadGitignoreAsync(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            _gitignoreParser.Clear();
            var content = await _fileSystem.ReadAllTextAsync(path);

            foreach (var line in content.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _gitignoreParser.AddPattern(line.Trim());
                }
            }
        }

        /// <summary>
        /// Clears all loaded gitignore rules.
        /// </summary>
        public void ClearGitignoreRules()
        {
            _gitignoreParser.Clear();
        }

        /// <summary>
        /// Determines whether a file should be included based on filter options.
        /// </summary>
        public async Task<bool> ShouldIncludeFileAsync(string path, FilterOptions options)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(options);

            // Check if path is hidden
            if (!options.ShowHiddenFiles && await _fileSystem.IsHiddenAsync(path))
            {
                return false;
            }

            // Check gitignore rules
            if (options.UseGitignore && _gitignoreParser.IsIgnored(path))
            {
                return false;
            }

            // Check extension filters
            var extension = Path.GetExtension(path).ToLowerInvariant();

            // If include extensions are specified, only include files with those extensions
            if (options.IncludeExtensions?.Any() == true)
            {
                return options.IncludeExtensions.Contains(extension);
            }

            // If exclude extensions are specified, exclude files with those extensions
            if (options.ExcludeExtensions?.Any() == true)
            {
                return !options.ExcludeExtensions.Contains(extension);
            }

            return true;
        }

        /// <summary>
        /// Determines whether a folder should be included based on filter options.
        /// </summary>
        public async Task<bool> ShouldIncludeFolderAsync(string path, FilterOptions options)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(options);

            // Check if path is hidden
            if (!options.ShowHiddenFiles && await _fileSystem.IsHiddenAsync(path))
            {
                return false;
            }

            // Check gitignore rules
            if (options.UseGitignore && _gitignoreParser.IsIgnored(path))
            {
                return false;
            }

            // Check excluded folders
            var folderName = Path.GetFileName(path);
            return !options.ExcludedFolders?.Contains(folderName) ?? true;
        }

        /// <summary>
        /// Checks if a path matches the maximum depth filter.
        /// </summary>
        public Task<bool> MatchesDepthFilterAsync(string path, string rootPath, FilterOptions options)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(rootPath);
            ArgumentNullException.ThrowIfNull(options);

            if (!options.MaxDepth.HasValue)
            {
                return Task.FromResult(true);
            }

            var relativePath = Path.GetRelativePath(rootPath, path);
            var depth = relativePath.Count(c => c == Path.DirectorySeparatorChar);
            return Task.FromResult(depth <= options.MaxDepth.Value);
        }

        /// <summary>
        /// Checks if a path is excluded by gitignore rules.
        /// </summary>
        public Task<bool> IsExcludedByGitignoreAsync(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            return Task.FromResult(_gitignoreParser.IsIgnored(path));
        }

        /// <summary>
        /// Checks if a file extension matches the extension filters.
        /// </summary>
        public Task<bool> MatchesExtensionFiltersAsync(string path, FilterOptions options)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(options);

            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return Task.FromResult(false);
            }

            // If include extensions are specified, only include files with those extensions
            if (options.IncludeExtensions?.Any() == true)
            {
                return Task.FromResult(options.IncludeExtensions.Contains(extension));
            }

            // If exclude extensions are specified, exclude files with those extensions
            if (options.ExcludeExtensions?.Any() == true)
            {
                return Task.FromResult(!options.ExcludeExtensions.Contains(extension));
            }

            return Task.FromResult(true);
        }
    }
}