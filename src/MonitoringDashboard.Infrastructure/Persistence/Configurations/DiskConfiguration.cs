using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class DiskConfiguration : IEntityTypeConfiguration<Disk>
{
    public void Configure(EntityTypeBuilder<Disk> builder)
    {
        builder.ToTable("Disks");
        builder.HasKey(d => d.DiskId);
        builder.Property(d => d.DriveLetter).HasMaxLength(5).IsRequired();
    }
}
