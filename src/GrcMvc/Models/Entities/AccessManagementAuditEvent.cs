using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Immutable audit event for Access Management controls (AM-01 through AM-12).
    /// Stored in append-only table for compliance and audit evidence.
    /// </summary>
    [Table("AccessManagementAuditEvents")]
    public class AccessManagementAuditEvent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Event identifier in format "AME-{timestamp}-{guid}" for traceability.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Event type code (e.g., AM01_USER_CREATED, AM03_ROLE_ASSIGNED).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// AM control number this event relates to (e.g., "AM-01", "AM-03").
        /// </summary>
        [MaxLength(10)]
        public string ControlNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tenant context for multi-tenant isolation.
        /// Null for platform-level events.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// User who performed the action (actor).
        /// Null for system-initiated events.
        /// </summary>
        public Guid? ActorUserId { get; set; }

        /// <summary>
        /// Email of the actor for display purposes.
        /// </summary>
        [MaxLength(256)]
        public string? ActorEmail { get; set; }

        /// <summary>
        /// User affected by the action (target).
        /// </summary>
        public Guid? TargetUserId { get; set; }

        /// <summary>
        /// Email of the target user for display purposes.
        /// </summary>
        [MaxLength(256)]
        public string? TargetEmail { get; set; }

        /// <summary>
        /// Entity type affected (e.g., "User", "Tenant", "Role", "Invitation").
        /// </summary>
        [MaxLength(50)]
        public string? AffectedEntityType { get; set; }

        /// <summary>
        /// ID of the affected entity.
        /// </summary>
        public Guid? AffectedEntityId { get; set; }

        /// <summary>
        /// Human-readable description of the event.
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Event severity: Low, Medium, High, Critical.
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "Low";

        /// <summary>
        /// Additional event details in JSON format.
        /// Contains role changes, status transitions, etc.
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? DetailsJson { get; set; }

        /// <summary>
        /// Previous value for change events (e.g., old role, old status).
        /// </summary>
        [MaxLength(200)]
        public string? PreviousValue { get; set; }

        /// <summary>
        /// New value for change events (e.g., new role, new status).
        /// </summary>
        [MaxLength(200)]
        public string? NewValue { get; set; }

        /// <summary>
        /// IP address of the request origin.
        /// </summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent string from the request.
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Correlation ID for linking related events across requests.
        /// </summary>
        [MaxLength(50)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Session ID if applicable.
        /// </summary>
        public Guid? SessionId { get; set; }

        /// <summary>
        /// Whether the action was successful.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if the action failed.
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when the event occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Source system or service that generated the event.
        /// </summary>
        [MaxLength(100)]
        public string Source { get; set; } = "GrcMvc";

        /// <summary>
        /// API key ID if the action was performed via API.
        /// </summary>
        public Guid? ApiKeyId { get; set; }

        /// <summary>
        /// Whether MFA was verified for this action.
        /// </summary>
        public bool MfaVerified { get; set; } = false;

        /// <summary>
        /// Creates a new audit event with generated EventId.
        /// </summary>
        public static AccessManagementAuditEvent Create(
            string eventType,
            string description,
            Guid? tenantId = null,
            Guid? actorUserId = null,
            Guid? targetUserId = null)
        {
            return new AccessManagementAuditEvent
            {
                EventId = $"AME-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}",
                EventType = eventType,
                ControlNumber = Constants.AuditEventTypes.GetControlNumber(eventType),
                Severity = Constants.AuditEventTypes.GetSeverity(eventType),
                TenantId = tenantId,
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                Description = description,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
