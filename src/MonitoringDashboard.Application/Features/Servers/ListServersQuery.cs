using MediatR;
using MonitoringDashboard.Application.Common.Models;

namespace MonitoringDashboard.Application.Features.Servers.ListServersQuery;

public record ListServersQuery(string? Search, string? Status, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ServerListItemDto>>;

public record ServerListItemDto(int ServerId, string Name, string? IPAddress, string Status, string? Description);
