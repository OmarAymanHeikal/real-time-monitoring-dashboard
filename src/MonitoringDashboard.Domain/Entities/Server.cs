using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// Monitored server or service entity.
/// </summary>
public class Server
{
    public int ServerId { get; set; }
    public required string Name { get; set; }
    public string? IPAddress { get; set; }
    public string Status { get; set; } = nameof(ServerStatus.Up);
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
    public ICollection<Disk> Disks { get; set; } = new List<Disk>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
