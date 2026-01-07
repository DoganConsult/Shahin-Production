using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrcMvc.Controllers;

/// <summary>
/// MVC Controller for Platform Admin UI
/// Super power feature - comprehensive multi-tenant administration
/// </summary>
[Authorize(Policy = "ActivePlatformAdmin")]
[Route("platform-admin")]
public class PlatformAdminController : Controller
{
    private readonly GrcDbContext _context;
    private readonly IUserDirectoryService _userDirectory;
    private readonly IPlatformAdminService _platformAdminService;
    private readonly IAuditEventService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<PlatformAdminController> _logger;

    public PlatformAdminController(
        GrcDbContext context,
        IUserDirectoryService userDirectory,
        IPlatformAdminService platformAdminService,
        IAuditEventService auditService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<PlatformAdminController> logger)
    {
        _context = context;
        _userDirectory = userDirectory;
        _platformAdminService = platformAdminService;
        _auditService = auditService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private string? GetClientIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    #region Dashboard

    /// <summary>
    /// Platform Admin Dashboard - Overview of all tenants and system health
    /// </summary>
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        var admin = await _platformAdminService.GetByUserIdAsync(userId);

        if (admin == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var model = new PlatformDashboardViewModel
        {
            Admin = admin,
            TotalTenants = await _context.Tenants.CountAsync(t => !t.IsDeleted),
            ActiveTenants = await _context.Tenants.CountAsync(t => t.IsActive && !t.IsDeleted),
            PendingTenants = await _context.Tenants.CountAsync(t => t.Status == "Pending" && !t.IsDeleted),
            SuspendedTenants = await _context.Tenants.CountAsync(t => t.Status == "Suspended" && !t.IsDeleted),
            TotalUsers = await _userDirectory.GetUserCountAsync(),
            ActiveUsers = await _userDirectory.GetUserCountAsync(), // Note: GetUserCountAsync returns all users, would need separate method for active only
            TotalPlatformAdmins = await _context.PlatformAdmins.CountAsync(p => !p.IsDeleted),
            RecentTenants = await _context.Tenants
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync(),
            RecentAuditEvents = await _context.AuditEvents
                .OrderByDescending(e => e.Timestamp)
                .Take(10)
                .ToListAsync()
        };

        return View(model);
    }

    #endregion

    #region Tenant Management

    /// <summary>
    /// List all tenants with filtering
    /// </summary>
    [HttpGet("tenants")]
    public async Task<IActionResult> Tenants(string? status = null, string? search = null)
    {
        var query = _context.Tenants
            .Include(t => t.Users)
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.OrganizationName.Contains(search) || t.TenantSlug.Contains(search));

        var tenants = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.SearchFilter = search;
        return View(tenants);
    }

    /// <summary>
    /// View tenant details
    /// </summary>
    [HttpGet("tenants/{id:guid}")]
    public async Task<IActionResult> TenantDetails(Guid id)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Users)
            .ThenInclude(tu => tu.User)
            .Include(t => t.OrganizationProfile)
            .Include(t => t.Plans)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null)
            return NotFound();

        var model = new TenantDetailsViewModel
        {
            Tenant = tenant,
            UserCount = tenant.Users.Count,
            ActiveUserCount = tenant.Users.Count(u => u.Status == "Active"),
            PlanCount = tenant.Plans.Count,
            RecentActivity = await _context.AuditEvents
                .Where(e => e.TenantId == id)
                .OrderByDescending(e => e.Timestamp)
                .Take(20)
                .ToListAsync()
        };

        return View(model);
    }

    /// <summary>
    /// Create tenant form
    /// </summary>
    [HttpGet("tenants/create")]
    public IActionResult CreateTenant()
    {
        return View(new CreateTenantViewModel());
    }

    /// <summary>
    /// Create tenant POST
    /// </summary>
    [HttpPost("tenants/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTenant(CreateTenantViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        var canCreate = await _platformAdminService.CanCreateTenantAsync(userId);
        if (!canCreate)
        {
            ModelState.AddModelError("", "You don't have permission to create tenants or quota exceeded");
            return View(model);
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            OrganizationName = model.OrganizationName,
            TenantSlug = model.TenantSlug.ToLower().Replace(" ", "-"),
            AdminEmail = model.AdminEmail,
            Status = "Pending",
            IsActive = true,
            SubscriptionTier = model.SubscriptionTier,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionEndDate = model.SubscriptionEndDate,
            CreatedByOwnerId = userId,
            IsOwnerCreated = true,
            BypassPayment = model.BypassPayment,
            ActivationToken = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        await _platformAdminService.IncrementTenantCreatedCountAsync(userId);

        // Audit log
        await _auditService.LogPlatformAdminActionAsync(
            userId,
            PlatformAuditEventTypes.TenantCreated,
            "Create",
            $"Created tenant: {model.OrganizationName} ({model.TenantSlug})",
            tenant.Id,
            ipAddress: GetClientIpAddress());

        TempData["Success"] = $"Tenant '{model.OrganizationName}' created successfully";
        return RedirectToAction(nameof(TenantDetails), new { id = tenant.Id });
    }

    /// <summary>
    /// Edit tenant
    /// </summary>
    [HttpGet("tenants/{id:guid}/edit")]
    public async Task<IActionResult> EditTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        var model = new EditTenantViewModel
        {
            Id = tenant.Id,
            OrganizationName = tenant.OrganizationName,
            TenantSlug = tenant.TenantSlug,
            AdminEmail = tenant.AdminEmail,
            Status = tenant.Status,
            SubscriptionTier = tenant.SubscriptionTier,
            SubscriptionEndDate = tenant.SubscriptionEndDate,
            BypassPayment = tenant.BypassPayment
        };

        return View(model);
    }

    /// <summary>
    /// Edit tenant POST
    /// </summary>
    [HttpPost("tenants/{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTenant(Guid id, EditTenantViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        tenant.OrganizationName = model.OrganizationName;
        tenant.AdminEmail = model.AdminEmail;
        tenant.Status = model.Status;
        tenant.SubscriptionTier = model.SubscriptionTier;
        tenant.SubscriptionEndDate = model.SubscriptionEndDate;
        tenant.BypassPayment = model.BypassPayment;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Tenant updated successfully";
        return RedirectToAction(nameof(TenantDetails), new { id });
    }

    /// <summary>
    /// Suspend tenant
    /// </summary>
    [HttpPost("tenants/{id:guid}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendTenant(Guid id, string reason)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        tenant.Status = "Suspended";
        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogPlatformAdminActionAsync(
            GetCurrentUserId(),
            PlatformAuditEventTypes.TenantSuspended,
            "Suspend",
            $"Suspended tenant: {tenant.OrganizationName}. Reason: {reason}",
            id,
            ipAddress: GetClientIpAddress());

        _logger.LogWarning("Tenant {TenantId} suspended. Reason: {Reason}", id, reason);
        TempData["Warning"] = $"Tenant suspended: {reason}";
        return RedirectToAction(nameof(TenantDetails), new { id });
    }

    /// <summary>
    /// Activate tenant
    /// </summary>
    [HttpPost("tenants/{id:guid}/activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        tenant.Status = "Active";
        tenant.IsActive = true;
        tenant.ActivatedAt = DateTime.UtcNow;
        tenant.ActivatedBy = GetCurrentUserId();
        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogPlatformAdminActionAsync(
            GetCurrentUserId(),
            PlatformAuditEventTypes.TenantActivated,
            "Activate",
            $"Activated tenant: {tenant.OrganizationName}",
            id,
            ipAddress: GetClientIpAddress());

        TempData["Success"] = "Tenant activated successfully";
        return RedirectToAction(nameof(TenantDetails), new { id });
    }

    /// <summary>
    /// Delete tenant (soft delete)
    /// </summary>
    [HttpPost("tenants/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var userId = GetCurrentUserId();
        var hasPermission = await _platformAdminService.HasPermissionAsync(userId, PlatformPermission.DeleteTenants);
        if (!hasPermission)
        {
            TempData["Error"] = "You don't have permission to delete tenants";
            return RedirectToAction(nameof(TenantDetails), new { id });
        }

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;
        tenant.Status = "Deleted";
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogPlatformAdminActionAsync(
            userId,
            PlatformAuditEventTypes.TenantDeleted,
            "Delete",
            $"Deleted tenant: {tenant.OrganizationName}",
            id,
            ipAddress: GetClientIpAddress());

        _logger.LogWarning("Tenant {TenantId} deleted by {UserId}", id, userId);
        TempData["Success"] = "Tenant deleted successfully";
        return RedirectToAction(nameof(Tenants));
    }

    /// <summary>
    /// Reset tenant admin credentials
    /// </summary>
    [HttpPost("tenants/{id:guid}/reset-admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetTenantAdmin(Guid id)
    {
        var userId = GetCurrentUserId();
        var (success, newPassword) = await _platformAdminService.RestartTenantAdminAccountAsync(userId, id);

        if (success)
        {
            TempData["Success"] = $"Admin credentials reset. New password: {newPassword} (expires in 72 hours)";
        }
        else
        {
            TempData["Error"] = "Failed to reset admin credentials";
        }

        return RedirectToAction(nameof(TenantDetails), new { id });
    }

    #endregion

    #region User Management

    /// <summary>
    /// List all users across tenants
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> Users(Guid? tenantId = null, string? search = null, int page = 1)
    {
        const int pageSize = 50;
        var query = _context.TenantUsers
            .Include(tu => tu.User)
            .Include(tu => tu.Tenant)
            .Where(tu => !tu.IsDeleted);

        if (tenantId.HasValue)
            query = query.Where(tu => tu.TenantId == tenantId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(tu => tu.User.Email.Contains(search) || tu.User.FullName.Contains(search));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var users = await query
            .OrderByDescending(tu => tu.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TenantId = tenantId;
        ViewBag.SearchFilter = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        ViewBag.Tenants = await _context.Tenants.Where(t => !t.IsDeleted).ToListAsync();
        return View(users);
    }

    /// <summary>
    /// View user details
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var tenantUsers = await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .Where(tu => tu.UserId == id && !tu.IsDeleted)
            .ToListAsync();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserDetailsViewModel
        {
            User = user,
            TenantMemberships = tenantUsers,
            Roles = roles.ToList(),
            RecentActivity = await _context.AuditEvents
                .Where(e => e.UserId == id)
                .OrderByDescending(e => e.Timestamp)
                .Take(20)
                .ToListAsync()
        };

        return View(model);
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    [HttpPost("users/{id}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetUserPassword(string id, string newPassword)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _platformAdminService.ResetPasswordAsync(adminUserId, id, newPassword);

        if (result)
        {
            // Force user to change password on next login
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.MustChangePassword = true;
                await _userManager.UpdateAsync(user);
            }

            // Audit log
            await _auditService.LogPlatformAdminActionAsync(
                adminUserId,
                PlatformAuditEventTypes.PasswordReset,
                "ResetPassword",
                $"Reset password for user: {id}",
                targetUserId: id,
                ipAddress: GetClientIpAddress());

            TempData["Success"] = "Password reset successfully. User must change password on next login.";
        }
        else
        {
            TempData["Error"] = "Failed to reset password - check permissions";
        }

        return RedirectToAction(nameof(UserDetails), new { id });
    }

    /// <summary>
    /// Force user to change password on next login
    /// </summary>
    [HttpPost("users/{id}/force-password-change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForcePasswordChange(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        // Audit log
        await _auditService.LogPlatformAdminActionAsync(
            GetCurrentUserId(),
            PlatformAuditEventTypes.PasswordReset,
            "ForcePasswordChange",
            $"Forced password change for user: {user.Email}",
            targetUserId: id,
            ipAddress: GetClientIpAddress());

        _logger.LogInformation("Admin forced password change for user {Email}", user.Email);
        TempData["Success"] = $"User {user.Email} must change password on next login";
        return RedirectToAction(nameof(UserDetails), new { id });
    }

    /// <summary>
    /// Deactivate user
    /// </summary>
    [HttpPost("users/{id}/deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "User deactivated";
        return RedirectToAction(nameof(UserDetails), new { id });
    }

    /// <summary>
    /// Activate user
    /// </summary>
    [HttpPost("users/{id}/activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "User activated";
        return RedirectToAction(nameof(UserDetails), new { id });
    }

    #endregion

    #region Platform Admins

    /// <summary>
    /// List platform admins
    /// </summary>
    [HttpGet("admins")]
    public async Task<IActionResult> Admins()
    {
        var admins = await _platformAdminService.GetAllAsync();
        return View(admins);
    }

    /// <summary>
    /// Create platform admin
    /// </summary>
    [HttpGet("admins/create")]
    public async Task<IActionResult> CreateAdmin()
    {
        var superAdminUsers = await _userManager.GetUsersInRoleAsync("PlatformAdmin");
        var existingAdminIds = await _context.PlatformAdmins.Select(p => p.UserId).ToListAsync();

        ViewBag.AvailableUsers = superAdminUsers.Where(u => !existingAdminIds.Contains(u.Id)).ToList();
        return View(new CreatePlatformAdminDto());
    }

    /// <summary>
    /// Create platform admin POST
    /// </summary>
    [HttpPost("admins/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreatePlatformAdminDto model)
    {
        if (!ModelState.IsValid)
        {
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("PlatformAdmin");
            var existingAdminIds = await _context.PlatformAdmins.Select(p => p.UserId).ToListAsync();
            ViewBag.AvailableUsers = superAdminUsers.Where(u => !existingAdminIds.Contains(u.Id)).ToList();
            return View(model);
        }

        try
        {
            var creatorId = GetCurrentUserId();
            await _platformAdminService.CreateAsync(model, creatorId);
            TempData["Success"] = "Platform admin created successfully";
            return RedirectToAction(nameof(Admins));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("PlatformAdmin");
            var existingAdminIds = await _context.PlatformAdmins.Select(p => p.UserId).ToListAsync();
            ViewBag.AvailableUsers = superAdminUsers.Where(u => !existingAdminIds.Contains(u.Id)).ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Suspend platform admin
    /// </summary>
    [HttpPost("admins/{id:guid}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendAdmin(Guid id, string reason)
    {
        var userId = GetCurrentUserId();
        var hasPermission = await _platformAdminService.HasPermissionAsync(userId, PlatformPermission.ManagePlatformAdmins);
        if (!hasPermission)
        {
            TempData["Error"] = "You don't have permission to suspend admins";
            return RedirectToAction(nameof(Admins));
        }

        var result = await _platformAdminService.SuspendAsync(id, reason);
        if (result)
        {
            await _auditService.LogPlatformAdminActionAsync(
                userId,
                PlatformAuditEventTypes.AdminSuspended,
                "Suspend",
                $"Suspended platform admin: {id}. Reason: {reason}",
                ipAddress: GetClientIpAddress());

            TempData["Success"] = "Admin suspended successfully";
        }
        else
        {
            TempData["Error"] = "Failed to suspend admin - cannot suspend Owner";
        }
        return RedirectToAction(nameof(Admins));
    }

    /// <summary>
    /// Reactivate platform admin
    /// </summary>
    [HttpPost("admins/{id:guid}/reactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivateAdmin(Guid id)
    {
        var userId = GetCurrentUserId();
        var hasPermission = await _platformAdminService.HasPermissionAsync(userId, PlatformPermission.ManagePlatformAdmins);
        if (!hasPermission)
        {
            TempData["Error"] = "You don't have permission to reactivate admins";
            return RedirectToAction(nameof(Admins));
        }

        var result = await _platformAdminService.ReactivateAsync(id);
        if (result)
        {
            await _auditService.LogPlatformAdminActionAsync(
                userId,
                PlatformAuditEventTypes.AdminReactivated,
                "Reactivate",
                $"Reactivated platform admin: {id}",
                ipAddress: GetClientIpAddress());

            TempData["Success"] = "Admin reactivated successfully";
        }
        else
        {
            TempData["Error"] = "Failed to reactivate admin";
        }
        return RedirectToAction(nameof(Admins));
    }

    #endregion

    #region Audit Logs

    /// <summary>
    /// View audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs(
        Guid? tenantId = null,
        string? userId = null,
        string? eventType = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1)
    {
        const int pageSize = 100;
        var query = _context.AuditEvents.AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(e => e.TenantId == tenantId);
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(e => e.UserId == userId);
        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(e => e.EventType == eventType);
        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var logs = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Lookup user names for actor IDs
        var actorIds = logs
            .Where(e => !string.IsNullOrEmpty(e.Actor) && e.Actor != "SYSTEM" && Guid.TryParse(e.Actor, out _))
            .Select(e => e.Actor)
            .Distinct()
            .ToList();

        var userNames = new Dictionary<string, string>();
        if (actorIds.Any())
        {
            var users = await _userManager.Users
                .Where(u => actorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email, u.FullName })
                .ToListAsync();

            foreach (var user in users)
            {
                userNames[user.Id] = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email;
            }
        }

        ViewBag.Tenants = await _context.Tenants.Where(t => !t.IsDeleted).ToListAsync();
        ViewBag.EventTypes = await _context.AuditEvents.Select(e => e.EventType).Distinct().ToListAsync();
        ViewBag.ActorNames = userNames;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        ViewBag.FilterTenantId = tenantId;
        ViewBag.FilterUserId = userId;
        ViewBag.FilterEventType = eventType;
        ViewBag.FilterFrom = from;
        ViewBag.FilterTo = to;
        return View(logs);
    }

    #endregion

    #region Impersonation

    /// <summary>
    /// Impersonate a tenant admin
    /// </summary>
    [HttpPost("impersonate/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Impersonate(string userId)
    {
        var adminUserId = GetCurrentUserId();
        var hasPermission = await _platformAdminService.HasPermissionAsync(adminUserId, PlatformPermission.ImpersonateUsers);
        if (!hasPermission)
        {
            TempData["Error"] = "You don't have permission to impersonate users";
            return RedirectToAction(nameof(Users));
        }

        var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
            return NotFound();

        // Store original admin ID in session for returning
        HttpContext.Session.SetString("OriginalAdminId", adminUserId);
        HttpContext.Session.SetString("IsImpersonating", "true");

        // Audit log
        await _auditService.LogPlatformAdminActionAsync(
            adminUserId,
            PlatformAuditEventTypes.UserImpersonated,
            "Impersonate",
            $"Started impersonating user: {targetUser.Email}",
            targetUserId: userId,
            ipAddress: GetClientIpAddress());

        // Sign in as the target user
        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(targetUser, isPersistent: false);

        _logger.LogWarning("Admin {AdminId} impersonating user {UserId}", adminUserId, userId);
        TempData["Warning"] = $"You are now impersonating {targetUser.Email}. Click 'End Impersonation' to return.";

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// End impersonation and return to admin
    /// </summary>
    [HttpPost("end-impersonation")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> EndImpersonation()
    {
        var originalAdminId = HttpContext.Session.GetString("OriginalAdminId");
        if (string.IsNullOrEmpty(originalAdminId))
        {
            return RedirectToAction("Login", "Account");
        }

        var adminUser = await _userManager.FindByIdAsync(originalAdminId);
        if (adminUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        HttpContext.Session.Remove("OriginalAdminId");
        HttpContext.Session.Remove("IsImpersonating");

        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(adminUser, isPersistent: false);

        TempData["Success"] = "Impersonation ended. You are back to your admin account.";
        return RedirectToAction(nameof(Dashboard));
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Bulk suspend tenants
    /// </summary>
    [HttpPost("tenants/bulk-suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkSuspendTenants([FromForm] List<Guid> tenantIds, [FromForm] string reason)
    {
        var userId = GetCurrentUserId();
        var successCount = 0;
        var failCount = 0;

        foreach (var tenantId in tenantIds)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null && tenant.Status != "Suspended")
            {
                tenant.Status = "Suspended";
                tenant.IsActive = false;
                tenant.UpdatedAt = DateTime.UtcNow;
                successCount++;

                await _auditService.LogPlatformAdminActionAsync(
                    userId,
                    PlatformAuditEventTypes.TenantSuspended,
                    "BulkSuspend",
                    $"Bulk suspended tenant: {tenant.OrganizationName}. Reason: {reason}",
                    tenantId,
                    ipAddress: GetClientIpAddress());
            }
            else
            {
                failCount++;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Bulk suspend complete: {successCount} suspended, {failCount} skipped";
        return RedirectToAction(nameof(Tenants));
    }

    /// <summary>
    /// Bulk activate tenants
    /// </summary>
    [HttpPost("tenants/bulk-activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkActivateTenants([FromForm] List<Guid> tenantIds)
    {
        var userId = GetCurrentUserId();
        var successCount = 0;

        foreach (var tenantId in tenantIds)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null && tenant.Status != "Active")
            {
                tenant.Status = "Active";
                tenant.IsActive = true;
                tenant.ActivatedAt = DateTime.UtcNow;
                tenant.ActivatedBy = userId;
                tenant.UpdatedAt = DateTime.UtcNow;
                successCount++;

                await _auditService.LogPlatformAdminActionAsync(
                    userId,
                    PlatformAuditEventTypes.TenantActivated,
                    "BulkActivate",
                    $"Bulk activated tenant: {tenant.OrganizationName}",
                    tenantId,
                    ipAddress: GetClientIpAddress());
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Bulk activate complete: {successCount} tenants activated";
        return RedirectToAction(nameof(Tenants));
    }

    #endregion

    #region Export Reports

    /// <summary>
    /// Export tenants report as CSV
    /// </summary>
    [HttpGet("export/tenants")]
    public async Task<IActionResult> ExportTenantsReport()
    {
        var tenants = await _context.Tenants
            .Include(t => t.Users)
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.OrganizationName)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Id,OrganizationName,TenantSlug,AdminEmail,Status,SubscriptionTier,UserCount,CreatedAt,ActivatedAt");

        foreach (var t in tenants)
        {
            csv.AppendLine($"\"{t.Id}\",\"{t.OrganizationName}\",\"{t.TenantSlug}\",\"{t.AdminEmail}\",\"{t.Status}\",\"{t.SubscriptionTier}\",{t.Users.Count},\"{t.CreatedAt:yyyy-MM-dd}\",\"{t.ActivatedAt:yyyy-MM-dd}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"tenants-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export users report as CSV
    /// </summary>
    [HttpGet("export/users")]
    public async Task<IActionResult> ExportUsersReport()
    {
        var users = await _context.TenantUsers
            .Include(tu => tu.User)
            .Include(tu => tu.Tenant)
            .Where(tu => !tu.IsDeleted)
            .OrderBy(tu => tu.Tenant.OrganizationName)
            .ThenBy(tu => tu.User.Email)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("UserId,Email,FullName,TenantName,RoleCode,Status,CreatedAt,LastLogin");

        foreach (var tu in users)
        {
            csv.AppendLine($"\"{tu.UserId}\",\"{tu.User?.Email}\",\"{tu.User?.FullName}\",\"{tu.Tenant?.OrganizationName}\",\"{tu.RoleCode}\",\"{tu.Status}\",\"{tu.CreatedAt:yyyy-MM-dd}\",\"{tu.User?.LastLoginDate:yyyy-MM-dd}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"users-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export audit logs as CSV
    /// </summary>
    [HttpGet("export/audit-logs")]
    public async Task<IActionResult> ExportAuditLogsReport(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AuditEvents.AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        var logs = await query
            .OrderByDescending(e => e.Timestamp)
            .Take(10000)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Id,Timestamp,EventType,Actor,Action,Description,TenantId,IpAddress");

        foreach (var log in logs)
        {
            csv.AppendLine($"\"{log.Id}\",\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.EventType}\",\"{log.Actor}\",\"{log.Action}\",\"{log.Description?.Replace("\"", "\"\"")}\",\"{log.TenantId}\",\"{log.IpAddress}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    #endregion

    #region System Health

    /// <summary>
    /// System health metrics dashboard
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> SystemHealth()
    {
        var model = new SystemHealthViewModel
        {
            DatabaseStatus = await CheckDatabaseHealthAsync(),
            TotalTenants = await _context.Tenants.CountAsync(t => !t.IsDeleted),
            ActiveTenants = await _context.Tenants.CountAsync(t => t.IsActive && !t.IsDeleted),
            TotalUsers = await _userDirectory.GetUserCountAsync(),
            TotalAuditEvents = await _context.AuditEvents.CountAsync(),
            RecentErrors = await _context.AuditEvents
                .Where(e => e.EventType.Contains("Error") || e.EventType.Contains("Exception"))
                .OrderByDescending(e => e.Timestamp)
                .Take(10)
                .ToListAsync(),
            ServerTime = DateTime.UtcNow,
            UptimeHours = (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalHours,
            MemoryUsageMb = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024
        };

        return View(model);
    }

    /// <summary>
    /// Health check API endpoint
    /// </summary>
    [HttpGet("health/api")]
    [AllowAnonymous]
    public async Task<IActionResult> HealthCheckApi()
    {
        var dbHealthy = await CheckDatabaseHealthAsync();

        return Ok(new
        {
            status = dbHealthy ? "healthy" : "unhealthy",
            timestamp = DateTime.UtcNow,
            database = dbHealthy ? "ok" : "error",
            uptime = (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalHours
        });
    }

    private async Task<bool> CheckDatabaseHealthAsync()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region System Settings

    /// <summary>
    /// System configuration
    /// </summary>
    [HttpGet("settings")]
    public IActionResult Settings()
    {
        return View();
    }

    /// <summary>
    /// Feature flags management
    /// </summary>
    [HttpGet("feature-flags")]
    public async Task<IActionResult> FeatureFlags()
    {
        // This would load feature flags from a configuration store
        var tenants = await _context.Tenants.Where(t => !t.IsDeleted).ToListAsync();
        return View(tenants);
    }

    #endregion
}

#region View Models

public class PlatformDashboardViewModel
{
    public PlatformAdmin Admin { get; set; } = null!;
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int PendingTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalPlatformAdmins { get; set; }
    public List<Tenant> RecentTenants { get; set; } = new();
    public List<AuditEvent> RecentAuditEvents { get; set; } = new();
}

public class TenantDetailsViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public int UserCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int PlanCount { get; set; }
    public List<AuditEvent> RecentActivity { get; set; } = new();
}

public class CreateTenantViewModel
{
    public string OrganizationName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = "MVP";
    public DateTime? SubscriptionEndDate { get; set; }
    public bool BypassPayment { get; set; } = true;
}

public class EditTenantViewModel
{
    public Guid Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = string.Empty;
    public DateTime? SubscriptionEndDate { get; set; }
    public bool BypassPayment { get; set; }
}

public class UserDetailsViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<TenantUser> TenantMemberships { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<AuditEvent> RecentActivity { get; set; } = new();
}

public class SystemHealthViewModel
{
    public bool DatabaseStatus { get; set; }
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAuditEvents { get; set; }
    public List<AuditEvent> RecentErrors { get; set; } = new();
    public DateTime ServerTime { get; set; }
    public double UptimeHours { get; set; }
    public long MemoryUsageMb { get; set; }
}

#endregion
