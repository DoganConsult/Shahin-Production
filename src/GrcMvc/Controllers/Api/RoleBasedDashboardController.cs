using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Role-Based Dashboard Stats API Controller
/// Provides real-time statistics based on user role
/// </summary>
[Route("api/dashboard")]
[ApiController]
[Authorize]
[IgnoreAntiforgeryToken]
public class RoleBasedDashboardController : ControllerBase
{
    private readonly GrcDbContext _dbContext;
    private readonly ITenantContextService _tenantContext;
    private readonly IClaudeAgentService _claudeAgent;
    private readonly ILogger<RoleBasedDashboardController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleBasedDashboardController(
        GrcDbContext dbContext,
        ITenantContextService tenantContext,
        IClaudeAgentService claudeAgent,
        ILogger<RoleBasedDashboardController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _claudeAgent = claudeAgent;
        _logger = logger;
        _userManager = userManager;
    }

    /// <summary>
    /// Get role-specific dashboard statistics
    /// GET /api/dashboard/stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetRoleBasedStats()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tenantId = _tenantContext.GetCurrentTenantId();
            var primaryRole = DeterminePrimaryRole(roles);

            var stats = await GetStatsForRoleAsync(primaryRole, tenantId);
            
            return Ok(new
            {
                role = primaryRole,
                stats,
                generatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role-based stats");
            return StatusCode(500, new { error = "An error occurred while fetching statistics" });
        }
    }

    /// <summary>
    /// Get admin-specific statistics
    /// GET /api/dashboard/stats/admin
    /// </summary>
    [HttpGet("stats/admin")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> GetAdminStats()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            var usersCount = await _dbContext.TenantUsers
                .Where(u => u.TenantId == tenantId)
                .CountAsync();

            var tenantsCount = await _dbContext.Tenants.CountAsync();

        var todayStart = DateTime.UtcNow.Date;
        var activitiesCount = await _dbContext.AuditEvents
            .Where(a => a.TenantId == tenantId && a.CreatedAt >= todayStart)
            .CountAsync();

        // Alerts = overdue tasks + critical risks + expiring items
        var alertsCount = await GetAlertsCountAsync(tenantId);

        return Ok(new
        {
            users = usersCount,
            tenants = tenantsCount,
            activities = activitiesCount,
            alerts = alertsCount
        });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin stats");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Get compliance manager / risk manager statistics
    /// GET /api/dashboard/stats/compliance
    /// </summary>
    [HttpGet("stats/compliance")]
    [Authorize]
    public async Task<IActionResult> GetComplianceStats()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();

            var activeRisks = await _dbContext.Risks
                .Where(r => r.TenantId == tenantId && r.Status != "Closed" && r.Status != "Accepted")
                .CountAsync();

        var effectiveControls = await _dbContext.Controls
            .Where(c => c.TenantId == tenantId && c.Effectiveness >= 70)
            .CountAsync();

            var pendingAssessments = await _dbContext.Assessments
                .Where(a => a.TenantId == tenantId && (a.Status == "Draft" || a.Status == "InProgress"))
                .CountAsync();

            // Calculate compliance percentage
            var allAssessments = await _dbContext.Assessments
                .Where(a => a.TenantId == tenantId)
                .ToListAsync();
            
            var compliancePercent = allAssessments.Any() 
                ? Math.Round(allAssessments.Average(a => a.Score), 1) 
                : 0;

            return Ok(new
            {
                risks = activeRisks,
                controls = effectiveControls,
                assessments = pendingAssessments,
                compliance = compliancePercent
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance stats");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Get auditor statistics
    /// GET /api/dashboard/stats/auditor
    /// </summary>
    [HttpGet("stats/auditor")]
    [Authorize]
    public async Task<IActionResult> GetAuditorStats()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();

            var activeAudits = await _dbContext.Audits
                .Where(a => a.TenantId == tenantId && a.Status != "Completed" && a.Status != "Closed")
                .CountAsync();

            var openFindings = await _dbContext.AuditFindings
                .Where(f => f.Status != "Closed" && f.Status != "Resolved")
                .CountAsync();

        var evidenceForReview = await _dbContext.Evidences
            .Where(e => e.TenantId == tenantId && e.VerificationStatus == "Pending")
            .CountAsync();

        var upcoming = await _dbContext.Audits
            .Where(a => a.TenantId == tenantId && 
                   a.PlannedStartDate > DateTime.UtcNow && 
                   a.PlannedStartDate <= DateTime.UtcNow.AddDays(30))
            .CountAsync();

            return Ok(new
            {
                audits = activeAudits,
                findings = openFindings,
                evidence = evidenceForReview,
                upcoming = upcoming
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auditor stats");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Get user task statistics (for regular users)
    /// GET /api/dashboard/stats/user
    /// </summary>
    [HttpGet("stats/user")]
    [Authorize]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            var tenantId = _tenantContext.GetCurrentTenantId();

            // Get user's inbox items
            var userGuid = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty;
            
            var workflowTasks = await _dbContext.WorkflowTasks
                .Where(t => t.AssignedToUserId == userGuid && t.Status != "Completed")
                .CountAsync();

            var completedTasks = await _dbContext.WorkflowTasks
                .Where(t => t.AssignedToUserId == userGuid && t.Status == "Completed")
                .CountAsync();

            var pendingTasks = await _dbContext.WorkflowTasks
                .Where(t => t.AssignedToUserId == userGuid && 
                       (t.Status == "Pending" || t.Status == "InProgress"))
                .CountAsync();

            return Ok(new
            {
                tasks = workflowTasks,
                completed = completedTasks,
                pending = pendingTasks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// AI Chat endpoint for شاهين assistant
    /// POST /api/dashboard/chat
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] DashboardChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message is required" });
            }

            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
            var primaryRole = DeterminePrimaryRole(roles);
            var tenantId = _tenantContext.GetCurrentTenantId();

            // Build context for AI
            var context = $"User Role: {primaryRole}, Tenant: {tenantId}, Language: Arabic";

            // Get response from Claude
            var response = await _claudeAgent.ChatAsync(
                request.Message,
                request.History?.Select(h => new GrcMvc.Services.Interfaces.ChatMessage { Role = h.Role, Content = h.Content }).ToList(),
                context);

            if (!response.Success)
            {
                // Fallback to rule-based responses if Claude unavailable
                var fallbackResponse = GetFallbackResponse(request.Message, primaryRole);
                return Ok(new
                {
                    success = true,
                    response = fallbackResponse,
                    isFallback = true,
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                response = response.Response,
                isFallback = false,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat");
            return Ok(new
            {
                success = true,
                response = "عذراً، حدث خطأ أثناء معالجة سؤالك. يرجى المحاولة مرة أخرى.",
                isFallback = true,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get suggestions for AI assistant
    /// GET /api/dashboard/suggestions
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
            var primaryRole = DeterminePrimaryRole(roles);

            var suggestions = GetRoleSuggestions(primaryRole);

            return Ok(new { role = primaryRole, suggestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions");
            return Ok(new
            {
                role = "User",
                suggestions = new[]
                {
                    new { text = "كيف أبدأ؟", icon = "bi-play-circle" },
                    new { text = "ما هي مهامي؟", icon = "bi-list-task" },
                    new { text = "شرح النظام", icon = "bi-info-circle" }
                }
            });
        }
    }

    #region Private Methods

    private string DeterminePrimaryRole(IList<string> roles)
    {
        // Priority order for role selection
        var roleHierarchy = new[] { "Admin", "TenantAdmin", "ComplianceManager", "RiskManager", "Auditor", "EvidenceOfficer" };
        
        foreach (var role in roleHierarchy)
        {
            if (roles.Contains(role))
                return role;
        }
        
        return "User";
    }

    private async Task<object> GetStatsForRoleAsync(string role, Guid? tenantId)
    {
        return role switch
        {
            "Admin" or "TenantAdmin" => await GetAdminStatsInternalAsync(tenantId),
            "ComplianceManager" or "RiskManager" => await GetComplianceStatsInternalAsync(tenantId),
            "Auditor" => await GetAuditorStatsInternalAsync(tenantId),
            _ => await GetUserStatsInternalAsync(tenantId)
        };
    }

    private async Task<object> GetAdminStatsInternalAsync(Guid? tenantId)
    {
        var usersCount = tenantId.HasValue
            ? await _dbContext.TenantUsers.Where(u => u.TenantId == tenantId).CountAsync()
            : await _dbContext.TenantUsers.CountAsync();

        var tenantsCount = await _dbContext.Tenants.CountAsync();
        
        var todayStart = DateTime.UtcNow.Date;
        var activitiesQuery = _dbContext.AuditEvents.AsQueryable();
        if (tenantId.HasValue)
            activitiesQuery = activitiesQuery.Where(a => a.TenantId == tenantId.Value);
        var activitiesCount = await activitiesQuery
            .Where(a => a.CreatedAt >= todayStart)
            .CountAsync();

        var alertsCount = await GetAlertsCountAsync(tenantId);

        return new { users = usersCount, tenants = tenantsCount, activities = activitiesCount, alerts = alertsCount };
    }

    private async Task<object> GetComplianceStatsInternalAsync(Guid? tenantId)
    {
        var risksQuery = _dbContext.Risks.AsQueryable();
        var controlsQuery = _dbContext.Controls.AsQueryable();
        var assessmentsQuery = _dbContext.Assessments.AsQueryable();

        if (tenantId.HasValue)
        {
            risksQuery = risksQuery.Where(r => r.TenantId == tenantId);
            controlsQuery = controlsQuery.Where(c => c.TenantId == tenantId);
            assessmentsQuery = assessmentsQuery.Where(a => a.TenantId == tenantId);
        }

        var activeRisks = await risksQuery
            .Where(r => r.Status != "Closed" && r.Status != "Accepted")
            .CountAsync();

        // Effectiveness score >= 70 is considered effective
        var effectiveControls = await controlsQuery
            .Where(c => c.Effectiveness >= 70)
            .CountAsync();

        var pendingAssessments = await assessmentsQuery
            .Where(a => a.Status == "Draft" || a.Status == "InProgress")
            .CountAsync();

        var allAssessments = await assessmentsQuery.ToListAsync();
        var compliancePercent = allAssessments.Any() 
            ? Math.Round(allAssessments.Average(a => a.Score), 1) 
            : 0;

        return new { risks = activeRisks, controls = effectiveControls, assessments = pendingAssessments, compliance = compliancePercent };
    }

    private async Task<object> GetAuditorStatsInternalAsync(Guid? tenantId)
    {
        var auditsQuery = _dbContext.Audits.AsQueryable();
        var evidenceQuery = _dbContext.Evidences.AsQueryable();

        if (tenantId.HasValue)
        {
            auditsQuery = auditsQuery.Where(a => a.TenantId == tenantId);
            evidenceQuery = evidenceQuery.Where(e => e.TenantId == tenantId);
        }

        var activeAudits = await auditsQuery
            .Where(a => a.Status != "Completed" && a.Status != "Closed")
            .CountAsync();

        var openFindings = await _dbContext.AuditFindings
            .Where(f => f.Status != "Closed" && f.Status != "Resolved")
            .CountAsync();

        var evidenceForReview = await evidenceQuery
            .Where(e => e.VerificationStatus == "Pending")
            .CountAsync();

        var upcoming = await auditsQuery
            .Where(a => a.PlannedStartDate > DateTime.UtcNow && 
                   a.PlannedStartDate <= DateTime.UtcNow.AddDays(30))
            .CountAsync();

        return new { audits = activeAudits, findings = openFindings, evidence = evidenceForReview, upcoming };
    }

    private async Task<object> GetUserStatsInternalAsync(Guid? tenantId)
    {
        var userId = _userManager.GetUserId(User);
        var userGuid = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty;

        var workflowTasks = await _dbContext.WorkflowTasks
            .Where(t => t.AssignedToUserId == userGuid && t.Status != "Completed")
            .CountAsync();

        var completedTasks = await _dbContext.WorkflowTasks
            .Where(t => t.AssignedToUserId == userGuid && t.Status == "Completed")
            .CountAsync();

        var pendingTasks = await _dbContext.WorkflowTasks
            .Where(t => t.AssignedToUserId == userGuid && 
                   (t.Status == "Pending" || t.Status == "InProgress"))
            .CountAsync();

        return new { tasks = workflowTasks, completed = completedTasks, pending = pendingTasks };
    }

    private async Task<int> GetAlertsCountAsync(Guid? tenantId)
    {
        var alerts = 0;

        // Overdue tasks
        var overdueQuery = _dbContext.WorkflowTasks
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");
        alerts += await overdueQuery.CountAsync();

        // Critical/High risks
        var risksQuery = _dbContext.Risks.AsQueryable();
        if (tenantId.HasValue)
            risksQuery = risksQuery.Where(r => r.TenantId == tenantId);
        
        alerts += await risksQuery
            .Where(r => (r.RiskLevel == "Critical" || r.RiskLevel == "High") && r.Status != "Closed")
            .CountAsync();

        return alerts;
    }

    private string GetFallbackResponse(string message, string role)
    {
        var q = message.ToLower();

        // Arabic keywords matching
        if (q.Contains("أبدأ") || q.Contains("بداية") || q.Contains("start"))
        {
            return role switch
            {
                "Admin" or "TenantAdmin" => "كمدير نظام، يمكنك البدء بـ:\n1. مراجعة لوحة التحكم\n2. إعداد المستخدمين والأدوار\n3. تكوين إعدادات المؤسسة",
                "ComplianceManager" => "كمدير امتثال، ابدأ بـ:\n1. مراجعة الضوابط المطبقة\n2. إنشاء تقييم جديد\n3. متابعة حالة الأدلة",
                "RiskManager" => "كمدير مخاطر، ابدأ بـ:\n1. مراجعة سجل المخاطر\n2. تقييم المخاطر الجديدة\n3. متابعة خطط المعالجة",
                "Auditor" => "كمراجع، ابدأ بـ:\n1. مراجعة المراجعات المفتوحة\n2. فحص الأدلة المرفقة\n3. تسجيل الملاحظات",
                _ => "مرحباً! ابدأ بـ:\n1. مراجعة مهامك في صندوق الوارد\n2. استكشاف لوحة التحكم\n3. قراءة دليل المساعدة"
            };
        }

        if (q.Contains("مهام") || q.Contains("tasks"))
        {
            return role switch
            {
                "Admin" or "TenantAdmin" => "مهامك تشمل: إدارة المستخدمين، مراقبة النظام، وإعداد التقارير التنفيذية.",
                "ComplianceManager" => "مهامك تشمل: متابعة التقييمات، مراجعة الأدلة، وضمان الامتثال للأنظمة.",
                "RiskManager" => "مهامك تشمل: تحديد المخاطر، تقييمها، ومتابعة خطط المعالجة.",
                "Auditor" => "مهامك تشمل: إجراء المراجعات، فحص الأدلة، وتسجيل الملاحظات.",
                _ => "مهامك متوفرة في صندوق الوارد. تحقق منها بانتظام لمتابعة الطلبات."
            };
        }

        if (q.Contains("نظام") || q.Contains("شرح") || q.Contains("system"))
        {
            return "نظام الحوكمة والمخاطر والامتثال (GRC) يساعدك في إدارة المخاطر، الامتثال للأنظمة، ومراقبة الضوابط. يمكنك البدء من لوحة التحكم لرؤية نظرة عامة على وضع مؤسستك.";
        }

        if (q.Contains("مخاطر") || q.Contains("risk"))
        {
            return "لإدارة المخاطر، انتقل إلى قسم \"إدارة المخاطر\" من القائمة. يمكنك إضافة مخاطر جديدة، تقييمها، وتعيين خطط معالجة.";
        }

        if (q.Contains("ضوابط") || q.Contains("control"))
        {
            return "الضوابط هي الإجراءات والسياسات التي تحمي مؤسستك. يمكنك عرض مصفوفة الضوابط ومتابعة حالة كل ضابط.";
        }

        if (q.Contains("أدلة") || q.Contains("evidence"))
        {
            return "الأدلة هي المستندات التي تثبت تطبيق الضوابط. يمكنك رفع أدلة جديدة وربطها بالضوابط المناسبة.";
        }

        if (q.Contains("تقييم") || q.Contains("assessment"))
        {
            return "التقييمات تساعدك في قياس مدى الامتثال. ابدأ تقييم جديد من قسم التقييمات.";
        }

        return "شكراً لسؤالك! يمكنني مساعدتك في التنقل في النظام وفهم المهام المطلوبة منك. جرب أحد الإجراءات السريعة أو اسألني سؤالاً محدداً.";
    }

    private object[] GetRoleSuggestions(string role)
    {
        return role switch
        {
            "Admin" or "TenantAdmin" => new object[]
            {
                new { text = "كيف أضيف مستخدم جديد؟", icon = "bi-person-plus" },
                new { text = "ما هي التقارير المتاحة؟", icon = "bi-bar-chart" },
                new { text = "كيف أدير الصلاحيات؟", icon = "bi-shield-lock" },
                new { text = "إعدادات المؤسسة", icon = "bi-gear" }
            },
            "ComplianceManager" => new object[]
            {
                new { text = "كيف أنشئ تقييم جديد؟", icon = "bi-clipboard-plus" },
                new { text = "عرض حالة الامتثال", icon = "bi-graph-up" },
                new { text = "الضوابط غير المفعّلة", icon = "bi-shield-x" },
                new { text = "الأدلة المعلقة", icon = "bi-file-earmark-x" }
            },
            "RiskManager" => new object[]
            {
                new { text = "المخاطر الحرجة", icon = "bi-exclamation-octagon" },
                new { text = "خطط المعالجة المفتوحة", icon = "bi-bandaid" },
                new { text = "تقرير المخاطر", icon = "bi-file-text" },
                new { text = "مؤشرات المخاطر", icon = "bi-speedometer2" }
            },
            "Auditor" => new object[]
            {
                new { text = "المراجعات الجارية", icon = "bi-search" },
                new { text = "الملاحظات المفتوحة", icon = "bi-flag" },
                new { text = "الأدلة للمراجعة", icon = "bi-file-earmark-check" },
                new { text = "تقرير المراجعة", icon = "bi-journal-text" }
            },
            _ => new object[]
            {
                new { text = "كيف أبدأ؟", icon = "bi-play-circle" },
                new { text = "ما هي مهامي؟", icon = "bi-list-task" },
                new { text = "شرح النظام", icon = "bi-info-circle" },
                new { text = "المساعدة", icon = "bi-question-circle" }
            }
        };
    }

    #endregion
}

public class DashboardChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<DashboardChatHistoryItem>? History { get; set; }
}

public class DashboardChatHistoryItem
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}
