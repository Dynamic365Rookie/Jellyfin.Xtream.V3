using System.Text.Json;
using System.Text.Json.Serialization;
using Jellyfin.Xtream.Domain.Models;

namespace Jellyfin.Xtream.Infrastructure.Serialization;

public sealed class XtreamChannelJsonConverter : JsonConverter<XtreamChannel>
{
    public override XtreamChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var streamId = root.TryGetProperty("stream_id", out var sid) ? sid.GetFlexibleInt32() : 0;

        return new XtreamChannel
        {
            Id = streamId,
            StreamId = streamId,
            Name = root.TryGetProperty("name", out var name) ? name.GetFlexibleString() ?? string.Empty : string.Empty,
            Icon = root.TryGetProperty("icon", out var icon) ? icon.GetFlexibleString() : null,
            CategoryId = root.TryGetProperty("category_id", out var catId) ? catId.GetFlexibleNullableInt32() : null,
            CategoryName = root.TryGetProperty("category_name", out var catName) ? catName.GetFlexibleString() : null,
            EpgChannelId = root.TryGetProperty("epg_channel_id", out var epgId) ? epgId.GetFlexibleNullableInt32() : null,
            Number = root.TryGetProperty("num", out var num) ? num.GetFlexibleNullableInt32() : null,
            Language = root.TryGetProperty("language", out var language) ? language.GetFlexibleString() : null,
            Added = root.TryGetProperty("added", out var added) ? added.GetFlexibleInt64() : 0
        };
    }

    public override void Write(Utf8JsonWriter writer, XtreamChannel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("stream_id", value.StreamId);
        writer.WriteString("name", value.Name);
        if (value.Icon != null) writer.WriteString("icon", value.Icon);
        if (value.CategoryId.HasValue) writer.WriteNumber("category_id", value.CategoryId.Value);
        if (value.CategoryName != null) writer.WriteString("category_name", value.CategoryName);
        if (value.EpgChannelId.HasValue) writer.WriteNumber("epg_channel_id", value.EpgChannelId.Value);
        if (value.Number.HasValue) writer.WriteNumber("num", value.Number.Value);
        if (value.Language != null) writer.WriteString("language", value.Language);
        writer.WriteNumber("added", value.Added);
        writer.WriteEndObject();
    }
}
