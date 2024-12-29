// FolderTreeGenerator.Core/Models/AppSettings.cs
// Purpose: Model for application settings.

using System.Text.Json;

namespace FolderTreeGenerator.Core.Models
{
    public class AppSettings
    {
        public string? LastDirectory { get; set; }
        public FilterOptions FilterOptions { get; set; } = new();
    }
}