using System.Net;
using System.Text.Json;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Domain.Exceptions;

namespace MonitoringDashboard.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private static async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title, errors) = ex switch
        {
            RequestValidationException ve => (HttpStatusCode.BadRequest, "Validation failed", (object?)ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList()),
            NotFoundException => (HttpStatusCode.NotFound, "Not found", (object?)new { message = ex.Message }),
            _ => (HttpStatusCode.InternalServerError, "Internal server error", (object?)new { message = "An error occurred." })
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { title, errors });
        await context.Response.WriteAsync(body);
    }
}
