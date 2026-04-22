# Script to clear Jellyfin's Live TV data and force rediscovery
# Run this if Live TV services are not being discovered properly

param(
    [Parameter(Mandatory=$false)]
    [string]$JellyfinDataPath = "C:\ProgramData\Jellyfin\Server\data"
)

Write-Host "Jellyfin Live TV Data Cleanup Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if Jellyfin is running
$jellyfinProcess = Get-Process -Name "jellyfin" -ErrorAction SilentlyContinue
if ($jellyfinProcess) {
    Write-Host "ERROR: Jellyfin is currently running!" -ForegroundColor Red
    Write-Host "Please stop Jellyfin before running this script." -ForegroundColor Red
    exit 1
}

# Live TV database path
$liveTvDb = Join-Path $JellyfinDataPath "library.db"

if (-not (Test-Path $liveTvDb)) {
    Write-Host "ERROR: Could not find Jellyfin database at: $liveTvDb" -ForegroundColor Red
    Write-Host "Please specify the correct path with -JellyfinDataPath parameter" -ForegroundColor Red
    exit 1
}

Write-Host "Found Jellyfin database: $liveTvDb" -ForegroundColor Green
Write-Host ""

# Backup the database
$backupPath = "$liveTvDb.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Creating backup: $backupPath" -ForegroundColor Yellow
Copy-Item $liveTvDb $backupPath -Force

# Clear Live TV tables (this forces Jellyfin to rediscover services)
Write-Host "Clearing Live TV data..." -ForegroundColor Yellow

try {
    # Load SQLite assembly (assumes it's available)
    Add-Type -Path "System.Data.SQLite.dll" -ErrorAction Stop

    $connectionString = "Data Source=$liveTvDb;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()

    # Delete Live TV channel data
    $commands = @(
        "DELETE FROM TypedBaseItems WHERE type IN ('TvChannel', 'LiveTvChannel');",
        "DELETE FROM TypedBaseItems WHERE Path LIKE '%/LiveTV/%';",
        "VACUUM;"
    )

    foreach ($sql in $commands) {
        Write-Host "  Executing: $sql" -ForegroundColor Gray
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $result = $command.ExecuteNonQuery()
        Write-Host "    Affected rows: $result" -ForegroundColor Gray
    }

    $connection.Close()

    Write-Host ""
    Write-Host "Live TV data cleared successfully!" -ForegroundColor Green
    Write-Host "Backup saved to: $backupPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Start Jellyfin" -ForegroundColor White
    Write-Host "2. Navigate to Live TV in the UI" -ForegroundColor White
    Write-Host "3. Check logs for '[Xtream] Returning X live channels to Jellyfin'" -ForegroundColor White

} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to clear Live TV data" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Manually delete the channels via Jellyfin UI or delete the database and let Jellyfin recreate it." -ForegroundColor Yellow
}
