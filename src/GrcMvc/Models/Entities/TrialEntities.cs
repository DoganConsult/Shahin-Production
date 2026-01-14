using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Trial signup record - captures initial interest
    /// </summary>
    [Table("TrialSignups")]
    public class TrialSignup : BaseEntity
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(255)]
        public string? CompanyName { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Sector { get; set; }

        [MaxLength(50)]
        public string Source { get; set; } = "website";

        [MaxLength(50)]
        public string? ReferralCode { get; set; }

        [MaxLength(255)]
        public string? ActivationToken { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "pending"; // pending, activated, provisioned, expired

        public Guid? TenantId { get; set; }

        public DateTime? ActivatedAt { get; set; }

        public DateTime? ProvisionedAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Trial extension record
    /// </summary>
    [Table("TrialExtensions")]
    public class TrialExtension : BaseEntity
    {
        public Guid TenantId { get; set; }

        public int DaysAdded { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime PreviousEndDate { get; set; }

        public DateTime NewEndDate { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        [MaxLength(100)]
        public string? ApprovalMethod { get; set; } // auto, admin, request
    }

    /// <summary>
    /// Trial email log - tracks nurture emails sent
    /// </summary>
    [Table("TrialEmailLogs")]
    public class TrialEmailLog : BaseEntity
    {
        public Guid TenantId { get; set; }

        [MaxLength(50)]
        public string EmailType { get; set; } = string.Empty; // Welcome, Nudge24h, etc.

        [MaxLength(255)]
        public string SentTo { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } // sent, delivered, opened, clicked, bounced

        public DateTime? OpenedAt { get; set; }

        public DateTime? ClickedAt { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Ecosystem connection between tenant and partners
    /// </summary>
    [Table("EcosystemConnections")]
    public class EcosystemConnection : BaseEntity
    {
        public Guid TenantId { get; set; }

        [MaxLength(50)]
        public string? PartnerType { get; set; } // consultant, auditor, vendor, regulator

        public Guid? PartnerId { get; set; }

        [MaxLength(255)]
        public string? PartnerEmail { get; set; }

        [MaxLength(255)]
        public string? PartnerName { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }

        public string? SharedDataTypesJson { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } // pending, approved, rejected, revoked

        public DateTime? RequestedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public int InteractionsCount { get; set; }

        public DateTime? LastInteractionAt { get; set; }
    }

    /// <summary>
    /// Team activity log for trial collaboration tracking
    /// </summary>
    [Table("TeamActivities")]
    public class TeamActivity : BaseEntity
    {
        public Guid TenantId { get; set; }

        public Guid UserId { get; set; }

        public Guid? TargetUserId { get; set; }

        [MaxLength(100)]
        public string ActivityType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? MetadataJson { get; set; }

        [MaxLength(50)]
        public string? Module { get; set; } // compliance, risk, audit, etc.

        [MaxLength(50)]
        public string? Action { get; set; } // create, update, delete, view, share

        public Guid? ResourceId { get; set; }

        [MaxLength(100)]
        public new string? ResourceType { get; set; }
    }

    /// <summary>
    /// Ecosystem partner registry
    /// </summary>
    [Table("EcosystemPartners")]
    public class EcosystemPartner : BaseEntity
    {
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // consultant, auditor, vendor, regulator

        [MaxLength(100)]
        public string? Sector { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? ServicesJson { get; set; }

        public string? CertificationsJson { get; set; }

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(255)]
        public string? LogoUrl { get; set; }

        public double Rating { get; set; }

        public int ReviewCount { get; set; }

        public int ConnectionsCount { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }
    }

    /// <summary>
    /// Workspace for team collaboration
    /// </summary>
    [Table("CollaborationWorkspaces")]
    public class CollaborationWorkspace : BaseEntity
    {
        public Guid TenantId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "general"; // general, project, audit, compliance

        public Guid? FrameworkId { get; set; }

        public Guid? ProjectId { get; set; }

        public string? MembersJson { get; set; }

        public string? SettingsJson { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ArchivedAt { get; set; }
    }

    /// <summary>
    /// Collaboration item (shared task, document, discussion)
    /// </summary>
    [Table("CollaborationItems")]
    public class CollaborationItem : BaseEntity
    {
        public Guid WorkspaceId { get; set; }

        public Guid TenantId { get; set; }

        [MaxLength(50)]
        public string ItemType { get; set; } = string.Empty; // task, document, discussion, note

        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } // open, in_progress, completed, archived

        [MaxLength(50)]
        public string? Priority { get; set; } // low, medium, high, critical

        public Guid? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public string? TagsJson { get; set; }

        public string? AttachmentsJson { get; set; }

        public int CommentsCount { get; set; }

        public int ReactionsCount { get; set; }

        public Guid? ParentItemId { get; set; }
    }

    /// <summary>
    /// Comment on collaboration item
    /// </summary>
    [Table("CollaborationComments")]
    public class CollaborationComment : BaseEntity
    {
        public Guid ItemId { get; set; }

        public Guid TenantId { get; set; }

        public Guid AuthorId { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? MentionsJson { get; set; }

        public string? AttachmentsJson { get; set; }

        public Guid? ParentCommentId { get; set; }

        public bool IsEdited { get; set; }

        public DateTime? EditedAt { get; set; }
    }
}
