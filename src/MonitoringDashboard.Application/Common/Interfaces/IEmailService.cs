namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendReportCompletedEmailAsync(string recipientEmail, int reportId, string serverName, string filePath, CancellationToken cancellationToken = default);
    Task SendAlertEmailAsync(string recipientEmail, string serverName, string metricType, double value, double threshold, CancellationToken cancellationToken = default);
    Task SendMaintenanceNotificationEmailAsync(string recipientEmail, string serverName, DateTime maintenanceTime, CancellationToken cancellationToken = default);
}
