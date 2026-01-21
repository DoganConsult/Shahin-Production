using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Auth;

/// <summary>
/// Multi-Factor Authentication Controller
/// Handles 2FA verification, setup, and management
/// </summary>
[AllowAnonymous]
public class MfaController : Controller
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<MfaController> _logger;

    public MfaController(
        IAuthenticationService authenticationService,
        ILogger<MfaController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Display MFA verification page
    /// GET /auth/mfa/verify
    /// </summary>
    [HttpGet]
    public IActionResult Verify(string? userId = null, string? mfaMethod = null, string? returnUrl = null)
    {
        ViewBag.UserId = userId;
        ViewBag.MfaMethod = mfaMethod ?? "Email";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    /// <summary>
    /// Verify MFA code
    /// POST /auth/mfa/verify
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(string userId, string mfaCode, string mfaMethod, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(mfaCode) || string.IsNullOrEmpty(mfaMethod))
        {
            ModelState.AddModelError("", "User ID, MFA code, and method are required");
            ViewBag.UserId = userId;
            ViewBag.MfaMethod = mfaMethod;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        try
        {
            var result = await _authenticationService.VerifyMfaAsync(userId, mfaCode, mfaMethod);
            
            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                // MFA verified successfully - sign in user
                // Store token in cookie or session
                Response.Cookies.Append("auth_token", result.AccessToken, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                _logger.LogInformation("MFA verified successfully for user {UserId}", userId);
                return Redirect(returnUrl ?? "/");
            }
            else
            {
                ModelState.AddModelError("", "Invalid MFA code. Please try again.");
                ViewBag.UserId = userId;
                ViewBag.MfaMethod = mfaMethod;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA code for user {UserId}", userId);
            ModelState.AddModelError("", "An error occurred while verifying your code. Please try again.");
            ViewBag.UserId = userId;
            ViewBag.MfaMethod = mfaMethod;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }

    /// <summary>
    /// Resend MFA code
    /// POST /auth/mfa/resend
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend(string userId, string mfaMethod)
    {
        try
        {
            var result = await _authenticationService.SendMfaCodeAsync(userId, mfaMethod);
            
            if (result)
            {
                TempData["Success"] = $"MFA code sent via {mfaMethod}";
            }
            else
            {
                TempData["Error"] = "Failed to send MFA code. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending MFA code for user {UserId}", userId);
            TempData["Error"] = "An error occurred while sending the code. Please try again.";
        }

        return RedirectToAction(nameof(Verify), new { userId, mfaMethod });
    }

    /// <summary>
    /// Setup TOTP (authenticator app)
    /// GET /auth/mfa/setup-totp
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SetupTotp()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authenticationService.SetupTotpAsync(userId);
            
            if (result.Success)
            {
                ViewBag.SecretKey = result.SecretKey;
                ViewBag.QrCodeUri = result.QrCodeUri;
                return View();
            }
            else
            {
                TempData["Error"] = "Failed to setup TOTP. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up TOTP");
            TempData["Error"] = "An error occurred while setting up TOTP. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Enable MFA for user
    /// POST /auth/mfa/enable
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(string mfaMethod, string? secretKey = null, string? phoneNumber = null)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authenticationService.EnableMfaAsync(userId, mfaMethod, secretKey, phoneNumber);
            
            if (result)
            {
                TempData["Success"] = $"MFA enabled successfully using {mfaMethod}";
            }
            else
            {
                TempData["Error"] = "Failed to enable MFA. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling MFA");
            TempData["Error"] = "An error occurred while enabling MFA. Please try again.";
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Disable MFA for user
    /// POST /auth/mfa/disable
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authenticationService.DisableMfaAsync(userId);
            
            if (result)
            {
                TempData["Success"] = "MFA disabled successfully";
            }
            else
            {
                TempData["Error"] = "Failed to disable MFA. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA");
            TempData["Error"] = "An error occurred while disabling MFA. Please try again.";
        }

        return RedirectToAction("Index", "Home");
    }
}
