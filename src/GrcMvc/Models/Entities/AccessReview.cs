using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// AM-11: Access Review Campaign
/// Represents a periodic review of user access rights within a tenant.
/// </summary>
[Table("AccessReviews")]
public class AccessReview : BaseEntity
{
    /// <summary>
    /// Tenant that owns this access review
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Unique code for the review (e.g., "AR-2026-Q1")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReviewCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the review campaign
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the review scope and purpose
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Review type: Quarterly, Annual, AdHoc, Privileged, Compliance
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReviewType { get; set; } = "Quarterly";

    /// <summary>
    /// Review scope: AllUsers, PrivilegedOnly, InactiveOnly, ByDepartment, ByRole
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Scope { get; set; } = "AllUsers";

    /// <summary>
    /// JSON filter criteria for the review scope
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? ScopeFilterJson { get; set; }

    /// <summary>
    /// Review status: Draft, Scheduled, InProgress, PendingApproval, Completed, Cancelled
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// When the review is scheduled to start
    /// </summary>
    public DateTime ScheduledStartDate { get; set; }

    /// <summary>
    /// When the review is due to be completed
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// When the review actually started
    /// </summary>
    public DateTime? ActualStartDate { get; set; }

    /// <summary>
    /// When the review was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// User ID assigned to conduct this review (reviewer)
    /// </summary>
    [MaxLength(450)]
    public string? ReviewerId { get; set; }

    /// <summary>
    /// User ID who approved the review results
    /// </summary>
    [MaxLength(450)]
    public string? ApprovedById { get; set; }

    /// <summary>
    /// Total number of items to review
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items reviewed
    /// </summary>
    public int ReviewedItems { get; set; }

    /// <summary>
    /// Number of items where access was approved/kept
    /// </summary>
    public int ApprovedItems { get; set; }

    /// <summary>
    /// Number of items where access was revoked
    /// </summary>
    public int RevokedItems { get; set; }

    /// <summary>
    /// Number of items pending remediation
    /// </summary>
    public int PendingRemediationItems { get; set; }

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercent { get; set; }

    /// <summary>
    /// Reviewer notes or summary
    /// </summary>
    [MaxLength(2000)]
    public string? ReviewerNotes { get; set; }

    /// <summary>
    /// Compliance framework this review satisfies (e.g., "SOC2", "ISO27001", "NCA")
    /// </summary>
    [MaxLength(100)]
    public string? ComplianceFramework { get; set; }

    /// <summary>
    /// Control reference (e.g., "AM-11")
    /// </summary>
    [MaxLength(20)]
    public string ControlReference { get; set; } = "AM-11";

    /// <summary>
    /// Reminder days before due date
    /// </summary>
    public int ReminderDays { get; set; } = 7;

    /// <summary>
    /// Whether reminder has been sent
    /// </summary>
    public bool ReminderSent { get; set; }

    /// <summary>
    /// When the last reminder was sent (for idempotency)
    /// </summary>
    public DateTime? LastReminderSentAt { get; set; }

    /// <summary>
    /// Next scheduled review date (for recurring reviews)
    /// </summary>
    public DateTime? NextReviewDate { get; set; }

    /// <summary>
    /// Recurrence pattern: None, Monthly, Quarterly, SemiAnnual, Annual
    /// </summary>
    [MaxLength(30)]
    public string RecurrencePattern { get; set; } = "Quarterly";

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<AccessReviewItem> Items { get; set; } = new List<AccessReviewItem>();
}

/// <summary>
/// AM-11: Individual item within an access review
/// Each item represents a user-role or user-permission combination to review.
/// </summary>
[Table("AccessReviewItems")]
public class AccessReviewItem : BaseEntity
{
    /// <summary>
    /// Parent access review
    /// </summary>
    public Guid AccessReviewId { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// User ID being reviewed
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's email for display
    /// </summary>
    [MaxLength(256)]
    public string? UserEmail { get; set; }

    /// <summary>
    /// User's full name for display
    /// </summary>
    [MaxLength(200)]
    public string? UserFullName { get; set; }

    /// <summary>
    /// Item type: Role, Permission, GroupMembership
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string ItemType { get; set; } = "Role";

    /// <summary>
    /// Role code or permission being reviewed
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AccessItem { get; set; } = string.Empty;

    /// <summary>
    /// Access item description
    /// </summary>
    [MaxLength(500)]
    public string? AccessItemDescription { get; set; }

    /// <summary>
    /// When this access was granted
    /// </summary>
    public DateTime? AccessGrantedDate { get; set; }

    /// <summary>
    /// Who granted this access
    /// </summary>
    [MaxLength(450)]
    public string? GrantedById { get; set; }

    /// <summary>
    /// User's last login date
    /// </summary>
    public DateTime? UserLastLoginDate { get; set; }

    /// <summary>
    /// Item status: Pending, Approved, Revoked, Flagged, Skipped
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Decision made: Keep, Revoke, Modify, Escalate
    /// </summary>
    [MaxLength(30)]
    public string? Decision { get; set; }

    /// <summary>
    /// User ID who made the decision
    /// </summary>
    [MaxLength(450)]
    public string? DecisionById { get; set; }

    /// <summary>
    /// When the decision was made
    /// </summary>
    public DateTime? DecisionDate { get; set; }

    /// <summary>
    /// Justification for the decision
    /// </summary>
    [MaxLength(1000)]
    public string? Justification { get; set; }

    /// <summary>
    /// Risk level: Low, Medium, High, Critical
    /// </summary>
    [MaxLength(20)]
    public string RiskLevel { get; set; } = "Low";

    /// <summary>
    /// Whether this is a privileged access item
    /// </summary>
    public bool IsPrivileged { get; set; }

    /// <summary>
    /// Flag for items needing attention
    /// </summary>
    public bool IsFlagged { get; set; }

    /// <summary>
    /// Flag reason
    /// </summary>
    [MaxLength(500)]
    public string? FlagReason { get; set; }

    /// <summary>
    /// Remediation status: None, Required, InProgress, Completed
    /// </summary>
    [MaxLength(30)]
    public string RemediationStatus { get; set; } = "None";

    /// <summary>
    /// Remediation notes
    /// </summary>
    [MaxLength(1000)]
    public string? RemediationNotes { get; set; }

    /// <summary>
    /// When remediation was completed
    /// </summary>
    public DateTime? RemediationCompletedDate { get; set; }

    // Navigation property
    public virtual AccessReview AccessReview { get; set; } = null!;
}

/// <summary>
/// Access review status values
/// </summary>
public static class AccessReviewStatus
{
    public const string Draft = "Draft";
    public const string Scheduled = "Scheduled";
    public const string InProgress = "InProgress";
    public const string PendingApproval = "PendingApproval";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Overdue = "Overdue";
}

/// <summary>
/// Access review item status values
/// </summary>
public static class AccessReviewItemStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Revoked = "Revoked";
    public const string Flagged = "Flagged";
    public const string Skipped = "Skipped";
}

/// <summary>
/// Access review decision values
/// </summary>
public static class AccessReviewDecision
{
    public const string Keep = "Keep";
    public const string Revoke = "Revoke";
    public const string Modify = "Modify";
    public const string Escalate = "Escalate";
}

/// <summary>
/// Access review scope types
/// </summary>
public static class AccessReviewScope
{
    public const string AllUsers = "AllUsers";
    public const string PrivilegedOnly = "PrivilegedOnly";
    public const string InactiveOnly = "InactiveOnly";
    public const string ByDepartment = "ByDepartment";
    public const string ByRole = "ByRole";
    public const string Custom = "Custom";
}

/// <summary>
/// Access review type values
/// </summary>
public static class AccessReviewType
{
    public const string Quarterly = "Quarterly";
    public const string Annual = "Annual";
    public const string AdHoc = "AdHoc";
    public const string Privileged = "Privileged";
    public const string Compliance = "Compliance";
    public const string Onboarding = "Onboarding";
}
