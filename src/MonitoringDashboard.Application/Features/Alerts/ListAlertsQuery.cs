using MediatR;
using MonitoringDashboard.Application.Common.Models;

namespace MonitoringDashboard.Application.Features.Alerts.ListAlertsQuery;

public record ListAlertsQuery(int? ServerId, string? Status, int Page = 1, int PageSize = 50) : IRequest<PagedResult<AlertDto>>;

public record AlertDto(int AlertId, int ServerId, string MetricType, double MetricValue, double Threshold, string Status, DateTime CreatedAt, DateTime? ResolvedAt);
