using System.Text.Json.Serialization;

namespace Jellyfin.Xtream.Domain.Models;

/// <summary>
/// Represents an episode from the Xtream API series info response.
/// </summary>
public sealed class XtreamEpisode
{
    [JsonPropertyName("id")]
    public string? StreamId { get; set; }

    [JsonPropertyName("episode_num")]
    public int EpisodeNumber { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("season")]
    public int Season { get; set; }

    [JsonPropertyName("container_extension")]
    public string? ContainerExtension { get; set; }

    [JsonPropertyName("info")]
    public XtreamEpisodeInfo? Info { get; set; }
}

/// <summary>
/// Nested episode metadata from the Xtream API.
/// </summary>
public sealed class XtreamEpisodeInfo
{
    [JsonPropertyName("movie_image")]
    public string? Image { get; set; }

    [JsonPropertyName("plot")]
    public string? Plot { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }
}
