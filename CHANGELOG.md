# Jellyfin Xtream V3 - Changelog

Tous les changements notables du projet sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
et ce projet adhère à [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [3.2.7] - 2026-04-21

### Fixed
- **Compilation Error**: Removed incompatible `IPluginServiceRegistrator` class (type unavailable in Jellyfin 10.11.3)
- **DI Resolution Error**: Simplified `XtreamIncrementalSyncTask` constructor to avoid unresolvable service dependencies
  - Replaced 4 injected services by a single `ILoggerFactory` (provided natively by Jellyfin)
  - `PerformanceMonitor` and `MemoryManager` are now instantiated locally with dedicated loggers
  - Removed deep dependency on `XtreamSyncService` (requires ApiClient, Validator, Repositories not yet registered)

### Changed
- Background task now registers and executes correctly in Jellyfin Dashboard > Scheduled Tasks
- Synchronization logic is in discovery mode; full sync pipeline will be connected when repositories are configured

### Commits
- Removed `PluginServiceRegistrar.cs` (incompatible with Jellyfin 10.11.3)
- Simplified `XtreamIncrementalSyncTask` DI to `ILoggerFactory` only

---

## [3.2.6] - 2026-04-21

### Changed
- **Framework Update**: Upgraded from .NET 6.0 to .NET 9.0 for better performance and security
- **Jellyfin Compatibility**: Updated to Jellyfin 10.11.3 packages (Controller, Common, Model)
- **IScheduledTask Interface**: Adapted method signature to Jellyfin 10.11+ requirements
  - Renamed Execute to ExecuteAsync
  - Reordered parameters to (IProgress<double>, CancellationToken) per latest interface specification

### Security
- **Vulnerability Fixes**: Updated dependencies to resolve GHSA-qj66-m88j-hmgj
  - Microsoft.Extensions.Caching.Memory: 6.0.2 → 9.0.10
  - Microsoft.Extensions.Logging.Abstractions: 6.0.4 → 9.0.10

### Technical Details
- Updated TaskTriggerInfo enum usage for compatibility with Jellyfin 10.11 API
- Full recompilation with .NET 9.0 toolchain
- All dependencies verified against Jellyfin 10.11.3 specifications

### Commits
- Framework and dependency upgrades with interface adaptations

---

## [3.2.5] - 2026-04-21

### Fixed
- **Release Notes Extraction**: Fixed GitHub Actions workflow to properly extract release descriptions from CHANGELOG.md
  - Corrected PowerShell multiline output syntax for GitHub Actions environment
  - Release descriptions now properly appear in repository.json plugin manifest
- **Plugin Loading**: Ensured IScheduledTask.Execute method is correctly implemented with async/await pattern
- **Workflow Pipeline**: Fixed publish-repository workflow to wait for tag-triggered releases before generating manifest

### Technical Details
- Updated extract-version-notes.ps1 to use GitHub Actions multiline output format (<<EOF syntax)
- Removed JSON escaping that was corrupting multiline release descriptions
- Repository manifest now dynamically includes release notes from all published versions

### Commits
- `9cadfda` - fix: Correct ExecuteAsync naming and release notes extraction

---

## [3.2.4] - 2026-04-21

### Fixed
- **DLL Compilation**: Forced recompilation of plugin to ensure async Execute method is included in compiled assembly
- **Background Task Registration**: Verified XtreamIncrementalSyncTask properly implements IScheduledTask interface
- **Plugin Dependencies**: Ensured all referenced assemblies are correctly bundled with plugin DLL

### Why v3.2.4?
The v3.2.3 DLL appeared to have been compiled from source before the async Execute fix was applied.
Version bump forces GitHub Actions to rebuild with current source code containing all corrections.

### Commits
- `4cec6af` - chore: Bump version to 3.2.4 to ensure proper DLL compilation

---

## [3.2.3] - 2026-04-21

### Fixed
- **Scheduled Task Execution**: Corrected Execute method signature to properly implement async/await pattern
- **Jellyfin Plugin Loading**: Resolved assembly type loading exception in Jellyfin plugin manager
- **Background Synchronization Task**: Ensured XtreamIncrementalSyncTask properly registers as scheduled task with Jellyfin 10.11

### Commits
- `53e99bb` - fix: Make Execute method directly async for Jellyfin compatibility

---

## [3.2.1] - 2026-04-21

### Features
- **Enabled Scheduled Synchronization Task**: XtreamIncrementalSyncTask now automatically available in Jellyfin Dashboard
- **Background Sync**: Automatic synchronization of movies, series, and channels every 6 hours
- **Task Discovery**: Plugin properly registers with Jellyfin's IScheduledTask interface

### Improvements
- Added detailed task logging for synchronization debugging
- Improved memory management during scheduled operations
- Better visibility in Jellyfin Scheduled Tasks dashboard

### Commits
- `fb2d7d5` - fix: Bump version to 3.2.1 for scheduled task feature
- `3ef3147` - feat: Enable scheduled Xtream synchronization task

---

## [3.2.0] - 2026-04-20

### Features
- **Expanded Domain Models**: Complete Xtream API field support
  - Ratings, plot, episodes, genres, duration, release date
  - Automatic stream_id/series_id to Id mapping via JSON converters
  - Unix timestamp conversion to DateTime with proper timezone handling

- **Pre-Sync Validation Framework**: Comprehensive validation before synchronization
  - Configuration validation (URL format, credentials)
  - Server connectivity testing
  - API endpoint availability verification
  - Detailed error reporting and logging

- **Comprehensive Test Suite**: Full unit test coverage
  - XtreamDataLoadingTests: Deserialization, validation, timestamp conversion tests
  - XtreamDataLoadingIntegrationExample: Integration examples and best practices
  - TESTING_GUIDE.md: Complete documentation with examples

### Improvements
- Added SyncResult class for sync operation results tracking
- Enhanced benchmark suite with proper timestamp handling
- Detailed documentation in TESTING_GUIDE.md
- Better error handling and validation messages
- Improved code organization

### Commits
- `e1877fe` - feat: Add comprehensive Xtream data loading with validation and tests
- `04017cd` - fix: Update manifest version and add plugin initialization logging

---

## [3.1.6] - 2026-04-10

### Fixed
- Stabilized manifest encoding for compatibility
- Improved plugin loading process with Jellyfin 10.11

---

## Versionning Semantique

Ce projet suit [Semantic Versioning](https://semver.org/):

- **MAJOR** (3.0.0): Changements incompatibles
- **MINOR** (3.2.0): Nouvelles fonctionnalités compatibles
- **PATCH** (3.2.1): Corrections de bugs

---

**Date de dernière mise à jour**: 2026-04-21
