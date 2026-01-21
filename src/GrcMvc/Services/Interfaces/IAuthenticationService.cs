using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// TOTP Setup Result
    /// </summary>
    public class TotpSetupResult
    {
        public bool Success { get; set; }
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthenticationService
    {
        Task<AuthTokenDto?> LoginAsync(string email, string password);
        Task<AuthTokenDto?> RegisterAsync(string email, string password, string fullName);
        Task<bool> ValidateTokenAsync(string token);
        Task<AuthUserDto?> GetUserFromTokenAsync(string token);
        Task<bool> LogoutAsync(string token);
        Task<AuthTokenDto?> RefreshTokenAsync(string refreshToken);
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto updateProfileDto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordDto);
        Task<PasswordResetResponseDto> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordDto);
        
        // Two-Factor Authentication methods
        Task<AuthTokenDto?> VerifyMfaAsync(string userId, string mfaCode, string mfaMethod);
        Task<bool> SendMfaCodeAsync(string userId, string mfaMethod);
        Task<TotpSetupResult> SetupTotpAsync(string userId);
        Task<bool> EnableMfaAsync(string userId, string mfaMethod, string? secretKey = null, string? phoneNumber = null);
        Task<bool> DisableMfaAsync(string userId);
        
        // External Authentication (OAuth2/OIDC) methods
        Task<AuthTokenDto?> ExternalLoginAsync(string provider, string providerKey, string email, string? name = null);
        Task<bool> LinkExternalLoginAsync(string userId, string provider, string providerKey);
        Task<bool> UnlinkExternalLoginAsync(string userId, string provider);
        Task<List<string>> GetLinkedProvidersAsync(string userId);
    }
}
