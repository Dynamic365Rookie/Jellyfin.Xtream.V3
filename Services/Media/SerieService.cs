using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using Jellyfin.Xtream.V3;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Services.Media;

/// <summary>
/// Exposes Xtream series as a browsable channel in Jellyfin.
/// Hierarchy: Categories > Series > Seasons > Episodes.
/// </summary>
public class XtreamSeriesChannel : IChannel, IRequiresMediaInfoCallback
{
    private readonly IXtreamRepository<XtreamSeries> _seriesRepo;
    private readonly XtreamApiClient _apiClient;
    private readonly ILogger<XtreamSeriesChannel> _logger;

    public XtreamSeriesChannel(
        IXtreamRepository<XtreamSeries> seriesRepo,
        XtreamApiClient apiClient,
        ILogger<XtreamSeriesChannel> logger)
    {
        _seriesRepo = seriesRepo;
        _apiClient = apiClient;
        _logger = logger;
        _logger.LogWarning("[Xtream Series] XtreamSeriesChannel constructed — channel registered");
    }

    public string Name => "Xtream Series";

    public string Description => "Browse TV series from your Xtream provider";

    public string DataVersion => "1";

    public string HomePageUrl => string.Empty;

    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    public InternalChannelFeatures GetChannelFeatures() => new()
    {
        ContentTypes = new List<ChannelMediaContentType> { ChannelMediaContentType.Episode },
        MediaTypes = new List<ChannelMediaType> { ChannelMediaType.Video },
        MaxPageSize = 500,
        AutoRefreshLevels = 3,
    };

    public bool IsEnabledFor(string userId)
        => Plugin.Instance?.Configuration?.EnableSeries == true;

    public IEnumerable<ImageType> GetSupportedChannelImages()
        => new[] { ImageType.Thumb };

    public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken ct)
        => Task.FromResult(new DynamicImageResponse { HasImage = false });

    public async Task<ChannelItemResult> GetChannelItems(
        InternalChannelItemQuery query, CancellationToken ct)
    {
        _logger.LogDebug("[Xtream Series] GetChannelItems folderId={FolderId}", query.FolderId);

        if (string.IsNullOrEmpty(query.FolderId))
        {
            return GetCategoryFolders();
        }

        if (query.FolderId.StartsWith("cat_", StringComparison.Ordinal))
        {
            return GetSeriesInCategory(query.FolderId);
        }

        if (query.FolderId.StartsWith("series_", StringComparison.Ordinal)
            && !query.FolderId.Contains("_season_", StringComparison.Ordinal))
        {
            return await GetSeasonsForSeries(query.FolderId, ct).ConfigureAwait(false);
        }

        if (query.FolderId.Contains("_season_", StringComparison.Ordinal))
        {
            return await GetEpisodesForSeason(query.FolderId, ct).ConfigureAwait(false);
        }

        return new ChannelItemResult();
    }

    public Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(
        string id, CancellationToken ct)
    {
        // Episode ID format: "ep_{streamId}" or just the stream ID
        var streamIdStr = id.StartsWith("ep_", StringComparison.Ordinal)
            ? id.AsSpan(3)
            : id.AsSpan();

        if (int.TryParse(streamIdStr, out var streamId))
        {
            var source = ChannelMediaHelper.BuildSeriesMediaSource(streamId);
            return Task.FromResult<IEnumerable<MediaSourceInfo>>(new[] { source });
        }

        return Task.FromResult<IEnumerable<MediaSourceInfo>>(Array.Empty<MediaSourceInfo>());
    }

    private ChannelItemResult GetCategoryFolders()
    {
        var series = _seriesRepo.GetAll().ToList();

        var categories = series
            .Where(s => !string.IsNullOrEmpty(s.CategoryName))
            .GroupBy(s => s.CategoryName!)
            .OrderBy(g => g.Key)
            .Select(g => new ChannelItemInfo
            {
                Id = $"cat_{g.First().CategoryId}",
                Name = g.Key,
                FolderType = ChannelFolderType.Container,
                Type = ChannelItemType.Folder,
            })
            .ToList();

        return new ChannelItemResult
        {
            Items = categories,
            TotalRecordCount = categories.Count
        };
    }

    private ChannelItemResult GetSeriesInCategory(string folderId)
    {
        var catIdStr = folderId.AsSpan(4);
        if (!int.TryParse(catIdStr, out var catId))
        {
            return new ChannelItemResult();
        }

        var allSeries = _seriesRepo.GetAll().ToList();
        var categorySeries = allSeries
            .Where(s => s.CategoryId == catId)
            .Select(s => new ChannelItemInfo
            {
                Id = $"series_{s.SeriesId}",
                Name = s.Name,
                ImageUrl = s.Image,
                Overview = s.Plot,
                Type = ChannelItemType.Folder,
                FolderType = ChannelFolderType.Container,
                ProductionYear = s.Year,
                CommunityRating = (float?)s.Rating,
            })
            .ToList();

        return new ChannelItemResult
        {
            Items = categorySeries,
            TotalRecordCount = categorySeries.Count
        };
    }

    private async Task<ChannelItemResult> GetSeasonsForSeries(string folderId, CancellationToken ct)
    {
        var seriesIdStr = folderId.AsSpan(7); // "series_"
        if (!int.TryParse(seriesIdStr, out var seriesId))
        {
            return new ChannelItemResult();
        }

        var seriesInfo = await FetchSeriesInfo(seriesId, ct).ConfigureAwait(false);
        if (seriesInfo?.Episodes == null || seriesInfo.Episodes.Count == 0)
        {
            return new ChannelItemResult();
        }

        var seasons = seriesInfo.Episodes.Keys
            .OrderBy(k => int.TryParse(k, out var n) ? n : 0)
            .Select(seasonNum => new ChannelItemInfo
            {
                Id = $"series_{seriesId}_season_{seasonNum}",
                Name = $"Season {seasonNum}",
                Type = ChannelItemType.Folder,
                FolderType = ChannelFolderType.Container,
            })
            .ToList();

        return new ChannelItemResult
        {
            Items = seasons,
            TotalRecordCount = seasons.Count
        };
    }

    private async Task<ChannelItemResult> GetEpisodesForSeason(string folderId, CancellationToken ct)
    {
        // Parse "series_{seriesId}_season_{seasonNum}"
        var parts = folderId.Split("_season_");
        if (parts.Length != 2)
        {
            return new ChannelItemResult();
        }

        var seriesIdStr = parts[0].AsSpan(7); // "series_"
        if (!int.TryParse(seriesIdStr, out var seriesId) || !int.TryParse(parts[1], out _))
        {
            return new ChannelItemResult();
        }

        var seasonNum = parts[1];
        var seriesInfo = await FetchSeriesInfo(seriesId, ct).ConfigureAwait(false);

        if (seriesInfo?.Episodes == null || !seriesInfo.Episodes.TryGetValue(seasonNum, out var episodes))
        {
            return new ChannelItemResult();
        }

        var items = episodes
            .OrderBy(e => e.EpisodeNumber)
            .Select(ep => new ChannelItemInfo
            {
                Id = $"ep_{ep.StreamId ?? ep.EpisodeNumber.ToString()}",
                Name = string.IsNullOrWhiteSpace(ep.Name)
                    ? $"Episode {ep.EpisodeNumber}"
                    : ep.Name,
                ImageUrl = ep.Info?.Image,
                Overview = ep.Info?.Plot,
                Type = ChannelItemType.Media,
                MediaType = ChannelMediaType.Video,
                ContentType = ChannelMediaContentType.Episode,
            })
            .ToList();

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count
        };
    }

    private async Task<XtreamSeriesInfo?> FetchSeriesInfo(int seriesId, CancellationToken ct)
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return null;
            }

            var url = XtreamApiEndpoints.SeriesInfo(
                config.ServerUrl, config.Username, config.Password, seriesId);

            return await _apiClient.GetAsync<XtreamSeriesInfo>(url, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Xtream Series] Failed to fetch series info for {SeriesId}", seriesId);
            return null;
        }
    }
}
