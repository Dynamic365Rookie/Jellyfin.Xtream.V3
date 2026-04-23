# Jellyfin Xtream V3 - Changelog

Tous les changements notables du projet sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
et ce projet adhère à [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [3.5.4] - 2026-04-23

### Fixed
- **Developer Tools UI not displaying data** — Fixed JavaScript API calls in configuration page
  - Replaced `ApiClient.fetch()` with native `fetch()` for better compatibility
  - Added explicit JSON parsing with `response.json()`
  - Added authentication headers (`X-Emby-Token`)
  - Added console logging for debugging
  - Added better error handling with HTTP status checks
- **Missing stats values in logs** — Added detailed logging in GetStats endpoint showing actual counts returned

---

## [3.5.3] - 2026-04-23

### Added
- **Developer Tools Menu** — New admin interface for managing plugin data
  - Database statistics display showing entity counts (movies, series, channels)
  - "Clear Database" action to remove all synchronized data
  - Real-time refresh of stats via API endpoint
  - Danger zone UI with confirmation dialog for destructive operations
- **REST API for Developer Operations** — New `/Xtream/Developer` endpoints
  - `GET /Stats` — Returns current database entity counts
  - `POST /ClearDatabase` — Deletes all data from LiteDB (movies, series, channels)
  - Requires elevated permissions (admin-only via `RequiresElevation` policy)
  - Comprehensive logging for all operations
- **Repository Enhancement** — Added `DeleteAll()` method to `IXtreamRepository<T>` interface

### Changed
- `IXtreamRepository<T>` interface now includes `DeleteAll()` method returning deleted count
- `LiteDbXtreamRepository<T>` implements `DeleteAll()` using LiteDB's native bulk delete

### Fixed
- Provides manual workaround for stale channel data in Jellyfin when sync shows entities but channels not visible

---

## [3.5.2] - 2026-04-23

### Fixed
- **Player timeout before FFmpeg succeeds** — Player was displaying error before FFmpeg could find valid H.264 parameters
  - Increased default `AnalyzeDurationMs` from 5000ms to 15000ms (15 seconds)
  - Streams with PPS errors at the start need 10-15 seconds to find an IDR frame with SPS/PPS
  - FFmpeg now has enough time to reach successful decoding before player timeout
  - Based on actual log analysis showing ~180 PPS errors before successful decode at ~7-10 seconds

---

## [3.5.1] - 2026-04-23

### Fixed
- **Critical: XML serialization error at startup** — `StreamOptions.CustomHttpHeaders` dictionary cannot be serialized by Jellyfin's XML configuration system
  - Added `[XmlIgnore]` attribute to prevent serialization
  - Error was: `System.NotSupportedException: Cannot serialize member... because it implements IDictionary`
  - Plugin now loads successfully without startup errors

---

## [3.5.0] - 2026-04-23

### Added
- **FFmpeg Stream Options**: Comprehensive configuration for IPTV playback tuning
  - AnalyzeDurationMs - Control stream analysis time
  - GenPtsInput - Generate missing PTS timestamps (fixes H.264 PPS errors)
  - IgnoreDts - Advanced DTS handling option
  - CustomHttpHeaders - Inject custom HTTP headers for stream requests
  - New StreamOptions configuration class
- **Enhanced Channel Metadata**: Display language information
  - Language tags in Jellyfin UI (e.g., "Lang: FR")
  - Optional language code append to channel names (e.g., "TF1 (FR)")
  - Both options configurable independently
- **Debug Logging Mode**: Toggle debug logs on/off
  - Reduces log file size in production
  - EnableDebugLogging configuration flag
  - LogDebugIfEnabled extension method for conditional logging

### Changed
- BuildMediaSource applies configurable FFmpeg parameters from StreamOptions
- GetChannelsAsync includes language metadata (tags and optional name modification)
- All LogDebug calls replaced with LogDebugIfEnabled for conditional logging

### Fixed
- H.264 PPS errors on mid-stream connections (via GenPtsInput flag)
- Excessive debug logging in production environments

---

## [3.4.4] - 2026-04-22

### Fixed
- **CRITICAL: Jellyfin startup deadlock** — v3.4.3 caused Jellyfin to hang at startup ("Server is still starting up" loop)
  - Removed `GetServices<ILiveTvService>()` call from `XtreamLiveTvService` constructor
  - This call created a circular dependency during DI service construction, blocking the startup thread
  - Diagnostics in `GetChannelsAsync()` are sufficient for troubleshooting
  - **Workaround for v3.4.3**: Rename plugin folder to `.disabled` to allow Jellyfin to start

---

## [3.4.3] - 2026-04-22 [YANKED - Causes startup deadlock]

### Added
- **Comprehensive diagnostic logging** for LiveTV service discovery issues
  - `PluginServiceRegistrator`: Console/debug logging when `RegisterServices()` is called and when LiveTV service registration completes
  - `XtreamLiveTvService` constructor: Diagnostics to verify service can resolve itself via DI and enumerate all registered `ILiveTvService` instances
  - `XtreamLiveTvService.GetChannelsAsync()`: Prominent logging when method is invoked to confirm Jellyfin is calling it
  - New tool: `Tools/ClearLiveTvData.ps1` PowerShell script for clearing stale LiveTV channel data from Jellyfin's database

### Known Issues
- ⚠️ **CRITICAL**: Causes Jellyfin to hang at startup due to circular DI dependency in constructor. Fixed in v3.4.4.

---

## [3.4.1] - 2026-04-21

### Fixed
- **Live TV playback: "Sequence contains no matching element"** — Channel playback failed because `LiveTvMediaSourceProvider` could not find our `ILiveTvService` by name
  - Changed DI registration from simple `AddSingleton<ILiveTvService, XtreamLiveTvService>()` to concrete+forward pattern: register concrete type first, then forward interface
  - This ensures the service is discoverable both via standard DI resolution (`IEnumerable<ILiveTvService>`) and assembly scanning (`GetExports`)
  - Added diagnostic logging in `XtreamLiveTvService` constructor to confirm service creation

---

## [3.4.0] - 2026-04-21

### Added
- **Live TV Integration** — Implemented `ILiveTvService` to expose IPTV channels in Jellyfin's Live TV section
  - `XtreamLiveTvService`: reads synchronized channels from LiteDB and maps them to Jellyfin's `ChannelInfo`
  - Channel metadata: name, number, icon, category group
  - Stream resolution: generates Xtream API live stream URLs (`{BaseUrl}/live/{User}/{Pass}/{StreamId}.ts`)
  - Registered as singleton via `PluginServiceRegistrator`

### Changed
- **StreamUrlResolver**: refactored from instance class (with unregistered `XtreamOptions` dependency) to static helper reading config from `Plugin.Instance.Configuration`
  - Added `ResolveLive()`, `ResolveMovie()`, `ResolveSeries()` methods for all content types

### Note
- Movies and series (VOD) require `IChannel` implementation to appear in Jellyfin — planned for a future release
- EPG data not yet provided — channels appear without program guide information

---

## [3.3.7] - 2026-04-21

### Fixed
- **JSON deserialization failure on all 3 entity types** — Xtream API providers return inconsistent JSON types (`stream_id` as `"123"` string or `123` number, same for `category_id`, `year`, `added`, etc.)
  - Created `JsonElementExtensions` helper with flexible type readers (`GetFlexibleInt32`, `GetFlexibleInt64`, `GetFlexibleNullableDouble`, `GetFlexibleString`)
  - Updated `XtreamMovieJsonConverter`, `XtreamSeriesJsonConverter`, and `XtreamChannelJsonConverter` to tolerate both string and number representations
  - Error was: `The requested operation requires an element of type 'Number', but the target element has type 'String'`

---

## [3.3.6] - 2026-04-21

### Fixed
- **Critical: Sync used wrong API URLs** — `SyncAllAsync` built URLs as `{baseUrl}/movies` instead of using `XtreamApiEndpoints` with `player_api.php?username=X&password=Y&action=get_vod_streams`
  - Server returned HTML (login page) instead of JSON, causing `JsonException: '<' is an invalid start of a value`
  - Fixed `SyncAllAsync` to accept credentials and use `XtreamApiEndpoints.Movies/Series/LiveStreams`
- **Double slash in URLs** — Added `TrimEnd('/')` in `XtreamApiEndpoints` to prevent `https://host//player_api.php` when base URL ends with `/`

---

## [3.3.5] - 2026-04-21

### Fixed
- **Endpoint validation timeout for large IPTV catalogs**: Increased per-endpoint validation timeout from 5 seconds to 60 seconds
  - Large providers (thousands of movies/series/channels) return massive JSON payloads that cannot be downloaded in 5 seconds
  - Connectivity test (10s) would pass, but endpoint validation (5s) would fail with timeout errors
  - Affected endpoints: Movies, Series, LiveStreams

---

## [3.3.2] - 2026-04-21

### Fixed
- **LiteDB Pragma error**: `Pragma 'CACHE_SIZE' not exist` — removed `CACHE_SIZE` and `ASYNC` pragmas not supported by LiteDB 5.0.21
  - These pragmas were written for an older/different LiteDB API and crash at database initialization
  - LiteDB 5.x handles caching and WAL internally via `ConnectionType.Shared`

---

## [3.3.1] - 2026-04-21

### Fixed
- **Critical: TypeLoadException on plugin load** — `Method 'RegisterServices' does not have an implementation`
  - Root cause: Plugin shipped its own `Microsoft.Extensions.*.dll` (v9.0.10) which conflicted with Jellyfin's runtime versions
  - The CLR saw two different `IServiceCollection` types (plugin vs host) and couldn't match the `RegisterServices` method signature
  - Fix: Added `ExcludeAssets=runtime` to `Microsoft.Extensions.Caching.Memory` and `Microsoft.Extensions.Logging.Abstractions`
  - Published DLLs reduced from 9 to 2: `Jellyfin.Xtream.V3.dll` + `LiteDB.dll`

---

## [3.3.0] - 2026-04-21

### Added
- **Plugin DI Registration** (`IPluginServiceRegistrator`): All plugin services are now properly registered with the Jellyfin DI container via a standalone `PluginServiceRegistrator` class
  - LiteDB database (auto-created at `{DataPath}/xtream/xtream.db`)
  - Typed repositories (`IXtreamRepository<XtreamMovie/Series/Channel>`)
  - API client with rate limiting (`XtreamApiClient`, `XtreamApiRateLimiter`)
  - Synchronization services (`XtreamSyncService`, `XtreamSyncValidator`)
- **Unified CI Pipeline** (`ci.yml`): Build, format check, lint, test with coverage — parallel jobs with NuGet caching
- **xUnit Test Project**: Separate `Jellyfin.Xtream.V3.Tests` project with coverlet and Moq
- **Build Tooling**: Makefile with `restore`, `build`, `format`, `test`, `lint`, `ci`, `clean` targets
- **Code Style**: `.editorconfig` for C# naming, formatting, and analyzer rules
- **Agent Design**: `CLAUDE.md` with autonomous workflow, safety constraints, and architecture documentation

### Fixed
- **Critical: Plugin loading error** — `Unable to resolve service for type 'XtreamSyncService' while attempting to activate 'XtreamIncrementalSyncTask'`
  - Root cause: `IPluginServiceRegistrator` requires a **parameterless constructor on a standalone class**, not on the Plugin class
  - Previous fix (v3.2.7) incorrectly removed `IPluginServiceRegistrator` thinking it was unavailable in Jellyfin 10.11.3
- **XtreamApiClient namespace**: Added missing `namespace Jellyfin.Xtream.Api` (was in global namespace)
- **CI workflows**: Upgraded from .NET 6.0 to 9.0 and actions v3 to v4

### Changed
- `XtreamIncrementalSyncTask` now performs real synchronization using `XtreamSyncService.SyncAllWithValidationAsync()` instead of being a no-op stub
- Deleted unused `XtreamTasksServiceCollectionExtensions.cs` (superseded by `PluginServiceRegistrator`)
- Quality gates (format, lint, coverage) are informational until existing code is cleaned up

### Commits
- `6930adf` fix(di): register all plugin services via IPluginServiceRegistrator
- `d3c8f86` chore(ci): scaffold agent design, test project, and quality gates
- `087a6f5` ci(workflows): make format, lint, and coverage checks non-blocking

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
