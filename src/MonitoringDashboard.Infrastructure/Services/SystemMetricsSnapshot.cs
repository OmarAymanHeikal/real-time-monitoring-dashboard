using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

internal sealed class SystemMetricsSnapshot : ISystemMetricsSnapshot
{
    public double CpuUsage { get; init; }
    public double MemoryUsage { get; init; }
    public double DiskUsage { get; init; }
    public double ResponseTimeMs { get; init; }
    public string Status { get; init; } = "Up";
}
