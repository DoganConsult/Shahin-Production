using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service to check owner existence and handle one-time owner setup
    /// </summary>
    public class OwnerSetupService : IOwnerSetupService
    {
        private readonly GrcDbContext _context;
        private readonly GrcAuthDbContext _authContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<OwnerSetupService> _logger;

        public OwnerSetupService(
            GrcDbContext context,
            GrcAuthDbContext authContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<OwnerSetupService> logger)
        {
            _context = context;
            _authContext = authContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Check if any owner (PlatformAdmin or Owner role) exists
        /// </summary>
        public async Task<bool> OwnerExistsAsync()
        {
            try
            {
                // Check if PlatformAdmin or Owner role exists
                var superAdminRole = await _roleManager.FindByNameAsync("PlatformAdmin");
                var ownerRole = await _roleManager.FindByNameAsync("Owner");

                if (superAdminRole == null && ownerRole == null)
                {
                    _logger.LogDebug("No owner roles exist");
                    return false;
                }

                // Get all users with PlatformAdmin or Owner role
                var ownerUserIds = new List<string>();

                if (superAdminRole != null)
                {
                    var superAdminUsers = await _authContext.Set<IdentityUserRole<string>>()
                        .Where(ur => ur.RoleId == superAdminRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    ownerUserIds.AddRange(superAdminUsers);
                }

                if (ownerRole != null)
                {
                    var ownerUsers = await _authContext.Set<IdentityUserRole<string>>()
                        .Where(ur => ur.RoleId == ownerRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    ownerUserIds.AddRange(ownerUsers);
                }

                var ownerExists = ownerUserIds.Any();
                _logger.LogDebug("Owner existence check: {OwnerExists}, Count: {Count}", ownerExists, ownerUserIds.Count);

                return ownerExists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if owner exists");
                // On error, assume owner exists to prevent unauthorized setup
                return true;
            }
        }

        /// <summary>
        /// Create the first owner account (one-time setup)
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage, string? UserId)> CreateFirstOwnerAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string? organizationName = null)
        {
            try
            {
                _logger.LogInformation("Creating first owner account for {Email}", email);

                // Double-check: owner should not exist
                var ownerExists = await OwnerExistsAsync();
                if (ownerExists)
                {
                    var error = "Owner account already exists. Setup is only allowed once.";
                    _logger.LogWarning("Owner already exists, setup blocked");
                    return (false, error, null);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    var error = "An account with this email already exists.";
                    _logger.LogWarning("Email {Email} already exists", email);
                    return (false, error, null);
                }

                // Create user
                var user = new ApplicationUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, password);
                
                if (createResult.Succeeded)
                {
                    // Set UserName, Email, and confirm email using UserManager methods
                    await _userManager.SetUserNameAsync(user, email);
                    await _userManager.SetEmailAsync(user, email);
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }
                
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed: {Errors}", errors);
                    return (false, errors, null);
                }

                _logger.LogInformation("User created successfully: {UserId}", user.Id);

                // Ensure PlatformAdmin role exists
                var superAdminRole = await _roleManager.FindByNameAsync("PlatformAdmin");
                if (superAdminRole == null)
                {
                    superAdminRole = new IdentityRole("PlatformAdmin");
                    var roleResult = await _roleManager.CreateAsync(superAdminRole);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("PlatformAdmin role creation failed: {Errors}", errors);
                        return (false, $"Failed to create PlatformAdmin role: {errors}", null);
                    }
                }

                // Add user to PlatformAdmin role
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "PlatformAdmin");
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add user to PlatformAdmin role: {Errors}", errors);
                    return (false, $"Failed to assign PlatformAdmin role: {errors}", null);
                }

                _logger.LogInformation("First owner account created: {Email} (ID: {UserId})", email, user.Id);
                return (true, null, user.Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating first owner account");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Check if owner setup is required (no owner exists)
        /// </summary>
        public async Task<bool> IsOwnerSetupRequiredAsync()
        {
            var ownerExists = await OwnerExistsAsync();
            return !ownerExists;
        }
    }
}
