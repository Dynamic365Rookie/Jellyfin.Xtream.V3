using System.Text.Json;
using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Infrastructure.Serialization;
using Jellyfin.Xtream.Services.Synchronization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Tests;

/// <summary>
/// Test suite for Xtream data loading, deserialization, and synchronization.
/// </summary>
public sealed class XtreamDataLoadingTests
{
    private readonly ILogger<XtreamDataLoadingTests> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public XtreamDataLoadingTests(ILogger<XtreamDataLoadingTests> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultBufferSize = 65536,
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Tests deserialization of a sample Xtream movie response.
    /// </summary>
    public void TestMovieDeserialization()
    {
        _logger.LogInformation("Running: Movie deserialization test");

        var sampleJson = @"{
            ""stream_id"": 123456,
            ""name"": ""The Matrix"",
            ""image"": ""http://example.com/matrix.jpg"",
            ""rating"": ""8.7"",
            ""rating_5based"": ""4.35"",
            ""plot"": ""A hacker discovers the truth about reality"",
            ""duration"": ""2h 16m"",
            ""year"": 1999,
            ""genre"": ""Action|Sci-Fi"",
            ""country"": ""United States"",
            ""director"": ""Lana Wachowski, Lilly Wachowski"",
            ""writer"": ""Lana Wachowski, Lilly Wachowski"",
            ""actor"": ""Keanu Reeves, Laurence Fishburne"",
            ""category_id"": 5,
            ""category_name"": ""Movies"",
            ""added"": 1640000000,
            ""last_modified"": 1650000000
        }";

        try
        {
            var movie = JsonSerializer.Deserialize<XtreamMovie>(sampleJson, _jsonOptions);

            if (movie == null)
                throw new InvalidOperationException("Deserialization returned null");

            ValidateMovieData(movie);
            _logger.LogInformation("✓ Movie deserialization test passed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Movie deserialization test failed");
            throw;
        }
    }

    /// <summary>
    /// Tests deserialization of a sample Xtream series response.
    /// </summary>
    public void TestSeriesDeserialization()
    {
        _logger.LogInformation("Running: Series deserialization test");

        var sampleJson = @"{
            ""series_id"": 789012,
            ""name"": ""Breaking Bad"",
            ""image"": ""http://example.com/breakingbad.jpg"",
            ""rating"": ""9.5"",
            ""rating_5based"": ""4.75"",
            ""plot"": ""A chemistry teacher turns to manufacturing methamphetamine"",
            ""year"": 2008,
            ""genre"": ""Crime|Drama|Thriller"",
            ""category_id"": 6,
            ""category_name"": ""Series"",
            ""episodes_count"": 62,
            ""seasons_count"": 5,
            ""added"": 1640000000,
            ""last_modified"": 1650000000
        }";

        try
        {
            var series = JsonSerializer.Deserialize<XtreamSeries>(sampleJson, _jsonOptions);

            if (series == null)
                throw new InvalidOperationException("Deserialization returned null");

            ValidateSeriesData(series);
            _logger.LogInformation("✓ Series deserialization test passed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Series deserialization test failed");
            throw;
        }
    }

    /// <summary>
    /// Tests deserialization of a sample Xtream channel response.
    /// </summary>
    public void TestChannelDeserialization()
    {
        _logger.LogInformation("Running: Channel deserialization test");

        var sampleJson = @"{
            ""stream_id"": 345678,
            ""name"": ""France 2"",
            ""icon"": ""http://example.com/france2.png"",
            ""category_id"": 1,
            ""category_name"": ""France"",
            ""epg_channel_id"": 101,
            ""num"": 2,
            ""language"": ""FR"",
            ""added"": 1640000000
        }";

        try
        {
            var channel = JsonSerializer.Deserialize<XtreamChannel>(sampleJson, _jsonOptions);

            if (channel == null)
                throw new InvalidOperationException("Deserialization returned null");

            ValidateChannelData(channel);
            _logger.LogInformation("✓ Channel deserialization test passed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Channel deserialization test failed");
            throw;
        }
    }

    /// <summary>
    /// Tests array deserialization (as returned by Xtream API).
    /// </summary>
    public void TestArrayDeserialization()
    {
        _logger.LogInformation("Running: Array deserialization test");

        var sampleJson = @"[
            {
                ""stream_id"": 1,
                ""name"": ""Movie 1"",
                ""image"": ""http://example.com/m1.jpg"",
                ""added"": 1640000000,
                ""last_modified"": 1650000000
            },
            {
                ""stream_id"": 2,
                ""name"": ""Movie 2"",
                ""image"": ""http://example.com/m2.jpg"",
                ""added"": 1640000001,
                ""last_modified"": 1650000001
            }
        ]";

        try
        {
            var movies = JsonSerializer.Deserialize<IEnumerable<XtreamMovie>>(sampleJson, _jsonOptions);

            if (movies == null)
                throw new InvalidOperationException("Deserialization returned null");

            var movieList = movies.ToList();
            if (movieList.Count != 2)
                throw new InvalidOperationException($"Expected 2 movies, got {movieList.Count}");

            foreach (var movie in movieList)
            {
                if (movie.Id == 0)
                    throw new InvalidOperationException("Movie ID was not mapped");
            }

            _logger.LogInformation("✓ Array deserialization test passed - loaded {Count} items", movieList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Array deserialization test failed");
            throw;
        }
    }

    /// <summary>
    /// Tests handling of empty API response.
    /// </summary>
    public void TestEmptyArrayDeserialization()
    {
        _logger.LogInformation("Running: Empty array deserialization test");

        var sampleJson = "[]";

        try
        {
            var movies = JsonSerializer.Deserialize<IEnumerable<XtreamMovie>>(sampleJson, _jsonOptions);

            if (movies == null)
                throw new InvalidOperationException("Deserialization returned null");

            var movieList = movies.ToList();
            if (movieList.Count != 0)
                throw new InvalidOperationException($"Expected 0 movies, got {movieList.Count}");

            _logger.LogInformation("✓ Empty array deserialization test passed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Empty array deserialization test failed");
            throw;
        }
    }

    /// <summary>
    /// Tests timestamp conversion from Unix epoch.
    /// </summary>
    public void TestUnixTimestampConversion()
    {
        _logger.LogInformation("Running: Unix timestamp conversion test");

        try
        {
            var sampleJson = @"{
                ""stream_id"": 1,
                ""name"": ""Test Movie"",
                ""added"": 1640000000,
                ""last_modified"": 1650000000
            }";

            var movie = JsonSerializer.Deserialize<XtreamMovie>(sampleJson, _jsonOptions);
            if (movie == null)
                throw new InvalidOperationException("Deserialization failed");

            var expectedDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1650000000);
            if (movie.LastModified != expectedDate)
                throw new InvalidOperationException($"Expected {expectedDate}, got {movie.LastModified}");

            _logger.LogInformation("✓ Unix timestamp conversion test passed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Unix timestamp conversion test failed");
            throw;
        }
    }

    /// <summary>
    /// Runs all tests.
    /// </summary>
    public void RunAllTests()
    {
        _logger.LogInformation("=== Starting Xtream Data Loading Test Suite ===");

        try
        {
            TestMovieDeserialization();
            TestSeriesDeserialization();
            TestChannelDeserialization();
            TestArrayDeserialization();
            TestEmptyArrayDeserialization();
            TestUnixTimestampConversion();

            _logger.LogInformation("=== All tests passed ✓ ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== Test suite failed ✗ ===");
            throw;
        }
    }

    private static void ValidateMovieData(XtreamMovie movie)
    {
        if (movie.Id == 0) throw new InvalidOperationException("Movie ID is missing");
        if (string.IsNullOrEmpty(movie.Name)) throw new InvalidOperationException("Movie name is missing");
        if (movie.Rating == null) throw new InvalidOperationException("Movie rating is missing");
        if (movie.Year == null) throw new InvalidOperationException("Movie year is missing");
    }

    private static void ValidateSeriesData(XtreamSeries series)
    {
        if (series.Id == 0) throw new InvalidOperationException("Series ID is missing");
        if (string.IsNullOrEmpty(series.Name)) throw new InvalidOperationException("Series name is missing");
        if (series.EpisodesCount == null) throw new InvalidOperationException("Episodes count is missing");
        if (series.SeasonsCount == null) throw new InvalidOperationException("Seasons count is missing");
    }

    private static void ValidateChannelData(XtreamChannel channel)
    {
        if (channel.Id == 0) throw new InvalidOperationException("Channel ID is missing");
        if (string.IsNullOrEmpty(channel.Name)) throw new InvalidOperationException("Channel name is missing");
    }
}
