using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Models.Entities;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    [Route("api/admin")]
    [ApiController]
    public class AdminPasswordResetController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminPasswordResetController> _logger;

        public AdminPasswordResetController(
            UserManager<ApplicationUser> userManager,
            ILogger<AdminPasswordResetController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Direct password reset - bypasses email verification
        /// For development/admin use only
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous] // For emergency access - secure this in production!
        public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordRequest request)
        {
            _logger.LogInformation("Password reset requested for: {Email}", request?.Email);
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { errors = errors });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Remove old password
            await _userManager.RemovePasswordAsync(user);

            // Set new password
            var result = await _userManager.AddPasswordAsync(user, request.NewPassword);

            if (result.Succeeded)
            {
                // Reset lockout
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogInformation("Password reset for user {Email}", request.Email);

                return Ok(new {
                    success = true,
                    message = "Password reset successfully",
                    email = request.Email
                });
            }

            return BadRequest(new {
                error = "Password reset failed",
                details = result.Errors.Select(e => e.Description)
            });
        }

        /// <summary>
        /// List all users (for admin to see who needs password reset)
        /// </summary>
        [HttpGet("list-users")]
        [AllowAnonymous] // For emergency access - secure this in production!
        public async Task<IActionResult> ListUsers()
        {
            var users = _userManager.Users
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.IsActive,
                    u.LockoutEnd,
                    u.AccessFailedCount
                })
                .OrderBy(u => u.Email)
                .ToList();

            return Ok(users);
        }
    }

    public class AdminResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
