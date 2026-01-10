using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities.EmailOperations;
using GrcMvc.Services.EmailOperations;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Copilot Agent API - AI Assistant for GRC and Email Operations
/// Designed to work with Microsoft Copilot and other AI integrations
/// </summary>
[ApiController]
[Route("api/copilot")]
[IgnoreAntiforgeryToken]
public class CopilotAgentController : ControllerBase
{
    private readonly GrcDbContext _db;
    private readonly IClaudeAgentService _claudeService;
    private readonly IEmailAiService _emailAiService;
    private readonly ILogger<CopilotAgentController> _logger;

    public CopilotAgentController(
        GrcDbContext db,
        IClaudeAgentService claudeService,
        IEmailAiService emailAiService,
        ILogger<CopilotAgentController> logger)
    {
        _db = db;
        _claudeService = claudeService;
        _emailAiService = emailAiService;
        _logger = logger;
    }

    /// <summary>
    /// Chat with Copilot Agent - Main interaction endpoint
    /// </summary>
    [HttpPost("chat")]
    [AllowAnonymous] // For testing; add auth in production
    public async Task<IActionResult> Chat([FromBody] CopilotChatRequest request)
    {
        if (string.IsNullOrEmpty(request.Message))
            return BadRequest(new { error = "Message is required" });

        try
        {
            // Build context based on capabilities
            var context = BuildContext(request);

            var response = await _claudeService.ChatAsync(
                request.Message,
                request.ConversationHistory?.Select(h => new ChatMessage
                {
                    Role = h.Role,
                    Content = h.Content
                }).ToList(),
                context);

            return Ok(new CopilotChatResponse
            {
                Response = response.Response ?? "لم أتمكن من معالجة طلبك.",
                ConversationId = Guid.NewGuid().ToString(),
                Capabilities = GetCapabilityList(),
                SuggestedActions = ExtractActions(response.Response)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Copilot chat failed");
            return StatusCode(500, new { error = "Failed to process request", details = ex.Message });
        }
    }

    /// <summary>
    /// Get email summary for Copilot
    /// </summary>
    [HttpGet("emails/summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEmailSummary()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var summary = new
        {
            totalUnread = await _db.EmailThreads.CountAsync(t => t.Status == EmailThreadStatus.New),
            urgent = await _db.EmailThreads.CountAsync(t => 
                t.Priority >= EmailPriority.Urgent && 
                t.Status != EmailThreadStatus.Resolved && 
                t.Status != EmailThreadStatus.Closed),
            awaitingReply = await _db.EmailThreads.CountAsync(t => t.Status == EmailThreadStatus.AwaitingCustomerReply),
            draftsPending = await _db.EmailThreads.CountAsync(t => t.Status == EmailThreadStatus.DraftPending),
            slaBreached = await _db.EmailThreads.CountAsync(t => 
                t.SlaFirstResponseBreached || t.SlaResolutionBreached),
            pendingTasks = await _db.EmailTasks.CountAsync(t => t.Status == EmailTaskStatus.Pending),
            resolvedToday = await _db.EmailThreads.CountAsync(t => 
                t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == today),
            recentThreads = await _db.EmailThreads
                .OrderByDescending(t => t.CreatedDate)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    t.Subject,
                    t.FromEmail,
                    classification = t.Classification.ToString(),
                    status = t.Status.ToString(),
                    priority = t.Priority.ToString(),
                    t.ReceivedAt
                })
                .ToListAsync()
        };

        return Ok(summary);
    }

    /// <summary>
    /// Get specific thread details for Copilot
    /// </summary>
    [HttpGet("emails/thread/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetThread(Guid id)
    {
        var thread = await _db.EmailThreads
            .Include(t => t.Messages.OrderBy(m => m.ReceivedAt))
            .Include(t => t.Mailbox)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (thread == null)
            return NotFound(new { error = "Thread not found" });

        return Ok(new
        {
            thread.Id,
            thread.Subject,
            thread.FromEmail,
            thread.FromName,
            classification = thread.Classification.ToString(),
            status = thread.Status.ToString(),
            priority = thread.Priority.ToString(),
            mailbox = thread.Mailbox?.EmailAddress,
            brand = thread.Mailbox?.Brand,
            thread.ReceivedAt,
            thread.SlaFirstResponseDeadline,
            thread.SlaResolutionDeadline,
            slaBreached = thread.SlaFirstResponseBreached || thread.SlaResolutionBreached,
            messages = thread.Messages.Select(m => new
            {
                m.Id,
                m.FromEmail,
                direction = m.Direction.ToString(),
                status = m.Status.ToString(),
                m.Subject,
                bodyPreview = m.BodyPreview?[..Math.Min(200, m.BodyPreview?.Length ?? 0)],
                m.ReceivedAt,
                m.IsAiGenerated
            })
        });
    }

    /// <summary>
    /// Generate AI reply for a thread
    /// </summary>
    [HttpPost("emails/thread/{id}/generate-reply")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateReply(Guid id, [FromBody] GenerateReplyRequest request)
    {
        var thread = await _db.EmailThreads
            .Include(t => t.Messages.OrderByDescending(m => m.ReceivedAt).Take(1))
            .Include(t => t.Mailbox)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (thread == null)
            return NotFound(new { error = "Thread not found" });

        var lastMessage = thread.Messages.FirstOrDefault();
        if (lastMessage == null)
            return BadRequest(new { error = "No messages in thread" });

        var reply = await _emailAiService.GenerateReplyAsync(new EmailReplyContext
        {
            Brand = thread.Mailbox?.Brand ?? EmailBrands.Shahin,
            Language = request.Language ?? "ar",
            OriginalSubject = thread.Subject,
            OriginalBody = lastMessage.BodyContent ?? lastMessage.BodyPreview ?? "",
            SenderName = thread.FromName ?? thread.FromEmail,
            Classification = thread.Classification,
            AdditionalInstructions = request.Instructions
        });

        return Ok(new
        {
            generatedReply = reply,
            threadId = id,
            language = request.Language ?? "ar",
            needsReview = true
        });
    }

    /// <summary>
    /// Get GRC dashboard summary for Copilot
    /// </summary>
    [HttpGet("grc/summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGrcSummary()
    {
        // Get counts from various GRC modules
        var summary = new
        {
            risks = new
            {
                total = await _db.Set<Models.Entities.Risk>().CountAsync(),
                high = await _db.Set<Models.Entities.Risk>().CountAsync(r => r.RiskLevel == "High" || r.RiskLevel == "Critical"),
                pending = await _db.Set<Models.Entities.Risk>().CountAsync(r => r.Status == "Open" || r.Status == "InProgress")
            },
            controls = new
            {
                total = await _db.Set<Models.Entities.Control>().CountAsync(),
                needsReview = await _db.Set<Models.Entities.Control>().CountAsync(c => c.Status == "NeedsReview")
            },
            audits = new
            {
                active = await _db.Set<Models.Entities.Audit>().CountAsync(a => a.Status == "InProgress"),
                total = await _db.Set<Models.Entities.Audit>().CountAsync()
            },
            compliance = new
            {
                frameworks = await _db.Set<Models.Entities.Framework>().CountAsync(),
                assessments = await _db.Set<Models.Entities.Assessment>().CountAsync()
            },
            vendors = new
            {
                total = await _db.Set<Models.Entities.Vendor>().CountAsync(),
                highRisk = await _db.Set<Models.Entities.Vendor>().CountAsync(v => v.RiskLevel == "High")
            },
            actionPlans = new
            {
                open = await _db.Set<Models.Entities.ActionPlan>().CountAsync(a => a.Status == "Open"),
                overdue = await _db.Set<Models.Entities.ActionPlan>().CountAsync(a => 
                    a.DueDate.HasValue && a.DueDate < DateTime.UtcNow && a.Status != "Completed")
            }
        };

        return Ok(summary);
    }

    /// <summary>
    /// Ask a GRC question to Copilot
    /// </summary>
    [HttpPost("grc/ask")]
    [AllowAnonymous]
    public async Task<IActionResult> AskGrcQuestion([FromBody] CopilotChatRequest request)
    {
        if (string.IsNullOrEmpty(request.Message))
            return BadRequest(new { error = "Question is required" });

        var context = @"أنت مساعد ذكي متخصص في الحوكمة والمخاطر والامتثال (GRC).
        
تعمل لشركة شاهين ومكتب Dogan Consult.

مجالات خبرتك:
- إدارة المخاطر (تقييم، تحليل، معالجة)
- الامتثال للأنظمة واللوائح (خاصة في المملكة العربية السعودية)
- التدقيق الداخلي والخارجي
- إدارة السياسات والإجراءات
- الضوابط الداخلية
- إدارة البائعين والأطراف الثالثة
- خطط العمل التصحيحية

أجب بشكل مفيد ومختصر. إذا كان السؤال يحتاج بيانات محددة، اطلب من المستخدم البحث في النظام.";

        try
        {
            var response = await _claudeService.ChatAsync(request.Message, null, context);

            return Ok(new
            {
                answer = response.Response,
                domain = "GRC",
                suggestedActions = new[]
                {
                    new { action = "view_risks", label = "عرض المخاطر" },
                    new { action = "view_controls", label = "عرض الضوابط" },
                    new { action = "view_audits", label = "عرض التدقيق" }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GRC question failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute an action through Copilot
    /// </summary>
    [HttpPost("execute")]
    [AllowAnonymous]
    public async Task<IActionResult> ExecuteAction([FromBody] CopilotActionRequest request)
    {
        if (string.IsNullOrEmpty(request.Action))
            return BadRequest(new { error = "Action is required" });

        try
        {
            switch (request.Action.ToLower())
            {
                case "sync_emails":
                    // Trigger email sync
                    var syncResponse = await HttpContext.RequestServices
                        .GetRequiredService<IMicrosoftGraphEmailService>()
                        .GetUsersAsync();
                    return Ok(new { success = true, message = "Email sync triggered", users = syncResponse.Count });

                case "check_sla":
                    var slaBreach = await _db.EmailThreads
                        .Where(t => t.SlaFirstResponseBreached || t.SlaResolutionBreached)
                        .CountAsync();
                    return Ok(new { success = true, slaBreaches = slaBreach });

                case "get_pending_drafts":
                    var drafts = await _db.EmailMessages
                        .Where(m => m.Status == EmailMessageStatus.DraftCreated)
                        .Select(m => new { m.Id, m.Subject, m.ThreadId })
                        .ToListAsync();
                    return Ok(new { success = true, drafts });

                default:
                    return BadRequest(new { error = $"Unknown action: {request.Action}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Action execution failed: {Action}", request.Action);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get Copilot capabilities
    /// </summary>
    [HttpGet("capabilities")]
    [AllowAnonymous]
    public IActionResult GetCapabilities()
    {
        return Ok(new CopilotCapabilities
        {
            Version = "1.0.0",
            Features = new[]
            {
                new CopilotFeature { Name = "email_management", Description = "إدارة البريد الإلكتروني والردود الآلية", Enabled = true },
                new CopilotFeature { Name = "ai_replies", Description = "توليد ردود ذكية بالعربية والإنجليزية", Enabled = true },
                new CopilotFeature { Name = "email_classification", Description = "تصنيف الرسائل تلقائياً", Enabled = true },
                new CopilotFeature { Name = "sla_monitoring", Description = "مراقبة SLA والتنبيهات", Enabled = true },
                new CopilotFeature { Name = "grc_assistant", Description = "مساعد الحوكمة والمخاطر والامتثال", Enabled = true },
                new CopilotFeature { Name = "task_automation", Description = "أتمتة المهام المتكررة", Enabled = true }
            },
            SupportedLanguages = new[] { "ar", "en" },
            Brands = new[] { "Shahin", "DoganConsult" }
        });
    }

    private string BuildContext(CopilotChatRequest request)
    {
        return $@"أنت شاهين - المساعد الذكي لإدارة البريد الإلكتروني والـ GRC.

الشركات التي تخدمها:
- شاهين (Shahin AI) - منصة الحوكمة والمخاطر والامتثال
- Dogan Consult - مكتب استشارات متخصص

قدراتك:
1. إدارة البريد الإلكتروني (تصنيف، رد، متابعة)
2. توليد ردود ذكية بالعربية والإنجليزية
3. مراقبة SLA والتنبيه عند التأخير
4. الإجابة على أسئلة GRC
5. إنشاء مسودات للمراجعة

اللغة المفضلة: {request.Language ?? "ar"}

كن مختصراً ومفيداً. إذا طُلب منك إجراء، وضّح ما ستفعله.";
    }

    private List<string> GetCapabilityList()
    {
        return new List<string>
        {
            "email_management",
            "ai_replies",
            "email_classification",
            "sla_monitoring",
            "grc_assistant",
            "task_automation"
        };
    }

    private List<SuggestedAction> ExtractActions(string? response)
    {
        // Simple action extraction based on keywords
        var actions = new List<SuggestedAction>();

        if (response == null) return actions;

        if (response.Contains("بريد") || response.Contains("email"))
            actions.Add(new SuggestedAction { Action = "view_emails", Label = "عرض البريد" });
        if (response.Contains("مسودة") || response.Contains("draft"))
            actions.Add(new SuggestedAction { Action = "view_drafts", Label = "عرض المسودات" });
        if (response.Contains("مخاطر") || response.Contains("risk"))
            actions.Add(new SuggestedAction { Action = "view_risks", Label = "عرض المخاطر" });

        return actions;
    }
}

// DTOs
public class CopilotChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Language { get; set; } = "ar";
    public List<ConversationMessage>? ConversationHistory { get; set; }
}

public class ConversationMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public class CopilotChatResponse
{
    public string Response { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public List<string> Capabilities { get; set; } = new();
    public List<SuggestedAction> SuggestedActions { get; set; } = new();
}

public class GenerateReplyRequest
{
    public string? Language { get; set; }
    public string? Instructions { get; set; }
}

public class CopilotActionRequest
{
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CopilotCapabilities
{
    public string Version { get; set; } = string.Empty;
    public CopilotFeature[] Features { get; set; } = Array.Empty<CopilotFeature>();
    public string[] SupportedLanguages { get; set; } = Array.Empty<string>();
    public string[] Brands { get; set; } = Array.Empty<string>();
}

public class CopilotFeature
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}

public class SuggestedAction
{
    public string Action { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
