using Microsoft.AspNetCore.SignalR;
using MonitoringDashboard.Application.Common.Interfaces;

namespace MonitoringDashboard.WebAPI.Hubs;

public class SignalRHubNotificationService : IHubNotificationService
{
    private readonly IHubContext<MonitoringHub> _hub;

    public SignalRHubNotificationService(IHubContext<MonitoringHub> hub) => _hub = hub;

    public Task SendMetricUpdateAsync(int serverId, object metric, CancellationToken cancellationToken = default)
        => _hub.Clients.Group($"server_{serverId}").SendAsync("MetricUpdate", metric, cancellationToken);

    public Task SendAlertAsync(object alert, CancellationToken cancellationToken = default)
        => _hub.Clients.Group("alerts").SendAsync("Alert", alert, cancellationToken);

    public Task SendReportStatusAsync(int reportId, string status, string? filePath, CancellationToken cancellationToken = default)
        => _hub.Clients.Group("reports").SendAsync("ReportStatus", new { reportId, status, filePath }, cancellationToken);
    
    public Task SendMaintenanceAlertAsync(object maintenance, CancellationToken cancellationToken = default)
        => _hub.Clients.All.SendAsync("MaintenanceAlert", maintenance, cancellationToken);
    
    public Task NotifyUserPresenceAsync(string userId, bool isOnline, CancellationToken cancellationToken = default)
        => _hub.Clients.All.SendAsync("UserPresence", new { userId, isOnline }, cancellationToken);
}
