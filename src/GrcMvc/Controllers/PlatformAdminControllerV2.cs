using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrcMvc.Controllers;

/// <summary>
/// V2 Platform Admin Controller - Uses facade pattern for gradual migration
/// Routes: /platform-admin/v2/* (parallel to existing /platform-admin/*)
/// </summary>
[Route("platform-admin/v2")]
[Authorize(Roles = "PlatformAdmin")]
public class PlatformAdminControllerV2 : Controller
{
    private readonly IUserManagementFacade _userFacade;
    private readonly ILogger<PlatformAdminControllerV2> _logger;
    
    public PlatformAdminControllerV2(
        IUserManagementFacade userFacade,
        ILogger<PlatformAdminControllerV2> logger)
    {
        _userFacade = userFacade;
        _logger = logger;
    }
    
    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    
    /// <summary>
    /// V2 Dashboard (testing endpoint)
    /// </summary>
    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        ViewData["Version"] = "V2 (Facade)";
        ViewData["Message"] = "This is the V2 controller using the facade pattern";
        return View("~/Views/PlatformAdmin/DashboardV2.cshtml");
    }
    
    /// <summary>
    /// Get user details (via facade)
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _userFacade.GetUserAsync(id);
            return Json(new { success = true, data = user, source = "V2-Facade" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return Json(new { success = false, error = "An error occurred processing your request." });
        }
    }
    
    /// <summary>
    /// Reset user password (via facade)
    /// </summary>
    [HttpPost("users/{id}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var newPassword = GenerateSecurePassword();
            
            var success = await _userFacade.ResetPasswordAsync(adminUserId, id, newPassword);
            
            if (success)
            {
                // Show password in secure modal (one-time display)
                ViewData["GeneratedPassword"] = newPassword;
                ViewData["ShowPasswordModal"] = true;
                TempData["Success"] = "Password reset successfully (V2)";
            }
            else
            {
                TempData["Error"] = "Failed to reset password";
            }
            
            return RedirectToAction("GetUser", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", id);
            TempData["Error"] = "An error occurred. Please try again.";
            return RedirectToAction("GetUser", new { id });
        }
    }
    
    /// <summary>
    /// List all users (via facade)
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> Users(int page = 1, int pageSize = 50)
    {
        try
        {
            var users = await _userFacade.GetUsersAsync(page, pageSize);
            ViewData["Version"] = "V2 (Facade)";
            return View("~/Views/PlatformAdmin/UsersV2.cshtml", users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            TempData["Error"] = "An error occurred. Please try again.";
            return View("~/Views/PlatformAdmin/UsersV2.cshtml", new List<UserDto>());
        }
    }
    
    private static string GenerateSecurePassword()
    {
        const int length = 18;
        const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length);
        var result = new char[length];
        
        for (int i = 0; i < length; i++)
            result[i] = validChars[bytes[i] % validChars.Length];
        
        return new string(result);
    }
}
