using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateAccessToken(User user, string roleName)
    {
        var key = _config["Jwt:Key"] ?? "MonitoringDashboard-SuperSecretKey-AtLeast32Chars!";
        var issuer = _config["Jwt:Issuer"] ?? "MonitoringDashboard";
        var audience = _config["Jwt:Audience"] ?? "MonitoringDashboard";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length < 32) Array.Resize(ref keyBytes, 32);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Validates refresh token format. User is resolved by token string in DB (handler).
    /// </summary>
    public (bool isValid, int? userId) ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length < 20) return (false, null);
        return (true, null);
    }
}
