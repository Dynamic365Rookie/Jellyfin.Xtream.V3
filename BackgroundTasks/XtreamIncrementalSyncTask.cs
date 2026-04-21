using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Xtream.BackgroundTasks;

public sealed class XtreamIncrementalSyncTask : IScheduledTask
{
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly MemoryManager _memoryManager;
    private readonly ILogger<XtreamIncrementalSyncTask> _logger;

    public XtreamIncrementalSyncTask(ILoggerFactory loggerFactory)
    {
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

        try
        {
            using (_performanceMonitor.Track("XtreamFullSync"))
            {
                // TODO: Intégrer XtreamSyncService via DI quand les repositories seront configurés
                _logger.LogWarning("Synchronisation en mode découverte - les services de sync seront connectés dans une prochaine version");

                progress?.Report(0);
                await Task.Delay(100, cancellationToken);

                _memoryManager.CheckMemoryUsage("Vérification mémoire");

                progress?.Report(50);
                await Task.Delay(100, cancellationToken);

                _memoryManager.CheckMemoryUsage("Fin vérification");
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
