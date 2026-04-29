using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.V3;
using Jellyfin.Xtream.V3.Configuration;
using Jellyfin.Xtream.V3.Observability;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Services.LiveTv;

/// <summary>
/// Exposes Xtream IPTV live channels to Jellyfin's Live TV system.
/// </summary>
public sealed class XtreamLiveTvService : ILiveTvService
{
    private readonly IXtreamRepository<XtreamChannel> _channelRepo;
    private readonly ILogger<XtreamLiveTvService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public XtreamLiveTvService(
        IXtreamRepository<XtreamChannel> channelRepo,
        ILogger<XtreamLiveTvService> logger,
        IServiceProvider serviceProvider)
    {
        _channelRepo = channelRepo;
        _logger = logger;
        _serviceProvider = serviceProvider;

        // Simple, non-blocking log - do NOT enumerate services here (causes deadlock during DI construction)
        _logger.LogWarning("[Xtream] XtreamLiveTvService constructor called — Name={Name}, HomePageUrl={HomePageUrl}", Name, HomePageUrl);
    }

    /// <inheritdoc />
    public string Name => "Xtream Codes";

    /// <inheritdoc />
    public string HomePageUrl => string.Empty;

    /// <summary>
    /// Builds the channel name with optional language code suffix.
    /// </summary>
    private static string BuildChannelName(XtreamChannel channel, PluginConfiguration? config)
    {
        var name = channel.Name;

        if (config?.AppendLanguageToChannelName == true && !string.IsNullOrWhiteSpace(channel.Language))
        {
            name = $"{name} ({channel.Language.ToUpperInvariant()})";
        }

        return name;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ChannelInfo>> GetChannelsAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("[Xtream] ============================================");
        _logger.LogWarning("[Xtream] GetChannelsAsync() CALLED");
        _logger.LogWarning("[Xtream] ============================================");

        // Diagnostic: list all ILiveTvService instances visible in DI
        try
        {
            var allServices = _serviceProvider.GetServices<ILiveTvService>().ToList();
            _logger.LogWarning(
                "[Xtream] DI contains {Count} ILiveTvService instances: [{Names}]",
                allServices.Count,
                string.Join(", ", allServices.Select(s => $"'{s.Name}' ({s.GetType().FullName})")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Xtream] Failed to enumerate ILiveTvService instances from DI");
        }

        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.EnableLiveTV)
        {
            _logger.LogDebug("Live TV disabled or plugin not initialized");
            return Task.FromResult(Enumerable.Empty<ChannelInfo>());
        }

        var channels = _channelRepo.GetAll().ToList();

        if (channels.Count == 0)
        {
            _logger.LogWarning("[Xtream] No channels in database — run synchronization first");
            return Task.FromResult(Enumerable.Empty<ChannelInfo>());
        }

        _logger.LogInformation("[Xtream] Returning {Count} live channels to Jellyfin", channels.Count);

        var result = channels.Select(c =>
        {
            var channelInfo = new ChannelInfo
            {
                Id = c.StreamId.ToString(),
                Name = BuildChannelName(c, config),
                Number = c.Number?.ToString(),
                ImageUrl = c.Icon,
                ChannelType = ChannelType.TV,
                ChannelGroup = c.CategoryName
            };

            // Add language as tag if enabled and available
            if (config?.ShowChannelLanguageTags == true && !string.IsNullOrWhiteSpace(c.Language))
            {
                channelInfo.Tags = new[] { $"Lang: {c.Language.ToUpperInvariant()}" };
            }

            return channelInfo;
        });

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<MediaSourceInfo> GetChannelStream(string channelId, string streamId, CancellationToken cancellationToken)
    {
        var url = StreamUrlResolver.ResolveLive(int.Parse(channelId));
        _logger.LogDebugIfEnabled("[Xtream] Resolving stream for channel {ChannelId}: {Url}", channelId, url);

        return Task.FromResult(BuildMediaSource(channelId, url));
    }

    /// <inheritdoc />
    public Task<List<MediaSourceInfo>> GetChannelStreamMediaSources(string channelId, CancellationToken cancellationToken)
    {
        var url = StreamUrlResolver.ResolveLive(int.Parse(channelId));
        return Task.FromResult(new List<MediaSourceInfo> { BuildMediaSource(channelId, url) });
    }

    /// <inheritdoc />
    public Task CloseLiveStream(string id, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task ResetTuner(string id, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<IEnumerable<ProgramInfo>> GetProgramsAsync(
        string channelId,
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken)
    {
        // EPG not implemented yet — Jellyfin will show channels without program data
        return Task.FromResult(Enumerable.Empty<ProgramInfo>());
    }

    #region Timer stubs (Xtream does not support recording)

    /// <inheritdoc />
    public Task<IEnumerable<TimerInfo>> GetTimersAsync(CancellationToken cancellationToken)
        => Task.FromResult(Enumerable.Empty<TimerInfo>());

    /// <inheritdoc />
    public Task CreateTimerAsync(TimerInfo info, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task UpdateTimerAsync(TimerInfo updatedTimer, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task CancelTimerAsync(string timerId, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<SeriesTimerInfo> GetNewTimerDefaultsAsync(CancellationToken cancellationToken, ProgramInfo? program = null)
        => Task.FromResult(new SeriesTimerInfo());

    /// <inheritdoc />
    public Task<IEnumerable<SeriesTimerInfo>> GetSeriesTimersAsync(CancellationToken cancellationToken)
        => Task.FromResult(Enumerable.Empty<SeriesTimerInfo>());

    /// <inheritdoc />
    public Task CreateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task UpdateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task CancelSeriesTimerAsync(string timerId, CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion

    private static MediaSourceInfo BuildMediaSource(string channelId, string url)
    {
        var config = Plugin.Instance?.Configuration;
        var streamOptions = config?.StreamOptions;

        // Always set a reasonable analyze duration to prevent Jellyfin's
        // default of 200M (200s) which causes infinite buffering on live streams.
        var analyzeDurationMs = streamOptions?.EnableStreamOptions == true && streamOptions.AnalyzeDurationMs.HasValue
            ? streamOptions.AnalyzeDurationMs.Value
            : 5000; // 5 seconds — fast enough for live TV startup

        var mediaSource = new MediaSourceInfo
        {
            Id = channelId,
            Path = url,
            Protocol = MediaProtocol.Http,
            IsRemote = true,

            // Override Jellyfin's excessive defaults for live streams
            AnalyzeDurationMs = analyzeDurationMs,

            // Stream capabilities
            SupportsDirectPlay = true,
            SupportsDirectStream = true,
            SupportsTranscoding = true,

            // Live TV flags
            IsInfiniteStream = true,
            ReadAtNativeFramerate = false,

            // Specify container format to help Jellyfin decision
            Container = "mpegts",

            // Add media streams info to avoid probing (which can fail with live streams)
            MediaStreams = new List<MediaBrowser.Model.Entities.MediaStream>
            {
                new MediaBrowser.Model.Entities.MediaStream
                {
                    Type = MediaBrowser.Model.Entities.MediaStreamType.Video,
                    Codec = "h264",
                    IsInterlaced = true
                },
                new MediaBrowser.Model.Entities.MediaStream
                {
                    Type = MediaBrowser.Model.Entities.MediaStreamType.Audio,
                    Codec = "aac"
                }
            }
        };

        // Apply additional FFmpeg stream options if enabled
        if (streamOptions?.EnableStreamOptions == true)
        {
            if (streamOptions.GenPtsInput.HasValue)
                mediaSource.GenPtsInput = streamOptions.GenPtsInput.Value;

            if (streamOptions.IgnoreDts.HasValue)
                mediaSource.IgnoreDts = streamOptions.IgnoreDts.Value;

            if (streamOptions.CustomHttpHeaders?.Any() == true)
                mediaSource.RequiredHttpHeaders = streamOptions.CustomHttpHeaders;
        }

        return mediaSource;
    }
}
