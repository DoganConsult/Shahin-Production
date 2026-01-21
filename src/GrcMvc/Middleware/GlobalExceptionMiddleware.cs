using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Application.Policy;
using GrcMvc.Exceptions;

namespace GrcMvc.Middleware;

/// <summary>
/// Global exception handling middleware that provides user-friendly error responses
/// and logs all exceptions with correlation IDs for support tracing.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle status code errors (404, 403, etc.)
            if (!context.Response.HasStarted && context.Response.StatusCode >= 400)
            {
                await HandleStatusCodeAsync(context);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleStatusCodeAsync(HttpContext context)
    {
        var statusCode = context.Response.StatusCode;
        var correlationId = context.TraceIdentifier;

        // Skip if response body already written or if it's an API call
        if (context.Response.HasStarted || IsApiRequest(context))
        {
            return;
        }

        // Log the status code
        _logger.LogWarning(
            "HTTP {StatusCode} for {Method} {Path} - CorrelationId: {CorrelationId}",
            statusCode, context.Request.Method, context.Request.Path, correlationId);

        // For MVC requests, redirect to error page
        if (ShouldShowErrorPage(context))
        {
            context.Response.Redirect($"/Home/Error?statusCode={statusCode}");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        // Log the exception with full details
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, User: {User}",
            correlationId,
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous");

        // Don't modify response if already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot modify for exception handling");
            return;
        }

        context.Response.Clear();

        if (IsApiRequest(context))
        {
            await HandleApiExceptionAsync(context, exception, correlationId);
        }
        else
        {
            await HandleMvcExceptionAsync(context, exception, correlationId);
        }
    }

    private async Task HandleApiExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        var statusCode = GetStatusCode(exception);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Success = false,
            StatusCode = statusCode,
            ErrorCode = GetErrorCode(exception),
            Message = GetUserFriendlyMessage(exception),
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        // Add validation errors if present
        if (exception is Exceptions.ValidationException validationEx)
        {
            response.ValidationErrors = validationEx.Errors;
        }

        // Include exception details only in development
        if (_environment.IsDevelopment())
        {
            response.Details = exception.Message;
            response.StackTrace = exception.StackTrace;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private async Task HandleMvcExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        var statusCode = GetStatusCode(exception);
        context.Response.StatusCode = statusCode;

        // Store error info for the error page
        context.Items["StatusCode"] = statusCode;
        context.Items["CorrelationId"] = correlationId;
        context.Items["ErrorMessage"] = GetUserFriendlyMessage(exception);

        if (_environment.IsDevelopment())
        {
            context.Items["ErrorDetails"] = exception.Message;
        }

        // Redirect to error page
        context.Response.Redirect($"/Home/Error?statusCode={statusCode}&correlationId={correlationId}");
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            // GRC domain exceptions (ABP-style with suggested status codes)
            GrcException grcEx => grcEx.SuggestedStatusCode,
            // Standard .NET exceptions
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            PolicyViolationException => (int)HttpStatusCode.BadRequest,
            OperationCanceledException => 499, // Client closed request
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            // GRC domain exceptions - use their message directly (already user-friendly)
            GrcException grcEx => grcEx.Message,
            // Standard .NET exceptions
            UnauthorizedAccessException => "You don't have permission to perform this action.",
            KeyNotFoundException => "The requested resource was not found.",
            ArgumentException => "Invalid input provided. Please check your data and try again.",
            InvalidOperationException => "This operation cannot be performed at this time.",
            PolicyViolationException pve => pve.Message,
            OperationCanceledException => "The request was cancelled.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }

    private static string? GetErrorCode(Exception exception)
    {
        return exception switch
        {
            GrcException grcEx => grcEx.ErrorCode,
            _ => null
        };
    }

    private static bool IsApiRequest(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        return path.StartsWith("/api/") ||
               context.Request.Headers["Accept"].ToString().Contains("application/json") ||
               context.Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
    }

    private static bool ShouldShowErrorPage(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Don't show error page for static files, API calls, or already on error page
        return !path.StartsWith("/api/") &&
               !path.StartsWith("/_") &&
               !path.Contains("/error") &&
               !path.EndsWith(".js") &&
               !path.EndsWith(".css") &&
               !path.EndsWith(".png") &&
               !path.EndsWith(".jpg") &&
               !path.EndsWith(".svg") &&
               !path.EndsWith(".ico");
    }
}

/// <summary>
/// Standardized API error response
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}

/// <summary>
/// Extension methods for GlobalExceptionMiddleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
