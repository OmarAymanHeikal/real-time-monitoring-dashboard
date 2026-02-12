using MediatR;

namespace MonitoringDashboard.Application.Features.Servers.DeleteServerCommand;

public record DeleteServerCommand(int ServerId) : IRequest<bool>;
