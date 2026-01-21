using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Services;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Shahin-AI Integration Controller
/// Shows the full flow: Onboarding → Assessment → Workflow → Evidence
/// </summary>
[Authorize]
public class ShahinAIIntegrationController : Controller
{
    private readonly GrcDbContext _db;
    private readonly IShahinAIOrchestrationService _orchestration;
    private readonly ISmartOnboardingService _onboarding;
    private readonly ILogger<ShahinAIIntegrationController> _logger;

    public ShahinAIIntegrationController(
        GrcDbContext db,
        IShahinAIOrchestrationService orchestration,
        ISmartOnboardingService onboarding,
        ILogger<ShahinAIIntegrationController> logger)
    {
        _db = db;
        _orchestration = orchestration;
        _onboarding = onboarding;
        _logger = logger;
    }

    /// <summary>
    /// Integrated Flow Dashboard - Shows complete GRC process
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var tenantId = GetCurrentTenantId();

        var model = new IntegratedFlowViewModel
        {
            // Onboarding Status
            OrganizationProfile = await _db.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId),
            OnboardingComplete = await _db.OrganizationProfiles.AnyAsync(p => p.TenantId == tenantId),

            // Derived Scope
            DerivedBaselines = await _db.TenantBaselines.Where(b => b.TenantId == tenantId).CountAsync(),
            DerivedPackages = await _db.TenantPackages.Where(p => p.TenantId == tenantId).CountAsync(),

            // Plans & Assessments
            ActivePlans = await _db.Plans.Where(p => p.TenantId == tenantId && p.Status == "Active").CountAsync(),
            TotalAssessments = await _db.Assessments.Where(a => a.TenantId == tenantId).CountAsync(),
            InProgressAssessments = await _db.Assessments.Where(a => a.TenantId == tenantId && a.Status == "InProgress").CountAsync(),

            // Workflows
            ActiveWorkflows = await _db.WorkflowInstances.Where(w => w.TenantId == tenantId && w.Status == "InProgress").CountAsync(),
            PendingTasks = await _db.WorkflowTasks.Where(t => t.Status == "Pending").CountAsync(),

            // Evidence (Shahin PROVE/VAULT)
            TotalEvidence = await _db.AutoTaggedEvidences.Where(e => e.TenantId == tenantId).CountAsync(),
            ApprovedEvidence = await _db.AutoTaggedEvidences.Where(e => e.TenantId == tenantId && e.Status == "Approved").CountAsync(),

            // Controls (Shahin MAP)
            TotalControls = await _db.CanonicalControls.Where(c => c.IsActive).CountAsync(),

            // Indicators (Shahin WATCH)
            TotalIndicators = await _db.RiskIndicators.Where(r => r.TenantId == tenantId && r.IsActive).CountAsync(),
            OpenAlerts = await _db.RiskIndicatorAlerts.Where(a => a.Indicator.TenantId == tenantId && a.Status == "Open").CountAsync(),

            // Exceptions (Shahin FIX)
            OpenExceptions = await _db.ControlExceptions.Where(e => e.TenantId == tenantId && e.Status == "Approved").CountAsync(),

            // Recent Activities
            RecentAssessments = await _db.Assessments
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .ToListAsync(),

            RecentWorkflows = await _db.WorkflowInstances
                .Where(w => w.TenantId == tenantId)
                .OrderByDescending(w => w.Id)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }

    /// <summary>
    /// Start Smart Onboarding Process
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartOnboarding()
    {
        var tenantId = GetCurrentTenantId();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

        try
        {
            var result = await _onboarding.CompleteSmartOnboardingAsync(tenantId, userId);
            TempData["Success"] = $"Smart onboarding completed! Generated {result.GeneratedTemplates?.Count ?? 0} templates.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during smart onboarding");
            TempData["Error"] = "An error occurred. Please try again.";
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// View Onboarding Profile
    /// </summary>
    public async Task<IActionResult> OnboardingProfile()
    {
        var tenantId = GetCurrentTenantId();
        var profile = await _db.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);
        return View(profile);
    }

    /// <summary>
    /// View Assessments
    /// </summary>
    public async Task<IActionResult> Assessments()
    {
        var tenantId = GetCurrentTenantId();
        var assessments = await _db.Assessments
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.CreatedDate)
            .Take(50)
            .ToListAsync();
        return View(assessments);
    }

    /// <summary>
    /// View Workflows
    /// </summary>
    public async Task<IActionResult> Workflows()
    {
        var tenantId = GetCurrentTenantId();
        var workflows = await _db.WorkflowInstances
            .Where(w => w.TenantId == tenantId)
            .OrderByDescending(w => w.Id)
            .Take(5)
            .ToListAsync();
        return View(workflows);
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

/// <summary>
/// View Model for Integrated Flow Dashboard
/// </summary>
public class IntegratedFlowViewModel
{
    // Onboarding
    public GrcMvc.Models.Entities.OrganizationProfile? OrganizationProfile { get; set; }
    public bool OnboardingComplete { get; set; }

    // Derived Scope
    public int DerivedBaselines { get; set; }
    public int DerivedPackages { get; set; }

    // Plans & Assessments
    public int ActivePlans { get; set; }
    public int TotalAssessments { get; set; }
    public int InProgressAssessments { get; set; }

    // Workflows
    public int ActiveWorkflows { get; set; }
    public int PendingTasks { get; set; }

    // Evidence
    public int TotalEvidence { get; set; }
    public int ApprovedEvidence { get; set; }

    // Controls
    public int TotalControls { get; set; }

    // Indicators
    public int TotalIndicators { get; set; }
    public int OpenAlerts { get; set; }

    // Exceptions
    public int OpenExceptions { get; set; }

    // Recent Activities
    public List<GrcMvc.Models.Entities.Assessment> RecentAssessments { get; set; } = new();
    public List<GrcMvc.Models.Entities.WorkflowInstance> RecentWorkflows { get; set; } = new();
}
