using MediatR;

namespace MonitoringDashboard.Application.Features.Reports.GetReportQuery;

public record GetReportQuery(int ReportId) : IRequest<ReportDto?>;

public record ReportDto(int ReportId, int ServerId, DateTime StartTime, DateTime EndTime, string Status, string? FilePath, DateTime CreatedAt, DateTime? CompletedAt);
