using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for managing invite tokens with expiry, revocation, and usage tracking
/// </summary>
public interface IInviteTokenService
{
    /// <summary>
    /// Generate a new invite token
    /// </summary>
    Task<InviteToken> CreateTokenAsync(CreateInviteTokenRequest request);

    /// <summary>
    /// Validate a token and return result
    /// </summary>
    Task<InviteTokenValidationResult> ValidateTokenAsync(string token, string? email = null);

    /// <summary>
    /// Redeem a token (increment usage count, log usage)
    /// </summary>
    Task<InviteTokenRedemptionResult> RedeemTokenAsync(string token, RedeemTokenRequest request);

    /// <summary>
    /// Revoke a single token
    /// </summary>
    Task<bool> RevokeTokenAsync(string token, string revokedBy, string? reason = null);

    /// <summary>
    /// Revoke all tokens for a tenant
    /// </summary>
    Task<int> RevokeAllTokensForTenantAsync(Guid tenantId, string revokedBy, string? reason = null);

    /// <summary>
    /// Get token by token string
    /// </summary>
    Task<InviteToken?> GetTokenAsync(string token);

    /// <summary>
    /// Get all tokens for a tenant
    /// </summary>
    Task<List<InviteToken>> GetTokensForTenantAsync(Guid tenantId, bool includeExpired = false);

    /// <summary>
    /// Get token usage logs
    /// </summary>
    Task<List<InviteTokenUsageLog>> GetTokenUsageLogsAsync(Guid tokenId);

    /// <summary>
    /// Clean up expired tokens (for background job)
    /// </summary>
    Task<int> CleanupExpiredTokensAsync(int daysOld = 30);
}

/// <summary>
/// Request to create a new invite token
/// </summary>
public class CreateInviteTokenRequest
{
    public string InviteType { get; set; } = InviteTokenType.Personal;
    public Guid? TenantId { get; set; }
    public string? TargetEmail { get; set; }
    public string? AllowedDomain { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int? ExpiryHours { get; set; } = 72; // Default 72 hours
    public int? MaxUses { get; set; } = 1; // Default single use
    public string? CampaignSource { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Result of token validation
/// </summary>
public class InviteTokenValidationResult
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // Valid, Expired, Revoked, Exhausted, InvalidDomain, NotFound
    public InviteToken? Token { get; set; }
    public string? Message { get; set; }
    public int? RemainingUses { get; set; }
    public TimeSpan? TimeUntilExpiry { get; set; }
}

/// <summary>
/// Request to redeem a token
/// </summary>
public class RedeemTokenRequest
{
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// Result of token redemption
/// </summary>
public class InviteTokenRedemptionResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public InviteToken? Token { get; set; }
    public string? RedirectUrl { get; set; }
}
