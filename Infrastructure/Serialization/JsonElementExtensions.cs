using System.Text.Json;

namespace Jellyfin.Xtream.Infrastructure.Serialization;

/// <summary>
/// Extension methods for flexible JSON type reading.
/// Xtream API providers are inconsistent — numeric fields may arrive as strings and vice versa.
/// </summary>
internal static class JsonElementExtensions
{
    /// <summary>
    /// Reads an int from either a JSON number or a JSON string containing a number.
    /// </summary>
    public static int GetFlexibleInt32(this JsonElement element, int defaultValue = 0)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt32(out var n) ? n : defaultValue,
            JsonValueKind.String => int.TryParse(element.GetString(), out var n) ? n : defaultValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Reads a nullable int from either a JSON number or a JSON string.
    /// </summary>
    public static int? GetFlexibleNullableInt32(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt32(out var n) ? n : null,
            JsonValueKind.String => int.TryParse(element.GetString(), out var n) ? n : null,
            JsonValueKind.Null => null,
            _ => null
        };
    }

    /// <summary>
    /// Reads a long from either a JSON number or a JSON string containing a number.
    /// </summary>
    public static long GetFlexibleInt64(this JsonElement element, long defaultValue = 0)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt64(out var n) ? n : defaultValue,
            JsonValueKind.String => long.TryParse(element.GetString(), out var n) ? n : defaultValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Reads a double from either a JSON number or a JSON string containing a number.
    /// </summary>
    public static double? GetFlexibleNullableDouble(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetDouble(out var n) ? n : null,
            JsonValueKind.String => double.TryParse(element.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : null,
            JsonValueKind.Null => null,
            _ => null
        };
    }

    /// <summary>
    /// Reads a string from either a JSON string or a JSON number (converts to string).
    /// </summary>
    public static string? GetFlexibleString(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }
}
