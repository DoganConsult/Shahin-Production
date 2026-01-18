using System.Net;
using System.Text.Json;
using GrcMvc.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Enhanced global exception middleware that handles all exception types
    /// with proper HTTP status codes and structured error responses.
    /// </summary>
    public class EnhancedExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnhancedExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public EnhancedExceptionMiddleware(
            RequestDelegate next,
            ILogger<EnhancedExceptionMiddleware> logger,
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
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(exception, context);
            response.StatusCode = errorResponse.StatusCode;

            LogException(exception, context, errorResponse);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }

        private ErrorResponse CreateErrorResponse(Exception exception, HttpContext context)
        {
            var traceId = context.TraceIdentifier;
            var includeDetails = _environment.IsDevelopment();

            return exception switch
            {
                // Security Exceptions - 401/403
                AuthenticationException authEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    ErrorCode = authEx.ErrorCode,
                    Message = "Authentication failed",
                    Details = includeDetails ? authEx.Message : null,
                    TraceId = traceId
                },

                AuthorizationException authzEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    ErrorCode = authzEx.ErrorCode,
                    Message = "Access denied",
                    Details = includeDetails ? $"Required permission: {authzEx.RequiredPermission}" : null,
                    TraceId = traceId
                },

                TokenException tokenEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    ErrorCode = tokenEx.ErrorCode,
                    Message = "Invalid or expired token",
                    Details = includeDetails ? tokenEx.Message : null,
                    TraceId = traceId
                },

                // Data Exceptions - 404/409
                EntityNotFoundException notFoundEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ErrorCode = notFoundEx.ErrorCode,
                    Message = notFoundEx.Message,
                    TraceId = traceId
                },

                DuplicateEntityException duplicateEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ErrorCode = duplicateEx.ErrorCode,
                    Message = duplicateEx.Message,
                    TraceId = traceId
                },

                DataIntegrityException dataEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ErrorCode = dataEx.ErrorCode,
                    Message = "Data integrity violation",
                    Details = includeDetails ? dataEx.Message : null,
                    TraceId = traceId
                },

                // Validation Exceptions - 400
                ValidationException validationEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = validationEx.ErrorCode,
                    Message = "Validation failed",
                    Details = validationEx.Message,
                    ValidationErrors = validationEx.Errors,
                    TraceId = traceId
                },

                BusinessRuleException businessEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = businessEx.ErrorCode,
                    Message = businessEx.Message,
                    Details = includeDetails ? $"Rule: {businessEx.RuleName}" : null,
                    TraceId = traceId
                },

                // Tenant Exceptions - 400/404
                TenantNotFoundException tenantNotFoundEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ErrorCode = "TENANT_NOT_FOUND",
                    Message = "Tenant not found",
                    TraceId = traceId
                },

                TenantContextRequiredException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "TENANT_CONTEXT_REQUIRED",
                    Message = "Tenant context is required for this operation",
                    TraceId = traceId
                },

                TenantException tenantEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = tenantEx.ErrorCode,
                    Message = tenantEx.Message,
                    TraceId = traceId
                },

                // Integration Exceptions - 502/503
                ExternalServiceException serviceEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    ErrorCode = serviceEx.ErrorCode,
                    Message = $"External service error: {serviceEx.ServiceName}",
                    Details = includeDetails ? serviceEx.Message : null,
                    TraceId = traceId
                },

                // Workflow Exceptions - 400/409
                WorkflowStateException stateEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ErrorCode = "WORKFLOW_STATE_INVALID",
                    Message = stateEx.Message,
                    TraceId = traceId
                },

                WorkflowException workflowEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = workflowEx.ErrorCode,
                    Message = workflowEx.Message,
                    TraceId = traceId
                },

                // Configuration Exceptions - 500
                ConfigurationException configEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ErrorCode = configEx.ErrorCode,
                    Message = "Configuration error",
                    Details = includeDetails ? configEx.Message : null,
                    TraceId = traceId
                },

                // Rate Limiting - 429
                RateLimitExceededException rateLimitEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.TooManyRequests,
                    ErrorCode = rateLimitEx.ErrorCode,
                    Message = "Rate limit exceeded",
                    Details = rateLimitEx.RetryAfter.HasValue 
                        ? $"Retry after {rateLimitEx.RetryAfter.Value.TotalSeconds} seconds" 
                        : null,
                    TraceId = traceId
                },

                // File Exceptions - 400/415
                InvalidFileTypeException fileTypeEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                    ErrorCode = "INVALID_FILE_TYPE",
                    Message = fileTypeEx.Message,
                    TraceId = traceId
                },

                FileOperationException fileEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = fileEx.ErrorCode,
                    Message = fileEx.Message,
                    TraceId = traceId
                },

                // Base GRC Exception
                GrcBaseException grcEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = grcEx.ErrorCode,
                    Message = grcEx.Message,
                    TraceId = traceId
                },

                // Standard .NET Exceptions
                ArgumentNullException argNullEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "INVALID_ARGUMENT",
                    Message = $"Required parameter missing: {argNullEx.ParamName}",
                    TraceId = traceId
                },

                ArgumentException argEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "INVALID_ARGUMENT",
                    Message = includeDetails ? argEx.Message : "Invalid argument provided",
                    TraceId = traceId
                },

                InvalidOperationException invalidOpEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "INVALID_OPERATION",
                    Message = includeDetails ? invalidOpEx.Message : "Operation not valid in current state",
                    TraceId = traceId
                },

                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    ErrorCode = "ACCESS_DENIED",
                    Message = "Access denied",
                    TraceId = traceId
                },

                NotImplementedException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotImplemented,
                    ErrorCode = "NOT_IMPLEMENTED",
                    Message = "This feature is not yet implemented",
                    TraceId = traceId
                },

                OperationCanceledException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "OPERATION_CANCELLED",
                    Message = "The operation was cancelled",
                    TraceId = traceId
                },

                TimeoutException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.GatewayTimeout,
                    ErrorCode = "TIMEOUT",
                    Message = "The operation timed out",
                    TraceId = traceId
                },

                // Default - Internal Server Error
                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ErrorCode = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred",
                    Details = includeDetails ? exception.Message : null,
                    TraceId = traceId
                }
            };
        }

        private void LogException(Exception exception, HttpContext context, ErrorResponse errorResponse)
        {
            var logLevel = errorResponse.StatusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 and < 500 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            _logger.Log(logLevel, exception,
                "HTTP {Method} {Path} responded {StatusCode} ({ErrorCode}) for user {UserId}. TraceId: {TraceId}",
                requestMethod, requestPath, errorResponse.StatusCode, errorResponse.ErrorCode, userId, errorResponse.TraceId);
        }
    }

    /// <summary>
    /// Structured error response for API errors
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; } = "UNKNOWN_ERROR";
        public string Message { get; set; } = "An error occurred";
        public string? Details { get; set; }
        public string? TraceId { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Extension methods for registering the enhanced exception middleware
    /// </summary>
    public static class EnhancedExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseEnhancedExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<EnhancedExceptionMiddleware>();
        }
    }
}
