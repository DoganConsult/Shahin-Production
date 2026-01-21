using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for looking up user information from the Auth database.
    /// Used to replace cross-database joins after auth DB split.
    /// All methods use batch lookups to avoid N+1 queries.
    /// </summary>
    public interface IUserDirectoryService
    {
        // ===== USER LOOKUPS =====

        /// <summary>
        /// Get a single user by ID
        /// </summary>
        Task<ApplicationUser?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Get multiple users by IDs (batch lookup)
        /// </summary>
        Task<Dictionary<string, ApplicationUser>> GetUsersByIdsAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Search users by name or email
        /// </summary>
        Task<List<ApplicationUser>> SearchUsersAsync(string searchTerm, int maxResults = 20);

        /// <summary>
        /// Get all active users (use sparingly - for admin views only)
        /// </summary>
        Task<List<ApplicationUser>> GetAllActiveUsersAsync();

        /// <summary>
        /// Get user count
        /// </summary>
        Task<int> GetUserCountAsync();

        // ===== ROLE LOOKUPS =====

        /// <summary>
        /// Get a single role by ID
        /// </summary>
        Task<IdentityRole?> GetRoleByIdAsync(string roleId);

        /// <summary>
        /// Get multiple roles by IDs (batch lookup)
        /// </summary>
        Task<Dictionary<string, IdentityRole>> GetRolesByIdsAsync(IEnumerable<string> roleIds);

        /// <summary>
        /// Get role by name
        /// </summary>
        Task<IdentityRole?> GetRoleByNameAsync(string roleName);

        /// <summary>
        /// Get all roles
        /// </summary>
        Task<List<IdentityRole>> GetAllRolesAsync();

        // ===== USER-ROLE LOOKUPS =====

        /// <summary>
        /// Get roles for a user
        /// </summary>
        Task<List<string>> GetUserRoleNamesAsync(string userId);

        /// <summary>
        /// Get users in a role
        /// </summary>
        Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName);

        /// <summary>
        /// Check if user is in role
        /// </summary>
        Task<bool> IsUserInRoleAsync(string userId, string roleName);

        // ===== DISPLAY HELPERS =====

        /// <summary>
        /// Get display name for a user (FirstName LastName or email if names empty)
        /// </summary>
        Task<string> GetUserDisplayNameAsync(string userId);

        /// <summary>
        /// Get display names for multiple users (batch)
        /// </summary>
        Task<Dictionary<string, string>> GetUserDisplayNamesAsync(IEnumerable<string> userIds);
    }
}
