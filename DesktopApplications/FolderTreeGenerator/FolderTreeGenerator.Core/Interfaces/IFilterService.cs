// FolderTreeGenerator.Core/Interfaces/IFilterService.cs
// Purpose: Interface for the filter service used to filter files and folders.

using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.Core.Interfaces
{
    public interface IFilterService
    {
        Task LoadGitignoreAsync(string path);
        void ClearGitignoreRules();
        Task<bool> ShouldIncludeFileAsync(string path, FilterOptions options);
        Task<bool> ShouldIncludeFolderAsync(string path, FilterOptions options);
        Task<bool> MatchesDepthFilterAsync(string path, string rootPath, FilterOptions options);
        Task<bool> IsExcludedByGitignoreAsync(string path);
        Task<bool> MatchesExtensionFiltersAsync(string path, FilterOptions options);
    }
}