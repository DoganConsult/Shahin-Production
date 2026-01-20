using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Support Ticket - Platform admin ticketing system for user issues
    /// </summary>
    public class SupportTicket : BaseEntity
    {
        /// <summary>
        /// Ticket number (e.g., TKT-2026-001234)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tenant ID (if issue is tenant-specific)
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// User ID who created the ticket
        /// </summary>
        [StringLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// User email (for non-authenticated users)
        /// </summary>
        [StringLength(255)]
        public string? UserEmail { get; set; }

        /// <summary>
        /// Subject/title of the ticket
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the issue
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category: Technical, Billing, Account, Feature Request, Bug Report, etc.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "Technical";

        /// <summary>
        /// Priority: Low, Medium, High, Urgent, Critical
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Status: New, Open, In Progress, Waiting for Customer, Resolved, Closed, Cancelled
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "New";

        /// <summary>
        /// Platform admin assigned to handle this ticket
        /// </summary>
        [StringLength(450)]
        public string? AssignedToUserId { get; set; }

        /// <summary>
        /// Platform admin who created/resolved the ticket
        /// </summary>
        [StringLength(450)]
        public string? CreatedByAdminId { get; set; }

        /// <summary>
        /// Resolution notes (when resolved)
        /// </summary>
        [Column(TypeName = "text")]
        public string? ResolutionNotes { get; set; }

        /// <summary>
        /// When ticket was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When ticket was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// When ticket was resolved
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// When ticket was closed
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// SLA deadline (based on priority)
        /// </summary>
        public DateTime? SlaDeadline { get; set; }

        /// <summary>
        /// Whether SLA was breached
        /// </summary>
        public bool SlaBreached { get; set; }

        /// <summary>
        /// Tags for categorization
        /// </summary>
        [StringLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Related entity type (e.g., "Risk", "Control", "Assessment")
        /// </summary>
        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Related entity ID
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Customer satisfaction rating (1-5)
        /// </summary>
        public int? SatisfactionRating { get; set; }

        /// <summary>
        /// Customer feedback
        /// </summary>
        [Column(TypeName = "text")]
        public string? Feedback { get; set; }

        /// <summary>
        /// Internal notes (visible only to platform admins)
        /// </summary>
        [Column(TypeName = "text")]
        public string? InternalNotes { get; set; }

        // Navigation properties
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual ApplicationUser? AssignedToUser { get; set; }

        public virtual ICollection<SupportTicketComment> Comments { get; set; } = new List<SupportTicketComment>();
        public virtual ICollection<SupportTicketAttachment> Attachments { get; set; } = new List<SupportTicketAttachment>();
        public virtual ICollection<SupportTicketHistory> History { get; set; } = new List<SupportTicketHistory>();
    }

    /// <summary>
    /// Comments on support tickets
    /// </summary>
    public class SupportTicketComment : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual SupportTicket Ticket { get; set; } = null!;

        /// <summary>
        /// User ID who made the comment
        /// </summary>
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Comment content
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Whether comment is internal (only visible to admins)
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// When comment was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }

    /// <summary>
    /// Attachments on support tickets
    /// </summary>
    public class SupportTicketAttachment : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual SupportTicket Ticket { get; set; } = null!;

        /// <summary>
        /// File name
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File path or URL
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// MIME type
        /// </summary>
        [StringLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Uploaded by user ID
        /// </summary>
        [StringLength(450)]
        public string? UploadedByUserId { get; set; }

        /// <summary>
        /// When uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UploadedByUserId")]
        public virtual ApplicationUser? UploadedByUser { get; set; }
    }

    /// <summary>
    /// History/audit trail for support tickets
    /// </summary>
    public class SupportTicketHistory : BaseEntity
    {
        [Required]
        public Guid TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual SupportTicket Ticket { get; set; } = null!;

        /// <summary>
        /// Action performed
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Previous value
        /// </summary>
        [StringLength(500)]
        public string? PreviousValue { get; set; }

        /// <summary>
        /// New value
        /// </summary>
        [StringLength(500)]
        public string? NewValue { get; set; }

        /// <summary>
        /// User who performed the action
        /// </summary>
        [StringLength(450)]
        public string? ChangedByUserId { get; set; }

        /// <summary>
        /// When change occurred
        /// </summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional notes
        /// </summary>
        [Column(TypeName = "text")]
        public string? Notes { get; set; }

        [ForeignKey("ChangedByUserId")]
        public virtual ApplicationUser? ChangedByUser { get; set; }
    }
}
