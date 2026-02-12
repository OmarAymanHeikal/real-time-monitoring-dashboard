using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendReportCompletedEmailAsync(string recipientEmail, int reportId, string serverName, string filePath, CancellationToken cancellationToken = default)
    {
        var subject = $"Report #{reportId} for {serverName} Ready";
        var body = $@"
            <h2>Report Generated Successfully</h2>
            <p>Your report for server <strong>{serverName}</strong> has been generated successfully.</p>
            <ul>
                <li><strong>Report ID:</strong> {reportId}</li>
                <li><strong>Server:</strong> {serverName}</li>
                <li><strong>File Path:</strong> {filePath}</li>
            </ul>
            <p>You can now download the report from the dashboard.</p>
        ";

        await SendEmailAsync(recipientEmail, subject, body, cancellationToken);
    }

    public async Task SendAlertEmailAsync(string recipientEmail, string serverName, string metricType, double value, double threshold, CancellationToken cancellationToken = default)
    {
        var subject = $"ALERT: {metricType} threshold exceeded on {serverName}";
        var body = $@"
            <h2 style='color: red;'>⚠️ Alert Notification</h2>
            <p>Server <strong>{serverName}</strong> has exceeded the {metricType} threshold.</p>
            <ul>
                <li><strong>Metric:</strong> {metricType}</li>
                <li><strong>Current Value:</strong> {value:F2}%</li>
                <li><strong>Threshold:</strong> {threshold:F2}%</li>
            </ul>
            <p style='color: red;'><strong>Please take immediate action.</strong></p>
        ";

        await SendEmailAsync(recipientEmail, subject, body, cancellationToken);
    }

    public async Task SendMaintenanceNotificationEmailAsync(string recipientEmail, string serverName, DateTime maintenanceTime, CancellationToken cancellationToken = default)
    {
        var subject = $"Scheduled Maintenance for {serverName}";
        var body = $@"
            <h2>Maintenance Notification</h2>
            <p>Server <strong>{serverName}</strong> is scheduled for maintenance.</p>
            <ul>
                <li><strong>Server:</strong> {serverName}</li>
                <li><strong>Maintenance Time:</strong> {maintenanceTime:yyyy-MM-dd HH:mm}</li>
                <li><strong>Expected Downtime:</strong> 30-60 minutes</li>
            </ul>
            <p>Please plan accordingly.</p>
        ";

        await SendEmailAsync(recipientEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["Email:FromName"] ?? "Monitoring Dashboard";
            
            // If SMTP not configured, fall back to logging only
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername))
            {
                _logger.LogWarning(
                    "SMTP not configured. Email simulated - To: {RecipientEmail}, Subject: {Subject}",
                    recipientEmail, subject);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(recipientEmail, recipientEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully - To: {RecipientEmail}, Subject: {Subject}",
                recipientEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email - To: {RecipientEmail}, Subject: {Subject}",
                recipientEmail, subject);
            throw;
        }
    }
}
