using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Reports.GetReportQuery;

public class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportDto?>
{
    private readonly IApplicationDbContext _db;

    public GetReportQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ReportDto?> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var r = await _db.Reports.FindAsync([request.ReportId], cancellationToken);
        return r == null ? null : new ReportDto(r.ReportId, r.ServerId, r.StartTime, r.EndTime, r.Status, r.FilePath, r.CreatedAt, r.CompletedAt);
    }
}
