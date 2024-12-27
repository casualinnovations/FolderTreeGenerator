using FolderTreeGenerator.UI.ViewModels;
using FolderTreeGenerator.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FolderTreeGenerator.UI;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;

        _viewModel.OpenFilterSettingsRequested += OnOpenFilterSettings;
    }

    private void OnOpenFilterSettings()
    {
        var filterWindow = _serviceProvider.GetRequiredService<FilterSettingsWindow>();
        filterWindow.SetOwner(this);
        filterWindow.ShowDialog();
    }
}