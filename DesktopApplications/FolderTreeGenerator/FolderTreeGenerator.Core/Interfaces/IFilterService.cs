using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.Core.Interfaces;

public interface IFilterService
{
    Task LoadGitignoreAsync(string path);
    Task<bool> ShouldInclude(string path, FilterOptions options);
    void ClearGitignoreRules();
    Task<bool> IsExcludedByGitignoreAsync(string path);
    Task<bool> MatchesExtensionFilters(string path, FilterOptions options);
    Task<bool> MatchesDepthFilter(string path, string rootPath, FilterOptions options);
}