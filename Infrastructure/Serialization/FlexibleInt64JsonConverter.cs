using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Xtream.Infrastructure.Serialization;

/// <summary>
/// JSON converter that handles both string and numeric representations of Int64.
/// Xtream API sometimes returns timestamps as strings instead of numbers.
/// </summary>
public sealed class FlexibleInt64JsonConverter : JsonConverter<long?>
{
    /// <summary>
    /// Reads a JSON token that can be a number or a string and converts it to long?.
    /// </summary>
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            // Handle numeric type directly
            JsonTokenType.Number when reader.TryGetInt64(out var value) => value,

            // Handle string representation of numbers
            JsonTokenType.String =>
                reader.GetString() switch
                {
                    null or "" => null,
                    var s => long.TryParse(s, out var parsed) ? parsed : null,
                },

            // Handle null explicitly
            JsonTokenType.Null => null,

            // Unexpected type - return null instead of throwing
            _ => null,
        };
    }

    /// <summary>
    /// Writes the long? value back as a JSON number.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
