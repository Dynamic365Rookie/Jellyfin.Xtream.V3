using Jellyfin.Xtream.V3;

namespace Jellyfin.Xtream.Services.LiveTv;

/// <summary>
/// Resolves Xtream API stream URLs for live, movie, and series content.
/// </summary>
public static class StreamUrlResolver
{
    public static string ResolveLive(int streamId) => BuildUrl("live", streamId, ".ts");

    public static string ResolveMovie(int streamId) => BuildUrl("movie", streamId, ".mkv");

    public static string ResolveSeries(int streamId) => BuildUrl("series", streamId, ".mkv");

    private static string BuildUrl(string type, int streamId, string ext)
    {
        var config = Plugin.Instance?.Configuration
            ?? throw new InvalidOperationException("Plugin not initialized");
        var baseUrl = config.ServerUrl.TrimEnd('/');
        return $"{baseUrl}/{type}/{config.Username}/{config.Password}/{streamId}{ext}";
    }
}
