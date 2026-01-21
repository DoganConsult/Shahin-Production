using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// Service for Microsoft Graph API email operations
/// </summary>
public interface IMicrosoftGraphEmailService
{
    /// <summary>
    /// Get access token for a mailbox using Client Credentials flow
    /// </summary>
    Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret);
    
    /// <summary>
    /// Fetch new messages from mailbox
    /// </summary>
    Task<List<GraphEmailMessage>> GetMessagesAsync(
        string accessToken, 
        string mailboxId, 
        DateTime? since = null, 
        string? folderId = null,
        int top = 50);
    
    /// <summary>
    /// Get a specific message with full body
    /// </summary>
    Task<GraphEmailMessage?> GetMessageAsync(string accessToken, string mailboxId, string messageId);
    
    /// <summary>
    /// Get attachments for a message
    /// </summary>
    Task<List<GraphAttachment>> GetAttachmentsAsync(string accessToken, string mailboxId, string messageId);
    
    /// <summary>
    /// Download attachment content
    /// </summary>
    Task<byte[]> DownloadAttachmentAsync(string accessToken, string mailboxId, string messageId, string attachmentId);
    
    /// <summary>
    /// Create a draft reply to a message
    /// </summary>
    Task<GraphEmailMessage> CreateReplyDraftAsync(
        string accessToken, 
        string mailboxId, 
        string messageId, 
        string replyBody,
        bool replyAll = false);
    
    /// <summary>
    /// Send a draft message
    /// </summary>
    Task SendDraftAsync(string accessToken, string mailboxId, string draftId);
    
    /// <summary>
    /// Send a new message directly
    /// </summary>
    Task SendMessageAsync(
        string accessToken, 
        string mailboxId, 
        string toEmail, 
        string subject, 
        string body,
        bool isHtml = true,
        List<string>? ccEmails = null);
    
    /// <summary>
    /// Send reply directly (without creating draft first)
    /// </summary>
    Task SendReplyAsync(
        string accessToken, 
        string mailboxId, 
        string messageId, 
        string replyBody,
        bool replyAll = false);
    
    /// <summary>
    /// Mark message as read
    /// </summary>
    Task MarkAsReadAsync(string accessToken, string mailboxId, string messageId);
    
    /// <summary>
    /// Move message to folder
    /// </summary>
    Task MoveMessageAsync(string accessToken, string mailboxId, string messageId, string destinationFolderId);
    
    /// <summary>
    /// Create a webhook subscription for new messages
    /// </summary>
    Task<GraphSubscription> CreateSubscriptionAsync(
        string accessToken, 
        string mailboxId, 
        string webhookUrl, 
        int expirationMinutes = 4230); // Max ~3 days
    
    /// <summary>
    /// Renew a webhook subscription
    /// </summary>
    Task<GraphSubscription> RenewSubscriptionAsync(string accessToken, string subscriptionId, int expirationMinutes = 4230);
    
    /// <summary>
    /// Delete a webhook subscription
    /// </summary>
    Task DeleteSubscriptionAsync(string accessToken, string subscriptionId);
    
    /// <summary>
    /// Get users from Microsoft 365 tenant
    /// </summary>
    Task<List<GraphUser>> GetUsersAsync(string? emailFilter = null);
}

/// <summary>
/// Graph API email message DTO
/// </summary>
public class GraphEmailMessage
{
    public string Id { get; set; } = string.Empty;
    public string? InternetMessageId { get; set; }
    public string? ConversationId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? BodyPreview { get; set; }
    public GraphEmailBody? Body { get; set; }
    public GraphEmailAddress? From { get; set; }
    public List<GraphEmailAddress> ToRecipients { get; set; } = new();
    public List<GraphEmailAddress> CcRecipients { get; set; } = new();
    public DateTime ReceivedDateTime { get; set; }
    public DateTime? SentDateTime { get; set; }
    public bool IsRead { get; set; }
    public bool HasAttachments { get; set; }
    public string Importance { get; set; } = "normal";
    public bool IsDraft { get; set; }
}

public class GraphEmailBody
{
    public string ContentType { get; set; } = "text";
    public string Content { get; set; } = string.Empty;
}

public class GraphEmailAddress
{
    public string Address { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class GraphAttachment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
}

public class GraphSubscription
{
    public string Id { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public DateTime ExpirationDateTime { get; set; }
}

/// <summary>
/// Graph API user DTO
/// </summary>
public class GraphUser
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? EmailAddress { get; set; }
    public string? UserPrincipalName { get; set; }
}
