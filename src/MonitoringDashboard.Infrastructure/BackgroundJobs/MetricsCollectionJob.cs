using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Services;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

public class MetricsCollectionJob
{
    private readonly IServiceProvider _serviceProvider;

    public MetricsCollectionJob(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task ExecuteAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var metricsService = scope.ServiceProvider.GetRequiredService<ISystemMetricsService>();
        var hubNotification = scope.ServiceProvider.GetService<Application.Common.Interfaces.IHubNotificationService>();
        var serverIds = await db.Servers.Select(s => s.ServerId).ToListAsync();

        foreach (var serverId in serverIds)
        {
            try
            {
                var snapshot = await metricsService.GetMetricsAsync(serverId);
                var metric = new Metric
                {
                    ServerId = serverId,
                    CpuUsage = snapshot.CpuUsage,
                    MemoryUsage = snapshot.MemoryUsage,
                    DiskUsage = snapshot.DiskUsage,
                    ResponseTime = snapshot.ResponseTimeMs,
                    Status = snapshot.Status
                };
                db.Metrics.Add(metric);
                var server = await db.Servers.FindAsync(serverId);
                if (server != null) server.Status = snapshot.Status;
                await db.SaveChangesAsync();
                if (hubNotification != null)
                    await hubNotification.SendMetricUpdateAsync(serverId, new { metric.ServerId, metric.CpuUsage, metric.MemoryUsage, metric.DiskUsage, metric.ResponseTime, metric.Status, metric.Timestamp });
            }
            catch (Exception ex)
            {
                scope.ServiceProvider.GetRequiredService<ILogger<MetricsCollectionJob>>().LogError(ex, "Metrics collection failed for server {ServerId}", serverId);
            }
        }
    }
}
