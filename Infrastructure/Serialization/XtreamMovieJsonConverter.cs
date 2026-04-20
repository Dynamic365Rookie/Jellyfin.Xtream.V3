using System.Text.Json;
using System.Text.Json.Serialization;
using Jellyfin.Xtream.Domain.Models;

namespace Jellyfin.Xtream.Infrastructure.Serialization;

public sealed class XtreamMovieJsonConverter : JsonConverter<XtreamMovie>
{
    public override XtreamMovie Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        return new XtreamMovie
        {
            Id = root.TryGetProperty("stream_id", out var streamId) ? streamId.GetInt32() : 0,
            StreamId = root.TryGetProperty("stream_id", out var sid) ? sid.GetInt32() : 0,
            Name = root.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
            Image = root.TryGetProperty("image", out var image) ? image.GetString() : null,
            Rating = root.TryGetProperty("rating", out var rating) ? double.TryParse(rating.GetString(), out var r) ? r : null : null,
            Rating5Based = root.TryGetProperty("rating_5based", out var r5) ? double.TryParse(r5.GetString(), out var r5val) ? r5val : null : null,
            Plot = root.TryGetProperty("plot", out var plot) ? plot.GetString() : null,
            Duration = root.TryGetProperty("duration", out var duration) ? duration.GetString() : null,
            Year = root.TryGetProperty("year", out var year) ? year.GetInt32() : null,
            Genre = root.TryGetProperty("genre", out var genre) ? genre.GetString() : null,
            Country = root.TryGetProperty("country", out var country) ? country.GetString() : null,
            Director = root.TryGetProperty("director", out var director) ? director.GetString() : null,
            Writer = root.TryGetProperty("writer", out var writer) ? writer.GetString() : null,
            Actor = root.TryGetProperty("actor", out var actor) ? actor.GetString() : null,
            CategoryId = root.TryGetProperty("category_id", out var catId) ? catId.GetInt32() : null,
            CategoryName = root.TryGetProperty("category_name", out var catName) ? catName.GetString() : null,
            Added = root.TryGetProperty("added", out var added) ? added.GetInt64() : 0,
            LastModifiedTimestamp = root.TryGetProperty("last_modified", out var lastMod) ? lastMod.GetInt64() : 0
        };
    }

    public override void Write(Utf8JsonWriter writer, XtreamMovie value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("stream_id", value.StreamId);
        writer.WriteString("name", value.Name);
        if (value.Image != null) writer.WriteString("image", value.Image);
        if (value.Rating.HasValue) writer.WriteNumber("rating", value.Rating.Value);
        if (value.Rating5Based.HasValue) writer.WriteNumber("rating_5based", value.Rating5Based.Value);
        if (value.Plot != null) writer.WriteString("plot", value.Plot);
        if (value.Duration != null) writer.WriteString("duration", value.Duration);
        if (value.Year.HasValue) writer.WriteNumber("year", value.Year.Value);
        if (value.Genre != null) writer.WriteString("genre", value.Genre);
        if (value.Country != null) writer.WriteString("country", value.Country);
        if (value.Director != null) writer.WriteString("director", value.Director);
        if (value.Writer != null) writer.WriteString("writer", value.Writer);
        if (value.Actor != null) writer.WriteString("actor", value.Actor);
        if (value.CategoryId.HasValue) writer.WriteNumber("category_id", value.CategoryId.Value);
        if (value.CategoryName != null) writer.WriteString("category_name", value.CategoryName);
        writer.WriteNumber("added", value.Added);
        writer.WriteNumber("last_modified", value.LastModifiedTimestamp);
        writer.WriteEndObject();
    }
}
