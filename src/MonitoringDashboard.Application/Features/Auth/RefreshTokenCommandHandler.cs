using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Auth.RefreshTokenCommand;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(IApplicationDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (isValid, _) = _jwt.ValidateRefreshToken(request.RefreshToken);
        if (!isValid)
            return new RefreshTokenResult(false, null, null, "Invalid or expired refresh token.");

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return new RefreshTokenResult(false, null, null, "Invalid or expired refresh token.");

        var accessToken = _jwt.GenerateAccessToken(user, user.Role.Name);
        var newRefresh = _jwt.GenerateRefreshToken();
        if (!string.IsNullOrEmpty(newRefresh))
        {
            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync(cancellationToken);
        }
        return new RefreshTokenResult(true, accessToken, user.RefreshToken, null);
    }
}
