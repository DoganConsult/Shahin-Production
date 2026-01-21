using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

/// <summary>
/// API Controller for support agent chat functionality
/// </summary>
[Authorize]
[ApiController]
[Route("api/support")]
public class SupportApiController : ControllerBase
{
    private readonly ISupportAgentService _supportService;
    private readonly IConsentService _consentService;
    private readonly ILogger<SupportApiController> _logger;

    public SupportApiController(
        ISupportAgentService supportService,
        IConsentService consentService,
        ILogger<SupportApiController> logger)
    {
        _supportService = supportService;
        _consentService = consentService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new support conversation
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest? request = null)
    {
        try
        {
            var userId = User?.FindFirst("sub")?.Value;
            var sessionId = HttpContext.Session?.Id ?? Guid.NewGuid().ToString();

            var conversation = await _supportService.StartConversationAsync(
                request?.TenantId,
                userId,
                sessionId,
                request?.Category ?? "Onboarding");

            return Ok(new
            {
                conversation.Id,
                conversation.Status,
                conversation.SessionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting support conversation");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Send a message and get AI response
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { error = "Message content is required" });

            // Record user message
            var userId = User?.FindFirst("sub")?.Value;
            await _supportService.SendMessageAsync(
                request.ConversationId,
                request.Content,
                "User",
                userId);

            // Get AI response
            var response = await _supportService.GetAgentResponseAsync(
                request.ConversationId,
                request.Content);

            return Ok(new
            {
                userMessage = request.Content,
                agentResponse = response.Content,
                timestamp = response.SentAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending support message");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Get conversation messages
    /// </summary>
    [HttpGet("messages/{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        try
        {
            var messages = await _supportService.GetConversationMessagesAsync(conversationId);
            return Ok(messages.Select(m => new
            {
                m.Id,
                m.Content,
                m.SenderType,
                m.SentAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Escalate to human agent
    /// </summary>
    [HttpPost("escalate")]
    public async Task<IActionResult> Escalate([FromBody] EscalateRequest request)
    {
        try
        {
            var conversation = await _supportService.EscalateToHumanAsync(
                request.ConversationId,
                request.Reason ?? "User requested human agent");

            return Ok(new
            {
                conversation.Id,
                conversation.Status,
                message = "Conversation escalated to human agent"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating conversation");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Quick help without conversation (for tooltips, etc.)
    /// </summary>
    [HttpGet("quick-help")]
    public async Task<IActionResult> QuickHelp([FromQuery] string question, [FromQuery] string context = "onboarding")
    {
        try
        {
            var response = await _supportService.GetQuickHelpAsync(question, context);
            return Ok(new { response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick help");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Get active legal document
    /// </summary>
    [HttpGet("legal/{documentType}")]
    public async Task<IActionResult> GetLegalDocument(string documentType)
    {
        try
        {
            var document = await _consentService.GetActiveDocumentAsync(documentType);
            if (document == null)
                return NotFound(new { error = $"Document type '{documentType}' not found" });

            return Ok(new
            {
                document.DocumentType,
                document.Version,
                document.Title,
                document.TitleAr,
                document.ContentEn,
                document.ContentAr,
                document.Summary,
                document.EffectiveDate,
                document.IsMandatory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting legal document");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Record user consent
    /// </summary>
    [HttpPost("consent")]
    public async Task<IActionResult> RecordConsent([FromBody] RecordConsentRequest request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var consent = await _consentService.RecordConsentAsync(
                request.TenantId,
                request.UserId,
                request.ConsentType,
                request.DocumentVersion,
                request.IsGranted,
                ipAddress,
                userAgent);

            return Ok(new
            {
                consent.Id,
                consent.ConsentType,
                consent.IsGranted,
                consent.ConsentedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording consent");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }

    /// <summary>
    /// Check if user has all mandatory consents
    /// </summary>
    [HttpGet("consent/check/{userId}")]
    public async Task<IActionResult> CheckConsents(string userId)
    {
        try
        {
            var hasAll = await _consentService.HasAllMandatoryConsentsAsync(userId);
            var consents = await _consentService.GetUserConsentsAsync(userId);

            return Ok(new
            {
                hasAllMandatory = hasAll,
                consents = consents.Select(c => new
                {
                    c.ConsentType,
                    c.IsGranted,
                    c.DocumentVersion,
                    c.ConsentedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking consents");
            return BadRequest(new { error = "An error occurred processing your request." });
        }
    }
}

// Request DTOs
public record StartConversationRequest(Guid? TenantId = null, string? Category = null);
public record SendMessageRequest(Guid ConversationId, string Content);
public record EscalateRequest(Guid ConversationId, string? Reason = null);
public record RecordConsentRequest(Guid TenantId, string UserId, string ConsentType, string DocumentVersion, bool IsGranted);
