namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// Report generated for a server over a time range.
/// </summary>
public class Report
{
    public int ReportId { get; set; }
    public int ServerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Pending";
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Server Server { get; set; } = null!;
}
