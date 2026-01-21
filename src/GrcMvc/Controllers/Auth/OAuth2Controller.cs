using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using IAuthenticationService = GrcMvc.Services.Interfaces.IAuthenticationService;

namespace GrcMvc.Controllers.Auth;

/// <summary>
/// OAuth2 / OIDC External Authentication Controller
/// Handles Google, Microsoft, and generic OIDC authentication
/// </summary>
[AllowAnonymous]
public class OAuth2Controller : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GrcAuthDbContext _authContext;
    private readonly ILogger<OAuth2Controller> _logger;
    private readonly IAuthenticationService? _authenticationService;

    public OAuth2Controller(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        GrcAuthDbContext authContext,
        ILogger<OAuth2Controller> logger,
        IAuthenticationService? authenticationService = null)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authContext = authContext;
        _logger = logger;
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Initiate Google OAuth2 login
    /// GET /auth/google
    /// </summary>
    [HttpGet]
    public IActionResult Google(string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "OAuth2", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    /// <summary>
    /// Initiate Microsoft OAuth2 login
    /// GET /auth/microsoft
    /// </summary>
    [HttpGet]
    public IActionResult Microsoft(string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(MicrosoftCallback), "OAuth2", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);
        return Challenge(properties, "Microsoft");
    }

    /// <summary>
    /// Initiate GitHub OAuth2 login
    /// GET /auth/github
    /// </summary>
    [HttpGet]
    public IActionResult GitHub(string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(GitHubCallback), "OAuth2", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", redirectUrl);
        return Challenge(properties, "GitHub");
    }

    /// <summary>
    /// Initiate generic OIDC login
    /// GET /auth/oidc
    /// </summary>
    [HttpGet]
    public IActionResult Oidc(string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(OidcCallback), "OAuth2", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("GenericOIDC", redirectUrl);
        return Challenge(properties, "GenericOIDC");
    }

    /// <summary>
    /// Google OAuth2 callback
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/", string remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (remoteError != null)
        {
            _logger.LogError("Google OAuth2 error: {Error}", remoteError);
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Google OAuth2: Failed to retrieve external login information");
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        return await ProcessExternalLoginAsync(info, returnUrl, "Google");
    }

    /// <summary>
    /// Microsoft OAuth2 callback
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MicrosoftCallback(string returnUrl = "/", string remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (remoteError != null)
        {
            _logger.LogError("Microsoft OAuth2 error: {Error}", remoteError);
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Microsoft OAuth2: Failed to retrieve external login information");
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        return await ProcessExternalLoginAsync(info, returnUrl, "Microsoft");
    }

    /// <summary>
    /// GitHub OAuth2 callback
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GitHubCallback(string returnUrl = "/", string remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (remoteError != null)
        {
            _logger.LogError("GitHub OAuth2 error: {Error}", remoteError);
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("GitHub OAuth2: Failed to retrieve external login information");
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        return await ProcessExternalLoginAsync(info, returnUrl, "GitHub");
    }

    /// <summary>
    /// Generic OIDC callback
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> OidcCallback(string returnUrl = "/", string remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (remoteError != null)
        {
            _logger.LogError("OIDC error: {Error}", remoteError);
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("OIDC: Failed to retrieve external login information");
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_auth_failed" });
        }

        return await ProcessExternalLoginAsync(info, returnUrl, "OIDC");
    }

    private async Task<IActionResult> ProcessExternalLoginAsync(
        ExternalLoginInfo info,
        string returnUrl,
        string provider)
    {
        // Sign in the user with this external login provider if the user already has a login
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            _logger.LogInformation("External login successful for provider: {Provider}", provider);
            
            // Log external login via AuthenticationService if available
            if (_authenticationService != null)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    // Update last login date
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
            }
            
            return LocalRedirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            return RedirectToAction("Lockout", "Account");
        }

        // If the user does not have an account, create one
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("External login failed: Email claim not found");
            return RedirectToAction("Login", "Account", new { returnUrl, error = "email_not_provided" });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true, // External providers verify email
                FullName = name ?? email,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user from external login: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return RedirectToAction("Login", "Account", new { returnUrl, error = "user_creation_failed" });
            }
        }

        // Add external login to user
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            _logger.LogError("Failed to add external login: {Errors}",
                string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
            return RedirectToAction("Login", "Account", new { returnUrl, error = "external_login_failed" });
        }

        // Sign in the user
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("User created and signed in via external provider: {Provider}, Email: {Email}", provider, email);

        return LocalRedirect(returnUrl);
    }
}
