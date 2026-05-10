using System.Text.Json;
using Xunit;
using Jellyfin.Xtream.Domain.Models;

namespace Jellyfin.Xtream.V3.Tests.Infrastructure.Serialization;

/// <summary>
/// Unit tests for FlexibleInt64JsonConverter to handle Xtream API timestamp variations.
/// </summary>
public sealed class FlexibleInt64JsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Test that EPG listing with numeric timestamp deserializes correctly.
    /// </summary>
    [Fact]
    public void Deserialize_NumericTimestamp_SucceedsAndParsesValue()
    {
        const string json = """
            {
              "epg_listings": [
                {
                  "id": "123",
                  "title": "Test Program",
                  "start": "2024-05-09 20:00:00",
                  "end": "2024-05-09 22:00:00",
                  "start_timestamp": 1715342400,
                  "stop_timestamp": 1715349600,
                  "description": "Test"
                }
              ]
            }
            """;

        var response = JsonSerializer.Deserialize<XtreamEpgResponse>(json, Options);

        Assert.NotNull(response);
        Assert.Single(response.Listings);
        Assert.Equal(1715342400, response.Listings[0].StartTimestamp);
        Assert.Equal(1715349600, response.Listings[0].StopTimestamp);
    }

    /// <summary>
    /// Test that EPG listing with string timestamp deserializes correctly (main bug fix).
    /// </summary>
    [Fact]
    public void Deserialize_StringTimestamp_SucceedsAndParsesValue()
    {
        const string json = """
            {
              "epg_listings": [
                {
                  "id": "456",
                  "title": "String Timestamp Program",
                  "start": "2024-05-09 20:00:00",
                  "end": "2024-05-09 22:00:00",
                  "start_timestamp": "1715342400",
                  "stop_timestamp": "1715349600",
                  "description": "Test with strings"
                }
              ]
            }
            """;

        var response = JsonSerializer.Deserialize<XtreamEpgResponse>(json, Options);

        Assert.NotNull(response);
        Assert.Single(response.Listings);
        Assert.Equal(1715342400, response.Listings[0].StartTimestamp);
        Assert.Equal(1715349600, response.Listings[0].StopTimestamp);
    }

    /// <summary>
    /// Test that EPG listing with null timestamp deserializes as null.
    /// </summary>
    [Fact]
    public void Deserialize_NullTimestamp_SucceedsAndReturnsNull()
    {
        const string json = """
            {
              "epg_listings": [
                {
                  "id": "789",
                  "title": "Null Timestamp Program",
                  "start": "2024-05-09 20:00:00",
                  "end": "2024-05-09 22:00:00",
                  "start_timestamp": null,
                  "stop_timestamp": null,
                  "description": "Test null"
                }
              ]
            }
            """;

        var response = JsonSerializer.Deserialize<XtreamEpgResponse>(json, Options);

        Assert.NotNull(response);
        Assert.Single(response.Listings);
        Assert.Null(response.Listings[0].StartTimestamp);
        Assert.Null(response.Listings[0].StopTimestamp);
    }

    /// <summary>
    /// Test that EPG listing with empty string timestamp deserializes as null.
    /// </summary>
    [Fact]
    public void Deserialize_EmptyStringTimestamp_SucceedsAndReturnsNull()
    {
        const string json = """
            {
              "epg_listings": [
                {
                  "id": "999",
                  "title": "Empty String Program",
                  "start": "2024-05-09 20:00:00",
                  "end": "2024-05-09 22:00:00",
                  "start_timestamp": "",
                  "stop_timestamp": "",
                  "description": "Test empty"
                }
              ]
            }
            """;

        var response = JsonSerializer.Deserialize<XtreamEpgResponse>(json, Options);

        Assert.NotNull(response);
        Assert.Single(response.Listings);
        Assert.Null(response.Listings[0].StartTimestamp);
        Assert.Null(response.Listings[0].StopTimestamp);
    }

    /// <summary>
    /// Test that EPG listing with invalid string timestamp deserializes as null (graceful handling).
    /// </summary>
    [Fact]
    public void Deserialize_InvalidStringTimestamp_SucceedsAndReturnsNull()
    {
        const string json = """
            {
              "epg_listings": [
                {
                  "id": "invalid",
                  "title": "Invalid String Program",
                  "start": "2024-05-09 20:00:00",
                  "end": "2024-05-09 22:00:00",
                  "start_timestamp": "not_a_number",
                  "stop_timestamp": "invalid_date",
                  "description": "Test invalid"
                }
              ]
            }
            """;

        var response = JsonSerializer.Deserialize<XtreamEpgResponse>(json, Options);

        Assert.NotNull(response);
        Assert.Single(response.Listings);
        Assert.Null(response.Listings[0].StartTimestamp);
        Assert.Null(response.Listings[0].StopTimestamp);
    }

    /// <summary>
    /// Test mixed format response with both numeric and string timestamps.
    /// </summary>
    [Fact]
    public void Deserialize_MixedTimestampFormats_SucceedsAndParsesCorrectly()
    {
        const string json = """
            {
              "epg_listings": [
                {
                  "id": "1",
                  "title": "Numeric Program",
                  "start": "2024-05-09 20:00:00",
                  "end": "2024-05-09 22:00:00",
                  "start_timestamp": 1715342400,
                  "stop_timestamp": 1715349600,
                  "description": "Numeric"
                },
                {
                  "id": "2",
                  "title": "String Program",
                  "start": "2024-05-09 22:00:00",
                  "end": "2024-05-10 00:00:00",
                  "start_timestamp": "1715349600",
                  "stop_timestamp": "1715356800",
                  "description": "String"
                }
              ]
            }
            """;

        var response = JsonSerializer.Deserialize<XtreamEpgResponse>(json, Options);

        Assert.NotNull(response);
        Assert.Equal(2, response.Listings.Count);

        // First listing (numeric)
        Assert.Equal(1715342400, response.Listings[0].StartTimestamp);
        Assert.Equal(1715349600, response.Listings[0].StopTimestamp);

        // Second listing (string)
        Assert.Equal(1715349600, response.Listings[1].StartTimestamp);
        Assert.Equal(1715356800, response.Listings[1].StopTimestamp);
    }
}
