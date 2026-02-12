using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendReportCompletedEmailAsync(string recipientEmail, int reportId, string serverName, string filePath, CancellationToken cancellationToken = default)
    {
        // Simulate sending email
        await Task.Delay(500, cancellationToken);
        
        _logger.LogInformation(
            "EMAIL SIMULATED: Report Completed. To: {RecipientEmail}, Subject: Report #{ReportId} for {ServerName} Ready, File Path: {FilePath}",
            recipientEmail, reportId, serverName, filePath);
    }

    public async Task SendAlertEmailAsync(string recipientEmail, string serverName, string metricType, double value, double threshold, CancellationToken cancellationToken = default)
    {
        // Simulate sending email
        await Task.Delay(500, cancellationToken);
        
        _logger.LogWarning(
            "EMAIL SIMULATED: Alert Notification. To: {RecipientEmail}, Server: {ServerName}, {MetricType} threshold exceeded. Current: {Value}%, Threshold: {Threshold}%",
            recipientEmail, serverName, metricType, value, threshold);
    }

    public async Task SendMaintenanceNotificationEmailAsync(string recipientEmail, string serverName, DateTime maintenanceTime, CancellationToken cancellationToken = default)
    {
        // Simulate sending email
        await Task.Delay(500, cancellationToken);
        
        _logger.LogInformation(
            "EMAIL SIMULATED: Maintenance Notification. To: {RecipientEmail}, Server: {ServerName}, Scheduled: {MaintenanceTime}",
            recipientEmail, serverName, maintenanceTime);
    }
}
