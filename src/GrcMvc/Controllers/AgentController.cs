using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using System.Security.Claims;

namespace GrcMvc.Controllers;

/// <summary>
/// AI Agent Management Controller
/// Manages 12 specialized AI agents in the Shahin AI GRC system
/// SECURITY: All endpoints require authentication and role-based authorization
/// </summary>
[Authorize] // ✅ CRITICAL FIX: Require authentication for all agent operations
[RequireHttps] // ✅ ADDED: HTTPS only
public class AgentController : Controller
{
    private readonly ILogger<AgentController> _logger;
    private readonly ITenantContextService _tenantContextService;

    public AgentController(
        ILogger<AgentController> logger,
        ITenantContextService tenantContextService)
    {
        _logger = logger;
        _tenantContextService = tenantContextService;
    }

    // Agent Dashboard - Overview of all 12 agents
    public IActionResult Index()
    {
        return View();
    }

    // 1. SHAHIN_AI - Main Orchestrator
    public IActionResult ShahinAi()
    {
        ViewData["AgentCode"] = "SHAHIN_AI";
        ViewData["AgentName"] = "Shahin AI Assistant";
        ViewData["AgentRole"] = "Orchestrator - Main AI, delegates to other agents";
        ViewData["OversightBy"] = "Platform Admin";
        return View();
    }

    // 2. COMPLIANCE_AGENT
    public IActionResult Compliance()
    {
        ViewData["AgentCode"] = "COMPLIANCE_AGENT";
        ViewData["AgentName"] = "Compliance Analysis";
        ViewData["AgentRole"] = "Analyzes KSA frameworks (NCA ECC, SAMA, PDPL)";
        ViewData["OversightBy"] = "Compliance Officer";
        return View();
    }

    // 3. RISK_AGENT
    public IActionResult Risk()
    {
        ViewData["AgentCode"] = "RISK_AGENT";
        ViewData["AgentName"] = "Risk Assessment";
        ViewData["AgentRole"] = "Scores risks, suggests mitigation";
        ViewData["OversightBy"] = "Risk Manager";
        return View();
    }

    // 4. AUDIT_AGENT
    public IActionResult Audit()
    {
        ViewData["AgentCode"] = "AUDIT_AGENT";
        ViewData["AgentName"] = "Audit Analysis";
        ViewData["AgentRole"] = "Analyzes findings, tracks remediation";
        ViewData["OversightBy"] = "Auditor";
        return View();
    }

    // 5. POLICY_AGENT
    public IActionResult Policy()
    {
        ViewData["AgentCode"] = "POLICY_AGENT";
        ViewData["AgentName"] = "Policy Management";
        ViewData["AgentRole"] = "Reviews policies, checks alignment";
        ViewData["OversightBy"] = "Policy Owner";
        return View();
    }

    // 6. ANALYTICS_AGENT
    public IActionResult Analytics()
    {
        ViewData["AgentCode"] = "ANALYTICS_AGENT";
        ViewData["AgentName"] = "Analytics & Insights";
        ViewData["AgentRole"] = "Predictive analytics, trends, KRIs";
        ViewData["OversightBy"] = "GRC Manager";
        return View();
    }

    // 7. REPORT_AGENT
    public IActionResult Report()
    {
        ViewData["AgentCode"] = "REPORT_AGENT";
        ViewData["AgentName"] = "Report Generation";
        ViewData["AgentRole"] = "Creates reports in Arabic/English";
        ViewData["OversightBy"] = "Report Owner";
        return View();
    }

    // 8. DIAGNOSTIC_AGENT
    public IActionResult Diagnostic()
    {
        ViewData["AgentCode"] = "DIAGNOSTIC_AGENT";
        ViewData["AgentName"] = "System Diagnostic";
        ViewData["AgentRole"] = "Monitors health, diagnoses issues";
        ViewData["OversightBy"] = "Platform Admin";
        return View();
    }

    // 9. SUPPORT_AGENT
    public IActionResult Support()
    {
        ViewData["AgentCode"] = "SUPPORT_AGENT";
        ViewData["AgentName"] = "Customer Support";
        ViewData["AgentRole"] = "Onboarding help, answers questions";
        ViewData["OversightBy"] = "Support Manager";
        return View();
    }

    // 10. WORKFLOW_AGENT
    public IActionResult Workflow()
    {
        ViewData["AgentCode"] = "WORKFLOW_AGENT";
        ViewData["AgentName"] = "Workflow Optimization";
        ViewData["AgentRole"] = "Routes tasks, manages SLAs";
        ViewData["OversightBy"] = "Workflow Admin";
        return View();
    }

    // 11. EVIDENCE_AGENT
    public IActionResult Evidence()
    {
        ViewData["AgentCode"] = "EVIDENCE_AGENT";
        ViewData["AgentName"] = "Evidence Collection";
        ViewData["AgentRole"] = "Collects from ERP/IAM/SIEM";
        ViewData["OversightBy"] = "Evidence Owner";
        return View();
    }

    // 12. EMAIL_AGENT
    public IActionResult Email()
    {
        ViewData["AgentCode"] = "EMAIL_AGENT";
        ViewData["AgentName"] = "Email Classification";
        ViewData["AgentRole"] = "Routes emails, drafts responses";
        ViewData["OversightBy"] = "Email Admin";
        return View();
    }

    // API endpoint to get agent status
    [HttpGet]
    [Authorize] // ✅ SECURITY FIX: Require authentication
    public IActionResult GetAgentStatus(string agentCode)
    {
        // ✅ SECURITY: Log who accessed agent status
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation(
            "Agent status requested for {AgentCode} by user {UserId}",
            agentCode, userId);

        // TODO #15: Implement actual agent status check from database/service
        // Requirements:
        // - Check agent heartbeat in database
        // - Verify agent is responsive
        // - Return last activity timestamp
        // - Include agent health metrics
        
        var status = new
        {
            AgentCode = agentCode,
            Status = "Online",
            LastActive = DateTime.UtcNow,
            TasksCompleted = 0,
            AverageResponseTime = "0ms",
            Message = "Status check not yet fully implemented"
        };

        return Json(status);
    }

    // API endpoint to trigger agent
    [HttpPost]
    [Authorize(Roles = "Admin,ComplianceOfficer,RiskManager")] // ✅ SECURITY FIX: Role-based access
    [ValidateAntiForgeryToken] // ✅ SECURITY: CSRF protection
    public IActionResult TriggerAgent(string agentCode, string action, string parameters)
    {
        // ✅ SECURITY: Validate and log who triggered the agent
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        
        _logger.LogWarning(
            "AGENT TRIGGER: Agent {AgentCode} triggered with action {Action} by user {UserId} ({Email})",
            agentCode, action, userId, userEmail);

        // ✅ SECURITY: Validate agent code
        var validAgentCodes = new[] 
        { 
            "SHAHIN_AI", "COMPLIANCE_AGENT", "RISK_AGENT", "AUDIT_AGENT",
            "POLICY_AGENT", "ANALYTICS_AGENT", "REPORT_AGENT", "DIAGNOSTIC_AGENT",
            "SUPPORT_AGENT", "WORKFLOW_AGENT", "EVIDENCE_AGENT", "EMAIL_AGENT"
        };
        
        if (!validAgentCodes.Contains(agentCode?.ToUpper()))
        {
            _logger.LogWarning("Invalid agent code: {AgentCode}", agentCode);
            return BadRequest(new { error = "Invalid agent code" });
        }

        // TODO #16: Implement actual agent trigger logic
        // Requirements:
        // - Validate agent exists and is authorized for user's role
        // - Queue agent task in background job system (Hangfire)
        // - Return task ID for status tracking
        // - Implement timeout and retry logic
        // - Send notification on completion

        return Json(new 
        { 
            success = true, 
            message = "Agent trigger queued (not yet fully implemented)",
            agentCode,
            action,
            triggeredBy = userEmail
        });
    }
}
