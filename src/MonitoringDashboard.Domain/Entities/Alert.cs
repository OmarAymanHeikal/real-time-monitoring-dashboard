namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// Alert raised when a metric exceeds a threshold.
/// </summary>
public class Alert
{
    public int AlertId { get; set; }
    public int ServerId { get; set; }
    public required string MetricType { get; set; }
    public double MetricValue { get; set; }
    public double Threshold { get; set; }
    public string Status { get; set; } = "Triggered";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public Server Server { get; set; } = null!;
}
