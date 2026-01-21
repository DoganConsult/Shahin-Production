using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;

namespace GrcMvc.Controllers;

/// <summary>
/// GRC Maturity Assessment Controller
/// CMM (Capability Maturity Model) Levels 1-5 visualization and assessment
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class MaturityController : Controller
{
    private readonly IGrcProcessOrchestrator _grcOrchestrator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MaturityController> _logger;

    public MaturityController(
        IGrcProcessOrchestrator grcOrchestrator,
        ICurrentUserService currentUserService,
        ILogger<MaturityController> logger)
    {
        _grcOrchestrator = grcOrchestrator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Default - redirect to CMM view
    /// </summary>
    [HttpGet]
    [Authorize(GrcPermissions.Maturity.View)]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(CMM));
    }

    /// <summary>
    /// CMM (Capability Maturity Model) visualization
    /// Shows maturity levels 1-5 with dimension scores
    /// </summary>
    [HttpGet("CMM")]
    [Authorize(GrcPermissions.Maturity.View)]
    public async Task<IActionResult> CMM()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();
            var maturityScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);
            return View(maturityScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CMM maturity view for tenant {TenantId}", _currentUserService.GetTenantId());
            TempData["Error"] = "Error loading maturity data";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    /// <summary>
    /// Maturity improvement roadmap
    /// Shows recommendations and action items to improve maturity level
    /// </summary>
    [HttpGet("Roadmap")]
    [Authorize(GrcPermissions.Maturity.Roadmap)]
    public async Task<IActionResult> Roadmap()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();
            var maturityScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);
            return View(maturityScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading maturity roadmap for tenant {TenantId}", _currentUserService.GetTenantId());
            TempData["Error"] = "Error loading roadmap data";
            return RedirectToAction(nameof(CMM));
        }
    }

    /// <summary>
    /// Maturity dimensions view
    /// Detailed breakdown by dimension: Governance, Risk, Compliance, Operations, Technology
    /// </summary>
    [HttpGet("Dimensions")]
    [Authorize(GrcPermissions.Maturity.View)]
    public async Task<IActionResult> Dimensions()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();
            var maturityScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);
            return View(maturityScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dimensions view for tenant {TenantId}", _currentUserService.GetTenantId());
            TempData["Error"] = "Error loading dimensions data";
            return RedirectToAction(nameof(CMM));
        }
    }

    /// <summary>
    /// Assessment form view
    /// </summary>
    [HttpGet("Assessment")]
    [Authorize(GrcPermissions.Maturity.Assess)]
    public async Task<IActionResult> Assessment()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();
            var currentScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);
            ViewBag.CurrentScore = currentScore;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assessment view");
            TempData["Error"] = "Error loading assessment form";
            return RedirectToAction(nameof(CMM));
        }
    }

    /// <summary>
    /// Run maturity assessment
    /// </summary>
    [HttpPost("Assessment")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Maturity.Assess)]
    public async Task<IActionResult> RunAssessment()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();

            // Trigger maturity assessment calculation
            var maturityScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);

            TempData["Success"] = $"Maturity assessment completed. Current Level: {maturityScore.OverallLevel} ({maturityScore.LevelName})";
            return RedirectToAction(nameof(CMM));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running maturity assessment for tenant {TenantId}", _currentUserService.GetTenantId());
            TempData["Error"] = "Error running assessment";
            return RedirectToAction(nameof(Assessment));
        }
    }

    /// <summary>
    /// Set maturity baseline for comparison
    /// </summary>
    [HttpPost("SetBaseline")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Maturity.Baseline)]
    public async Task<IActionResult> SetBaseline()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();
            var currentScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);

            // Store baseline in session/TempData for comparison
            // In production, this would be persisted to database
            TempData["BaselineLevel"] = currentScore.OverallLevel;
            TempData["BaselineScore"] = currentScore.Score;
            TempData["BaselineDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

            TempData["Success"] = $"Baseline set at Level {currentScore.OverallLevel} ({currentScore.Score}/100) on {DateTime.UtcNow:yyyy-MM-dd}";
            return RedirectToAction(nameof(CMM));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting maturity baseline");
            TempData["Error"] = "Error setting baseline";
            return RedirectToAction(nameof(CMM));
        }
    }

    /// <summary>
    /// API endpoint to get maturity score as JSON
    /// </summary>
    [HttpGet("Score")]
    [Authorize(GrcPermissions.Maturity.View)]
    public async Task<IActionResult> GetScore()
    {
        try
        {
            var tenantId = _currentUserService.GetTenantId();
            var maturityScore = await _grcOrchestrator.GetMaturityScoreAsync(tenantId);
            return Json(maturityScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maturity score");
            return Json(new { error = "Error loading maturity score" });
        }
    }
}
