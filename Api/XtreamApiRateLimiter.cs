using System.Threading;

namespace Jellyfin.Xtream.Api;

public sealed class XtreamApiRateLimiter
{
    private readonly SemaphoreSlim _semaphore = new(4);

    public async Task WaitAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        _ = Task.Delay(300, ct)
            .ContinueWith(_ => _semaphore.Release(), ct);
    }
}
