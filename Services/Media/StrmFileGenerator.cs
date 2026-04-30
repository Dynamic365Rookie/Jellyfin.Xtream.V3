using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.LiveTv;
using Jellyfin.Xtream.V3;
using Jellyfin.Xtream.V3.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Services.Media;

/// <summary>
/// Generates .strm files on disk so movies and series appear in Jellyfin's standard library.
/// </summary>
public sealed class StrmFileGenerator
{
    private readonly IXtreamRepository<XtreamMovie> _movieRepo;
    private readonly IXtreamRepository<XtreamSeries> _seriesRepo;
    private readonly XtreamApiClient _apiClient;
    private readonly ILogger<StrmFileGenerator> _logger;

    private static readonly char[] InvalidFileChars = Path.GetInvalidFileNameChars();
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    public StrmFileGenerator(
        IXtreamRepository<XtreamMovie> movieRepo,
        IXtreamRepository<XtreamSeries> seriesRepo,
        XtreamApiClient apiClient,
        ILogger<StrmFileGenerator> logger)
    {
        _movieRepo = movieRepo;
        _seriesRepo = seriesRepo;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task GenerateAllAsync(CancellationToken ct)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.EnableStrmGeneration)
        {
            return;
        }

        _logger.LogInformation("[Xtream STRM] Starting STRM file generation...");

        var movieCount = GenerateMovieStrms(config);
        var seriesCount = await GenerateSeriesStrmsAsync(config, ct).ConfigureAwait(false);

        _logger.LogWarning(
            "[Xtream STRM] STRM generation complete — {Movies} movies, {Series} series processed",
            movieCount, seriesCount);
    }

    private int GenerateMovieStrms(PluginConfiguration config)
    {
        var moviesPath = config.StrmMoviesPath;
        if (string.IsNullOrWhiteSpace(moviesPath))
        {
            _logger.LogWarning("[Xtream STRM] StrmMoviesPath is empty, skipping movies");
            return 0;
        }

        _logger.LogWarning("[Xtream STRM] Generating movie STRMs to: {Path}", moviesPath);

        var movies = _movieRepo.GetAll().ToList();
        _logger.LogWarning("[Xtream STRM] Found {Count} movies in database", movies.Count);

        var count = 0;
        var errors = 0;

        foreach (var movie in movies)
        {
            try
            {
                var categoryFolder = SanitizeFileName(movie.CategoryName ?? "Uncategorized");
                var movieName = BuildMovieFileName(movie);
                var dirPath = Path.Combine(moviesPath, categoryFolder);
                var filePath = Path.Combine(dirPath, movieName + ".strm");

                if (File.Exists(filePath))
                {
                    count++;
                    continue;
                }

                Directory.CreateDirectory(dirPath);
                var url = StreamUrlResolver.ResolveMovie(movie.StreamId);
                File.WriteAllText(filePath, url);
                count++;
            }
            catch (Exception ex)
            {
                errors++;
                if (errors <= 5)
                {
                    _logger.LogWarning(ex, "[Xtream STRM] Failed to write STRM for movie {Name}", movie.Name);
                }
            }
        }

        if (errors > 5)
        {
            _logger.LogWarning("[Xtream STRM] {Errors} total movie STRM write errors (suppressed after 5)", errors);
        }

        return count;
    }

    private async Task<int> GenerateSeriesStrmsAsync(PluginConfiguration config, CancellationToken ct)
    {
        var seriesPath = config.StrmSeriesPath;
        if (string.IsNullOrWhiteSpace(seriesPath))
        {
            _logger.LogWarning("[Xtream STRM] StrmSeriesPath is empty, skipping series");
            return 0;
        }

        _logger.LogWarning("[Xtream STRM] Generating series STRMs to: {Path}", seriesPath);

        var allSeries = _seriesRepo.GetAll().ToList();
        _logger.LogWarning("[Xtream STRM] Found {Count} series in database", allSeries.Count);

        var count = 0;

        foreach (var series in allSeries)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var seriesFolderName = series.Year.HasValue && series.Year > 0
                    ? $"{SanitizeFileName(series.Name)} ({series.Year})"
                    : SanitizeFileName(series.Name);

                var seriesDirPath = Path.Combine(seriesPath, seriesFolderName);

                // Skip if series folder already has STRM files (incremental)
                if (Directory.Exists(seriesDirPath) && Directory.GetFiles(seriesDirPath, "*.strm", SearchOption.AllDirectories).Length > 0)
                {
                    count++;
                    continue;
                }

                var seriesInfo = await FetchSeriesInfo(series.SeriesId, ct).ConfigureAwait(false);
                if (seriesInfo?.Episodes == null || seriesInfo.Episodes.Count == 0)
                {
                    continue;
                }

                foreach (var (seasonKey, episodes) in seriesInfo.Episodes)
                {
                    var seasonNum = int.TryParse(seasonKey, out var sn) ? sn : 0;
                    var seasonFolder = $"Season {seasonNum:D2}";
                    var seasonDir = Path.Combine(seriesDirPath, seasonFolder);
                    Directory.CreateDirectory(seasonDir);

                    foreach (var ep in episodes)
                    {
                        var epName = string.IsNullOrWhiteSpace(ep.Name)
                            ? $"S{seasonNum:D2}E{ep.EpisodeNumber:D2}"
                            : $"S{seasonNum:D2}E{ep.EpisodeNumber:D2} - {SanitizeFileName(ep.Name)}";

                        var filePath = Path.Combine(seasonDir, epName + ".strm");
                        if (File.Exists(filePath))
                        {
                            continue;
                        }

                        if (int.TryParse(ep.StreamId, out var streamId))
                        {
                            var url = StreamUrlResolver.ResolveSeries(streamId);
                            File.WriteAllText(filePath, url);
                        }
                    }
                }

                count++;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[Xtream STRM] Failed to process series {Name}", series.Name);
            }
        }

        return count;
    }

    private async Task<XtreamSeriesInfo?> FetchSeriesInfo(int seriesId, CancellationToken ct)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return null;
            }

            var url = XtreamApiEndpoints.SeriesInfo(
                config.ServerUrl, config.Username, config.Password, seriesId);

            return await _apiClient.GetAsync<XtreamSeriesInfo>(url, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[Xtream STRM] Failed to fetch series info for {SeriesId}", seriesId);
            return null;
        }
    }

    private static string BuildMovieFileName(XtreamMovie movie)
    {
        var name = SanitizeFileName(movie.Name);
        if (movie.Year.HasValue && movie.Year > 0)
        {
            name = $"{name} ({movie.Year})";
        }

        return name;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Unknown";
        }

        var sanitized = name;
        foreach (var c in InvalidFileChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        // Also replace common problematic characters
        sanitized = sanitized.Replace(':', '-').Replace('/', '_').Replace('\\', '_');

        return sanitized.Trim().TrimEnd('.');
    }
}
