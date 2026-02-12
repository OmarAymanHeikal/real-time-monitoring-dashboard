using MediatR;

namespace MonitoringDashboard.Application.Features.Auth.RegisterCommand;

public record RegisterCommand(string UserName, string Email, string Password, int RoleId = 2) : IRequest<RegisterResult>;

public record RegisterResult(bool Success, int? UserId, string? Error);
