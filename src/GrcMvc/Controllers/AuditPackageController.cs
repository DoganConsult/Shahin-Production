using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Audit Package Controller
/// Generate and export audit evidence packages
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class AuditPackageController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<AuditPackageController> _logger;
    private readonly PolicyEnforcementHelper _policyHelper;

    public AuditPackageController(GrcDbContext db, ILogger<AuditPackageController> logger, PolicyEnforcementHelper policyHelper)
    {
        _db = db;
        _logger = logger;
        _policyHelper = policyHelper;
    }

    [HttpGet]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var assessments = await _db.Assessments.Where(a => a.TenantId == tenantId && a.Status == "Completed").ToListAsync();
            var evidenceCount = await _db.AutoTaggedEvidences.CountAsync(e => e.TenantId == tenantId && e.Status == "Approved");

            ViewBag.Assessments = assessments;
            ViewBag.EvidenceCount = evidenceCount;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit packages");
            TempData["Error"] = "Error loading audit packages";
            return View();
        }
    }

    [HttpGet("Generate/{assessmentId}")]
    [Authorize(GrcPermissions.Reports.View)]
    public async Task<IActionResult> Generate(Guid assessmentId)
    {
        try
        {
            var assessment = await _db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
            if (assessment == null) return NotFound();

            var requirements = await _db.AssessmentRequirements.Where(r => r.AssessmentId == assessmentId).ToListAsync();
            var evidence = await _db.AutoTaggedEvidences.Where(e => e.TenantId == assessment.TenantId && e.Status == "Approved").ToListAsync();

            ViewBag.Assessment = assessment;
            ViewBag.Requirements = requirements;
            ViewBag.Evidence = evidence;

            return View("Package");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit package for assessment {AssessmentId}", assessmentId);
            TempData["Error"] = "Error generating audit package";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("Export/{assessmentId}")]
    [Authorize(GrcPermissions.Reports.Export)]
    public async Task<IActionResult> Export(Guid assessmentId)
    {
        try
        {
            var assessment = await _db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
            if (assessment == null) return NotFound();

            // POLICY ENFORCEMENT: Check if export is allowed
            await _policyHelper.EnforceAsync("export", "Assessment", assessment,
                dataClassification: assessment.DataClassification ?? "confidential",
                owner: assessment.Owner ?? "System");

            var content = $"Audit Package for {assessment.Name}\nGenerated: {DateTime.UtcNow:yyyy-MM-dd HH:mm}\nStatus: {assessment.Status}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", $"audit-package-{assessment.Id}.txt");
        }
        catch (PolicyViolationException pex)
        {
            _logger.LogWarning(pex, "Policy violation exporting audit package for assessment {AssessmentId}", assessmentId);
            TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report for assessment {AssessmentId}", assessmentId);
            TempData["Error"] = "Error exporting report";
            return RedirectToAction("Index");
        }
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}
