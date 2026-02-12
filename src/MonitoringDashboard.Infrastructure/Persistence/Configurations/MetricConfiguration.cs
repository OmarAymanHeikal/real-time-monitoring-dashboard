using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metrics");
        builder.HasKey(m => m.MetricId);
        builder.Property(m => m.Status).HasMaxLength(20).IsRequired();
        builder.HasIndex(m => new { m.ServerId, m.Timestamp });
    }
}
