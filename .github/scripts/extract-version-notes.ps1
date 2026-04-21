#!/usr/bin/env pwsh
# Script to extract version notes from CHANGELOG.md

param(
    [string]$Version,
    [string]$ChangelogPath = "./CHANGELOG.md",
    [string]$OutputPath = "$env:GITHUB_OUTPUT"
)

if (-not (Test-Path $ChangelogPath)) {
    Write-Host "Error: CHANGELOG.md not found at $ChangelogPath" -ForegroundColor Red
    exit 1
}

# Read changelog
$changelog = Get-Content $ChangelogPath -Raw -Encoding UTF8

# Extract version section using regex - look for [X.Y.Z] - date pattern
$pattern = "## \[$Version\]\s*-\s*(.+?)(?=##\s*\[|$)"
$match = [regex]::Match($changelog, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

if ($match.Success) {
    $versionContent = $match.Groups[1].Value.Trim()

    # Clean up the content
    $versionContent = $versionContent -replace '^\s+', ''

    # Output to GitHub Actions
    if ($OutputPath -eq "$env:GITHUB_OUTPUT" -and $env:GITHUB_OUTPUT) {
        # Use multiline output syntax for GitHub Actions
        $delimiter = "EOF_$(Get-Random)"
        "release_notes<<$delimiter" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding UTF8 -Append
        $versionContent | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding UTF8 -Append
        $delimiter | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding UTF8 -Append
        Write-Host "Release notes exported to GITHUB_OUTPUT"
    } else {
        Write-Host "Release Notes for v$Version:"
        Write-Host ""
        Write-Host $versionContent
        Write-Host ""
    }

    exit 0
} else {
    Write-Host "Warning: Version [$Version] not found in CHANGELOG.md" -ForegroundColor Yellow
    Write-Host "Falling back to generic release notes" -ForegroundColor Yellow
    exit 1
}
