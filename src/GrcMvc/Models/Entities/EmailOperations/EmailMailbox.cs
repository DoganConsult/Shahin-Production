using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.EmailOperations;

/// <summary>
/// Represents a monitored mailbox (e.g., support@shahin.ai, info@doganconsult.com)
/// </summary>
public class EmailMailbox : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string EmailAddress { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Company/Brand this mailbox belongs to (Shahin, DoganConsult, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;
    
    /// <summary>
    /// Purpose: Support, Sales, Operations, Info, etc.
    /// </summary>
    [MaxLength(50)]
    public string Purpose { get; set; } = "General";
    
    /// <summary>
    /// Microsoft Graph User ID or Shared Mailbox ID
    /// </summary>
    [MaxLength(100)]
    public string? GraphUserId { get; set; }
    
    /// <summary>
    /// OAuth Client ID for this mailbox (from Azure AD App Registration)
    /// </summary>
    [MaxLength(100)]
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Encrypted Client Secret
    /// </summary>
    public string? EncryptedClientSecret { get; set; }
    
    /// <summary>
    /// Azure AD Tenant ID for Microsoft 365 (not to be confused with app TenantId)
    /// </summary>
    [MaxLength(100)]
    public string? AzureTenantId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Auto-reply enabled for routine messages
    /// </summary>
    public bool AutoReplyEnabled { get; set; } = false;
    
    /// <summary>
    /// Create draft for review instead of sending directly
    /// </summary>
    public bool DraftModeDefault { get; set; } = true;
    
    /// <summary>
    /// Default SLA hours (used in quick add)
    /// </summary>
    public int DefaultSlaHours { get; set; } = 24;
    
    /// <summary>
    /// SLA hours for first response
    /// </summary>
    public int SlaFirstResponseHours { get; set; } = 24;
    
    /// <summary>
    /// SLA hours for resolution
    /// </summary>
    public int SlaResolutionHours { get; set; } = 72;
    
    /// <summary>
    /// Webhook subscription ID from Graph API
    /// </summary>
    [MaxLength(200)]
    public string? WebhookSubscriptionId { get; set; }
    
    public DateTime? WebhookExpiresAt { get; set; }
    
    public DateTime? LastSyncAt { get; set; }
    
    // Navigation
    public virtual ICollection<EmailThread> Threads { get; set; } = new List<EmailThread>();
    public virtual ICollection<EmailAutoReplyRule> AutoReplyRules { get; set; } = new List<EmailAutoReplyRule>();
}

/// <summary>
/// Known brands/companies for email operations
/// </summary>
public static class EmailBrands
{
    public const string Shahin = "Shahin";
    public const string DoganConsult = "DoganConsult";
}
