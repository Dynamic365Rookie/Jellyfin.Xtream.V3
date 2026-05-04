using System.Text.RegularExpressions;

namespace Jellyfin.Xtream.Services.Mapping;

/// <summary>
/// Cleans IPTV channel names by removing country prefixes, quality suffixes,
/// and bracketed tags commonly found in Xtream-compatible providers.
/// </summary>
public static partial class ChannelNameCleaner
{
    // 1. Country prefix: "FR: ", "FR | ", "|FR| ", "FR- ", "US : ", etc.
    [GeneratedRegex(@"^\s*\|?\s*[A-Za-z]{2,3}\s*[:\|\-]+\s*", RegexOptions.Compiled)]
    private static partial Regex CountryPrefixPattern();

    // 1b. Pipe-wrapped country code: "|FR| ", "|USA|"
    [GeneratedRegex(@"^\|[A-Za-z]{2,3}\|\s*", RegexOptions.Compiled)]
    private static partial Regex PipeCountryPattern();

    // 2. Bracketed/parenthesized tags: (BACKUP), [Multi-Sub], (VIP), [LOW]
    [GeneratedRegex(@"[\(\[]\s*[^\)\]]*\s*[\)\]]", RegexOptions.Compiled)]
    private static partial Regex BracketedTagPattern();

    // 3. Quality suffixes: HD, FHD, SD, 4K, UHD, HEVC, H265, H.265
    [GeneratedRegex(@"\b(H\.?265|HEVC|UHD|FHD|HD|SD|4K)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex QualitySuffixPattern();

    // 3b. Language/version suffixes: VF, VOSTFR, MULTI, QFR, TRUEFRENCH
    [GeneratedRegex(@"\b(VF|VO|VOSTFR|MULTI|QFR|TRUEFRENCH|FRENCH)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex LanguageSuffixPattern();

    // 4. Trailing separators left after other removals
    [GeneratedRegex(@"[\s\|\-\.\:]+$", RegexOptions.Compiled)]
    private static partial Regex TrailingSeparatorPattern();

    // 5. Whitespace normalization
    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex MultipleSpacesPattern();

    /// <summary>
    /// Cleans a raw IPTV channel name by removing noise.
    /// </summary>
    /// <param name="rawName">The raw channel name from the Xtream API.</param>
    /// <returns>The cleaned channel name.</returns>
    public static string Clean(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return rawName;
        }

        var name = rawName;

        // 1. Remove country prefix
        name = PipeCountryPattern().Replace(name, string.Empty);
        name = CountryPrefixPattern().Replace(name, string.Empty);

        // 2. Remove bracketed tags
        name = BracketedTagPattern().Replace(name, string.Empty);

        // 3. Remove quality suffix
        name = QualitySuffixPattern().Replace(name, string.Empty);

        // 3b. Remove language/version suffix
        name = LanguageSuffixPattern().Replace(name, string.Empty);

        // 4. Remove trailing separators
        name = TrailingSeparatorPattern().Replace(name, string.Empty);

        // 5. Normalize whitespace
        name = MultipleSpacesPattern().Replace(name, " ").Trim();

        return name;
    }
}
