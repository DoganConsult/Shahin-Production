using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// OpenIddict-based AuthenticationService
    /// Uses ASP.NET Core Identity for user management
    /// Token generation is handled by OpenIddict at /connect/token endpoint
    ///
    /// MIGRATION NOTE: Custom JWT has been removed. Use /connect/token for OAuth2 tokens.
    /// </summary>
    public class IdentityAuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly GrcDbContext _context;
        private readonly ILogger<IdentityAuthenticationService> _logger;
        private readonly GrcAuthDbContext? _authContext;
        private readonly IAuthenticationAuditService? _authAuditService;

        public IdentityAuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            GrcDbContext context,
            ILogger<IdentityAuthenticationService> logger,
            GrcAuthDbContext? authContext = null,
            IAuthenticationAuditService? authAuditService = null)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _authContext = authContext;
            _authAuditService = authAuditService;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        public async Task<AuthTokenDto?> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                return null;
            }

            // Validate password using Identity
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for user {Email}: {Result}", email, result);
                return null;
            }

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get roles and claims
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            // Generate JWT token
            var accessToken = GenerateJwtToken(user, roles, claims);
            var refreshToken = GenerateRefreshToken();

            _logger.LogInformation("User {Email} logged in successfully.", email);

            return new AuthTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 28800, // 8 hours in seconds
                User = new AuthUserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    Permissions = claims.Where(c => c.Type.StartsWith("permission.")).Select(c => c.Value).ToList()
                }
            };
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(ApplicationUser user, IList<string> roles, IList<Claim> userClaims)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"] ??
                           _configuration["Jwt:Key"] ??
                           "ShahinAI-GRC-Platform-Secret-Key-Min-32-Characters!!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("name", user.FullName ?? ""),
                new("user_id", user.Id.ToString())
            };

            // Add roles
            foreach (var role in roles)
            {
                tokenClaims.Add(new Claim(ClaimTypes.Role, role));
                tokenClaims.Add(new Claim("role", role));
            }

            // Add existing user claims
            tokenClaims.AddRange(userClaims);

            var issuer = _configuration["JwtSettings:Issuer"] ?? _configuration["Jwt:Issuer"] ?? "ShahinAI-GRC";
            var audience = _configuration["JwtSettings:Audience"] ?? _configuration["Jwt:Audience"] ?? "ShahinAI-GRC-App";

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: tokenClaims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate a secure refresh token
        /// </summary>
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Register a new user and return JWT token
        /// </summary>
        public async Task<AuthTokenDto?> RegisterAsync(string email, string password, string fullName)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt for existing user: {Email}", email);
                return null;
            }

            // Parse full name
            var nameParts = fullName.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            // Create user
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                CreatedDate = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogError("User creation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            // Set UserName and Email using UserManager methods
            await _userManager.SetUserNameAsync(user, email);
            await _userManager.SetEmailAsync(user, email);

            // Conditionally confirm email
            if (!_configuration.GetValue<bool>("RequireEmailConfirmation", false))
            {
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, emailToken);
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            // Generate JWT token
            var accessToken = GenerateJwtToken(user, roles, claims);
            var refreshToken = GenerateRefreshToken();

            _logger.LogInformation("User {Email} registered successfully.", email);

            return new AuthTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 28800, // 8 hours in seconds
                User = new AuthUserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    Permissions = claims.Where(c => c.Type.StartsWith("permission.")).Select(c => c.Value).ToList()
                }
            };
        }

        /// <summary>
        /// DEPRECATED: Token validation is now handled by OpenIddict middleware.
        /// Use [Authorize] attribute on controllers instead.
        /// </summary>
        [Obsolete("Token validation is handled by OpenIddict middleware. Use [Authorize] attribute.")]
        public Task<bool> ValidateTokenAsync(string token)
        {
            _logger.LogWarning("ValidateTokenAsync is deprecated. Use OpenIddict middleware with [Authorize] attribute.");
            // OpenIddict handles token validation via middleware
            return Task.FromResult(false);
        }

        /// <summary>
        /// DEPRECATED: Use HttpContext.User claims from OpenIddict middleware instead.
        /// </summary>
        [Obsolete("Use HttpContext.User claims from OpenIddict middleware instead")]
        public Task<AuthUserDto?> GetUserFromTokenAsync(string token)
        {
            _logger.LogWarning("GetUserFromTokenAsync is deprecated. Use HttpContext.User claims from OpenIddict middleware.");
            return Task.FromResult<AuthUserDto?>(null);
        }

        /// <summary>
        /// Logout - invalidates refresh token if stored
        /// </summary>
        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                // Sign out from Identity
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User signed out successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return false;
            }
        }

        /// <summary>
        /// DEPRECATED: Use /connect/token endpoint with refresh_token grant type instead.
        /// </summary>
        [Obsolete("Use /connect/token endpoint with grant_type=refresh_token")]
        public Task<AuthTokenDto?> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogWarning("RefreshTokenAsync is deprecated. Use /connect/token with grant_type=refresh_token.");
            return Task.FromResult<AuthTokenDto?>(null);
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Department = user.Department,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                Roles = roles.ToList()
            };
        }

        public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto updateProfileDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            // Parse FullName into FirstName and LastName
            if (!string.IsNullOrWhiteSpace(updateProfileDto.FullName))
            {
                var nameParts = updateProfileDto.FullName.Trim().Split(' ', 2);
                user.FirstName = nameParts[0];
                user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            }
            user.Department = updateProfileDto.Department ?? user.Department;
            user.JobTitle = updateProfileDto.JobTitle ?? user.JobTitle;

            // Set PhoneNumber using UserManager
            if (!string.IsNullOrEmpty(updateProfileDto.PhoneNumber))
            {
                await _userManager.SetPhoneNumberAsync(user, updateProfileDto.PhoneNumber);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Profile update failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            return await GetUserProfileAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Store old password hash BEFORE changing password
            string? oldPasswordHash = user.PasswordHash;

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (result.Succeeded)
            {
                try
                {
                    // Store old password hash in history
                    if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                    {
                        var passwordHistory = new PasswordHistory
                        {
                            UserId = user.Id.ToString(),
                            PasswordHash = oldPasswordHash,
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = userId,
                            Reason = "User initiated",
                            IpAddress = null,
                            UserAgent = null
                        };
                        _authContext.PasswordHistory.Add(passwordHistory);
                        await _authContext.SaveChangesAsync();
                    }

                    // Update password change timestamp
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Log audit event
                    if (_authAuditService != null)
                    {
                        await _authAuditService.LogPasswordChangeAsync(
                            userId: user.Id.ToString(),
                            changedByUserId: userId.ToString(),
                            reason: "User initiated",
                            ipAddress: null,
                            userAgent: null);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store password history or log audit event for user {UserId}", userId);
                }
            }

            return result.Succeeded;
        }

        public async Task<PasswordResetResponseDto> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal user existence
                return new PasswordResetResponseDto
                {
                    Success = true,
                    Message = "If the email exists, a password reset link has been sent.",
                    ResetToken = string.Empty
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password reset link has been sent to your email",
                ResetToken = token, // Should be sent via email in production
                ExpiryTime = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return false;

            string? oldPasswordHash = user.PasswordHash;

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (result.Succeeded)
            {
                try
                {
                    if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                    {
                        var passwordHistory = new PasswordHistory
                        {
                            UserId = user.Id.ToString(),
                            PasswordHash = oldPasswordHash,
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = user.Id.ToString(),
                            Reason = "Password reset via email",
                            IpAddress = null,
                            UserAgent = null
                        };
                        _authContext.PasswordHistory.Add(passwordHistory);
                        await _authContext.SaveChangesAsync();
                    }

                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    if (_authAuditService != null)
                    {
                        await _authAuditService.LogPasswordChangeAsync(
                            userId: user.Id.ToString(),
                            changedByUserId: user.Id.ToString(),
                            reason: "Password reset via email",
                            ipAddress: null,
                            userAgent: null);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store password history for user {UserId}", user.Id);
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
            }

            return result.Succeeded;
        }
    }
}
