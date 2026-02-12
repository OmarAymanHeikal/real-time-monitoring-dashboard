namespace MonitoringDashboard.Domain.Events;

/// <summary>
/// Raised when a metric threshold is breached and an alert is created.
/// </summary>
public record AlertTriggeredEvent(
    int AlertId,
    int ServerId,
    string MetricType,
    double MetricValue,
    double Threshold) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
