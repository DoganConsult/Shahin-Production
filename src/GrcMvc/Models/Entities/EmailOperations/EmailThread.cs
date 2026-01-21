using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Represents an email conversation thread
/// </summary>
public class EmailThread : BaseEntity
{
    /// <summary>
    /// Microsoft Graph Conversation ID
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ConversationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Subject line of the thread
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Original sender email
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FromEmail { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? FromName { get; set; }
    
    /// <summary>
    /// AI-determined classification
    /// </summary>
    public EmailClassification Classification { get; set; } = EmailClassification.Unclassified;
    
    /// <summary>
    /// AI confidence score (0-100)
    /// </summary>
    public int ClassificationConfidence { get; set; }
    
    /// <summary>
    /// Current status of this thread
    /// </summary>
    public EmailThreadStatus Status { get; set; } = EmailThreadStatus.New;
    
    /// <summary>
    /// Priority level
    /// </summary>
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    
    /// <summary>
    /// Assigned team member (if any)
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    
    [MaxLength(200)]
    public string? AssignedToUserName { get; set; }
    
    /// <summary>
    /// Linked mailbox
    /// </summary>
    public Guid MailboxId { get; set; }
    
    [ForeignKey(nameof(MailboxId))]
    public virtual EmailMailbox? Mailbox { get; set; }
    
    /// <summary>
    /// When first message was received
    /// </summary>
    public DateTime ReceivedAt { get; set; }
    
    /// <summary>
    /// When we first responded
    /// </summary>
    public DateTime? FirstResponseAt { get; set; }
    
    /// <summary>
    /// When thread was resolved/closed
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
    
    /// <summary>
    /// SLA deadline for first response
    /// </summary>
    public DateTime? SlaFirstResponseDeadline { get; set; }
    
    /// <summary>
    /// SLA deadline for resolution
    /// </summary>
    public DateTime? SlaResolutionDeadline { get; set; }
    
    public bool SlaFirstResponseBreached { get; set; }
    public bool SlaResolutionBreached { get; set; }
    
    /// <summary>
    /// Next follow-up scheduled
    /// </summary>
    public DateTime? NextFollowUpAt { get; set; }
    
    public int FollowUpCount { get; set; }
    
    /// <summary>
    /// Extracted entities/data from AI
    /// </summary>
    public string? ExtractedDataJson { get; set; }
    
    /// <summary>
    /// Internal notes
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// Tags for filtering
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    // Navigation
    public virtual ICollection<EmailMessage> Messages { get; set; } = new List<EmailMessage>();
    public virtual ICollection<EmailTask> Tasks { get; set; } = new List<EmailTask>();
}

public enum EmailClassification
{
    Unclassified = 0,
    
    // Support
    TechnicalSupport = 10,
    BillingInquiry = 11,
    AccountIssue = 12,
    FeatureRequest = 13,
    BugReport = 14,
    
    // Sales
    QuoteRequest = 20,
    DemoRequest = 21,
    PricingInquiry = 22,
    PartnershipInquiry = 23,
    
    // Operations
    ContractQuestion = 30,
    ComplianceQuery = 31,
    AuditRequest = 32,
    DocumentRequest = 33,
    
    // HR/Admin
    JobApplication = 40,
    VendorInquiry = 41,
    MediaInquiry = 42,
    
    // Sensitive (requires human)
    Complaint = 50,
    Legal = 51,
    Escalation = 52,
    Executive = 53,
    
    // Auto-handle
    AutoReply = 90,
    OutOfOffice = 91,
    Newsletter = 92,
    Spam = 99
}

public enum EmailThreadStatus
{
    New = 0,
    AwaitingClassification = 1,
    AwaitingAssignment = 2,
    Assigned = 3,
    InProgress = 4,
    AwaitingCustomerReply = 5,
    DraftPending = 6,
    FollowUpScheduled = 7,
    Resolved = 8,
    Closed = 9,
    Spam = 99
}

public enum EmailPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4,
    Critical = 5
}
