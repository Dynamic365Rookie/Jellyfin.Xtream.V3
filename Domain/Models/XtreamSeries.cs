namespace Jellyfin.Xtream.Domain.Models;

[System.Text.Json.Serialization.JsonConverter(typeof(Jellyfin.Xtream.Infrastructure.Serialization.XtreamSeriesJsonConverter))]
public sealed record XtreamSeries : Jellyfin.Xtream.Infrastructure.Persistence.IEntity
{
    public int Id { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("series_id")]
    public int SeriesId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("image")]
    public string? Image { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("rating")]
    public double? Rating { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("rating_5based")]
    public double? Rating5Based { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("plot")]
    public string? Plot { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("year")]
    public int? Year { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("genre")]
    public string? Genre { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("category_id")]
    public int? CategoryId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("category_name")]
    public string? CategoryName { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("episodes_count")]
    public int? EpisodesCount { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("seasons_count")]
    public int? SeasonsCount { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("added")]
    public long Added { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("last_modified")]
    public long LastModifiedTimestamp { get; init; }

    public DateTime LastModified => UnixTimeStampToDateTime(LastModifiedTimestamp);

    private static DateTime UnixTimeStampToDateTime(long timestamp)
    {
        if (timestamp == 0) return DateTime.UtcNow;
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(timestamp);
        return dateTime;
    }
}
