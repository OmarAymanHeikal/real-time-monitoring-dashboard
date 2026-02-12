using MediatR;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Servers.CreateServerCommand;

public class CreateServerCommandHandler : IRequestHandler<CreateServerCommand, int>
{
    private readonly IApplicationDbContext _db;

    public CreateServerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateServerCommand request, CancellationToken cancellationToken)
    {
        var server = new Server
        {
            Name = request.Name,
            IPAddress = request.IPAddress,
            Description = request.Description,
            Status = nameof(ServerStatus.Up)
        };
        _db.Servers.Add(server);
        await _db.SaveChangesAsync(cancellationToken);
        return server.ServerId;
    }
}
