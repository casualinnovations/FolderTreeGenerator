// FilterSettingsViewModel.cs
// purpose: Provide the view model for the FilterSettingsWindow view
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.UI.ViewModels;

public class FilterSettingsViewModel : INotifyPropertyChanged
{
    private readonly IFilterService _filterService;
    private readonly IFileSystemService _fileSystem;
    private readonly IConfigurationService _configService;
    private int? _maxDepth;
    private bool _useGitignore;
    private bool _showHiddenFiles;
    private bool _includeEmpty = true;
    private string _newExtension = string.Empty;
    private string _newExcludedFolder = string.Empty;

    public FilterSettingsViewModel(
        IFilterService filterService,
        IFileSystemService fileSystem,
        IConfigurationService configService)
    {
        _filterService = filterService;
        _fileSystem = fileSystem;
        _configService = configService;

        // Initialize collections
        IncludeExtensions = new ObservableCollection<string>();
        ExcludeExtensions = new ObservableCollection<string>();
        ExcludedFolders = new ObservableCollection<string>();

        // Initialize commands
        SaveCommand = new RelayCommand(_ => SaveSettings());
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
        ClearFiltersCommand = new RelayCommand(_ => ClearAllFilters());
        ResetToDefaultsCommand = new RelayCommand(async _ => await ResetToDefaults());
        AddExtensionCommand = new RelayCommand(_ => AddExtension(), _ => !string.IsNullOrWhiteSpace(NewExtension));
        RemoveExtensionCommand = new RelayCommand(extension => RemoveExtension((string)extension!));
        AddExcludedFolderCommand = new RelayCommand(_ => AddExcludedFolder(), _ => !string.IsNullOrWhiteSpace(NewExcludedFolder));
        RemoveExcludedFolderCommand = new RelayCommand(folder => RemoveExcludedFolder((string)folder!));

        // Load settings or defaults
        LoadSettingsAsync().FireAndForgetSafeAsync();
    }
    public ICommand SaveCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand CloseCommand { get; }

    public event Action? RequestClose;
    private async Task ResetToDefaults()
    {
        await _configService.ResetToDefaultsAsync();
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _configService.LoadSettingsAsync();
            var filterOptions = settings.FilterOptions;

            // Clear collections before loading
            IncludeExtensions.Clear();
            ExcludeExtensions.Clear();
            ExcludedFolders.Clear();

            // Load Include Extensions with defaults if empty
            if (!filterOptions.IncludeExtensions.Any())
            {
                foreach (var ext in new[] { ".txt", ".md", ".cs" })
                {
                    IncludeExtensions.Add(ext);
                }
            }
            else
            {
                foreach (var ext in filterOptions.IncludeExtensions)
                {
                    IncludeExtensions.Add(ext);
                }
            }

            // Load Exclude Extensions
            foreach (var ext in filterOptions.ExcludeExtensions)
            {
                ExcludeExtensions.Add(ext);
            }

            // Load Excluded Folders with defaults if empty
            if (!filterOptions.ExcludedFolders.Any())
            {
                foreach (var folder in new[] { "bin", "obj", "node_modules" })
                {
                    ExcludedFolders.Add(folder);
                }
            }
            else
            {
                foreach (var folder in filterOptions.ExcludedFolders)
                {
                    ExcludedFolders.Add(folder);
                }
            }

            // Set properties
            MaxDepth = filterOptions.MaxDepth;
            UseGitignore = filterOptions.UseGitignore;
            ShowHiddenFiles = filterOptions.ShowHiddenFiles;
            IncludeEmpty = filterOptions.IncludeEmpty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading filter settings: {ex}");
            LoadDefaults();
        }
    }

    private void SaveSettings()
    {
        SaveSettingsAsync().FireAndForgetSafeAsync();
    }

    private void ClearAllFilters()
    {
        IncludeExtensions.Clear();
        ExcludeExtensions.Clear();
        ExcludedFolders.Clear();

        MaxDepth = null;
        UseGitignore = false;
        ShowHiddenFiles = false;
        IncludeEmpty = true;
        
        SaveSettingsAsync().FireAndForgetSafeAsync();
    }

    private void LoadDefaults()
    {
        IncludeExtensions.Clear();
        IncludeExtensions.Add(".txt");
        IncludeExtensions.Add(".md");
        IncludeExtensions.Add(".json");
        IncludeExtensions.Add(".xml");
        IncludeExtensions.Add(".yml");
        IncludeExtensions.Add(".yaml");

        ExcludeExtensions.Clear();

        ExcludedFolders.Clear();
        ExcludedFolders.Add("bin");
        ExcludedFolders.Add("obj");

        MaxDepth = null;
        UseGitignore = false;
        ShowHiddenFiles = false;
        IncludeEmpty = true;
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            var settings = await _configService.LoadSettingsAsync();
            settings.FilterOptions = GetFilterOptions();
            await _configService.SaveSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving filter settings: {ex}");
        }
    }

    private void AddExtension()
    {
        var ext = NewExtension.Trim().ToLowerInvariant();
        if (!ext.StartsWith(".")) ext = "." + ext;

        if (!IncludeExtensions.Contains(ext))
        {
            IncludeExtensions.Add(ext);
            SaveSettingsAsync().FireAndForgetSafeAsync();
        }

        NewExtension = string.Empty;
    }

    public int? MaxDepth
    {
        get => _maxDepth;
        set
        {
            if (SetProperty(ref _maxDepth, value))
            {
                SaveSettingsAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public bool UseGitignore
    {
        get => _useGitignore;
        set
        {
            if (SetProperty(ref _useGitignore, value))
            {
                SaveSettingsAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public bool ShowHiddenFiles
    {
        get => _showHiddenFiles;
        set
        {
            if (SetProperty(ref _showHiddenFiles, value))
            {
                SaveSettingsAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public bool IncludeEmpty
    {
        get => _includeEmpty;
        set
        {
            if (SetProperty(ref _includeEmpty, value))
            {
                SaveSettingsAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public string NewExtension
    {
        get => _newExtension;
        set
        {
            if (SetProperty(ref _newExtension, value))
            {
                SaveSettingsAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public string NewExcludedFolder
    {
        get => _newExcludedFolder;
        set
        {
            if (SetProperty(ref _newExcludedFolder, value))
            {
                SaveSettingsAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public ObservableCollection<string> IncludeExtensions { get; }
    public ObservableCollection<string> ExcludeExtensions { get; }
    public ObservableCollection<string> ExcludedFolders { get; }

    public ICommand AddExtensionCommand { get; }
    public ICommand RemoveExtensionCommand { get; }
    public ICommand AddExcludedFolderCommand { get; }
    public ICommand RemoveExcludedFolderCommand { get; }

    private void RemoveExtension(string extension)
    {
        IncludeExtensions.Remove(extension);
    }

    private void AddExcludedFolder()
    {
        var folder = NewExcludedFolder.Trim();
        if (!ExcludedFolders.Contains(folder))
        {
            ExcludedFolders.Add(folder);
        }

        NewExcludedFolder = string.Empty;
    }

    private void RemoveExcludedFolder(string folder)
    {
        ExcludedFolders.Remove(folder);
    }

    public FilterOptions GetFilterOptions()
    {
        return new FilterOptions
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}