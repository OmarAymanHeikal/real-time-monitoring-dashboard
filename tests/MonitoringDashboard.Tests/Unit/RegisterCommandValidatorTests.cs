using Xunit;
using FluentValidation.TestHelper;
using MonitoringDashboard.Application.Features.Auth.RegisterCommand;

namespace MonitoringDashboard.Tests.Unit;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "Password@123", 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserName_FailsValidation()
    {
        // Arrange
        var command = new RegisterCommand("", "test@example.com", "Password@123", 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_InvalidEmail_FailsValidation()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "invalid-email", "Password@123", 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ShortPassword_FailsValidation()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "short", 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
