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

        var streamId = root.TryGetProperty("stream_id", out var sid) ? sid.GetFlexibleInt32() : 0;

        return new XtreamMovie
        {
            Id = streamId,
            StreamId = streamId,
            Name = root.TryGetProperty("name", out var name) ? name.GetFlexibleString() ?? string.Empty : string.Empty,
            Image = root.TryGetProperty("image", out var image) ? image.GetFlexibleString() : null,
            Rating = root.TryGetProperty("rating", out var rating) ? rating.GetFlexibleNullableDouble() : null,
            Rating5Based = root.TryGetProperty("rating_5based", out var r5) ? r5.GetFlexibleNullableDouble() : null,
            Plot = root.TryGetProperty("plot", out var plot) ? plot.GetFlexibleString() : null,
            Duration = root.TryGetProperty("duration", out var duration) ? duration.GetFlexibleString() : null,
            Year = root.TryGetProperty("year", out var year) ? year.GetFlexibleNullableInt32() : null,
            Genre = root.TryGetProperty("genre", out var genre) ? genre.GetFlexibleString() : null,
            Country = root.TryGetProperty("country", out var country) ? country.GetFlexibleString() : null,
            Director = root.TryGetProperty("director", out var director) ? director.GetFlexibleString() : null,
            Writer = root.TryGetProperty("writer", out var writer) ? writer.GetFlexibleString() : null,
            Actor = root.TryGetProperty("actor", out var actor) ? actor.GetFlexibleString() : null,
            CategoryId = root.TryGetProperty("category_id", out var catId) ? catId.GetFlexibleNullableInt32() : null,
            CategoryName = root.TryGetProperty("category_name", out var catName) ? catName.GetFlexibleString() : null,
            ContainerExtension = root.TryGetProperty("container_extension", out var ext) ? ext.GetFlexibleString() : null,
            Added = root.TryGetProperty("added", out var added) ? added.GetFlexibleInt64() : 0,
            LastModifiedTimestamp = root.TryGetProperty("last_modified", out var lastMod) ? lastMod.GetFlexibleInt64() : 0
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
        if (value.ContainerExtension != null) writer.WriteString("container_extension", value.ContainerExtension);
        writer.WriteNumber("added", value.Added);
        writer.WriteNumber("last_modified", value.LastModifiedTimestamp);
        writer.WriteEndObject();
    }
}
