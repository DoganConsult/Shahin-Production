using System;
using System.Collections.Generic;

namespace GrcMvc.Exceptions
{
    /// <summary>
    /// Base exception for all GRC domain errors.
    /// Provides consistent error handling across the entire GRC system.
    /// </summary>
    public class GrcException : Exception
    {
        /// <summary>Machine-readable error code for client handling</summary>
        public string ErrorCode { get; }
        
        /// <summary>Additional error details for debugging</summary>
        public List<string>? Details { get; }
        
        /// <summary>HTTP status code suggestion (for API responses)</summary>
        public int SuggestedStatusCode { get; }

        public GrcException(string message, string errorCode = GrcErrorCodes.GeneralError, 
            int statusCode = 400, List<string>? details = null)
            : base(message)
        {
            ErrorCode = errorCode;
            SuggestedStatusCode = statusCode;
            Details = details;
        }

        public GrcException(string message, Exception innerException, string errorCode = GrcErrorCodes.GeneralError)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            SuggestedStatusCode = 500;
        }
    }

    #region Entity Exceptions

    /// <summary>
    /// Thrown when a requested entity is not found.
    /// </summary>
    public class EntityNotFoundException : GrcException
    {
        public string EntityType { get; }
        public object EntityId { get; }

        public EntityNotFoundException(string entityType, object entityId)
            : base($"{entityType} with ID '{entityId}' not found", GrcErrorCodes.NotFound, 404)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public static EntityNotFoundException For<T>(Guid id) where T : class
            => new(typeof(T).Name, id);

        public static EntityNotFoundException For<T>(string identifier) where T : class
            => new(typeof(T).Name, identifier);
    }

    /// <summary>
    /// Thrown when an entity already exists (duplicate).
    /// </summary>
    public class EntityExistsException : GrcException
    {
        public string EntityType { get; }
        public string ConflictField { get; }

        public EntityExistsException(string entityType, string conflictField, string value)
            : base($"{entityType} with {conflictField} '{value}' already exists", GrcErrorCodes.Conflict, 409)
        {
            EntityType = entityType;
            ConflictField = conflictField;
        }
    }

    /// <summary>
    /// Thrown when entity validation fails.
    /// </summary>
    public class ValidationException : GrcException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(string message, Dictionary<string, string[]>? errors = null)
            : base(message, GrcErrorCodes.ValidationFailed, 400)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string field, string error)
            : base($"Validation failed: {field} - {error}", GrcErrorCodes.ValidationFailed, 400)
        {
            Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
        }
    }

    #endregion

    #region Authorization Exceptions

    /// <summary>
    /// Thrown when user is not authenticated.
    /// </summary>
    public class AuthenticationException : GrcException
    {
        public AuthenticationException(string message = "Authentication required")
            : base(message, GrcErrorCodes.Unauthenticated, 401)
        {
        }
    }

    /// <summary>
    /// Thrown when user lacks required permissions.
    /// </summary>
    public class AuthorizationException : GrcException
    {
        public string? RequiredPermission { get; }
        public string? RequiredRole { get; }

        public AuthorizationException(string message = "Access denied")
            : base(message, GrcErrorCodes.Unauthorized, 403)
        {
        }

        public AuthorizationException(string permission, bool isRole = false)
            : base(isRole ? $"Role '{permission}' required" : $"Permission '{permission}' required", 
                  GrcErrorCodes.Unauthorized, 403)
        {
            if (isRole)
                RequiredRole = permission;
            else
                RequiredPermission = permission;
        }
    }

    #endregion

    #region Tenant Exceptions

    /// <summary>
    /// Thrown when tenant context is required but not available.
    /// </summary>
    public class TenantRequiredException : GrcException
    {
        public TenantRequiredException(string message = "Tenant context is required for this operation")
            : base(message, GrcErrorCodes.TenantRequired, 400)
        {
        }
    }

    /// <summary>
    /// Thrown when operation violates tenant boundaries.
    /// </summary>
    public class TenantMismatchException : GrcException
    {
        public Guid ExpectedTenantId { get; }
        public Guid ActualTenantId { get; }

        public TenantMismatchException(Guid expected, Guid actual)
            : base($"Tenant mismatch: expected {expected}, got {actual}", GrcErrorCodes.TenantMismatch, 403)
        {
            ExpectedTenantId = expected;
            ActualTenantId = actual;
        }
    }

    /// <summary>
    /// Thrown when tenant is not in a valid state for the operation.
    /// </summary>
    public class TenantStateException : GrcException
    {
        public string CurrentState { get; }
        public string RequiredState { get; }

        public TenantStateException(string currentState, string requiredState)
            : base($"Tenant is in '{currentState}' state, '{requiredState}' required", GrcErrorCodes.InvalidState, 400)
        {
            CurrentState = currentState;
            RequiredState = requiredState;
        }
    }

    #endregion

    #region User/Role Exceptions

    /// <summary>
    /// Thrown when a user is not found.
    /// </summary>
    public class UserNotFoundException : GrcException
    {
        public object UserId { get; }

        public UserNotFoundException(Guid userId)
            : base($"User with ID '{userId}' not found", GrcErrorCodes.UserNotFound, 404)
        {
            UserId = userId;
        }

        public UserNotFoundException(string identifier)
            : base($"User '{identifier}' not found", GrcErrorCodes.UserNotFound, 404)
        {
            UserId = identifier;
        }
    }

    /// <summary>
    /// Thrown when a role operation fails.
    /// </summary>
    public class RoleException : GrcException
    {
        public string RoleName { get; }

        public RoleException(string roleName, string message)
            : base(message, GrcErrorCodes.RoleError, 400)
        {
            RoleName = roleName;
        }
    }

    /// <summary>
    /// Thrown when task delegation fails.
    /// </summary>
    public class DelegationException : GrcException
    {
        public Guid TaskId { get; }
        public Guid FromUserId { get; }
        public Guid ToUserId { get; }

        public DelegationException(Guid taskId, Guid fromUserId, Guid toUserId, string reason)
            : base($"Cannot delegate task {taskId}: {reason}", GrcErrorCodes.DelegationFailed, 400)
        {
            TaskId = taskId;
            FromUserId = fromUserId;
            ToUserId = toUserId;
        }
    }

    #endregion

    #region Evidence/Assessment Exceptions

    /// <summary>
    /// Thrown when evidence operation fails.
    /// </summary>
    public class EvidenceException : GrcException
    {
        public Guid EvidenceId { get; }

        public EvidenceException(Guid evidenceId, string message)
            : base(message, GrcErrorCodes.EvidenceError, 400)
        {
            EvidenceId = evidenceId;
        }
    }

    /// <summary>
    /// Thrown when assessment operation fails.
    /// </summary>
    public class AssessmentException : GrcException
    {
        public Guid AssessmentId { get; }

        public AssessmentException(Guid assessmentId, string message)
            : base(message, GrcErrorCodes.AssessmentError, 400)
        {
            AssessmentId = assessmentId;
        }
    }

    /// <summary>
    /// Thrown when requirement operation fails.
    /// </summary>
    public class RequirementException : GrcException
    {
        public Guid RequirementId { get; }

        public RequirementException(Guid requirementId, string message)
            : base(message, GrcErrorCodes.RequirementError, 400)
        {
            RequirementId = requirementId;
        }

        public static RequirementException NotFound(Guid id)
            => new(id, "Assessment requirement not found");
    }

    #endregion

    #region Catalog Exceptions

    /// <summary>
    /// Thrown when catalog entity operation fails.
    /// </summary>
    public class CatalogException : GrcException
    {
        public string CatalogType { get; }

        public CatalogException(string catalogType, string message)
            : base(message, GrcErrorCodes.CatalogError, 400)
        {
            CatalogType = catalogType;
        }

        public static CatalogException NotFound(string type, object id)
            => new(type, $"{type} with ID '{id}' not found");
    }

    #endregion

    #region Integration Exceptions

    /// <summary>
    /// Thrown when external service integration fails.
    /// </summary>
    public class IntegrationException : GrcException
    {
        public string ServiceName { get; }

        public IntegrationException(string serviceName, string message, Exception? inner = null)
            : base($"Integration with {serviceName} failed: {message}", 
                  inner ?? new Exception(), GrcErrorCodes.IntegrationFailed)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// Thrown when agent operation fails.
    /// </summary>
    public class AgentException : GrcException
    {
        public string AgentType { get; }

        public AgentException(string agentType, string message)
            : base($"Agent '{agentType}' error: {message}", GrcErrorCodes.AgentError, 400)
        {
            AgentType = agentType;
        }

        public static AgentException InvalidType(string agentType)
            => new(agentType, "Invalid agent type");
    }

    #endregion

    #region Subscription Exceptions

    /// <summary>
    /// Thrown when subscription operation fails.
    /// </summary>
    public class SubscriptionException : GrcException
    {
        public Guid? SubscriptionId { get; }

        public SubscriptionException(string message, Guid? subscriptionId = null)
            : base(message, GrcErrorCodes.SubscriptionError, 400)
        {
            SubscriptionId = subscriptionId;
        }
    }

    /// <summary>
    /// Thrown when feature is not available in current plan.
    /// </summary>
    public class FeatureNotAvailableException : GrcException
    {
        public string FeatureName { get; }
        public string RequiredPlan { get; }

        public FeatureNotAvailableException(string featureName, string requiredPlan)
            : base($"Feature '{featureName}' requires '{requiredPlan}' plan or higher", 
                  GrcErrorCodes.FeatureNotAvailable, 403)
        {
            FeatureName = featureName;
            RequiredPlan = requiredPlan;
        }
    }

    #endregion

    /// <summary>
    /// Standard error codes for GRC operations.
    /// Use these codes consistently across the application for client-side error handling.
    /// </summary>
    public static class GrcErrorCodes
    {
        // General
        public const string GeneralError = "GRC_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string InvalidState = "INVALID_STATE";

        // Auth
        public const string Unauthenticated = "UNAUTHENTICATED";
        public const string Unauthorized = "UNAUTHORIZED";

        // Tenant
        public const string TenantRequired = "TENANT_REQUIRED";
        public const string TenantMismatch = "TENANT_MISMATCH";
        public const string TenantNotActive = "TENANT_NOT_ACTIVE";

        // User/Role
        public const string UserNotFound = "USER_NOT_FOUND";
        public const string RoleError = "ROLE_ERROR";
        public const string DelegationFailed = "DELEGATION_FAILED";

        // Domain
        public const string EvidenceError = "EVIDENCE_ERROR";
        public const string AssessmentError = "ASSESSMENT_ERROR";
        public const string RequirementError = "REQUIREMENT_ERROR";
        public const string CatalogError = "CATALOG_ERROR";
        public const string WorkflowError = "WORKFLOW_ERROR";

        // Integration
        public const string IntegrationFailed = "INTEGRATION_FAILED";
        public const string AgentError = "AGENT_ERROR";

        // Subscription
        public const string SubscriptionError = "SUBSCRIPTION_ERROR";
        public const string FeatureNotAvailable = "FEATURE_NOT_AVAILABLE";
    }
}
