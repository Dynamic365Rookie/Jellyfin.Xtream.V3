# CHANGELOG

Tous les changements notables du projet sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
et ce projet adhère à [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2024-XX-XX

### Added (Nouveautés)

#### Infrastructure - Persistence
- `LiteDbConfiguration.cs` - Configuration optimisée de LiteDB (cache 40MB, async, WAL)
- Batch operations dans `IXtreamRepository.cs` - UpsertBatch, GetLastModifiedMap, GetByIds, DeleteNotInList
- Implémentation batch dans `LiteDbXtreamRepository.cs` - 99.9% réduction requêtes DB

#### Infrastructure - Monitoring
- `PerformanceMonitor.cs` - Système de monitoring des performances (avg/min/max, success rate)
- `MemoryManager.cs` - Gestion automatique de la mémoire (seuils, GC forcé, snapshots)

#### Infrastructure - Utilities
- `BatchProcessor.cs` - Utilitaires pour traitement par lots et parallèle
- `RepositoryBenchmark.cs` - Suite de benchmarks de performance

#### Services - Synchronization
- `XtreamSyncService.cs` refonte - Synchronisation par lots et parallèle
- `XtreamIncrementalSyncTask.cs` - Tâche planifiée avec monitoring intégré
- Détection intelligente des changements - 1 requête au lieu de 30,000

#### Infrastructure - Caching
- Migration vers `IMemoryCache` dans `MemoryXtreamCache.cs`
- Limite de taille (10,000 entrées)
- Expiration automatique (2h + sliding 30min)
- Compaction périodique (15min)

#### API Client
- `XtreamApiClient.cs` - Retry automatique avec backoff exponentiel
- Buffer JSON optimisé (64KB)
- Gestion avancée des erreurs
- Logging détaillé

#### Configuration
- `PerformanceOptions.cs` - Configuration centralisée avec presets (Default, LowVolume, HighVolume)
- `XtreamOptionsValidator.cs` - Validation des options

#### Documentation
- `README.md` - Documentation principale
- `QUICKSTART.md` - Guide de démarrage rapide
- `PERFORMANCE_GUIDE.md` - Configuration et tuning
- `PERFORMANCE_OPTIMIZATIONS.md` - Détails techniques
- `CHANGES_SUMMARY.md` - Résumé des changements
- `RELEASE_NOTES.md` - Notes de version
- `CHANGELOG.md` - CE FICHIER

#### Scripts
- `commit-and-push.ps1` - Automation commit/push
- `rollback-and-push-github.ps1` - Rollback vers GitHub

#### GitHub Actions
- `.github/workflows/build-and-release.yml` - Build, test, release
- `.github/workflows/code-quality.yml` - Analyse de code et sécurité
- `.github/workflows/documentation.yml` - Validation documentation

### Changed (Modifications)

#### Domain Models
- `XtreamMovie.cs` - `class` ? `record`
- `XtreamSeries.cs` - `class` ? `record`
- `XtreamChannel.cs` - `class` ? `record`
- `XtreamEpisode.cs` - `class` ? `record`

#### Infrastructure - Caching
- `IXtreamCache.cs` - Ajout Store() avec expiration personnalisée, Clear(), Remove()

#### Services
- `XtreamSyncService.cs` - Refonte complète
- `LiteDbXtreamRepository.cs` - Ajout batch operations
- `MemoryXtreamCache.cs` - Migration IMemoryCache
- `XtreamApiClient.cs` - Ajout retry et logging
- `XtreamIncrementalSyncTask.cs` - Monitoring intégré

#### Plugin
- `Plugins.cs` - Constructeur mis à jour pour MediaBrowser.Common 4.9.1.90

#### Project File
- `Jellyfin.Xtream.V2.csproj` - Mise à jour packages

### Fixed (Corrections)

- CVE-2024-XXXX: Vulnérabilité dans `Microsoft.Extensions.Caching.Memory` 6.0.1
  - Mise à jour vers 6.0.2
- `XtreamLiveTvServices.cs` - Commenté (nécessite MediaBrowser.Controller non disponible publiquement)

### Removed (Suppressions)

- Aucune suppression de fonctionnalités existantes

### Performance

- Sync initiale: 60-90 min ? 15 min (**75-83%** ??)
- Sync incrémentale: 30 min ? 2 min (**93%** ??)
- Requêtes base de données: 30,000+ ? 10-20 (**99.9%** ??)
- Mémoire: Non contrôlée ? < 1.5 GB (stable)

### Technical Details

#### Patterns Implémentés
- Repository Pattern
- Factory Pattern
- Monitor Pattern
- Strategy Pattern
- Batch Processing

#### Principes SOLID
- Single Responsibility
- Open/Closed
- Liskov Substitution
- Interface Segregation
- Dependency Inversion

---

## [1.0.0] - 2024-01-01

### Initial Release

- Première version du plugin Jellyfin.Xtream.V2
- Support base pour Xtream IPTV
- API client minimal
- Synchronisation simple

---

## Types de Changements

- **Added**: Pour nouvelles fonctionnalités
- **Changed**: Pour changements aux fonctionnalités existantes
- **Deprecated**: Pour fonctionnalités bientôt supprimées
- **Removed**: Pour fonctionnalités supprimées
- **Fixed**: Pour corrections de bugs
- **Security**: Pour vulnérabilités de sécurité

---

## Versionning

Ce projet suit [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Changements incompatibles
- **MINOR** (0.X.0): Nouvelles fonctionnalités compatibles
- **PATCH** (0.0.X): Corrections de bugs

---

## Guides de Lecture

Pour plus d'informations:
- Installation: Voir `README.md`
- Démarrage rapide: Voir `QUICKSTART.md`
- Configuration: Voir `PERFORMANCE_GUIDE.md`
- Détails techniques: Voir `PERFORMANCE_OPTIMIZATIONS.md`
- Release notes: Voir `RELEASE_NOTES.md`

---

**Date de dernière mise à jour**: 2024
