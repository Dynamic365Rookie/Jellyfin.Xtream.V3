# ?? Guide de D�marrage Rapide

## Installation

### 1. Restaurer les packages
```bash
cd Jellyfin.Xtream.V2
dotnet restore
dotnet build
```

### 2. V�rifier les d�pendances
Les packages suivants sont requis :
- ? `LiteDB` 5.0.21
- ? `Microsoft.Extensions.Caching.Memory` 6.0.1
- ? `Microsoft.Extensions.Logging.Abstractions` 6.0.4
- ? `MediaBrowser.Common` 4.9.1.90

---

## Configuration Minimale

### 1. Initialiser la base de donn�es optimis�e

```csharp
using Jellyfin.Xtream.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

// Cr�er la base de donn�es avec configuration optimis�e
var db = LiteDbConfiguration.CreateOptimizedDatabase("Filename=xtream.db");

// Cr�er les repositories
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
        // Synchronisation compl�te (parall�le)
        await syncService.SyncAllAsync("http://your-xtream-api.com", cancellationToken);

        // V�rifier la m�moire
        memManager.LogMemoryUsage("Apr�s synchronisation");
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

            Console.WriteLine("=== Jellyfin Xtream - D�marrage ===");
            Console.WriteLine($"BatchSize: {perfOptions.BatchSize}");
            Console.WriteLine($"MaxMemory: {perfOptions.MaxMemoryMB}MB");
            Console.WriteLine($"Parallelism: {perfOptions.MaxDegreeOfParallelism}");
            Console.WriteLine();

            // 3. Initialisation de la base de donn�es
            var db = LiteDbConfiguration.CreateOptimizedDatabase("Filename=xtream.db");
            Console.WriteLine("? Base de donn�es initialis�e");

            // 4. Cr�ation des repositories
            var movieRepo = new LiteDbXtreamRepository<XtreamMovie>(db, "movies");
            var seriesRepo = new LiteDbXtreamRepository<XtreamSeries>(db, "series");
            var channelRepo = new LiteDbXtreamRepository<XtreamChannel>(db, "channels");
            Console.WriteLine("? Repositories cr��s");

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
            Console.WriteLine("? Services initialis�s");
            Console.WriteLine();

            // 6. Afficher l'�tat initial
            Console.WriteLine("=== �tat Initial ===");
            Console.WriteLine($"Films: {movieRepo.Count()}");
            Console.WriteLine($"S�ries: {seriesRepo.Count()}");
            Console.WriteLine($"Cha�nes: {channelRepo.Count()}");
            memManager.LogMemoryUsage("Initial");
            Console.WriteLine();

            // 7. Lancer la synchronisation
            try
            {
                Console.WriteLine("=== D�marrage de la synchronisation ===");
                var cts = new CancellationTokenSource();

                // Permettre l'annulation avec Ctrl+C
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine("\n??  Annulation demand�e...");
                    cts.Cancel();
                    e.Cancel = true;
                };

                using (perfMonitor.Track("FullSync"))
                {
                    // TODO: Remplacer par votre URL Xtream r�elle
                    var baseUrl = "http://your-xtream-server.com";

                    await syncService.SyncAllAsync(baseUrl, cts.Token);
                }

                Console.WriteLine();
                Console.WriteLine("? Synchronisation termin�e avec succ�s");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("??  Synchronisation annul�e par l'utilisateur");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Erreur: {ex.Message}");
                logger.LogError(ex, "Erreur lors de la synchronisation");
            }
            finally
            {
                Console.WriteLine();

                // 8. Afficher l'�tat final
                Console.WriteLine("=== �tat Final ===");
                Console.WriteLine($"Films: {movieRepo.Count()}");
                Console.WriteLine($"S�ries: {seriesRepo.Count()}");
                Console.WriteLine($"Cha�nes: {channelRepo.Count()}");
                memManager.LogMemoryUsage("Final");
                Console.WriteLine();

                // 9. Afficher les statistiques de performance
                Console.WriteLine("=== Statistiques de Performance ===");
                perfMonitor.LogStatistics();
                Console.WriteLine();

                // 10. Nettoyage
                memManager.ForceGarbageCollection();
                db.Dispose();

                Console.WriteLine("? Nettoyage termin�");
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

// Test 1: Individual vs Batch (1000 entit�s)
var result1 = await benchmark.BenchmarkIndividualVsBatch(movieRepo, 1000);
Console.WriteLine(result1);

// Test 2: Change Detection (10000 entit�s, 10% de changements)
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

### R�sultats Attendus

Pour 15,000 films:
- **Individual upserts**: ~30-60 secondes
- **Batch upserts**: ~1-3 secondes
- **Improvement**: 10-20x plus rapide

---

## Configuration pour Diff�rentes Volum�tries

### Petite Volum�trie (< 5,000 entit�s)
```csharp
var options = PerformanceOptions.LowVolume;
```

### Volum�trie Moyenne (5,000 - 30,000 entit�s)
```csharp
var options = PerformanceOptions.Default;
```

### Haute Volum�trie (> 30,000 entit�s)
```csharp
var options = PerformanceOptions.HighVolume;
```

### Configuration Personnalis�e
```csharp
var options = new PerformanceOptions
{
    BatchSize = 1500,              // Ajuster selon volum�trie
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

### 1. Activer le Logging D�taill�

```csharp
builder.SetMinimumLevel(LogLevel.Debug); // Pour debugging
builder.SetMinimumLevel(LogLevel.Information); // Pour production
```

### 2. Surveiller la M�moire

```csharp
// V�rifier p�riodiquement
var timer = new Timer(_ =>
{
    memManager.CheckMemoryUsage("Periodic check");
}, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
```

### 3. Capturer les M�triques

```csharp
// � chaque synchronisation
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
**Solution**: Diminuer `BatchSize` � 500 et `MaxCacheEntries` � 5000

### ? Synchronisation trop lente
**Solution**: Augmenter `BatchSize` � 2000 et `MaxDegreeOfParallelism` � 8

### ? Timeouts API
**Solution**: Augmenter `ApiTimeoutSeconds` et activer `EnableApiRetry`

### ? Base de donn�es verrouill�e
**Solution**: V�rifier que le fichier DB n'est pas ouvert ailleurs

---

## Ressources

- ?? **PERFORMANCE_GUIDE.md** - Guide complet
- ?? **PERFORMANCE_OPTIMIZATIONS.md** - D�tails techniques

---

**Pr�t � commencer ? Ex�cutez l'exemple ci-dessus et regardez les performances !** ??
