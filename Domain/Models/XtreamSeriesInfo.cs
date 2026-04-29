using System.Text.Json.Serialization;

namespace Jellyfin.Xtream.Domain.Models;

/// <summary>
/// Response from the Xtream API get_series_info action.
/// Episodes are grouped by season number (dictionary key is season number as string).
/// </summary>
public sealed class XtreamSeriesInfo
{
    [JsonPropertyName("episodes")]
    public Dictionary<string, List<XtreamEpisode>> Episodes { get; set; } = new();

    [JsonPropertyName("info")]
    public XtreamSeries? Info { get; set; }
}
