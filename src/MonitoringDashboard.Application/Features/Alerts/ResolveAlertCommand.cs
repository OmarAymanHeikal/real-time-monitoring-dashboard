using MediatR;

namespace MonitoringDashboard.Application.Features.Alerts.ResolveAlertCommand;

public record ResolveAlertCommand(int AlertId) : IRequest<bool>;
