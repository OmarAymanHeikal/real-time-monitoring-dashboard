using MediatR;
using MonitoringDashboard.Application.Common.Models;

namespace MonitoringDashboard.Application.Features.Users.ListUsersQuery;

public record ListUsersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserDto>>;

public record UserDto(int UserId, string UserName, string Email, string RoleName, DateTime CreatedAt);
