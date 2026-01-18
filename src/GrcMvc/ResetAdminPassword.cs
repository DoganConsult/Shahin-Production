using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using System;
using System.Threading.Tasks;

namespace GrcMvc
{
    /// <summary>
    /// Utility to reset admin password when locked out
    /// Run this with: dotnet run --reset-admin-password
    /// </summary>
    public static class ResetAdminPassword
    {
        public static async Task<bool> ResetPasswordAsync(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<GrcDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // Find the admin user
                var adminUser = await userManager.FindByEmailAsync("support@shahin-ai.com");

                // Get password from environment variable or generate secure one
                var newPassword = Environment.GetEnvironmentVariable("GRC_ADMIN_PASSWORD") ?? GenerateSecurePassword();

                if (adminUser == null)
                {
                    Console.WriteLine("Admin user not found. Creating new admin user...");

                    adminUser = new ApplicationUser
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Department = "Management",
                        JobTitle = "System Administrator"
                    };

                    var createResult = await userManager.CreateAsync(adminUser, newPassword);

                    if (createResult.Succeeded)
                    {
                        // Set UserName, Email, and confirm email using UserManager methods
                        await userManager.SetUserNameAsync(adminUser, "support@shahin-ai.com");
                        await userManager.SetEmailAsync(adminUser, "support@shahin-ai.com");
                        var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                        await userManager.ConfirmEmailAsync(adminUser, confirmToken);
                    }

                    if (!createResult.Succeeded)
                    {
                        Console.WriteLine($"Failed to create admin user: {string.Join(", ", createResult.Errors)}");
                        return false;
                    }

                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    Console.WriteLine($"Found admin user: {adminUser.Email}");

                    // Reset password to new value
                    var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetResult = await userManager.ResetPasswordAsync(adminUser, passwordResetToken, newPassword);

                    if (!resetResult.Succeeded)
                    {
                        Console.WriteLine($"Failed to reset password: {string.Join(", ", resetResult.Errors)}");
                        return false;
                    }

                    // Clear any lockout
                    await userManager.SetLockoutEndDateAsync(adminUser, null);
                    await userManager.ResetAccessFailedCountAsync(adminUser);

                    // Ensure email is confirmed
                    if (!adminUser.EmailConfirmed)
                    {
                        var emailConfirmToken = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                        await userManager.ConfirmEmailAsync(adminUser, emailConfirmToken);
                    }

                    Console.WriteLine("Admin password reset successfully!");
                }

                // CRITICAL SECURITY FIX: Never log passwords to console - use secure logger or send via secure channel
                Console.WriteLine("\n==============================");
                Console.WriteLine("Login Credentials:");
                Console.WriteLine($"Email: support@shahin-ai.com");
                Console.WriteLine("Password has been reset. Check secure credentials delivery channel.");
                Console.WriteLine("==============================\n");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        private static string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%&*";
            var password = new char[16];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            for (int i = 0; i < 16; i++)
                password[i] = chars[bytes[i] % chars.Length];
            return new string(password);
        }
    }
}