namespace MonitoringDashboard.Domain.Exceptions;

/// <summary>
/// Thrown when an entity is not found by id.
/// </summary>
public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object? Key { get; }

    public NotFoundException(string entityName, object? key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}
