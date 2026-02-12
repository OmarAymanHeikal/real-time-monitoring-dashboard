using MediatR;

namespace MonitoringDashboard.Application.Features.Servers.UpdateServerCommand;

public record UpdateServerCommand(int ServerId, string Name, string? IPAddress, string? Description, string? Status) : IRequest<bool>;
