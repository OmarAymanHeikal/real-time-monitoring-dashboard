using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MonitoringDashboard.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext db, IIdentityService identity)
    {
        if (await db.Roles.AnyAsync()) return;

        var adminRole = new Role { Name = "Admin", Description = "Administrator" };
        var userRole = new Role { Name = "User", Description = "Standard user" };
        db.Roles.AddRange(adminRole, userRole);
        await db.SaveChangesAsync();
        
        // Seed Permissions (Many-to-Many)
        var permissions = new[]
        {
            new Permission { Name = "ViewServers", Description = "View server list and details", Category = "Servers" },
            new Permission { Name = "ManageServers", Description = "Create, update, delete servers", Category = "Servers" },
            new Permission { Name = "ViewMetrics", Description = "View metrics data", Category = "Metrics" },
            new Permission { Name = "ViewAlerts", Description = "View alerts", Category = "Alerts" },
            new Permission { Name = "ManageAlerts", Description = "Resolve and manage alerts", Category = "Alerts" },
            new Permission { Name = "ViewReports", Description = "View reports", Category = "Reports" },
            new Permission { Name = "GenerateReports", Description = "Generate new reports", Category = "Reports" },
            new Permission { Name = "ManageUsers", Description = "Manage users and roles", Category = "Users" },
            new Permission { Name = "ViewJobs", Description = "View background jobs", Category = "Jobs" }
        };
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        var adminUser = new User
        {
            UserName = "admin",
            Email = "admin@demo.local",
            PasswordHash = identity.HashPassword("Admin@123"),
            RoleId = adminRole.RoleId
        };
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();
        
        // Assign all permissions to admin user (Many-to-Many)
        adminUser.Permissions = permissions.ToList();
        await db.SaveChangesAsync();

        var servers = new[]
        {
            new Server { Name = "Web Server 01", IPAddress = "192.168.1.10", Status = "Up", Description = "Primary web server" },
            new Server { Name = "DB Server 01", IPAddress = "192.168.1.20", Status = "Up", Description = "SQL Server" },
            new Server { Name = "App Server 01", IPAddress = "192.168.1.30", Status = "Up", Description = "Application server" }
        };
        db.Servers.AddRange(servers);
        await db.SaveChangesAsync();

        var rnd = new Random();
        foreach (var s in servers)
        {
            for (var i = 0; i < 20; i++)
            {
                db.Metrics.Add(new Metric
                {
                    ServerId = s.ServerId,
                    CpuUsage = 20 + rnd.NextDouble() * 60,
                    MemoryUsage = 30 + rnd.NextDouble() * 50,
                    DiskUsage = 25 + rnd.NextDouble() * 45,
                    ResponseTime = 5 + rnd.NextDouble() * 95,
                    Status = "Up",
                    Timestamp = DateTime.UtcNow.AddMinutes(-i * 5)
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
