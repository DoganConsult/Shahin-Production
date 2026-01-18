namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for IP-based abuse detection and prevention.
    /// Tracks suspicious activities, manages IP blocking, and provides risk assessment.
    /// </summary>
    public interface IAbuseDetectionService
    {
        /// <summary>
        /// Check if an IP address is allowed to make requests
        /// </summary>
        /// <param name="ipAddress">The IP address to check</param>
        /// <returns>True if allowed, false if blocked</returns>
        Task<bool> IsIpAllowedAsync(string ipAddress);
        
        /// <summary>
        /// Check if an IP is suspicious (high risk but not blocked)
        /// </summary>
        Task<bool> IsIpSuspiciousAsync(string ipAddress);
        
        /// <summary>
        /// Get the current risk score for an IP (0-100)
        /// </summary>
        Task<int> GetIpRiskScoreAsync(string ipAddress);
        
        /// <summary>
        /// Record a failed login attempt
        /// </summary>
        Task RecordFailedLoginAsync(string ipAddress, string? email = null, string? userAgent = null);
        
        /// <summary>
        /// Record a failed registration attempt
        /// </summary>
        Task RecordFailedRegistrationAsync(string ipAddress, string? email = null, string? userAgent = null);
        
        /// <summary>
        /// Record a CAPTCHA failure
        /// </summary>
        Task RecordCaptchaFailureAsync(string ipAddress, string? userAgent = null);
        
        /// <summary>
        /// Record a rate limit violation
        /// </summary>
        Task RecordRateLimitViolationAsync(string ipAddress, string? requestPath = null);
        
        /// <summary>
        /// Record a successful authentication (reduces risk score)
        /// </summary>
        Task RecordSuccessfulAuthAsync(string ipAddress);
        
        /// <summary>
        /// Record a generic suspicious activity
        /// </summary>
        Task RecordSuspiciousActivityAsync(string ipAddress, string eventType, string severity, string? details = null);
        
        /// <summary>
        /// Block an IP address
        /// </summary>
        /// <param name="ipAddress">IP to block</param>
        /// <param name="reason">Reason for blocking</param>
        /// <param name="duration">How long to block (null = permanent)</param>
        Task BlockIpAsync(string ipAddress, string reason, TimeSpan? duration = null);
        
        /// <summary>
        /// Unblock an IP address
        /// </summary>
        Task UnblockIpAsync(string ipAddress);
        
        /// <summary>
        /// Add an IP to the whitelist (always allowed)
        /// </summary>
        Task WhitelistIpAsync(string ipAddressOrRange, string reason, string? addedBy = null);
        
        /// <summary>
        /// Add an IP to the blacklist (always blocked)
        /// </summary>
        Task BlacklistIpAsync(string ipAddressOrRange, string reason, TimeSpan? duration = null, string? addedBy = null);
        
        /// <summary>
        /// Determine what action to take for a request based on IP risk
        /// </summary>
        /// <returns>None, RequireCaptcha, Block, or Allow</returns>
        Task<AbuseAction> DetermineActionAsync(string ipAddress);
        
        /// <summary>
        /// Clean up old tracking data and expired blocks
        /// </summary>
        Task CleanupExpiredDataAsync(int daysToKeep = 30);
        
        /// <summary>
        /// Get abuse statistics for reporting
        /// </summary>
        Task<AbuseStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null);
    }
    
    /// <summary>
    /// Action to take based on abuse detection
    /// </summary>
    public enum AbuseAction
    {
        /// <summary>Allow the request</summary>
        Allow,
        
        /// <summary>Require CAPTCHA verification</summary>
        RequireCaptcha,
        
        /// <summary>Show warning but allow</summary>
        Warn,
        
        /// <summary>Block the request</summary>
        Block
    }
    
    /// <summary>
    /// Abuse detection statistics
    /// </summary>
    public class AbuseStatistics
    {
        public int TotalTrackedIps { get; set; }
        public int BlockedIps { get; set; }
        public int HighRiskIps { get; set; }
        public int TotalEventsToday { get; set; }
        public int FailedLoginsToday { get; set; }
        public int FailedRegistrationsToday { get; set; }
        public int CaptchaFailuresToday { get; set; }
        public int RateLimitViolationsToday { get; set; }
        public int WhitelistedIps { get; set; }
        public int BlacklistedIps { get; set; }
        public List<TopOffenderInfo> TopOffenders { get; set; } = new();
    }
    
    /// <summary>
    /// Information about top offending IPs
    /// </summary>
    public class TopOffenderInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public int TotalActivities { get; set; }
        public string? CountryCode { get; set; }
        public bool IsBlocked { get; set; }
    }
}
