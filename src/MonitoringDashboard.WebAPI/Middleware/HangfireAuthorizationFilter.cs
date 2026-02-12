using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MonitoringDashboard.WebAPI.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _configuration;

    public HangfireAuthorizationFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow in development environment
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            return true;
        }

        // Check for JWT token in Authorization header
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (ValidateToken(token, out var claims))
            {
                // Check if user has Admin role
                return claims?.FindFirst(ClaimTypes.Role)?.Value == "Admin";
            }
        }

        // Check for token in query string (for direct browser access)
        var tokenFromQuery = httpContext.Request.Query["access_token"].ToString();
        if (!string.IsNullOrEmpty(tokenFromQuery))
        {
            if (ValidateToken(tokenFromQuery, out var claims))
            {
                return claims?.FindFirst(ClaimTypes.Role)?.Value == "Admin";
            }
        }

        return false;
    }

    private bool ValidateToken(string token, out ClaimsPrincipal? claims)
    {
        claims = null;
        try
        {
            var key = _configuration["Jwt:Key"] ?? "MonitoringDashboard-SuperSecretKey-AtLeast32Chars!";
            var keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length < 32) Array.Resize(ref keyBytes, 32);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "MonitoringDashboard",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "MonitoringDashboard",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            claims = tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
