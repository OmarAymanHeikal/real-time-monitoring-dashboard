using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.PermissionId);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Description)
            .HasMaxLength(250);
            
        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(p => p.Name).IsUnique();
        
        // Many-to-many configuration
        builder.HasMany(p => p.Users)
            .WithMany(u => u.Permissions)
            .UsingEntity<Dictionary<string, object>>(
                "UserPermissions",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                j =>
                {
                    j.HasKey("UserId", "PermissionId");
                    j.ToTable("UserPermissions");
                });
    }
}
