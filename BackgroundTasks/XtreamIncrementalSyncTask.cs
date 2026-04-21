using Jellyfin.Xtream.Services.Synchronization;
using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Utilities;
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
        PerformanceMonitor performanceMonitor,
        MemoryManager memoryManager,
        ILogger<XtreamIncrementalSyncTask> logger)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "Xtream IPTV - Synchronisation Incrémentale";

    public string Description => "Synchronise les films, séries et chaînes depuis l'API Xtream de manière optimisée";

    public string Category => "Xtream IPTV";

    public string Key => "XtreamIncrementalSync";

    // ✅ CORRECTION : nom + ordre des paramètres conformes à IScheduledTask Jellyfin 10.10+
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Démarrage de la synchronisation Xtream...");
        _memoryManager.LogMemoryUsage("Avant synchronisation");

        try
        {
            using (_performanceMonitor.Track("XtreamFullSync"))
            {
                // TODO: Récupérer l'URL de base depuis la configuration
                var baseUrl = "http://your-xtream-api.com";

                // Progress: 0-30% pour les films
                progress?.Report(0);
                await SyncWithProgress(
                    () => _syncService.SyncMoviesAsync($"{baseUrl}/movies", cancellationToken),
                    "Films",
                    progress,
                    0,
                    30);

                _memoryManager.CheckMemoryUsage("Après sync films");

                // Progress: 30-60% pour les séries
                progress?.Report(30);
                await SyncWithProgress(
                    () => _syncService.SyncSeriesAsync($"{baseUrl}/series", cancellationToken),
                    "Séries",
                    progress,
                    30,
                    60);

                _memoryManager.CheckMemoryUsage("Après sync séries");

                // Progress: 60-100% pour les chaînes
                progress?.Report(60);
                await SyncWithProgress(
                    () => _syncService.SyncChannelsAsync($"{baseUrl}/channels", cancellationToken),
                    "Chaînes",
                    progress,
                    60,
                    100);

                _memoryManager.CheckMemoryUsage("Après sync chaînes");
            }

            progress?.Report(100);

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
            // Nettoyage final
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


    private async Task SyncWithProgress(
        Func<Task> syncAction,
        string entityType,
        IProgress<double>? progress,
        double startProgress,
        double endProgress)
    {
        using (_performanceMonitor.Track($"Sync{entityType}"))
        {
            _logger.LogInformation("Synchronisation de {EntityType}...", entityType);

            await syncAction();

            progress?.Report(endProgress);

            _logger.LogInformation("Synchronisation de {EntityType} terminée", entityType);
        }
    }
}
