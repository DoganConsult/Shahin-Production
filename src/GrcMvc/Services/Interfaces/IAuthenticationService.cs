using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
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
    }
}
