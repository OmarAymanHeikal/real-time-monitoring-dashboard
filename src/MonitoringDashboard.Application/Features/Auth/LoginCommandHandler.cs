using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Auth.LoginCommand;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(IApplicationDbContext db, IIdentityService identity, IJwtService jwt)
    {
        _db = db;
        _identity = identity;
        _jwt = jwt;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

        if (user == null || !_identity.VerifyPassword(request.Password, user.PasswordHash))
            return new LoginResult(false, null, null, null, "Invalid credentials.");

        var accessToken = _jwt.GenerateAccessToken(user, user.Role.Name);
        var refreshToken = _jwt.GenerateRefreshToken();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new LoginResult(true, accessToken, refreshToken, user.Role.Name, null);
    }
}
