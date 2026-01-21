using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models;

namespace GrcMvc.Controllers;

/// <summary>
/// Base controller for API endpoints with secure error handling.
/// SECURITY: All API controllers should inherit from this to prevent exception disclosure.
/// </summary>
[ApiController]
public abstract class SecureApiControllerBase : ControllerBase
{
    private readonly IWebHostEnvironment? _environment;
    private readonly ILogger? _logger;

    protected SecureApiControllerBase() { }

    protected SecureApiControllerBase(IWebHostEnvironment environment, ILogger logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Returns a safe error response that doesn't expose internal exception details in production.
    /// Use this instead of: return BadRequest(new { error = ex.Message })
    /// </summary>
    protected IActionResult SafeBadRequest(Exception ex, string? userFriendlyMessage = null)
    {
        LogException(ex);
        var message = GetSafeMessage(ex, userFriendlyMessage);
        return BadRequest(new { success = false, error = message });
    }

    /// <summary>
    /// Returns a safe error response using ApiResponse wrapper.
    /// Use this instead of: return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message))
    /// </summary>
    protected IActionResult SafeApiError<T>(Exception ex, string? userFriendlyMessage = null)
    {
        LogException(ex);
        var message = GetSafeMessage(ex, userFriendlyMessage);
        return BadRequest(ApiResponse<T>.ErrorResponse(message));
    }

    /// <summary>
    /// Returns a safe 500 error response.
    /// Use this instead of: return StatusCode(500, new { error = ex.Message })
    /// </summary>
    protected IActionResult SafeServerError(Exception ex, string? userFriendlyMessage = null)
    {
        LogException(ex);
        var message = GetSafeMessage(ex, userFriendlyMessage ?? "An internal error occurred. Please try again later.");
        return StatusCode(500, new { success = false, error = message });
    }

    /// <summary>
    /// Returns a safe not found response.
    /// </summary>
    protected IActionResult SafeNotFound(Exception ex, string? userFriendlyMessage = null)
    {
        LogException(ex);
        var message = GetSafeMessage(ex, userFriendlyMessage ?? "The requested resource was not found.");
        return NotFound(new { success = false, error = message });
    }

    /// <summary>
    /// Gets a safe message based on environment.
    /// In development: returns actual exception message for debugging.
    /// In production: returns generic user-friendly message.
    /// </summary>
    private string GetSafeMessage(Exception ex, string? userFriendlyMessage)
    {
        var isDevelopment = _environment?.IsDevelopment() ?? false;
        
        if (isDevelopment)
        {
            return ex.Message;
        }

        return userFriendlyMessage ?? "An error occurred processing your request.";
    }

    /// <summary>
    /// Logs the exception with correlation ID for troubleshooting.
    /// </summary>
    private void LogException(Exception ex)
    {
        var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
        _logger?.LogError(ex, "Error occurred. CorrelationId: {CorrelationId}", correlationId);
    }
}

/// <summary>
/// Static helper for controllers that can't inherit from SecureApiControllerBase.
/// </summary>
public static class SecureErrorHelper
{
    /// <summary>
    /// Gets a safe error message based on environment.
    /// </summary>
    public static string GetSafeMessage(Exception ex, IWebHostEnvironment? environment, string? userFriendlyMessage = null)
    {
        var isDevelopment = environment?.IsDevelopment() ?? false;
        
        if (isDevelopment)
        {
            return ex.Message;
        }

        return userFriendlyMessage ?? "An error occurred processing your request.";
    }

    /// <summary>
    /// Creates a safe error object for JSON responses.
    /// </summary>
    public static object CreateSafeError(Exception ex, IWebHostEnvironment? environment, string? userFriendlyMessage = null)
    {
        return new { success = false, error = GetSafeMessage(ex, environment, userFriendlyMessage) };
    }
}
