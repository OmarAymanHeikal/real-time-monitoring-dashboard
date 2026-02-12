using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MonitoringDashboard.WebAPI.Hubs;

[Authorize]
public class MonitoringHub : Hub
{
    private static readonly HashSet<string> OnlineUsers = new();
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
        
        lock (OnlineUsers)
        {
            OnlineUsers.Add(userId);
        }
        
        // Notify all clients about user presence
        await Clients.All.SendAsync("UserPresence", new { userId, isOnline = true, totalOnline = OnlineUsers.Count });
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
        
        lock (OnlineUsers)
        {
            OnlineUsers.Remove(userId);
        }
        
        // Notify all clients about user disconnection
        await Clients.All.SendAsync("UserPresence", new { userId, isOnline = false, totalOnline = OnlineUsers.Count });
        await base.OnDisconnectedAsync(exception);
    }
    
    public Task<int> GetOnlineUsersCount()
    {
        return Task.FromResult(OnlineUsers.Count);
    }

    public async Task SubscribeToServer(int serverId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"server_{serverId}");
    }

    public async Task UnsubscribeFromServer(int serverId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"server_{serverId}");
    }

    public async Task JoinAlerts()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "alerts");
    }

    public async Task JoinReports()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "reports");
    }
}
