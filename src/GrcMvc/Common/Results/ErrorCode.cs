namespace GrcMvc.Common.Results
{
    /// <summary>
    /// Standard error codes used throughout the application
    /// </summary>
    public static class ErrorCode
    {
        /// <summary>
        /// Entity or resource not found
        /// </summary>
        public const string NotFound = "NOT_FOUND";

        /// <summary>
        /// Validation error (invalid input, business rule violation)
        /// </summary>
        public const string ValidationError = "VALIDATION_ERROR";

        /// <summary>
        /// Unauthorized access (authentication required)
        /// </summary>
        public const string Unauthorized = "UNAUTHORIZED";

        /// <summary>
        /// Forbidden access (insufficient permissions)
        /// </summary>
        public const string Forbidden = "FORBIDDEN";

        /// <summary>
        /// Conflict (duplicate entry, concurrent modification)
        /// </summary>
        public const string Conflict = "CONFLICT";

        /// <summary>
        /// Invalid operation (operation not allowed in current state)
        /// </summary>
        public const string InvalidOperation = "INVALID_OPERATION";

        /// <summary>
        /// Internal server error
        /// </summary>
        public const string InternalError = "INTERNAL_ERROR";

        /// <summary>
        /// Database error
        /// </summary>
        public const string DatabaseError = "DATABASE_ERROR";

        /// <summary>
        /// External service error
        /// </summary>
        public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";

        /// <summary>
        /// Configuration error
        /// </summary>
        public const string ConfigurationError = "CONFIGURATION_ERROR";

        /// <summary>
        /// Timeout error
        /// </summary>
        public const string Timeout = "TIMEOUT";

        /// <summary>
        /// Rate limit exceeded
        /// </summary>
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";

        /// <summary>
        /// Bad request (malformed request)
        /// </summary>
        public const string BadRequest = "BAD_REQUEST";
    }
}
