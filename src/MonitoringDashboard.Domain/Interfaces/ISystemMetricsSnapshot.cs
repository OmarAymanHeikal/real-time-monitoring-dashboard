namespace MonitoringDashboard.Domain.Interfaces;

/// <summary>
/// Snapshot of system metrics (CPU, memory, disk, response time) for a single point in time.
/// Used by infrastructure to collect or simulate metrics.
/// </summary>
public interface ISystemMetricsSnapshot
{
    double CpuUsage { get; }
    double MemoryUsage { get; }
    double DiskUsage { get; }
    double ResponseTimeMs { get; }
    string Status { get; }
}
