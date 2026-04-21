# Jellyfin Xtream V3 - Changelog

Tous les changements notables du projet sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
et ce projet adhère à [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [3.2.4] - 2026-04-21

### Fixed
- **Plugin Loading Issue**: Ensured DLL compilation includes async Execute method fix from v3.2.3
- **Background Task Registration**: Verified IScheduledTask interface implementation is correctly compiled

### Commits
- Recompile v3.2.3 fix with proper DLL generation

---

## [3.2.3] - 2026-04-21

### Fixed
- **Scheduled Task Execution**: Fixed Execute method to be directly async for IScheduledTask interface compatibility
- **Jellyfin Plugin Loading**: Resolved "ExecuteAsync method does not have an implementation" error
- **Background Synchronization**: Ensured XtreamIncrementalSyncTask properly implements async pattern

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
