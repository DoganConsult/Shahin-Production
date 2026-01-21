using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Assessment Template Controller
/// Manage assessment templates for audits and compliance assessments
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class AssessmentTemplateController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<AssessmentTemplateController> _logger;

    public AssessmentTemplateController(GrcDbContext db, ILogger<AssessmentTemplateController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Assessment Templates List
    /// </summary>
    [HttpGet]
    [Authorize(GrcPermissions.Assessments.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var templates = await _db.TemplateCatalogs
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            ViewBag.TotalTemplates = templates.Count;
            return View(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assessment templates");
            TempData["Error"] = "Error loading templates";
            return View(new List<Models.Entities.Catalogs.TemplateCatalog>());
        }
    }

    /// <summary>
    /// View template details
    /// </summary>
    [HttpGet("Details/{id}")]
    [Authorize(GrcPermissions.Assessments.View)]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var template = await _db.TemplateCatalogs.FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
            {
                TempData["Error"] = "Template not found";
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template {TemplateId}", id);
            TempData["Error"] = "Error loading template details";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Create new assessment from template
    /// </summary>
    [HttpGet("CreateAssessment/{templateId}")]
    [Authorize(GrcPermissions.Assessments.Create)]
    public async Task<IActionResult> CreateAssessment(Guid templateId)
    {
        try
        {
            var template = await _db.TemplateCatalogs.FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
            {
                TempData["Error"] = "Template not found";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Template = template;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create assessment form for template {TemplateId}", templateId);
            TempData["Error"] = "Error loading form";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Submit new assessment
    /// </summary>
    [HttpPost("CreateAssessment/{templateId}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Assessments.Create)]
    public async Task<IActionResult> CreateAssessmentPost(Guid templateId, [FromForm] string name)
    {
        try
        {
            var template = await _db.TemplateCatalogs.FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
            {
                TempData["Error"] = "Template not found";
                return RedirectToAction(nameof(Index));
            }

            var tenantId = GetCurrentTenantId();
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

            var assessment = new Assessment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name ?? $"Assessment from {template.TemplateName}",
                Status = "Draft",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _db.Assessments.Add(assessment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Assessment {AssessmentId} created from template {TemplateId} by user {UserId}",
                assessment.Id, templateId, userId);

            TempData["Success"] = $"Assessment '{assessment.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment from template {TemplateId}", templateId);
            TempData["Error"] = "Error creating assessment";
            return RedirectToAction("CreateAssessment", new { templateId });
        }
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}
