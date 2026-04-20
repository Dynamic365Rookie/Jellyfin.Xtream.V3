# Configuration de Performance pour Jellyfin.Xtream.V2

## Volumétrie Cible
- 15,000 films
- 8,500 séries  
- 1,500 chaînes

## Optimisations Implémentées

### 1. Base de Données (LiteDB)

#### Configuration Optimale
```csharp
// Utiliser LiteDbConfiguration.CreateOptimizedDatabase()
var db = LiteDbConfiguration.CreateOptimizedDatabase("Filename=xtream.db");
```

#### Paramètres Clés
- **Cache Size**: 5000 pages (~40MB)
- **Connection Mode**: Shared pour meilleure concurrence
- **Async Mode**: Activé pour écritures asynchrones
- **Enum as Integer**: Activé pour sérialisation plus rapide

#### Index
- Index unique sur `Id` (toutes les entités)
- Index sur `LastModified` (pour détection de changements)

### 2. Synchronisation par Lots

#### Taille des Lots Recommandée
- **Movies**: 1000 entités par lot
- **Series**: 1000 entités par lot
- **Channels**: 500 entités par lot

#### Stratégie de Synchronisation
```csharp
// Synchronisation parallèle des différents types
await syncService.SyncAllAsync(baseUrl, cancellationToken);

// Synchronisation intelligente (uniquement les changements)
// Utilise GetLastModifiedMap() pour une seule requête DB
```

### 3. Cache Mémoire

#### Configuration
- **Taille Maximum**: 10,000 entrées
- **Expiration par Défaut**: 2 heures
- **Sliding Expiration**: 30 minutes
- **Compaction Automatique**: Toutes les 15 minutes

#### Utilisation
```csharp
// Le cache gère automatiquement l'expiration et la compaction
cache.Store(channelId, programs);
```

### 4. Gestion de la Mémoire

#### Monitoring
```csharp
var memoryManager = new MemoryManager(logger, maxMemoryMB: 2048);

// Vérifier périodiquement
memoryManager.CheckMemoryUsage("After sync");

// Logger l'utilisation
memoryManager.LogMemoryUsage("Current state");
```

#### Seuil d'Alerte
- **Seuil**: 80% de la mémoire maximale
- **Action**: Garbage collection forcé automatiquement

### 5. API Client

#### Rate Limiting
- Utilise `XtreamApiRateLimiter` pour éviter la surcharge
- Retry automatique avec backoff exponentiel

#### Buffer de Désérialisation
- **Buffer Size**: 64KB pour JSON
- **HttpCompletionOption**: ResponseHeadersRead pour streaming

### 6. Traitement Parallèle

#### Utilisation de BatchProcessor
```csharp
// Traiter en lots parallèles
await BatchProcessor.ProcessInParallelAsync(
    items, 
    maxDegreeOfParallelism: 4,
    async item => await ProcessItem(item),
    cancellationToken);
```

#### Recommandations
- **Max Parallelism**: 4-8 threads selon CPU
- **Éviter** la parallélisation excessive (overhead)

## Métriques de Performance Attendues

### Temps de Synchronisation Initiaux
- **15,000 films**: ~5-10 minutes
- **8,500 séries**: ~3-7 minutes
- **1,500 chaînes**: ~30-60 secondes

### Temps de Synchronisation Incrémentale
- Si 10% de changements: ~1-2 minutes au total
- Si 1% de changements: ~15-30 secondes au total

### Utilisation Mémoire
- **Au repos**: ~200-300 MB
- **Pendant sync**: ~500-800 MB (pics à ~1GB)
- **Après compaction**: retour à ~300-400 MB

### Taille Base de Données
- **Estimation**: ~500MB pour la volumétrie cible
- **Avec index**: +10-15%

## Monitoring des Performances

### Utiliser PerformanceMonitor
```csharp
using (performanceMonitor.Track("SyncMovies"))
{
    await syncService.SyncMoviesAsync(url, ct);
}

// Logger les statistiques
performanceMonitor.LogStatistics();
```

### Logs Recommandés
- Activer **LogLevel.Information** pour les opérations principales
- Activer **LogLevel.Debug** pour le debugging de performance
- Activer **LogLevel.Warning** pour les alertes mémoire

## Optimisations Futures

### Si Performance Insuffisante
1. **Migrer vers SQLite** avec indexes plus avancés
2. **Implémenter un cache distribué** (Redis)
3. **Partitionner les données** par catégories
4. **Utiliser des vues matérialisées** pour les requêtes fréquentes

### Si Mémoire Insuffisante
1. Réduire le cache size à 5,000 entrées
2. Diminuer l'expiration du cache à 1 heure
3. Augmenter la fréquence de compaction à 10 minutes
4. Traiter en lots plus petits (500 au lieu de 1000)

## Tests de Charge

### Script de Test
```bash
# TODO: Créer un script de génération de données de test
# Simuler 15K films + 8.5K séries + 1.5K chaînes
```

### Critères de Succès
- ? Synchronisation complète < 20 minutes
- ? Utilisation mémoire < 1.5 GB
- ? Taille DB < 1 GB
- ? Pas de crash ou timeout
- ? Logs sans erreurs critiques
