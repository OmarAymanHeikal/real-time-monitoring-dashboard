using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Servers.ListServersQuery;

public class ListServersQueryHandler : IRequestHandler<ListServersQuery, PagedResult<ServerListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public ListServersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ServerListItemDto>> Handle(ListServersQuery request, CancellationToken cancellationToken)
    {
        var q = _db.Servers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(s => s.Name.Contains(request.Search) || (s.IPAddress != null && s.IPAddress.Contains(request.Search)));
        if (!string.IsNullOrWhiteSpace(request.Status))
            q = q.Where(s => s.Status == request.Status);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ServerListItemDto(s.ServerId, s.Name, s.IPAddress, s.Status, s.Description))
            .ToListAsync(cancellationToken);

        return new PagedResult<ServerListItemDto> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }
}
