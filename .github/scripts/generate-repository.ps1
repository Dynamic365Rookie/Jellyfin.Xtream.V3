#!/usr/bin/env pwsh
# Script to generate repository.json for Jellyfin plugin distribution
# This script fetches ALL releases from GitHub and creates a complete manifest

param(
    [string]$OutputPath = "./repository.json",
    [string]$RepoOwner = "Dynamic365Rookie",
    [string]$RepoName = "Jellyfin.Xtream.V3"
)

# Plugin metadata (ASCII-safe to avoid encoding issues in clients)
$PluginGuid = "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
$PluginName = "Jellyfin Xtream"
$PluginDescription = "Performance-optimized IPTV plugin for Xtream Codes API. Support for large catalogs with advanced caching and memory management."
$PluginOverview = "Stream Live TV, Movies, and Series from an Xtream-compatible server using this optimized plugin."
$PluginOwner = "Dynamic365Rookie"
$PluginCategory = "LiveTV"
$JellyfinTargetAbi = "10.11.0.0"

# Function to calculate MD5 checksum from URL
function Get-RemoteFileMD5 {
    param([string]$Url)

    try {
        $tempFile = [System.IO.Path]::GetTempFileName()
        Write-Host "Downloading for checksum: $Url" -ForegroundColor Cyan
        Invoke-WebRequest -Uri $Url -OutFile $tempFile -ErrorAction Stop
        $hash = (Get-FileHash -Path $tempFile -Algorithm MD5).Hash.ToLower()
        Remove-Item $tempFile -Force
        Write-Host "  Checksum: $hash" -ForegroundColor Yellow
        return $hash
    }
    catch {
        Write-Host "  Warning: Could not calculate checksum for $Url" -ForegroundColor Yellow
        return ""
    }
}

# Function to sanitize text for safe JSON/Jellyfin consumption
function Convert-ToSafeText {
    param([string]$Text)

    if ([string]::IsNullOrEmpty($Text)) {
        return ""
    }

    # Normalize line endings first
    $sanitized = $Text -replace "`r`n", "`n"

    # Remove invalid replacement character often produced by bad encodings
    $sanitized = $sanitized -replace [char]0xFFFD, ""

    # Remove control characters except tab/newline/carriage return
    $sanitized = [regex]::Replace($sanitized, "[\x00-\x08\x0B\x0C\x0E-\x1F]", "")

    # Remove surrogate pairs (emojis / unsupported symbols in some clients)
    $chars = $sanitized.ToCharArray() | Where-Object { -not [char]::IsSurrogate($_) }
    $sanitized = -join $chars

    # Keep output ASCII-safe for maximum compatibility in Jellyfin clients
    $sanitized = [regex]::Replace($sanitized, "[^\u0009\u000A\u000D\u0020-\u007E]", "")

    # Unicode normalization for stable output
    $sanitized = $sanitized.Normalize([Text.NormalizationForm]::FormC)

    return $sanitized
}

Write-Host ""
Write-Host "Generating repository.json for Jellyfin plugin..." -ForegroundColor Green
Write-Host ""

# Fetch all releases from GitHub API
Write-Host "Fetching releases from GitHub API..." -ForegroundColor Cyan
$apiUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases"

try {
    $releases = Invoke-RestMethod -Uri $apiUrl -Method Get
    Write-Host "Found $($releases.Count) release(s)" -ForegroundColor Green
}
catch {
    Write-Host "Error fetching releases from GitHub API" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Build versions array from all releases
$versions = @()

foreach ($release in $releases) {
    $tagName = $release.tag_name
    $version = $tagName -replace '^v', ''

    # Find the plugin ZIP asset
    $zipAsset = $release.assets | Where-Object { $_.name -like "*.zip" } | Select-Object -First 1

    if ($null -eq $zipAsset) {
        Write-Host "  Skipping release $tagName (no ZIP asset found)" -ForegroundColor Yellow
        continue
    }

    Write-Host "Processing release: $tagName (version: $version)" -ForegroundColor Cyan

    $sourceUrl = $zipAsset.browser_download_url
    $releaseTitle = if ($release.name) { $release.name } else { "Release $tagName" }

    # Use release body (description) if available, otherwise use title
    $changelogRaw = if ($release.body -and $release.body.Trim().Length -gt 0) {
        $release.body
    } else {
        "$releaseTitle - $tagName"
    }

    $changelog = Convert-ToSafeText -Text $changelogRaw
    $timestamp = if ($release.published_at) {
        [DateTime]::Parse($release.published_at).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    else {
        [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
    }

    # Calculate checksum
    $checksum = Get-RemoteFileMD5 -Url $sourceUrl

    $versionEntry = @{
        changelog = $changelog
        checksum = $checksum
        sourceUrl = $sourceUrl
        targetAbi = $JellyfinTargetAbi
        timestamp = $timestamp
        version = $version
    }

    $versions += $versionEntry
}

if ($versions.Count -eq 0) {
    Write-Host "Error: No valid releases found with ZIP assets" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Included $($versions.Count) version(s) in manifest" -ForegroundColor Green

# Build the repository structure
$repository = @(
    @{
        category = (Convert-ToSafeText -Text $PluginCategory)
        description = (Convert-ToSafeText -Text $PluginDescription)
        guid = $PluginGuid
        name = (Convert-ToSafeText -Text $PluginName)
        overview = (Convert-ToSafeText -Text $PluginOverview)
        owner = (Convert-ToSafeText -Text $PluginOwner)
        versions = $versions
    }
)

# Convert to JSON - force array format
if ($PSVersionTable.PSVersion.Major -ge 7 -and $PSVersionTable.PSVersion.Minor -ge 3) {
    $json = $repository | ConvertTo-Json -Depth 10 -Compress:$false -AsArray
} else {
    $jsonObject = $repository[0] | ConvertTo-Json -Depth 10 -Compress:$false
    $json = "[$jsonObject]"
}

# Write with UTF-8 encoding without BOM
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($OutputPath, $json, $utf8NoBom)

Write-Host ""
Write-Host "Repository JSON generated successfully!" -ForegroundColor Green
Write-Host "Location: $OutputPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Plugin: $PluginName" -ForegroundColor White
Write-Host "  GUID: $PluginGuid" -ForegroundColor White
Write-Host "  Versions included: $($versions.Count)" -ForegroundColor White
Write-Host "  Target ABI: $JellyfinTargetAbi" -ForegroundColor White
Write-Host ""

Write-Host "Content:" -ForegroundColor Cyan
Write-Host $json
Write-Host ""
Write-Host "Repository JSON Details:" -ForegroundColor Green
Write-Host "  Plugin: $PluginName" -ForegroundColor White
Write-Host "  GUID: $PluginGuid" -ForegroundColor White
Write-Host "  Version: $Version" -ForegroundColor White
Write-Host "  Target ABI: $JellyfinTargetAbi" -ForegroundColor White
Write-Host "  Source URL: $ReleaseUrl" -ForegroundColor White
Write-Host "  Checksum: $checksum" -ForegroundColor White
Write-Host "  Timestamp: $timestamp" -ForegroundColor White
