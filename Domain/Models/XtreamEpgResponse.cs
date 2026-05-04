using System.Text.Json.Serialization;

namespace Jellyfin.Xtream.Domain.Models;

/// <summary>
/// Response from the Xtream API get_simple_data_table (EPG) action.
/// </summary>
public sealed class XtreamEpgResponse
{
    [JsonPropertyName("epg_listings")]
    public List<XtreamEpgListing> Listings { get; set; } = new();
}

/// <summary>
/// A single EPG listing entry from the Xtream API.
/// </summary>
public sealed class XtreamEpgListing
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("epg_id")]
    public string? EpgId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string? Language { get; set; }

    [JsonPropertyName("start")]
    public string Start { get; set; } = string.Empty;

    [JsonPropertyName("end")]
    public string End { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("start_timestamp")]
    public long? StartTimestamp { get; set; }

    [JsonPropertyName("stop_timestamp")]
    public long? StopTimestamp { get; set; }
}
