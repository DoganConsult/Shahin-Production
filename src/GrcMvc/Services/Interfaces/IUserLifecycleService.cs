using GrcMvc.Models.Enums;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing user lifecycle (Joiner/Mover/Leaver) operations.
    /// Implements AM-05 controls for access management.
    /// </summary>
    public interface IUserLifecycleService
    {
        // Status transition operations
        Task<UserLifecycleResult> ActivateUserAsync(Guid userId, Guid tenantId, string activationMethod, Guid? activatedBy = null);
        Task<UserLifecycleResult> SuspendUserAsync(Guid userId, Guid tenantId, Guid suspendedBy, string reason);
        Task<UserLifecycleResult> ReactivateUserAsync(Guid userId, Guid tenantId, Guid reactivatedBy);
        Task<UserLifecycleResult> DeprovisionUserAsync(Guid userId, Guid tenantId, Guid deprovisionedBy, string reason);
        Task<UserLifecycleResult> SuspendForInactivityAsync(Guid userId, Guid tenantId, int inactiveDays);

        // Role management (Mover)
        Task<UserLifecycleResult> ChangeUserRoleAsync(Guid userId, Guid tenantId, string newRole, Guid changedBy, string? reason = null);
        Task<UserLifecycleResult> RemoveUserRoleAsync(Guid userId, Guid tenantId, string role, Guid removedBy, string? reason = null);

        // Session management
        Task RevokeUserSessionAsync(Guid userId, Guid sessionId, Guid? revokedBy, string reason);
        Task RevokeAllUserSessionsAsync(Guid userId, Guid revokedBy, string reason);

        // Query operations
        Task<UserStatus> GetUserStatusAsync(Guid userId, Guid tenantId);
        Task<IEnumerable<UserLifecycleEvent>> GetUserHistoryAsync(Guid userId, Guid tenantId);
        Task<IEnumerable<Guid>> GetInactiveUsersAsync(Guid tenantId, int inactiveDays);
        Task<bool> CanTransitionAsync(Guid userId, Guid tenantId, UserStatus targetStatus);

        // Validation
        Task<bool> IsUserActiveAsync(Guid userId, Guid tenantId);
        Task<bool> CanAccessSystemAsync(Guid userId, Guid tenantId);
    }

    /// <summary>
    /// Result of a user lifecycle operation.
    /// </summary>
    public class UserLifecycleResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserStatus? PreviousStatus { get; set; }
        public UserStatus? NewStatus { get; set; }
        public string? ErrorCode { get; set; }

        public static UserLifecycleResult Succeeded(string message, UserStatus? previousStatus = null, UserStatus? newStatus = null)
            => new() { Success = true, Message = message, PreviousStatus = previousStatus, NewStatus = newStatus };

        public static UserLifecycleResult Failed(string message, string? errorCode = null)
            => new() { Success = false, Message = message, ErrorCode = errorCode };
    }

    /// <summary>
    /// Represents a lifecycle event in user history.
    /// </summary>
    public class UserLifecycleEvent
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public UserStatus? PreviousStatus { get; set; }
        public UserStatus? NewStatus { get; set; }
        public string? Reason { get; set; }
        public Guid? PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
