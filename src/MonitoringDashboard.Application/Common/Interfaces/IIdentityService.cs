namespace MonitoringDashboard.Application.Common.Interfaces;

/// <summary>
/// Abstraction for password hashing and verification (Infrastructure implements).
/// </summary>
public interface IIdentityService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
