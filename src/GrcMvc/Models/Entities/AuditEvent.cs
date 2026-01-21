using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Append-only audit trail for all significant events in the system.
    /// Immutable: once created, never modified.
    /// Enables compliance reporting and regulatory audit.
    /// </summary>
    public class AuditEvent : BaseEntity
    {
        public Guid TenantId { get; set; }

        /// <summary>
        /// Unique event identifier for idempotency
        /// Format: evt-{guid}
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Event type: TenantActivated, OnboardingCompleted, ScopeGenerated, PlanCreated,
        ///             AssessmentCreated, EvidenceSubmitted, EvidenceScored,
        ///             WorkflowStepCompleted, RequirementStatusUpdated
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Correlation ID linking related events (planId, assessmentId, etc.)
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// What entity was affected: Tenant, Plan, Assessment, Evidence, Requirement, etc.
        /// </summary>
        public string AffectedEntityType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the affected entity
        /// </summary>
        public string AffectedEntityId { get; set; } = string.Empty;

        /// <summary>
        /// Who triggered the event (user ID or "SYSTEM")
        /// </summary>
        public string Actor { get; set; } = "SYSTEM";

        /// <summary>
        /// What action was performed: Create, Update, Delete, Approve, Reject, etc.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Complete event payload as JSON
        /// Stores all details needed to replay or audit the event
        /// </summary>
        public string PayloadJson { get; set; } = string.Empty;

        /// <summary>
        /// Status: Success, Partial, Failed
        /// </summary>
        public string Status { get; set; } = "Success";

        /// <summary>
        /// Error message if event failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Severity level: Info, Warning, Error, Critical
        /// </summary>
        public string Severity { get; set; } = "Info";

        /// <summary>
        /// IP address of the user who triggered the event
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Human-readable description of what happened
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// User ID (Identity) who triggered the event
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Alias for EventTimestamp for compatibility
        /// </summary>
        [NotMapped]
        public new DateTime CreatedAt
        {
            get => EventTimestamp;
            set => EventTimestamp = value;
        }

        /// <summary>
        /// Alias for EventTimestamp (Timestamp property)
        /// </summary>
        [NotMapped]
        public DateTime Timestamp
        {
            get => EventTimestamp;
            set => EventTimestamp = value;
        }

        /// <summary>
        /// Alias for ErrorMessage for diagnostic compatibility
        /// </summary>
        [NotMapped]
        public string? Details
        {
            get => ErrorMessage;
            set => ErrorMessage = value ?? string.Empty;
        }

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
