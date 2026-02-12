using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Server> Servers => Set<Server>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<Disk> Disks => Set<Disk>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
