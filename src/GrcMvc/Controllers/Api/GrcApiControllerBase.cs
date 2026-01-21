using Microsoft.AspNetCore.Mvc;
using GrcMvc.Exceptions;
using GrcMvc.Filters;
using GrcMvc.Common;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Base API controller following ASP.NET/ABP best practices.
/// Provides common functionality for all API controllers:
/// - Standardized error responses
/// - Result pattern helpers
/// - Common authorization
/// </summary>
[ApiController]
[Produces("application/json")]
[ServiceFilter(typeof(ApiExceptionFilterAttribute))]
public abstract class GrcApiControllerBase : ControllerBase
{
    /// <summary>
    /// Returns a successful response with data
    /// </summary>
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(new ApiSuccessResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a successful response without data
    /// </summary>
    protected IActionResult Success(string? message = null)
    {
        return Ok(new ApiSuccessResponse<object>
        {
            Success = true,
            Data = null,
            Message = message ?? "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a created response (201) with data and location
    /// </summary>
    protected IActionResult Created<T>(T data, string actionName, object routeValues, string? message = null)
    {
        return CreatedAtAction(actionName, routeValues, new ApiSuccessResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Resource created successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a bad request response with error details
    /// </summary>
    protected IActionResult BadRequestError(string message, string? errorCode = null)
    {
        return BadRequest(new ApiErrorResponse
        {
            Success = false,
            StatusCode = 400,
            ErrorCode = errorCode ?? GrcErrorCodes.ValidationFailed,
            Message = message,
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a not found response
    /// </summary>
    protected IActionResult NotFoundError(string message, string? errorCode = null)
    {
        return NotFound(new ApiErrorResponse
        {
            Success = false,
            StatusCode = 404,
            ErrorCode = errorCode ?? GrcErrorCodes.NotFound,
            Message = message,
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a forbidden response
    /// </summary>
    protected IActionResult ForbiddenError(string message, string? errorCode = null)
    {
        return StatusCode(403, new ApiErrorResponse
        {
            Success = false,
            StatusCode = 403,
            ErrorCode = errorCode ?? GrcErrorCodes.Unauthorized,
            Message = message,
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns response based on Result pattern
    /// </summary>
    protected IActionResult FromResult<T>(Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return Success(result.Value, successMessage);
        }
        return BadRequestError(result.Error ?? "Operation failed");
    }

    /// <summary>
    /// Gets current user ID from claims
    /// </summary>
    protected string? GetCurrentUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets current user name from claims
    /// </summary>
    protected string GetCurrentUserName()
    {
        return User.Identity?.Name ?? "Anonymous";
    }
}

/// <summary>
/// Standardized API success response
/// </summary>
public class ApiSuccessResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// API error response (imported from GlobalExceptionMiddleware for consistency)
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
