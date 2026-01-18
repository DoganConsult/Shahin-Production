using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks suspicious IP addresses and their activities for abuse detection.
    /// </summary>
    [Table("abuse_ip_tracking")]
    public class AbuseIpTracking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// The IP address being tracked
        /// </summary>
        [Required]
        [MaxLength(45)] // IPv6 max length
        public string IpAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of failed login attempts from this IP
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;
        
        /// <summary>
        /// Number of failed registration attempts from this IP
        /// </summary>
        public int FailedRegistrationAttempts { get; set; } = 0;
        
        /// <summary>
        /// Number of rate limit violations from this IP
        /// </summary>
        public int RateLimitViolations { get; set; } = 0;
        
        /// <summary>
        /// Number of CAPTCHA failures from this IP
        /// </summary>
        public int CaptchaFailures { get; set; } = 0;
        
        /// <summary>
        /// Total suspicious activities detected
        /// </summary>
        public int TotalSuspiciousActivities { get; set; } = 0;
        
        /// <summary>
        /// Whether this IP is currently blocked
        /// </summary>
        public bool IsBlocked { get; set; } = false;
        
        /// <summary>
        /// When the IP was blocked (if applicable)
        /// </summary>
        public DateTime? BlockedAt { get; set; }
        
        /// <summary>
        /// When the block expires (null = permanent)
        /// </summary>
        public DateTime? BlockExpiresAt { get; set; }
        
        /// <summary>
        /// Reason for blocking
        /// </summary>
        [MaxLength(500)]
        public string? BlockReason { get; set; }
        
        /// <summary>
        /// Country code from IP geolocation (if available)
        /// </summary>
        [MaxLength(10)]
        public string? CountryCode { get; set; }
        
        /// <summary>
        /// Risk score (0-100) calculated based on activity patterns
        /// </summary>
        public int RiskScore { get; set; } = 0;
        
        /// <summary>
        /// First activity detected from this IP
        /// </summary>
        public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Last activity from this IP
        /// </summary>
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When counters were last reset (daily reset for sliding window)
        /// </summary>
        public DateTime? CountersResetAt { get; set; }
    }
    
    /// <summary>
    /// Log of individual abuse events for audit trail
    /// </summary>
    [Table("abuse_event_logs")]
    public class AbuseEventLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// The IP address associated with this event
        /// </summary>
        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of abuse event
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty;
        
        /// <summary>
        /// Severity: Low, Medium, High, Critical
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "Low";
        
        /// <summary>
        /// Associated email (if applicable)
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }
        
        /// <summary>
        /// User agent string
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        /// <summary>
        /// Request path
        /// </summary>
        [MaxLength(500)]
        public string? RequestPath { get; set; }
        
        /// <summary>
        /// Additional details in JSON format
        /// </summary>
        public string? DetailsJson { get; set; }
        
        /// <summary>
        /// Action taken: None, Warning, Blocked, Captcha
        /// </summary>
        [MaxLength(50)]
        public string ActionTaken { get; set; } = "None";
        
        /// <summary>
        /// When this event occurred
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// IP whitelist/blacklist for explicit allow/deny
    /// </summary>
    [Table("ip_access_list")]
    public class IpAccessList
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// The IP address or CIDR range
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string IpAddressOrRange { get; set; } = string.Empty;
        
        /// <summary>
        /// Type: Whitelist or Blacklist
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ListType { get; set; } = "Blacklist";
        
        /// <summary>
        /// Reason for adding to list
        /// </summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
        
        /// <summary>
        /// When this entry expires (null = permanent)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// Who added this entry
        /// </summary>
        [MaxLength(256)]
        public string? AddedBy { get; set; }
        
        /// <summary>
        /// Whether this entry is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
    }
}
