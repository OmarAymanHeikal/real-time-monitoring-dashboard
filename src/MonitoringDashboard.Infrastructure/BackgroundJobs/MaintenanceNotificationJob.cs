using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

/// <summary>
/// Delayed job example: Schedule maintenance window notification
/// </summary>
public class MaintenanceNotificationJob
{
    private readonly IServiceProvider _serviceProvider;

    public MaintenanceNotificationJob(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task ExecuteAsync(int serverId, DateTime maintenanceTime)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hubNotification = scope.ServiceProvider.GetService<IHubNotificationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MaintenanceNotificationJob>>();

        var server = await db.Servers.FirstOrDefaultAsync(s => s.ServerId == serverId);
        if (server == null)
        {
            logger.LogWarning("Server {ServerId} not found for maintenance notification.", serverId);
            return;
        }

        logger.LogInformation("Sending maintenance notification for server {ServerName} scheduled at {MaintenanceTime}", 
            server.Name, maintenanceTime);

        // Update server status
        server.Status = "Maintenance";
        await db.SaveChangesAsync();

        // Send real-time notification
        if (hubNotification != null)
        {
            await hubNotification.SendMaintenanceAlertAsync(new
            {
                serverId,
                serverName = server.Name,
                maintenanceTime,
                message = $"Server {server.Name} entering maintenance mode"
            });
        }
    }
}
