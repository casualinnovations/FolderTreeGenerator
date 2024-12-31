// DefaultSettings.cs
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace FolderTreeGenerator.Core.Models;

public class DefaultSettings
{
    public FilterOptions Filters { get; set; } = new()
    {
        IncludeExtensions = new HashSet<string>(new[] { ".txt", ".md", ".cs" }, StringComparer.OrdinalIgnoreCase),
        ExcludedFolders = new HashSet<string>(new[] { "bin", "obj", "node_modules" }, StringComparer.OrdinalIgnoreCase),
        IncludeEmpty = true,
        ShowHiddenFiles = false,
        UseGitignore = false
    };

    public GeneratorOptions Generator { get; set; } = new()
    {
        Format = OutputFormat.PlainText,
        IncludeMetadata = true,
        DateFormat = "yyyy-MM-dd HH:mm:ss"
    };

    public static DefaultSettings CreateDefault() => new();
}
