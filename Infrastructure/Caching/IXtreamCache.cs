namespace Jellyfin.Xtream.Infrastructure.Caching;

public interface IXtreamCache
{
    bool TryGet(int channelId, DateTime from, DateTime to, out IEnumerable<object> programs);
    void Store(int channelId, IEnumerable<object> programs);
    void Store(int channelId, IEnumerable<object> programs, TimeSpan expiration);
    void Clear();
    void Remove(int channelId);
}
