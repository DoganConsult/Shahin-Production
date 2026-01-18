using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks user consent for Terms of Service, Privacy Policy, and other legal agreements
    /// </summary>
    public class UserConsent : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Type of consent: TermsOfService, PrivacyPolicy, DataProcessing, Marketing, etc.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ConsentType { get; set; } = string.Empty;

        /// <summary>
        /// Version of the document that was accepted
        /// </summary>
        [Required]
        [StringLength(50)]
        public string DocumentVersion { get; set; } = "1.0";

        /// <summary>
        /// Whether consent was granted
        /// </summary>
        public bool IsGranted { get; set; } = false;

        /// <summary>
        /// Timestamp when consent was recorded
        /// </summary>
        public DateTime ConsentedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP address from which consent was given
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (browser) info
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// If consent was withdrawn, when
        /// </summary>
        public DateTime? WithdrawnAt { get; set; }

        /// <summary>
        /// Reason for withdrawal if applicable
        /// </summary>
        [StringLength(500)]
        public string? WithdrawalReason { get; set; }

        /// <summary>
        /// Hash of the document content at time of consent (for audit trail)
        /// </summary>
        [StringLength(256)]
        public string? DocumentHash { get; set; }
    }

    /// <summary>
    /// Stores legal document versions (ToS, Privacy Policy, etc.)
    /// </summary>
    public class LegalDocument : BaseEntity
    {
        /// <summary>
        /// Document type: TermsOfService, PrivacyPolicy, DataProcessingAgreement, etc.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>
        /// Version number
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Document title
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Arabic title
        /// </summary>
        [StringLength(255)]
        public string? TitleAr { get; set; }

        /// <summary>
        /// Full content in English (HTML or Markdown)
        /// </summary>
        [Required]
        public string ContentEn { get; set; } = string.Empty;

        /// <summary>
        /// Full content in Arabic (HTML or Markdown)
        /// </summary>
        public string? ContentAr { get; set; }

        /// <summary>
        /// Summary for display
        /// </summary>
        [StringLength(1000)]
        public string? Summary { get; set; }

        /// <summary>
        /// When this version becomes effective
        /// </summary>
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this is the current active version
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether acceptance is mandatory for registration
        /// </summary>
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// Hash of the content for integrity verification
        /// </summary>
        [StringLength(256)]
        public string? ContentHash { get; set; }
    }

    /// <summary>
    /// Support chat/agent conversation history
    /// </summary>
    public class SupportConversation : BaseEntity
    {
        public Guid? TenantId { get; set; }

        public string? UserId { get; set; }

        /// <summary>
        /// Session identifier for anonymous users
        /// </summary>
        [StringLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// Conversation status: Active, Resolved, Escalated, Closed
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Category of support request
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Subject/topic of conversation
        /// </summary>
        [StringLength(255)]
        public string? Subject { get; set; }

        /// <summary>
        /// Priority: Low, Medium, High, Urgent
        /// </summary>
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Whether AI agent is handling (vs human)
        /// </summary>
        public bool IsAgentHandled { get; set; } = true;

        /// <summary>
        /// Human agent assigned if escalated
        /// </summary>
        public string? AssignedAgentId { get; set; }

        /// <summary>
        /// When conversation started
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When conversation was resolved/closed
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// User satisfaction rating (1-5)
        /// </summary>
        public int? SatisfactionRating { get; set; }

        /// <summary>
        /// Feedback from user
        /// </summary>
        [StringLength(1000)]
        public string? Feedback { get; set; }

        // Navigation
        public virtual ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
    }

    /// <summary>
    /// Individual messages in a support conversation
    /// </summary>
    public class SupportMessage : BaseEntity
    {
        [Required]
        public Guid ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual SupportConversation Conversation { get; set; } = null!;

        /// <summary>
        /// Who sent: User, Agent, System
        /// </summary>
        [Required]
        [StringLength(20)]
        public string SenderType { get; set; } = "User";

        /// <summary>
        /// Sender identifier
        /// </summary>
        [StringLength(100)]
        public string? SenderId { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Message type: Text, Link, Attachment, Action
        /// </summary>
        [StringLength(50)]
        public string MessageType { get; set; } = "Text";

        /// <summary>
        /// Attachments or action metadata as JSON
        /// </summary>
        public string? MetadataJson { get; set; }

        /// <summary>
        /// When message was sent
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When message was read by recipient
        /// </summary>
        public DateTime? ReadAt { get; set; }
    }
}
