using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

// NOTE: CatalogAdminController has been moved to CatalogAdminController.cs
// NOTE: RoleDelegationController has been moved to RoleDelegationController.cs

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
