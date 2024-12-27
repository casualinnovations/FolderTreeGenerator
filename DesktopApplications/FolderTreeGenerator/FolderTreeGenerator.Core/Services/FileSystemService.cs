using FolderTreeGenerator.Core.Interfaces;
using System.IO;

namespace FolderTreeGenerator.Core.Services;

public class FileSystemService : IFileSystemService
{
    public async Task<bool> DirectoryExistsAsync(string path)
    {
        return await Task.Run(() => Directory.Exists(path));
    }

    public async Task<string[]> GetDirectoriesAsync(string path, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Directory.GetDirectories(path), cancellationToken);
    }

    public async Task<DirectoryInfo> GetDirectoryInfoAsync(string path)
    {
        return await Task.Run(() => new DirectoryInfo(path));
    }

    public async Task<FileInfo> GetFileInfoAsync(string path)
    {
        return await Task.Run(() => new FileInfo(path));
    }

    public async Task<string[]> GetFilesAsync(string path, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Directory.GetFiles(path), cancellationToken);
    }

    public async Task<bool> IsHiddenAsync(string path)
    {
        return await Task.Run(() =>
        {
            var attr = File.GetAttributes(path);
            return (attr & FileAttributes.Hidden) == FileAttributes.Hidden;
        });
    }

    public async Task<string> ReadAllTextAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteAllTextAsync(string path, string content)
    {
        await File.WriteAllTextAsync(path, content);
    }

    public async Task<string?> FindGitignoreAsync(string startPath)
    {
        var currentPath = startPath;
        while (!string.IsNullOrEmpty(currentPath))
        {
            var gitignorePath = Path.Combine(currentPath, ".gitignore");
            if (await Task.Run(() => File.Exists(gitignorePath)))
            {
                return gitignorePath;
            }

            currentPath = Path.GetDirectoryName(currentPath);
        }

        return null;
    }
}