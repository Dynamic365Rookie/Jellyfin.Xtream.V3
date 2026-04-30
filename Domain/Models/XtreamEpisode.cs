using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Xtream.Domain.Models;

/// <summary>
/// Converts JSON values that may be either a string or a number into a string.
/// </summary>
public sealed class StringOrNumberConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetInt64().ToString(System.Globalization.CultureInfo.InvariantCulture),
            JsonTokenType.Null => null,
            _ => reader.GetString(),
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}

/// <summary>
/// Represents an episode from the Xtream API series info response.
/// </summary>
public sealed class XtreamEpisode
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringOrNumberConverter))]
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
