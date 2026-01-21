using System.Diagnostics;

namespace GrcMvc.Middleware;

/// <summary>
/// Middleware to log all HTTP requests and responses with timing information
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var userIdentity = context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            await _next(context);
            sw.Stop();

            var statusCode = context.Response.StatusCode;
            var logLevel = statusCode >= 500 ? LogLevel.Error :
                          statusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms - User: {User}",
                requestMethod,
                requestPath,
                statusCode,
                sw.ElapsedMilliseconds,
                userIdentity);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "HTTP {Method} {Path} failed after {ElapsedMs}ms - User: {User}",
                requestMethod,
                requestPath,
                sw.ElapsedMilliseconds,
                userIdentity);
            throw;
        }
    }
}

/// <summary>
/// Extension methods for RequestLoggingMiddleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
