using MediatR;

namespace MonitoringDashboard.Application.Features.Servers.CreateServerCommand;

public record CreateServerCommand(string Name, string? IPAddress, string? Description) : IRequest<int>;
