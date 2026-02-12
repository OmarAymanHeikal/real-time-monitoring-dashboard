using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

/// <summary>
/// Continuation job example: Cleanup after report generation
/// </summary>
public class ReportCleanupJob
{
    private readonly IServiceProvider _serviceProvider;

    public ReportCleanupJob(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task ExecuteAsync(int reportId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportCleanupJob>>();

        var report = await db.Reports.FindAsync(reportId);
        if (report == null)
        {
            logger.LogWarning("Report {ReportId} not found for cleanup.", reportId);
            return;
        }

        logger.LogInformation("Running cleanup tasks after report {ReportId} generation", reportId);

        // Simulate cleanup: Archive old reports, compress files, etc.
        await Task.Delay(1000); // Simulate work

        logger.LogInformation("Cleanup completed for report {ReportId}", reportId);
    }
}
