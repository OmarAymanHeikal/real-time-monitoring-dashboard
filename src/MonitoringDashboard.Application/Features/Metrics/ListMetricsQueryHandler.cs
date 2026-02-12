using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Metrics.ListMetricsQuery;

public class ListMetricsQueryHandler : IRequestHandler<ListMetricsQuery, PagedResult<MetricDto>>
{
    private readonly IApplicationDbContext _db;

    public ListMetricsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<MetricDto>> Handle(ListMetricsQuery request, CancellationToken cancellationToken)
    {
        var q = _db.Metrics.AsNoTracking().Where(m => m.ServerId == request.ServerId);
        if (request.From.HasValue) q = q.Where(m => m.Timestamp >= request.From.Value);
        if (request.To.HasValue) q = q.Where(m => m.Timestamp <= request.To.Value);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(m => m.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MetricDto(m.MetricId, m.ServerId, m.CpuUsage, m.MemoryUsage, m.DiskUsage, m.ResponseTime, m.Status, m.Timestamp))
            .ToListAsync(cancellationToken);

        return new PagedResult<MetricDto> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }
}
