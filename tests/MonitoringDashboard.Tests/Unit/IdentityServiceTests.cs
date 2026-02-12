using Xunit;
using MonitoringDashboard.Infrastructure.Services;

namespace MonitoringDashboard.Tests.Unit;

public class IdentityServiceTests
{
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        _identityService = new IdentityService();
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "TestPassword@123";

        // Act
        var hash = _identityService.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword@123";
        var hash = _identityService.HashPassword(password);

        // Act
        var result = _identityService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword@123";
        var wrongPassword = "WrongPassword@123";
        var hash = _identityService.HashPassword(password);

        // Act
        var result = _identityService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_SamePassword_GeneratesDifferentHashes()
    {
        // Arrange
        var password = "TestPassword@123";

        // Act
        var hash1 = _identityService.HashPassword(password);
        var hash2 = _identityService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // BCrypt generates different hashes with different salts
    }
}
