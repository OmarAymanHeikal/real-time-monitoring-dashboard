namespace MonitoringDashboard.Domain.Interfaces;

/// <summary>
/// Service responsible for collecting or generating system metrics.
/// Implemented in Infrastructure (real PerformanceCounter on Windows or simulated values).
/// </summary>
public interface ISystemMetricsService
{
    /// <summary>
    /// Gets current metrics for the given server (or simulated if not local).
    /// </summary>
    Task<ISystemMetricsSnapshot> GetMetricsAsync(int serverId, CancellationToken cancellationToken = default);
}
