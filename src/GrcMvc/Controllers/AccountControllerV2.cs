using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Models.Entities;
using GrcMvc.Models.ViewModels;
using GrcMvc.Services.Interfaces;
using GrcMvc.Configuration;
using Microsoft.Extensions.Options;

namespace GrcMvc.Controllers;

/// <summary>
/// V2 Account Controller - Enhanced security implementation
/// Routes: /account/v2/* (parallel to existing /account/*)
/// 
/// Security Enhancements:
/// 1. NO hard-coded credentials (uses configuration/user secrets)
/// 2. Session-based claims (not DB-persisted)
/// 3. Structured logging (no file I/O)
/// 4. Deterministic tenant resolution
/// </summary>
[Route("account/v2")]
public class AccountControllerV2 : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEnhancedAuthService _enhancedAuthService;
    private readonly IEnhancedTenantResolver _tenantResolver;
    private readonly ILogger<AccountControllerV2> _logger;
    private readonly IOptions<GrcFeatureOptions> _featureOptions;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    
    public AccountControllerV2(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEnhancedAuthService enhancedAuthService,
        IEnhancedTenantResolver tenantResolver,
        ILogger<AccountControllerV2> logger,
        IOptions<GrcFeatureOptions> featureOptions,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _enhancedAuthService = enhancedAuthService;
        _tenantResolver = tenantResolver;
        _logger = logger;
        _featureOptions = featureOptions;
        _configuration = configuration;
        _environment = environment;
    }
    
    /// <summary>
    /// GET: /account/v2/login
    /// </summary>
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Version"] = "V2 (Enhanced Security)";
        return View("~/Views/Account/LoginV2.cshtml");
    }
    
    /// <summary>
    /// POST: /account/v2/login
    /// Enhanced: Uses structured logging (no file I/O)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/LoginV2.cshtml", model);
        }
        
        // Enhanced logging: structured, no file I/O
        _logger.LogInformation("Login attempt for {Email} from {IpAddress}", 
            model.Email, 
            HttpContext.Connection.RemoteIpAddress);
        
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);
        
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Enhanced: Deterministic tenant resolution
                var tenantId = await _tenantResolver.ResolveCurrentTenantAsync(user.Id);
                
                if (tenantId.HasValue)
                {
                    // Record tenant access
                    await _tenantResolver.RecordTenantAccessAsync(user.Id, tenantId.Value);
                }
                
                _logger.LogInformation(
                    "User {UserId} logged in successfully, tenant resolved: {TenantId}",
                    user.Id, tenantId);
                
                return RedirectToLocal(returnUrl);
            }
        }
        
        if (result.IsLockedOut)
        {
            _logger.LogWarning("Account {Email} is locked out", model.Email);
            ModelState.AddModelError(string.Empty, "Account is locked out. Please try again later.");
        }
        else
        {
            _logger.LogWarning("Invalid login attempt for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        
        return View("~/Views/Account/LoginV2.cshtml", model);
    }
    
    /// <summary>
    /// GET: /account/v2/tenant-login
    /// Enhanced: Session-based claims (not DB-persisted)
    /// </summary>
    [HttpGet("tenant-login")]
    [AllowAnonymous]
    public IActionResult TenantLogin(string? tenantSlug = null, string? returnUrl = null)
    {
        ViewData["TenantSlug"] = tenantSlug;
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Version"] = "V2 (Session-Based Claims)";
        return View("~/Views/Account/TenantLoginV2.cshtml");
    }
    
    /// <summary>
    /// POST: /account/v2/tenant-login
    /// Enhanced: Uses session-based claims (fixes bug where claims were persisted to DB)
    /// </summary>
    [HttpPost("tenant-login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TenantLogin(TenantLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/TenantLoginV2.cshtml", model);
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("~/Views/Account/TenantLoginV2.cshtml", model);
        }
        
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password for tenant login: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("~/Views/Account/TenantLoginV2.cshtml", model);
        }
        
        // Enhanced: Use session-based claims (NOT persisted to AspNetUserClaims table)
        await _enhancedAuthService.SignInWithTenantContextAsync(
            user,
            model.TenantId,
            model.Role ?? "TenantUser",
            isPersistent: model.RememberMe);
        
        _logger.LogInformation(
            "User {UserId} logged into tenant {TenantId} with session-based claims",
            user.Id, model.TenantId);
        
        return RedirectToLocal(model.ReturnUrl ?? $"/tenant/{model.TenantSlug}");
    }
    
    /// <summary>
    /// GET: /account/v2/demo-login
    /// Enhanced: NO hard-coded credentials (uses user secrets in dev, disabled in prod)
    /// </summary>
    [HttpGet("demo-login")]
    [AllowAnonymous]
    public async Task<IActionResult> DemoLogin()
    {
        // Security fix: Check feature flag
        if (_featureOptions.Value.DisableDemoLogin)
        {
            _logger.LogWarning("Demo login attempted but disabled by feature flag");
            return NotFound();
        }
        
        // Security fix: Only allow in development
        if (_environment.IsProduction())
        {
            _logger.LogWarning("Demo login attempted in production environment");
            return NotFound();
        }
        
        // Security fix: Get credentials from user secrets (NOT hard-coded)
        var demoEmail = _configuration["Demo:Email"];
        var demoPassword = _configuration["Demo:Password"];
        
        if (string.IsNullOrEmpty(demoEmail) || string.IsNullOrEmpty(demoPassword))
        {
            _logger.LogError("Demo credentials not configured in user secrets");
            TempData["Error"] = "Demo login not configured. Please set Demo:Email and Demo:Password in user secrets.";
            return RedirectToAction(nameof(Login));
        }
        
        var user = await _userManager.FindByEmailAsync(demoEmail);
        if (user == null)
        {
            _logger.LogError("Demo user {Email} not found", demoEmail);
            TempData["Error"] = "Demo user not found.";
            return RedirectToAction(nameof(Login));
        }
        
        var result = await _signInManager.PasswordSignInAsync(
            demoEmail,
            demoPassword,
            isPersistent: false,
            lockoutOnFailure: false);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Demo login successful for {Email} (DEVELOPMENT ONLY)", demoEmail);
            return RedirectToAction("Index", "Home");
        }
        
        _logger.LogWarning("Demo login failed for {Email}", demoEmail);
        TempData["Error"] = "Demo login failed.";
        return RedirectToAction(nameof(Login));
    }
    
    /// <summary>
    /// POST: /account/v2/logout
    /// </summary>
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        await _signInManager.SignOutAsync();
        
        _logger.LogInformation("User {UserId} logged out", userId);
        
        return RedirectToAction("Index", "Home");
    }
    
    /// <summary>
    /// GET: /account/v2/switch-tenant/{tenantId}
    /// Enhanced: Session-based tenant switching
    /// </summary>
    [HttpGet("switch-tenant/{tenantId}")]
    [Authorize]
    public async Task<IActionResult> SwitchTenant(Guid tenantId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        // Verify user has access to this tenant
        var userTenants = await _tenantResolver.GetUserTenantsAsync(userId);
        var targetTenant = userTenants.FirstOrDefault(t => t.TenantId == tenantId);
        
        if (targetTenant == null)
        {
            _logger.LogWarning(
                "User {UserId} attempted to switch to unauthorized tenant {TenantId}",
                userId, tenantId);
            return Forbid();
        }
        
        // Switch tenant context (session-only)
        await _enhancedAuthService.SwitchTenantContextAsync(tenantId, targetTenant.Role);
        
        _logger.LogInformation(
            "User {UserId} switched to tenant {TenantId} (session-based)",
            userId, tenantId);
        
        return Redirect($"/tenant/{targetTenant.TenantSlug}/dashboard");
    }
    
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction("Index", "Home");
    }
}

/// <summary>
/// Tenant login view model
/// </summary>
public class TenantLoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public string? Role { get; set; }
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
