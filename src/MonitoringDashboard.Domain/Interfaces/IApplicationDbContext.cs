using MonitoringDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MonitoringDashboard.Domain.Interfaces;

/// <summary>
/// Abstraction for the application's database context (Dependency Inversion).
/// Infrastructure implements this with EF Core.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Server> Servers { get; }
    DbSet<Metric> Metrics { get; }
    DbSet<Disk> Disks { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<Report> Reports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
