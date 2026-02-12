namespace MonitoringDashboard.Domain.Entities;

/// <summary>
/// Role entity for role-based authorization.
/// </summary>
public class Role
{
    public int RoleId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
