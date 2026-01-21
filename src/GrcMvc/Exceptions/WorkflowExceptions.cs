using System;
using System.Collections.Generic;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Exceptions
{
    /// <summary>
    /// Base exception for all workflow-related errors.
    /// </summary>
    public class WorkflowException : Exception
    {
        public string ErrorCode { get; }
        public List<string>? Details { get; }

        public WorkflowException(string message, string errorCode = WorkflowErrorCodes.WorkflowError, List<string>? details = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public WorkflowException(string message, Exception innerException, string errorCode = WorkflowErrorCodes.WorkflowError)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public WorkflowApiResponse ToApiResponse()
        {
            return WorkflowApiResponse.Fail(Message, ErrorCode, Details);
        }
    }

    /// <summary>
    /// Exception thrown when a workflow or task is not found.
    /// </summary>
    public class WorkflowNotFoundException : WorkflowException
    {
        public Guid EntityId { get; }
        public string EntityType { get; }

        public WorkflowNotFoundException(string entityType, Guid entityId)
            : base($"{entityType} with ID {entityId} not found", WorkflowErrorCodes.NotFound)
        {
            EntityId = entityId;
            EntityType = entityType;
        }
    }

    /// <summary>
    /// Exception thrown when an invalid state transition is attempted.
    /// </summary>
    public class InvalidStateTransitionException : WorkflowException
    {
        public string CurrentState { get; }
        public string AttemptedState { get; }
        public List<string> AllowedTransitions { get; }

        public InvalidStateTransitionException(string currentState, string attemptedState, IEnumerable<string> allowedTransitions)
            : base($"Cannot transition from '{currentState}' to '{attemptedState}'", WorkflowErrorCodes.InvalidStateTransition)
        {
            CurrentState = currentState;
            AttemptedState = attemptedState;
            AllowedTransitions = new List<string>(allowedTransitions);
        }
    }

    /// <summary>
    /// Exception thrown when task assignment fails.
    /// </summary>
    public class TaskAssignmentException : WorkflowException
    {
        public Guid TaskId { get; }
        public string? AssigneeId { get; }

        public TaskAssignmentException(Guid taskId, string message, string? assigneeId = null)
            : base(message, WorkflowErrorCodes.AssignmentFailed)
        {
            TaskId = taskId;
            AssigneeId = assigneeId;
        }
    }

    /// <summary>
    /// Exception thrown when workflow validation fails.
    /// </summary>
    public class WorkflowValidationException : WorkflowException
    {
        public List<string> ValidationErrors { get; }

        public WorkflowValidationException(IEnumerable<string> errors)
            : base("Workflow validation failed", WorkflowErrorCodes.ValidationFailed, new List<string>(errors))
        {
            ValidationErrors = new List<string>(errors);
        }
    }

    /// <summary>
    /// Exception thrown when audit logging fails.
    /// </summary>
    public class AuditFailureException : WorkflowException
    {
        public string Operation { get; }
        public Guid? EntityId { get; }

        public AuditFailureException(string message, Exception? innerException = null)
            : base(message, innerException ?? new Exception(), WorkflowErrorCodes.AuditFailed)
        {
            Operation = string.Empty;
        }

        public AuditFailureException(string operation, Guid entityId, Exception innerException)
            : base($"Audit operation '{operation}' failed for entity {entityId}", innerException, WorkflowErrorCodes.AuditFailed)
        {
            Operation = operation;
            EntityId = entityId;
        }
    }

    /// <summary>
    /// Exception thrown when tenant isolation is violated.
    /// </summary>
    public class TenantIsolationException : WorkflowException
    {
        public Guid? ExpectedTenantId { get; }
        public Guid? ActualTenantId { get; }

        public TenantIsolationException(string message, Guid? expectedTenantId = null, Guid? actualTenantId = null)
            : base(message, WorkflowErrorCodes.TenantMismatch)
        {
            ExpectedTenantId = expectedTenantId;
            ActualTenantId = actualTenantId;
        }

        public static TenantIsolationException TenantRequired()
        {
            return new TenantIsolationException("Tenant ID is required for this operation");
        }

        public static TenantIsolationException Mismatch(Guid expected, Guid actual)
        {
            return new TenantIsolationException(
                $"Tenant mismatch: expected {expected}, got {actual}",
                expected,
                actual);
        }
    }

    /// <summary>
    /// Exception thrown when SLA is breached.
    /// </summary>
    public class SlaBreachedException : WorkflowException
    {
        public Guid WorkflowId { get; }
        public DateTime DueDate { get; }
        public DateTime BreachedAt { get; }

        public SlaBreachedException(Guid workflowId, DateTime dueDate)
            : base($"SLA breached for workflow {workflowId}. Due: {dueDate:u}", WorkflowErrorCodes.SlaBreached)
        {
            WorkflowId = workflowId;
            DueDate = dueDate;
            BreachedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exception thrown when workflow is not in active state.
    /// </summary>
    public class WorkflowNotActiveException : WorkflowException
    {
        public Guid WorkflowId { get; }
        public string CurrentStatus { get; }

        public WorkflowNotActiveException(Guid workflowId, string currentStatus)
            : base($"Workflow {workflowId} is not active (current status: {currentStatus})", WorkflowErrorCodes.WorkflowNotActive)
        {
            WorkflowId = workflowId;
            CurrentStatus = currentStatus;
        }
    }

    /// <summary>
    /// Standard error codes for workflow operations.
    /// Single source of truth - use these codes consistently across the application.
    /// </summary>
    public static class WorkflowErrorCodes
    {
        // General errors
        public const string WorkflowError = "WORKFLOW_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Timeout = "TIMEOUT";

        // Validation errors
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string InvalidStateTransition = "INVALID_STATE_TRANSITION";
        public const string WorkflowNotActive = "WORKFLOW_NOT_ACTIVE";

        // Assignment errors
        public const string AssignmentFailed = "ASSIGNMENT_FAILED";
        public const string AssigneeNotFound = "ASSIGNEE_NOT_FOUND";
        public const string DelegationFailed = "DELEGATION_FAILED";

        // Tenant errors
        public const string TenantMismatch = "TENANT_MISMATCH";
        public const string TenantRequired = "TENANT_REQUIRED";

        // SLA errors
        public const string SlaBreached = "SLA_BREACHED";
        public const string DueDatePassed = "DUE_DATE_PASSED";

        // Audit errors
        public const string AuditFailed = "AUDIT_FAILED";
    }
}
