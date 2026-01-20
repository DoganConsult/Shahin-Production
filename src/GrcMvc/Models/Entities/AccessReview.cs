using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// AM-11: Access Review entity for periodic user access certification.
    /// </summary>
    [Table("AccessReviews")]
    public class AccessReview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tenant this review belongs to.
        /// </summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Review name/title.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Review type: Monthly, Quarterly, Annual, Ad-hoc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReviewType { get; set; } = "Quarterly";

        /// <summary>
        /// Current status: Draft, InProgress, PendingCompletion, Completed, Cancelled.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Description/scope of the review.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// User who initiated the review.
        /// </summary>
        public Guid InitiatedBy { get; set; }

        /// <summary>
        /// When the review was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the review was started.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Deadline for completing the review.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// When the review was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// User who marked the review as complete.
        /// </summary>
        public Guid? CompletedBy { get; set; }

        /// <summary>
        /// Total number of items in the review.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Number of items reviewed.
        /// </summary>
        public int ReviewedItems { get; set; }

        /// <summary>
        /// Number of items certified (access maintained).
        /// </summary>
        public int CertifiedItems { get; set; }

        /// <summary>
        /// Number of items revoked.
        /// </summary>
        public int RevokedItems { get; set; }

        /// <summary>
        /// Number of items modified.
        /// </summary>
        public int ModifiedItems { get; set; }

        /// <summary>
        /// When the last reminder was sent (for idempotent reminders).
        /// </summary>
        public DateTime? LastReminderSentAt { get; set; }

        /// <summary>
        /// Number of reminders sent.
        /// </summary>
        public int ReminderCount { get; set; }

        /// <summary>
        /// Completion percentage.
        /// </summary>
        [NotMapped]
        public double CompletionPercentage =>
            TotalItems > 0 ? (double)ReviewedItems / TotalItems * 100 : 0;

        /// <summary>
        /// Whether the review is overdue.
        /// </summary>
        [NotMapped]
        public bool IsOverdue =>
            DueDate.HasValue && DateTime.UtcNow > DueDate &&
            Status != "Completed" && Status != "Cancelled";

        /// <summary>
        /// Navigation property to review items.
        /// </summary>
        public virtual ICollection<AccessReviewItem> Items { get; set; } = new List<AccessReviewItem>();

        /// <summary>
        /// Navigation property to tenant.
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }
    }

    /// <summary>
    /// AM-11: Individual access review item for a specific user.
    /// </summary>
    [Table("AccessReviewItems")]
    public class AccessReviewItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Parent review.
        /// </summary>
        [Required]
        public Guid ReviewId { get; set; }

        /// <summary>
        /// User being reviewed.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// User's email for display.
        /// </summary>
        [MaxLength(256)]
        public string? UserEmail { get; set; }

        /// <summary>
        /// User's display name.
        /// </summary>
        [MaxLength(200)]
        public string? UserDisplayName { get; set; }

        /// <summary>
        /// User's current role being reviewed.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CurrentRole { get; set; } = string.Empty;

        /// <summary>
        /// User's current status.
        /// </summary>
        [MaxLength(50)]
        public string? UserStatus { get; set; }

        /// <summary>
        /// When the user last logged in.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Days since last activity.
        /// </summary>
        public int? InactiveDays { get; set; }

        /// <summary>
        /// Why this user was included in the review.
        /// </summary>
        [MaxLength(200)]
        public string? InclusionReason { get; set; }

        /// <summary>
        /// Review decision: Pending, Certified, Revoked, Modified.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Decision { get; set; } = "Pending";

        /// <summary>
        /// New role if decision is Modified.
        /// </summary>
        [MaxLength(100)]
        public string? NewRole { get; set; }

        /// <summary>
        /// Justification for the decision.
        /// </summary>
        [MaxLength(500)]
        public string? Justification { get; set; }

        /// <summary>
        /// User who reviewed this item.
        /// </summary>
        public Guid? ReviewedBy { get; set; }

        /// <summary>
        /// When the item was reviewed.
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Whether the decision has been executed.
        /// </summary>
        public bool IsExecuted { get; set; }

        /// <summary>
        /// When the decision was executed.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Error message if execution failed.
        /// </summary>
        [MaxLength(500)]
        public string? ExecutionError { get; set; }

        /// <summary>
        /// Navigation property to parent review.
        /// </summary>
        [ForeignKey(nameof(ReviewId))]
        public virtual AccessReview? Review { get; set; }
    }
}
