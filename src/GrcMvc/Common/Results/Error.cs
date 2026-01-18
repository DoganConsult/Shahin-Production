namespace GrcMvc.Common.Results
{
    /// <summary>
    /// Represents an error that occurred during an operation
    /// </summary>
    public class Error
    {
        /// <summary>
        /// The error code (e.g., "NOT_FOUND", "VALIDATION_ERROR")
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// A human-readable error message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Additional details about the error (optional)
        /// </summary>
        public string? Details { get; }

        /// <summary>
        /// Creates a new error
        /// </summary>
        public Error(string code, string message, string? details = null)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Details = details;
        }

        /// <summary>
        /// Returns a string representation of the error
        /// </summary>
        public override string ToString()
        {
            return Details != null 
                ? $"[{Code}] {Message} - {Details}" 
                : $"[{Code}] {Message}";
        }

        /// <summary>
        /// Predefined error: Entity not found
        /// </summary>
        public static Error NotFound(string entityName, object id) 
            => new Error(
                ErrorCode.NotFound, 
                $"{entityName} not found",
                $"No {entityName} found with ID: {id}");

        /// <summary>
        /// Predefined error: Validation failed
        /// </summary>
        public static Error Validation(string message, string? details = null) 
            => new Error(ErrorCode.ValidationError, message, details);

        /// <summary>
        /// Predefined error: Unauthorized access
        /// </summary>
        public static Error Unauthorized(string message = "Unauthorized access") 
            => new Error(ErrorCode.Unauthorized, message);

        /// <summary>
        /// Predefined error: Forbidden access
        /// </summary>
        public static Error Forbidden(string message = "Access forbidden") 
            => new Error(ErrorCode.Forbidden, message);

        /// <summary>
        /// Predefined error: Conflict (e.g., duplicate entry)
        /// </summary>
        public static Error Conflict(string message, string? details = null) 
            => new Error(ErrorCode.Conflict, message, details);

        /// <summary>
        /// Predefined error: Invalid operation
        /// </summary>
        public static Error InvalidOperation(string message, string? details = null) 
            => new Error(ErrorCode.InvalidOperation, message, details);

        /// <summary>
        /// Predefined error: Internal server error
        /// </summary>
        public static Error Internal(string message = "An internal error occurred", string? details = null) 
            => new Error(ErrorCode.InternalError, message, details);
    }
}
