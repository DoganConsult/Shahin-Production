using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Pages.Account;

/// <summary>
/// Custom Login Page - Overrides ABP's default Login page to fix LazyServiceProvider null issue.
/// This page uses constructor injection instead of property injection.
/// </summary>
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IAuthenticationAuditService? _authAuditService;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger,
        IAuthenticationAuditService? authAuditService = null)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _authAuditService = authAuditService;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public string? ReturnUrlHash { get; set; }

    public bool EnableLocalLogin { get; set; } = true;

    public bool IsSelfRegistrationEnabled { get; set; } = true;

    public bool ShowCancelButton { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string UserNameOrEmailAddress { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null, string? returnUrlHash = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ReturnUrlHash = returnUrlHash;

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Get external authentication schemes
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        _logger.LogInformation("[GOLDEN_PATH] Login page accessed. ReturnUrl={ReturnUrl}", ReturnUrl);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null, string? returnUrlHash = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ReturnUrlHash = returnUrlHash;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        _logger.LogInformation(
            "[GOLDEN_PATH] Login attempt. Email={Email}, IP={IpAddress}",
            Input.UserNameOrEmailAddress, ipAddress);

        // Try to find user by email
        var user = await _userManager.FindByEmailAsync(Input.UserNameOrEmailAddress);
        if (user == null)
        {
            // Try by username
            user = await _userManager.FindByNameAsync(Input.UserNameOrEmailAddress);
        }

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            await LogFailedLoginAsync(Input.UserNameOrEmailAddress, ipAddress, userAgent, "User not found");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "[GOLDEN_PATH] User {Email} logged in successfully from IP {IpAddress}",
                Input.UserNameOrEmailAddress, ipAddress);

            await LogSuccessfulLoginAsync(user, ipAddress, userAgent);

            // Handle return URL with hash
            if (!string.IsNullOrEmpty(ReturnUrlHash))
            {
                return Redirect(ReturnUrl + ReturnUrlHash);
            }

            return LocalRedirect(ReturnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl, RememberMe = Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("[GOLDEN_PATH] User {Email} account locked out", Input.UserNameOrEmailAddress);
            await LogFailedLoginAsync(Input.UserNameOrEmailAddress, ipAddress, userAgent, "Account locked out");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        await LogFailedLoginAsync(Input.UserNameOrEmailAddress, ipAddress, userAgent, "Invalid password");
        return Page();
    }

    private async Task LogSuccessfulLoginAsync(ApplicationUser user, string ipAddress, string userAgent)
    {
        if (_authAuditService != null)
        {
            try
            {
                await _authAuditService.LogLoginAttemptAsync(
                    userId: user.Id,
                    email: user.Email ?? Input.UserNameOrEmailAddress,
                    success: true,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log successful login audit");
            }
        }
    }

    private async Task LogFailedLoginAsync(string email, string ipAddress, string userAgent, string reason)
    {
        if (_authAuditService != null)
        {
            try
            {
                await _authAuditService.LogLoginAttemptAsync(
                    userId: null,
                    email: email,
                    success: false,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    failureReason: reason);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log failed login audit");
            }
        }
    }
}
