using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Account;
// NOTE: JWT imports removed - using OpenIddict for token validation

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// ABP-based AuthenticationService
    /// Uses ABP OpenIddict for OAuth2/OIDC token generation
    /// Uses ABP Identity + Account services for user management
    /// </summary>
    public class IdentityAuthenticationService : IAuthenticationService
    {
        private readonly IAccountAppService _accountAppService;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly GrcDbContext _context;
        private readonly ILogger<IdentityAuthenticationService> _logger;
        // NOTE: JwtSettings removed - using OpenIddict for authentication
        private readonly GrcAuthDbContext? _authContext;
        private readonly IAuthenticationAuditService? _authAuditService;

        public IdentityAuthenticationService(
            IAccountAppService accountAppService,
            IIdentityUserAppService identityUserAppService,
            IIdentityUserRepository identityUserRepository,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            GrcDbContext context,
            ILogger<IdentityAuthenticationService> logger,
            GrcAuthDbContext? authContext = null,
            IAuthenticationAuditService? authAuditService = null)
        {
            _accountAppService = accountAppService;
            _identityUserAppService = identityUserAppService;
            _identityUserRepository = identityUserRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _authContext = authContext;
            _authAuditService = authAuditService;
            // NOTE: JWT settings removed - OpenIddict handles token validation automatically
        }

        public async Task<AuthTokenDto?> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                return null;
            }

            // Validate password using SignInManager
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for user {Email}: {Result}", email, result);
                return null;
            }

            // Get token from ABP OpenIddict token endpoint
            var tokenResponse = await GetOpenIddictTokenAsync(email, password);
            if (tokenResponse == null)
            {
                _logger.LogError("OpenIddict token generation failed for user {Email}. Check OpenIddict configuration.", email);
                return null;
            }

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get roles for user info
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AuthTokenDto
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = tokenResponse.ExpiresIn,
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

        /// <summary>
        /// Get token from ABP OpenIddict /connect/token endpoint
        /// </summary>
        private async Task<OpenIddictTokenResponse?> GetOpenIddictTokenAsync(string username, string password)
        {
            try
            {
                var baseUrl = _configuration["App:SelfUrl"] ?? "https://localhost:5001";
                var clientId = _configuration["OpenIddict:ClientId"] ?? "GrcMvc_App";
                var clientSecret = _configuration["OpenIddict:ClientSecret"] ?? "";
                var scope = _configuration["OpenIddict:Scope"] ?? "openid profile email roles";

                using var client = _httpClientFactory.CreateClient();
                var tokenEndpoint = $"{baseUrl}/connect/token";

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = username,
                    ["password"] = password,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["scope"] = scope
                });

                var response = await client.PostAsync(tokenEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("OpenIddict token request failed: {StatusCode} - {Error}", response.StatusCode, error);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OpenIddictTokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenIddict token");
                return null;
            }
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

            // Get token from ABP OpenIddict
            var tokenResponse = await GetOpenIddictTokenAsync(email, password);
            if (tokenResponse == null)
            {
                _logger.LogError("OpenIddict token generation failed after registration for user {Email}", email);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AuthTokenDto
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = tokenResponse.ExpiresIn,
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

        /// <summary>
        /// NOTE: JWT token validation removed - use OpenIddict introspection endpoint instead
        /// For OpenIddict token validation, use: POST /connect/introspect
        /// Or rely on OpenIddict validation middleware which validates tokens automatically
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            // TODO: Implement OpenIddict token introspection if needed
            // For now, return false as JWT validation is removed
            _logger.LogWarning("ValidateTokenAsync called - JWT validation removed. Use OpenIddict /connect/introspect endpoint instead.");
            return await Task.FromResult(false);
        }

        /// <summary>
        /// NOTE: JWT token parsing removed - use OpenIddict userinfo endpoint instead
        /// For OpenIddict user info, use: GET /connect/userinfo
        /// Or rely on OpenIddict validation middleware which provides user claims automatically
        /// </summary>
        public async Task<AuthUserDto?> GetUserFromTokenAsync(string token)
        {
            // TODO: Implement OpenIddict userinfo retrieval if needed
            // For now, return null as JWT parsing is removed
            _logger.LogWarning("GetUserFromTokenAsync called - JWT parsing removed. Use OpenIddict /connect/userinfo endpoint instead.");
            return await Task.FromResult<AuthUserDto?>(null);
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
            // Use OpenIddict refresh token endpoint
            var tokenResponse = await GetOpenIddictRefreshTokenAsync(refreshToken);
            if (tokenResponse == null)
            {
                _logger.LogWarning("OpenIddict refresh token failed");
                return null;
            }

            // NOTE: JWT token parsing removed - OpenIddict tokens are validated by middleware
            // User info is available from HttpContext.User.Claims after authentication
            string? userId = null;

            if (string.IsNullOrEmpty(userId))
            {
                return new AuthTokenDto
                {
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                    TokenType = "Bearer",
                    ExpiresIn = tokenResponse.ExpiresIn
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthTokenDto
                {
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                    TokenType = "Bearer",
                    ExpiresIn = tokenResponse.ExpiresIn
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AuthTokenDto
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = tokenResponse.ExpiresIn,
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

        /// <summary>
        /// Get new token using OpenIddict refresh_token grant
        /// </summary>
        private async Task<OpenIddictTokenResponse?> GetOpenIddictRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var baseUrl = _configuration["App:SelfUrl"] ?? "https://localhost:5001";
                var clientId = _configuration["OpenIddict:ClientId"] ?? "GrcMvc_App";
                var clientSecret = _configuration["OpenIddict:ClientSecret"] ?? "";

                using var client = _httpClientFactory.CreateClient();
                var tokenEndpoint = $"{baseUrl}/connect/token";

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                });

                var response = await client.PostAsync(tokenEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("OpenIddict refresh token request failed: {StatusCode} - {Error}", response.StatusCode, error);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OpenIddictTokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing OpenIddict token");
                return null;
            }
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

        #region Two-Factor Authentication (Stub Implementation)

        public async Task<AuthTokenDto?> VerifyMfaAsync(string userId, string mfaCode, string mfaMethod)
        {
            // Stub implementation - delegate to main AuthenticationService if needed
            _logger.LogWarning("MFA verification not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult<AuthTokenDto?>(null);
        }

        public async Task<bool> SendMfaCodeAsync(string userId, string mfaMethod)
        {
            // Stub implementation
            _logger.LogWarning("SendMfaCodeAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(false);
        }

        public async Task<TotpSetupResult> SetupTotpAsync(string userId)
        {
            // Stub implementation
            _logger.LogWarning("SetupTotpAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(new TotpSetupResult { Success = false });
        }

        public async Task<bool> EnableMfaAsync(string userId, string mfaMethod, string? secretKey = null, string? phoneNumber = null)
        {
            // Stub implementation
            _logger.LogWarning("EnableMfaAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(false);
        }

        public async Task<bool> DisableMfaAsync(string userId)
        {
            // Stub implementation
            _logger.LogWarning("DisableMfaAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(false);
        }

        #endregion

        #region External Authentication (Stub Implementation)

        public async Task<AuthTokenDto?> ExternalLoginAsync(string provider, string providerKey, string email, string? name = null)
        {
            _logger.LogWarning("ExternalLoginAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult<AuthTokenDto?>(null);
        }

        public async Task<bool> LinkExternalLoginAsync(string userId, string provider, string providerKey)
        {
            _logger.LogWarning("LinkExternalLoginAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(false);
        }

        public async Task<bool> UnlinkExternalLoginAsync(string userId, string provider)
        {
            _logger.LogWarning("UnlinkExternalLoginAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(false);
        }

        public async Task<List<string>> GetLinkedProvidersAsync(string userId)
        {
            _logger.LogWarning("GetLinkedProvidersAsync not implemented in IdentityAuthenticationService - use AuthenticationService instead");
            return await Task.FromResult(new List<string>());
        }

        #endregion
    }

    /// <summary>
    /// OpenIddict token response DTO
    /// </summary>
    public class OpenIddictTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string? Scope { get; set; }
        public string? IdToken { get; set; }
    }
}
