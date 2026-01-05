using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services;
using GrcMvc.Services.Implementations;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Shahin-AI Dashboard Controller
/// Main entry point for the Shahin-AI platform
/// </summary>
public class ShahinAIController : Controller
{
    private readonly IShahinAIOrchestrationService _orchestrationService;
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<ShahinAIController> _logger;

    public ShahinAIController(
        IShahinAIOrchestrationService orchestrationService,
        GrcDbContext dbContext,
        ILogger<ShahinAIController> logger)
    {
        _orchestrationService = orchestrationService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Main Dashboard - Overview of all 6 modules
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var tenantId = GetCurrentTenantId();
        var dashboard = await _orchestrationService.GetCompleteDashboardAsync(tenantId);
        return View(dashboard);
    }

    /// <summary>
    /// MAP Module - Control Library
    /// </summary>
    public async Task<IActionResult> Map()
    {
        var controls = await _dbContext.CanonicalControls
            .Where(c => c.IsActive)
            .OrderBy(c => c.ControlName)
            .Take(100)
            .ToListAsync();

        ViewBag.TotalControls = controls.Count();
        return View(controls);
    }

    /// <summary>
    /// APPLY Module - Scope & Applicability
    /// </summary>
    public async Task<IActionResult> Apply()
    {
        var tenantId = GetCurrentTenantId();
        var entries = await _dbContext.ApplicabilityEntries
            .Where(a => a.TenantId == tenantId)
            .Take(100)
            .ToListAsync();

        ViewBag.TotalEntries = entries.Count;
        return View(entries);
    }

    /// <summary>
    /// PROVE Module - Evidence & Testing
    /// </summary>
    public async Task<IActionResult> Prove()
    {
        var tenantId = GetCurrentTenantId();
        var evidence = await _dbContext.AutoTaggedEvidences
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.CapturedAt)
            .Take(50)
            .ToListAsync();

        var packs = await _dbContext.UniversalEvidencePacks
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

        ViewBag.EvidenceCount = evidence.Count;
        ViewBag.PackCount = packs.Count;
        ViewBag.Packs = packs;
        return View(evidence);
    }

    /// <summary>
    /// WATCH Module - Monitoring & Alerts
    /// </summary>
    public async Task<IActionResult> Watch()
    {
        var tenantId = GetCurrentTenantId();
        var indicators = await _dbContext.RiskIndicators
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync();

        var alerts = await _dbContext.RiskIndicatorAlerts
            .Include(a => a.Indicator)
            .Where(a => a.Indicator.TenantId == tenantId && a.Status == "Open")
            .OrderByDescending(a => a.TriggeredAt)
            .Take(20)
            .ToListAsync();

        ViewBag.IndicatorCount = indicators.Count;
        ViewBag.AlertCount = alerts.Count;
        ViewBag.Alerts = alerts;
        return View(indicators);
    }

    /// <summary>
    /// FIX Module - Remediation & Exceptions
    /// </summary>
    public async Task<IActionResult> Fix()
    {
        var tenantId = GetCurrentTenantId();
        var exceptions = await _dbContext.ControlExceptions
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.Id)
            .Take(50)
            .ToListAsync();

        var ccmExceptions = await _dbContext.CCMExceptions
            .Where(e => e.Status == "Open")
            .Take(20)
            .ToListAsync();

        ViewBag.ExceptionCount = exceptions.Count;
        ViewBag.CCMExceptionCount = ccmExceptions.Count;
        ViewBag.CCMExceptions = ccmExceptions;
        return View(exceptions);
    }

    /// <summary>
    /// VAULT Module - Evidence Repository
    /// </summary>
    public async Task<IActionResult> Vault()
    {
        var tenantId = GetCurrentTenantId();
        var evidence = await _dbContext.CapturedEvidences
            .Where(e => e.TenantId == tenantId && e.IsCurrent)
            .OrderByDescending(e => e.CapturedAt)
            .Take(50)
            .ToListAsync();

        ViewBag.TotalEvidence = evidence.Count;
        return View(evidence);
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

/// <summary>
/// Shahin-AI API Controller
/// REST endpoints for module operations
/// </summary>
[ApiController]
[Route("api/shahin")]
[Authorize]
public class ShahinAIApiController : ControllerBase
{
    private readonly IShahinAIOrchestrationService _orchestrationService;
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<ShahinAIApiController> _logger;

    public ShahinAIApiController(
        IShahinAIOrchestrationService orchestrationService,
        GrcDbContext dbContext,
        ILogger<ShahinAIApiController> logger)
    {
        _orchestrationService = orchestrationService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get complete dashboard data
    /// </summary>
    [HttpGet("dashboard/{tenantId}")]
    public async Task<IActionResult> GetDashboard(Guid tenantId)
    {
        var dashboard = await _orchestrationService.GetCompleteDashboardAsync(tenantId);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get MAP module statistics
    /// </summary>
    [HttpGet("map/stats")]
    public async Task<IActionResult> GetMapStats()
    {
        var result = await _orchestrationService.GetControlLibraryAsync(Guid.Empty);
        return Ok(result);
    }

    /// <summary>
    /// Get PROVE module statistics
    /// </summary>
    [HttpGet("prove/stats/{tenantId}")]
    public async Task<IActionResult> GetProveStats(Guid tenantId)
    {
        var result = await _orchestrationService.GetEvidenceStatusAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get WATCH module statistics
    /// </summary>
    [HttpGet("watch/stats/{tenantId}")]
    public async Task<IActionResult> GetWatchStats(Guid tenantId)
    {
        var result = await _orchestrationService.GetMonitoringDashboardAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get FIX module statistics
    /// </summary>
    [HttpGet("fix/stats/{tenantId}")]
    public async Task<IActionResult> GetFixStats(Guid tenantId)
    {
        var result = await _orchestrationService.GetRemediationStatusAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get VAULT module statistics
    /// </summary>
    [HttpGet("vault/stats/{tenantId}")]
    public async Task<IActionResult> GetVaultStats(Guid tenantId)
    {
        var result = await _orchestrationService.GetVaultStatisticsAsync(tenantId);
        return Ok(result);
    }

    /// <summary>
    /// Get all Shahin-AI modules
    /// </summary>
    [HttpGet("modules")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModules()
    {
        var modules = await _dbContext.ShahinAIModules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new
            {
                m.ModuleCode,
                m.Name,
                m.NameAr,
                m.ShortDescription,
                m.ShortDescriptionAr,
                m.IconClass,
                m.ModuleColor,
                m.RoutePath
            })
            .ToListAsync();

        return Ok(modules);
    }

    /// <summary>
    /// Get brand configuration
    /// </summary>
    [HttpGet("brand")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBrandConfig()
    {
        var brand = await _dbContext.ShahinAIBrandConfigs
            .Where(b => b.IsActive)
            .FirstOrDefaultAsync();

        return Ok(brand);
    }
}
