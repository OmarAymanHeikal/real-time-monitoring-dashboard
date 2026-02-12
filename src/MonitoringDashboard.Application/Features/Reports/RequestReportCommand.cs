using MediatR;

namespace MonitoringDashboard.Application.Features.Reports.RequestReportCommand;

public record RequestReportCommand(int ServerId, DateTime StartTime, DateTime EndTime) : IRequest<int>;
