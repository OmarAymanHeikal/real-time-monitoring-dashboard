using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Alerts.ListAlertsQuery;

public class ListAlertsQueryHandler : IRequestHandler<ListAlertsQuery, PagedResult<AlertDto>>
{
    private readonly IApplicationDbContext _db;

    public ListAlertsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AlertDto>> Handle(ListAlertsQuery request, CancellationToken cancellationToken)
    {
        var q = _db.Alerts.AsNoTracking();
        if (request.ServerId.HasValue) q = q.Where(a => a.ServerId == request.ServerId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(a => a.Status == request.Status);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AlertDto(a.AlertId, a.ServerId, a.MetricType, a.MetricValue, a.Threshold, a.Status, a.CreatedAt, a.ResolvedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AlertDto> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }
}
