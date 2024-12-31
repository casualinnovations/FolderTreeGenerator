// ConfigurationService.cs
using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly string _defaultConfigPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationService()
    {
        // Get the application's base directory
        var appFolder = AppDomain.CurrentDomain.BaseDirectory;
        var configFolder = Path.Combine(appFolder, "config");

        // Ensure config directory exists
        Directory.CreateDirectory(configFolder);

        _configPath = Path.Combine(configFolder, "settings.json");
        _defaultConfigPath = Path.Combine(configFolder, "default-settings.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        // Create default settings file if it doesn't exist
        if (!File.Exists(_defaultConfigPath))
        {
            SaveDefaultSettingsAsync().Wait();
        }
    }

    private async Task SaveDefaultSettingsAsync()
    {
        try
        {
            var defaults = DefaultSettings.CreateDefault();
            var json = JsonSerializer.Serialize(defaults, _jsonOptions);
            await File.WriteAllTextAsync(_defaultConfigPath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving default settings: {ex}");
            throw; // Rethrow as this is a critical initialization error
        }
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                return settings ?? await LoadDefaultSettingsAsync();
            }

            return await LoadDefaultSettingsAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading settings: {ex}");
            return await LoadDefaultSettingsAsync();
        }
    }

    private async Task<AppSettings> LoadDefaultSettingsAsync()
    {
        try
        {
            if (File.Exists(_defaultConfigPath))
            {
                var json = await File.ReadAllTextAsync(_defaultConfigPath);
                var defaults = JsonSerializer.Deserialize<DefaultSettings>(json, _jsonOptions);
                if (defaults != null)
                {
                    return new AppSettings
                    {
                        FilterOptions = defaults.Filters,
                        LastDirectory = null
                    };
                }
            }

            // If default settings file doesn't exist or is invalid, 
            // create new defaults from code
            var defaultSettings = DefaultSettings.CreateDefault();
            return new AppSettings
            {
                FilterOptions = defaultSettings.Filters,
                LastDirectory = null
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading default settings: {ex}");
            // Fall back to hardcoded defaults
            return new AppSettings
            {
                FilterOptions = DefaultSettings.CreateDefault().Filters
            };
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            // Write to temporary file first to prevent corruption
            var tempPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, _configPath, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex}");
            throw;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                File.Delete(_configPath);
            }
            await SaveSettingsAsync(await LoadDefaultSettingsAsync());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error resetting settings: {ex}");
            throw;
        }
    }
}