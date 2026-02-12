using MediatR;
using MonitoringDashboard.Domain.Exceptions;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Servers.UpdateServerCommand;

public class UpdateServerCommandHandler : IRequestHandler<UpdateServerCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public UpdateServerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateServerCommand request, CancellationToken cancellationToken)
    {
        var server = await _db.Servers.FindAsync([request.ServerId], cancellationToken);
        if (server == null) throw new NotFoundException(nameof(server), request.ServerId);
        server.Name = request.Name;
        server.IPAddress = request.IPAddress;
        server.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Status)) server.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
