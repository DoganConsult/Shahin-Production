using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing password history to prevent password reuse
    /// SECURITY: Enforces password reuse policy for GRC compliance
    /// </summary>
    public interface IPasswordHistoryService
    {
        /// <summary>
        /// Check if the new password was used recently (within the history limit)
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="newPassword">The new password to check (plain text)</param>
        /// <param name="historyCount">Number of previous passwords to check (default: 5)</param>
        /// <returns>True if password was used recently and should be rejected</returns>
        Task<bool> IsPasswordInHistoryAsync(string userId, string newPassword, int historyCount = 5);

        /// <summary>
        /// Add a password hash to the user's password history
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="passwordHash">The password hash to store</param>
        /// <param name="changedByUserId">ID of user who initiated the change</param>
        /// <param name="reason">Reason for password change</param>
        /// <param name="ipAddress">IP address of the request</param>
        /// <param name="userAgent">User agent of the request</param>
        Task AddPasswordToHistoryAsync(
            string userId,
            string passwordHash,
            string? changedByUserId = null,
            string? reason = null,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Get the number of password history entries for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>Count of password history entries</returns>
        Task<int> GetPasswordHistoryCountAsync(string userId);

        /// <summary>
        /// Clean up old password history entries beyond the retention limit
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="retainCount">Number of most recent entries to retain (default: 10)</param>
        Task CleanupOldHistoryAsync(string userId, int retainCount = 10);
    }
}
