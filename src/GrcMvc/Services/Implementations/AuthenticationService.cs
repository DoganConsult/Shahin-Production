using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for handling authentication (login, register, token management)
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Dictionary<string, AuthUserDto> _mockUsers = new();
        private readonly Dictionary<string, string> _tokenStore = new();

        public AuthenticationService()
        {
            // Initialize with mock users
            InitializeMockUsers();
        }

        private void InitializeMockUsers()
        {
            _mockUsers["admin@grc.com"] = new AuthUserDto
            {
                Id = "1",
                Email = "admin@grc.com",
                FullName = "Admin User",
                Department = "Administration",
                Roles = new List<string> { "Admin", "Auditor", "Approver" },
                Permissions = new List<string> { "read", "write", "approve", "audit" }
            };

            _mockUsers["auditor@grc.com"] = new AuthUserDto
            {
                Id = "2",
                Email = "auditor@grc.com",
                FullName = "Auditor User",
                Department = "Audit",
                Roles = new List<string> { "Auditor" },
                Permissions = new List<string> { "read", "audit" }
            };

            _mockUsers["approver@grc.com"] = new AuthUserDto
            {
                Id = "3",
                Email = "approver@grc.com",
                FullName = "Approver User",
                Department = "Governance",
                Roles = new List<string> { "Approver" },
                Permissions = new List<string> { "read", "approve" }
            };
        }

        public async Task<AuthTokenDto?> LoginAsync(string email, string password)
        {
            await Task.Delay(100); // Simulate async operation
            
            if (!_mockUsers.ContainsKey(email))
                return null;

            var user = _mockUsers[email];
            var token = GenerateToken(email);
            _tokenStore[token] = email;

            return new AuthTokenDto
            {
                AccessToken = token,
                RefreshToken = GenerateRefreshToken(),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                User = user
            };
        }

        public async Task<AuthTokenDto?> RegisterAsync(string email, string password, string fullName)
        {
            await Task.Delay(100);

            if (_mockUsers.ContainsKey(email))
                return null; // User already exists

            var newUser = new AuthUserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                FullName = fullName,
                Department = "General",
                Roles = new List<string> { "User" },
                Permissions = new List<string> { "read" }
            };

            _mockUsers[email] = newUser;

            var token = GenerateToken(email);
            _tokenStore[token] = email;

            return new AuthTokenDto
            {
                AccessToken = token,
                RefreshToken = GenerateRefreshToken(),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                User = newUser
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            await Task.Delay(10);
            return _tokenStore.ContainsKey(token);
        }

        public async Task<AuthUserDto?> GetUserFromTokenAsync(string token)
        {
            await Task.Delay(10);
            
            if (!_tokenStore.ContainsKey(token))
                return null;

            var email = _tokenStore[token];
            return _mockUsers.ContainsKey(email) ? _mockUsers[email] : null;
        }

        public async Task<bool> LogoutAsync(string token)
        {
            await Task.Delay(10);
            return _tokenStore.Remove(token);
        }

        public async Task<AuthTokenDto?> RefreshTokenAsync(string refreshToken)
        {
            await Task.Delay(100);
            
            // In real implementation, validate refresh token
            // For mock, we'll create a new token
            var newToken = GenerateToken(Guid.NewGuid().ToString());
            _tokenStore[newToken] = "refreshed-user@grc.com";

            return new AuthTokenDto
            {
                AccessToken = newToken,
                RefreshToken = GenerateRefreshToken(),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                User = new AuthUserDto
                {
                    Id = "refreshed",
                    Email = "refreshed-user@grc.com",
                    FullName = "Refreshed User",
                    Department = "General",
                    Roles = new List<string> { "User" },
                    Permissions = new List<string> { "read" }
                }
            };
        }

        private string GenerateToken(string email)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"{email}:{Guid.NewGuid()}:{DateTime.UtcNow.Ticks}"));
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"refresh:{Guid.NewGuid()}:{DateTime.UtcNow.Ticks}"));
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            await Task.Delay(50);

            // Find user by ID or email
            var user = _mockUsers.Values.FirstOrDefault(u => u.Id == userId || u.Email == userId);
            if (user == null)
                return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Department = user.Department,
                PhoneNumber = "+1-555-0100",
                JobTitle = "GRC Specialist",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                LastLoginDate = DateTime.UtcNow.AddDays(-1),
                Roles = user.Roles
            };
        }

        public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto updateProfileDto)
        {
            await Task.Delay(100);

            var user = _mockUsers.Values.FirstOrDefault(u => u.Id == userId || u.Email == userId);
            if (user == null)
                return null;

            user.FullName = updateProfileDto.FullName ?? user.FullName;
            user.Department = updateProfileDto.Department ?? user.Department;

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Department = user.Department,
                PhoneNumber = updateProfileDto.PhoneNumber ?? "+1-555-0100",
                JobTitle = updateProfileDto.JobTitle ?? "GRC Specialist",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                LastLoginDate = DateTime.UtcNow,
                Roles = user.Roles
            };
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordDto)
        {
            await Task.Delay(100);

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                return false;

            var user = _mockUsers.Values.FirstOrDefault(u => u.Id == userId || u.Email == userId);
            return user != null;
        }

        public async Task<PasswordResetResponseDto> ForgotPasswordAsync(string email)
        {
            await Task.Delay(100);

            if (!_mockUsers.ContainsKey(email))
                return new PasswordResetResponseDto 
                { 
                    Success = false, 
                    Message = "User not found",
                    ResetToken = string.Empty
                };

            var resetToken = GenerateResetToken();
            return new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password reset link has been sent to your email",
                ResetToken = resetToken,
                ExpiryTime = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordDto)
        {
            await Task.Delay(100);

            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                return false;

            if (!_mockUsers.ContainsKey(resetPasswordDto.Email))
                return false;

            // In real implementation, validate the reset token
            // For mock, just accept it
            return true;
        }

        private string GenerateResetToken()
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"reset:{Guid.NewGuid()}:{DateTime.UtcNow.Ticks}"));
        }
    }
}
