using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Individual email message within a thread
/// </summary>
public class EmailMessage : BaseEntity
{
    /// <summary>
    /// Microsoft Graph Message ID
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string GraphMessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Internet Message ID (for threading)
    /// </summary>
    [MaxLength(500)]
    public string? InternetMessageId { get; set; }
    
    public Guid ThreadId { get; set; }
    
    [ForeignKey(nameof(ThreadId))]
    public virtual EmailThread? Thread { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FromEmail { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? FromName { get; set; }
    
    [MaxLength(1000)]
    public string? ToRecipients { get; set; }
    
    [MaxLength(1000)]
    public string? CcRecipients { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Plain text body preview (first 500 chars)
    /// </summary>
    [MaxLength(500)]
    public string? BodyPreview { get; set; }
    
    /// <summary>
    /// Full body content (HTML or text)
    /// </summary>
    public string? BodyContent { get; set; }
    
    /// <summary>
    /// Body content type (html/text)
    /// </summary>
    [MaxLength(20)]
    public string BodyContentType { get; set; } = "text";
    
    /// <summary>
    /// Direction: Inbound (from customer) or Outbound (our reply)
    /// </summary>
    public EmailDirection Direction { get; set; } = EmailDirection.Inbound;
    
    /// <summary>
    /// Status of this specific message
    /// </summary>
    public EmailMessageStatus Status { get; set; } = EmailMessageStatus.Received;
    
    /// <summary>
    /// If this is an AI-generated draft
    /// </summary>
    public bool IsAiGenerated { get; set; }
    
    /// <summary>
    /// AI prompt used to generate (for auditing)
    /// </summary>
    public string? AiPromptUsed { get; set; }
    
    /// <summary>
    /// Who approved/sent this message
    /// </summary>
    public Guid? ApprovedByUserId { get; set; }
    
    [MaxLength(200)]
    public string? ApprovedByUserName { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public DateTime ReceivedAt { get; set; }
    
    public bool HasAttachments { get; set; }
    
    /// <summary>
    /// JSON array of attachment info
    /// </summary>
    public string? AttachmentsJson { get; set; }
    
    /// <summary>
    /// Importance from Graph (low/normal/high)
    /// </summary>
    [MaxLength(20)]
    public string Importance { get; set; } = "normal";
    
    public bool IsRead { get; set; }
    
    // Navigation
    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}

public enum EmailDirection
{
    Inbound = 0,
    Outbound = 1
}

public enum EmailMessageStatus
{
    Received = 0,
    Processing = 1,
    DraftCreated = 2,
    AwaitingApproval = 3,
    Approved = 4,
    Sent = 5,
    Failed = 6,
    Ignored = 7
}
