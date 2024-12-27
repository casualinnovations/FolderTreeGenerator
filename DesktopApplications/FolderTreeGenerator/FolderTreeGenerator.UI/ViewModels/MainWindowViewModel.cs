using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.IO;
using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Models;
using Microsoft.Win32;

namespace FolderTreeGenerator.UI.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ITreeGeneratorService _treeGenerator;
    private readonly IFileSystemService _fileSystem;
    private string? _selectedFolder;
    private string? _previewText;
    private bool _isGenerating;
    private double _progress;
    private string _statusMessage = string.Empty;
    private readonly FilterSettingsViewModel _filterSettings;
    private CancellationTokenSource? _cancellationTokenSource;

    public event Action? OpenFilterSettingsRequested;

    public MainWindowViewModel(
        ITreeGeneratorService treeGenerator,
        IFileSystemService fileSystem,
        FilterSettingsViewModel filterSettings)
    {
        _treeGenerator = treeGenerator;
        _fileSystem = fileSystem;
        _filterSettings = filterSettings;

        BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
        ExportCommand = new RelayCommand(_ => Export(), _ => !string.IsNullOrEmpty(PreviewText));
        CancelCommand = new RelayCommand(_ => Cancel(), _ => IsGenerating);
        OpenFilterSettingsCommand = new RelayCommand(_ => OpenFilterSettings());

        PropertyChanged += OnPropertyChanged;
    }

    public string? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                GenerateTreeAsync().FireAndForgetSafeAsync();
            }
        }
    }

    public string? PreviewText
    {
        get => _previewText;
        private set => SetProperty(ref _previewText, value);
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        private set => SetProperty(ref _isGenerating, value);
    }

    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ICommand BrowseFolderCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenFilterSettingsCommand { get; }

    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select folder to analyze"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFolder = dialog.FolderName;
        }
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedFolder))
        {
            await GenerateTreeAsync();
        }
    }

    private async Task GenerateTreeAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolder)) return;

        // Create a new token source before any operation
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsGenerating = true;
            StatusMessage = "Generating tree...";
            Progress = 0;

            var progress = new Progress<(int current, int total, string currentItem)>(report =>
            {
                Progress = (double)report.current / report.total * 100;
                StatusMessage = $"Processing: {report.currentItem}";
            });

            // Store token in local variable to ensure it's not null during usage
            var token = _cancellationTokenSource.Token;

            var tree = await _treeGenerator.GenerateTreeAsync(
                SelectedFolder,
                _filterSettings.GetFilterOptions(),
                new GeneratorOptions { Format = OutputFormat.PlainText },
                progress,
                token
            );

            if (tree != null)  // Add null check
            {
                PreviewText = await _treeGenerator.GenerateOutputAsync(
                    tree,
                    new GeneratorOptions { Format = OutputFormat.PlainText },
                    token
                );
            }

            StatusMessage = "Tree generation completed";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation cancelled";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Debug.WriteLine($"Tree generation error: {ex}");  // Add logging
        }
        finally
        {
            IsGenerating = false;
            Progress = 0;

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }

    private async void Export()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|Markdown files (*.md)|*.md|All files (*.*)|*.*",
            DefaultExt = ".txt"
        };

        if (dialog.ShowDialog() == true && PreviewText != null)
        {
            try
            {
                StatusMessage = "Exporting...";
                var format = Path.GetExtension(dialog.FileName).ToLower() switch
                {
                    ".md" => OutputFormat.Markdown,
                    _ => OutputFormat.PlainText
                };

                await _fileSystem.WriteAllTextAsync(dialog.FileName, PreviewText);
                StatusMessage = "Export completed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export failed: {ex.Message}";
            }
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void OpenFilterSettings()
    {
        OpenFilterSettingsRequested?.Invoke();
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

public static class TaskExtensions
{
    public static async void FireAndForgetSafeAsync(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            // Log the exception
            Debug.WriteLine($"Fire and forget task failed: {ex}");
        }
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}