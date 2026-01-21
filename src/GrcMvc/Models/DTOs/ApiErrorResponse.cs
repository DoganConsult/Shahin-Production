using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Standardized API error response for consistent error handling across all API endpoints.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Machine-readable error code for client handling.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed validation errors by field (optional).
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        /// <summary>
        /// Request trace ID for debugging (optional).
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional details for debugging (only in development).
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Create a validation error response.
        /// </summary>
        public static ApiErrorResponse Validation(string message, Dictionary<string, string[]>? errors = null)
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.ValidationFailed,
                Message = message,
                ValidationErrors = errors,
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create a not found error response.
        /// </summary>
        public static ApiErrorResponse NotFound(string resource, Guid? id = null)
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.ResourceNotFound,
                Message = id.HasValue
                    ? $"{resource} with ID '{id}' was not found."
                    : $"{resource} was not found.",
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create an unauthorized error response.
        /// </summary>
        public static ApiErrorResponse Unauthorized(string message = "You do not have permission to access this resource.")
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.Unauthorized,
                Message = message,
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create an internal error response.
        /// </summary>
        public static ApiErrorResponse InternalError(string? details = null)
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.InternalError,
                Message = "An internal error occurred. Please try again later.",
                Details = details,
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create a tenant not found error response.
        /// </summary>
        public static ApiErrorResponse TenantNotFound(Guid tenantId)
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.TenantNotFound,
                Message = $"Tenant '{tenantId}' was not found or you do not have access.",
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create a wizard not found error response.
        /// </summary>
        public static ApiErrorResponse WizardNotFound(Guid tenantId)
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.WizardNotFound,
                Message = "Onboarding wizard not found. Please start a new wizard first.",
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create a scope derivation error response.
        /// </summary>
        public static ApiErrorResponse ScopeDerivationFailed(string? details = null)
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.ScopeDerivationFailed,
                Message = "Failed to derive compliance scope. Please contact support.",
                Details = details,
                TraceId = Activity.Current?.Id
            };
        }

        /// <summary>
        /// Create a rate limit exceeded error response.
        /// </summary>
        public static ApiErrorResponse RateLimitExceeded()
        {
            return new ApiErrorResponse
            {
                ErrorCode = OnboardingErrorCodes.RateLimitExceeded,
                Message = "Too many requests. Please wait before trying again.",
                TraceId = Activity.Current?.Id
            };
        }
    }

    /// <summary>
    /// Standardized error codes for onboarding API.
    /// </summary>
    public static class OnboardingErrorCodes
    {
        // Validation errors (4xx)
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string InvalidInput = "INVALID_INPUT";
        public const string MissingRequiredField = "MISSING_REQUIRED_FIELD";

        // Resource errors (4xx)
        public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
        public const string TenantNotFound = "TENANT_NOT_FOUND";
        public const string WizardNotFound = "WIZARD_NOT_FOUND";

        // Authorization errors (4xx)
        public const string Unauthorized = "UNAUTHORIZED";
        public const string AccessDenied = "ACCESS_DENIED";
        public const string TenantAccessDenied = "TENANT_ACCESS_DENIED";

        // Business logic errors (4xx)
        public const string WizardAlreadyCompleted = "WIZARD_ALREADY_COMPLETED";
        public const string SectionNotAvailable = "SECTION_NOT_AVAILABLE";
        public const string InvalidWizardState = "INVALID_WIZARD_STATE";
        public const string ScopeDerivationFailed = "SCOPE_DERIVATION_FAILED";

        // Server errors (5xx)
        public const string InternalError = "INTERNAL_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string DatabaseError = "DATABASE_ERROR";

        // Rate limiting (429)
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    }
}
