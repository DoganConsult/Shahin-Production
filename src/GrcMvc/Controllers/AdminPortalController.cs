using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Controllers;

/// <summary>
/// Admin Portal Controller - For login.shahin-ai.com
/// Manages platform-level administration: tenants, users, subscriptions
/// Routes: /admin/login, /admin/dashboard, /admin/tenants
/// </summary>
public class AdminPortalController : Controller
{
    private readonly GrcDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AdminPortalController> _logger;
    private readonly IEnvironmentVariableService _envVarService;
    private readonly ICacheClearService _cacheClearService;
    private readonly IConfigurationRoot _configurationRoot;

    public AdminPortalController(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AdminPortalController> logger,
        IEnvironmentVariableService envVarService,
        ICacheClearService cacheClearService,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _envVarService = envVarService;
        _cacheClearService = cacheClearService;
        _configurationRoot = configuration as IConfigurationRoot ?? throw new InvalidOperationException("IConfiguration must be IConfigurationRoot");
    }

    /// <summary>
    /// Platform Admin Login Page
    /// Route: /admin/login (via conventional routing)
    /// </summary>
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in as platform admin, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("PlatformAdmin"))
        {
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

        // Check if user is a platform admin
        var isPlatformAdmin = await _userManager.IsInRoleAsync(user, "PlatformAdmin") ||
                              await _userManager.IsInRoleAsync(user, "Admin");

        if (!isPlatformAdmin)
        {
            ModelState.AddModelError(string.Empty, "ليس لديك صلاحية الوصول لهذه الصفحة");
            _logger.LogWarning("Non-admin user {Email} attempted platform admin login", model.Email);
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
    /// </summary>
    [Authorize(Roles = "PlatformAdmin,Admin")]
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
    /// List all tenants
    /// Route: /admin/tenants
    /// </summary>
    [Authorize(Roles = "PlatformAdmin,Admin")]
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
    /// </summary>
    [Authorize(Roles = "PlatformAdmin,Admin")]
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
    /// Environment Variables Management
    /// Route: /admin/environment-variables
    /// </summary>
    [Authorize(Roles = "PlatformAdmin,Admin")]
    public async Task<IActionResult> EnvironmentVariables()
    {
        try
        {
            var variables = await _envVarService.GetAllVariablesAsync();
            var envFilePath = _envVarService.GetEnvFilePath();
            var isWritable = _envVarService.IsWritable();

            ViewBag.EnvFilePath = envFilePath;
            ViewBag.IsWritable = isWritable;
            ViewBag.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            ViewBag.EnvVarService = _envVarService; // For GetAbpSettingName method

            return View(variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading environment variables");
            TempData["Error"] = "Failed to load environment variables";
            return View(new Dictionary<string, List<EnvironmentVariableItem>>());
        }
    }

    /// <summary>
    /// Update Environment Variables
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnvironmentVariables(Dictionary<string, string> variables, bool useAbpSettings = true, bool clearCache = true)
    {
        try
        {
            _logger.LogInformation("[CONFIG] 🔄 Starting environment variable update...");
            _logger.LogInformation("[CONFIG] 📊 Updating {Count} variables (ABP Settings: {UseAbp}, Clear Cache: {ClearCache})", 
                variables.Count, useAbpSettings, clearCache);

            // Update variables
            await _envVarService.UpdateVariablesAsync(variables, useAbpSettings);
            
            // Reload configuration to pick up new values
            _logger.LogInformation("[CONFIG] 🔄 Reloading IConfiguration to pick up new values...");
            _configurationRoot.Reload();
            _logger.LogInformation("[CONFIG] ✅ IConfiguration reloaded");

            // Clear caches if requested
            if (clearCache)
            {
                _logger.LogInformation("[CONFIG] 🗑️  Clearing configuration caches...");
                await _cacheClearService.ClearConfigurationCacheAsync();
                _logger.LogInformation("[CONFIG] ✅ Caches cleared");
            }

            // Log updated variables (masked)
            foreach (var kvp in variables)
            {
                var masked = kvp.Key.Contains("Secret") || kvp.Key.Contains("Password") || kvp.Key.Contains("Key")
                    ? "***" 
                    : kvp.Value;
                _logger.LogInformation("[CONFIG] ✅ Updated: {Key} = {Value}", kvp.Key, masked);
            }
            
            var message = useAbpSettings
                ? $"Successfully updated {variables.Count} variable(s) in ABP Settings (encrypted in database). Changes take effect immediately. Caches cleared."
                : $"Successfully updated {variables.Count} variable(s) in .env file. Configuration reloaded. Caches cleared.";
            
            TempData["Success"] = message;
            
            return RedirectToAction(nameof(EnvironmentVariables));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONFIG] ❌ Error updating environment variables");
            TempData["Error"] = $"Failed to update environment variables: {ex.Message}";
            return RedirectToAction(nameof(EnvironmentVariables));
        }
    }

    /// <summary>
    /// Clear all caches
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAllCaches()
    {
        try
        {
            _logger.LogInformation("[CACHE] 🗑️  Clearing all caches...");
            await _cacheClearService.ClearAllCachesAsync();
            
            // Reload configuration
            _configurationRoot.Reload();
            
            TempData["Success"] = "All caches cleared and configuration reloaded. New environment variables will be picked up.";
            return RedirectToAction(nameof(EnvironmentVariables));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CACHE] ❌ Error clearing caches");
            TempData["Error"] = $"Failed to clear caches: {ex.Message}";
            return RedirectToAction(nameof(EnvironmentVariables));
        }
    }

    /// <summary>
    /// Migrate environment variables to ABP Settings
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MigrateToAbpSettings()
    {
        try
        {
            await _envVarService.MigrateToAbpSettingsAsync();
            _logger.LogInformation("Platform admin migrated environment variables to ABP Settings");
            TempData["Success"] = "Successfully migrated environment variables to ABP Settings (encrypted in database).";
            
            return RedirectToAction(nameof(EnvironmentVariables));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating to ABP Settings");
            TempData["Error"] = $"Failed to migrate: {ex.Message}";
            return RedirectToAction(nameof(EnvironmentVariables));
        }
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
