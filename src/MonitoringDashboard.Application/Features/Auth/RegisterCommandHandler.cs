using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Auth.RegisterCommand;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identity;

    public RegisterCommandHandler(IApplicationDbContext db, IIdentityService identity)
    {
        _db = db;
        _identity = identity;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
            return new RegisterResult(false, null, "Username already exists.");
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return new RegisterResult(false, null, "Email already registered.");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _identity.HashPassword(request.Password),
            RoleId = request.RoleId
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return new RegisterResult(true, user.UserId, null);
    }
}
