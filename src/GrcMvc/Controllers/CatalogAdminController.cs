using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Application.Permissions;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Catalog Admin Controller - Manage regulators, frameworks, controls
/// Platform-level catalog management (not tenant-specific)
/// </summary>
[Route("[controller]")]
[Authorize(Roles = "Admin,PlatformAdmin")]
public class CatalogAdminController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<CatalogAdminController> _logger;

    public CatalogAdminController(GrcDbContext db, ILogger<CatalogAdminController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.RegulatorCount = await _db.RegulatorCatalogs.CountAsync();
            ViewBag.FrameworkCount = await _db.FrameworkCatalogs.CountAsync();
            ViewBag.ControlCount = await _db.CanonicalControls.CountAsync();
            ViewBag.BaselineCount = await _db.BaselineCatalogs.CountAsync();
            ViewBag.PackageCount = await _db.PackageCatalogs.CountAsync();
            ViewBag.TemplateCount = await _db.TemplateCatalogs.CountAsync();
            ViewBag.EvidenceTypeCount = await _db.EvidenceTypeCatalogs.CountAsync();

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading catalog admin dashboard");
            TempData["Error"] = "Error loading catalog dashboard";
            return View();
        }
    }

    [HttpGet("Regulators")]
    public async Task<IActionResult> Regulators()
    {
        try
        {
            var items = await _db.RegulatorCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading regulators");
            TempData["Error"] = "Error loading regulators";
            return View(new List<RegulatorCatalog>());
        }
    }

    [HttpGet("Frameworks")]
    public async Task<IActionResult> Frameworks()
    {
        try
        {
            var items = await _db.FrameworkCatalogs.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading frameworks");
            TempData["Error"] = "Error loading frameworks";
            return View(new List<FrameworkCatalog>());
        }
    }

    [HttpGet("Controls")]
    public async Task<IActionResult> Controls()
    {
        try
        {
            var items = await _db.CanonicalControls.Where(c => c.IsActive).OrderBy(c => c.ControlName).Take(100).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading canonical controls");
            TempData["Error"] = "Error loading controls";
            return View(new List<CanonicalControl>());
        }
    }

    [HttpGet("Baselines")]
    public async Task<IActionResult> Baselines()
    {
        try
        {
            var items = await _db.BaselineCatalogs.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading baselines");
            TempData["Error"] = "Error loading baselines";
            return View(new List<BaselineCatalog>());
        }
    }

    [HttpGet("Packages")]
    public async Task<IActionResult> Packages()
    {
        try
        {
            var items = await _db.PackageCatalogs.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading packages");
            TempData["Error"] = "Error loading packages";
            return View(new List<PackageCatalog>());
        }
    }

    [HttpGet("EvidenceTypes")]
    public async Task<IActionResult> EvidenceTypes()
    {
        try
        {
            var items = await _db.EvidenceTypeCatalogs.Where(e => e.IsActive).OrderBy(e => e.DisplayOrder).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading evidence types");
            TempData["Error"] = "Error loading evidence types";
            return View(new List<EvidenceTypeCatalog>());
        }
    }

    [HttpGet("Templates")]
    public async Task<IActionResult> Templates()
    {
        try
        {
            var items = await _db.TemplateCatalogs.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading templates");
            TempData["Error"] = "Error loading templates";
            return View(new List<TemplateCatalog>());
        }
    }
}
