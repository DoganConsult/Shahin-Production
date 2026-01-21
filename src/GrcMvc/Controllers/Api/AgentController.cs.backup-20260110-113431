using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Claude AI Agent operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IClaudeAgentService _agentService;
    private readonly IDiagnosticAgentService _diagnosticService;
    private readonly ISupportAgentService _supportService;
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IClaudeAgentService agentService,
        IDiagnosticAgentService diagnosticService,
        ISupportAgentService supportService,
        GrcDbContext dbContext,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _diagnosticService = diagnosticService;
        _supportService = supportService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Check if AI agents are available
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus()
    {
        var isAvailable = await _agentService.IsAvailableAsync();
        return Ok(new
        {
            available = isAvailable,
            agents = new[]
            {
                new { name = "ComplianceAgent", enabled = isAvailable, description = "Analyzes compliance requirements and gaps" },
                new { name = "RiskAgent", enabled = isAvailable, description = "Analyzes and scores risks" },
                new { name = "AuditAgent", enabled = isAvailable, description = "Analyzes audit trails and findings" },
                new { name = "PolicyAgent", enabled = isAvailable, description = "Reviews and recommends policy changes" },
                new { name = "AnalyticsAgent", enabled = isAvailable, description = "Generates insights from data" },
                new { name = "ReportAgent", enabled = isAvailable, description = "Generates natural language reports" },
                new { name = "DiagnosticAgent", enabled = isAvailable, description = "Analyzes system errors and health" },
                new { name = "SupportAgent", enabled = true, description = "Assists users with onboarding and questions" },
                new { name = "WorkflowAgent", enabled = isAvailable, description = "Optimizes workflow processes" }
            }
        });
    }

    /// <summary>
    /// Get registered AI Agent Team (Dr. Dogan's Team)
    /// Returns all registered agents with their roles, capabilities, and permissions
    /// </summary>
    [HttpGet("team")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAgentTeam()
    {
        var isAiAvailable = await _agentService.IsAvailableAsync();
        
        var agents = await _dbContext.AgentDefinitions
            .Where(a => a.IsActive)
            .OrderBy(a => a.AgentCode)
            .Select(a => new
            {
                id = a.Id,
                code = a.AgentCode,
                name = a.Name,
                nameAr = a.NameAr,
                description = a.Description,
                type = a.AgentType,
                version = a.Version,
                isActive = a.IsActive,
                oversightRole = a.OversightRoleCode,
                escalationRole = a.EscalationRoleCode,
                autoApprovalThreshold = a.AutoApprovalConfidenceThreshold,
                capabilities = a.CapabilitiesJson,
                dataSources = a.DataSourcesJson,
                allowedActions = a.AllowedActionsJson,
                approvalRequiredActions = a.ApprovalRequiredActionsJson,
                activatedAt = a.ActivatedAt
            })
            .ToListAsync();

        var capabilities = await _dbContext.AgentCapabilities
            .Where(c => c.IsActive)
            .GroupBy(c => c.AgentId)
            .Select(g => new { AgentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AgentId, x => x.Count);

        var approvalGates = await _dbContext.AgentApprovalGates
            .Where(g => g.IsActive)
            .GroupBy(g => g.AgentId)
            .Select(g => new { AgentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AgentId, x => x.Count);

        var sodRulesCount = await _dbContext.AgentSoDRules.CountAsync(r => r.IsActive);
        var humanResponsibilitiesCount = await _dbContext.HumanRetainedResponsibilities.CountAsync(r => r.IsActive);

        return Ok(new
        {
            team = "Dr. Dogan AI Team",
            teamAr = "فريق د. دوغان للذكاء الاصطناعي",
            organization = "Dogan Consult",
            platform = "Shahin AI GRC Platform",
            claudeApiEnabled = isAiAvailable,
            totalAgents = agents.Count,
            agents = agents.Select(a => new
            {
                a.id,
                a.code,
                a.name,
                a.nameAr,
                a.description,
                a.type,
                a.version,
                a.isActive,
                aiPowered = isAiAvailable,
                oversight = new { role = a.oversightRole, escalation = a.escalationRole },
                autoApprovalThreshold = a.autoApprovalThreshold,
                capabilitiesCount = capabilities.GetValueOrDefault(a.id, 0),
                approvalGatesCount = approvalGates.GetValueOrDefault(a.id, 0),
                capabilities = !string.IsNullOrEmpty(a.capabilities) 
                    ? JsonSerializer.Deserialize<string[]>(a.capabilities) 
                    : Array.Empty<string>(),
                dataSources = !string.IsNullOrEmpty(a.dataSources) 
                    ? JsonSerializer.Deserialize<string[]>(a.dataSources) 
                    : Array.Empty<string>(),
                allowedActions = !string.IsNullOrEmpty(a.allowedActions) 
                    ? JsonSerializer.Deserialize<string[]>(a.allowedActions) 
                    : Array.Empty<string>(),
                approvalRequiredActions = !string.IsNullOrEmpty(a.approvalRequiredActions) 
                    ? JsonSerializer.Deserialize<string[]>(a.approvalRequiredActions) 
                    : Array.Empty<string>(),
                a.activatedAt
            }),
            governance = new
            {
                segregationOfDutiesRules = sodRulesCount,
                humanRetainedResponsibilities = humanResponsibilitiesCount,
                approvalGatesTotal = approvalGates.Values.Sum()
            },
            metadata = new
            {
                generatedAt = DateTime.UtcNow,
                apiVersion = "1.0",
                model = "claude-sonnet-4-20250514"
            }
        });
    }

    /// <summary>
    /// Get specific agent details by code
    /// </summary>
    [HttpGet("team/{agentCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAgentByCode(string agentCode)
    {
        var agent = await _dbContext.AgentDefinitions
            .FirstOrDefaultAsync(a => a.AgentCode == agentCode.ToUpper() && a.IsActive);

        if (agent == null)
        {
            return NotFound(new { error = "Agent not found", code = agentCode });
        }

        var capabilities = await _dbContext.AgentCapabilities
            .Where(c => c.AgentId == agent.Id && c.IsActive)
            .Select(c => new
            {
                c.CapabilityCode,
                c.Name,
                c.Description,
                c.Category,
                c.RiskLevel,
                c.RequiresApproval,
                c.MaxUsesPerHour
            })
            .ToListAsync();

        var approvalGates = await _dbContext.AgentApprovalGates
            .Where(g => g.AgentId == agent.Id && g.IsActive)
            .Select(g => new
            {
                g.GateCode,
                g.Name,
                g.Description,
                g.TriggerActionTypes,
                g.ApproverRoleCode,
                g.ApprovalSLAHours,
                g.EscalationRoleCode,
                g.BypassConfidenceThreshold
            })
            .ToListAsync();

        var recentActions = await _dbContext.AgentActions
            .Where(a => a.AgentId == agent.Id)
            .OrderByDescending(a => a.ExecutedAt)
            .Take(10)
            .Select(a => new
            {
                a.ActionCorrelationId,
                a.ActionType,
                a.ActionDescription,
                a.Status,
                a.ConfidenceScore,
                a.RequiredApproval,
                a.WasApproved,
                a.ExecutedAt
            })
            .ToListAsync();

        var isAiAvailable = await _agentService.IsAvailableAsync();

        return Ok(new
        {
            agent = new
            {
                agent.Id,
                agent.AgentCode,
                agent.Name,
                agent.NameAr,
                agent.Description,
                agent.AgentType,
                agent.Version,
                agent.IsActive,
                aiPowered = isAiAvailable,
                oversight = new
                {
                    role = agent.OversightRoleCode,
                    escalation = agent.EscalationRoleCode
                },
                agent.AutoApprovalConfidenceThreshold,
                agent.ActivatedAt
            },
            capabilities,
            approvalGates,
            recentActions,
            statistics = new
            {
                totalCapabilities = capabilities.Count,
                highRiskCapabilities = capabilities.Count(c => c.RiskLevel == "High"),
                totalApprovalGates = approvalGates.Count,
                recentActionsCount = recentActions.Count
            }
        });
    }

    /// <summary>
    /// Get human-retained responsibilities (what humans must always do)
    /// </summary>
    [HttpGet("governance/human-responsibilities")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHumanRetainedResponsibilities()
    {
        var responsibilities = await _dbContext.HumanRetainedResponsibilities
            .Where(r => r.IsActive)
            .OrderBy(r => r.Category)
            .ThenBy(r => r.RoleCode)
            .Select(r => new
            {
                r.ResponsibilityCode,
                r.Name,
                r.NameAr,
                r.Description,
                r.Category,
                r.RoleCode,
                r.NonDelegableReason,
                r.RegulatoryReference,
                r.AgentSupportDescription
            })
            .ToListAsync();

        return Ok(new
        {
            title = "Human-Retained Responsibilities",
            titleAr = "المسؤوليات المحتفظة للبشر",
            description = "These responsibilities cannot be delegated to AI agents and must be performed by humans",
            descriptionAr = "هذه المسؤوليات لا يمكن تفويضها للوكلاء الذكاء الاصطناعي ويجب أن يؤديها البشر",
            count = responsibilities.Count,
            responsibilities,
            categories = responsibilities.GroupBy(r => r.Category).Select(g => new
            {
                category = g.Key,
                count = g.Count()
            })
        });
    }

    /// <summary>
    /// Get Segregation of Duties rules for agents
    /// </summary>
    [HttpGet("governance/sod-rules")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSoDRules()
    {
        var rules = await _dbContext.AgentSoDRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.RuleCode)
            .Select(r => new
            {
                r.RuleCode,
                r.Name,
                r.Description,
                conflictingActions = new
                {
                    action1 = r.Action1,
                    action1AgentTypes = r.Action1AgentTypes,
                    action2 = r.Action2,
                    action2AgentTypes = r.Action2AgentTypes
                },
                r.RiskDescription,
                r.Enforcement
            })
            .ToListAsync();

        return Ok(new
        {
            title = "Agent Segregation of Duties Rules",
            titleAr = "قواعد فصل المهام للوكلاء",
            description = "Rules preventing same agent from performing conflicting actions",
            count = rules.Count,
            rules
        });
    }

    /// <summary>
    /// Analyze compliance for a framework
    /// </summary>
    [HttpPost("compliance/analyze")]
    [Authorize(Policy = "Grc.Assessments.View")]
    public async Task<IActionResult> AnalyzeCompliance([FromBody] ComplianceAnalyzeRequest request)
    {
        var result = await _agentService.AnalyzeComplianceAsync(
            request.FrameworkCode,
            request.AssessmentId,
            request.TenantId);
        return Ok(result);
    }

    /// <summary>
    /// Analyze a risk
    /// </summary>
    [HttpPost("risk/analyze")]
    [Authorize(Policy = "Grc.Risks.View")]
    public async Task<IActionResult> AnalyzeRisk([FromBody] RiskAnalyzeRequest request)
    {
        var result = await _agentService.AnalyzeRiskAsync(
            request.RiskDescription,
            request.Context);
        return Ok(result);
    }

    /// <summary>
    /// Analyze an audit
    /// </summary>
    [HttpPost("audit/analyze")]
    [Authorize(Policy = "Grc.Audits.View")]
    public async Task<IActionResult> AnalyzeAudit([FromBody] AuditAnalyzeRequest request)
    {
        var result = await _agentService.AnalyzeAuditAsync(request.AuditId);
        return Ok(result);
    }

    /// <summary>
    /// Analyze a policy document
    /// </summary>
    [HttpPost("policy/analyze")]
    [Authorize(Policy = "Grc.Policies.View")]
    public async Task<IActionResult> AnalyzePolicy([FromBody] PolicyAnalyzeRequest request)
    {
        var result = await _agentService.AnalyzePolicyAsync(
            request.PolicyContent,
            request.FrameworkCode);
        return Ok(result);
    }

    /// <summary>
    /// Generate analytics insights
    /// </summary>
    [HttpPost("analytics/insights")]
    [Authorize(Policy = "Grc.Reports.View")]
    public async Task<IActionResult> GenerateInsights([FromBody] InsightsRequest request)
    {
        var result = await _agentService.GenerateInsightsAsync(
            request.DataType,
            request.Filters,
            request.TenantId);
        return Ok(result);
    }

    /// <summary>
    /// Generate a report
    /// </summary>
    [HttpPost("report/generate")]
    [Authorize(Policy = "Grc.Reports.Generate")]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
    {
        var result = await _agentService.GenerateReportAsync(
            request.ReportType,
            request.Parameters,
            request.TenantId);
        return Ok(result);
    }

    /// <summary>
    /// Chat with AI assistant (authenticated users)
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var result = await _agentService.ChatAsync(
            request.Message,
            request.ConversationHistory,
            request.Context);
        return Ok(result);
    }

    /// <summary>
    /// Public chat for trial/visitor assistance (anonymous allowed)
    /// Limited responses for unauthenticated users
    /// GET: /api/agent/chat/public?message=hello
    /// POST: /api/agent/chat/public with JSON body
    /// </summary>
    [HttpGet("chat/public")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicChatGet([FromQuery] string message, [FromQuery] string? context = null)
    {
        return await ProcessPublicChat(message, context);
    }

    [HttpPost("chat/public")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PublicChatPost([FromBody] PublicChatRequest request)
    {
        return await ProcessPublicChat(request.Message, request.Context);
    }

    private async Task<IActionResult> ProcessPublicChat(string message, string? context)
    {
        _logger.LogInformation("Public AI chat request: {Context}", context);

        // Check if AI is available
        var isAvailable = await _agentService.IsAvailableAsync();
        
        if (!isAvailable || string.IsNullOrEmpty(message))
        {
            // Return helpful static responses when API not configured
            var staticResponse = GetStaticTrialResponse(message ?? "", context);
            return Ok(new ChatResponse
            {
                Success = true,
                Response = staticResponse,
                SuggestedActions = new List<string> { "سجل مجاناً", "تواصل مع الدعم" }
            });
        }

        try
        {
            // Add context for public/trial users
            var aiContext = $"هذا مستخدم زائر في صفحة التسجيل التجريبي. سياق: {context ?? "trial_registration"}. " +
                         "أجب بشكل مختصر ومفيد بالعربية. ركز على مساعدته للتسجيل.";

            var result = await _agentService.ChatAsync(
                message,
                null, // No conversation history for anonymous
                aiContext);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in public chat");
            return Ok(new ChatResponse
            {
                Success = true,
                Response = GetStaticTrialResponse(message, context)
            });
        }
    }

    private string GetStaticTrialResponse(string message, string? context)
    {
        var lowerMessage = message.ToLowerInvariant();
        
        // Arabic keywords
        if (message.Contains("مجان") || message.Contains("سعر") || message.Contains("تكلفة"))
            return "🎉 التجربة مجانية لمدة 7 أيام بدون بطاقة ائتمان! سجل الآن وابدأ رحلتك مع شاهين.";
        
        if (message.Contains("ضوابط") || message.Contains("امتثال") || message.Contains("نظام"))
            return "📋 شاهين يغطي أكثر من 13,500 ضابط من 130+ جهة تنظيمية سعودية، بما في ذلك NCA ECC و SAMA CSF و PDPL.";
        
        if (message.Contains("تسجيل") || message.Contains("حساب"))
            return "✅ للتسجيل: أدخل بياناتك في النموذج أعلاه وستحصل على 7 أيام تجريبية مجانية فوراً!";
        
        if (message.Contains("دعم") || message.Contains("مساعد") || message.Contains("تواصل"))
            return "📞 فريق الدعم متاح على مدار الساعة! سجل الآن وسيتواصل معك أحد خبرائنا.";
        
        if (lowerMessage.Contains("email") || lowerMessage.Contains("password") || message.Contains("كلمة") || message.Contains("بريد"))
            return "📧 أدخل بريدك الإلكتروني وكلمة مرور قوية (8 أحرف على الأقل) للتسجيل.";

        // Default response
        return "مرحباً! 👋 شاهين هو منصة الحوكمة والمخاطر والامتثال الذكية. سجل مجاناً لمدة 7 أيام وأجب على 96 سؤالاً فقط للحصول على خطة امتثال مخصصة!";
    }

    /// <summary>
    /// Assess control effectiveness
    /// </summary>
    [HttpPost("control/assess")]
    [Authorize(Policy = "Grc.Controls.View")]
    public async Task<IActionResult> AssessControl([FromBody] ControlAssessRequest request)
    {
        var result = await _agentService.AssessControlAsync(
            request.ControlId,
            request.EvidenceDescription);
        return Ok(result);
    }

    /// <summary>
    /// Analyze evidence quality
    /// </summary>
    [HttpPost("evidence/analyze")]
    [Authorize(Policy = "Grc.Evidence.View")]
    public async Task<IActionResult> AnalyzeEvidence([FromBody] EvidenceAnalyzeRequest request)
    {
        var result = await _agentService.AnalyzeEvidenceAsync(
            request.EvidenceId,
            request.Content);
        return Ok(result);
    }

    /// <summary>
    /// Optimize workflow
    /// </summary>
    [HttpPost("workflow/optimize")]
    [Authorize(Policy = "Grc.Workflow.View")]
    public async Task<IActionResult> OptimizeWorkflow([FromBody] WorkflowOptimizeRequest request)
    {
        var result = await _agentService.OptimizeWorkflowAsync(
            request.WorkflowType,
            request.CurrentMetrics);
        return Ok(result);
    }

    /// <summary>
    /// Get diagnostic report
    /// </summary>
    [HttpGet("diagnostic/report")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetDiagnosticReport([FromQuery] int hoursBack = 24)
    {
        var result = await _diagnosticService.AnalyzeErrorsAsync(hoursBack);
        return Ok(result);
    }

    /// <summary>
    /// Get system health diagnosis
    /// </summary>
    [HttpGet("diagnostic/health")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetHealthDiagnosis()
    {
        var result = await _diagnosticService.AnalyzeHealthAsync();
        return Ok(result);
    }

    /// <summary>
    /// Start support conversation
    /// </summary>
    [HttpPost("support/start")]
    public async Task<IActionResult> StartSupportConversation([FromBody] StartConversationRequest? request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var tenantId = User.FindFirst("tenant_id")?.Value;

        var conversation = await _supportService.StartConversationAsync(
            tenantId != null ? Guid.Parse(tenantId) : null,
            userId,
            HttpContext.Session.Id,
            request?.Category);

        return Ok(new { conversationId = conversation.Id });
    }

    /// <summary>
    /// Send message to support agent
    /// </summary>
    [HttpPost("support/message")]
    public async Task<IActionResult> SendSupportMessage([FromBody] SupportMessageRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Send user message
        await _supportService.SendMessageAsync(request.ConversationId, request.Message, "User", userId);

        // Get AI response
        var response = await _supportService.GetAgentResponseAsync(request.ConversationId, request.Message);

        return Ok(new
        {
            response = response.Content,
            messageId = response.Id
        });
    }

    /// <summary>
    /// Get quick help response
    /// </summary>
    [HttpPost("support/quick")]
    public async Task<IActionResult> GetQuickHelp([FromBody] QuickHelpRequest request)
    {
        var response = await _supportService.GetQuickHelpAsync(request.Question, request.Context ?? "general");
        return Ok(new { response });
    }
}

#region Request Models

public class ComplianceAnalyzeRequest
{
    public string FrameworkCode { get; set; } = string.Empty;
    public Guid? AssessmentId { get; set; }
    public Guid? TenantId { get; set; }
}

public class RiskAnalyzeRequest
{
    public string RiskDescription { get; set; } = string.Empty;
    public Dictionary<string, object>? Context { get; set; }
}

public class AuditAnalyzeRequest
{
    public Guid AuditId { get; set; }
}

public class PolicyAnalyzeRequest
{
    public string PolicyContent { get; set; } = string.Empty;
    public string? FrameworkCode { get; set; }
}

public class InsightsRequest
{
    public string DataType { get; set; } = string.Empty;
    public Dictionary<string, object>? Filters { get; set; }
    public Guid? TenantId { get; set; }
}

public class ReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public Guid? TenantId { get; set; }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessage>? ConversationHistory { get; set; }
    public string? Context { get; set; }
}

public class PublicChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
}

public class ControlAssessRequest
{
    public Guid ControlId { get; set; }
    public string? EvidenceDescription { get; set; }
}

public class EvidenceAnalyzeRequest
{
    public Guid EvidenceId { get; set; }
    public string? Content { get; set; }
}

public class WorkflowOptimizeRequest
{
    public string WorkflowType { get; set; } = string.Empty;
    public Dictionary<string, object>? CurrentMetrics { get; set; }
}

public class StartConversationRequest
{
    public string? Category { get; set; }
}

public class SupportMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class QuickHelpRequest
{
    public string Question { get; set; } = string.Empty;
    public string? Context { get; set; }
}

#endregion
