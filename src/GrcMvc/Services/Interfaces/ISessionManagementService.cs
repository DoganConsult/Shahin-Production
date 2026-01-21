using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing user sessions and enforcing concurrent session limits
    /// SECURITY: Implements session limiting for GRC compliance
    /// </summary>
    public interface ISessionManagementService
    {
        /// <summary>
        /// Get the number of active sessions for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>Count of active sessions</returns>
        Task<int> GetActiveSessionCountAsync(string userId);

        /// <summary>
        /// Get all active sessions for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>List of active refresh tokens representing sessions</returns>
        Task<List<RefreshToken>> GetActiveSessionsAsync(string userId);

        /// <summary>
        /// Check if a new session can be created (based on max session limit)
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="maxSessions">Maximum allowed sessions (default: 5)</param>
        /// <returns>True if a new session can be created</returns>
        Task<bool> CanCreateSessionAsync(string userId, int maxSessions = 5);

        /// <summary>
        /// Create a new session (refresh token) for a user
        /// If the user has exceeded max sessions, the oldest session will be revoked
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="tokenHash">The hashed refresh token</param>
        /// <param name="expiresAt">When the token expires</param>
        /// <param name="ipAddress">IP address of the request</param>
        /// <param name="userAgent">User agent of the request</param>
        /// <param name="deviceFingerprint">Optional device fingerprint</param>
        /// <param name="maxSessions">Maximum allowed sessions (oldest will be revoked if exceeded)</param>
        /// <returns>The created refresh token</returns>
        Task<RefreshToken> CreateSessionAsync(
            string userId,
            string tokenHash,
            System.DateTime expiresAt,
            string? ipAddress = null,
            string? userAgent = null,
            string? deviceFingerprint = null,
            int maxSessions = 5);

        /// <summary>
        /// Revoke a specific session by token ID
        /// </summary>
        /// <param name="tokenId">The token ID to revoke</param>
        /// <param name="reason">Reason for revocation</param>
        Task RevokeSessionAsync(System.Guid tokenId, string reason = "User logout");

        /// <summary>
        /// Revoke all sessions for a user (e.g., on password change or security concern)
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="exceptTokenId">Optional token ID to exclude (for current session)</param>
        Task RevokeAllSessionsAsync(string userId, string reason, System.Guid? exceptTokenId = null);

        /// <summary>
        /// Validate a refresh token and return the token entity if valid
        /// </summary>
        /// <param name="tokenHash">The hashed refresh token to validate</param>
        /// <returns>The refresh token entity if valid, null otherwise</returns>
        Task<RefreshToken?> ValidateSessionAsync(string tokenHash);

        /// <summary>
        /// Clean up expired sessions (can be called by a background job)
        /// </summary>
        /// <param name="userId">Optional: Clean up for a specific user only</param>
        Task CleanupExpiredSessionsAsync(string? userId = null);
    }
}
