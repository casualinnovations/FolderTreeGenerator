using FolderTreeGenerator.Core.Interfaces;
using FolderTreeGenerator.Core.Services;
using FolderTreeGenerator.UI.ViewModels;
using FolderTreeGenerator.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FolderTreeGenerator.UI;

public partial class App : Application
{
    private IServiceProvider ServiceProvider { get; set; }

    public App()
    {
        ServiceProvider = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register Core Services
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IFilterService, FilterService>();
        services.AddSingleton<ITreeGeneratorService, TreeGeneratorService>();

        // Register ViewModels
        services.AddSingleton<FilterSettingsViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        // Register Views
        services.AddTransient<MainWindow>();
        services.AddTransient<FilterSettingsWindow>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}