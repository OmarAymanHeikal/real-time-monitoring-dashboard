using MediatR;

namespace MonitoringDashboard.Application.Features.Users.ListRolesQuery;

public record ListRolesQuery() : IRequest<IReadOnlyList<RoleDto>>;

public record RoleDto(int RoleId, string Name, string? Description);
