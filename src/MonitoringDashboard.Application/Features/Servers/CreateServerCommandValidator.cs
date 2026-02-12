using FluentValidation;

namespace MonitoringDashboard.Application.Features.Servers.CreateServerCommand;

public class CreateServerCommandValidator : AbstractValidator<CreateServerCommand>
{
    public CreateServerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IPAddress).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}
