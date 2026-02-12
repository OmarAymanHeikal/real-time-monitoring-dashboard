using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Users.ListUsersQuery;

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, PagedResult<UserDto>>
{
    private readonly IApplicationDbContext _db;

    public ListUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<UserDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var q = _db.Users.AsNoTracking().Include(u => u.Role);
        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderBy(u => u.UserName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto(u.UserId, u.UserName, u.Email, u.Role.Name, u.CreatedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<UserDto> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }
}
