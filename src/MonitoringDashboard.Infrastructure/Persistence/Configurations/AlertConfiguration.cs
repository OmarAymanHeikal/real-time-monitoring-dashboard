using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");
        builder.HasKey(a => a.AlertId);
        builder.Property(a => a.MetricType).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Triggered");
        builder.HasIndex(a => new { a.ServerId, a.CreatedAt });
    }
}
