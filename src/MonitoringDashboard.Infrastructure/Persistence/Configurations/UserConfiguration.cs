using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.RefreshToken).HasMaxLength(500);
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
