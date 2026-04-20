namespace Jellyfin.Xtream.Infrastructure.Utilities;

public static class BatchProcessor
{
    /// <summary>
    /// Traite une collection par lots pour éviter les problèmes de mémoire
    /// </summary>
    public static async Task ProcessInBatchesAsync<T>(
        IEnumerable<T> items,
        int batchSize,
        Func<IEnumerable<T>, Task> processFunc,
        CancellationToken ct = default)
    {
        var batch = new List<T>(batchSize);

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();

            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                await processFunc(batch);
                batch.Clear();
            }
        }

        // Traiter le dernier lot s'il reste des éléments
        if (batch.Count > 0)
        {
            await processFunc(batch);
        }
    }

    /// <summary>
    /// Traite une collection par lots de manière synchrone
    /// </summary>
    public static void ProcessInBatches<T>(
        IEnumerable<T> items,
        int batchSize,
        Action<IEnumerable<T>> processFunc,
        CancellationToken ct = default)
    {
        var batch = new List<T>(batchSize);

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();

            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                processFunc(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            processFunc(batch);
        }
    }

    /// <summary>
    /// Traite une collection en parallèle avec un degré de parallélisme spécifié
    /// </summary>
    public static async Task ProcessInParallelAsync<T>(
        IEnumerable<T> items,
        int maxDegreeOfParallelism,
        Func<T, Task> processFunc,
        CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        var tasks = new List<Task>();

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();

            await semaphore.WaitAsync(ct);

            var task = Task.Run(async () =>
            {
                try
                {
                    await processFunc(item);
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Découpe une collection en lots
    /// </summary>
    public static IEnumerable<IEnumerable<T>> ToBatches<T>(this IEnumerable<T> items, int batchSize)
    {
        var batch = new List<T>(batchSize);

        foreach (var item in items)
        {
            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}
