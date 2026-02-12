using MediatR;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Reports.RequestReportCommand;

public class RequestReportCommandHandler : IRequestHandler<RequestReportCommand, int>
{
    private readonly IApplicationDbContext _db;

    public RequestReportCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(RequestReportCommand request, CancellationToken cancellationToken)
    {
        var report = new Report
        {
            ServerId = request.ServerId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = "Pending"
        };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);
        return report.ReportId;
    }
}
