using MediatR;
using MonitoringDashboard.Domain.Exceptions;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Alerts.ResolveAlertCommand;

public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public ResolveAlertCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _db.Alerts.FindAsync([request.AlertId], cancellationToken);
        if (alert == null) throw new NotFoundException("Alert", request.AlertId);
        alert.Status = "Resolved";
        alert.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
