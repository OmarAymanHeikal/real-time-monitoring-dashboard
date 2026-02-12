using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Infrastructure.Persistence;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

/// <summary>
/// Recurring job: check latest metrics against thresholds and create alerts. Can also be used as delayed job.
/// </summary>
public class AlertThresholdJob
{
    private readonly IServiceProvider _serviceProvider;
    private const double CpuThreshold = 80;
    private const double MemoryThreshold = 90;
    private const double DiskThreshold = 85;

    public AlertThresholdJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hubNotification = scope.ServiceProvider.GetService<Application.Common.Interfaces.IHubNotificationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AlertThresholdJob>>();

        var serverIds = await db.Servers.Select(s => s.ServerId).ToListAsync();
        foreach (var serverId in serverIds)
        {
            var m = await db.Metrics.AsNoTracking().Where(x => x.ServerId == serverId).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();
            if (m == null) continue;
            if (m.CpuUsage > CpuThreshold)
                await CreateAlertIfNewAsync(db, m.ServerId, "CpuUsage", m.CpuUsage, CpuThreshold, hubNotification, logger);
            if (m.MemoryUsage > MemoryThreshold)
                await CreateAlertIfNewAsync(db, m.ServerId, "MemoryUsage", m.MemoryUsage, MemoryThreshold, hubNotification, logger);
            if (m.DiskUsage > DiskThreshold)
                await CreateAlertIfNewAsync(db, m.ServerId, "DiskUsage", m.DiskUsage, DiskThreshold, hubNotification, logger);
        }

        await db.SaveChangesAsync();
    }

    private static async Task CreateAlertIfNewAsync(
        ApplicationDbContext db,
        int serverId,
        string metricType,
        double value,
        double threshold,
        Application.Common.Interfaces.IHubNotificationService? hub,
        ILogger logger)
    {
        var recent = await db.Alerts.AnyAsync(a => a.ServerId == serverId && a.MetricType == metricType && a.Status == "Triggered" && a.CreatedAt > DateTime.UtcNow.AddMinutes(-15));
        if (recent) return;
        var alert = new Alert { ServerId = serverId, MetricType = metricType, MetricValue = value, Threshold = threshold, Status = "Triggered" };
        db.Alerts.Add(alert);
        await db.SaveChangesAsync();
        if (hub != null)
            await hub.SendAlertAsync(new { alert.AlertId, alert.ServerId, alert.MetricType, alert.MetricValue, alert.Threshold, alert.Status, alert.CreatedAt });
    }
}
