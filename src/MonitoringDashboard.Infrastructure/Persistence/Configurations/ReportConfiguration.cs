using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(r => r.ReportId);
        builder.Property(r => r.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Pending");
        builder.Property(r => r.FilePath).HasMaxLength(500);
    }
}
