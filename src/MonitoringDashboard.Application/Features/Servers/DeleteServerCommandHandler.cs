using MediatR;
using MonitoringDashboard.Domain.Exceptions;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Servers.DeleteServerCommand;

public class DeleteServerCommandHandler : IRequestHandler<DeleteServerCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public DeleteServerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteServerCommand request, CancellationToken cancellationToken)
    {
        var server = await _db.Servers.FindAsync([request.ServerId], cancellationToken);
        if (server == null) throw new NotFoundException("Server", request.ServerId);
        _db.Servers.Remove(server);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
