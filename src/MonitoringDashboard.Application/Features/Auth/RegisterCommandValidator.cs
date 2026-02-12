using FluentValidation;

namespace MonitoringDashboard.Application.Features.Auth.RegisterCommand;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.RoleId).GreaterThan(0);
    }
}
