using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Application.Features.Reports.GetReportQuery;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Reports.ListReportsQuery;

public class ListReportsQueryHandler : IRequestHandler<ListReportsQuery, PagedResult<ReportDto>>
{
    private readonly IApplicationDbContext _db;

    public ListReportsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ReportDto>> Handle(ListReportsQuery request, CancellationToken cancellationToken)
    {
        var q = _db.Reports.AsNoTracking();
        if (request.ServerId.HasValue) q = q.Where(r => r.ServerId == request.ServerId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(r => r.Status == request.Status);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReportDto(r.ReportId, r.ServerId, r.StartTime, r.EndTime, r.Status, r.FilePath, r.CreatedAt, r.CompletedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReportDto> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }
}
