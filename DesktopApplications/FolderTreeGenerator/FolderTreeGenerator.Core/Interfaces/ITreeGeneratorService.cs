// FolderTreeGenerator.Core/Interfaces/ITreeGeneratorService.cs
// Purpose: Define the ITreeGeneratorService interface

using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.Core.Interfaces
{
    public interface ITreeGeneratorService
    {
        Task<TreeNode> GenerateTreeAsync(
            string rootPath,
            FilterOptions filterOptions,
            GeneratorOptions generatorOptions,
            IProgress<(int current, int total, string currentItem)>? progress = null,
            CancellationToken cancellationToken = default);

        Task<string> GenerateOutputAsync(
            TreeNode tree,
            GeneratorOptions options,
            CancellationToken cancellationToken = default);

        Task ExportAsync(
            TreeNode tree,
            string outputPath,
            GeneratorOptions options,
            CancellationToken cancellationToken = default);
    }
}