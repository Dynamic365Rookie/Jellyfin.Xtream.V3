using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Infrastructure.Utilities;
using Jellyfin.Xtream.Services.Media;
using Jellyfin.Xtream.Services.Synchronization;
using Jellyfin.Xtream.V3;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Xtream.BackgroundTasks;

public sealed class XtreamIncrementalSyncTask : IScheduledTask
{
    private readonly XtreamSyncService _syncService;
    private readonly StrmFileGenerator _strmGenerator;
    private readonly IXtreamRepository<XtreamMovie> _movies;
    private readonly IXtreamRepository<XtreamSeries> _series;
    private readonly IXtreamRepository<XtreamChannel> _channels;
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly MemoryManager _memoryManager;
    private readonly ILogger<XtreamIncrementalSyncTask> _logger;

    public XtreamIncrementalSyncTask(
        XtreamSyncService syncService,
        StrmFileGenerator strmGenerator,
        IXtreamRepository<XtreamMovie> movies,
        IXtreamRepository<XtreamSeries> series,
        IXtreamRepository<XtreamChannel> channels,
        ILoggerFactory loggerFactory)
    {
        _syncService = syncService;
        _strmGenerator = strmGenerator;
        _movies = movies;
        _series = series;
        _channels = channels;
        _logger = loggerFactory.CreateLogger<XtreamIncrementalSyncTask>();
        _performanceMonitor = new PerformanceMonitor(loggerFactory.CreateLogger<PerformanceMonitor>());
        _memoryManager = new MemoryManager(loggerFactory.CreateLogger<MemoryManager>());
    }

    public string Name => "Xtream IPTV - Synchronisation Incrémentale";

    public string Description => "Synchronise les films, séries et chaînes depuis l'API Xtream de manière optimisée";

    public string Category => "Xtream IPTV";

    public string Key => "XtreamIncrementalSync";

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            _logger.LogError("[Xtream] Plugin configuration not found. Sync aborted.");
            return;
        }

        if (string.IsNullOrWhiteSpace(config.ServerUrl))
        {
            _logger.LogWarning("[Xtream] Server URL not configured. Sync skipped.");
            return;
        }

        var startTime = DateTime.UtcNow;

        try
        {
            using (_performanceMonitor.Track("XtreamFullSync"))
            {
                progress?.Report(0);

                var result = await _syncService.SyncAllWithValidationAsync(
                    config.ServerUrl,
                    config.Username,
                    config.Password,
                    cancellationToken);

                if (!result.IsSuccess)
                {
                    _logger.LogError("[Xtream] Sync failed: {Errors}", string.Join("; ", result.Errors));
                    return;
                }

                progress?.Report(70);
            }

            // Generate STRM files for standard library integration
            await _strmGenerator.GenerateAllAsync(cancellationToken).ConfigureAwait(false);

            progress?.Report(100);

            var duration = DateTime.UtcNow - startTime;
            var movieCount = _movies.Count();
            var seriesCount = _series.Count();
            var channelCount = _channels.Count();

            _logger.LogInformation(
                "[Xtream] Sync complete in {Duration:F1}s — {Movies} movies, {Series} series, {Channels} channels",
                duration.TotalSeconds,
                movieCount,
                seriesCount,
                channelCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[Xtream] Sync cancelled by user");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Xtream] Sync error");
            throw;
        }
        finally
        {
            _memoryManager.ForceGarbageCollection();
        }
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.IntervalTrigger,
                IntervalTicks = TimeSpan.FromHours(6).Ticks
            }
        };
    }
}
