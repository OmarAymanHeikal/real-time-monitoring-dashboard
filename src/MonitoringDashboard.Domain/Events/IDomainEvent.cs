namespace MonitoringDashboard.Domain.Events;

/// <summary>
/// Marker for domain events (for future event sourcing or in-process messaging).
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
