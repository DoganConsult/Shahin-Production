namespace GrcMvc.Common.Results;

/// <summary>
/// Standardized error codes used throughout the application
/// </summary>
public static class ErrorCodes
{
    // Entity-related errors
    public const string EntityNotFound = "ENTITY_NOT_FOUND";
    public const string EntityAlreadyExists = "ENTITY_ALREADY_EXISTS";
    public const string EntityDeleted = "ENTITY_DELETED";

    // Validation errors
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string InvalidInput = "INVALID_INPUT";
    public const string InvalidFormat = "INVALID_FORMAT";
    public const string RequiredFieldMissing = "REQUIRED_FIELD_MISSING";

    // State transition errors
    public const string StateTransitionInvalid = "STATE_TRANSITION_INVALID";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";

    // Configuration errors
    public const string ConfigurationMissing = "CONFIGURATION_MISSING";
    public const string ConfigurationInvalid = "CONFIGURATION_INVALID";

    // External service errors
    public const string ExternalApiFailure = "EXTERNAL_API_FAILURE";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string TimeoutError = "TIMEOUT_ERROR";

    // Authorization errors
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InsufficientPermissions = "INSUFFICIENT_PERMISSIONS";

    // Business rule violations
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string DuplicateOperation = "DUPLICATE_OPERATION";
    public const string MaxLimitReached = "MAX_LIMIT_REACHED";
    public const string ExpiredEntity = "EXPIRED_ENTITY";

    // Database errors
    public const string DatabaseError = "DATABASE_ERROR";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string TransactionFailed = "TRANSACTION_FAILED";
}
