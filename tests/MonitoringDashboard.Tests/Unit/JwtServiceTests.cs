using Xunit;
using Moq;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace MonitoringDashboard.Tests.Unit;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ValidInput_ReturnsNonEmptyToken()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Jwt:Key"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        mockConfig.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
        mockConfig.Setup(x => x["Jwt:ExpiryMinutes"]).Returns("60");

        var jwtService = new JwtService(mockConfig.Object);

        var user = new MonitoringDashboard.Domain.Entities.User
        {
            UserId = 1,
            UserName = "testuser",
            Email = "test@example.com",
            PasswordHash = "dummy-hash"
        };

        // Act
        var token = jwtService.GenerateAccessToken(user, "Admin");

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
        Assert.Contains(".", token); // JWT format contains dots
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueToken()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Jwt:Key"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        mockConfig.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");

        var jwtService = new JwtService(mockConfig.Object);

        // Act
        var token1 = jwtService.GenerateRefreshToken();
        var token2 = jwtService.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrEmpty(token1));
        Assert.False(string.IsNullOrEmpty(token2));
        Assert.NotEqual(token1, token2);
    }
}
