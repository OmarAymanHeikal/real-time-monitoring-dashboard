using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Servers.GetServerQuery;

public class GetServerQueryHandler : IRequestHandler<GetServerQuery, ServerDto?>
{
    private readonly IApplicationDbContext _db;

    public GetServerQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ServerDto?> Handle(GetServerQuery request, CancellationToken cancellationToken)
    {
        var s = await _db.Servers.FindAsync([request.ServerId], cancellationToken);
        return s == null ? null : new ServerDto(s.ServerId, s.Name, s.IPAddress, s.Status, s.Description, s.CreatedAt);
    }
}
