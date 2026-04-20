using System.Diagnostics;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Infrastructure.Benchmarks;

/// <summary>
/// Classe utilitaire pour benchmarker les performances du repository
/// </summary>
public sealed class RepositoryBenchmark
{
    private readonly ILogger<RepositoryBenchmark> _logger;

    public RepositoryBenchmark(ILogger<RepositoryBenchmark> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Benchmark des opérations individuelles vs batch
    /// </summary>
    public async Task<BenchmarkResult> BenchmarkIndividualVsBatch(
        IXtreamRepository<XtreamMovie> repository,
        int entityCount)
    {
        _logger.LogInformation("Démarrage du benchmark: Individual vs Batch ({Count} entités)", entityCount);

        var movies = GenerateTestMovies(entityCount);

        // Test 1: Insertions individuelles
        var sw1 = Stopwatch.StartNew();
        foreach (var movie in movies)
        {
            repository.Upsert(movie);
        }
        sw1.Stop();
        var individualTime = sw1.Elapsed;

        _logger.LogInformation("Insertions individuelles: {Time}ms", individualTime.TotalMilliseconds);

        // Nettoyer
        var allIds = movies.Select(m => m.Id).ToList();
        repository.DeleteNotInList(new List<int>());

        // Test 2: Insertion par batch
        var sw2 = Stopwatch.StartNew();
        repository.UpsertBatch(movies);
        sw2.Stop();
        var batchTime = sw2.Elapsed;

        _logger.LogInformation("Insertion par batch: {Time}ms", batchTime.TotalMilliseconds);

        var improvement = (individualTime.TotalMilliseconds / batchTime.TotalMilliseconds);

        var result = new BenchmarkResult
        {
            EntityCount = entityCount,
            IndividualOperationTime = individualTime,
            BatchOperationTime = batchTime,
            ImprovementFactor = improvement,
            IndividualOperationsPerSecond = entityCount / individualTime.TotalSeconds,
            BatchOperationsPerSecond = entityCount / batchTime.TotalSeconds
        };

        _logger.LogInformation(
            "Résultat: Batch est {Improvement:F2}x plus rapide ({Individual:F0} ops/s -> {Batch:F0} ops/s)",
            improvement,
            result.IndividualOperationsPerSecond,
            result.BatchOperationsPerSecond);

        return result;
    }

    /// <summary>
    /// Benchmark de la détection de changements
    /// </summary>
    public async Task<BenchmarkResult> BenchmarkChangeDetection(
        IXtreamRepository<XtreamMovie> repository,
        int entityCount,
        double changePercentage)
    {
        _logger.LogInformation(
            "Démarrage du benchmark: Change Detection ({Count} entités, {Percent}% changements)",
            entityCount,
            changePercentage * 100);

        var movies = GenerateTestMovies(entityCount);

        // Insertion initiale
        repository.UpsertBatch(movies);

        // Modifier un pourcentage des entités
        var entitiesToChange = (int)(entityCount * changePercentage);
        for (int i = 0; i < entitiesToChange; i++)
        {
            movies[i] = movies[i] with { LastModified = DateTime.UtcNow };
        }

        // Méthode 1: Vérification individuelle (old way)
        var sw1 = Stopwatch.StartNew();
        var changedEntities1 = new List<XtreamMovie>();
        foreach (var movie in movies)
        {
            if (repository.HasChanged(movie))
            {
                changedEntities1.Add(movie);
            }
        }
        sw1.Stop();
        var individualTime = sw1.Elapsed;

        _logger.LogInformation(
            "Détection individuelle: {Time}ms, {Count} changements détectés",
            individualTime.TotalMilliseconds,
            changedEntities1.Count);

        // Méthode 2: Utilisation de GetLastModifiedMap (new way)
        var sw2 = Stopwatch.StartNew();
        var existingDates = repository.GetLastModifiedMap();
        var changedEntities2 = movies.Where(m =>
            !existingDates.TryGetValue(m.Id, out var existingDate) ||
            existingDate != m.LastModified
        ).ToList();
        sw2.Stop();
        var batchTime = sw2.Elapsed;

        _logger.LogInformation(
            "Détection par map: {Time}ms, {Count} changements détectés",
            batchTime.TotalMilliseconds,
            changedEntities2.Count);

        var improvement = individualTime.TotalMilliseconds / batchTime.TotalMilliseconds;

        var result = new BenchmarkResult
        {
            EntityCount = entityCount,
            IndividualOperationTime = individualTime,
            BatchOperationTime = batchTime,
            ImprovementFactor = improvement,
            ChangedEntityCount = changedEntities2.Count,
            ChangePercentage = changePercentage
        };

        _logger.LogInformation(
            "Résultat: Map-based est {Improvement:F2}x plus rapide",
            improvement);

        return result;
    }

    /// <summary>
    /// Benchmark complet simulant une synchronisation réelle
    /// </summary>
    public async Task<BenchmarkResult> BenchmarkFullSync(
        IXtreamRepository<XtreamMovie> repository,
        int existingCount,
        int newCount,
        double changePercentage)
    {
        _logger.LogInformation(
            "Démarrage du benchmark: Full Sync (Existing: {Existing}, New: {New}, Changes: {Percent}%)",
            existingCount,
            newCount,
            changePercentage * 100);

        // Préparer les données existantes
        var existing = GenerateTestMovies(existingCount);
        repository.UpsertBatch(existing);

        // Préparer les nouvelles données
        var incoming = GenerateTestMovies(newCount, startId: existingCount);

        // Modifier un pourcentage des entités existantes
        var entitiesToChange = (int)(existingCount * changePercentage);
        for (int i = 0; i < entitiesToChange && i < existing.Count; i++)
        {
            incoming.Add(existing[i] with { LastModified = DateTime.UtcNow });
        }

        // Simuler la synchronisation optimisée
        var sw = Stopwatch.StartNew();

        var existingDates = repository.GetLastModifiedMap();
        var toUpdate = incoming.Where(m =>
            !existingDates.TryGetValue(m.Id, out var existingDate) ||
            existingDate != m.LastModified
        ).ToList();

        repository.UpsertBatch(toUpdate);

        sw.Stop();

        var result = new BenchmarkResult
        {
            EntityCount = incoming.Count,
            BatchOperationTime = sw.Elapsed,
            ChangedEntityCount = toUpdate.Count,
            TotalOperations = toUpdate.Count,
            BatchOperationsPerSecond = toUpdate.Count / sw.Elapsed.TotalSeconds
        };

        _logger.LogInformation(
            "Full Sync complété en {Time}ms ({Updated} entités mises à jour, {OpsPerSec:F0} ops/s)",
            sw.Elapsed.TotalMilliseconds,
            toUpdate.Count,
            result.BatchOperationsPerSecond);

        return result;
    }

    private List<XtreamMovie> GenerateTestMovies(int count, int startId = 0)
    {
        var movies = new List<XtreamMovie>(count);
        var baseDate = DateTime.UtcNow.AddDays(-30);

        for (int i = 0; i < count; i++)
        {
            movies.Add(new XtreamMovie
            {
                Id = startId + i,
                Name = $"Test Movie {startId + i}",
                LastModified = baseDate.AddMinutes(i)
            });
        }

        return movies;
    }

    public class BenchmarkResult
    {
        public int EntityCount { get; init; }
        public TimeSpan IndividualOperationTime { get; init; }
        public TimeSpan BatchOperationTime { get; init; }
        public double ImprovementFactor { get; init; }
        public double IndividualOperationsPerSecond { get; init; }
        public double BatchOperationsPerSecond { get; init; }
        public int ChangedEntityCount { get; init; }
        public double ChangePercentage { get; init; }
        public int TotalOperations { get; init; }

        public override string ToString()
        {
            return $@"
Benchmark Results:
==================
Entities: {EntityCount}
Individual Time: {IndividualOperationTime.TotalMilliseconds:F2}ms
Batch Time: {BatchOperationTime.TotalMilliseconds:F2}ms
Improvement: {ImprovementFactor:F2}x
Individual Ops/s: {IndividualOperationsPerSecond:F0}
Batch Ops/s: {BatchOperationsPerSecond:F0}
Changed: {ChangedEntityCount} ({ChangePercentage:P})
";
        }
    }
}
