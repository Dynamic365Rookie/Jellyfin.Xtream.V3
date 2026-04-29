namespace Jellyfin.Xtream.Api;

public static class XtreamApiEndpoints
{
    public static string Movies(string baseUrl, string user, string pass)
        => $"{baseUrl.TrimEnd('/')}/player_api.php?username={user}&password={pass}&action=get_vod_streams";

    public static string Series(string baseUrl, string user, string pass)
        => $"{baseUrl.TrimEnd('/')}/player_api.php?username={user}&password={pass}&action=get_series";

    public static string LiveStreams(string baseUrl, string user, string pass)
        => $"{baseUrl.TrimEnd('/')}/player_api.php?username={user}&password={pass}&action=get_live_streams";

    public static string Epg(string baseUrl, string user, string pass, int channelId)
        => $"{baseUrl.TrimEnd('/')}/player_api.php?username={user}&password={pass}&action=get_short_epg&stream_id={channelId}";

    public static string SeriesInfo(string baseUrl, string user, string pass, int seriesId)
        => $"{baseUrl.TrimEnd('/')}/player_api.php?username={user}&password={pass}&action=get_series_info&series_id={seriesId}";
}
