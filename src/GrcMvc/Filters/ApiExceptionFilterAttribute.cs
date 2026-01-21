using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GrcMvc.Exceptions;

namespace GrcMvc.Filters;

/// <summary>
/// ASP.NET Core exception filter for API controllers.
/// Provides consistent error responses following ABP-style patterns.
/// Apply to API controllers: [ApiExceptionFilter] or globally in Program.cs
/// </summary>
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;
    private readonly IWebHostEnvironment _environment;

    public ApiExceptionFilterAttribute(
        ILogger<ApiExceptionFilterAttribute> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var correlationId = context.HttpContext.TraceIdentifier;

        // Log the exception
        _logger.LogError(
            exception,
            "API Exception - CorrelationId: {CorrelationId}, Path: {Path}, User: {User}",
            correlationId,
            context.HttpContext.Request.Path,
            context.HttpContext.User?.Identity?.Name ?? "Anonymous");

        // Build error response
        var response = BuildErrorResponse(exception, correlationId);

        context.Result = new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };

        context.ExceptionHandled = true;
    }

    private ApiErrorResult BuildErrorResponse(Exception exception, string correlationId)
    {
        var (statusCode, errorCode, message) = GetExceptionDetails(exception);

        var response = new ApiErrorResult
        {
            Success = false,
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        // Add validation errors for ValidationException
        if (exception is ValidationException validationEx)
        {
            response.Errors = validationEx.Errors;
        }

        // Include details in development only
        if (_environment.IsDevelopment())
        {
            response.Details = exception.Message;
            response.StackTrace = exception.StackTrace;
        }

        return response;
    }

    private static (int StatusCode, string? ErrorCode, string Message) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            // GRC Domain Exceptions (ABP-style)
            EntityNotFoundException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            EntityExistsException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            ValidationException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            AuthenticationException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            AuthorizationException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            TenantRequiredException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            TenantMismatchException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),
            GrcException ex => (ex.SuggestedStatusCode, ex.ErrorCode, ex.Message),

            // Standard .NET Exceptions
            UnauthorizedAccessException => (403, "FORBIDDEN", "You don't have permission to perform this action."),
            KeyNotFoundException => (404, "NOT_FOUND", "The requested resource was not found."),
            ArgumentNullException => (400, "BAD_REQUEST", "A required parameter was not provided."),
            ArgumentException => (400, "BAD_REQUEST", "Invalid input provided."),
            InvalidOperationException => (400, "INVALID_OPERATION", "This operation cannot be performed."),
            OperationCanceledException => (499, "CANCELLED", "The request was cancelled."),

            // Default
            _ => (500, "INTERNAL_ERROR", "An unexpected error occurred.")
        };
    }
}

/// <summary>
/// Standardized API error result following ABP conventions.
/// </summary>
public class ApiErrorResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
