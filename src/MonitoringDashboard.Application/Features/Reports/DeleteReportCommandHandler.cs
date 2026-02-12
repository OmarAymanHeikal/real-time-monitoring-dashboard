using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Reports.DeleteReportCommand;

public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public DeleteReportCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _db.Reports.FirstOrDefaultAsync(r => r.ReportId == request.ReportId, cancellationToken);
        if (report == null) return false;

        // Delete the physical file if it exists
        if (!string.IsNullOrEmpty(report.FilePath) && System.IO.File.Exists(report.FilePath))
        {
            try
            {
                System.IO.File.Delete(report.FilePath);
            }
            catch
            {
                // Ignore file deletion errors
            }
        }

        _db.Reports.Remove(report);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
