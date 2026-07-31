using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Turnero.SL.Services;

/// <summary>
/// Monitors container memory usage via cgroup v2/v1 and logs warnings 
/// when usage exceeds configured thresholds.
/// Helps detect OOM risk before the container is killed by the kernel.
/// </summary>
public class ContainerMemoryMonitor : IHostedService, IDisposable
{
    private readonly ILogger<ContainerMemoryMonitor> _logger;
    private Timer? _timer;
    private int _checkCount;

    private const int CheckIntervalSeconds = 30;

    // Thresholds: warn at 80%, critical at 90%, OOM-risk at 95%
    private static readonly (double Threshold, string Level)[] Thresholds =
    [
        (0.80, "WARN"),
        (0.90, "CRITICAL"),
        (0.95, "OOM-RISK")
    ];

    public ContainerMemoryMonitor(ILogger<ContainerMemoryMonitor> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ContainerMemoryMonitor started – checking cgroup memory every {Interval}s",
            CheckIntervalSeconds);

        // Run initial check immediately
        CheckMemory();

        _timer = new Timer(_ => CheckMemory(), null,
            TimeSpan.FromSeconds(CheckIntervalSeconds),
            TimeSpan.FromSeconds(CheckIntervalSeconds));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        _logger.LogInformation("ContainerMemoryMonitor stopped.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    // -----------------------------------------------------------------------
    //  Internal
    // -----------------------------------------------------------------------

    private void CheckMemory()
    {
        Interlocked.Increment(ref _checkCount);

        try
        {
            var info = ReadMemoryInfo();
            if (info is null)
            {
                // Only log the "not available" message once, on the first check
                if (_checkCount == 1)
                    _logger.LogInformation(
                        "ContainerMemoryMonitor: cgroup memory stats unavailable " +
                        "(not running inside a container or unsupported cgroup version). " +
                        "Monitoring disabled.");

                // Stop the timer to avoid spamming
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            var infoVal = info.Value;
            var usageMb = infoVal.CurrentBytes / (1024.0 * 1024.0);
            var limitMb = infoVal.LimitBytes / (1024.0 * 1024.0);
            var percentage = infoVal.Percentage;

            // ----- Normal range (< 80 %) – log infrequently (every ~2 min) -----
            if (percentage < 0.80)
            {
                if (_checkCount % 4 == 0) // every 4th check = ~2 minutes
                {
                    _logger.LogInformation(
                        "[MemoryMonitor] Memory usage: {Current:F1} MB / {Limit:F1} MB ({Percent:F1}%)",
                        usageMb, limitMb, percentage * 100);
                }
                return;
            }

            // ----- Threshold exceeded – log every time -----
            foreach (var (threshold, level) in Thresholds)
            {
                if (!(percentage >= threshold)) continue;

                _logger.LogWarning(
                    "[MemoryMonitor] {Level} – Memory usage: {Current:F1} MB / {Limit:F1} MB ({Percent:F1}%)",
                    level, usageMb, limitMb, percentage * 100);
                break; // log only the most specific level that was crossed
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[MemoryMonitor] Failed to read memory stats – {Message}", ex.Message);
        }
    }

    // -----------------------------------------------------------------------
    //  Public helpers (also used by the /health endpoint)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reads the current memory usage and limit from cgroup v2 or cgroup v1.
    /// Returns <see langword="null"/> when neither is available (not in a container).
    /// </summary>
    public static MemoryInfo? ReadMemoryInfo()
    {
        // --- cgroup v2 (default since Docker 20.10+, kernel ≥ 4.15) ---
        const string cg2Current = "/sys/fs/cgroup/memory.current";
        const string cg2Max = "/sys/fs/cgroup/memory.max";

        if (File.Exists(cg2Current) && File.Exists(cg2Max))
        {
            var currentStr = File.ReadAllText(cg2Current).Trim();
            var maxStr = File.ReadAllText(cg2Max).Trim();

            if (long.TryParse(currentStr, out var currentBytes) &&
                long.TryParse(maxStr, out var maxBytes) &&
                maxBytes > 0)
            {
                return new MemoryInfo(currentBytes, maxBytes, (double)currentBytes / maxBytes);
            }
        }

        // --- cgroup v1 (legacy) ---
        const string cg1Current = "/sys/fs/cgroup/memory/memory.usage_in_bytes";
        const string cg1Max = "/sys/fs/cgroup/memory/memory.limit_in_bytes";

        if (File.Exists(cg1Current) && File.Exists(cg1Max))
        {
            var currentStr = File.ReadAllText(cg1Current).Trim();
            var maxStr = File.ReadAllText(cg1Max).Trim();

            if (long.TryParse(currentStr, out var currentBytes) &&
                long.TryParse(maxStr, out var maxBytes) &&
                maxBytes > 0)
            {
                // In cgroup v1, limit_in_bytes can be 2^63-1 (≈ 9.2 EB) when
                // no explicit limit is set.  Only treat values < 1 TB as real.
                if (maxBytes < 1L << 40)
                {
                    return new MemoryInfo(currentBytes, maxBytes, (double)currentBytes / maxBytes);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a human-readable memory summary string, e.g. "384 MB / 768 MB (50.0%)",
    /// or <see langword="null"/> if cgroup stats are unavailable.
    /// </summary>
    public static string? GetMemorySummary()
    {
        var info = ReadMemoryInfo();
        if (info is null) return null;

        var usageMb = info.Value.CurrentBytes / (1024.0 * 1024.0);
        var limitMb = info.Value.LimitBytes / (1024.0 * 1024.0);
        return $"{usageMb:F0} MB / {limitMb:F0} MB ({info.Value.Percentage * 100:F1}%)";
    }
}

// -----------------------------------------------------------------------
//  Value type
// -----------------------------------------------------------------------

/// <summary>
/// Holds a snapshot of container memory metrics.
/// </summary>
public readonly record struct MemoryInfo(long CurrentBytes, long LimitBytes, double Percentage);
