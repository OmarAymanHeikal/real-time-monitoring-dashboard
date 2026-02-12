using Microsoft.Extensions.Logging;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

/// <summary>
/// Collects or simulates system metrics. On Windows can use PerformanceCounter; otherwise simulates.
/// </summary>
public class SystemMetricsService : ISystemMetricsService
{
    private readonly ILogger<SystemMetricsService> _logger;
    private static readonly Random Rnd = new();

    public SystemMetricsService(ILogger<SystemMetricsService> logger)
    {
        _logger = logger;
    }

    public Task<ISystemMetricsSnapshot> GetMetricsAsync(int serverId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (OperatingSystem.IsWindows())
        {
            try
            {
                var snapshot = GetWindowsMetrics();
                if (snapshot != null)
                    return Task.FromResult<ISystemMetricsSnapshot>(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PerformanceCounter failed, falling back to simulated metrics.");
            }
        }

        var simulated = new SystemMetricsSnapshot
        {
            CpuUsage = Math.Round(25 + Rnd.NextDouble() * 55, 2),
            MemoryUsage = Math.Round(30 + Rnd.NextDouble() * 50, 2),
            DiskUsage = Math.Round(20 + Rnd.NextDouble() * 60, 2),
            ResponseTimeMs = Math.Round(5 + Rnd.NextDouble() * 95, 2),
            Status = Rnd.Next(100) > 5 ? "Up" : "Warning"
        };
        return Task.FromResult<ISystemMetricsSnapshot>(simulated);
    }

    private static SystemMetricsSnapshot? GetWindowsMetrics()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            using var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
            using var ramCounter = new System.Diagnostics.PerformanceCounter("Memory", "% Committed Bytes In Use");
            cpuCounter.NextValue();
            ramCounter.NextValue();
            System.Threading.Thread.Sleep(100);
            var cpu = (double)cpuCounter.NextValue();
            var mem = (double)ramCounter.NextValue();
            var disk = 100.0 - (OperatingSystem.IsWindows() ? GetDiskFreePercent() : 50.0);
            return new SystemMetricsSnapshot
            {
                CpuUsage = Math.Round(Math.Min(100, cpu), 2),
                MemoryUsage = Math.Round(Math.Min(100, mem), 2),
                DiskUsage = Math.Round(Math.Min(100, disk), 2),
                ResponseTimeMs = Math.Round(10 + new Random().NextDouble() * 40, 2),
                Status = cpu > 95 || mem > 95 ? "Warning" : "Up"
            };
        }
        catch
        {
            return null;
        }
    }

    private static double GetDiskFreePercent()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == "C:\\");
            if (drive != null)
                return 100.0 - (drive.TotalSize > 0 ? (double)drive.AvailableFreeSpace / drive.TotalSize * 100 : 0);
        }
        catch { }
        return 50;
    }
}
