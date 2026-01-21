using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Auto-reply rules for specific email patterns
/// </summary>
public class EmailAutoReplyRule : BaseEntity
{
    public Guid MailboxId { get; set; }
    
    [ForeignKey(nameof(MailboxId))]
    public virtual EmailMailbox? Mailbox { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Which classifications trigger this rule
    /// </summary>
    public EmailClassification[] TriggerClassifications { get; set; } = Array.Empty<EmailClassification>();
    
    /// <summary>
    /// Subject pattern (regex)
    /// </summary>
    [MaxLength(500)]
    public string? SubjectPattern { get; set; }
    
    /// <summary>
    /// Body pattern (regex)
    /// </summary>
    [MaxLength(1000)]
    public string? BodyPattern { get; set; }
    
    /// <summary>
    /// From email pattern (regex or exact)
    /// </summary>
    [MaxLength(500)]
    public string? FromPattern { get; set; }
    
    /// <summary>
    /// Action to take
    /// </summary>
    public AutoReplyAction Action { get; set; } = AutoReplyAction.CreateDraft;
    
    /// <summary>
    /// Template name or ID to use
    /// </summary>
    [MaxLength(100)]
    public string? TemplateName { get; set; }
    
    /// <summary>
    /// Static reply content (if not using template)
    /// </summary>
    public string? ReplyContent { get; set; }
    
    /// <summary>
    /// Use AI to generate reply based on context
    /// </summary>
    public bool UseAiGeneration { get; set; }
    
    /// <summary>
    /// AI prompt template for reply generation
    /// </summary>
    public string? AiPromptTemplate { get; set; }
    
    /// <summary>
    /// Priority for rule matching (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Schedule follow-up after (hours)
    /// </summary>
    public int? FollowUpAfterHours { get; set; }
    
    /// <summary>
    /// Maximum auto-replies per thread
    /// </summary>
    public int MaxAutoRepliesPerThread { get; set; } = 2;
}

public enum AutoReplyAction
{
    /// <summary>
    /// Create draft for human review
    /// </summary>
    CreateDraft = 0,
    
    /// <summary>
    /// Send immediately (for routine confirmations)
    /// </summary>
    SendImmediately = 1,
    
    /// <summary>
    /// Create task for human action
    /// </summary>
    CreateTask = 2,
    
    /// <summary>
    /// Forward to specific team/person
    /// </summary>
    Forward = 3,
    
    /// <summary>
    /// Mark as handled (auto-reply/OOO detection)
    /// </summary>
    MarkAsHandled = 4,
    
    /// <summary>
    /// Escalate to manager
    /// </summary>
    Escalate = 5,
    
    /// <summary>
    /// Ignore (spam, etc.)
    /// </summary>
    Ignore = 6
}
