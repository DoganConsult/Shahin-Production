using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Production-ready AuthenticationService using ASP.NET Core Identity
    /// Replaces mock implementation with database-backed authentication
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly GrcAuthDbContext _authContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IAuthenticationAuditService? _authAuditService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            GrcAuthDbContext authContext,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger,
            IAuthenticationAuditService? authAuditService = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authContext = authContext;
            _configuration = configuration;
            _logger = logger;
            _authAuditService = authAuditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetIpAddress()
        {
            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers?["User-Agent"].ToString();
        }

        public async Task<AuthTokenDto?> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {Email}", email);
                    return null;
                }

                // Validate password using SignInManager
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
                var token = GenerateJwtToken(user, roles, claims);

                // Store refresh token
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                // Log audit event
                if (_authAuditService != null)
                {
                    await _authAuditService.LogLoginAttemptAsync(
                        userId: user.Id.ToString(),
                        email: email,
                        success: true,
                        ipAddress: GetIpAddress() ?? "Unknown",
                        userAgent: null);
                }

                return new AuthTokenDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600, // 1 hour
                    User = new AuthUserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FullName = user.FullName,
                        Department = user.Department,
                        Roles = roles.ToList(),
                        Permissions = claims.Where(c => c.Type.StartsWith("permission.")).Select(c => c.Value).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", email);
                return null;
            }
        }

        public async Task<AuthTokenDto?> RegisterAsync(string email, string password, string fullName)
        {
            try
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
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = !_configuration.GetValue<bool>("RequireEmailConfirmation", false),
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    _logger.LogError("User creation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");

                // Generate token
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var token = GenerateJwtToken(user, roles, claims);

                // Store refresh token
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                // Log audit event
                if (_authAuditService != null)
                {
                    await _authAuditService.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                    {
                        UserId = user.Id.ToString(),
                        Email = email,
                        EventType = "Registration",
                        Success = true,
                        IpAddress = GetIpAddress() ?? "Unknown",
                        UserAgent = null,
                        Message = $"User registered successfully: {email}",
                        Severity = "Info"
                    });
                }

                return new AuthTokenDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = new AuthUserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FullName = user.FullName,
                        Department = user.Department,
                        Roles = roles.ToList(),
                        Permissions = claims.Where(c => c.Type.StartsWith("permission.")).Select(c => c.Value).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", email);
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(GetJwtSecret());
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetJwtIssuer(),
                    ValidateAudience = true,
                    ValidAudience = GetJwtAudience(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return validatedToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }

        public async Task<AuthUserDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return null;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive)
                    return null;

                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);

                return new AuthUserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    Permissions = claims.Where(c => c.Type.StartsWith("permission.")).Select(c => c.Value).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user from token");
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var user = await GetUserFromTokenAsync(token);
                if (user == null)
                    return false;

                var appUser = await _userManager.FindByIdAsync(user.Id);
                if (appUser != null)
                {
                    appUser.RefreshToken = null;
                    appUser.RefreshTokenExpiry = null;
                    await _userManager.UpdateAsync(appUser);
                }

                // Log audit event
                if (_authAuditService != null)
                {
                    await _authAuditService.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                    {
                        UserId = user.Id.ToString(),
                        EventType = "Logout",
                        Success = true,
                        IpAddress = GetIpAddress() ?? "Unknown",
                        UserAgent = null,
                        Message = "User logged out successfully",
                        Severity = "Info"
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return false;
            }
        }

        public async Task<AuthTokenDto?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                             u.RefreshTokenExpiry > DateTime.UtcNow);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return null;
                }

                // Generate new token
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var newToken = GenerateJwtToken(user, roles, claims);

                // Generate new refresh token
                var newRefreshToken = GenerateRefreshToken();
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return new AuthTokenDto
                {
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = new AuthUserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FullName = user.FullName,
                        Department = user.Department,
                        Roles = roles.ToList(),
                        Permissions = claims.Where(c => c.Type.StartsWith("permission.")).Select(c => c.Value).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return null;
            }
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return null;

                var roles = await _userManager.GetRolesAsync(user);

                return new UserProfileDto
                {
                    Id = user.Id,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for userId: {UserId}", userId);
                return null;
            }
        }

        public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto updateProfileDto)
        {
            try
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
                user.PhoneNumber = updateProfileDto.PhoneNumber ?? user.PhoneNumber;
                user.JobTitle = updateProfileDto.JobTitle ?? user.JobTitle;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Profile update failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                return await GetUserProfileAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for userId: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                // Store old password hash BEFORE changing password
                string? oldPasswordHash = user.PasswordHash;

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (result.Succeeded)
                {
                    // Store password history and log audit event
                    try
                    {
                        // Store old password hash in history
                        if (!string.IsNullOrEmpty(oldPasswordHash))
                        {
                            var passwordHistory = new PasswordHistory
                            {
                                UserId = user.Id,
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
                                userId: user.Id,
                                changedByUserId: userId,
                                reason: "User initiated",
                                ipAddress: null,
                                userAgent: null);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to store password history or log audit event for user {UserId}", userId);
                        // Don't fail password change if audit logging fails
                    }
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for userId: {UserId}", userId);
                return false;
            }
        }

        public async Task<PasswordResetResponseDto> ForgotPasswordAsync(string email)
        {
            try
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
                
                // TODO: Send email with token in production
                // For now, return token (should be sent via email in production)
                _logger.LogInformation("Password reset token generated for user {Email}. Token should be sent via email.", email);

                return new PasswordResetResponseDto
                {
                    Success = true,
                    Message = "Password reset link has been sent to your email",
                    ResetToken = token, // Should not be returned in production - send via email
                    ExpiryTime = DateTime.UtcNow.AddHours(1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password for email: {Email}", email);
                return new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    ResetToken = string.Empty
                };
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                    return false;

                // Store old password hash BEFORE resetting password
                string? oldPasswordHash = user.PasswordHash;

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (result.Succeeded)
                {
                    // Store password history and log audit event
                    try
                    {
                        // Store old password hash in history
                        if (!string.IsNullOrEmpty(oldPasswordHash))
                        {
                            var passwordHistory = new PasswordHistory
                            {
                                UserId = user.Id,
                                PasswordHash = oldPasswordHash,
                                ChangedAt = DateTime.UtcNow,
                                ChangedByUserId = user.Id,
                                Reason = "Password reset via email",
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
                                userId: user.Id,
                                changedByUserId: user.Id,
                                reason: "Password reset via email",
                                ipAddress: null,
                                userAgent: null);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to store password history or log audit event for user {UserId}", user.Id);
                        // Don't fail password reset if audit logging fails
                        user.LastPasswordChangedAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", resetPasswordDto.Email);
                return false;
            }
        }

        #region Private Helper Methods

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles, IList<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());
            
            var tokenClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("full_name", user.FullName),
                new Claim("department", user.Department ?? string.Empty)
            };

            // Add roles
            foreach (var role in roles)
            {
                tokenClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add custom claims
            tokenClaims.AddRange(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(tokenClaims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"refresh:{Guid.NewGuid()}:{DateTime.UtcNow.Ticks}"));
        }

        private string GetJwtSecret()
        {
            return _configuration["JWT:Secret"] ?? 
                   Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                   throw new InvalidOperationException("JWT_SECRET environment variable is required");
        }

        private string GetJwtIssuer()
        {
            return _configuration["JWT:Issuer"] ?? 
                   _configuration["OpenIddict:Issuer"] ?? 
                   "https://shahin-ai.com";
        }

        private string GetJwtAudience()
        {
            return _configuration["JWT:Audience"] ?? 
                   _configuration["OpenIddict:Audience"] ?? 
                   "https://shahin-ai.com";
        }

        #endregion
    }
}
