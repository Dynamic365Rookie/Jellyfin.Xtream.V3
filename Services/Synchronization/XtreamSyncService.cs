using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Services.Synchronization;

public sealed class XtreamSyncService
{
    private readonly XtreamApiClient _api;
    private readonly IXtreamRepository<XtreamMovie> _movies;
    private readonly IXtreamRepository<XtreamSeries> _series;
    private readonly IXtreamRepository<XtreamChannel> _channels;
    private readonly ILogger<XtreamSyncService> _logger;

    public XtreamSyncService(
        XtreamApiClient api,
        IXtreamRepository<XtreamMovie> movies,
        IXtreamRepository<XtreamSeries> series,
        IXtreamRepository<XtreamChannel> channels,
        ILogger<XtreamSyncService> logger)
    {
        _api = api;
        _movies = movies;
        _series = series;
        _channels = channels;
        _logger = logger;
    }

    public async Task SyncMoviesAsync(string url, CancellationToken ct)
    {
        _logger.LogInformation("Starting movies synchronization...");
        var startTime = DateTime.UtcNow;

        var movies = await _api.GetAsync<IEnumerable<XtreamMovie>>(url, ct)
                     ?? Enumerable.Empty<XtreamMovie>();

        var moviesList = movies.ToList();
        _logger.LogInformation("Retrieved {Count} movies from API", moviesList.Count);

        await SyncEntitiesOptimizedAsync(_movies, moviesList, ct);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Movies synchronization completed in {Duration}ms", duration.TotalMilliseconds);
    }

    public async Task SyncSeriesAsync(string url, CancellationToken ct)
    {
        _logger.LogInformation("Starting series synchronization...");
        var startTime = DateTime.UtcNow;

        var series = await _api.GetAsync<IEnumerable<XtreamSeries>>(url, ct)
                     ?? Enumerable.Empty<XtreamSeries>();

        var seriesList = series.ToList();
        _logger.LogInformation("Retrieved {Count} series from API", seriesList.Count);

        await SyncEntitiesOptimizedAsync(_series, seriesList, ct);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Series synchronization completed in {Duration}ms", duration.TotalMilliseconds);
    }

    public async Task SyncChannelsAsync(string url, CancellationToken ct)
    {
        _logger.LogInformation("Starting channels synchronization...");
        var startTime = DateTime.UtcNow;

        var channels = await _api.GetAsync<IEnumerable<XtreamChannel>>(url, ct)
                       ?? Enumerable.Empty<XtreamChannel>();

        var channelsList = channels.ToList();
        _logger.LogInformation("Retrieved {Count} channels from API", channelsList.Count);

        await SyncEntitiesOptimizedAsync(_channels, channelsList, ct);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Channels synchronization completed in {Duration}ms", duration.TotalMilliseconds);
    }

    private async Task SyncEntitiesOptimizedAsync<T>(
        IXtreamRepository<T> repository,
        List<T> newEntities,
        CancellationToken ct) where T : class, IEntity
    {
        if (!newEntities.Any())
        {
            _logger.LogInformation("No entities to sync");
            return;
        }

        ct.ThrowIfCancellationRequested();

        // Récupérer toutes les dates de modification existantes en une seule requête
        var existingLastModified = repository.GetLastModifiedMap();
        _logger.LogDebug("Loaded {Count} existing entities from database", existingLastModified.Count);

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

        _logger.LogInformation("Found {Count} entities to update out of {Total}", 
            entitiesToUpdate.Count, newEntities.Count);

        if (entitiesToUpdate.Any())
        {
            // Batch upsert pour meilleures performances
            await Task.Run(() => repository.UpsertBatch(entitiesToUpdate), ct);
            _logger.LogInformation("Batch upsert completed for {Count} entities", entitiesToUpdate.Count);
        }

        // Optionnel: Supprimer les entités qui n'existent plus dans l'API
        var newEntityIds = newEntities.Select(e => e.Id).ToHashSet();
        var entitiesToDelete = existingLastModified.Keys.Where(id => !newEntityIds.Contains(id)).ToList();
        
        if (entitiesToDelete.Any())
        {
            _logger.LogInformation("Removing {Count} obsolete entities", entitiesToDelete.Count);
            await Task.Run(() => repository.DeleteNotInList(newEntityIds), ct);
        }
    }

    public async Task SyncAllAsync(string baseUrl, CancellationToken ct)
    {
        _logger.LogInformation("Starting full synchronization...");
        var startTime = DateTime.UtcNow;

        // Synchroniser en parallèle pour améliorer les performances
        var tasks = new[]
        {
            SyncMoviesAsync($"{baseUrl}/movies", ct),
            SyncSeriesAsync($"{baseUrl}/series", ct),
            SyncChannelsAsync($"{baseUrl}/channels", ct)
        };

        await Task.WhenAll(tasks);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Full synchronization completed in {Duration}s", duration.TotalSeconds);
    }
}
