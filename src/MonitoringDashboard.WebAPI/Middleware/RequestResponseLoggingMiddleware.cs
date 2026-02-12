using System.Diagnostics;
using System.Text;
using Microsoft.IO;

namespace MonitoringDashboard.WebAPI.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        // Log request
        await LogRequestAsync(context, correlationId);

        // Capture response
        var originalBodyStream = context.Response.Body;
        await using var responseBody = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed. CorrelationId: {CorrelationId}, Duration: {Duration}ms",
                correlationId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        context.Request.EnableBuffering();

        var requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Mask sensitive data
            requestBody = MaskSensitiveData(requestBody);
        }

        var userId = context.User?.Identity?.Name ?? "Anonymous";

        _logger.LogInformation(
            "HTTP Request: {Method} {Path} | UserId: {UserId} | CorrelationId: {CorrelationId} | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            userId,
            correlationId,
            requestBody);
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "HTTP Response: {StatusCode} | CorrelationId: {CorrelationId} | Duration: {Duration}ms | Body: {Body}",
            context.Response.StatusCode,
            correlationId,
            elapsedMilliseconds,
            responseBody.Length > 1000 ? $"{responseBody[..1000]}..." : responseBody);
    }

    private static string MaskSensitiveData(string input)
    {
        // Mask passwords and tokens
        var patterns = new[] { "password", "token", "secret", "apiKey" };
        
        foreach (var pattern in patterns)
        {
            if (input.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                // Simple masking - in production, use proper JSON parsing
                input = System.Text.RegularExpressions.Regex.Replace(
                    input,
                    $@"""{pattern}"":\s*""([^""]+)""",
                    $@"""{pattern}"": ""***MASKED***""",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }

        return input;
    }
}
