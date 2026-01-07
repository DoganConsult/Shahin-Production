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
                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "check-owner", hypothesisId = "A", location = "OwnerSetupService.OwnerExistsAsync:entry", message = "Checking if owner exists", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion

                // Check if PlatformAdmin or Owner role exists
                var superAdminRole = await _roleManager.FindByNameAsync("PlatformAdmin");
                var ownerRole = await _roleManager.FindByNameAsync("Owner");

                if (superAdminRole == null && ownerRole == null)
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "check-owner", hypothesisId = "A", location = "OwnerSetupService.OwnerExistsAsync:no-roles", message = "No owner roles exist", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    return false;
                }

                // Get all users with PlatformAdmin or Owner role
                var ownerUserIds = new List<string>();

                if (superAdminRole != null)
                {
                    var superAdminUsers = await _authContext.UserRoles
                        .Where(ur => ur.RoleId == superAdminRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    ownerUserIds.AddRange(superAdminUsers);
                }

                if (ownerRole != null)
                {
                    var ownerUsers = await _authContext.UserRoles
                        .Where(ur => ur.RoleId == ownerRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    ownerUserIds.AddRange(ownerUsers);
                }

                var ownerExists = ownerUserIds.Any();

                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "check-owner", hypothesisId = "A", location = "OwnerSetupService.OwnerExistsAsync:result", message = "Owner existence check result", data = new { ownerExists, ownerUserIdsCount = ownerUserIds.Count, hasPlatformAdminRole = superAdminRole != null, hasOwnerRole = ownerRole != null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion

                return ownerExists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if owner exists");
                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "check-owner", hypothesisId = "A", location = "OwnerSetupService.OwnerExistsAsync:error", message = "Error checking owner existence", data = new { error = ex.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion
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
                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:entry", message = "Creating first owner account", data = new { email, firstName, lastName, organizationName, hasPassword = !string.IsNullOrEmpty(password) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion

                // Double-check: owner should not exist
                var ownerExists = await OwnerExistsAsync();
                if (ownerExists)
                {
                    var error = "Owner account already exists. Setup is only allowed once.";
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:owner-exists", message = "Owner already exists, setup blocked", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    return (false, error, null);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    var error = "An account with this email already exists.";
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:email-exists", message = "Email already exists", data = new { email }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    return (false, error, null);
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:create-failed", message = "User creation failed", data = new { errors }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    return (false, errors, null);
                }

                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:user-created", message = "User created successfully", data = new { userId = user.Id, email }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion

                // Ensure PlatformAdmin role exists
                var superAdminRole = await _roleManager.FindByNameAsync("PlatformAdmin");
                if (superAdminRole == null)
                {
                    superAdminRole = new IdentityRole("PlatformAdmin");
                    var roleResult = await _roleManager.CreateAsync(superAdminRole);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        // #region agent log
                        System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:role-create-failed", message = "PlatformAdmin role creation failed", data = new { errors }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                        // #endregion
                        return (false, $"Failed to create PlatformAdmin role: {errors}", null);
                    }
                }

                // Add user to PlatformAdmin role
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "PlatformAdmin");
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:add-role-failed", message = "Failed to add user to PlatformAdmin role", data = new { errors }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    return (false, $"Failed to assign PlatformAdmin role: {errors}", null);
                }

                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:success", message = "First owner created successfully", data = new { userId = user.Id, email, roleAssigned = true }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion

                _logger.LogInformation("First owner account created: {Email} (ID: {UserId})", email, user.Id);
                return (true, null, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating first owner account");
                // #region agent log
                System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "create-owner", hypothesisId = "B", location = "OwnerSetupService.CreateFirstOwnerAsync:exception", message = "Exception creating owner", data = new { error = ex.Message, stackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion
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
