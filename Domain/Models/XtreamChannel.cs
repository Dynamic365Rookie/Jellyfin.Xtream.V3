namespace Jellyfin.Xtream.Domain.Models;

public sealed record XtreamChannel : Jellyfin.Xtream.Infrastructure.Persistence.IEntity
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int StreamId { get; init; }
}
