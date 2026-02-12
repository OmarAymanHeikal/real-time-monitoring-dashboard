using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class ServerConfiguration : IEntityTypeConfiguration<Server>
{
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.ToTable("Servers");
        builder.HasKey(s => s.ServerId);
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.IPAddress).HasMaxLength(50);
        builder.Property(s => s.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Up");
        builder.Property(s => s.Description).HasMaxLength(250);
        builder.HasMany(s => s.Metrics).WithOne(m => m.Server).HasForeignKey(m => m.ServerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Disks).WithOne(d => d.Server).HasForeignKey(d => d.ServerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Alerts).WithOne(a => a.Server).HasForeignKey(a => a.ServerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Reports).WithOne(r => r.Server).HasForeignKey(r => r.ServerId).OnDelete(DeleteBehavior.Cascade);
    }
}
