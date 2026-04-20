using MediaBrowser.Model.Plugins;

namespace Jellyfin.Xtream.V3.Configuration;

/// <summary>
/// Plugin configuration for Jellyfin Xtream.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the Xtream server URL.
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Xtream username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Xtream password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration in hours.
    /// </summary>
    public int CacheDurationHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the maximum number of concurrent requests.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 5;

    /// <summary>
    /// Gets or sets the batch size for processing.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether Live TV is enabled.
    /// </summary>
    public bool EnableLiveTV { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Movies are enabled.
    /// </summary>
    public bool EnableMovies { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Series are enabled.
    /// </summary>
    public bool EnableSeries { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether EPG is enabled.
    /// </summary>
    public bool EnableEPG { get; set; } = true;

    /// <summary>
    /// Gets or sets the sync interval in hours.
    /// </summary>
    public int SyncIntervalHours { get; set; } = 6;

    /// <summary>
    /// Gets or sets a value indicating whether auto-sync is enabled.
    /// </summary>
    public bool AutoSync { get; set; } = true;
}
