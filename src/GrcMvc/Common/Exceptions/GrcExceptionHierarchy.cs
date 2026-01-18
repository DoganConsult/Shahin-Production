using System;

namespace GrcMvc.Common.Exceptions
{
    /// <summary>
    /// Base exception for all GRC system exceptions.
    /// Provides structured error handling with error codes and context.
    /// </summary>
    public class GrcBaseException : Exception
    {
        public string ErrorCode { get; }
        public string? Context { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public GrcBaseException(string message, string errorCode = "GRC_ERROR") 
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public GrcBaseException(string message, string errorCode, string? context) 
            : base(message)
        {
            ErrorCode = errorCode;
            Context = context;
        }

        public GrcBaseException(string message, string errorCode, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    #region Security Exceptions

    /// <summary>
    /// Exception for authentication failures
    /// </summary>
    public class AuthenticationException : GrcBaseException
    {
        public string? UserId { get; }
        public string? AttemptedAction { get; }

        public AuthenticationException(string message, string? userId = null) 
            : base(message, "AUTH_FAILED")
        {
            UserId = userId;
        }

        public AuthenticationException(string message, string? userId, string? attemptedAction) 
            : base(message, "AUTH_FAILED")
        {
            UserId = userId;
            AttemptedAction = attemptedAction;
        }
    }

    /// <summary>
    /// Exception for authorization/permission failures
    /// </summary>
    public class AuthorizationException : GrcBaseException
    {
        public string? UserId { get; }
        public string? RequiredPermission { get; }
        public string? Resource { get; }

        public AuthorizationException(string message, string? userId = null) 
            : base(message, "AUTH_DENIED")
        {
            UserId = userId;
        }

        public AuthorizationException(string message, string? userId, string? requiredPermission, string? resource = null) 
            : base(message, "AUTH_DENIED")
        {
            UserId = userId;
            RequiredPermission = requiredPermission;
            Resource = resource;
        }
    }

    /// <summary>
    /// Exception for invalid or expired tokens
    /// </summary>
    public class TokenException : GrcBaseException
    {
        public string? TokenType { get; }

        public TokenException(string message, string? tokenType = null) 
            : base(message, "TOKEN_INVALID")
        {
            TokenType = tokenType;
        }
    }

    #endregion

    #region Data Exceptions

    /// <summary>
    /// Exception when a requested entity is not found
    /// </summary>
    public class EntityNotFoundException : GrcBaseException
    {
        public string EntityType { get; }
        public string? EntityId { get; }

        public EntityNotFoundException(string entityType, string? entityId = null) 
            : base($"{entityType} not found" + (entityId != null ? $": {entityId}" : ""), "ENTITY_NOT_FOUND")
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }

    /// <summary>
    /// Exception for duplicate entity conflicts
    /// </summary>
    public class DuplicateEntityException : GrcBaseException
    {
        public string EntityType { get; }
        public string? ConflictField { get; }

        public DuplicateEntityException(string entityType, string? conflictField = null) 
            : base($"Duplicate {entityType}" + (conflictField != null ? $" on {conflictField}" : ""), "DUPLICATE_ENTITY")
        {
            EntityType = entityType;
            ConflictField = conflictField;
        }
    }

    /// <summary>
    /// Exception for data integrity violations
    /// </summary>
    public class DataIntegrityException : GrcBaseException
    {
        public string? ConstraintName { get; }

        public DataIntegrityException(string message, string? constraintName = null) 
            : base(message, "DATA_INTEGRITY_VIOLATION")
        {
            ConstraintName = constraintName;
        }
    }

    #endregion

    #region Validation Exceptions

    /// <summary>
    /// Exception for input validation failures
    /// </summary>
    public class ValidationException : GrcBaseException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(string message) 
            : base(message, "VALIDATION_FAILED")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message, Dictionary<string, string[]> errors) 
            : base(message, "VALIDATION_FAILED")
        {
            Errors = errors;
        }

        public ValidationException(string field, string error) 
            : base($"Validation failed for {field}: {error}", "VALIDATION_FAILED")
        {
            Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
        }
    }

    /// <summary>
    /// Exception for business rule violations
    /// </summary>
    public class BusinessRuleException : GrcBaseException
    {
        public string RuleName { get; }

        public BusinessRuleException(string message, string ruleName) 
            : base(message, "BUSINESS_RULE_VIOLATION")
        {
            RuleName = ruleName;
        }
    }

    #endregion

    #region Tenant Exceptions

    /// <summary>
    /// Exception for tenant-related errors
    /// </summary>
    public class TenantException : GrcBaseException
    {
        public Guid? TenantId { get; }

        public TenantException(string message, Guid? tenantId = null) 
            : base(message, "TENANT_ERROR")
        {
            TenantId = tenantId;
        }
    }

    /// <summary>
    /// Exception when tenant context is required but not available
    /// </summary>
    public class TenantContextRequiredException : TenantException
    {
        public TenantContextRequiredException() 
            : base("Tenant context is required for this operation")
        {
        }
    }

    /// <summary>
    /// Exception when tenant is not found
    /// </summary>
    public class TenantNotFoundException : TenantException
    {
        public TenantNotFoundException(Guid tenantId) 
            : base($"Tenant not found: {tenantId}", tenantId)
        {
        }
    }

    #endregion

    #region Integration Exceptions

    /// <summary>
    /// Exception for external service/API failures
    /// </summary>
    public class ExternalServiceException : GrcBaseException
    {
        public string ServiceName { get; }
        public int? StatusCode { get; }

        public ExternalServiceException(string serviceName, string message, int? statusCode = null) 
            : base(message, "EXTERNAL_SERVICE_ERROR")
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
        }

        public ExternalServiceException(string serviceName, string message, Exception innerException) 
            : base(message, "EXTERNAL_SERVICE_ERROR", innerException)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// Exception for email service failures
    /// </summary>
    public class EmailServiceException : ExternalServiceException
    {
        public string? RecipientEmail { get; }

        public EmailServiceException(string message, string? recipientEmail = null) 
            : base("EmailService", message)
        {
            RecipientEmail = recipientEmail;
        }
    }

    /// <summary>
    /// Exception for payment service failures
    /// </summary>
    public class PaymentServiceException : ExternalServiceException
    {
        public string? TransactionId { get; }

        public PaymentServiceException(string message, string? transactionId = null) 
            : base("PaymentService", message)
        {
            TransactionId = transactionId;
        }
    }

    #endregion

    #region Workflow Exceptions

    /// <summary>
    /// Exception for workflow execution errors
    /// </summary>
    public class WorkflowException : GrcBaseException
    {
        public Guid? WorkflowInstanceId { get; }
        public string? WorkflowType { get; }

        public WorkflowException(string message, Guid? workflowInstanceId = null, string? workflowType = null) 
            : base(message, "WORKFLOW_ERROR")
        {
            WorkflowInstanceId = workflowInstanceId;
            WorkflowType = workflowType;
        }
    }

    /// <summary>
    /// Exception for invalid workflow state transitions
    /// </summary>
    public class WorkflowStateException : WorkflowException
    {
        public string CurrentState { get; }
        public string AttemptedState { get; }

        public WorkflowStateException(string currentState, string attemptedState, Guid? workflowInstanceId = null) 
            : base($"Invalid workflow state transition from {currentState} to {attemptedState}", workflowInstanceId)
        {
            CurrentState = currentState;
            AttemptedState = attemptedState;
        }
    }

    #endregion

    #region Configuration Exceptions

    /// <summary>
    /// Exception for configuration errors
    /// </summary>
    public class ConfigurationException : GrcBaseException
    {
        public string ConfigurationKey { get; }

        public ConfigurationException(string configurationKey, string message) 
            : base(message, "CONFIG_ERROR")
        {
            ConfigurationKey = configurationKey;
        }
    }

    /// <summary>
    /// Exception for missing required configuration
    /// </summary>
    public class MissingConfigurationException : ConfigurationException
    {
        public MissingConfigurationException(string configurationKey) 
            : base(configurationKey, $"Required configuration missing: {configurationKey}")
        {
        }
    }

    #endregion

    #region Rate Limiting Exceptions

    /// <summary>
    /// Exception when rate limit is exceeded
    /// </summary>
    public class RateLimitExceededException : GrcBaseException
    {
        public string? Resource { get; }
        public TimeSpan? RetryAfter { get; }

        public RateLimitExceededException(string message, string? resource = null, TimeSpan? retryAfter = null) 
            : base(message, "RATE_LIMIT_EXCEEDED")
        {
            Resource = resource;
            RetryAfter = retryAfter;
        }
    }

    #endregion

    #region File/Storage Exceptions

    /// <summary>
    /// Exception for file operation failures
    /// </summary>
    public class FileOperationException : GrcBaseException
    {
        public string? FileName { get; }
        public string Operation { get; }

        public FileOperationException(string operation, string message, string? fileName = null) 
            : base(message, "FILE_OPERATION_ERROR")
        {
            Operation = operation;
            FileName = fileName;
        }
    }

    /// <summary>
    /// Exception for invalid file type
    /// </summary>
    public class InvalidFileTypeException : FileOperationException
    {
        public string? ActualType { get; }
        public string[]? AllowedTypes { get; }

        public InvalidFileTypeException(string? actualType, string[]? allowedTypes) 
            : base("Upload", $"Invalid file type: {actualType}. Allowed types: {string.Join(", ", allowedTypes ?? Array.Empty<string>())}")
        {
            ActualType = actualType;
            AllowedTypes = allowedTypes;
        }
    }

    #endregion
}
