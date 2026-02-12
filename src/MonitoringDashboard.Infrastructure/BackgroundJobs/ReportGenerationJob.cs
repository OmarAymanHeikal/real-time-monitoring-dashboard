using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

public class ReportGenerationJob
{
    private readonly IServiceProvider _serviceProvider;

    public ReportGenerationJob(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task ExecuteAsync(int reportId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reportFile = scope.ServiceProvider.GetRequiredService<IReportFileService>();
        var hubNotification = scope.ServiceProvider.GetService<IHubNotificationService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportGenerationJob>>();

        var report = await db.Reports.Include(r => r.Server).FirstOrDefaultAsync(r => r.ReportId == reportId);
        if (report == null) { logger.LogWarning("Report {ReportId} not found.", reportId); return; }

        try
        {
            report.Status = "Processing";
            await db.SaveChangesAsync();

            var metrics = await db.Metrics.AsNoTracking()
                .Where(m => m.ServerId == report.ServerId && m.Timestamp >= report.StartTime && m.Timestamp <= report.EndTime)
                .OrderBy(m => m.Timestamp)
                .Select(m => new { m.Timestamp, m.CpuUsage, m.MemoryUsage, m.DiskUsage, m.ResponseTime, m.Status })
                .ToListAsync();

            var reportData = new { ReportId = report.ReportId, ServerId = report.ServerId, ServerName = report.Server.Name, report.StartTime, report.EndTime, GeneratedAt = DateTime.UtcNow, MetricsCount = metrics.Count, Metrics = metrics };
            var path = await reportFile.WriteReportAsync(report.ReportId, report.ServerId, report.StartTime, report.EndTime, reportData);
            report.FilePath = path;
            report.Status = "Completed";
            report.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            
            // Send email notification (simulated)
            await emailService.SendReportCompletedEmailAsync("admin@demo.local", report.ReportId, report.Server.Name, path);
            
            if (hubNotification != null) await hubNotification.SendReportStatusAsync(report.ReportId, report.Status, report.FilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Report generation failed for {ReportId}", reportId);
            report.Status = "Failed";
            await db.SaveChangesAsync();
            if (hubNotification != null) await hubNotification.SendReportStatusAsync(reportId, "Failed", null);
        }
    }
}
