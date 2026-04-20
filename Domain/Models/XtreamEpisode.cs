namespace Jellyfin.Xtream.Domain.Models;

public sealed record XtreamEpisode : Jellyfin.Xtream.Infrastructure.Persistence.IEntity
{
    public int Id { get; init; }
    public int SeriesId { get; init; }
    public int Season { get; init; }
    public int EpisodeNumber { get; init; }
}
