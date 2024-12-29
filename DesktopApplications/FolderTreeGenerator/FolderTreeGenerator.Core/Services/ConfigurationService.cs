// FolderTreeGenerator.Core/Services/ConfigurationService.cs
// Purpose: Service for loading and saving application settings.

using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;
using System.IO;
using System.Text.Json;

namespace FolderTreeGenerator.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configPath;

        public ConfigurationService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "FolderTreeGenerator");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "settings.json");
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            if (!File.Exists(_configPath))
            {
                return new AppSettings();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                return settings;
            }
            catch (Exception)
            {
                return new AppSettings();
            }
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}