using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers;

/// <summary>
/// Platform Admin Tenant Management Controller
/// Allows PlatformAdmin to create, view, and manage all tenants
/// </summary>
[Route("platform/tenants")]
[Authorize(Roles = "PlatformAdmin,Admin")]
public class PlatformTenantsController : Controller
{
    private readonly GrcDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly ILogger<PlatformTenantsController> _logger;

    public PlatformTenantsController(
        GrcDbContext dbContext,
        ITenantService tenantService,
        ILogger<PlatformTenantsController> logger)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// List all tenants
    /// GET /platform/tenants
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? status = null, string? search = null, int page = 1)
    {
        try
        {
            var pageSize = 20;
            var query = _dbContext.Tenants.Where(t => !t.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t =>
                    t.OrganizationName.Contains(search) ||
                    t.TenantSlug.Contains(search));

            var totalCount = await query.CountAsync();
            var tenants = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get user counts for each tenant
            var tenantIds = tenants.Select(t => t.Id).ToList();
            var userCounts = await _dbContext.TenantUsers
                .Where(tu => tenantIds.Contains(tu.TenantId) && !tu.IsDeleted)
                .GroupBy(tu => tu.TenantId)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TenantId, x => x.Count);

            ViewBag.UserCounts = userCounts;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.StatusFilter = status;
            ViewBag.SearchFilter = search;

            return View(tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tenants list");
            TempData["Error"] = "Failed to load tenants.";
            return View(new List<Tenant>());
        }
    }

    /// <summary>
    /// Create tenant form
    /// GET /platform/tenants/create
    /// </summary>
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new CreateTenantViewModel());
    }

    /// <summary>
    /// Create tenant
    /// POST /platform/tenants/create
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTenantViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            // Auto-generate slug if not provided
            var slug = string.IsNullOrEmpty(model.TenantSlug)
                ? GenerateSlug(model.OrganizationName)
                : model.TenantSlug.ToLowerInvariant();

            // Check if slug exists
            var slugExists = await _dbContext.Tenants.AnyAsync(t => t.TenantSlug == slug);
            if (slugExists)
            {
                ModelState.AddModelError("TenantSlug", "This slug is already in use.");
                return View(model);
            }

            var tenant = await _tenantService.CreateTenantAsync(
                model.OrganizationName,
                model.AdminEmail,
                slug);

            // Set subscription tier if specified
            if (!string.IsNullOrEmpty(model.SubscriptionTier))
            {
                tenant.SubscriptionTier = model.SubscriptionTier;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Platform admin created tenant {TenantId} ({Slug})", tenant.Id, tenant.TenantSlug);
            TempData["Success"] = $"Tenant '{model.OrganizationName}' created successfully. Activation email sent to {model.AdminEmail}.";

            return RedirectToAction(nameof(Details), new { id = tenant.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            ModelState.AddModelError("", "Failed to create tenant. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Tenant details
    /// GET /platform/tenants/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var tenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                return NotFound();

            // Get stats
            ViewBag.UserCount = await _dbContext.TenantUsers
                .CountAsync(tu => tu.TenantId == id && !tu.IsDeleted);

            ViewBag.ActiveUserCount = await _dbContext.TenantUsers
                .CountAsync(tu => tu.TenantId == id && !tu.IsDeleted && tu.Status == "Active");

            ViewBag.AssessmentCount = await _dbContext.Assessments
                .CountAsync(a => a.TenantId == id && a.DeletedAt == null);

            ViewBag.RiskCount = await _dbContext.Risks
                .CountAsync(r => r.TenantId == id && !r.IsDeleted);

            return View(tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tenant details {TenantId}", id);
            TempData["Error"] = "Failed to load tenant details.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Edit tenant form
    /// GET /platform/tenants/{id}/edit
    /// </summary>
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var tenant = await _dbContext.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        return View(new EditTenantViewModel
        {
            Id = tenant.Id,
            OrganizationName = tenant.OrganizationName,
            TenantSlug = tenant.TenantSlug,
            Status = tenant.Status,
            SubscriptionTier = tenant.SubscriptionTier
        });
    }

    /// <summary>
    /// Update tenant
    /// POST /platform/tenants/{id}/edit
    /// </summary>
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditTenantViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            tenant.OrganizationName = model.OrganizationName;
            tenant.Status = model.Status;
            tenant.SubscriptionTier = model.SubscriptionTier;
            tenant.ModifiedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Platform admin updated tenant {TenantId}", id);
            TempData["Success"] = "Tenant updated successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            ModelState.AddModelError("", "Failed to update tenant.");
            return View(model);
        }
    }

    /// <summary>
    /// Activate tenant manually
    /// POST /platform/tenants/{id}/activate
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            tenant.Status = "Active";
            tenant.ActivatedAt = DateTime.UtcNow;
            tenant.ModifiedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Platform admin manually activated tenant {TenantId}", id);
            TempData["Success"] = "Tenant activated successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating tenant {TenantId}", id);
            TempData["Error"] = "Failed to activate tenant.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Suspend tenant
    /// POST /platform/tenants/{id}/suspend
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(Guid id)
    {
        try
        {
            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            tenant.Status = "Suspended";
            tenant.ModifiedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Platform admin suspended tenant {TenantId}", id);
            TempData["Success"] = "Tenant suspended.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending tenant {TenantId}", id);
            TempData["Error"] = "Failed to suspend tenant.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private static string GenerateSlug(string organizationName)
    {
        return organizationName
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Substring(0, Math.Min(organizationName.Length, 50));
    }
}

#region ViewModels

public class CreateTenantViewModel
{
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string? TenantSlug { get; set; }
    public string SubscriptionTier { get; set; } = "Starter";
}

public class EditTenantViewModel
{
    public Guid Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string SubscriptionTier { get; set; } = "Starter";
}

#endregion
