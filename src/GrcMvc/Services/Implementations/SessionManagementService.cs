using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Session Management Service - Enforces concurrent session limits for GRC compliance
    /// SECURITY: Limits the number of active sessions per user
    /// </summary>
    public class SessionManagementService : ISessionManagementService
    {
        private readonly GrcAuthDbContext _authContext;
        private readonly ILogger<SessionManagementService> _logger;
        private readonly IAuthenticationAuditService? _auditService;
        private readonly int _defaultMaxSessions;

        public SessionManagementService(
            GrcAuthDbContext authContext,
            ILogger<SessionManagementService> logger,
            IConfiguration configuration,
            IAuthenticationAuditService? auditService = null)
        {
            _authContext = authContext;
            _logger = logger;
            _auditService = auditService;
            _defaultMaxSessions = configuration.GetValue<int>("Security:Session:MaxConcurrentSessions", 5);
        }

        /// <summary>
        /// Get the number of active (non-revoked, non-expired) sessions for a user
        /// </summary>
        public async Task<int> GetActiveSessionCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            return await _authContext.RefreshTokens
                .CountAsync(rt => rt.UserId == userId 
                    && rt.RevokedAt == null 
                    && rt.ExpiresAt > DateTime.UtcNow);
        }

        /// <summary>
        /// Get all active sessions for a user
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveSessionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<RefreshToken>();
            }

            return await _authContext.RefreshTokens
                .Where(rt => rt.UserId == userId 
                    && rt.RevokedAt == null 
                    && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Check if a new session can be created (based on max session limit)
        /// </summary>
        public async Task<bool> CanCreateSessionAsync(string userId, int maxSessions = 0)
        {
            if (maxSessions <= 0)
            {
                maxSessions = _defaultMaxSessions;
            }

            var activeCount = await GetActiveSessionCountAsync(userId);
            return activeCount < maxSessions;
        }

        /// <summary>
        /// Create a new session and revoke the oldest if max sessions exceeded
        /// </summary>
        public async Task<RefreshToken> CreateSessionAsync(
            string userId,
            string tokenHash,
            DateTime expiresAt,
            string? ipAddress = null,
            string? userAgent = null,
            string? deviceFingerprint = null,
            int maxSessions = 0)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (maxSessions <= 0)
            {
                maxSessions = _defaultMaxSessions;
            }

            // Check if we need to revoke old sessions
            var activeSessions = await GetActiveSessionsAsync(userId);
            if (activeSessions.Count >= maxSessions)
            {
                // Revoke oldest sessions to make room
                var sessionsToRevoke = activeSessions
                    .OrderBy(s => s.CreatedAt)
                    .Take(activeSessions.Count - maxSessions + 1)
                    .ToList();

                foreach (var session in sessionsToRevoke)
                {
                    session.RevokedAt = DateTime.UtcNow;
                    session.RevokedReason = "Session limit exceeded - oldest session revoked";
                    _logger.LogInformation(
                        "Revoked session {TokenId} for user {UserId} due to session limit", 
                        session.Id, userId);
                }
            }

            // Create the new session
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceFingerprint = deviceFingerprint
            };

            _authContext.RefreshTokens.Add(refreshToken);
            await _authContext.SaveChangesAsync();

            _logger.LogInformation("Created new session {TokenId} for user {UserId}", refreshToken.Id, userId);

            return refreshToken;
        }

        /// <summary>
        /// Revoke a specific session by token ID
        /// </summary>
        public async Task RevokeSessionAsync(Guid tokenId, string reason = "User logout")
        {
            var token = await _authContext.RefreshTokens.FindAsync(tokenId);
            if (token != null && token.RevokedAt == null)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = reason;
                await _authContext.SaveChangesAsync();

                _logger.LogInformation("Revoked session {TokenId} for user {UserId}: {Reason}", 
                    tokenId, token.UserId, reason);
            }
        }

        /// <summary>
        /// Revoke all sessions for a user (e.g., on password change)
        /// </summary>
        public async Task RevokeAllSessionsAsync(string userId, string reason, Guid? exceptTokenId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var sessions = await _authContext.RefreshTokens
                .Where(rt => rt.UserId == userId 
                    && rt.RevokedAt == null
                    && (exceptTokenId == null || rt.Id != exceptTokenId))
                .ToListAsync();

            var revokedCount = 0;
            foreach (var session in sessions)
            {
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = reason;
                revokedCount++;
            }

            await _authContext.SaveChangesAsync();

            _logger.LogInformation(
                "Revoked {Count} sessions for user {UserId}: {Reason}", 
                revokedCount, userId, reason);

            // Log audit event
            if (_auditService != null)
            {
                await _auditService.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = userId,
                    EventType = "AllSessionsRevoked",
                    Success = true,
                    Message = $"Revoked {revokedCount} sessions: {reason}",
                    Severity = "Warning"
                });
            }
        }

        /// <summary>
        /// Validate a refresh token
        /// </summary>
        public async Task<RefreshToken?> ValidateSessionAsync(string tokenHash)
        {
            if (string.IsNullOrEmpty(tokenHash))
            {
                return null;
            }

            var token = await _authContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (token == null)
            {
                _logger.LogWarning("Token not found during validation");
                return null;
            }

            if (token.RevokedAt != null)
            {
                _logger.LogWarning("Attempted to use revoked token {TokenId}", token.Id);
                return null;
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Attempted to use expired token {TokenId}", token.Id);
                return null;
            }

            return token;
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public async Task CleanupExpiredSessionsAsync(string? userId = null)
        {
            var query = _authContext.RefreshTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.RevokedAt != null);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(rt => rt.UserId == userId);
            }

            // Only delete tokens that are at least 30 days old (for audit trail)
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var tokensToDelete = await query
                .Where(rt => rt.CreatedAt < cutoffDate)
                .ToListAsync();

            if (tokensToDelete.Any())
            {
                _authContext.RefreshTokens.RemoveRange(tokensToDelete);
                await _authContext.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired/revoked tokens", tokensToDelete.Count);
            }
        }
    }
}
