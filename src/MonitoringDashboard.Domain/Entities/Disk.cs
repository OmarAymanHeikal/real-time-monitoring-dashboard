namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// Disk usage snapshot for a server drive.
/// </summary>
public class Disk
{
    public int DiskId { get; set; }
    public int ServerId { get; set; }
    public required string DriveLetter { get; set; }
    public long FreeSpaceMB { get; set; }
    public long TotalSpaceMB { get; set; }
    public double UsedPercentage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Server Server { get; set; } = null!;
}
