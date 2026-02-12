using Moq;
using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Application.Features.Auth.LoginCommand;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;
using Xunit;

namespace MonitoringDashboard.Tests.Unit;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_InvalidCredentials_ReturnsFailure()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Login_Invalid")
            .Options;
        await using var db = new ApplicationDbContext(options);
        db.Roles.Add(new Role { RoleId = 1, Name = "User" });
        db.Users.Add(new User { UserId = 1, UserName = "u", Email = "u@t.com", PasswordHash = "x", RoleId = 1 });
        await db.SaveChangesAsync();

        var identity = new Mock<IIdentityService>();
        identity.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var jwt = new Mock<IJwtService>();

        var handler = new LoginCommandHandler(db, identity.Object, jwt.Object);
        var result = await handler.Handle(new LoginCommand("u", "wrong"), default);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }
}
