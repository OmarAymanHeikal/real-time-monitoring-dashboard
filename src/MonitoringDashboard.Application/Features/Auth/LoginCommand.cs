using MediatR;

namespace MonitoringDashboard.Application.Features.Auth.LoginCommand;

public record LoginCommand(string UserName, string Password) : IRequest<LoginResult>;

public record LoginResult(bool Success, string? AccessToken, string? RefreshToken, string? Role, string? Error);
