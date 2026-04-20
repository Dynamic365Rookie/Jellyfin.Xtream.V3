# ?? Guide de Démarrage Rapide

## Installation

### 1. Restaurer les packages
```bash
cd Jellyfin.Xtream.V2
dotnet restore
dotnet build
```

### 2. Vérifier les dépendances
Les packages suivants sont requis :
- ? `LiteDB` 5.0.21
- ? `Microsoft.Extensions.Caching.Memory` 6.0.1
- ? `Microsoft.Extensions.Logging.Abstractions` 6.0.4
- ? `MediaBrowser.Common` 4.9.1.90

---

## Configuration Minimale

### 1. Initialiser la base de données optimisée

```csharp
using Jellyfin.Xtream.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

// Créer la base de données avec configuration optimisée
var db = LiteDbConfiguration.CreateOptimizedDatabase("Filename=xtream.db");

// Créer les repositories
var movieRepo = new LiteDbXtreamRepository<XtreamMovie>(db, "movies");
var seriesRepo = new LiteDbXtreamRepository<XtreamSeries>(db, "series");
var channelRepo = new LiteDbXtreamRepository<XtreamChannel>(db, "channels");
```

### 2. Configurer les services

```csharp
using Jellyfin.Xtream.Services.Synchronization;
using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Utilities;
using Jellyfin.Xtream.Configuration;

// Configuration de performance
var perfOptions = PerformanceOptions.Default;

// Services de monitoring
var logger = loggerFactory.CreateLogger<XtreamSyncService>();
var perfMonitor = new PerformanceMonitor(logger);
var memManager = new MemoryManager(logger, perfOptions.MaxMemoryMB);

// API Client
var httpClient = new HttpClient();
var rateLimiter = new XtreamApiRateLimiter();
var apiClient = new XtreamApiClient(httpClient, rateLimiter, logger);

// Service de synchronisation
var syncService = new XtreamSyncService(
    apiClient,
    movieRepo,
    seriesRepo,
    channelRepo,
    logger
);
```

### 3. Lancer la synchronisation

```csharp
using (perfMonitor.Track("FirstSync"))
{
    try
    {
        // Synchronisation complète (parallèle)
        await syncService.SyncAllAsync("http://your-xtream-api.com", cancellationToken);

        // Vérifier la mémoire
        memManager.LogMemoryUsage("Après synchronisation");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de la synchronisation");
    }
    finally
    {
        // Afficher les statistiques
        perfMonitor.LogStatistics();
    }
}
```

---

## Exemple Complet

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.Synchronization;
using Jellyfin.Xtream.Infrastructure.Monitoring;
using Jellyfin.Xtream.Infrastructure.Utilities;
using Jellyfin.Xtream.Configuration;
using Jellyfin.Xtream.Domain.Models;

namespace Jellyfin.Xtream.Example
{
    public class QuickStart
    {
        public static async Task Main(string[] args)
        {
            // 1. Configuration du logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<XtreamSyncService>();
            var perfLogger = loggerFactory.CreateLogger<PerformanceMonitor>();
            var memLogger = loggerFactory.CreateLogger<MemoryManager>();

            // 2. Configuration de performance
            var perfOptions = PerformanceOptions.Default;
            perfOptions.Validate();

            Console.WriteLine("=== Jellyfin Xtream - Démarrage ===");
            Console.WriteLine($"BatchSize: {perfOptions.BatchSize}");
            Console.WriteLine($"MaxMemory: {perfOptions.MaxMemoryMB}MB");
            Console.WriteLine($"Parallelism: {perfOptions.MaxDegreeOfParallelism}");
            Console.WriteLine();

            // 3. Initialisation de la base de données
            var db = LiteDbConfiguration.CreateOptimizedDatabase("Filename=xtream.db");
            Console.WriteLine("? Base de données initialisée");

            // 4. Création des repositories
            var movieRepo = new LiteDbXtreamRepository<XtreamMovie>(db, "movies");
            var seriesRepo = new LiteDbXtreamRepository<XtreamSeries>(db, "series");
            var channelRepo = new LiteDbXtreamRepository<XtreamChannel>(db, "channels");
            Console.WriteLine("? Repositories créés");

            // 5. Initialisation des services
            var perfMonitor = new PerformanceMonitor(perfLogger);
            var memManager = new MemoryManager(memLogger, perfOptions.MaxMemoryMB);

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(perfOptions.ApiTimeoutSeconds)
            };

            var rateLimiter = new XtreamApiRateLimiter();
            var apiClient = new XtreamApiClient(httpClient, rateLimiter, logger);

            var syncService = new XtreamSyncService(
                apiClient,
                movieRepo,
                seriesRepo,
                channelRepo,
                logger
            );
            Console.WriteLine("? Services initialisés");
            Console.WriteLine();

            // 6. Afficher l'état initial
            Console.WriteLine("=== État Initial ===");
            Console.WriteLine($"Films: {movieRepo.Count()}");
            Console.WriteLine($"Séries: {seriesRepo.Count()}");
            Console.WriteLine($"Chaînes: {channelRepo.Count()}");
            memManager.LogMemoryUsage("Initial");
            Console.WriteLine();

            // 7. Lancer la synchronisation
            try
            {
                Console.WriteLine("=== Démarrage de la synchronisation ===");
                var cts = new CancellationTokenSource();

                // Permettre l'annulation avec Ctrl+C
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine("\n??  Annulation demandée...");
                    cts.Cancel();
                    e.Cancel = true;
                };

                using (perfMonitor.Track("FullSync"))
                {
                    // TODO: Remplacer par votre URL Xtream réelle
                    var baseUrl = "http://your-xtream-server.com";

                    await syncService.SyncAllAsync(baseUrl, cts.Token);
                }

                Console.WriteLine();
                Console.WriteLine("? Synchronisation terminée avec succès");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("??  Synchronisation annulée par l'utilisateur");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Erreur: {ex.Message}");
                logger.LogError(ex, "Erreur lors de la synchronisation");
            }
            finally
            {
                Console.WriteLine();

                // 8. Afficher l'état final
                Console.WriteLine("=== État Final ===");
                Console.WriteLine($"Films: {movieRepo.Count()}");
                Console.WriteLine($"Séries: {seriesRepo.Count()}");
                Console.WriteLine($"Chaînes: {channelRepo.Count()}");
                memManager.LogMemoryUsage("Final");
                Console.WriteLine();

                // 9. Afficher les statistiques de performance
                Console.WriteLine("=== Statistiques de Performance ===");
                perfMonitor.LogStatistics();
                Console.WriteLine();

                // 10. Nettoyage
                memManager.ForceGarbageCollection();
                db.Dispose();

                Console.WriteLine("? Nettoyage terminé");
            }

            Console.WriteLine();
            Console.WriteLine("Appuyez sur une touche pour quitter...");
            Console.ReadKey();
        }
    }
}
```

---

## Test de Performance

### Benchmark Simple

```csharp
using Jellyfin.Xtream.Infrastructure.Benchmarks;

var benchmark = new RepositoryBenchmark(logger);

// Test 1: Individual vs Batch (1000 entités)
var result1 = await benchmark.BenchmarkIndividualVsBatch(movieRepo, 1000);
Console.WriteLine(result1);

// Test 2: Change Detection (10000 entités, 10% de changements)
var result2 = await benchmark.BenchmarkChangeDetection(movieRepo, 10000, 0.1);
Console.WriteLine(result2);

// Test 3: Full Sync Simulation
var result3 = await benchmark.BenchmarkFullSync(
    movieRepo,
    existingCount: 15000,
    newCount: 15000,
    changePercentage: 0.1
);
Console.WriteLine(result3);
```

### Résultats Attendus

Pour 15,000 films:
- **Individual upserts**: ~30-60 secondes
- **Batch upserts**: ~1-3 secondes
- **Improvement**: 10-20x plus rapide

---

## Configuration pour Différentes Volumétries

### Petite Volumétrie (< 5,000 entités)
```csharp
var options = PerformanceOptions.LowVolume;
```

### Volumétrie Moyenne (5,000 - 30,000 entités)
```csharp
var options = PerformanceOptions.Default;
```

### Haute Volumétrie (> 30,000 entités)
```csharp
var options = PerformanceOptions.HighVolume;
```

### Configuration Personnalisée
```csharp
var options = new PerformanceOptions
{
    BatchSize = 1500,              // Ajuster selon volumétrie
    MaxCacheEntries = 15000,       // Plus grand cache
    MaxMemoryMB = 3072,            // 3GB de RAM
    MaxDegreeOfParallelism = 6,    // Plus de threads
    EnablePerformanceLogging = true,
    EnableMemoryMonitoring = true
};

options.Validate(); // Toujours valider
```

---

## Monitoring en Production

### 1. Activer le Logging Détaillé

```csharp
builder.SetMinimumLevel(LogLevel.Debug); // Pour debugging
builder.SetMinimumLevel(LogLevel.Information); // Pour production
```

### 2. Surveiller la Mémoire

```csharp
// Vérifier périodiquement
var timer = new Timer(_ =>
{
    memManager.CheckMemoryUsage("Periodic check");
}, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
```

### 3. Capturer les Métriques

```csharp
// À chaque synchronisation
using (perfMonitor.Track("Sync"))
{
    await syncService.SyncAllAsync(url, ct);
}

// Analyser les tendances
perfMonitor.LogStatistics();
```

---

## Troubleshooting Rapide

### ? "Out of Memory Exception"
**Solution**: Diminuer `BatchSize` à 500 et `MaxCacheEntries` à 5000

### ? Synchronisation trop lente
**Solution**: Augmenter `BatchSize` à 2000 et `MaxDegreeOfParallelism` à 8

### ? Timeouts API
**Solution**: Augmenter `ApiTimeoutSeconds` et activer `EnableApiRetry`

### ? Base de données verrouillée
**Solution**: Vérifier que le fichier DB n'est pas ouvert ailleurs

---

## Ressources

- ?? **PERFORMANCE_GUIDE.md** - Guide complet
- ?? **PERFORMANCE_OPTIMIZATIONS.md** - Détails techniques
- ?? **CHANGES_SUMMARY.md** - Liste des modifications

---

**Prêt à commencer ? Exécutez l'exemple ci-dessus et regardez les performances !** ??
