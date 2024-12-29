# FolderTreeGenerator

FolderTreeGenerator is a WPF application designed to generate and display a folder tree structure based on user-defined filter settings. The application allows users to include or exclude specific file extensions and folders, set a maximum depth for the folder tree, and choose whether to use `.gitignore` rules.

## Features

- **Filter Settings**: Customize which files and folders to include or exclude.
- **Gitignore Support**: Optionally use `.gitignore` rules to filter files and folders.
- **Hidden Files**: Choose whether to display hidden files.
- **Empty Folders**: Optionally include empty folders in the folder tree.
- **Max Depth**: Set a maximum depth for the folder tree.

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 8.0 SDK

### Installation

1. Clone the repository: ```git clone https://github.com/casualinnovations/FolderTreeGenerator.git```
2. Open the solution in Visual Studio 2022.

### Building and Running

1. Build the solution:
    - In Visual Studio, go to `Build` > `Build Solution`.
2. Run the application:
    - Press `F5` or go to `Debug` > `Start Debugging`.

## Project Structure

- **FolderTreeGenerator.Core**: Contains core interfaces and models.
- **FolderTreeGenerator.UI**: Contains the WPF application, including view models and views.

## Key Classes and Interfaces

### FilterSettingsViewModel

Located in `FolderTreeGenerator.UI/ViewModels/FilterSettingsViewModel.cs`, this class manages the filter settings and commands for adding/removing extensions and folders.

### IFileSystemService

Defines methods for interacting with the file system, such as checking if a directory exists and reading files.

### IFilterService

Defines methods for applying filter rules, including loading `.gitignore` rules and checking if a path should be included based on the filter options.

### FilterOptions

A model class that holds the filter settings, such as included/excluded extensions and folders, maximum depth, and whether to use `.gitignore` rules.

## Contributing

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -am 'Add new feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Create a new Pull Request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) - Windows Presentation Foundation
- [MVVM](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm) - Model-View-ViewModel pattern

