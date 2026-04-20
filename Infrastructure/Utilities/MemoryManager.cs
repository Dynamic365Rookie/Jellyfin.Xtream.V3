using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Infrastructure.Utilities;

public sealed class MemoryManager
{
    private readonly ILogger<MemoryManager> _logger;
    private readonly long _maxMemoryBytes;
    private readonly double _memoryThreshold;

    public MemoryManager(ILogger<MemoryManager> logger, long maxMemoryMB = 2048)
    {
        _logger = logger;
        _maxMemoryBytes = maxMemoryMB * 1024 * 1024;
        _memoryThreshold = 0.8; // 80% du max
    }

    public void CheckMemoryUsage(string context = "")
    {
        var process = Process.GetCurrentProcess();
        var currentMemory = process.WorkingSet64;
        var percentUsed = (double)currentMemory / _maxMemoryBytes;

        if (percentUsed > _memoryThreshold)
        {
            _logger.LogWarning(
                "High memory usage detected ({Context}): {CurrentMB}MB / {MaxMB}MB ({Percent:P})",
                context,
                currentMemory / (1024 * 1024),
                _maxMemoryBytes / (1024 * 1024),
                percentUsed);

            ForceGarbageCollection();
        }
    }

    public void ForceGarbageCollection()
    {
        _logger.LogInformation("Forcing garbage collection...");

        var beforeMemory = GC.GetTotalMemory(false);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);

        var afterMemory = GC.GetTotalMemory(true);
        var freedMemory = beforeMemory - afterMemory;

        _logger.LogInformation(
            "Garbage collection completed. Freed {FreedMB}MB",
            freedMemory / (1024 * 1024));
    }

    public MemorySnapshot GetMemorySnapshot()
    {
        var process = Process.GetCurrentProcess();

        return new MemorySnapshot
        {
            WorkingSet = process.WorkingSet64,
            PrivateMemory = process.PrivateMemorySize64,
            VirtualMemory = process.VirtualMemorySize64,
            ManagedMemory = GC.GetTotalMemory(false),
            GCGen0Collections = GC.CollectionCount(0),
            GCGen1Collections = GC.CollectionCount(1),
            GCGen2Collections = GC.CollectionCount(2)
        };
    }

    public void LogMemoryUsage(string context = "")
    {
        var snapshot = GetMemorySnapshot();

        _logger.LogInformation(
            "Memory usage ({Context}): Working={WorkingMB}MB, Private={PrivateMB}MB, Managed={ManagedMB}MB, GC(0/1/2)={GC0}/{GC1}/{GC2}",
            context,
            snapshot.WorkingSet / (1024 * 1024),
            snapshot.PrivateMemory / (1024 * 1024),
            snapshot.ManagedMemory / (1024 * 1024),
            snapshot.GCGen0Collections,
            snapshot.GCGen1Collections,
            snapshot.GCGen2Collections);
    }

    public class MemorySnapshot
    {
        public long WorkingSet { get; init; }
        public long PrivateMemory { get; init; }
        public long VirtualMemory { get; init; }
        public long ManagedMemory { get; init; }
        public int GCGen0Collections { get; init; }
        public int GCGen1Collections { get; init; }
        public int GCGen2Collections { get; init; }
    }
}
