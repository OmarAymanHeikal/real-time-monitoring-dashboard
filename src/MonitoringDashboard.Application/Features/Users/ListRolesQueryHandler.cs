using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Users.ListRolesQuery;

public class ListRolesQueryHandler : IRequestHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IApplicationDbContext _db;

    public ListRolesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<RoleDto>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(r.RoleId, r.Name, r.Description))
            .ToListAsync(cancellationToken);
    }
}
