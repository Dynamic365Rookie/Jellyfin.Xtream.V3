using System.Xml.Serialization;

namespace Jellyfin.Xtream.V3.Configuration;

/// <summary>
/// FFmpeg stream tuning options for IPTV playback.
/// </summary>
public class StreamOptions
{
    /// <summary>
    /// Enable/disable stream options globally.
    /// </summary>
    public bool EnableStreamOptions { get; set; } = true;

    /// <summary>
    /// FFmpeg analyze duration in milliseconds (default: 15000 = 15 seconds).
    /// Increased to allow FFmpeg time to find valid H.264 parameters in streams with PPS errors.
    /// </summary>
    public int? AnalyzeDurationMs { get; set; } = 15000;

    /// <summary>
    /// Generate PTS from DTS if missing (FFmpeg: -fflags +genpts).
    /// Critical for streams with H.264 PPS errors.
    /// </summary>
    public bool? GenPtsInput { get; set; } = true;

    /// <summary>
    /// Ignore Decode Time Stamps (FFmpeg: -fflags +igndts).
    /// Use with caution - may cause sync issues.
    /// </summary>
    public bool? IgnoreDts { get; set; } = false;

    /// <summary>
    /// FFmpeg probe size in bytes (default: 5MB).
    /// Larger values = more accurate detection but slower startup.
    /// </summary>
    public int? ProbeSizeBytes { get; set; } = 5000000;

    /// <summary>
    /// Custom HTTP headers for stream requests.
    /// Example: { "User-Agent", "Jellyfin-Xtream/1.0" }
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, string> CustomHttpHeaders { get; set; } = new();
}
