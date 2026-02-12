using FluentValidation;

namespace MonitoringDashboard.Application.Features.Auth.LoginCommand;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty();
    }
}
