// FolderTreeGenerator.Core/Models/GeneratorOptions.cs
// Purpose: Model for generator options used to configure the output of the folder tree generator.

namespace FolderTreeGenerator.Core.Models
{
    public class GeneratorOptions
    {
        public OutputFormat Format { get; set; } = OutputFormat.PlainText;
        public bool IncludeMetadata { get; set; }
        public string IndentationString { get; set; } = "    ";
        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public Dictionary<string, object> CustomOptions { get; init; } = new();
    }

    public enum OutputFormat
    {
        PlainText,
        Markdown,
        Json,
        Xml,
        Html
    }
}