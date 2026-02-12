using MediatR;
using MonitoringDashboard.Application.Common.Models;

namespace MonitoringDashboard.Application.Features.Metrics.ListMetricsQuery;

public record ListMetricsQuery(
    int ServerId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 100) : IRequest<PagedResult<MetricDto>>;

public record MetricDto(int MetricId, int ServerId, double CpuUsage, double MemoryUsage, double DiskUsage, double ResponseTime, string Status, DateTime Timestamp);
