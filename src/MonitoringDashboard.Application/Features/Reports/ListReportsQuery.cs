using MediatR;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Application.Features.Reports.GetReportQuery;

namespace MonitoringDashboard.Application.Features.Reports.ListReportsQuery;

public record ListReportsQuery(int? ServerId, string? Status, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReportDto>>;
