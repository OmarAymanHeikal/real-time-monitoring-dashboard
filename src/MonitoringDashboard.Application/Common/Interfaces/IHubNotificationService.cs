namespace MonitoringDashboard.Application.Common.Interfaces;

/// <summary>
/// Abstraction for pushing real-time notifications via SignalR (WebAPI implements).
/// </summary>
public interface IHubNotificationService
{
    Task SendMetricUpdateAsync(int serverId, object metric, CancellationToken cancellationToken = default);
    Task SendAlertAsync(object alert, CancellationToken cancellationToken = default);
    Task SendReportStatusAsync(int reportId, string status, string? filePath, CancellationToken cancellationToken = default);
    Task SendMaintenanceAlertAsync(object maintenance, CancellationToken cancellationToken = default);
    Task NotifyUserPresenceAsync(string userId, bool isOnline, CancellationToken cancellationToken = default);
}
