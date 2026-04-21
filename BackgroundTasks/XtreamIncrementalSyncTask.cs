using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Utilities;
using Jellyfin.Xtream.Services.Synchronization;
using Jellyfin.Xtream.V3;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Xtream.BackgroundTasks;

public sealed class XtreamIncrementalSyncTask : IScheduledTask
{
    private readonly XtreamSyncService _syncService;
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly MemoryManager _memoryManager;
    private readonly ILogger<XtreamIncrementalSyncTask> _logger;

    public XtreamIncrementalSyncTask(
        XtreamSyncService syncService,
        ILoggerFactory loggerFactory)
    {
        _syncService = syncService;
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
        _logger.LogInformation("Démarrage de la synchronisation Xtream...");
        _memoryManager.LogMemoryUsage("Avant synchronisation");

        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            _logger.LogError("Configuration du plugin introuvable. Synchronisation annulée.");
            return;
        }

        if (string.IsNullOrWhiteSpace(config.ServerUrl))
        {
            _logger.LogWarning("URL du serveur Xtream non configurée. Synchronisation ignorée.");
            return;
        }

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
                    _logger.LogError("Synchronisation échouée: {Errors}", string.Join("; ", result.Errors));
                    return;
                }

                progress?.Report(100);
            }

            _performanceMonitor.LogStatistics();
            _memoryManager.LogMemoryUsage("Fin de synchronisation");

            _logger.LogInformation("Synchronisation Xtream terminée avec succès");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Synchronisation Xtream annulée par l'utilisateur");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation Xtream");
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
