using GrcMvc.Models.Entities;

namespace GrcMvc.Models.Workflows
{
    /// <summary>
    /// Workflow state machine enums and models for all 10 workflow types
    /// </summary>

    // ===== WORKFLOW STATE ENUMS =====

    /// <summary>Control Implementation Workflow States</summary>
    public enum ControlImplementationState
    {
        NotStarted,
        InPlanning,
        InImplementation,
        UnderReview,
        Approved,
        Deployed,
        Monitored,
        Completed,
        Failed
    }

    /// <summary>Risk Assessment Workflow States</summary>
    public enum RiskAssessmentState
    {
        NotStarted,
        DataGathering,
        Analysis,
        Evaluation,
        UnderReview,
        Approved,
        Documented,
        Monitored,
        Closed
    }

    /// <summary>Approval/Sign-off Workflow States</summary>
    public enum ApprovalState
    {
        Submitted,
        PendingManagerReview,
        ManagerApproved,
        PendingComplianceReview,
        ComplianceApproved,
        PendingExecutiveSignOff,
        ExecutiveApproved,
        Rejected,
        Completed
    }

    /// <summary>Evidence Collection Workflow States</summary>
    public enum EvidenceCollectionState
    {
        NotStarted,
        PendingSubmission,
        Submitted,
        UnderReview,
        RequestedRevisions,
        Approved,
        Archived,
        Expired
    }

    /// <summary>Compliance Testing Workflow States</summary>
    public enum ComplianceTestingState
    {
        NotStarted,
        TestPlanCreated,
        TestsInProgress,
        TestsCompleted,
        ResultsReview,
        NonCompliance,
        Compliant,
        Remediation,
        Verified
    }

    /// <summary>Remediation Workflow States</summary>
    public enum RemediationState
    {
        Identified,
        PlanningPhase,
        RemediationInProgress,
        UnderVerification,
        Verified,
        Monitored,
        Closed
    }

    /// <summary>Policy Review Workflow States</summary>
    public enum PolicyReviewState
    {
        ScheduledForReview,
        InReview,
        RequestedRevisions,
        UnderApproval,
        Approved,
        Published,
        InEffect,
        Obsolete
    }

    /// <summary>Training Assignment Workflow States</summary>
    public enum TrainingAssignmentState
    {
        Assigned,
        Acknowledged,
        InProgress,
        Completed,
        Passed,
        Failed,
        Reassigned,
        Archived
    }

    /// <summary>Audit Workflow States</summary>
    public enum AuditState
    {
        NotStarted,
        PlanningPhase,
        FieldworkInProgress,
        DocumentationPhase,
        UnderReview,
        DraftReportIssued,
        AwaitingManagementResponse,
        FinalReportIssued,
        FollowUpScheduled,
        Closed
    }

    /// <summary>Exception Handling Workflow States</summary>
    public enum ExceptionHandlingState
    {
        Submitted,
        PendingReview,
        UnderInvestigation,
        RiskAssessed,
        PendingApproval,
        Approved,
        RejectedWithExplanation,
        Monitoring,
        Resolved,
        Closed
    }

    // ===== WORKFLOW RUNTIME MODELS =====
    // These are workflow-engine/runtime types, NOT persistence entities.
    // For EF Core persistence, use Models.Entities.WorkflowInstance and Models.Entities.WorkflowTask.

    /// <summary>
    /// WorkflowRuntimeInstance - Runtime/workflow-engine representation of a workflow.
    /// Use this for workflow logic, state machines, and service layer operations.
    /// Map to/from Entities.WorkflowInstance for persistence.
    /// </summary>
    public class WorkflowRuntimeInstance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string InitiatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedBy { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>Create from persistence entity</summary>
        public static WorkflowRuntimeInstance FromEntity(Entities.WorkflowInstance entity)
        {
            return new WorkflowRuntimeInstance
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                WorkflowType = entity.WorkflowType,
                EntityId = entity.EntityId,
                EntityType = entity.EntityType,
                CurrentState = entity.CurrentState,
                Status = entity.Status,
                InitiatedBy = entity.InitiatedByUserId?.ToString() ?? string.Empty,
                CreatedAt = entity.CreatedDate,
                CompletedAt = entity.CompletedAt,
                CompletedBy = entity.CompletedByUserId
            };
        }

        /// <summary>Apply changes to persistence entity</summary>
        public void ApplyToEntity(Entities.WorkflowInstance entity)
        {
            entity.CurrentState = this.CurrentState;
            entity.Status = this.Status;
            entity.CompletedAt = this.CompletedAt;
            entity.CompletedByUserId = this.CompletedBy;
        }
    }

    /// <summary>
    /// WorkflowRuntimeTask - Runtime/workflow-engine representation of a task.
    /// Use this for workflow logic and service layer operations.
    /// Map to/from Entities.WorkflowTask for persistence.
    /// </summary>
    public class WorkflowRuntimeTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssignedToUserId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletionNotes { get; set; }
        public int Priority { get; set; } = 2;
        public bool IsEscalated { get; set; } = false;
        public string? EscalatedToUserId { get; set; }

        /// <summary>Create from persistence entity</summary>
        public static WorkflowRuntimeTask FromEntity(Entities.WorkflowTask entity)
        {
            return new WorkflowRuntimeTask
            {
                Id = entity.Id,
                WorkflowInstanceId = entity.WorkflowInstanceId,
                TaskName = entity.TaskName,
                Description = entity.Description,
                AssignedToUserId = entity.AssignedToUserId?.ToString() ?? string.Empty,
                Status = entity.Status,
                DueDate = entity.DueDate ?? DateTime.UtcNow.AddDays(7),
                CompletedAt = entity.CompletedAt,
                CompletionNotes = entity.CompletionNotes,
                Priority = entity.Priority,
                IsEscalated = entity.IsEscalated,
                EscalatedToUserId = entity.EscalatedToUserId?.ToString()
            };
        }

        /// <summary>Apply changes to persistence entity</summary>
        public void ApplyToEntity(Entities.WorkflowTask entity)
        {
            entity.Status = this.Status;
            entity.CompletedAt = this.CompletedAt;
            entity.CompletionNotes = this.CompletionNotes;
            entity.IsEscalated = this.IsEscalated;
        }
    }

    /// <summary>Approval record in approval workflow</summary>
    public class WorkflowApproval
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public string ApprovalLevel { get; set; } = string.Empty; // Manager, Compliance, Executive
        public string ApprovedByUserId { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty; // Approved, Rejected, NeedsRevision
        public string? Comments { get; set; }
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
        public string ApproversRole { get; set; } = string.Empty;

        // Navigation property
        public virtual Entities.WorkflowInstance? WorkflowInstance { get; set; }
    }

    /// <summary>Workflow transition audit trail</summary>
    public class WorkflowTransition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public string FromState { get; set; } = string.Empty;
        public string ToState { get; set; } = string.Empty;
        public string TriggeredBy { get; set; } = string.Empty; // UserId
        public DateTime TransitionDate { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
        public Dictionary<string, object>? ContextData { get; set; }

        // Navigation property
        public virtual Entities.WorkflowInstance? WorkflowInstance { get; set; }
    }

    /// <summary>Notification/Alert for workflow</summary>
    public class WorkflowNotification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public Guid TenantId { get; set; }
        public string NotificationType { get; set; } = string.Empty; // TaskAssigned, TaskDue, TaskOverdue, ApprovalPending, Escalation
        public string RecipientUserId { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty; // Email or user identifier
        public string Message { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical
        public bool IsSent { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public bool IsDelivered { get; set; } = false;
        public bool RequiresEmail { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public int DeliveryAttempts { get; set; } = 0;
        public string? DeliveryNote { get; set; }
        public string? DeliveryError { get; set; }

        // Navigation property
        public virtual Entities.WorkflowInstance? WorkflowInstance { get; set; }
    }

    /// <summary>
    /// Represents an escalation record for a workflow
    /// </summary>
    public class WorkflowEscalation
    {
        public int Id { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public Guid? TaskId { get; set; }
        public int EscalationLevel { get; set; }
        public string EscalationReason { get; set; } = string.Empty;
        public DateTime EscalatedAt { get; set; }
        public string? EscalatedToUserId { get; set; }
        public string? OriginalAssignee { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Acknowledged, Resolved
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
        public bool IsSlaEscalation { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties - use Entities versions
        public virtual Entities.WorkflowInstance? WorkflowInstance { get; set; }
        public virtual Entities.WorkflowTask? Task { get; set; }
    }

    /// <summary>
    /// User notification preferences
    /// </summary>
    public class UserNotificationPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = false;
        public bool PushEnabled { get; set; } = true;
        public string? EnabledNotificationTypes { get; set; } // Comma-separated list
        public string? QuietHoursStart { get; set; } // HH:mm format
        public string? QuietHoursEnd { get; set; }
        public bool DigestEnabled { get; set; } = false;
        public string DigestFrequency { get; set; } = "Daily"; // Daily, Weekly
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Audit log entry for compliance tracking
    /// </summary>
    public class AuditLogEntry
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangedByUserId { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? AdditionalData { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// HRIS Employee record for manager lookups
    /// </summary>
    public class HrisEmployee
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? ManagerId { get; set; }
        public string? DepartmentId { get; set; }
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
