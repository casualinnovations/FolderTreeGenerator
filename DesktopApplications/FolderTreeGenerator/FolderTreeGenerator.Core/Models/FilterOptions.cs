// FolderTreeGenerator.Core/Models/FilterOptions.cs
// Purpose: Model for filter options used to filter files and folders.

namespace FolderTreeGenerator.Core.Models
{
    public class FilterOptions
    {
        public int? MaxDepth { get; set; }
        public HashSet<string> IncludeExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ExcludeExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ExcludedFolders { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public bool UseGitignore { get; set; }
        public bool ShowHiddenFiles { get; set; }
        public bool IncludeEmpty { get; set; } = true;  // Default to true

        public FilterOptions Clone() => new()
        {
            MaxDepth = MaxDepth,
            UseGitignore = UseGitignore,
            ShowHiddenFiles = ShowHiddenFiles,
            IncludeEmpty = IncludeEmpty,
            IncludeExtensions = new HashSet<string>(IncludeExtensions, StringComparer.OrdinalIgnoreCase),
            ExcludeExtensions = new HashSet<string>(ExcludeExtensions, StringComparer.OrdinalIgnoreCase),
            ExcludedFolders = new HashSet<string>(ExcludedFolders, StringComparer.OrdinalIgnoreCase)
        };
    }
}