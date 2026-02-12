namespace MonitoringDashboard.Domain.Events;

/// <summary>
/// Raised when report generation completes (for SignalR notification).
/// </summary>
public record ReportCompletedEvent(
    int ReportId,
    int ServerId,
    string Status,
    string? FilePath) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
