using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Configuration;
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
    /// CRITICAL FIX: Identity-based AuthenticationService
    /// Replaces mock implementation with proper ASP.NET Core Identity integration
    /// Uses UserManager, SignInManager, and proper JWT token generation
    /// </summary>
    public class IdentityAuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly GrcDbContext _context;
        private readonly ILogger<IdentityAuthenticationService> _logger;
        private readonly JwtSettings _jwtSettings;
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
            _jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() 
                ?? throw new InvalidOperationException("JWT settings are not configured");
        }

        public async Task<AuthTokenDto?> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                return null;
            }

            // CRITICAL FIX: Use proper password validation with UserManager
            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);
            
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

            // Get tenant user for tenant context
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            // CRITICAL FIX: Generate proper JWT token with signing
            var token = GenerateJwtToken(user, roles.ToList(), claims.ToList(), tenantUser?.TenantId);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = (int)(token.ValidTo - DateTime.UtcNow).TotalSeconds,
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
            var token = GenerateJwtToken(user, roles.ToList(), claims.ToList(), null);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = (int)(token.ValidTo - DateTime.UtcNow).TotalSeconds,
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

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

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
            if (!await ValidateTokenAsync(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return null;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
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
                _logger.LogError(ex, "Failed to get user from token");
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
            // Use UserManager's underlying store instead of non-existent Users DbSet
            var allUsers = await _userManager.Users.ToListAsync();
            var user = allUsers.FirstOrDefault(u => u.RefreshToken == refreshToken && 
                                         u.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            var newToken = GenerateJwtToken(user, roles.ToList(), claims.ToList(), tenantUser?.TenantId);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newToken),
                RefreshToken = newRefreshToken,
                TokenType = "Bearer",
                ExpiresIn = (int)(newToken.ValidTo - DateTime.UtcNow).TotalSeconds,
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

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
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

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // CRITICAL FIX: Store old password hash BEFORE changing password
            string? oldPasswordHash = user.PasswordHash;

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (result.Succeeded)
            {
                // CRITICAL FIX: Store password history and log audit event
                try
                {
                    // Store old password hash in history (captured before change)
                    if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                    {
                        var passwordHistory = new PasswordHistory
                        {
                            UserId = user.Id,
                            PasswordHash = oldPasswordHash, // Store old hash (captured before change)
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = userId,
                            Reason = "User initiated",
                            IpAddress = null, // Not available in service layer
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
            // In real implementation, send email with token
            // For now, return token (should be sent via email in production)

            return new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password reset link has been sent to your email",
                ResetToken = token, // Should not be returned in production - send via email
                ExpiryTime = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return false;

            // CRITICAL FIX: Store old password hash BEFORE resetting password
            string? oldPasswordHash = user.PasswordHash;

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (result.Succeeded)
            {
                // CRITICAL FIX: Store password history and log audit event
                try
                {
                    // Store old password hash in history (captured before change)
                    if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                    {
                        var passwordHistory = new PasswordHistory
                        {
                            UserId = user.Id,
                            PasswordHash = oldPasswordHash, // Store old hash (captured before change)
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = user.Id,
                            Reason = "Password reset via email",
                            IpAddress = null, // Not available in service layer
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

        private JwtSecurityToken GenerateJwtToken(ApplicationUser user, List<string> roles, List<Claim> claims, Guid? tenantId)
        {
            var jwtClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            // Add tenant ID if available
            if (tenantId.HasValue)
            {
                jwtClaims.Add(new Claim(ClaimConstants.TenantId, tenantId.Value.ToString()));
            }

            // Add roles
            foreach (var role in roles)
            {
                jwtClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add custom claims
            jwtClaims.AddRange(claims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: jwtClaims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: creds
            );
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
