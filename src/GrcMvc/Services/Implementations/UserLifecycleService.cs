using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of user lifecycle management service.
    /// Handles Joiner/Mover/Leaver operations with full audit trail.
    /// </summary>
    public class UserLifecycleService : IUserLifecycleService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly ILogger<UserLifecycleService> _logger;

        public UserLifecycleService(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            ILogger<UserLifecycleService> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserLifecycleResult> ActivateUserAsync(Guid userId, Guid tenantId, string activationMethod, Guid? activatedBy = null)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var currentStatus = ParseStatus(tenantUser.Status);

            // Validate transition is allowed
            if (!UserStatusTransitions.IsValidTransition(currentStatus, UserStatus.Active))
            {
                var reason = UserStatusTransitions.GetInvalidTransitionReason(currentStatus, UserStatus.Active);
                return UserLifecycleResult.Failed(reason, "INVALID_TRANSITION");
            }

            // Update status
            var previousStatus = tenantUser.Status;
            tenantUser.Status = UserStatus.Active.ToString();
            tenantUser.ActivatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Log audit event
            await _auditService.LogUserActivatedAsync(userId, email, tenantId, activationMethod, activatedBy);
            await _auditService.LogStatusChangedAsync(userId, email, currentStatus, UserStatus.Active, tenantId, activatedBy, $"Activated via {activationMethod}");

            _logger.LogInformation("User {UserId} activated in tenant {TenantId} via {Method}", userId, tenantId, activationMethod);

            return UserLifecycleResult.Succeeded("User activated successfully", currentStatus, UserStatus.Active);
        }

        public async Task<UserLifecycleResult> SuspendUserAsync(Guid userId, Guid tenantId, Guid suspendedBy, string reason)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var currentStatus = ParseStatus(tenantUser.Status);

            if (!UserStatusTransitions.IsValidTransition(currentStatus, UserStatus.Suspended))
                return UserLifecycleResult.Failed($"Cannot suspend user in {currentStatus} status", "INVALID_TRANSITION");

            // Update status
            tenantUser.Status = UserStatus.Suspended.ToString();

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Revoke all sessions
            await RevokeAllUserSessionsAsync(userId, suspendedBy, "User suspended");

            // Log audit events
            await _auditService.LogUserSuspendedAsync(userId, email, tenantId, suspendedBy, reason);
            await _auditService.LogStatusChangedAsync(userId, email, currentStatus, UserStatus.Suspended, tenantId, suspendedBy, reason);

            _logger.LogInformation("User {UserId} suspended in tenant {TenantId} by {SuspendedBy}: {Reason}", userId, tenantId, suspendedBy, reason);

            return UserLifecycleResult.Succeeded("User suspended successfully", currentStatus, UserStatus.Suspended);
        }

        public async Task<UserLifecycleResult> ReactivateUserAsync(Guid userId, Guid tenantId, Guid reactivatedBy)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var currentStatus = ParseStatus(tenantUser.Status);

            if (!UserStatusTransitions.CanReactivate(currentStatus))
                return UserLifecycleResult.Failed($"Cannot reactivate user in {currentStatus} status", "INVALID_TRANSITION");

            // Update status
            tenantUser.Status = UserStatus.Active.ToString();

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Log audit events
            await _auditService.LogUserReactivatedAsync(userId, email, tenantId, reactivatedBy);
            await _auditService.LogStatusChangedAsync(userId, email, currentStatus, UserStatus.Active, tenantId, reactivatedBy, "User reactivated");

            _logger.LogInformation("User {UserId} reactivated in tenant {TenantId} by {ReactivatedBy}", userId, tenantId, reactivatedBy);

            return UserLifecycleResult.Succeeded("User reactivated successfully", currentStatus, UserStatus.Active);
        }

        public async Task<UserLifecycleResult> DeprovisionUserAsync(Guid userId, Guid tenantId, Guid deprovisionedBy, string reason)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var currentStatus = ParseStatus(tenantUser.Status);

            if (currentStatus == UserStatus.Deprovisioned)
                return UserLifecycleResult.Failed("User is already deprovisioned", "ALREADY_DEPROVISIONED");

            // Update status
            tenantUser.Status = UserStatus.Deprovisioned.ToString();
            tenantUser.IsDeleted = true;
            tenantUser.DeletedAt = DateTime.UtcNow;

            // Remove workspace memberships
            await RemoveWorkspaceMembershipsAsync(userId, tenantId);

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Revoke all sessions
            await RevokeAllUserSessionsAsync(userId, deprovisionedBy, "User deprovisioned");

            // Log audit events
            await _auditService.LogUserDeprovisionedAsync(userId, email, tenantId, deprovisionedBy, reason);
            await _auditService.LogStatusChangedAsync(userId, email, currentStatus, UserStatus.Deprovisioned, tenantId, deprovisionedBy, reason);

            _logger.LogInformation("User {UserId} deprovisioned in tenant {TenantId} by {DeprovisionedBy}: {Reason}", userId, tenantId, deprovisionedBy, reason);

            return UserLifecycleResult.Succeeded("User deprovisioned successfully", currentStatus, UserStatus.Deprovisioned);
        }

        public async Task<UserLifecycleResult> SuspendForInactivityAsync(Guid userId, Guid tenantId, int inactiveDays)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var currentStatus = ParseStatus(tenantUser.Status);

            if (currentStatus != UserStatus.Active)
                return UserLifecycleResult.Failed($"Cannot suspend inactive user in {currentStatus} status", "INVALID_TRANSITION");

            // Update status
            tenantUser.Status = UserStatus.InactiveSuspended.ToString();

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Log audit events
            await _auditService.LogInactivitySuspensionAsync(userId, email, tenantId, inactiveDays);
            await _auditService.LogStatusChangedAsync(userId, email, currentStatus, UserStatus.InactiveSuspended, tenantId, null, $"Suspended due to {inactiveDays} days of inactivity");

            _logger.LogInformation("User {UserId} suspended for inactivity ({InactiveDays} days) in tenant {TenantId}", userId, inactiveDays, tenantId);

            return UserLifecycleResult.Succeeded("User suspended for inactivity", currentStatus, UserStatus.InactiveSuspended);
        }

        public async Task<UserLifecycleResult> ChangeUserRoleAsync(Guid userId, Guid tenantId, string newRole, Guid changedBy, string? reason = null)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var previousRole = tenantUser.RoleCode;

            // Normalize role code
            newRole = RoleConstants.NormalizeRoleCode(newRole);

            tenantUser.RoleCode = newRole;

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Log audit event
            await _auditService.LogRoleChangedAsync(userId, email, previousRole ?? "None", newRole, tenantId, changedBy);

            _logger.LogInformation("User {UserId} role changed from {OldRole} to {NewRole} in tenant {TenantId}", userId, previousRole, newRole, tenantId);

            return UserLifecycleResult.Succeeded($"Role changed from {previousRole} to {newRole}");
        }

        public async Task<UserLifecycleResult> RemoveUserRoleAsync(Guid userId, Guid tenantId, string role, Guid removedBy, string? reason = null)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserLifecycleResult.Failed("User not found in tenant", "USER_NOT_FOUND");

            var previousRole = tenantUser.RoleCode;

            // Set to default employee role
            tenantUser.RoleCode = RoleConstants.Employee;

            await _dbContext.SaveChangesAsync();

            // Get user email for audit
            var user = await _dbContext.Users.FindAsync(userId.ToString());
            var email = user?.Email ?? "unknown";

            // Log audit event
            await _auditService.LogRoleRemovedAsync(userId, email, role, tenantId, removedBy);

            _logger.LogInformation("User {UserId} role {Role} removed in tenant {TenantId}", userId, role, tenantId);

            return UserLifecycleResult.Succeeded($"Role {role} removed");
        }

        public async Task RevokeUserSessionAsync(Guid userId, Guid sessionId, Guid? revokedBy, string reason)
        {
            // Get session from GrcAuthDbContext (RefreshToken table)
            // This would need to inject GrcAuthDbContext
            await _auditService.LogSessionRevokedAsync(userId, sessionId, revokedBy, reason);
            _logger.LogInformation("Session {SessionId} revoked for user {UserId}: {Reason}", sessionId, userId, reason);
        }

        public async Task RevokeAllUserSessionsAsync(Guid userId, Guid revokedBy, string reason)
        {
            // Revoke all sessions in GrcAuthDbContext
            await _auditService.LogAllSessionsRevokedAsync(userId, revokedBy, reason);
            _logger.LogInformation("All sessions revoked for user {UserId}: {Reason}", userId, reason);
        }

        public async Task<UserStatus> GetUserStatusAsync(Guid userId, Guid tenantId)
        {
            var tenantUser = await GetTenantUserAsync(userId, tenantId);
            if (tenantUser == null)
                return UserStatus.Deprovisioned;

            return ParseStatus(tenantUser.Status);
        }

        public async Task<IEnumerable<UserLifecycleEvent>> GetUserHistoryAsync(Guid userId, Guid tenantId)
        {
            // Query audit events for this user
            var events = await _auditService.GetEventsByUserAsync(userId, limit: 50);

            return events
                .Where(e => e.TenantId == tenantId && e.ControlNumber == "AM-05")
                .Select(e => new UserLifecycleEvent
                {
                    Id = e.Id,
                    EventType = e.EventType,
                    PreviousStatus = ParseStatusOrNull(e.PreviousValue),
                    NewStatus = ParseStatusOrNull(e.NewValue),
                    Reason = e.Description,
                    PerformedBy = e.ActorUserId,
                    Timestamp = e.Timestamp
                })
                .ToList();
        }

        public async Task<IEnumerable<Guid>> GetInactiveUsersAsync(Guid tenantId, int inactiveDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);

            // Get users who haven't logged in since cutoff and are active
            var inactiveUsers = await _dbContext.Set<TenantUser>()
                .Where(tu => tu.TenantId == tenantId &&
                             tu.Status == "Active" &&
                             !tu.IsDeleted &&
                             (tu.LastLoginAt == null || tu.LastLoginAt < cutoffDate))
                .Select(tu => Guid.Parse(tu.UserId))
                .ToListAsync();

            return inactiveUsers;
        }

        public async Task<bool> CanTransitionAsync(Guid userId, Guid tenantId, UserStatus targetStatus)
        {
            var currentStatus = await GetUserStatusAsync(userId, tenantId);
            return UserStatusTransitions.IsValidTransition(currentStatus, targetStatus);
        }

        public async Task<bool> IsUserActiveAsync(Guid userId, Guid tenantId)
        {
            var status = await GetUserStatusAsync(userId, tenantId);
            return status == UserStatus.Active;
        }

        public async Task<bool> CanAccessSystemAsync(Guid userId, Guid tenantId)
        {
            var status = await GetUserStatusAsync(userId, tenantId);
            return UserStatusTransitions.CanAccess(status);
        }

        // Private helper methods

        private async Task<TenantUser?> GetTenantUserAsync(Guid userId, Guid tenantId)
        {
            return await _dbContext.Set<TenantUser>()
                .FirstOrDefaultAsync(tu => tu.UserId.ToString() == userId.ToString() && tu.TenantId == tenantId && !tu.IsDeleted);
        }

        private async Task RemoveWorkspaceMembershipsAsync(Guid userId, Guid tenantId)
        {
            // Remove all workspace memberships for deprovisioned user
            var memberships = await _dbContext.Set<WorkspaceMembership>()
                .Where(wm => wm.UserId == userId.ToString())
                .ToListAsync();

            foreach (var membership in memberships)
            {
                membership.Status = "Removed";
            }
        }

        private static UserStatus ParseStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return UserStatus.PendingVerification;

            // Handle legacy status values
            return status.ToLowerInvariant() switch
            {
                "pending" => UserStatus.PendingInvitation,
                "pendingverification" => UserStatus.PendingVerification,
                "pendinginvitation" => UserStatus.PendingInvitation,
                "pendingpasswordset" => UserStatus.PendingPasswordSet,
                "active" => UserStatus.Active,
                "suspended" => UserStatus.Suspended,
                "deprovisioned" => UserStatus.Deprovisioned,
                "cancelled" => UserStatus.Deprovisioned,
                "inactive" => UserStatus.InactiveSuspended,
                "inactivesuspended" => UserStatus.InactiveSuspended,
                _ => Enum.TryParse<UserStatus>(status, true, out var parsed) ? parsed : UserStatus.PendingVerification
            };
        }

        private static UserStatus? ParseStatusOrNull(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return null;

            return Enum.TryParse<UserStatus>(status, true, out var parsed) ? parsed : null;
        }
    }
}
