namespace Jellyfin.Xtream.Domain.Models;

public sealed record XtreamSeries : Jellyfin.Xtream.Infrastructure.Persistence.IEntity
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime LastModified { get; init; }
}
