namespace GrcMvc.Exceptions
{
    /// <summary>
    /// Base exception for workflow operations
    /// </summary>
    public class WorkflowException : Exception
    {
        public string ErrorCode { get; }
        public int? WorkflowId { get; }
        public string? WorkflowType { get; }

        public WorkflowException(string message) : base(message)
        {
            ErrorCode = "WORKFLOW_ERROR";
        }

        public WorkflowException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public WorkflowException(string message, int workflowId, string? workflowType = null) : base(message)
        {
            ErrorCode = "WORKFLOW_ERROR";
            WorkflowId = workflowId;
            WorkflowType = workflowType;
        }

        public WorkflowException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "WORKFLOW_ERROR";
        }
    }

    /// <summary>
    /// Exception thrown when a workflow is not found
    /// </summary>
    public class WorkflowNotFoundException : WorkflowException
    {
        public WorkflowNotFoundException(int workflowId)
            : base($"Workflow with ID {workflowId} was not found.", workflowId)
        {
        }

        public WorkflowNotFoundException(string workflowType, int entityId)
            : base($"Workflow of type '{workflowType}' for entity {entityId} was not found.")
        {
        }
    }

    /// <summary>
    /// Exception thrown when a state transition is invalid
    /// </summary>
    public class InvalidStateTransitionException : WorkflowException
    {
        public string CurrentState { get; }
        public string TargetState { get; }

        public InvalidStateTransitionException(string currentState, string targetState, int workflowId)
            : base($"Cannot transition from '{currentState}' to '{targetState}' for workflow {workflowId}.", "INVALID_TRANSITION")
        {
            CurrentState = currentState;
            TargetState = targetState;
        }

        public InvalidStateTransitionException(string currentState, string targetState, string reason)
            : base($"Cannot transition from '{currentState}' to '{targetState}': {reason}", "INVALID_TRANSITION")
        {
            CurrentState = currentState;
            TargetState = targetState;
        }
    }

    /// <summary>
    /// Exception thrown when user lacks permission for workflow operation
    /// </summary>
    public class WorkflowAuthorizationException : WorkflowException
    {
        public string UserId { get; }
        public string RequiredPermission { get; }

        public WorkflowAuthorizationException(string userId, string requiredPermission)
            : base($"User '{userId}' does not have permission '{requiredPermission}' for this workflow operation.", "AUTHORIZATION_FAILED")
        {
            UserId = userId;
            RequiredPermission = requiredPermission;
        }

        public WorkflowAuthorizationException(string userId, int workflowId, string action)
            : base($"User '{userId}' is not authorized to perform '{action}' on workflow {workflowId}.", "AUTHORIZATION_FAILED")
        {
            UserId = userId;
            RequiredPermission = action;
        }
    }

    /// <summary>
    /// Exception thrown when workflow validation fails
    /// </summary>
    public class WorkflowValidationException : WorkflowException
    {
        public Dictionary<string, string[]> Errors { get; }

        public WorkflowValidationException(string message)
            : base(message, "VALIDATION_FAILED")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public WorkflowValidationException(Dictionary<string, string[]> errors)
            : base("Workflow validation failed.", "VALIDATION_FAILED")
        {
            Errors = errors;
        }

        public WorkflowValidationException(string field, string error)
            : base($"Validation failed for '{field}': {error}", "VALIDATION_FAILED")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
        }
    }

    /// <summary>
    /// Exception thrown when a workflow is already completed
    /// </summary>
    public class WorkflowAlreadyCompletedException : WorkflowException
    {
        public DateTime CompletedAt { get; }

        public WorkflowAlreadyCompletedException(int workflowId, DateTime completedAt)
            : base($"Workflow {workflowId} has already been completed at {completedAt}.", "WORKFLOW_COMPLETED")
        {
            CompletedAt = completedAt;
        }
    }

    /// <summary>
    /// Exception thrown when a workflow is cancelled
    /// </summary>
    public class WorkflowCancelledException : WorkflowException
    {
        public string CancelledBy { get; }
        public DateTime CancelledAt { get; }

        public WorkflowCancelledException(int workflowId, string cancelledBy, DateTime cancelledAt)
            : base($"Workflow {workflowId} has been cancelled by {cancelledBy} at {cancelledAt}.", "WORKFLOW_CANCELLED")
        {
            CancelledBy = cancelledBy;
            CancelledAt = cancelledAt;
        }
    }

    /// <summary>
    /// Exception thrown when task assignment fails
    /// </summary>
    public class TaskAssignmentException : WorkflowException
    {
        public int TaskId { get; }
        public string AssigneeId { get; }

        public TaskAssignmentException(int taskId, string assigneeId, string reason)
            : base($"Cannot assign task {taskId} to user '{assigneeId}': {reason}", "TASK_ASSIGNMENT_FAILED")
        {
            TaskId = taskId;
            AssigneeId = assigneeId;
        }
    }

    /// <summary>
    /// Exception thrown when approval fails
    /// </summary>
    public class ApprovalException : WorkflowException
    {
        public string ApproverId { get; }
        public int ApprovalLevel { get; }

        public ApprovalException(int workflowId, string approverId, string reason)
            : base($"Approval failed for workflow {workflowId}: {reason}", workflowId)
        {
            ApproverId = approverId;
            ApprovalLevel = 0;
        }

        public ApprovalException(int workflowId, string approverId, int level, string reason)
            : base($"Level {level} approval failed for workflow {workflowId}: {reason}", workflowId)
        {
            ApproverId = approverId;
            ApprovalLevel = level;
        }
    }

    /// <summary>
    /// Exception thrown when SLA is breached
    /// </summary>
    public class SlaBreachException : WorkflowException
    {
        public DateTime SlaDueDate { get; }
        public DateTime BreachTime { get; }

        public SlaBreachException(int workflowId, DateTime slaDueDate)
            : base($"SLA breached for workflow {workflowId}. Due date was {slaDueDate}.", workflowId)
        {
            SlaDueDate = slaDueDate;
            BreachTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exception thrown when escalation fails
    /// </summary>
    public class EscalationException : WorkflowException
    {
        public int EscalationLevel { get; }

        public EscalationException(int workflowId, int level, string reason)
            : base($"Escalation to level {level} failed for workflow {workflowId}: {reason}", workflowId)
        {
            EscalationLevel = level;
        }
    }

    /// <summary>
    /// Exception thrown when notification delivery fails
    /// </summary>
    public class NotificationDeliveryException : WorkflowException
    {
        public int NotificationId { get; }
        public string RecipientId { get; }
        public int AttemptCount { get; }

        public NotificationDeliveryException(int notificationId, string recipientId, int attempts, string reason)
            : base($"Notification {notificationId} delivery failed after {attempts} attempts: {reason}", "NOTIFICATION_FAILED")
        {
            NotificationId = notificationId;
            RecipientId = recipientId;
            AttemptCount = attempts;
        }
    }

    /// <summary>
    /// Exception thrown when a workflow dependency is not met
    /// </summary>
    public class WorkflowDependencyException : WorkflowException
    {
        public int DependencyWorkflowId { get; }
        public string DependencyStatus { get; }

        public WorkflowDependencyException(int workflowId, int dependencyId, string dependencyStatus)
            : base($"Workflow {workflowId} cannot proceed: dependency workflow {dependencyId} is in status '{dependencyStatus}'.", workflowId)
        {
            DependencyWorkflowId = dependencyId;
            DependencyStatus = dependencyStatus;
        }
    }

    /// <summary>
    /// Exception thrown when evidence is invalid or missing
    /// </summary>
    public class EvidenceException : WorkflowException
    {
        public int? EvidenceId { get; }

        public EvidenceException(string reason)
            : base($"Evidence error: {reason}", "EVIDENCE_ERROR")
        {
        }

        public EvidenceException(int evidenceId, string reason)
            : base($"Evidence {evidenceId} error: {reason}", "EVIDENCE_ERROR")
        {
            EvidenceId = evidenceId;
        }
    }

    /// <summary>
    /// Exception thrown when concurrent modification is detected
    /// </summary>
    public class WorkflowConcurrencyException : WorkflowException
    {
        public WorkflowConcurrencyException(int workflowId)
            : base($"Workflow {workflowId} was modified by another user. Please refresh and try again.", workflowId)
        {
        }
    }
}
