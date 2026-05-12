using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.V3.Observability;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Services.Synchronization;

public sealed class XtreamSyncService
{
    private readonly XtreamApiClient _api;
    private readonly XtreamSyncValidator _validator;
    private readonly IXtreamRepository<XtreamMovie> _movies;
    private readonly IXtreamRepository<XtreamSeries> _series;
    private readonly IXtreamRepository<XtreamChannel> _channels;
    private readonly SyncHistoryService _syncHistory;
    private readonly ILogger<XtreamSyncService> _logger;

    public XtreamSyncService(
        XtreamApiClient api,
        XtreamSyncValidator validator,
        IXtreamRepository<XtreamMovie> movies,
        IXtreamRepository<XtreamSeries> series,
        IXtreamRepository<XtreamChannel> channels,
        SyncHistoryService syncHistory,
        ILogger<XtreamSyncService> logger)
    {
        _api = api;
        _validator = validator;
        _movies = movies;
        _series = series;
        _channels = channels;
        _syncHistory = syncHistory;
        _logger = logger;
    }

    public async Task<SyncEntityResult> SyncMoviesAsync(string url, CancellationToken ct)
    {
        _logger.LogDebug("Starting movies synchronization...");
        var startTime = DateTime.UtcNow;

        var movies = await _api.GetAsync<IEnumerable<XtreamMovie>>(url, ct)
                     ?? Enumerable.Empty<XtreamMovie>();

        var moviesList = movies.ToList();
        _logger.LogDebug("Retrieved {Count} movies from API", moviesList.Count);

        var result = await SyncEntitiesOptimizedAsync(_movies, moviesList, ct);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogDebug("Movies synchronization completed in {Duration}ms", duration.TotalMilliseconds);

        return result;
    }

    public async Task<SyncEntityResult> SyncSeriesAsync(string url, CancellationToken ct)
    {
        _logger.LogDebug("Starting series synchronization...");
        var startTime = DateTime.UtcNow;

        var series = await _api.GetAsync<IEnumerable<XtreamSeries>>(url, ct)
                     ?? Enumerable.Empty<XtreamSeries>();

        var seriesList = series.ToList();
        _logger.LogDebug("Retrieved {Count} series from API", seriesList.Count);

        var result = await SyncEntitiesOptimizedAsync(_series, seriesList, ct);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogDebug("Series synchronization completed in {Duration}ms", duration.TotalMilliseconds);

        return result;
    }

    public async Task<SyncEntityResult> SyncChannelsAsync(string url, CancellationToken ct)
    {
        _logger.LogDebug("Starting channels synchronization...");
        var startTime = DateTime.UtcNow;

        var channels = await _api.GetAsync<IEnumerable<XtreamChannel>>(url, ct)
                       ?? Enumerable.Empty<XtreamChannel>();

        var channelsList = channels.ToList();
        _logger.LogDebug("Retrieved {Count} channels from API", channelsList.Count);

        var result = await SyncEntitiesOptimizedAsync(_channels, channelsList, ct);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogDebug("Channels synchronization completed in {Duration}ms", duration.TotalMilliseconds);

        return result;
    }

    private async Task<SyncEntityResult> SyncEntitiesOptimizedAsync<T>(
        IXtreamRepository<T> repository,
        List<T> newEntities,
        CancellationToken ct) where T : class, IEntity
    {
        if (!newEntities.Any())
        {
            _logger.LogDebug("No entities to sync");
            return new SyncEntityResult(0, 0, 0, 0);
        }

        ct.ThrowIfCancellationRequested();

        // Récupérer toutes les dates de modification existantes en une seule requête
        var existingLastModified = repository.GetLastModifiedMap();
        _logger.LogDebugIfEnabled("Loaded {Count} existing entities from database", existingLastModified.Count);

        // Déterminer quelles entités doivent être mises à jour
        var entitiesToUpdate = new List<T>();
        var hasLastModifiedProperty = typeof(T).GetProperty("LastModified") != null;

        foreach (var entity in newEntities)
        {
            if (!hasLastModifiedProperty)
            {
                // Si pas de LastModified, on met à jour tout
                entitiesToUpdate.Add(entity);
                continue;
            }

            var lastModProp = typeof(T).GetProperty("LastModified");
            var newLastModified = (DateTime)(lastModProp?.GetValue(entity) ?? DateTime.MinValue);

            if (!existingLastModified.TryGetValue(entity.Id, out var existingDate) ||
                existingDate != newLastModified)
            {
                entitiesToUpdate.Add(entity);
            }
        }

        _logger.LogDebug("Found {Count} entities to update out of {Total}",
            entitiesToUpdate.Count, newEntities.Count);

        var newCount = 0;
        var updatedCount = 0;

        if (entitiesToUpdate.Any())
        {
            // Compter les nouvelles entités vs mises à jour
            newCount = entitiesToUpdate.Count(e => !existingLastModified.ContainsKey(e.Id));
            updatedCount = entitiesToUpdate.Count - newCount;

            // Batch upsert pour meilleures performances
            await Task.Run(() => repository.UpsertBatch(entitiesToUpdate), ct);
            _logger.LogDebug("Batch upsert completed for {Count} entities", entitiesToUpdate.Count);
        }

        // Optionnel: Supprimer les entités qui n'existent plus dans l'API
        var newEntityIds = newEntities.Select(e => e.Id).ToHashSet();
        var entitiesToDelete = existingLastModified.Keys.Where(id => !newEntityIds.Contains(id)).ToList();

        if (entitiesToDelete.Any())
        {
            _logger.LogDebug("Removing {Count} obsolete entities", entitiesToDelete.Count);
            await Task.Run(() => repository.DeleteNotInList(newEntityIds), ct);
        }

        return new SyncEntityResult(newEntities.Count, newCount, updatedCount, entitiesToDelete.Count);
    }

    public sealed record SyncEntityResult(int Total, int New, int Updated, int Deleted);

    public async Task SyncAllAsync(string baseUrl, string username, string password, CancellationToken ct)
    {
        _logger.LogDebug("Starting full synchronization...");
        var startTime = DateTime.UtcNow;

        var cleanBaseUrl = baseUrl.TrimEnd('/');

        // Synchroniser en parallèle pour améliorer les performances
        var movieTask = SyncMoviesAsync(XtreamApiEndpoints.Movies(cleanBaseUrl, username, password), ct);
        var seriesTask = SyncSeriesAsync(XtreamApiEndpoints.Series(cleanBaseUrl, username, password), ct);
        var channelsTask = SyncChannelsAsync(XtreamApiEndpoints.LiveStreams(cleanBaseUrl, username, password), ct);

        await Task.WhenAll(movieTask, seriesTask, channelsTask);

        var movieResult = movieTask.Result;
        var seriesResult = seriesTask.Result;
        var channelsResult = channelsTask.Result;

        var duration = DateTime.UtcNow - startTime;

        // Créer l'entrée d'historique
        var history = new SyncHistory
        {
            StartTime = startTime,
            EndTime = DateTime.UtcNow,
            Status = "Success",
            DurationSeconds = duration.TotalSeconds,
            MoviesTotal = movieResult.Total,
            MoviesNew = movieResult.New,
            MoviesUpdated = movieResult.Updated,
            SeriesTotal = seriesResult.Total,
            SeriesNew = seriesResult.New,
            SeriesUpdated = seriesResult.Updated,
            ChannelsTotal = channelsResult.Total,
            ChannelsNew = channelsResult.New,
            ChannelsUpdated = channelsResult.Updated,
            ErrorCount = 0
        };

        _syncHistory.AddHistory(history);
        _logger.LogInformation(history.GetSummary());
    }

    /// <summary>
    /// Validates configuration and performs full synchronization if validation passes.
    /// </summary>
    public async Task<SyncResult> SyncAllWithValidationAsync(string baseUrl, string username, string password, CancellationToken ct)
    {
        _logger.LogDebug("Starting synchronized data load with validation");
        var startTime = DateTime.UtcNow;

        // Validate before sync
        var validationResult = await _validator.ValidateBeforeSyncAsync(baseUrl, username, password, ct);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Sync validation failed: {string.Join("; ", validationResult.Errors)}";
            _logger.LogError(errorMessage);

            var failedHistory = new SyncHistory
            {
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                Status = "Failed",
                DurationSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
                ErrorCount = validationResult.Errors.Count(),
                ErrorMessage = errorMessage
            };
            _syncHistory.AddHistory(failedHistory);

            return SyncResult.Failure(validationResult.Errors);
        }

        try
        {
            await SyncAllAsync(baseUrl, username, password, ct);
            return SyncResult.Success();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Synchronization was cancelled");

            var cancelledHistory = new SyncHistory
            {
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                Status = "Cancelled",
                DurationSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
                ErrorMessage = "Synchronization was cancelled"
            };
            _syncHistory.AddHistory(cancelledHistory);

            return SyncResult.Failure(new[] { "Synchronization was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during synchronization");

            var errorHistory = new SyncHistory
            {
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                Status = "Failed",
                DurationSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
                ErrorCount = 1,
                ErrorMessage = ex.Message
            };
            _syncHistory.AddHistory(errorHistory);

            return SyncResult.Failure(new[] { $"Synchronization error: {ex.Message}" });
        }
    }
}
