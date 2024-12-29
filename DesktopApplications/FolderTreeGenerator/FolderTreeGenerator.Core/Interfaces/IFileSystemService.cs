// FolderTreeGenerator.Core/Interfaces/IFileSystemService.cs
// purpose: Define the IFileSystemService interface

using System.IO;

namespace FolderTreeGenerator.Core.Interfaces
{
    public interface IFileSystemService
    {
        Task<bool> DirectoryExistsAsync(string path);
        Task<string[]> GetFilesAsync(string path, CancellationToken cancellationToken = default);
        Task<string[]> GetDirectoriesAsync(string path, CancellationToken cancellationToken = default);
        Task<FileInfo> GetFileInfoAsync(string path);
        Task<DirectoryInfo> GetDirectoryInfoAsync(string path);
        Task<string> ReadAllTextAsync(string path);
        Task WriteAllTextAsync(string path, string content);
        Task<bool> IsHiddenAsync(string path);
        Task<string?> FindGitignoreAsync(string startPath);
    }
}