using FluentValidation.Results;

namespace MonitoringDashboard.Application.Common.Exceptions;

/// <summary>
/// Thrown when request validation fails (FluentValidation).
/// </summary>
public class RequestValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Errors { get; }

    public RequestValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures.ToList();
    }
}
