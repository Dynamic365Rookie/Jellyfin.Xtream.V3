using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Xtream.Infrastructure.Monitoring;

public sealed class PerformanceMonitor : IDisposable
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly Dictionary<string, PerformanceMetrics> _metrics = new();
    private readonly object _lock = new();

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public IDisposable Track(string operationName)
    {
        return new PerformanceTracker(this, operationName);
    }

    internal void RecordMetric(string operation, TimeSpan duration, bool success)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(operation, out var metrics))
            {
                metrics = new PerformanceMetrics(operation);
                _metrics[operation] = metrics;
            }

            metrics.Record(duration, success);
        }
    }

    public void LogStatistics()
    {
        lock (_lock)
        {
            foreach (var metric in _metrics.Values)
            {
                _logger.LogInformation(
                    "Performance: {Operation} - Count: {Count}, Avg: {Avg}ms, Min: {Min}ms, Max: {Max}ms, Success Rate: {SuccessRate:P}",
                    metric.OperationName,
                    metric.TotalCount,
                    metric.AverageDuration.TotalMilliseconds,
                    metric.MinDuration.TotalMilliseconds,
                    metric.MaxDuration.TotalMilliseconds,
                    metric.SuccessRate);
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }
    }

    public void Dispose()
    {
        LogStatistics();
    }

    private class PerformanceTracker : IDisposable
    {
        private readonly PerformanceMonitor _monitor;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private bool _success = true;

        public PerformanceTracker(PerformanceMonitor monitor, string operationName)
        {
            _monitor = monitor;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void MarkFailure()
        {
            _success = false;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _monitor.RecordMetric(_operationName, _stopwatch.Elapsed, _success);
        }
    }

    private class PerformanceMetrics
    {
        public string OperationName { get; }
        public int TotalCount { get; private set; }
        public int SuccessCount { get; private set; }
        public TimeSpan TotalDuration { get; private set; }
        public TimeSpan MinDuration { get; private set; } = TimeSpan.MaxValue;
        public TimeSpan MaxDuration { get; private set; }
        public TimeSpan AverageDuration => TotalCount > 0 
            ? TimeSpan.FromTicks(TotalDuration.Ticks / TotalCount) 
            : TimeSpan.Zero;
        public double SuccessRate => TotalCount > 0 
            ? (double)SuccessCount / TotalCount 
            : 0;

        public PerformanceMetrics(string operationName)
        {
            OperationName = operationName;
        }

        public void Record(TimeSpan duration, bool success)
        {
            TotalCount++;
            if (success) SuccessCount++;

            TotalDuration += duration;

            if (duration < MinDuration)
                MinDuration = duration;

            if (duration > MaxDuration)
                MaxDuration = duration;
        }
    }
}
