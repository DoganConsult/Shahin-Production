using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of authentication audit logging service
    /// Provides comprehensive audit trail for all authentication-related events
    /// </summary>
    public class AuthenticationAuditService : IAuthenticationAuditService
    {
        private readonly GrcAuthDbContext _authContext;
        private readonly ILogger<AuthenticationAuditService> _logger;

        public AuthenticationAuditService(
            GrcAuthDbContext authContext,
            ILogger<AuthenticationAuditService> logger)
        {
            _authContext = authContext;
            _logger = logger;
        }

        /// <summary>
        /// Log authentication event (login, logout, failed login, etc.)
        /// </summary>
        public async Task LogAuthenticationEventAsync(AuthenticationAuditEvent authEvent)
        {
            try
            {
                var auditLog = new AuthenticationAuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = authEvent.UserId,
                    Email = authEvent.Email,
                    EventType = authEvent.EventType,
                    Success = authEvent.Success,
                    IpAddress = authEvent.IpAddress,
                    UserAgent = authEvent.UserAgent,
                    Message = authEvent.Message,
                    Severity = authEvent.Severity,
                    CorrelationId = authEvent.CorrelationId ?? Guid.NewGuid().ToString(),
                    RelatedEntityId = authEvent.RelatedEntityId,
                    RelatedEntityType = authEvent.RelatedEntityType,
                    Timestamp = DateTime.UtcNow
                };

                // Serialize details to JSON
                if (authEvent.Details != null && authEvent.Details.Any())
                {
                    auditLog.Details = JsonSerializer.Serialize(authEvent.Details);
                }

                _authContext.AuthenticationAuditLogs.Add(auditLog);
                await _authContext.SaveChangesAsync();

                _logger.LogDebug(
                    "Authentication audit event logged: {EventType} for user {UserId}, Success: {Success}",
                    authEvent.EventType, authEvent.UserId, authEvent.Success);
            }
            catch (Exception ex)
            {
                // Never fail the main operation due to audit logging failure
                _logger.LogError(ex,
                    "Failed to log authentication audit event: {EventType} for user {UserId}",
                    authEvent.EventType, authEvent.UserId);
            }
        }

        /// <summary>
        /// Log login attempt (success or failure)
        /// </summary>
        public async Task LogLoginAttemptAsync(
            string? userId,
            string? email,
            bool success,
            string ipAddress,
            string? userAgent = null,
            string? failureReason = null,
            bool triggeredLockout = false)
        {
            try
            {
                var loginAttempt = new LoginAttempt
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AttemptedEmail = email,
                    Success = success,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    FailureReason = failureReason,
                    TriggeredLockout = triggeredLockout,
                    Timestamp = DateTime.UtcNow
                };

                _authContext.LoginAttempts.Add(loginAttempt);
                await _authContext.SaveChangesAsync();

                // Also log as authentication audit event
                await LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = userId,
                    Email = email,
                    EventType = success ? "Login" : "FailedLogin",
                    Success = success,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Message = success 
                        ? $"Successful login for {email}" 
                        : $"Failed login attempt for {email}: {failureReason}",
                    Severity = success ? "Info" : "Warning",
                    Details = new Dictionary<string, object>
                    {
                        ["TriggeredLockout"] = triggeredLockout,
                        ["FailureReason"] = failureReason ?? "N/A"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to log login attempt for user {UserId}, email {Email}",
                    userId, email);
            }
        }

        /// <summary>
        /// Log password change event
        /// </summary>
        public async Task LogPasswordChangeAsync(
            string userId,
            string? changedByUserId,
            string reason,
            string? ipAddress = null,
            string? userAgent = null)
        {
            try
            {
                await LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = userId,
                    EventType = "PasswordChanged",
                    Success = true,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Message = $"Password changed. Reason: {reason}",
                    Severity = "Info",
                    Details = new Dictionary<string, object>
                    {
                        ["ChangedByUserId"] = changedByUserId ?? userId,
                        ["Reason"] = reason
                    },
                    RelatedEntityId = changedByUserId,
                    RelatedEntityType = "User"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log password change for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Log account lockout event
        /// </summary>
        public async Task LogAccountLockoutAsync(
            string userId,
            string reason,
            string? ipAddress = null)
        {
            try
            {
                await LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = userId,
                    EventType = "AccountLocked",
                    Success = false,
                    IpAddress = ipAddress,
                    Message = $"Account locked. Reason: {reason}",
                    Severity = "Warning",
                    Details = new Dictionary<string, object>
                    {
                        ["Reason"] = reason
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log account lockout for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Log role assignment/removal
        /// </summary>
        public async Task LogRoleChangeAsync(
            string userId,
            string roleName,
            bool added,
            string? changedByUserId = null,
            string? ipAddress = null)
        {
            try
            {
                await LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = userId,
                    EventType = added ? "RoleAdded" : "RoleRemoved",
                    Success = true,
                    IpAddress = ipAddress,
                    Message = $"Role '{roleName}' {(added ? "added to" : "removed from")} user",
                    Severity = "Info",
                    Details = new Dictionary<string, object>
                    {
                        ["RoleName"] = roleName,
                        ["Action"] = added ? "Added" : "Removed",
                        ["ChangedByUserId"] = changedByUserId ?? "System"
                    },
                    RelatedEntityId = changedByUserId,
                    RelatedEntityType = "Role"
                });
            }
            catch (Exception ex)
            {
                var action = added ? "added to" : "removed from";
                _logger.LogError(ex,
                    "Failed to log role change: {RoleName} {Action} user {UserId}",
                    roleName, action, userId);
            }
        }

        /// <summary>
        /// Log claims modification
        /// </summary>
        public async Task LogClaimsModificationAsync(
            string userId,
            IEnumerable<string> addedClaims,
            IEnumerable<string> removedClaims,
            string? changedByUserId = null,
            string? ipAddress = null)
        {
            try
            {
                var added = addedClaims.ToList();
                var removed = removedClaims.ToList();

                if (!added.Any() && !removed.Any())
                    return;

                await LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = userId,
                    EventType = "ClaimsModified",
                    Success = true,
                    IpAddress = ipAddress,
                    Message = $"Claims modified. Added: {added.Count}, Removed: {removed.Count}",
                    Severity = "Info",
                    Details = new Dictionary<string, object>
                    {
                        ["AddedClaims"] = added,
                        ["RemovedClaims"] = removed,
                        ["ChangedByUserId"] = changedByUserId ?? "System"
                    },
                    RelatedEntityId = changedByUserId,
                    RelatedEntityType = "Claims"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log claims modification for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get authentication audit logs for a user
        /// </summary>
        public async Task<IEnumerable<AuthenticationAuditLog>> GetUserAuditLogsAsync(
            string userId,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 100)
        {
            try
            {
                var query = _authContext.AuthenticationAuditLogs
                    .Where(aal => aal.UserId == userId)
                    .AsQueryable();

                if (from.HasValue)
                    query = query.Where(aal => aal.Timestamp >= from.Value);

                if (to.HasValue)
                    query = query.Where(aal => aal.Timestamp <= to.Value);

                return await query
                    .OrderByDescending(aal => aal.Timestamp)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get audit logs for user {UserId}", userId);
                return Enumerable.Empty<AuthenticationAuditLog>();
            }
        }

        /// <summary>
        /// Get failed login attempts for an IP address (for brute force detection)
        /// </summary>
        public async Task<IEnumerable<LoginAttempt>> GetFailedLoginAttemptsByIpAsync(
            string ipAddress,
            TimeSpan? timeWindow = null)
        {
            try
            {
                var query = _authContext.LoginAttempts
                    .Where(la => la.IpAddress == ipAddress && !la.Success)
                    .AsQueryable();

                if (timeWindow.HasValue)
                {
                    var cutoff = DateTime.UtcNow - timeWindow.Value;
                    query = query.Where(la => la.Timestamp >= cutoff);
                }

                return await query
                    .OrderByDescending(la => la.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get failed login attempts for IP {IpAddress}", ipAddress);
                return Enumerable.Empty<LoginAttempt>();
            }
        }

        /// <summary>
        /// Get recent login attempts for a user
        /// </summary>
        public async Task<IEnumerable<LoginAttempt>> GetRecentLoginAttemptsAsync(
            string userId,
            int limit = 10)
        {
            try
            {
                return await _authContext.LoginAttempts
                    .Where(la => la.UserId == userId)
                    .OrderByDescending(la => la.Timestamp)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent login attempts for user {UserId}", userId);
                return Enumerable.Empty<LoginAttempt>();
            }
        }
    }
}
