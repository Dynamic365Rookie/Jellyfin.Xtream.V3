#!/usr/bin/env pwsh
# Script to generate repository.json for Jellyfin plugin distribution
# This script creates the manifest file required by Jellyfin to recognize the plugin repository

param(
    [string]$OutputPath = "./repository.json",
    [string]$PluginZipPath = "",
    [string]$Version = "3.0.0",
    [string]$CommitHash = "unknown",
    [string]$ReleaseUrl = ""
)

# Plugin metadata
$PluginGuid = "5d774c35-8567-46d3-a950-9bb8227a0c5d"
$PluginName = "Jellyfin Xtream"
$PluginDescription = "Plugin IPTV Xtream optimisé pour les services Xtream - Support jusqu'à 25,000+ entités"
$PluginOverview = "Plugin IPTV Xtream optimisé pour les services Xtream - Support jusqu'à 25,000+ entités"
$PluginOwner = "Dynamic365Rookie"
$PluginCategory = "Live TV"
$PluginImageUrl = "https://raw.githubusercontent.com/Dynamic365Rookie/Jellyfin.Xtream.V3/main/Assets/icon.png"
$JellyfinTargetAbi = "10.11.0.0"

# Function to calculate MD5 checksum
function Get-FileHash256 {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        Write-Error "File not found: $FilePath"
        return ""
    }

    $hash = (Get-FileHash -Path $FilePath -Algorithm MD5).Hash.ToLower()
    return $hash
}

# Function to get file size
function Get-FileSizeBytes {
    param([string]$FilePath)

    if (Test-Path $FilePath) {
        return (Get-Item $FilePath).Length
    }
    return 0
}

# Main logic
Write-Host "Generating repository.json for Jellyfin plugin..." -ForegroundColor Green

# Get current timestamp
$timestamp = Get-Date -Format "o"

# Calculate checksum if zip file provided
$checksum = ""
$fileSize = 0

if ($PluginZipPath -and (Test-Path $PluginZipPath)) {
    Write-Host "Calculating checksum for: $PluginZipPath" -ForegroundColor Cyan
    $checksum = Get-FileHash256 $PluginZipPath
    $fileSize = Get-FileSizeBytes $PluginZipPath
    Write-Host "Checksum: $checksum" -ForegroundColor Yellow
    Write-Host "File size: $fileSize bytes" -ForegroundColor Yellow
}

# Validate release URL
if (-not $ReleaseUrl) {
    $ReleaseUrl = "https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3/releases/download/v${Version}/Jellyfin.Xtream.V3-v${Version}.zip"
}

# Build the repository structure compatible with Jellyfin
$repository = @(
    @{
        guid = $PluginGuid
        name = $PluginName
        description = $PluginDescription
        overview = $PluginOverview
        owner = $PluginOwner
        category = $PluginCategory
        imageUrl = $PluginImageUrl
        versions = @(
            @{
                version = $Version
                changelog = "Release version $Version - Performance optimized IPTV plugin. Commit: $CommitHash"
                targetAbi = $JellyfinTargetAbi
                sourceUrl = $ReleaseUrl
                checksum = $checksum
                timestamp = $timestamp
            }
        )
    }
)

# Convert to JSON with proper formatting for Jellyfin
# Note: Jellyfin expects specific property names (case-sensitive)
$json = $repository | ConvertTo-Json -Depth 10 -Compress:$false

# Ensure proper encoding
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($OutputPath, $json, $utf8NoBom)

Write-Host ""
Write-Host "? repository.json generated successfully!" -ForegroundColor Green
Write-Host "Location: $OutputPath" -ForegroundColor Yellow
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
