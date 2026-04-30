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
/// Exposes Xtream movies as a browsable channel in Jellyfin.
/// </summary>
public class XtreamMovieChannel : IChannel, IRequiresMediaInfoCallback
{
    private readonly IXtreamRepository<XtreamMovie> _movieRepo;
    private readonly ILogger<XtreamMovieChannel> _logger;

    public XtreamMovieChannel(
        IXtreamRepository<XtreamMovie> movieRepo,
        ILogger<XtreamMovieChannel> logger)
    {
        _movieRepo = movieRepo;
        _logger = logger;
        _logger.LogWarning("[Xtream Movies] XtreamMovieChannel constructed — channel registered");
    }

    public string Name => "Xtream Movies";

    public string Description => "Browse movies from your Xtream provider";

    public string DataVersion => "1";

    public string HomePageUrl => string.Empty;

    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    public InternalChannelFeatures GetChannelFeatures() => new()
    {
        ContentTypes = new List<ChannelMediaContentType> { ChannelMediaContentType.Movie },
        MediaTypes = new List<ChannelMediaType> { ChannelMediaType.Video },
        MaxPageSize = 500,
    };

    public bool IsEnabledFor(string userId)
        => Plugin.Instance?.Configuration?.EnableMovies == true;

    public IEnumerable<ImageType> GetSupportedChannelImages()
        => new[] { ImageType.Thumb };

    public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken ct)
        => Task.FromResult(new DynamicImageResponse { HasImage = false });

    public Task<ChannelItemResult> GetChannelItems(
        InternalChannelItemQuery query, CancellationToken ct)
    {
        var movies = _movieRepo.GetAll().ToList();
        _logger.LogDebug("[Xtream Movies] GetChannelItems folderId={FolderId}, total movies={Count}", query.FolderId, movies.Count);

        if (string.IsNullOrEmpty(query.FolderId))
        {
            // Root: return category folders
            var categories = movies
                .Where(m => !string.IsNullOrEmpty(m.CategoryName))
                .GroupBy(m => m.CategoryName!)
                .OrderBy(g => g.Key)
                .Select(g => new ChannelItemInfo
                {
                    Id = $"cat_{g.First().CategoryId}",
                    Name = g.Key,
                    FolderType = ChannelFolderType.Container,
                    Type = ChannelItemType.Folder,
                })
                .ToList();

            return Task.FromResult(new ChannelItemResult
            {
                Items = categories,
                TotalRecordCount = categories.Count
            });
        }

        // Category folder: return movies
        if (query.FolderId.StartsWith("cat_", StringComparison.Ordinal))
        {
            var catIdStr = query.FolderId.AsSpan(4);
            if (int.TryParse(catIdStr, out var catId))
            {
                var categoryMovies = movies
                    .Where(m => m.CategoryId == catId)
                    .Select(MapMovieToChannelItem)
                    .ToList();

                return Task.FromResult(new ChannelItemResult
                {
                    Items = categoryMovies,
                    TotalRecordCount = categoryMovies.Count
                });
            }
        }

        return Task.FromResult(new ChannelItemResult());
    }

    public Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(
        string id, CancellationToken ct)
    {
        if (int.TryParse(id, out var streamId))
        {
            var source = ChannelMediaHelper.BuildMovieMediaSource(streamId);
            return Task.FromResult<IEnumerable<MediaSourceInfo>>(new[] { source });
        }

        return Task.FromResult<IEnumerable<MediaSourceInfo>>(Array.Empty<MediaSourceInfo>());
    }

    private static ChannelItemInfo MapMovieToChannelItem(XtreamMovie movie)
    {
        var item = new ChannelItemInfo
        {
            Id = movie.StreamId.ToString(),
            Name = movie.Name,
            ImageUrl = movie.Image,
            Overview = movie.Plot,
            Type = ChannelItemType.Media,
            MediaType = ChannelMediaType.Video,
            ContentType = ChannelMediaContentType.Movie,
            ProductionYear = movie.Year,
            CommunityRating = (float?)movie.Rating,
        };

        if (!string.IsNullOrEmpty(movie.Genre))
        {
            item.Genres = movie.Genre.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        var people = new List<MediaBrowser.Controller.Entities.PersonInfo>();

        if (!string.IsNullOrEmpty(movie.Actor))
        {
            people.AddRange(movie.Actor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Take(10)
                .Select(a => new MediaBrowser.Controller.Entities.PersonInfo { Name = a, Type = Jellyfin.Data.Enums.PersonKind.Actor }));
        }

        if (!string.IsNullOrEmpty(movie.Director))
        {
            people.AddRange(movie.Director.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(d => new MediaBrowser.Controller.Entities.PersonInfo { Name = d, Type = Jellyfin.Data.Enums.PersonKind.Director }));
        }

        if (people.Count > 0)
        {
            item.People = people;
        }

        return item;
    }
}
