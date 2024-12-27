using System.Text.RegularExpressions;

namespace FolderTreeGenerator.Core.Services;

public class GitignoreParser
{
    private readonly List<(Regex regex, bool isNegative)> _patterns = new();

    public void AddPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern.StartsWith('#'))
            return;

        pattern = pattern.Trim();
        var isNegative = pattern.StartsWith('!');
        if (isNegative)
            pattern = pattern[1..];

        _patterns.Add((ConvertGitignoreToRegex(pattern), isNegative));
    }

    public void Clear()
    {
        _patterns.Clear();
    }

    public bool IsIgnored(string path)
    {
        path = path.Replace('\\', '/');
        var isIgnored = false;

        foreach (var (regex, isNegative) in _patterns)
        {
            if (regex.IsMatch(path))
            {
                isIgnored = !isNegative;
            }
        }

        return isIgnored;
    }

    private static Regex ConvertGitignoreToRegex(string pattern)
    {
        // Escape special regex characters
        pattern = Regex.Escape(pattern);

        // Convert gitignore glob patterns to regex
        pattern = pattern
            .Replace(@"\*\*/", "(.*/)?") // Match directory wildcard
            .Replace(@"\*", "[^/]*")     // Match single-level wildcard
            .Replace(@"\?", "[^/]")      // Match single character
            .Replace(@"\[", "[")         // Allow character classes
            .Replace(@"\]", "]");

        // Handle trailing slash for directories
        if (pattern.EndsWith(@"\/"))
        {
            pattern = pattern[..^2] + "(/.*)?$";
        }
        else
        {
            pattern += "(/.*)?$";
        }

        // Make pattern match full path
        pattern = "^" + pattern;

        return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}