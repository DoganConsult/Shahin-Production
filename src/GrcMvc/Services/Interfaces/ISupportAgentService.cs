using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for AI-powered support agent that assists users during onboarding and usage
/// </summary>
public interface ISupportAgentService
{
    /// <summary>
    /// Start a new support conversation
    /// </summary>
    Task<SupportConversation> StartConversationAsync(Guid? tenantId, string? userId, string? sessionId, string? category = null);

    /// <summary>
    /// Send a message in a conversation and get AI response
    /// </summary>
    Task<SupportMessage> SendMessageAsync(Guid conversationId, string content, string senderType = "User", string? senderId = null);

    /// <summary>
    /// Get AI response to a user message
    /// </summary>
    Task<SupportMessage> GetAgentResponseAsync(Guid conversationId, string userMessage);

    /// <summary>
    /// Get conversation history
    /// </summary>
    Task<IEnumerable<SupportMessage>> GetConversationMessagesAsync(Guid conversationId);

    /// <summary>
    /// Escalate conversation to human agent
    /// </summary>
    Task<SupportConversation> EscalateToHumanAsync(Guid conversationId, string reason);

    /// <summary>
    /// Resolve/close a conversation
    /// </summary>
    Task<SupportConversation> ResolveConversationAsync(Guid conversationId, int? satisfactionRating = null, string? feedback = null);

    /// <summary>
    /// Get quick help response for common questions (no conversation needed)
    /// </summary>
    Task<string> GetQuickHelpAsync(string question, string context = "onboarding");
}

/// <summary>
/// Service for managing user consent and legal documents
/// </summary>
public interface IConsentService
{
    /// <summary>
    /// Get active legal document by type
    /// </summary>
    Task<LegalDocument?> GetActiveDocumentAsync(string documentType);

    /// <summary>
    /// Record user consent
    /// </summary>
    Task<UserConsent> RecordConsentAsync(
        Guid tenantId,
        Guid userId,
        string consentType,
        string documentVersion,
        bool isGranted,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Check if user has given consent for a specific document type
    /// </summary>
    Task<bool> HasConsentAsync(Guid userId, string consentType);

    /// <summary>
    /// Get all consents for a user
    /// </summary>
    Task<IEnumerable<UserConsent>> GetUserConsentsAsync(Guid userId);

    /// <summary>
    /// Withdraw consent
    /// </summary>
    Task<UserConsent> WithdrawConsentAsync(Guid userId, string consentType, string reason);

    /// <summary>
    /// Check if all mandatory consents are given
    /// </summary>
    Task<bool> HasAllMandatoryConsentsAsync(Guid userId);
}
