using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Application.Common.Interfaces;

/// <summary>
/// JWT token generation and validation (Infrastructure implements).
/// </summary>
public interface IJwtService
{
    string GenerateAccessToken(User user, string roleName);
    string? GenerateRefreshToken();
    (bool isValid, int? userId) ValidateRefreshToken(string refreshToken);
}
