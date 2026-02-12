using MediatR;

namespace MonitoringDashboard.Application.Features.Servers.GetServerQuery;

public record GetServerQuery(int ServerId) : IRequest<ServerDto?>;

public record ServerDto(int ServerId, string Name, string? IPAddress, string Status, string? Description, DateTime CreatedAt);
