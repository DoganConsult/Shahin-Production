using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers;

/// <summary>
/// MVC Controller for Platform Admin UI - handles views for admin management
/// Route: /platform-admin/*
/// </summary>
[Authorize(Policy = "ActivePlatformAdmin")]
[Route("platform-admin")]
public class PlatformAdminMvcController : Controller
{
    private readonly IPlatformAdminService _platformAdminService;
    private readonly ITenantService _tenantService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GrcDbContext _context;
    private readonly ILogger<PlatformAdminMvcController> _logger;

    public PlatformAdminMvcController(
        IPlatformAdminService platformAdminService,
        ITenantService tenantService,
        UserManager<ApplicationUser> userManager,
        GrcDbContext context,
        ILogger<PlatformAdminMvcController> logger)
    {
        _platformAdminService = platformAdminService;
        _tenantService = tenantService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    /// <summary>
    /// Dashboard
    /// </summary>
    [HttpGet]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var admins = await _platformAdminService.GetActiveAdminsAsync();
        ViewBag.AdminCount = admins.Count;
        ViewBag.TenantCount = await _context.Set<Volo.Abp.TenantManagement.Tenant>().CountAsync();
        return View("~/Views/PlatformAdmin/Dashboard.cshtml");
    }

    /// <summary>
    /// Endpoint Management - View all API endpoints
    /// </summary>
    [HttpGet("endpoints")]
    public IActionResult Endpoints()
    {
        return View("~/Views/PlatformAdmin/Endpoints.cshtml");
    }

    /// <summary>
    /// Support Tickets Management - List all tickets
    /// </summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets(
        string? status = null,
        string? priority = null,
        string? category = null,
        string? assignedTo = null,
        int page = 1,
        int pageSize = 50)
    {
        var ticketService = HttpContext.RequestServices.GetRequiredService<ISupportTicketService>();
        
        var filter = new TicketFilterDto
        {
            Status = status,
            Priority = priority,
            Category = category,
            AssignedToUserId = assignedTo,
            Page = page,
            PageSize = pageSize
        };

        var tickets = await ticketService.GetTicketsAsync(filter);
        var stats = await ticketService.GetStatisticsAsync();

        ViewBag.Stats = stats;
        ViewBag.StatusFilter = status;
        ViewBag.PriorityFilter = priority;
        ViewBag.CategoryFilter = category;
        ViewBag.AssignedToFilter = assignedTo;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;

        // Get platform admins for assignment dropdown
        var platformAdmins = await _context.PlatformAdmins
            .Include(pa => pa.User)
            .Where(pa => pa.IsActive && !pa.IsDeleted)
            .ToListAsync();
        ViewBag.PlatformAdmins = platformAdmins;

        return View("~/Views/PlatformAdmin/Tickets.cshtml", tickets);
    }

    /// <summary>
    /// Support Ticket Details - View and manage a single ticket
    /// </summary>
    [HttpGet("tickets/{id}")]
    public async Task<IActionResult> TicketDetails(Guid id)
    {
        var ticketService = HttpContext.RequestServices.GetRequiredService<ISupportTicketService>();
        var ticket = await ticketService.GetTicketByIdAsync(id);

        if (ticket == null)
        {
            return NotFound();
        }

        // Get platform admins for assignment dropdown
        var platformAdmins = await _context.PlatformAdmins
            .Include(pa => pa.User)
            .Where(pa => pa.IsActive && !pa.IsDeleted)
            .ToListAsync();
        ViewBag.PlatformAdmins = platformAdmins;

        return View("~/Views/PlatformAdmin/TicketDetails.cshtml", ticket);
    }

    /// <summary>
    /// Support Ticket Dashboard - Metrics and statistics
    /// </summary>
    [HttpGet("tickets/dashboard")]
    public async Task<IActionResult> TicketDashboard()
    {
        var ticketService = HttpContext.RequestServices.GetRequiredService<ISupportTicketService>();
        var stats = await ticketService.GetStatisticsAsync();
        var ticketsRequiringAttention = await ticketService.GetTicketsRequiringAttentionAsync();

        ViewBag.Stats = stats;
        ViewBag.TicketsRequiringAttention = ticketsRequiringAttention;

        return View("~/Views/PlatformAdmin/TicketDashboard.cshtml");
    }

    /// <summary>
    /// SLA Compliance Report
    /// </summary>
    [HttpGet("tickets/reports/sla")]
    public async Task<IActionResult> SlaComplianceReport(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var ticketService = HttpContext.RequestServices.GetRequiredService<ISupportTicketService>();
        
        var filter = new TicketStatisticsFilterDto
        {
            FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
            ToDate = toDate ?? DateTime.UtcNow
        };

        var stats = await ticketService.GetStatisticsAsync(filter);

        // Get detailed SLA metrics
        var allTickets = await ticketService.GetTicketsAsync(new TicketFilterDto
        {
            CreatedFrom = filter.FromDate,
            CreatedTo = filter.ToDate
        });

        var slaMetrics = new
        {
            TotalTickets = allTickets.Count,
            BreachedTickets = allTickets.Count(t => t.SlaBreached),
            BreachRate = allTickets.Count > 0 ? (double)allTickets.Count(t => t.SlaBreached) / allTickets.Count * 100 : 0,
            ResolvedTickets = allTickets.Where(t => t.ResolvedAt.HasValue).ToList(),
            AvgResolutionTime = allTickets.Where(t => t.ResolvedAt.HasValue)
                .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                .DefaultIfEmpty(0)
                .Average(),
            TicketsByPriority = allTickets.GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => new
                {
                    Total = g.Count(),
                    Breached = g.Count(t => t.SlaBreached),
                    AvgResolutionHours = g.Where(t => t.ResolvedAt.HasValue)
                        .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average()
                })
        };

        ViewBag.SlaMetrics = slaMetrics;
        ViewBag.FromDate = filter.FromDate;
        ViewBag.ToDate = filter.ToDate;

        return View("~/Views/PlatformAdmin/SlaComplianceReport.cshtml", stats);
    }

    /// <summary>
    /// List all Platform Admins
    /// </summary>
    [HttpGet("admins")]
    public async Task<IActionResult> Admins()
    {
        var admins = await _platformAdminService.GetAllAsync();
        return View("~/Views/PlatformAdmin/Admins.cshtml", admins);
    }

    /// <summary>
    /// Create Platform Admin - GET (show form)
    /// </summary>
    [HttpGet("admins/create")]
    public async Task<IActionResult> CreateAdmin()
    {
        // Get users who are not already platform admins
        var existingAdminUserIds = await _context.PlatformAdmins
            .Where(p => !p.IsDeleted)
            .Select(p => p.UserId)
            .ToListAsync();

        var availableUsers = await _context.Users
            .Where(u => u.IsActive && !existingAdminUserIds.Contains(u.Id))
            .OrderBy(u => u.Email)
            .ToListAsync();

        ViewBag.AvailableUsers = availableUsers;
        return View("~/Views/PlatformAdmin/CreateAdmin.cshtml", new CreatePlatformAdminDto());
    }

    /// <summary>
    /// Create Platform Admin - POST (process form)
    /// </summary>
    [HttpPost("admins/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreatePlatformAdminDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAvailableUsers();
            return View("~/Views/PlatformAdmin/CreateAdmin.cshtml", dto);
        }

        try
        {
            var creatorId = GetCurrentUserId();
            var admin = await _platformAdminService.CreateAsync(dto, creatorId);

            TempData["Success"] = $"Platform Admin '{admin.DisplayName}' created successfully.";
            _logger.LogInformation("Platform Admin created: {AdminId} by {CreatorId}", admin.Id, creatorId);

            return RedirectToAction(nameof(Admins));
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError("", ex.Message);
            _logger.LogWarning("Unauthorized attempt to create platform admin: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred creating the admin. Please try again.");
            _logger.LogError(ex, "Error creating platform admin");
        }

        await PopulateAvailableUsers();
        return View("~/Views/PlatformAdmin/CreateAdmin.cshtml", dto);
    }

    /// <summary>
    /// List all tenants
    /// </summary>
    [HttpGet("tenants")]
    public async Task<IActionResult> Tenants()
    {
        var tenants = await _context.Set<Volo.Abp.TenantManagement.Tenant>()
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
        return View("~/Views/PlatformAdmin/Tenants.cshtml", tenants);
    }

    /// <summary>
    /// Create Tenant - GET
    /// </summary>
    [HttpGet("tenants/create")]
    public IActionResult CreateTenant()
    {
        return View("~/Views/PlatformAdmin/CreateTenant.cshtml");
    }

    /// <summary>
    /// Create Tenant - POST
    /// </summary>
    [HttpPost("tenants/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTenant(string organizationName, string adminEmail, string tenantSlug)
    {
        if (string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(tenantSlug))
        {
            ModelState.AddModelError("", "All fields are required.");
            return View("~/Views/PlatformAdmin/CreateTenant.cshtml");
        }

        try
        {
            var tenant = await _tenantService.CreateTenantAsync(organizationName, adminEmail, tenantSlug);
            TempData["Success"] = $"Tenant '{organizationName}' created successfully. Activation email sent to {adminEmail}.";
            return RedirectToAction(nameof(Tenants));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            _logger.LogError(ex, "Error creating tenant");
            return View("~/Views/PlatformAdmin/CreateTenant.cshtml");
        }
    }

    /// <summary>
    /// List all users
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedDate)
            .Take(100)
            .ToListAsync();
        return View("~/Views/PlatformAdmin/Users.cshtml", users);
    }

    /// <summary>
    /// User details
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Roles = roles;

        return View("~/Views/PlatformAdmin/UserDetails.cshtml", user);
    }

    /// <summary>
    /// Audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs()
    {
        var logs = await _context.Set<AuditEvent>()
            .OrderByDescending(a => a.CreatedDate)
            .Take(100)
            .ToListAsync();
        return View("~/Views/PlatformAdmin/AuditLogs.cshtml", logs);
    }

    /// <summary>
    /// Settings
    /// </summary>
    [HttpGet("settings")]
    public IActionResult Settings()
    {
        return View("~/Views/PlatformAdmin/Settings.cshtml");
    }

    #region Role Management

    /// <summary>
    /// List all roles
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> Roles()
    {
        var roles = await _context.Set<RoleCatalog>()
            .Include(r => r.AllowedTitles)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync();
        return View("~/Views/PlatformAdmin/Roles.cshtml", roles);
    }

    /// <summary>
    /// Create Role - GET
    /// </summary>
    [HttpGet("roles/create")]
    public IActionResult CreateRole()
    {
        return View("~/Views/PlatformAdmin/CreateRole.cshtml", new RoleCatalog());
    }

    /// <summary>
    /// Create Role - POST
    /// </summary>
    [HttpPost("roles/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(RoleCatalog model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/PlatformAdmin/CreateRole.cshtml", model);

        try
        {
            model.Id = Guid.NewGuid();
            model.RoleCode = model.RoleCode.ToUpperInvariant().Replace(" ", "_");
            model.CreatedDate = DateTime.UtcNow;

            _context.Set<RoleCatalog>().Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Role '{model.RoleName}' created successfully.";
            _logger.LogInformation("Role created: {RoleCode} by {UserId}", model.RoleCode, GetCurrentUserId());

            return RedirectToAction(nameof(Roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            ModelState.AddModelError("", "Error creating role. Please try again.");
            return View("~/Views/PlatformAdmin/CreateRole.cshtml", model);
        }
    }

    /// <summary>
    /// Edit Role - GET
    /// </summary>
    [HttpGet("roles/{id}/edit")]
    public async Task<IActionResult> EditRole(Guid id)
    {
        var role = await _context.Set<RoleCatalog>()
            .Include(r => r.AllowedTitles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound();

        return View("~/Views/PlatformAdmin/EditRole.cshtml", role);
    }

    /// <summary>
    /// Edit Role - POST
    /// </summary>
    [HttpPost("roles/{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(Guid id, RoleCatalog model)
    {
        if (id != model.Id)
            return BadRequest();

        var existing = await _context.Set<RoleCatalog>().FindAsync(id);
        if (existing == null)
            return NotFound();

        try
        {
            existing.RoleName = model.RoleName;
            existing.Description = model.Description;
            existing.Layer = model.Layer;
            existing.Department = model.Department;
            existing.ApprovalLevel = model.ApprovalLevel;
            existing.CanApprove = model.CanApprove;
            existing.CanReject = model.CanReject;
            existing.CanEscalate = model.CanEscalate;
            existing.CanReassign = model.CanReassign;
            existing.DisplayOrder = model.DisplayOrder;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Role '{model.RoleName}' updated successfully.";
            return RedirectToAction(nameof(Roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            ModelState.AddModelError("", "Error updating role.");
            return View("~/Views/PlatformAdmin/EditRole.cshtml", model);
        }
    }

    /// <summary>
    /// Delete Role - POST
    /// </summary>
    [HttpPost("roles/{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _context.Set<RoleCatalog>().FindAsync(id);
        if (role == null)
            return NotFound();

        try
        {
            role.IsDeleted = true;
            role.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Role '{role.RoleName}' deleted.";
            return RedirectToAction(nameof(Roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            TempData["Error"] = "Error deleting role.";
            return RedirectToAction(nameof(Roles));
        }
    }

    #endregion

    private async Task PopulateAvailableUsers()
    {
        var existingAdminUserIds = await _context.PlatformAdmins
            .Where(p => !p.IsDeleted)
            .Select(p => p.UserId)
            .ToListAsync();

        ViewBag.AvailableUsers = await _context.Users
            .Where(u => u.IsActive && !existingAdminUserIds.Contains(u.Id))
            .OrderBy(u => u.Email)
            .ToListAsync();
    }
}
