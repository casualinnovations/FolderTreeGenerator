using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;

namespace FolderTreeGenerator.UI.ViewModels;

public class FilterSettingsViewModel : INotifyPropertyChanged
{
    private readonly IFilterService _filterService;
    private readonly IFileSystemService _fileSystem;
    private int? _maxDepth;
    private bool _useGitignore;
    private bool _showHiddenFiles;
    private bool _includeEmpty = true;  // Default to true to show empty folders
    private string _newExtension = string.Empty;
    private string _newExcludedFolder = string.Empty;

    public FilterSettingsViewModel(IFilterService filterService, IFileSystemService fileSystem)
    {
        _filterService = filterService;
        _fileSystem = fileSystem;

        // Initialize collections
        IncludeExtensions = new ObservableCollection<string>();
        ExcludeExtensions = new ObservableCollection<string>();
        ExcludedFolders = new ObservableCollection<string> { "bin", "obj", "node_modules" };

        // Add default extensions to include
        IncludeExtensions.Add(".txt");
        IncludeExtensions.Add(".md");
        IncludeExtensions.Add(".cs");

        AddExtensionCommand = new RelayCommand(_ => AddExtension(), _ => !string.IsNullOrWhiteSpace(NewExtension));
        RemoveExtensionCommand = new RelayCommand(extension => RemoveExtension((string)extension!));
        AddExcludedFolderCommand = new RelayCommand(_ => AddExcludedFolder(), _ => !string.IsNullOrWhiteSpace(NewExcludedFolder));
        RemoveExcludedFolderCommand = new RelayCommand(folder => RemoveExcludedFolder((string)folder!));
    }

    public int? MaxDepth
    {
        get => _maxDepth;
        set => SetProperty(ref _maxDepth, value);
    }

    public bool UseGitignore
    {
        get => _useGitignore;
        set => SetProperty(ref _useGitignore, value);
    }

    public bool ShowHiddenFiles
    {
        get => _showHiddenFiles;
        set => SetProperty(ref _showHiddenFiles, value);
    }

    public bool IncludeEmpty
    {
        get => _includeEmpty;
        set => SetProperty(ref _includeEmpty, value);
    }

    public string NewExtension
    {
        get => _newExtension;
        set => SetProperty(ref _newExtension, value);
    }

    public string NewExcludedFolder
    {
        get => _newExcludedFolder;
        set => SetProperty(ref _newExcludedFolder, value);
    }

    public ObservableCollection<string> IncludeExtensions { get; }
    public ObservableCollection<string> ExcludeExtensions { get; }
    public ObservableCollection<string> ExcludedFolders { get; }

    public ICommand AddExtensionCommand { get; }
    public ICommand RemoveExtensionCommand { get; }
    public ICommand AddExcludedFolderCommand { get; }
    public ICommand RemoveExcludedFolderCommand { get; }

    private void AddExtension()
    {
        var ext = NewExtension.Trim().ToLowerInvariant();
        if (!ext.StartsWith(".")) ext = "." + ext;

        if (!IncludeExtensions.Contains(ext))
        {
            IncludeExtensions.Add(ext);
        }

        NewExtension = string.Empty;
    }

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