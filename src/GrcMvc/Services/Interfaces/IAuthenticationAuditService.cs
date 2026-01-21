using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for comprehensive authentication audit logging
    /// Required for GRC compliance and security monitoring
    /// </summary>
    public interface IAuthenticationAuditService
    {
        /// <summary>
        /// Log authentication event (login, logout, failed login, etc.)
        /// </summary>
        Task LogAuthenticationEventAsync(AuthenticationAuditEvent authEvent);

        /// <summary>
        /// Log login attempt (success or failure)
        /// </summary>
        Task LogLoginAttemptAsync(
            string? userId,
            string? email,
            bool success,
            string ipAddress,
            string? userAgent = null,
            string? failureReason = null,
            bool triggeredLockout = false);

        /// <summary>
        /// Log password change event
        /// </summary>
        Task LogPasswordChangeAsync(
            string userId,
            string? changedByUserId,
            string reason,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Log account lockout event
        /// </summary>
        Task LogAccountLockoutAsync(
            string userId,
            string reason,
            string? ipAddress = null);

        /// <summary>
        /// Log role assignment/removal
        /// </summary>
        Task LogRoleChangeAsync(
            string userId,
            string roleName,
            bool added,
            string? changedByUserId = null,
            string? ipAddress = null);

        /// <summary>
        /// Log claims modification
        /// </summary>
        Task LogClaimsModificationAsync(
            string userId,
            IEnumerable<string> addedClaims,
            IEnumerable<string> removedClaims,
            string? changedByUserId = null,
            string? ipAddress = null);

        /// <summary>
        /// Get authentication audit logs for a user
        /// </summary>
        Task<IEnumerable<AuthenticationAuditLog>> GetUserAuditLogsAsync(
            string userId,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 100);

        /// <summary>
        /// Get failed login attempts for an IP address (for brute force detection)
        /// </summary>
        Task<IEnumerable<LoginAttempt>> GetFailedLoginAttemptsByIpAsync(
            string ipAddress,
            TimeSpan? timeWindow = null);

        /// <summary>
        /// Get recent login attempts for a user
        /// </summary>
        Task<IEnumerable<LoginAttempt>> GetRecentLoginAttemptsAsync(
            string userId,
            int limit = 10);
    }

    /// <summary>
    /// Authentication audit event DTO for logging
    /// </summary>
    public class AuthenticationAuditEvent
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public required string EventType { get; set; }
        public bool Success { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Message { get; set; }
        public string Severity { get; set; } = "Info";
        public string? CorrelationId { get; set; }
        public Dictionary<string, object>? Details { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }
}
