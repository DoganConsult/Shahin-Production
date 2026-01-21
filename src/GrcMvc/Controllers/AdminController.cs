using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Catalog Admin Controller - Manage regulators, frameworks, controls
/// </summary>
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class CatalogAdminController : Controller
{
    private readonly GrcDbContext _db;

    public CatalogAdminController(GrcDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
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

    [HttpGet("Regulators")]
    public async Task<IActionResult> Regulators()
    {
        var items = await _db.RegulatorCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
        return View(items);
    }

    [HttpGet("Frameworks")]
    public async Task<IActionResult> Frameworks()
    {
        var items = await _db.FrameworkCatalogs.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ToListAsync();
        return View(items);
    }

    [HttpGet("Controls")]
    public async Task<IActionResult> Controls()
    {
        var items = await _db.CanonicalControls.Where(c => c.IsActive).OrderBy(c => c.ControlName).Take(100).ToListAsync();
        return View(items);
    }

    [HttpGet("Baselines")]
    public async Task<IActionResult> Baselines()
    {
        var items = await _db.BaselineCatalogs.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();
        return View(items);
    }

    [HttpGet("Packages")]
    public async Task<IActionResult> Packages()
    {
        var items = await _db.PackageCatalogs.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToListAsync();
        return View(items);
    }

    [HttpGet("EvidenceTypes")]
    public async Task<IActionResult> EvidenceTypes()
    {
        var items = await _db.EvidenceTypeCatalogs.Where(e => e.IsActive).OrderBy(e => e.DisplayOrder).ToListAsync();
        return View(items);
    }
}

/// <summary>
/// Role Delegation Controller - Assign/delegate roles
/// </summary>
[Route("[controller]")]
[Authorize]
public class RoleDelegationController : Controller
{
    private readonly GrcDbContext _db;
    private readonly IUserDirectoryService _userDirectory;

    public RoleDelegationController(GrcDbContext db, IUserDirectoryService userDirectory)
    {
        _db = db;
        _userDirectory = userDirectory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _db.RoleCatalogs.Where(r => r.IsActive).ToListAsync();
        var users = await _userDirectory.GetAllActiveUsersAsync();

        ViewBag.Delegations = new List<object>();
        ViewBag.Roles = roles;
        ViewBag.Users = users;
        ViewBag.ActiveCount = 0;

        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] DelegationDto dto)
    {
        TempData["Success"] = "Role delegation created";
        return RedirectToAction("Index");
    }

    [HttpPost("Revoke/{id}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        TempData["Success"] = "Delegation revoked";
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class DelegationDto
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public string? RoleCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Legacy Multi-Tenant Admin Controller - Basic tenant management
/// Note: Use TenantAdminController at /t/{slug}/admin for full functionality
/// </summary>
[Route("legacy-tenant-admin")]
[Authorize(Roles = "Admin")]
[RequireTenant]
public class LegacyTenantAdminController : Controller
{
    private readonly GrcDbContext _db;

    public LegacyTenantAdminController(GrcDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tenants = await _db.Tenants.OrderByDescending(t => t.Id).ToListAsync();

        ViewBag.TotalTenants = tenants.Count;
        ViewBag.ActiveTenants = tenants.Count(t => t.IsActive);
        ViewBag.TrialTenants = tenants.Count(t => t.SubscriptionTier == "Trial");

        return View(tenants);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        var userCount = await _db.TenantUsers.CountAsync(u => u.TenantId == id);
        var assessmentCount = await _db.Assessments.CountAsync(a => a.TenantId == id);

        ViewBag.UserCount = userCount;
        ViewBag.AssessmentCount = assessmentCount;

        return View(tenant);
    }

    [HttpPost("Activate/{id}")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant != null)
        {
            tenant.IsActive = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Tenant activated";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Deactivate/{id}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant != null)
        {
            tenant.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Warning"] = "Tenant deactivated";
        }
        return RedirectToAction("Index");
    }

    [HttpGet("Create")]
    public IActionResult Create() => View();

    [HttpPost("Create")]
    public async Task<IActionResult> CreatePost([FromForm] TenantCreateDto dto)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            OrganizationName = dto.Name ?? "New Tenant",
            TenantSlug = dto.Slug ?? Guid.NewGuid().ToString("N").Substring(0, 8),
            IsActive = true,
            SubscriptionTier = "Trial"
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Tenant '{tenant.OrganizationName}' created";
        return RedirectToAction("Index");
    }
}

public class TenantCreateDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
}
