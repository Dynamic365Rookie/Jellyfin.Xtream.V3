using System.Diagnostics;
using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.LiveTv;
using Jellyfin.Xtream.Services.Mapping;
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

        var totalSw = Stopwatch.StartNew();

        var movieSw = Stopwatch.StartNew();
        var (movieCount, movieErrors) = GenerateMovieStrms(config);
        movieSw.Stop();

        var seriesSw = Stopwatch.StartNew();
        var (seriesCount, seriesErrors) = await GenerateSeriesStrmsAsync(config, ct).ConfigureAwait(false);
        seriesSw.Stop();

        totalSw.Stop();

        // Always log the summary at Information level
        _logger.LogInformation(
            "[Xtream STRM] Generation complete in {Total:F1}s — Movies: {Movies} ({MovieTime:F1}s, {MovieErrors} errors) | Series: {Series} ({SeriesTime:F1}s, {SeriesErrors} errors)",
            totalSw.Elapsed.TotalSeconds,
            movieCount, movieSw.Elapsed.TotalSeconds, movieErrors,
            seriesCount, seriesSw.Elapsed.TotalSeconds, seriesErrors);
    }

    private (int count, int errors) GenerateMovieStrms(PluginConfiguration config)
    {
        var moviesPath = config.StrmMoviesPath;
        if (string.IsNullOrWhiteSpace(moviesPath))
        {
            _logger.LogDebug("[Xtream STRM] StrmMoviesPath is empty, skipping movies");
            return (0, 0);
        }

        var movies = _movieRepo.GetAll().ToList();
        _logger.LogDebug("[Xtream STRM] Generating {Count} movie STRMs to: {Path}", movies.Count, moviesPath);

        var count = 0;
        var errors = 0;

        Parallel.ForEach(movies, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, movie =>
        {
            try
            {
                var categoryFolder = SanitizeFileName(movie.CategoryName ?? "Uncategorized");
                var movieName = BuildMovieFileName(movie, config.EnableTitleCleaning);
                var dirPath = Path.Combine(moviesPath, categoryFolder);
                var filePath = Path.Combine(dirPath, movieName + ".strm");

                Directory.CreateDirectory(dirPath);
                var url = StreamUrlResolver.ResolveMovie(movie.StreamId, movie.ContainerExtension);
                File.WriteAllText(filePath, url);
                Interlocked.Increment(ref count);
            }
            catch (Exception ex)
            {
                var currentErrors = Interlocked.Increment(ref errors);
                if (currentErrors <= 3)
                {
                    _logger.LogDebug(ex, "[Xtream STRM] Failed to write STRM for movie {Name}", movie.Name);
                }
            }
        });

        return (count, errors);
    }

    private async Task<(int count, int errors)> GenerateSeriesStrmsAsync(PluginConfiguration config, CancellationToken ct)
    {
        var seriesPath = config.StrmSeriesPath;
        if (string.IsNullOrWhiteSpace(seriesPath))
        {
            _logger.LogDebug("[Xtream STRM] StrmSeriesPath is empty, skipping series");
            return (0, 0);
        }

        var allSeries = _seriesRepo.GetAll().ToList();
        _logger.LogDebug("[Xtream STRM] Generating STRMs for {Count} series to: {Path}", allSeries.Count, seriesPath);

        var count = 0;
        var errors = 0;
        var maxConcurrency = config.MaxConcurrentRequests > 0 ? config.MaxConcurrentRequests : 20;

        await Parallel.ForEachAsync(
            allSeries,
            new ParallelOptions { MaxDegreeOfParallelism = maxConcurrency, CancellationToken = ct },
            async (series, token) =>
            {
                try
                {
                    var rawName = config.EnableTitleCleaning
                        ? ChannelNameCleaner.Clean(series.Name)
                        : series.Name;
                    var seriesFolderName = series.Year.HasValue && series.Year > 0
                        ? $"{SanitizeFileName(rawName)} ({series.Year})"
                        : SanitizeFileName(rawName);

                    var seriesDirPath = Path.Combine(seriesPath, seriesFolderName);

                    var seriesInfo = await FetchSeriesInfo(series.SeriesId, token).ConfigureAwait(false);
                    if (seriesInfo?.Episodes == null || seriesInfo.Episodes.Count == 0)
                    {
                        return;
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

                            if (int.TryParse(ep.StreamId, out var streamId))
                            {
                                var url = StreamUrlResolver.ResolveSeries(streamId, ep.ContainerExtension);
                                File.WriteAllText(filePath, url);
                            }
                        }
                    }

                    Interlocked.Increment(ref count);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errors);
                    _logger.LogDebug(ex, "[Xtream STRM] Failed to process series {Name}", series.Name);
                }
            }).ConfigureAwait(false);

        return (count, errors);
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

    private static string BuildMovieFileName(XtreamMovie movie, bool cleanTitle)
    {
        var rawName = cleanTitle ? ChannelNameCleaner.Clean(movie.Name) : movie.Name;
        var name = SanitizeFileName(rawName);
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

        sanitized = sanitized.Replace(':', '-').Replace('/', '_').Replace('\\', '_');

        return sanitized.Trim().TrimEnd('.');
    }
}
