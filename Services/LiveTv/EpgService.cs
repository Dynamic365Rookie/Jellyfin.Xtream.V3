using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Infrastructure.Caching;

namespace Jellyfin.Xtream.Services.LiveTv;

public sealed class EpgService
{
    private readonly XtreamApiClient _api;
    private readonly IXtreamCache _cache;

    public EpgService(XtreamApiClient api, IXtreamCache cache)
    {
        _api = api;
        _cache = cache;
    }

    public async Task<IEnumerable<object>> GetAsync(
        int channelId,
        string url,
        CancellationToken ct)
    {
        if (_cache.TryGet(channelId, DateTime.UtcNow, DateTime.UtcNow, out var cached))
            return cached;

        var data = await _api.GetAsync<IEnumerable<object>>(url, ct)
                   ?? Enumerable.Empty<object>();

        _cache.Store(channelId, data);
        return data;
    }
}
