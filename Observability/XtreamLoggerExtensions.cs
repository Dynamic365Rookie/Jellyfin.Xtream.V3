using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.V3.Observability;

/// <summary>
/// Extension methods for conditional debug logging.
/// </summary>
public static class XtreamLoggerExtensions
{
    /// <summary>
    /// Logs debug message only if EnableDebugLogging is true.
    /// </summary>
    public static void LogDebugIfEnabled<T>(this ILogger<T> logger, string message, params object?[] args)
    {
        if (Plugin.Instance?.Configuration?.EnableDebugLogging == true)
        {
            logger.LogDebug(message, args);
        }
    }
}
