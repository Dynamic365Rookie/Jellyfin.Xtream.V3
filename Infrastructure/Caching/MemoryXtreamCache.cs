using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Jellyfin.Xtream.Infrastructure.Caching;

public sealed class MemoryXtreamCache : IXtreamCache, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;
    private readonly SemaphoreSlim _cleanupLock = new(1, 1);
    private DateTime _lastCleanup = DateTime.UtcNow;
    private const int MaxCacheEntries = 10000;

    public MemoryXtreamCache()
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = MaxCacheEntries,
            CompactionPercentage = 0.25,
            ExpirationScanFrequency = TimeSpan.FromMinutes(5)
        });
        _defaultExpiration = TimeSpan.FromHours(2);
    }

    public bool TryGet(int channelId, DateTime from, DateTime to, out IEnumerable<object> programs)
    {
        var key = GetCacheKey(channelId, from, to);

        if (_cache.TryGetValue(key, out IEnumerable<object>? cachedPrograms) && cachedPrograms != null)
        {
            programs = cachedPrograms;
            return true;
        }

        programs = Enumerable.Empty<object>();
        return false;
    }

    public void Store(int channelId, IEnumerable<object> programs)
    {
        Store(channelId, programs, _defaultExpiration);
    }

    public void Store(int channelId, IEnumerable<object> programs, TimeSpan expiration)
    {
        var now = DateTime.UtcNow;
        var key = GetCacheKey(channelId, now, now.AddDays(7));

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetAbsoluteExpiration(expiration)
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .RegisterPostEvictionCallback((evictedKey, value, reason, state) =>
            {
                // Log ou cleanup si nécessaire
            });

        _cache.Set(key, programs, cacheEntryOptions);

        // Cleanup périodique pour éviter la surcharge
        if ((DateTime.UtcNow - _lastCleanup).TotalMinutes > 15)
        {
            _ = Task.Run(() => PerformCleanupAsync());
        }
    }

    public void Clear()
    {
        if (_cache is MemoryCache memCache)
        {
            memCache.Compact(1.0);
        }
    }

    public void Remove(int channelId)
    {
        // Impossible de supprimer efficacement sans tracker toutes les clés
        // Une amélioration serait d'ajouter un index
    }

    private string GetCacheKey(int channelId, DateTime from, DateTime to)
    {
        return $"epg_{channelId}_{from:yyyyMMdd}_{to:yyyyMMdd}";
    }

    private async Task PerformCleanupAsync()
    {
        await _cleanupLock.WaitAsync();
        try
        {
            if (_cache is MemoryCache memCache)
            {
                memCache.Compact(0.1); // Retire 10% des entrées les moins utilisées
            }
            _lastCleanup = DateTime.UtcNow;
        }
        finally
        {
            _cleanupLock.Release();
        }
    }

    public void Dispose()
    {
        _cache?.Dispose();
        _cleanupLock?.Dispose();
    }
}
