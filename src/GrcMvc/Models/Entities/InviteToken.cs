using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Invite token for controlled access to the platform.
/// Supports time-limited, single-use, and revocable invites.
/// </summary>
[Table("InviteTokens")]
public class InviteToken : BaseEntity
{
    /// <summary>
    /// Unique token string (URL-safe, cryptographically random)
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Type of invite: Personal, Team, Campaign
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InviteType { get; set; } = "Personal";

    /// <summary>
    /// Optional: Tenant this invite belongs to
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Optional: Specific email this invite is for
    /// </summary>
    [MaxLength(256)]
    public string? TargetEmail { get; set; }

    /// <summary>
    /// Optional: Allowed email domain (e.g., "company.com")
    /// </summary>
    [MaxLength(256)]
    public string? AllowedDomain { get; set; }

    /// <summary>
    /// Who created this invite
    /// </summary>
    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this token expires (null = never expires)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Maximum number of times this token can be used (null = unlimited)
    /// </summary>
    public int? MaxUses { get; set; }

    /// <summary>
    /// Current number of times this token has been used
    /// </summary>
    public int UsedCount { get; set; } = 0;

    /// <summary>
    /// When this token was revoked (null = not revoked)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Who revoked this token
    /// </summary>
    [MaxLength(256)]
    public string? RevokedBy { get; set; }

    /// <summary>
    /// Reason for revocation
    /// </summary>
    [MaxLength(500)]
    public string? RevocationReason { get; set; }

    /// <summary>
    /// Last time this token was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Optional metadata (JSON) for custom data
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Optional: Campaign or source tracking
    /// </summary>
    [MaxLength(100)]
    public string? CampaignSource { get; set; }

    /// <summary>
    /// Description for admin reference
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    // ============ Computed Properties ============

    /// <summary>
    /// Check if token is valid (not expired, not revoked, not exceeded max uses)
    /// </summary>
    [NotMapped]
    public bool IsValid => !IsExpired && !IsRevoked && !IsExhausted;

    /// <summary>
    /// Check if token has expired
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Check if token has been revoked
    /// </summary>
    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Check if token has reached max uses
    /// </summary>
    [NotMapped]
    public bool IsExhausted => MaxUses.HasValue && UsedCount >= MaxUses.Value;

    /// <summary>
    /// Get remaining uses (null if unlimited)
    /// </summary>
    [NotMapped]
    public int? RemainingUses => MaxUses.HasValue ? MaxUses.Value - UsedCount : null;

    /// <summary>
    /// Time until expiry (null if no expiry or already expired)
    /// </summary>
    [NotMapped]
    public TimeSpan? TimeUntilExpiry => ExpiresAt.HasValue && !IsExpired 
        ? ExpiresAt.Value - DateTime.UtcNow 
        : null;
}

/// <summary>
/// Invite token usage log for audit trail
/// </summary>
[Table("InviteTokenUsageLogs")]
public class InviteTokenUsageLog : BaseEntity
{
    /// <summary>
    /// Reference to the invite token
    /// </summary>
    public Guid InviteTokenId { get; set; }

    [ForeignKey(nameof(InviteTokenId))]
    public InviteToken? InviteToken { get; set; }

    /// <summary>
    /// Email used when redeeming
    /// </summary>
    [MaxLength(256)]
    public string? UsedByEmail { get; set; }

    /// <summary>
    /// IP address of the user
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Result of the usage attempt
    /// </summary>
    [MaxLength(50)]
    public string Result { get; set; } = "Success"; // Success, Expired, Revoked, Exhausted, InvalidDomain

    /// <summary>
    /// Additional details about the usage
    /// </summary>
    [MaxLength(500)]
    public string? Details { get; set; }
}

/// <summary>
/// Constants for invite token types
/// </summary>
public static class InviteTokenType
{
    public const string Personal = "Personal";
    public const string Team = "Team";
    public const string Campaign = "Campaign";
    public const string Trial = "Trial";
}

/// <summary>
/// Constants for invite token usage results
/// </summary>
public static class InviteTokenUsageResult
{
    public const string Success = "Success";
    public const string Expired = "Expired";
    public const string Revoked = "Revoked";
    public const string Exhausted = "Exhausted";
    public const string InvalidDomain = "InvalidDomain";
    public const string InvalidEmail = "InvalidEmail";
    public const string NotFound = "NotFound";
}
