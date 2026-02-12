namespace MonitoringDashboard.Domain.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Many-to-many navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
