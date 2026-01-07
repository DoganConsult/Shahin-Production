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
                        UserName = "support@shahin-ai.com",
                        Email = "support@shahin-ai.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Department = "Management",
                        JobTitle = "System Administrator",
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(adminUser, newPassword);

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
                    var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetResult = await userManager.ResetPasswordAsync(adminUser, token, newPassword);

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
                        adminUser.EmailConfirmed = true;
                        await userManager.UpdateAsync(adminUser);
                    }

                    Console.WriteLine("Admin password reset successfully!");
                }

                Console.WriteLine("\n==============================");
                Console.WriteLine("Login Credentials:");
                Console.WriteLine($"Email: support@shahin-ai.com");
                Console.WriteLine($"Password: {newPassword}");
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