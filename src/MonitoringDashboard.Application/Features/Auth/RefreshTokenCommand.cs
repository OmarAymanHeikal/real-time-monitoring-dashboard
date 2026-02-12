using MediatR;

namespace MonitoringDashboard.Application.Features.Auth.RefreshTokenCommand;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;

public record RefreshTokenResult(bool Success, string? AccessToken, string? NewRefreshToken, string? Error);
