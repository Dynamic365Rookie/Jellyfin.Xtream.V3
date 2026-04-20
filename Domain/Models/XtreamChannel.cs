namespace Jellyfin.Xtream.Domain.Models;

[System.Text.Json.Serialization.JsonConverter(typeof(Jellyfin.Xtream.Infrastructure.Serialization.XtreamChannelJsonConverter))]
public sealed record XtreamChannel : Jellyfin.Xtream.Infrastructure.Persistence.IEntity
{
    public int Id { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("stream_id")]
    public int StreamId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("category_id")]
    public int? CategoryId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("category_name")]
    public string? CategoryName { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("epg_channel_id")]
    public int? EpgChannelId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("num")]
    public int? Number { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("language")]
    public string? Language { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("added")]
    public long Added { get; init; }

    // Les channels Xtream n'ont pas toujours last_modified, on utilise 'added' comme fallback
    public DateTime LastModified => UnixTimeStampToDateTime(Added);

    private static DateTime UnixTimeStampToDateTime(long timestamp)
    {
        if (timestamp == 0) return DateTime.UtcNow;
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(timestamp);
        return dateTime;
    }
}
