using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Reports & Analytics Controller
/// Compliance scores, trends, drill-downs
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class ReportsController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(GrcDbContext db, ILogger<ReportsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var assessments = await _db.Assessments.Where(a => a.TenantId == tenantId).ToListAsync();
            var evidence = await _db.AutoTaggedEvidences.Where(e => e.TenantId == tenantId).ToListAsync();
            var exceptions = await _db.ControlExceptions.Where(e => e.TenantId == tenantId).ToListAsync();

            ViewBag.TotalAssessments = assessments.Count;
            ViewBag.CompletedAssessments = assessments.Count(a => a.Status == "Completed");
            ViewBag.TotalEvidence = evidence.Count;
            ViewBag.ApprovedEvidence = evidence.Count(e => e.Status == "Approved");
            ViewBag.OpenExceptions = exceptions.Count(e => e.Status == "Approved" && e.ExpiryDate > DateTime.UtcNow);

            var complianceScore = assessments.Count > 0
                ? (decimal)assessments.Count(a => a.Status == "Completed") / assessments.Count * 100
                : 0;
            ViewBag.ComplianceScore = Math.Round(complianceScore, 1);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports dashboard");
            TempData["Error"] = "Error loading reports";
            return View();
        }
    }

    [HttpGet("Compliance")]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Compliance()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var baselines = await _db.TenantBaselines.Where(b => b.TenantId == tenantId).ToListAsync();
            var packages = await _db.TenantPackages.Where(p => p.TenantId == tenantId).ToListAsync();

            ViewBag.Baselines = baselines;
            ViewBag.Packages = packages;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading compliance report");
            TempData["Error"] = "Error loading compliance report";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("Evidence")]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Evidence()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var evidence = await _db.AutoTaggedEvidences
                .Where(e => e.TenantId == tenantId)
                .OrderByDescending(e => e.CapturedAt)
                .Take(100)
                .ToListAsync();
            return View(evidence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading evidence report");
            TempData["Error"] = "Error loading evidence report";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("Risks")]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Risks()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var risks = await _db.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .OrderByDescending(r => r.InherentRiskScore)
                .Take(50)
                .ToListAsync();

            ViewBag.TotalRisks = risks.Count;
            ViewBag.CriticalRisks = risks.Count(r => r.InherentRiskScore >= 20);
            ViewBag.HighRisks = risks.Count(r => r.InherentRiskScore >= 15 && r.InherentRiskScore < 20);
            ViewBag.MediumRisks = risks.Count(r => r.InherentRiskScore >= 10 && r.InherentRiskScore < 15);
            ViewBag.LowRisks = risks.Count(r => r.InherentRiskScore < 10);

            return View(risks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading risks report");
            TempData["Error"] = "Error loading risks report";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("Controls")]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Controls()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var controls = await _db.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .OrderBy(c => c.ControlCode)
                .Take(100)
                .ToListAsync();

            ViewBag.TotalControls = controls.Count;
            ViewBag.ImplementedControls = controls.Count(c => c.ImplementationStatus == "Implemented");
            ViewBag.PartialControls = controls.Count(c => c.ImplementationStatus == "Partial");
            ViewBag.NotImplementedControls = controls.Count(c => c.ImplementationStatus == "Not Implemented");

            return View(controls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading controls report");
            TempData["Error"] = "Error loading controls report";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("Executive")]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Executive()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            // Summary metrics
            var assessments = await _db.Assessments.Where(a => a.TenantId == tenantId).ToListAsync();
            var risks = await _db.Risks.Where(r => r.TenantId == tenantId && !r.IsDeleted).ToListAsync();
            var controls = await _db.Controls.Where(c => c.TenantId == tenantId && !c.IsDeleted).ToListAsync();
            var exceptions = await _db.ControlExceptions.Where(e => e.TenantId == tenantId).ToListAsync();

            ViewBag.AssessmentStats = new
            {
                Total = assessments.Count,
                Completed = assessments.Count(a => a.Status == "Completed"),
                InProgress = assessments.Count(a => a.Status == "In Progress"),
                ComplianceRate = assessments.Count > 0 ? Math.Round((decimal)assessments.Count(a => a.Status == "Completed") / assessments.Count * 100, 1) : 0
            };

            ViewBag.RiskStats = new
            {
                Total = risks.Count,
                Critical = risks.Count(r => r.InherentRiskScore >= 20),
                High = risks.Count(r => r.InherentRiskScore >= 15 && r.InherentRiskScore < 20),
                AverageScore = risks.Count > 0 ? Math.Round(risks.Average(r => r.InherentRiskScore ?? 0), 1) : 0
            };

            ViewBag.ControlStats = new
            {
                Total = controls.Count,
                Implemented = controls.Count(c => c.ImplementationStatus == "Implemented"),
                ImplementationRate = controls.Count > 0 ? Math.Round((decimal)controls.Count(c => c.ImplementationStatus == "Implemented") / controls.Count * 100, 1) : 0
            };

            ViewBag.ExceptionStats = new
            {
                Total = exceptions.Count,
                Open = exceptions.Count(e => e.Status == "Approved" && e.ExpiryDate > DateTime.UtcNow),
                Pending = exceptions.Count(e => e.Status == "Pending"),
                Expired = exceptions.Count(e => e.ExpiryDate <= DateTime.UtcNow)
            };

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading executive report");
            TempData["Error"] = "Error loading executive report";
            return RedirectToAction("Index");
        }
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}
