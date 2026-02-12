namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// A single metric snapshot for a server (CPU, memory, disk, response time).
/// </summary>
public class Metric
{
    public int MetricId { get; set; }
    public int ServerId { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double ResponseTime { get; set; }
    public required string Status { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Server Server { get; set; } = null!;
}
