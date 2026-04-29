using Jellyfin.Xtream.V3;

namespace Jellyfin.Xtream.Services.LiveTv;

/// <summary>
/// Resolves Xtream API stream URLs for live, movie, and series content.
/// </summary>
public static class StreamUrlResolver
{
    public static string ResolveLive(int streamId) => BuildUrl(null, streamId);

    public static string ResolveMovie(int streamId) => BuildUrl("movie", streamId);

    public static string ResolveSeries(int streamId) => BuildUrl("series", streamId);

    private static string BuildUrl(string? type, int streamId)
    {
        var config = Plugin.Instance?.Configuration
            ?? throw new InvalidOperationException("Plugin not initialized");
        var baseUrl = config.ServerUrl.TrimEnd('/');

        // Live streams use the base format without type prefix (per Xtream API spec)
        // Series/Movies include the type prefix
        var path = type == null
            ? $"{baseUrl}/{config.Username}/{config.Password}/{streamId}"
            : $"{baseUrl}/{type}/{config.Username}/{config.Password}/{streamId}";

        return path;
    }
}
