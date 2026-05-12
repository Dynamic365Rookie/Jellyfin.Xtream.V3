using System.Collections.Concurrent;
using Jellyfin.Xtream.Domain.Models;

namespace Jellyfin.Xtream.Services.Synchronization;

/// <summary>
/// Service to track synchronization history in memory.
/// Stores the last 100 sync operations for debugging purposes.
/// </summary>
public sealed class SyncHistoryService
{
    private readonly ConcurrentQueue<SyncHistory> _history = new();
    private const int MaxHistorySize = 100;

    /// <summary>
    /// Adds a sync history entry.
    /// </summary>
    public void AddHistory(SyncHistory entry)
    {
        _history.Enqueue(entry);

        // Keep only last 100 entries
        while (_history.Count > MaxHistorySize)
        {
            _history.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Gets recent sync history (most recent first).
    /// </summary>
    public IEnumerable<SyncHistory> GetRecentHistory(int count = 20)
    {
        return _history
            .OrderByDescending(h => h.StartTime)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets the last sync operation.
    /// </summary>
    public SyncHistory? GetLastSync()
    {
        return _history
            .OrderByDescending(h => h.StartTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Clears all history.
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }
}
