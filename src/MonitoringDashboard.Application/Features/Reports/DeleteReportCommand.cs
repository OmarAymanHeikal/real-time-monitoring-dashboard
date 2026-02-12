using MediatR;

namespace MonitoringDashboard.Application.Features.Reports.DeleteReportCommand;

public record DeleteReportCommand(int ReportId) : IRequest<bool>;
