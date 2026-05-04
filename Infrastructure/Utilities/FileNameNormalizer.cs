using System.Globalization;
using System.Text;

namespace Jellyfin.Xtream.Infrastructure.Utilities;

/// <summary>
/// Provides advanced filename normalization with proper encoding handling for international characters.
/// </summary>
public static class FileNameNormalizer
{
    private static readonly char[] InvalidFileChars = Path.GetInvalidFileNameChars();

    /// <summary>
    /// Normalize filename with proper handling of accented characters.
    /// Converts: "L'Été Français" → "L'Ete Francais"
    /// </summary>
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Unknown";
        }

        var normalized = name;

        // Step 1: Decompose accented characters (NFD normalization)
        // "É" becomes "E" + combining accent mark
        normalized = normalized.Normalize(NormalizationForm.FormD);

        // Step 2: Remove combining diacritical marks
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        normalized = sb.ToString();

        // Step 3: Replace invalid characters with underscores
        foreach (var c in InvalidFileChars)
        {
            normalized = normalized.Replace(c, '_');
        }

        // Step 4: Replace problematic punctuation
        normalized = normalized
            .Replace(':', '-')
            .Replace('/', '_')
            .Replace('\\', '_')
            .Replace('|', '_')
            .Replace('?', '_')
            .Replace('*', '_')
            .Replace('"', '_')
            .Replace('<', '_')
            .Replace('>', '_');

        // Step 5: Clean up multiple underscores and trim
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "_+", "_");
        normalized = normalized.Trim().TrimEnd('.');
        normalized = normalized.TrimStart('_').TrimEnd('_');

        return string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized;
    }
}
