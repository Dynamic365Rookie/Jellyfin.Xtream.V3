namespace Jellyfin.Xtream.Services.Synchronization;

/// <summary>
/// Result of a synchronization operation.
/// </summary>
public sealed class SyncResult
{
    private SyncResult(bool isSuccess, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public IReadOnlyList<string> Errors { get; }

    public static SyncResult Success() => new(true, Array.Empty<string>());

    public static SyncResult Failure(IEnumerable<string> errors) =>
        new(false, errors.ToList().AsReadOnly());

    public override string ToString() =>
        IsSuccess ? "Success" : $"Failed: {string.Join("; ", Errors)}";
}
