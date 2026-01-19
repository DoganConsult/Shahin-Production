using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Account API Controller
    /// Handles REST API requests for user login, registration, profile management, and password operations
    /// Route: /api/auth
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly GrcDbContext _context;

        public AccountApiController(
            IAuthenticationService authenticationService,
            GrcDbContext context)
        {
            _authenticationService = authenticationService;
            _context = context;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <remarks>
        /// Test with: admin@grc.com / password
        /// Returns token along with onboarding status to help frontend determine next step.
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")] // SECURITY: Rate limiting to prevent brute force attacks
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginRequest?.Email) || string.IsNullOrWhiteSpace(loginRequest?.Password))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Email and password are required"));

                var result = await _authenticationService.LoginAsync(loginRequest.Email, loginRequest.Password);
                if (result == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid email or password"));

                // Check tenant and onboarding status
                var tenantUser = await _context.TenantUsers
                    .Include(tu => tu.Tenant)
                    .FirstOrDefaultAsync(tu => tu.UserId == result.User.Id && !tu.IsDeleted);

                var requiresOnboarding = tenantUser == null ||
                    tenantUser.Tenant == null ||
                    tenantUser.Tenant.OnboardingStatus != "COMPLETED";

                string? onboardingUrl = null;
                Guid? tenantId = tenantUser?.TenantId;

                if (requiresOnboarding && tenantId.HasValue)
                {
                    onboardingUrl = $"/OnboardingWizard?tenantId={tenantId}";
                }
                else if (requiresOnboarding && !tenantId.HasValue)
                {
                    // User has no tenant yet - redirect to create tenant/trial flow
                    onboardingUrl = "/OnboardingWizard";
                }

                // Return extended response with onboarding info
                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = new
                    {
                        token = result.AccessToken,
                        refreshToken = result.RefreshToken,
                        tokenType = result.TokenType,
                        expiresIn = result.ExpiresIn,
                        user = result.User,
                        tenantId = tenantId,
                        onboardingStatus = tenantUser?.Tenant?.OnboardingStatus ?? "NOT_STARTED",
                        requiresOnboarding = requiresOnboarding,
                        onboardingUrl = onboardingUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// User logout endpoint
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] dynamic request)
        {
            try
            {
                var token = request?.token?.ToString();
                if (string.IsNullOrEmpty(token))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Token is required"));

                await _authenticationService.LogoutAsync(token);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Logout successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")] // SECURITY: Rate limiting to prevent registration abuse
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registerRequest?.Email) ||
                    string.IsNullOrWhiteSpace(registerRequest?.Password) ||
                    string.IsNullOrWhiteSpace(registerRequest?.FullName))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Email, password, and full name are required"));

                var result = await _authenticationService.RegisterAsync(
                    registerRequest.Email,
                    registerRequest.Password,
                    registerRequest.FullName);

                if (result == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("User already exists"));

                return CreatedAtAction(nameof(GetUserProfile),
                    ApiResponse<AuthTokenDto>.SuccessResponse(result, "Registration successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshRequest?.RefreshToken))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token is required"));

                var result = await _authenticationService.RefreshTokenAsync(refreshRequest.RefreshToken);
                if (result == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid refresh token"));

                return Ok(ApiResponse<AuthTokenDto>.SuccessResponse(result, "Token refreshed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Forgot password endpoint - sends reset link via email
        /// SECURITY: Returns success even if user doesn't exist to prevent account enumeration
        /// </summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting("auth")] // SECURITY: Rate limiting to prevent enumeration attacks
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(forgotPasswordRequest?.Email))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Email is required"));

                // SECURITY: Always attempt the operation, don't reveal if user exists
                var result = await _authenticationService.ForgotPasswordAsync(forgotPasswordRequest.Email);

                // SECURITY: Return success message regardless of whether user was found
                // This prevents account enumeration attacks
                return Ok(ApiResponse<object>.SuccessResponse(null, "If an account with that email exists, a password reset link has been sent."));
            }
            catch (Exception ex)
            {
                // SECURITY: Still return generic success to prevent enumeration
                return Ok(ApiResponse<object>.SuccessResponse(null, "If an account with that email exists, a password reset link has been sent."));
            }
        }

        /// <summary>
        /// Reset password endpoint
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")] // SECURITY: Rate limiting to prevent abuse
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequest)
        {
            try
            {
                if (resetPasswordRequest == null ||
                    string.IsNullOrWhiteSpace(resetPasswordRequest.Email) ||
                    string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword) ||
                    string.IsNullOrWhiteSpace(resetPasswordRequest.Token))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Email, token, and new password are required"));

                if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmPassword)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Passwords do not match"));

                var success = await _authenticationService.ResetPasswordAsync(resetPasswordRequest);

                if (!success)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Password reset failed"));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        [HttpGet("user-profile")]
        public async Task<IActionResult> GetUserProfile([FromQuery] string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResponse("User ID is required"));

                var profile = await _authenticationService.GetUserProfileAsync(userId);
                if (profile == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "User profile retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] string userId, [FromBody] UpdateProfileRequestDto updateProfileRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResponse("User ID is required"));

                if (updateProfileRequest == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Profile data is required"));

                var profile = await _authenticationService.UpdateProfileAsync(userId, updateProfileRequest);
                if (profile == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromQuery] string userId, [FromBody] ChangePasswordRequestDto changePasswordRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResponse("User ID is required"));

                if (changePasswordRequest == null ||
                    string.IsNullOrWhiteSpace(changePasswordRequest.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(changePasswordRequest.NewPassword))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Current and new passwords are required"));

                if (changePasswordRequest.NewPassword != changePasswordRequest.ConfirmPassword)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Passwords do not match"));

                var success = await _authenticationService.ChangePasswordAsync(userId, changePasswordRequest);

                if (!success)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Password change failed"));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }
    }
}
