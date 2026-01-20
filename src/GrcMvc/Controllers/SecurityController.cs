using System.Text;
using System.Text.Encodings.Web;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.ViewModels;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Security Controller - Manages 2FA, security settings, and security logs
/// Uses ABP built-in Identity features
/// </summary>
[Authorize]
[Route("account/security")]
public class SecurityController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GrcDbContext _context;
    private readonly IAuditEventService _auditService;
    private readonly ILogger<SecurityController> _logger;
    private readonly UrlEncoder _urlEncoder;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    public SecurityController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        GrcDbContext context,
        IAuditEventService auditService,
        ILogger<SecurityController> logger,
        UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _auditService = auditService;
        _logger = logger;
        _urlEncoder = urlEncoder;
    }

    /// <summary>
    /// Security Settings Dashboard
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new SecuritySettingsViewModel
        {
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
            LastPasswordChange = user.LastPasswordChangedAt,
            RecentSecurityLogs = await GetRecentSecurityLogsAsync(user.Id)
        };

        return View("~/Views/Account/SecuritySettings.cshtml", model);
    }

    /// <summary>
    /// Enable Authenticator - GET (show QR code)
    /// </summary>
    [HttpGet("enable-authenticator")]
    public async Task<IActionResult> EnableAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Reset authenticator key to get a new one
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(unformattedKey))
        {
            TempData["Error"] = "Could not generate authenticator key.";
            return RedirectToAction(nameof(Index));
        }

        var model = new EnableAuthenticatorViewModel
        {
            SharedKey = FormatKey(unformattedKey),
            QrCodeUrl = GenerateQrCodeUri(user.Email!, unformattedKey)
        };

        return View("~/Views/Account/EnableAuthenticator.cshtml", model);
    }

    /// <summary>
    /// Enable Authenticator - POST (verify and enable)
    /// </summary>
    [HttpPost("enable-authenticator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            model.QrCodeUrl = GenerateQrCodeUri(user.Email!, key!);
            return View("~/Views/Account/EnableAuthenticator.cshtml", model);
        }

        // Verify the code
        var verificationCode = model.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("VerificationCode", "Invalid verification code.");
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            model.QrCodeUrl = GenerateQrCodeUri(user.Email!, key!);
            return View("~/Views/Account/EnableAuthenticator.cshtml", model);
        }

        // Enable 2FA
        await _userManager.SetTwoFactorEnabledAsync(user, true);

        // Log the event
        await _auditService.LogEventAsync(
            Guid.Empty, // Platform level
            "2FA_ENABLED",
            "User",
            user.Id,
            "Enable2FA",
            user.Id,
            System.Text.Json.JsonSerializer.Serialize(new { Method = "Authenticator" }));

        _logger.LogInformation("User {UserId} enabled 2FA with authenticator app", user.Id);

        // Generate recovery codes
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        TempData["Success"] = "Two-factor authentication has been enabled!";
        return RedirectToAction(nameof(ShowRecoveryCodes), new { codes = string.Join(",", recoveryCodes!) });
    }

    /// <summary>
    /// Show Recovery Codes
    /// </summary>
    [HttpGet("recovery-codes")]
    public IActionResult ShowRecoveryCodes(string codes)
    {
        if (string.IsNullOrEmpty(codes))
            return RedirectToAction(nameof(Index));

        var model = new RecoveryCodesViewModel
        {
            RecoveryCodes = codes.Split(',').ToList(),
            ShowRecoveryCodes = true
        };

        return View("~/Views/Account/RecoveryCodes.cshtml", model);
    }

    /// <summary>
    /// Generate New Recovery Codes
    /// </summary>
    [HttpPost("generate-recovery-codes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            TempData["Error"] = "Cannot generate recovery codes - 2FA is not enabled.";
            return RedirectToAction(nameof(Index));
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await _auditService.LogEventAsync(
            Guid.Empty,
            "RECOVERY_CODES_GENERATED",
            "User",
            user.Id,
            "GenerateRecoveryCodes",
            user.Id,
            "{}");

        return RedirectToAction(nameof(ShowRecoveryCodes), new { codes = string.Join(",", recoveryCodes!) });
    }

    /// <summary>
    /// Disable 2FA - GET
    /// </summary>
    [HttpGet("disable-2fa")]
    public async Task<IActionResult> Disable2fa()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            TempData["Error"] = "2FA is not currently enabled.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Account/Disable2fa.cshtml", new Disable2faViewModel());
    }

    /// <summary>
    /// Disable 2FA - POST
    /// </summary>
    [HttpPost("disable-2fa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2fa(Disable2faViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Account/Disable2fa.cshtml", model);

        // Verify password
        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError("Password", "Incorrect password.");
            return View("~/Views/Account/Disable2fa.cshtml", model);
        }

        // Disable 2FA
        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
        {
            TempData["Error"] = "Failed to disable 2FA.";
            return RedirectToAction(nameof(Index));
        }

        // Reset authenticator key
        await _userManager.ResetAuthenticatorKeyAsync(user);

        await _auditService.LogEventAsync(
            Guid.Empty,
            "2FA_DISABLED",
            "User",
            user.Id,
            "Disable2FA",
            user.Id,
            "{}");

        _logger.LogWarning("User {UserId} disabled 2FA", user.Id);

        TempData["Success"] = "Two-factor authentication has been disabled.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Forget Browser (remove 2FA remember cookie)
    /// </summary>
    [HttpPost("forget-browser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgetBrowser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _signInManager.ForgetTwoFactorClientAsync();

        TempData["Success"] = "This browser has been forgotten. You will need to enter your 2FA code on next login.";
        return RedirectToAction(nameof(Index));
    }

    #region Helper Methods

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }
        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        // Use a QR code generation service URL
        var uri = string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode("Shahin-GRC"),
            _urlEncoder.Encode(email),
            unformattedKey);

        // Return Google Charts QR code URL
        return $"https://chart.googleapis.com/chart?cht=qr&chs=200x200&chl={_urlEncoder.Encode(uri)}";
    }

    private async Task<List<SecurityLogItem>> GetRecentSecurityLogsAsync(string userId)
    {
        return await _context.Set<AuditEvent>()
            .Where(a => a.Actor == userId && 
                (a.EventType.Contains("Login") || a.EventType.Contains("2FA") || 
                 a.EventType.Contains("Password") || a.EventType.Contains("Security")))
            .OrderByDescending(a => a.CreatedDate)
            .Take(10)
            .Select(a => new SecurityLogItem
            {
                Timestamp = a.CreatedDate,
                Action = a.EventType,
                IpAddress = a.IpAddress ?? "Unknown",
                Browser = "Web",
                IsSuccess = a.Status == "Success"
            })
            .ToListAsync();
    }

    #endregion
}
