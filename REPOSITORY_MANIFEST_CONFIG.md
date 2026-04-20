# Repository Manifest Configuration

## Overview

This configuration generates and publishes a `repository.json` file to GitHub Pages, allowing Jellyfin to discover and install the Xtream plugin directly from the repository manifest.

## Files

### 1. `.github/scripts/generate-repository.ps1`
PowerShell script that generates the `repository.json` manifest file.

**Features:**
- Calculates MD5 checksum of the plugin ZIP file
- Extracts version and commit information
- Generates proper JSON structure for Jellyfin compatibility
- Handles timestamp and metadata

**Usage:**
```powershell
./.github/scripts/generate-repository.ps1 `
  -OutputPath "./repository.json" `
  -Version "3.0.0" `
  -CommitHash "abc1234" `
  -ReleaseUrl "https://github.com/.../releases/download/v3.0.0/..."
```

### 2. `.github/workflows/publish-repository.yml`
GitHub Actions workflow that automatically generates and publishes the manifest.

**Triggers:**
- Push to main branch
- Creation of version tags (v*)
- Manual workflow dispatch

**Steps:**
1. Checkout code
2. Extract version and commit info
3. Generate repository.json
4. Verify the JSON structure
5. Upload to GitHub Pages artifact
6. Deploy to GitHub Pages

## Configuration

### GitHub Pages Setup

**Required:**
1. Go to Repository Settings ? Pages
2. Select "GitHub Actions" as the source
3. Branch will be automatically configured

### Environment Variables (Workflow)

The workflow automatically sets:
- `GITHUB_TOKEN` - For authentication
- `version` - Extracted from tag or generated
- `commit` - Short commit hash
- `release_url` - Constructed from version

## Repository JSON Structure

```json
[
  {
    "guid": "5d774c35-8567-46d3-a950-9bb8227a0c5d",
    "name": "Jellyfin Xtream",
    "description": "Plugin IPTV Xtream optimisé...",
    "overview": "Plugin IPTV Xtream optimisé...",
    "owner": "Dynamic365Rookie",
    "category": "Live TV",
    "imageUrl": "https://...",
    "versions": [
      {
        "version": "3.0.0",
        "changelog": "Release notes...",
        "targetAbi": "10.11.0.0",
        "sourceUrl": "https://github.com/.../releases/download/...",
        "checksum": "67f18ae14dadbdedae61cc333a3fd318",
        "timestamp": "2025-01-15T10:30:00Z"
      }
    ]
  }
]
```

## Usage in Jellyfin

1. Open Jellyfin Dashboard
2. Go to Plugins ? Repositories
3. Add Repository:
   - URL: `https://dynamic365rookie.github.io/Jellyfin.Xtream.V3/repository.json`
   - Name: "Jellyfin Xtream"
4. Refresh plugin catalog
5. Find and install "Jellyfin Xtream" plugin

## Automated Release Process

When you create a new release:

1. **Create Tag:**
   ```bash
   git tag v3.0.1
   git push origin v3.0.1
   ```

2. **GitHub Actions:**
   - Build workflow creates release ZIP
   - Release workflow creates GitHub Release
   - Publish-repository workflow generates manifest

3. **Result:**
   - ZIP file available at: `https://github.com/.../releases/download/v3.0.1/...`
   - Manifest updated at: `https://dynamic365rookie.github.io/Jellyfin.Xtream.V3/repository.json`

## Fields Explanation

| Field | Description |
|-------|-------------|
| `guid` | Unique identifier for the plugin (must match plugin metadata) |
| `name` | Display name in Jellyfin |
| `description` | Short description |
| `overview` | Longer overview text |
| `owner` | Plugin author/owner |
| `category` | Plugin category (Live TV, Media, etc.) |
| `imageUrl` | Plugin icon URL |
| `version` | Plugin version |
| `targetAbi` | Target Jellyfin ABI version (10.11.0.0, etc.) |
| `sourceUrl` | Direct download URL for plugin ZIP |
| `checksum` | MD5 checksum of ZIP file |
| `timestamp` | ISO 8601 timestamp |

## Troubleshooting

### Repository.json not appearing
1. Check workflow execution: Actions ? Publish Repository Manifest
2. Verify Pages deployment: Settings ? Pages
3. Check branch selection (should be gh-pages)

### Jellyfin can't find plugin
1. Verify repository URL is correct
2. Check plugin GUID matches in code
3. Verify targetAbi matches Jellyfin version
4. Ensure release ZIP is accessible

### Invalid checksum
The checksum is automatically calculated from the published ZIP file during the publish-repository workflow.

## Version Management

### Automatic Versioning (Manual publish)
```bash
git push origin main  # Triggers with version 3.0.0-YYYYMMDD.HHMMSS
```

### Semantic Versioning (Tag-based)
```bash
git tag v3.0.1
git push origin v3.0.1  # Triggers with version 3.0.1
```

## Security

- Workflow has minimal required permissions
- No secrets needed (uses built-in GITHUB_TOKEN)
- Pages are public (plugin distribution)
- ZIP file integrity verified via checksum

## Maintenance

- Manifest is automatically generated on each release
- Previous versions remain accessible in GitHub Releases
- GitHub Pages caches the latest manifest

## Next Steps

1. Enable GitHub Pages in repository settings
2. Commit and push these files
3. Create a release tag: `git tag v3.0.0`
4. Verify manifest at: `https://dynamic365rookie.github.io/Jellyfin.Xtream.V3/repository.json`
5. Test in Jellyfin with the repository URL
