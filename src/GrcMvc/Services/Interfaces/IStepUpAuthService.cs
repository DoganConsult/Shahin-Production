using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// AM-04: Step-Up Authentication Service for privileged actions.
    /// Requires re-authentication for sensitive operations even when user is logged in.
    /// </summary>
    public interface IStepUpAuthService
    {
        /// <summary>
        /// Check if the current session has valid step-up authentication.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <param name="sessionId">Current session ID</param>
        /// <param name="action">Action requiring step-up auth (e.g., "role.change")</param>
        /// <returns>True if step-up auth is valid and not expired</returns>
        Task<bool> HasValidStepUpAuthAsync(Guid userId, Guid sessionId, string action);

        /// <summary>
        /// Record successful step-up authentication for a session.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="action">Action that was authenticated</param>
        /// <param name="method">Authentication method used (MFA, password, etc.)</param>
        Task RecordStepUpAuthAsync(Guid userId, Guid sessionId, string action, string method);

        /// <summary>
        /// Check if an action requires step-up authentication.
        /// </summary>
        /// <param name="action">Action to check</param>
        /// <returns>True if step-up auth is required</returns>
        bool RequiresStepUpAuth(string action);

        /// <summary>
        /// Get remaining validity time for step-up auth.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="action">Action to check</param>
        /// <returns>Time remaining, or null if not valid</returns>
        Task<TimeSpan?> GetStepUpAuthValidityAsync(Guid userId, Guid sessionId, string action);

        /// <summary>
        /// Invalidate step-up auth for a session (e.g., on logout or security event).
        /// </summary>
        /// <param name="sessionId">Session ID to invalidate</param>
        Task InvalidateStepUpAuthAsync(Guid sessionId);

        /// <summary>
        /// Check if a user has MFA enabled.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if MFA is enabled</returns>
        Task<bool> UserHasMfaEnabledAsync(Guid userId);

        /// <summary>
        /// Check if MFA is required for a user based on their roles.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>True if MFA is required</returns>
        Task<bool> IsMfaRequiredForUserAsync(Guid userId, Guid tenantId);

        /// <summary>
        /// Verify MFA code for step-up authentication.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="code">MFA code to verify</param>
        /// <returns>Verification result</returns>
        Task<MfaVerificationResult> VerifyMfaCodeAsync(Guid userId, string code);
    }
}
