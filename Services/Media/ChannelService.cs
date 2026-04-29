using Jellyfin.Xtream.Services.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.MediaInfo;

namespace Jellyfin.Xtream.Services.Media;

/// <summary>
/// Shared utilities for building Jellyfin media sources from Xtream stream URLs.
/// </summary>
public static class ChannelMediaHelper
{
    public static MediaSourceInfo BuildMovieMediaSource(int streamId)
    {
        var url = StreamUrlResolver.ResolveMovie(streamId);
        return BuildVodMediaSource(streamId.ToString(), url);
    }

    public static MediaSourceInfo BuildSeriesMediaSource(int streamId, string? container = null)
    {
        var url = StreamUrlResolver.ResolveSeries(streamId);
        return BuildVodMediaSource(streamId.ToString(), url, container);
    }

    private static MediaSourceInfo BuildVodMediaSource(string id, string url, string? container = null)
    {
        return new MediaSourceInfo
        {
            Id = id,
            Path = url,
            Protocol = MediaProtocol.Http,
            IsRemote = true,
            SupportsDirectPlay = true,
            SupportsDirectStream = true,
            SupportsTranscoding = true,
            IsInfiniteStream = false,
            Container = container,
        };
    }
}
