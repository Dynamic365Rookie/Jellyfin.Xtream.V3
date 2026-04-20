using System.Text.Json;
using System.Text.Json.Serialization;
using Jellyfin.Xtream.Domain.Models;

namespace Jellyfin.Xtream.Infrastructure.Serialization;

public sealed class XtreamSeriesJsonConverter : JsonConverter<XtreamSeries>
{
    public override XtreamSeries Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        return new XtreamSeries
        {
            Id = root.TryGetProperty("series_id", out var seriesId) ? seriesId.GetInt32() : 0,
            SeriesId = root.TryGetProperty("series_id", out var sid) ? sid.GetInt32() : 0,
            Name = root.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
            Image = root.TryGetProperty("image", out var image) ? image.GetString() : null,
            Rating = root.TryGetProperty("rating", out var rating) ? double.TryParse(rating.GetString(), out var r) ? r : null : null,
            Rating5Based = root.TryGetProperty("rating_5based", out var r5) ? double.TryParse(r5.GetString(), out var r5val) ? r5val : null : null,
            Plot = root.TryGetProperty("plot", out var plot) ? plot.GetString() : null,
            Year = root.TryGetProperty("year", out var year) ? year.GetInt32() : null,
            Genre = root.TryGetProperty("genre", out var genre) ? genre.GetString() : null,
            CategoryId = root.TryGetProperty("category_id", out var catId) ? catId.GetInt32() : null,
            CategoryName = root.TryGetProperty("category_name", out var catName) ? catName.GetString() : null,
            EpisodesCount = root.TryGetProperty("episodes_count", out var epCount) ? epCount.GetInt32() : null,
            SeasonsCount = root.TryGetProperty("seasons_count", out var seCount) ? seCount.GetInt32() : null,
            Added = root.TryGetProperty("added", out var added) ? added.GetInt64() : 0,
            LastModifiedTimestamp = root.TryGetProperty("last_modified", out var lastMod) ? lastMod.GetInt64() : 0
        };
    }

    public override void Write(Utf8JsonWriter writer, XtreamSeries value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("series_id", value.SeriesId);
        writer.WriteString("name", value.Name);
        if (value.Image != null) writer.WriteString("image", value.Image);
        if (value.Rating.HasValue) writer.WriteNumber("rating", value.Rating.Value);
        if (value.Rating5Based.HasValue) writer.WriteNumber("rating_5based", value.Rating5Based.Value);
        if (value.Plot != null) writer.WriteString("plot", value.Plot);
        if (value.Year.HasValue) writer.WriteNumber("year", value.Year.Value);
        if (value.Genre != null) writer.WriteString("genre", value.Genre);
        if (value.CategoryId.HasValue) writer.WriteNumber("category_id", value.CategoryId.Value);
        if (value.CategoryName != null) writer.WriteString("category_name", value.CategoryName);
        if (value.EpisodesCount.HasValue) writer.WriteNumber("episodes_count", value.EpisodesCount.Value);
        if (value.SeasonsCount.HasValue) writer.WriteNumber("seasons_count", value.SeasonsCount.Value);
        writer.WriteNumber("added", value.Added);
        writer.WriteNumber("last_modified", value.LastModifiedTimestamp);
        writer.WriteEndObject();
    }
}
