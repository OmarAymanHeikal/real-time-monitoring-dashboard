using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.RoleId);
        builder.Property(r => r.Name).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(250);
        builder.HasIndex(r => r.Name).IsUnique();
        builder.HasMany(r => r.Users).WithOne(u => u.Role).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
