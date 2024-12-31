// FolderTreeGenerator.Core/Interfaces/IConfigurationService.cs
using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.Core.Interfaces
{
    public interface IConfigurationService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
        Task ResetToDefaultsAsync();
    }
}
