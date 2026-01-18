using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// IP-based abuse detection service implementation.
    /// Tracks suspicious activities, manages IP blocking, and calculates risk scores.
    /// </summary>
    public class AbuseDetectionService : IAbuseDetectionService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<AbuseDetectionService> _logger;
        private readonly IConfiguration _configuration;
        
        // Configurable thresholds
        private readonly int _maxFailedLogins;
        private readonly int _maxFailedRegistrations;
        private readonly int _maxCaptchaFailures;
        private readonly int _maxRateLimitViolations;
        private readonly int _blockThresholdScore;
        private readonly int _captchaThresholdScore;
        private readonly int _autoBlockDurationHours;

        public AbuseDetectionService(
            GrcDbContext context,
            ILogger<AbuseDetectionService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            
            // Load thresholds from configuration
            var section = configuration.GetSection("AbuseDetection");
            _maxFailedLogins = section.GetValue("MaxFailedLogins", 5);
            _maxFailedRegistrations = section.GetValue("MaxFailedRegistrations", 3);
            _maxCaptchaFailures = section.GetValue("MaxCaptchaFailures", 5);
            _maxRateLimitViolations = section.GetValue("MaxRateLimitViolations", 10);
            _blockThresholdScore = section.GetValue("BlockThresholdScore", 80);
            _captchaThresholdScore = section.GetValue("CaptchaThresholdScore", 50);
            _autoBlockDurationHours = section.GetValue("AutoBlockDurationHours", 24);
        }

        public async Task<bool> IsIpAllowedAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return true;
            
            // Check whitelist first
            var isWhitelisted = await _context.Set<IpAccessList>()
                .AnyAsync(x => x.IpAddressOrRange == ipAddress && 
                              x.ListType == "Whitelist" && 
                              x.IsActive &&
                              (!x.ExpiresAt.HasValue || x.ExpiresAt > DateTime.UtcNow));
            
            if (isWhitelisted) return true;
            
            // Check blacklist
            var isBlacklisted = await _context.Set<IpAccessList>()
                .AnyAsync(x => x.IpAddressOrRange == ipAddress && 
                              x.ListType == "Blacklist" && 
                              x.IsActive &&
                              (!x.ExpiresAt.HasValue || x.ExpiresAt > DateTime.UtcNow));
            
            if (isBlacklisted) return false;
            
            // Check if blocked by abuse tracking
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            
            if (tracking.IsBlocked)
            {
                // Check if block has expired
                if (tracking.BlockExpiresAt.HasValue && tracking.BlockExpiresAt < DateTime.UtcNow)
                {
                    await UnblockIpAsync(ipAddress);
                    return true;
                }
                return false;
            }
            
            return true;
        }

        public async Task<bool> IsIpSuspiciousAsync(string ipAddress)
        {
            var riskScore = await GetIpRiskScoreAsync(ipAddress);
            return riskScore >= _captchaThresholdScore;
        }

        public async Task<int> GetIpRiskScoreAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return 0;
            
            var tracking = await _context.Set<AbuseIpTracking>()
                .FirstOrDefaultAsync(t => t.IpAddress == ipAddress);
            
            return tracking?.RiskScore ?? 0;
        }

        public async Task RecordFailedLoginAsync(string ipAddress, string? email = null, string? userAgent = null)
        {
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            tracking.FailedLoginAttempts++;
            tracking.TotalSuspiciousActivities++;
            tracking.LastActivityAt = DateTime.UtcNow;
            
            // Update risk score
            UpdateRiskScore(tracking);
            
            // Log the event
            await LogEventAsync(ipAddress, "FailedLogin", "Medium", email, userAgent, null, "None");
            
            // Auto-block if threshold exceeded
            await CheckAutoBlockAsync(tracking);
            
            await _context.SaveChangesAsync();
            
            _logger.LogWarning("Failed login recorded for IP {IpAddress}. Attempts: {Attempts}, Risk: {Risk}",
                ipAddress, tracking.FailedLoginAttempts, tracking.RiskScore);
        }

        public async Task RecordFailedRegistrationAsync(string ipAddress, string? email = null, string? userAgent = null)
        {
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            tracking.FailedRegistrationAttempts++;
            tracking.TotalSuspiciousActivities++;
            tracking.LastActivityAt = DateTime.UtcNow;
            
            UpdateRiskScore(tracking);
            await LogEventAsync(ipAddress, "FailedRegistration", "Medium", email, userAgent, null, "None");
            await CheckAutoBlockAsync(tracking);
            
            await _context.SaveChangesAsync();
        }

        public async Task RecordCaptchaFailureAsync(string ipAddress, string? userAgent = null)
        {
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            tracking.CaptchaFailures++;
            tracking.TotalSuspiciousActivities++;
            tracking.LastActivityAt = DateTime.UtcNow;
            
            UpdateRiskScore(tracking);
            await LogEventAsync(ipAddress, "CaptchaFailure", "High", null, userAgent, null, "None");
            await CheckAutoBlockAsync(tracking);
            
            await _context.SaveChangesAsync();
        }

        public async Task RecordRateLimitViolationAsync(string ipAddress, string? requestPath = null)
        {
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            tracking.RateLimitViolations++;
            tracking.TotalSuspiciousActivities++;
            tracking.LastActivityAt = DateTime.UtcNow;
            
            UpdateRiskScore(tracking);
            await LogEventAsync(ipAddress, "RateLimitViolation", "Medium", null, null, requestPath, "Warning");
            await CheckAutoBlockAsync(tracking);
            
            await _context.SaveChangesAsync();
        }

        public async Task RecordSuccessfulAuthAsync(string ipAddress)
        {
            var tracking = await _context.Set<AbuseIpTracking>()
                .FirstOrDefaultAsync(t => t.IpAddress == ipAddress);
            
            if (tracking != null)
            {
                // Reduce risk score on successful auth (but don't go below 0)
                tracking.RiskScore = Math.Max(0, tracking.RiskScore - 10);
                tracking.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RecordSuspiciousActivityAsync(string ipAddress, string eventType, string severity, string? details = null)
        {
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            tracking.TotalSuspiciousActivities++;
            tracking.LastActivityAt = DateTime.UtcNow;
            
            // Increase risk based on severity
            var severityBoost = severity switch
            {
                "Critical" => 25,
                "High" => 15,
                "Medium" => 10,
                _ => 5
            };
            tracking.RiskScore = Math.Min(100, tracking.RiskScore + severityBoost);
            
            await LogEventAsync(ipAddress, eventType, severity, null, null, null, "None");
            await CheckAutoBlockAsync(tracking);
            
            await _context.SaveChangesAsync();
        }

        public async Task BlockIpAsync(string ipAddress, string reason, TimeSpan? duration = null)
        {
            var tracking = await GetOrCreateTrackingAsync(ipAddress);
            tracking.IsBlocked = true;
            tracking.BlockedAt = DateTime.UtcNow;
            tracking.BlockExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null;
            tracking.BlockReason = reason;
            
            await LogEventAsync(ipAddress, "ManualBlock", "Critical", null, null, null, "Blocked");
            
            await _context.SaveChangesAsync();
            
            _logger.LogWarning("IP {IpAddress} blocked. Reason: {Reason}, Duration: {Duration}",
                ipAddress, reason, duration?.ToString() ?? "Permanent");
        }

        public async Task UnblockIpAsync(string ipAddress)
        {
            var tracking = await _context.Set<AbuseIpTracking>()
                .FirstOrDefaultAsync(t => t.IpAddress == ipAddress);
            
            if (tracking != null)
            {
                tracking.IsBlocked = false;
                tracking.BlockExpiresAt = null;
                
                // Reset counters but keep history
                tracking.FailedLoginAttempts = 0;
                tracking.FailedRegistrationAttempts = 0;
                tracking.CaptchaFailures = 0;
                tracking.RateLimitViolations = 0;
                tracking.RiskScore = Math.Max(0, tracking.RiskScore - 20);
                tracking.CountersResetAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("IP {IpAddress} unblocked", ipAddress);
            }
        }

        public async Task WhitelistIpAsync(string ipAddressOrRange, string reason, string? addedBy = null)
        {
            var existing = await _context.Set<IpAccessList>()
                .FirstOrDefaultAsync(x => x.IpAddressOrRange == ipAddressOrRange && x.ListType == "Whitelist");
            
            if (existing != null)
            {
                existing.IsActive = true;
                existing.Reason = reason;
                existing.AddedBy = addedBy;
                existing.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Set<IpAccessList>().Add(new IpAccessList
                {
                    IpAddressOrRange = ipAddressOrRange,
                    ListType = "Whitelist",
                    Reason = reason,
                    AddedBy = addedBy
                });
            }
            
            // Remove from blacklist if present
            var blacklisted = await _context.Set<IpAccessList>()
                .FirstOrDefaultAsync(x => x.IpAddressOrRange == ipAddressOrRange && x.ListType == "Blacklist");
            if (blacklisted != null)
            {
                blacklisted.IsActive = false;
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task BlacklistIpAsync(string ipAddressOrRange, string reason, TimeSpan? duration = null, string? addedBy = null)
        {
            var existing = await _context.Set<IpAccessList>()
                .FirstOrDefaultAsync(x => x.IpAddressOrRange == ipAddressOrRange && x.ListType == "Blacklist");
            
            if (existing != null)
            {
                existing.IsActive = true;
                existing.Reason = reason;
                existing.AddedBy = addedBy;
                existing.ExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null;
                existing.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Set<IpAccessList>().Add(new IpAccessList
                {
                    IpAddressOrRange = ipAddressOrRange,
                    ListType = "Blacklist",
                    Reason = reason,
                    AddedBy = addedBy,
                    ExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null
                });
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<AbuseAction> DetermineActionAsync(string ipAddress)
        {
            if (!await IsIpAllowedAsync(ipAddress))
                return AbuseAction.Block;
            
            var riskScore = await GetIpRiskScoreAsync(ipAddress);
            
            if (riskScore >= _blockThresholdScore)
                return AbuseAction.Block;
            
            if (riskScore >= _captchaThresholdScore)
                return AbuseAction.RequireCaptcha;
            
            if (riskScore >= 30)
                return AbuseAction.Warn;
            
            return AbuseAction.Allow;
        }

        public async Task CleanupExpiredDataAsync(int daysToKeep = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
            
            // Remove old event logs
            var oldLogs = await _context.Set<AbuseEventLog>()
                .Where(l => l.CreatedAt < cutoff)
                .ToListAsync();
            
            _context.Set<AbuseEventLog>().RemoveRange(oldLogs);
            
            // Remove tracking for IPs with no activity
            var inactiveTracking = await _context.Set<AbuseIpTracking>()
                .Where(t => t.LastActivityAt < cutoff && !t.IsBlocked)
                .ToListAsync();
            
            _context.Set<AbuseIpTracking>().RemoveRange(inactiveTracking);
            
            // Remove expired access list entries
            var expiredEntries = await _context.Set<IpAccessList>()
                .Where(x => x.ExpiresAt.HasValue && x.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
            
            foreach (var entry in expiredEntries)
            {
                entry.IsActive = false;
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Abuse detection cleanup: Removed {Logs} logs, {Tracking} inactive IPs",
                oldLogs.Count, inactiveTracking.Count);
        }

        public async Task<AbuseStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.UtcNow.Date;
            var toDate = to ?? DateTime.UtcNow;
            
            var todayStart = DateTime.UtcNow.Date;
            
            var stats = new AbuseStatistics
            {
                TotalTrackedIps = await _context.Set<AbuseIpTracking>().CountAsync(),
                BlockedIps = await _context.Set<AbuseIpTracking>().CountAsync(t => t.IsBlocked),
                HighRiskIps = await _context.Set<AbuseIpTracking>().CountAsync(t => t.RiskScore >= _captchaThresholdScore),
                TotalEventsToday = await _context.Set<AbuseEventLog>().CountAsync(l => l.CreatedAt >= todayStart),
                FailedLoginsToday = await _context.Set<AbuseEventLog>().CountAsync(l => l.CreatedAt >= todayStart && l.EventType == "FailedLogin"),
                FailedRegistrationsToday = await _context.Set<AbuseEventLog>().CountAsync(l => l.CreatedAt >= todayStart && l.EventType == "FailedRegistration"),
                CaptchaFailuresToday = await _context.Set<AbuseEventLog>().CountAsync(l => l.CreatedAt >= todayStart && l.EventType == "CaptchaFailure"),
                RateLimitViolationsToday = await _context.Set<AbuseEventLog>().CountAsync(l => l.CreatedAt >= todayStart && l.EventType == "RateLimitViolation"),
                WhitelistedIps = await _context.Set<IpAccessList>().CountAsync(x => x.ListType == "Whitelist" && x.IsActive),
                BlacklistedIps = await _context.Set<IpAccessList>().CountAsync(x => x.ListType == "Blacklist" && x.IsActive),
                TopOffenders = await _context.Set<AbuseIpTracking>()
                    .OrderByDescending(t => t.RiskScore)
                    .Take(10)
                    .Select(t => new TopOffenderInfo
                    {
                        IpAddress = t.IpAddress,
                        RiskScore = t.RiskScore,
                        TotalActivities = t.TotalSuspiciousActivities,
                        CountryCode = t.CountryCode,
                        IsBlocked = t.IsBlocked
                    })
                    .ToListAsync()
            };
            
            return stats;
        }

        // Private helpers
        
        private async Task<AbuseIpTracking> GetOrCreateTrackingAsync(string ipAddress)
        {
            var tracking = await _context.Set<AbuseIpTracking>()
                .FirstOrDefaultAsync(t => t.IpAddress == ipAddress);
            
            if (tracking == null)
            {
                tracking = new AbuseIpTracking
                {
                    IpAddress = ipAddress,
                    FirstSeenAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow
                };
                _context.Set<AbuseIpTracking>().Add(tracking);
            }
            
            // Reset daily counters if needed
            if (tracking.CountersResetAt.HasValue && 
                tracking.CountersResetAt.Value.Date < DateTime.UtcNow.Date)
            {
                tracking.FailedLoginAttempts = 0;
                tracking.FailedRegistrationAttempts = 0;
                tracking.CaptchaFailures = 0;
                tracking.RateLimitViolations = 0;
                tracking.CountersResetAt = DateTime.UtcNow;
            }
            
            return tracking;
        }
        
        private void UpdateRiskScore(AbuseIpTracking tracking)
        {
            // Calculate risk score based on various factors
            var score = 0;
            
            // Failed logins (10 points each, max 30)
            score += Math.Min(tracking.FailedLoginAttempts * 10, 30);
            
            // Failed registrations (15 points each, max 30)
            score += Math.Min(tracking.FailedRegistrationAttempts * 15, 30);
            
            // CAPTCHA failures (20 points each, max 40)
            score += Math.Min(tracking.CaptchaFailures * 20, 40);
            
            // Rate limit violations (5 points each, max 25)
            score += Math.Min(tracking.RateLimitViolations * 5, 25);
            
            // Total suspicious activities add small bonus
            score += Math.Min(tracking.TotalSuspiciousActivities, 10);
            
            tracking.RiskScore = Math.Min(100, score);
        }
        
        private async Task CheckAutoBlockAsync(AbuseIpTracking tracking)
        {
            if (tracking.IsBlocked) return;
            
            // Auto-block if thresholds exceeded
            var shouldBlock = 
                tracking.FailedLoginAttempts >= _maxFailedLogins ||
                tracking.FailedRegistrationAttempts >= _maxFailedRegistrations ||
                tracking.CaptchaFailures >= _maxCaptchaFailures ||
                tracking.RateLimitViolations >= _maxRateLimitViolations ||
                tracking.RiskScore >= _blockThresholdScore;
            
            if (shouldBlock)
            {
                tracking.IsBlocked = true;
                tracking.BlockedAt = DateTime.UtcNow;
                tracking.BlockExpiresAt = DateTime.UtcNow.AddHours(_autoBlockDurationHours);
                tracking.BlockReason = $"Auto-blocked: Risk score {tracking.RiskScore}, " +
                    $"Logins: {tracking.FailedLoginAttempts}, " +
                    $"Registrations: {tracking.FailedRegistrationAttempts}, " +
                    $"CAPTCHA: {tracking.CaptchaFailures}";
                
                _logger.LogWarning("IP {IpAddress} auto-blocked. {Reason}",
                    tracking.IpAddress, tracking.BlockReason);
            }
        }
        
        private async Task LogEventAsync(string ipAddress, string eventType, string severity, 
            string? email, string? userAgent, string? requestPath, string actionTaken)
        {
            var log = new AbuseEventLog
            {
                IpAddress = ipAddress,
                EventType = eventType,
                Severity = severity,
                Email = email?.Length > 256 ? email.Substring(0, 256) : email,
                UserAgent = userAgent?.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
                RequestPath = requestPath?.Length > 500 ? requestPath.Substring(0, 500) : requestPath,
                ActionTaken = actionTaken,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Set<AbuseEventLog>().Add(log);
        }
    }
}
