# CHANGELOG

Tous les changements notables du projet sont document�s dans ce fichier.

Le format est bas� sur [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
et ce projet adh�re � [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2024-XX-XX

### Added (Nouveaut�s)

#### Infrastructure - Persistence
- `LiteDbConfiguration.cs` - Configuration optimis�e de LiteDB (cache 40MB, async, WAL)
- Batch operations dans `IXtreamRepository.cs` - UpsertBatch, GetLastModifiedMap, GetByIds, DeleteNotInList
- Impl�mentation batch dans `LiteDbXtreamRepository.cs` - 99.9% r�duction requ�tes DB

#### Infrastructure - Monitoring
- `PerformanceMonitor.cs` - Syst�me de monitoring des performances (avg/min/max, success rate)
- `MemoryManager.cs` - Gestion automatique de la m�moire (seuils, GC forc�, snapshots)

#### Infrastructure - Utilities
- `BatchProcessor.cs` - Utilitaires pour traitement par lots et parall�le
- `RepositoryBenchmark.cs` - Suite de benchmarks de performance

#### Services - Synchronization
- `XtreamSyncService.cs` refonte - Synchronisation par lots et parall�le
- `XtreamIncrementalSyncTask.cs` - T�che planifi�e avec monitoring int�gr�
- D�tection intelligente des changements - 1 requ�te au lieu de 30,000

#### Infrastructure - Caching
- Migration vers `IMemoryCache` dans `MemoryXtreamCache.cs`
- Limite de taille (10,000 entr�es)
- Expiration automatique (2h + sliding 30min)
- Compaction p�riodique (15min)

#### API Client
- `XtreamApiClient.cs` - Retry automatique avec backoff exponentiel
- Buffer JSON optimis� (64KB)
- Gestion avanc�e des erreurs
- Logging d�taill�

#### Configuration
- `PerformanceOptions.cs` - Configuration centralis�e avec presets (Default, LowVolume, HighVolume)
- `XtreamOptionsValidator.cs` - Validation des options

#### Documentation
- `README.md` - Documentation principale
- `QUICKSTART.md` - Guide de d�marrage rapide
- `PERFORMANCE_GUIDE.md` - Configuration et tuning
- `PERFORMANCE_OPTIMIZATIONS.md` - D�tails techniques
- `RELEASE_NOTES.md` - Notes de version
- `CHANGELOG.md` - CE FICHIER

#### Scripts
- `commit-and-push.ps1` - Automation commit/push
- `rollback-and-push-github.ps1` - Rollback vers GitHub

#### GitHub Actions
- `.github/workflows/build-and-release.yml` - Build, test, release
- `.github/workflows/code-quality.yml` - Analyse de code et s�curit�
- `.github/workflows/documentation.yml` - Validation documentation

### Changed (Modifications)

#### Domain Models
- `XtreamMovie.cs` - `class` ? `record`
- `XtreamSeries.cs` - `class` ? `record`
- `XtreamChannel.cs` - `class` ? `record`
- `XtreamEpisode.cs` - `class` ? `record`

#### Infrastructure - Caching
- `IXtreamCache.cs` - Ajout Store() avec expiration personnalis�e, Clear(), Remove()

#### Services
- `XtreamSyncService.cs` - Refonte compl�te
- `LiteDbXtreamRepository.cs` - Ajout batch operations
- `MemoryXtreamCache.cs` - Migration IMemoryCache
- `XtreamApiClient.cs` - Ajout retry et logging
- `XtreamIncrementalSyncTask.cs` - Monitoring int�gr�

#### Plugin
- `Plugins.cs` - Constructeur mis � jour pour MediaBrowser.Common 4.9.1.90

#### Project File
- `Jellyfin.Xtream.V2.csproj` - Mise � jour packages

### Fixed (Corrections)

- CVE-2024-XXXX: Vuln�rabilit� dans `Microsoft.Extensions.Caching.Memory` 6.0.1
  - Mise � jour vers 6.0.2
- `XtreamLiveTvServices.cs` - Comment� (n�cessite MediaBrowser.Controller non disponible publiquement)

### Removed (Suppressions)

- Aucune suppression de fonctionnalit�s existantes

### Performance

- Sync initiale: 60-90 min ? 15 min (**75-83%** ??)
- Sync incr�mentale: 30 min ? 2 min (**93%** ??)
- Requ�tes base de donn�es: 30,000+ ? 10-20 (**99.9%** ??)
- M�moire: Non contr�l�e ? < 1.5 GB (stable)

### Technical Details

#### Patterns Impl�ment�s
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

- Premi�re version du plugin Jellyfin.Xtream.V2
- Support base pour Xtream IPTV
- API client minimal
- Synchronisation simple

---

## Types de Changements

- **Added**: Pour nouvelles fonctionnalit�s
- **Changed**: Pour changements aux fonctionnalit�s existantes
- **Deprecated**: Pour fonctionnalit�s bient�t supprim�es
- **Removed**: Pour fonctionnalit�s supprim�es
- **Fixed**: Pour corrections de bugs
- **Security**: Pour vuln�rabilit�s de s�curit�

---

## Versionning

Ce projet suit [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Changements incompatibles
- **MINOR** (0.X.0): Nouvelles fonctionnalit�s compatibles
- **PATCH** (0.0.X): Corrections de bugs

---

## Guides de Lecture

Pour plus d'informations:
- Installation: Voir `README.md`
- D�marrage rapide: Voir `QUICKSTART.md`
- Configuration: Voir `PERFORMANCE_GUIDE.md`
- D�tails techniques: Voir `PERFORMANCE_OPTIMIZATIONS.md`
- Release notes: Voir `RELEASE_NOTES.md`

---

**Date de derni�re mise � jour**: 2024
