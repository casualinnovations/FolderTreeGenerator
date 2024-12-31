// Views/FilterSettingsWindow.xaml.cs
// purpose: Provide the code-behind for the FilterSettingsWindow view
using System.Windows;
using FolderTreeGenerator.UI.ViewModels;

namespace FolderTreeGenerator.UI.Views;

public partial class FilterSettingsWindow : Window
{
    private readonly FilterSettingsViewModel _viewModel;

    public FilterSettingsWindow(FilterSettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        this.DataContext = _viewModel;
        _viewModel.RequestClose += () => Close();
    }

    public void SetOwner(Window owner)
    {
        this.Owner = owner;
    }
}