using System.Globalization;
using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.Services.Mapping;
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
        _logger.LogDebug("[Xtream] XtreamLiveTvService constructor called — Name={Name}, HomePageUrl={HomePageUrl}", Name, HomePageUrl);
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

        if (config?.EnableChannelNameCleaning == true)
        {
            name = ChannelNameCleaner.Clean(name);
        }

        if (config?.AppendLanguageToChannelName == true && !string.IsNullOrWhiteSpace(channel.Language))
        {
            name = $"{name} ({channel.Language.ToUpperInvariant()})";
        }

        return name;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ChannelInfo>> GetChannelsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("[Xtream] GetChannelsAsync() called");

        // Diagnostic: list all ILiveTvService instances visible in DI
        try
        {
            var allServices = _serviceProvider.GetServices<ILiveTvService>().ToList();
            _logger.LogDebug(
                "[Xtream] DI contains {Count} ILiveTvService instances: [{Names}]",
                allServices.Count,
                string.Join(", ", allServices.Select(s => $"'{s.Name}' ({s.GetType().FullName})")));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[Xtream] Failed to enumerate ILiveTvService instances from DI");
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
                HasImage = !string.IsNullOrWhiteSpace(c.Icon),
                ChannelType = ChannelType.TV,
                ChannelGroup = c.CategoryName
            };

            // Build tags list (HD badge + language)
            var tags = new List<string>();
            if (System.Text.RegularExpressions.Regex.IsMatch(
                c.Name, @"\b(HD|FHD|4K|UHD)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                tags.Add("HD");
            }

            if (config?.ShowChannelLanguageTags == true && !string.IsNullOrWhiteSpace(c.Language))
            {
                tags.Add($"Lang: {c.Language.ToUpperInvariant()}");
            }

            if (tags.Count > 0)
            {
                channelInfo.Tags = tags.ToArray();
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
    public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(
        string channelId,
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.EnableEPG)
        {
            return Enumerable.Empty<ProgramInfo>();
        }

        if (!int.TryParse(channelId, out var streamId))
        {
            return Enumerable.Empty<ProgramInfo>();
        }

        try
        {
            var apiClient = _serviceProvider.GetRequiredService<XtreamApiClient>();
            var url = XtreamApiEndpoints.FullEpg(config.ServerUrl, config.Username, config.Password, streamId);
            var epgData = await apiClient.GetAsync<XtreamEpgResponse>(url, cancellationToken).ConfigureAwait(false);

            if (epgData?.Listings == null || epgData.Listings.Count == 0)
            {
                return Enumerable.Empty<ProgramInfo>();
            }

            var programs = new List<ProgramInfo>();
            foreach (var listing in epgData.Listings)
            {
                var start = ParseEpgDateTime(listing.Start, listing.StartTimestamp);
                var end = ParseEpgDateTime(listing.End, listing.StopTimestamp);

                if (start == null || end == null) continue;
                if (end.Value < startDateUtc || start.Value > endDateUtc) continue;

                programs.Add(new ProgramInfo
                {
                    Id = $"{channelId}_{listing.Id ?? start.Value.Ticks.ToString()}",
                    ChannelId = channelId,
                    Name = DecodeBase64IfNeeded(listing.Title),
                    Overview = DecodeBase64IfNeeded(listing.Description),
                    StartDate = start.Value,
                    EndDate = end.Value
                });
            }

            return programs;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[Xtream] Failed to fetch EPG for channel {ChannelId}", channelId);
            return Enumerable.Empty<ProgramInfo>();
        }
    }

    private static DateTime? ParseEpgDateTime(string? dateStr, long? timestamp)
    {
        if (timestamp.HasValue && timestamp.Value > 0)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).UtcDateTime;
        }

        if (string.IsNullOrWhiteSpace(dateStr)) return null;

        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
        {
            return dt;
        }

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal, out var dt2))
        {
            return dt2;
        }

        return null;
    }

    private static string DecodeBase64IfNeeded(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(value);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return value;
        }
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
