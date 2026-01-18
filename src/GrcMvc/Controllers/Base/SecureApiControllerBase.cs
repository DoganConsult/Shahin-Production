using Microsoft.AspNetCore.Mvc;
using GrcMvc.Common.Exceptions;

namespace GrcMvc.Controllers.Base
{
    /// <summary>
    /// Base controller for API endpoints with secure exception handling.
    /// Uses the custom exception hierarchy instead of generic Exception catches.
    /// </summary>
    [ApiController]
    public abstract class SecureApiControllerBase : ControllerBase
    {
        protected readonly ILogger _logger;

        protected SecureApiControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes an async action with proper exception handling.
        /// Throws specific exceptions that are caught by EnhancedExceptionMiddleware.
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync<T>(
            Func<Task<T>> action,
            string actionName)
        {
            try
            {
                var result = await action();
                return Ok(result);
            }
            catch (GrcBaseException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error in {ActionName}", actionName);
                throw new ValidationException(ex.ParamName ?? "input", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in {ActionName}", actionName);
                throw new BusinessRuleException(ex.Message, "InvalidOperation");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in {ActionName}", actionName);
                throw new AuthorizationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in {ActionName}", actionName);
                throw;
            }
        }

        /// <summary>
        /// Executes an async action that returns IActionResult.
        /// </summary>
        protected async Task<IActionResult> ExecuteActionAsync(
            Func<Task<IActionResult>> action,
            string actionName)
        {
            try
            {
                return await action();
            }
            catch (GrcBaseException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error in {ActionName}", actionName);
                throw new ValidationException(ex.ParamName ?? "input", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in {ActionName}", actionName);
                throw new BusinessRuleException(ex.Message, "InvalidOperation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in {ActionName}", actionName);
                throw;
            }
        }

        /// <summary>
        /// Returns a standardized success response.
        /// </summary>
        protected IActionResult Success<T>(T data, string? message = null)
        {
            return Ok(new ApiSuccessResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            });
        }

        /// <summary>
        /// Returns a standardized created response.
        /// </summary>
        protected IActionResult Created<T>(T data, string? location = null, string? message = null)
        {
            var response = new ApiSuccessResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Resource created successfully"
            };

            if (!string.IsNullOrEmpty(location))
            {
                return Created(location, response);
            }

            return StatusCode(201, response);
        }

        /// <summary>
        /// Returns a standardized no content response.
        /// </summary>
        protected new IActionResult NoContent()
        {
            return StatusCode(204);
        }

        /// <summary>
        /// Validates that a required parameter is not null.
        /// </summary>
        protected void ValidateNotNull<T>(T? value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ValidationException(parameterName, $"{parameterName} is required");
            }
        }

        /// <summary>
        /// Validates that a GUID is not empty.
        /// </summary>
        protected void ValidateGuid(Guid value, string parameterName)
        {
            if (value == Guid.Empty)
            {
                throw new ValidationException(parameterName, $"{parameterName} is required");
            }
        }

        /// <summary>
        /// Gets the current user ID from claims.
        /// </summary>
        protected string? GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Gets the current tenant ID from claims.
        /// </summary>
        protected Guid? GetCurrentTenantId()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return tenantId;
            }
            return null;
        }

        /// <summary>
        /// Requires a tenant context or throws TenantContextRequiredException.
        /// </summary>
        protected Guid RequireTenantContext()
        {
            var tenantId = GetCurrentTenantId();
            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            {
                throw new TenantContextRequiredException();
            }
            return tenantId.Value;
        }
    }

    /// <summary>
    /// Standardized API success response.
    /// </summary>
    public class ApiSuccessResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
