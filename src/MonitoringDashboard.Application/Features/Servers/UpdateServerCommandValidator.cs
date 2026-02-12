using FluentValidation;

namespace MonitoringDashboard.Application.Features.Servers.UpdateServerCommand;

public class UpdateServerCommandValidator : AbstractValidator<UpdateServerCommand>
{
    public UpdateServerCommandValidator()
    {
        RuleFor(x => x.ServerId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IPAddress).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}
