namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// User entity for authentication and authorization.
/// </summary>
public class User
{
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = null!;
    
    // Many-to-many navigation
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
