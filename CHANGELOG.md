# Jellyfin Xtream V3 - Changelog

Tous les changements notables du projet sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
et ce projet adhère à [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---
## [3.9.8.1] - 2026-05-11

### Added
- Unit tests for stream_icon field mapping from Xtream API
  - Validates `stream_icon` field parsing
  - Tests fallback logic (`stream_icon` → `icon`)

### Fixed
- Skipped LogoGenerator test - utility not used in production

## [3.9.8] - 2026-05-11

### Fixed
- **Channel logos now display correctly** — Fixed stream_icon field mapping
  - Xtream API returns logos in `stream_icon` field, now properly mapped to channel icons
  - Channel logos now display in Jellyfin Live TV guide
  - Improved: XtreamChannelJsonConverter with fallback logic (`stream_icon` → `icon`)

### Changed
- Relaxed namespace validation rule (IDE0130) in .editorconfig
  - Pre-existing namespace/folder structure misalignment documented
  - Can be addressed in future dedicated refactoring

## [3.9.8] - 2026-05-11

### Fixed
- **Channel logos now display correctly** — Fixed stream_icon field mapping
  - Xtream API returns logos in `stream_icon` field, now properly mapped to channel icons
  - Channel logos now display in Jellyfin Live TV guide
  - Improved: XtreamChannelJsonConverter with fallback logic (`stream_icon` → `icon`)

### Changed
- Relaxed namespace validation rule (IDE0130) in .editorconfig
  - Pre-existing namespace/folder structure misalignment documented
  - Can be addressed in future dedicated refactoring

## [3.9.7] - 2026-05-11

### Security
- **Debug endpoints now require Jellyfin admin authentication**
  - `/Xtream/Debug/*` endpoints now protected with `[Authorize(Policy = "RequiresElevation")]`
  - Prevents unauthorized access to plugin configuration and EPG data
  - Fixes: Unauthenticated access to diagnostic information

## [3.9.6] - 2026-05-11

### Added
- **EPG test endpoint** — `GET /Xtream/Debug/epg/test/{streamId}` for troubleshooting specific channels
  - Tests EPG API connectivity and data retrieval per channel
  - Useful for identifying which channels have EPG data available

### Note
- **Channel logos**: Xtream API returns `null` for icon URLs — this is expected behavior and cannot be fixed at plugin level
  - Channels display without logos in Jellyfin
  - Consider using external metadata sources (TMDb) for artwork
- **EPG programs**: Verified working with proper base64 title decoding
  - Ensure Jellyfin's Live TV guide is refreshed (may require manual trigger or service restart)

## [3.9.5] - 2026-05-11

### Fixed
- **Debug endpoints** — Fixed PluginConfiguration dependency injection
  - Use Plugin.Instance pattern for configuration access
  - Fixes: "Unable to resolve service" error on /Xtream/Debug/* endpoints

## [3.9.4] - 2026-05-10

### Added
- **Debug diagnostics endpoints** for troubleshooting channel icons and EPG issues
  - GET /Xtream/Debug/channels/icons — diagnose icon URL accessibility
  - GET /Xtream/Debug/channels/epg — diagnose EPG data retrieval
  - GET /Xtream/Debug/config — check plugin configuration
  - Detailed troubleshooting suggestions included in responses


## [3.9.3] - 2026-05-10

### Fixed
- **EPG JSON deserialization errors** — Fixed handling of Xtream API inconsistent timestamp formats
  - Created `FlexibleInt64JsonConverter` to handle both numeric and string timestamp values
  - EPG listings now parse successfully regardless of API response format
  - Added graceful fallback for null, empty, and invalid timestamp strings
  - Fixes: "Cannot get the value of a token type 'String' as a number" error
  - Comprehensive unit tests added for timestamp conversion edge cases

---
## [3.9.8] - 2026-05-11

### Fixed
- **Channel logos now display correctly** — Fixed stream_icon field mapping
  - Xtream API returns logos in `stream_icon` field, now properly mapped to channel icons
  - Channel logos now display in Jellyfin Live TV guide
  - Improved: XtreamChannelJsonConverter with fallback logic (`stream_icon` → `icon`)

### Changed
- Relaxed namespace validation rule (IDE0130) in .editorconfig
  - Pre-existing namespace/folder structure misalignment documented
  - Can be addressed in future dedicated refactoring

