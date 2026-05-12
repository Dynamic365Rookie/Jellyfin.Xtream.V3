namespace Jellyfin.Xtream.Domain.Models;

/// <summary>
/// Represents a synchronization history entry with summary statistics.
/// </summary>
public sealed record SyncHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime StartTime { get; init; }

    public DateTime? EndTime { get; init; }

    public string Status { get; init; } = "Running"; // Running, Success, Failed, Cancelled

    public double DurationSeconds { get; init; }

    // Statistics
    public int MoviesTotal { get; init; }
    public int MoviesNew { get; init; }
    public int MoviesUpdated { get; init; }

    public int SeriesTotal { get; init; }
    public int SeriesNew { get; init; }
    public int SeriesUpdated { get; init; }

    public int ChannelsTotal { get; init; }
    public int ChannelsNew { get; init; }
    public int ChannelsUpdated { get; init; }

    public int ErrorCount { get; init; }

    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Summary message for logging.
    /// </summary>
    public string GetSummary() =>
        $"Sync completed: Movies({MoviesTotal}, +{MoviesNew}, ~{MoviesUpdated}), " +
        $"Series({SeriesTotal}, +{SeriesNew}, ~{SeriesUpdated}), " +
        $"Channels({ChannelsTotal}, +{ChannelsNew}, ~{ChannelsUpdated}) " +
        $"- {DurationSeconds:F1}s - Errors: {ErrorCount}";
}
