using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;

namespace GrcMvc.Controllers;

/// <summary>
/// Admin Portal Controller - For login.shahin-ai.com and admin.shahin-ai.com
/// Manages platform-level administration: tenants, users, subscriptions
/// Routes: /admin/login, /admin/dashboard, /admin/tenants
/// SECURITY: Only active Platform Admins can access (uses ActivePlatformAdmin policy)
/// </summary>
[Authorize(Policy = "ActivePlatformAdmin")]
public class AdminPortalController : Controller
{
    private readonly GrcDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AdminPortalController> _logger;

    public AdminPortalController(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AdminPortalController> logger)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Platform Admin Login Page
    /// Route: /admin/login (via conventional routing)
    /// SECURITY: Public access for login, but only Platform Admins can successfully authenticate
    /// </summary>
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in as active platform admin, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("PlatformAdmin"))
        {
            // Additional check: verify user is an active platform admin
            return RedirectToAction(nameof(Dashboard));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Platform Admin Login POST
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(PlatformAdminLoginModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة");
            return View(model);
        }

        // SECURITY: Only PlatformAdmin role allowed (not just "Admin")
        // Must be an active PlatformAdmin with valid record in database
        var isPlatformAdmin = await _userManager.IsInRoleAsync(user, "PlatformAdmin");

        if (!isPlatformAdmin)
        {
            ModelState.AddModelError(string.Empty, "ليس لديك صلاحية الوصول لهذه الصفحة. فقط مدراء المنصة يمكنهم الوصول.");
            _logger.LogWarning("Non-platform-admin user {Email} attempted platform admin login", model.Email);
            return View(model);
        }

        // Additional security: Verify user has active PlatformAdmin record
        var hasActiveAdminRecord = await _context.PlatformAdmins
            .AnyAsync(pa => pa.UserId == user.Id && pa.Status == "Active" && !pa.IsDeleted);

        if (!hasActiveAdminRecord)
        {
            ModelState.AddModelError(string.Empty, "حسابك غير نشط. يرجى التواصل مع الدعم.");
            _logger.LogWarning("User {Email} has PlatformAdmin role but no active PlatformAdmin record", model.Email);
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, 
            model.Password, 
            model.RememberMe, 
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Platform admin {Email} logged in", model.Email);
            return RedirectToLocal(returnUrl ?? "/admin/dashboard");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Platform admin {Email} account locked out", model.Email);
            return View("Lockout");
        }

        ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة");
        return View(model);
    }

    /// <summary>
    /// Platform Admin Dashboard
    /// Route: /admin/dashboard
    /// SECURITY: Protected by ActivePlatformAdmin policy (class-level)
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        var stats = new PlatformDashboardStats
        {
            TotalTenants = await _context.Tenants.CountAsync(),
            ActiveTenants = await _context.Tenants.CountAsync(t => t.Status == "Active"),
            TrialTenants = await _context.Tenants.CountAsync(t => t.IsTrial),
            TotalUsers = await _context.TenantUsers.CountAsync(),
            RecentTenants = await _context.Tenants
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .Select(t => new TenantSummary
                {
                    Id = t.Id,
                    OrganizationName = t.OrganizationName,
                    AdminEmail = t.AdminEmail,
                    Status = t.Status,
                    OnboardingStatus = t.OnboardingStatus,
                    IsTrial = t.IsTrial,
                    TrialEndsAt = t.TrialEndsAt,
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync()
        };

        return View(stats);
    }

    /// <summary>
    /// Endpoint Management - View all API endpoints
    /// Route: /admin/endpoints
    /// SECURITY: Protected by ActivePlatformAdmin policy (class-level)
    /// </summary>
    public IActionResult Endpoints()
    {
        return View("~/Views/PlatformAdmin/Endpoints.cshtml");
    }

    /// <summary>
    /// List all tenants
    /// Route: /admin/tenants
    /// SECURITY: Protected by ActivePlatformAdmin policy (class-level)
    /// </summary>
    public async Task<IActionResult> Tenants()
    {
        var tenants = await _context.Tenants
            .OrderByDescending(t => t.CreatedDate)
            .Select(t => new TenantSummary
            {
                Id = t.Id,
                OrganizationName = t.OrganizationName,
                AdminEmail = t.AdminEmail,
                Status = t.Status,
                OnboardingStatus = t.OnboardingStatus,
                IsTrial = t.IsTrial,
                TrialEndsAt = t.TrialEndsAt,
                CreatedDate = t.CreatedDate,
                UserCount = t.Users.Count(u => !u.IsDeleted)
            })
            .ToListAsync();

        return View(tenants);
    }

    /// <summary>
    /// View tenant details
    /// Route: /admin/tenantdetails/{id}
    /// SECURITY: Protected by ActivePlatformAdmin policy (class-level)
    /// </summary>
    public async Task<IActionResult> TenantDetails(Guid id)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Users)
            .Include(t => t.OrganizationProfile)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null)
        {
            return NotFound();
        }

        return View(tenant);
    }

    /// <summary>
    /// Platform Admin Logout
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Platform admin logged out");
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Dashboard));
    }
}

#region View Models

public class PlatformAdminLoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class PlatformDashboardStats
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TrialTenants { get; set; }
    public int TotalUsers { get; set; }
    public List<TenantSummary> RecentTenants { get; set; } = new();
}

public class TenantSummary
{
    public Guid Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OnboardingStatus { get; set; } = string.Empty;
    public bool IsTrial { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public int UserCount { get; set; }
}

#endregion
