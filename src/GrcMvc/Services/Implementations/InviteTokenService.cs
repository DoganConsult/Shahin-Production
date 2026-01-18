using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for managing invite tokens with expiry, revocation, and usage tracking
/// </summary>
public class InviteTokenService : IInviteTokenService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<InviteTokenService> _logger;

    public InviteTokenService(GrcDbContext context, ILogger<InviteTokenService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<InviteToken> CreateTokenAsync(CreateInviteTokenRequest request)
    {
        var token = new InviteToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateSecureToken(),
            InviteType = request.InviteType,
            TenantId = request.TenantId,
            TargetEmail = request.TargetEmail?.ToLowerInvariant(),
            AllowedDomain = request.AllowedDomain?.ToLowerInvariant(),
            CreatedBy = request.CreatedBy,
            ExpiresAt = request.ExpiryHours.HasValue 
                ? DateTime.UtcNow.AddHours(request.ExpiryHours.Value) 
                : null,
            MaxUses = request.MaxUses,
            CampaignSource = request.CampaignSource,
            Description = request.Description,
            MetadataJson = request.Metadata != null 
                ? JsonSerializer.Serialize(request.Metadata) 
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<InviteToken>().Add(token);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created invite token {TokenId} for {Email}/{Domain}, expires at {ExpiresAt}, max uses {MaxUses}",
            token.Id, request.TargetEmail ?? "any", request.AllowedDomain ?? "any", 
            token.ExpiresAt, token.MaxUses);

        return token;
    }

    /// <inheritdoc />
    public async Task<InviteTokenValidationResult> ValidateTokenAsync(string token, string? email = null)
    {
        var inviteToken = await GetTokenAsync(token);

        if (inviteToken == null)
        {
            return new InviteTokenValidationResult
            {
                IsValid = false,
                Status = InviteTokenUsageResult.NotFound,
                Message = "Invalid or unknown invite token"
            };
        }

        if (inviteToken.IsRevoked)
        {
            return new InviteTokenValidationResult
            {
                IsValid = false,
                Status = InviteTokenUsageResult.Revoked,
                Token = inviteToken,
                Message = "This invite has been revoked"
            };
        }

        if (inviteToken.IsExpired)
        {
            return new InviteTokenValidationResult
            {
                IsValid = false,
                Status = InviteTokenUsageResult.Expired,
                Token = inviteToken,
                Message = "This invite has expired"
            };
        }

        if (inviteToken.IsExhausted)
        {
            return new InviteTokenValidationResult
            {
                IsValid = false,
                Status = InviteTokenUsageResult.Exhausted,
                Token = inviteToken,
                Message = "This invite has reached its maximum uses"
            };
        }

        // Check email restrictions
        if (!string.IsNullOrEmpty(email))
        {
            // Check specific target email
            if (!string.IsNullOrEmpty(inviteToken.TargetEmail) && 
                !inviteToken.TargetEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                return new InviteTokenValidationResult
                {
                    IsValid = false,
                    Status = InviteTokenUsageResult.InvalidEmail,
                    Token = inviteToken,
                    Message = "This invite is for a different email address"
                };
            }

            // Check allowed domain
            if (!string.IsNullOrEmpty(inviteToken.AllowedDomain))
            {
                var emailDomain = email.Split('@').LastOrDefault()?.ToLowerInvariant();
                if (emailDomain != inviteToken.AllowedDomain.ToLowerInvariant())
                {
                    return new InviteTokenValidationResult
                    {
                        IsValid = false,
                        Status = InviteTokenUsageResult.InvalidDomain,
                        Token = inviteToken,
                        Message = $"This invite is only valid for @{inviteToken.AllowedDomain} emails"
                    };
                }
            }
        }

        return new InviteTokenValidationResult
        {
            IsValid = true,
            Status = "Valid",
            Token = inviteToken,
            RemainingUses = inviteToken.RemainingUses,
            TimeUntilExpiry = inviteToken.TimeUntilExpiry
        };
    }

    /// <inheritdoc />
    public async Task<InviteTokenRedemptionResult> RedeemTokenAsync(string token, RedeemTokenRequest request)
    {
        var validation = await ValidateTokenAsync(token, request.Email);

        if (!validation.IsValid)
        {
            // Log failed attempt
            if (validation.Token != null)
            {
                await LogUsageAsync(validation.Token.Id, request, validation.Status);
            }

            return new InviteTokenRedemptionResult
            {
                Success = false,
                Status = validation.Status,
                Message = validation.Message
            };
        }

        var inviteToken = validation.Token!;

        // Increment usage count
        inviteToken.UsedCount++;
        inviteToken.LastUsedAt = DateTime.UtcNow;
        inviteToken.UpdatedAt = DateTime.UtcNow;

        // Log successful usage
        await LogUsageAsync(inviteToken.Id, request, InviteTokenUsageResult.Success);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Token {TokenId} redeemed by {Email}, usage count: {UsedCount}/{MaxUses}",
            inviteToken.Id, request.Email, inviteToken.UsedCount, inviteToken.MaxUses ?? -1);

        // Determine redirect URL
        var redirectUrl = inviteToken.TenantId.HasValue
            ? $"/OnboardingWizard?tenantId={inviteToken.TenantId}"
            : "/Account/Register";

        return new InviteTokenRedemptionResult
        {
            Success = true,
            Status = InviteTokenUsageResult.Success,
            Token = inviteToken,
            RedirectUrl = redirectUrl,
            Message = "Invite accepted successfully"
        };
    }

    /// <inheritdoc />
    public async Task<bool> RevokeTokenAsync(string token, string revokedBy, string? reason = null)
    {
        var inviteToken = await GetTokenAsync(token);
        if (inviteToken == null) return false;

        inviteToken.RevokedAt = DateTime.UtcNow;
        inviteToken.RevokedBy = revokedBy;
        inviteToken.RevocationReason = reason;
        inviteToken.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Token {TokenId} revoked by {RevokedBy}: {Reason}",
            inviteToken.Id, revokedBy, reason ?? "No reason provided");

        return true;
    }

    /// <inheritdoc />
    public async Task<int> RevokeAllTokensForTenantAsync(Guid tenantId, string revokedBy, string? reason = null)
    {
        var tokens = await _context.Set<InviteToken>()
            .Where(t => t.TenantId == tenantId && t.RevokedAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.RevokedBy = revokedBy;
            token.RevocationReason = reason ?? "Bulk revocation";
            token.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Revoked {Count} tokens for tenant {TenantId} by {RevokedBy}",
            tokens.Count, tenantId, revokedBy);

        return tokens.Count;
    }

    /// <inheritdoc />
    public async Task<InviteToken?> GetTokenAsync(string token)
    {
        return await _context.Set<InviteToken>()
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    /// <inheritdoc />
    public async Task<List<InviteToken>> GetTokensForTenantAsync(Guid tenantId, bool includeExpired = false)
    {
        var query = _context.Set<InviteToken>()
            .Where(t => t.TenantId == tenantId);

        if (!includeExpired)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.ExpiresAt == null || t.ExpiresAt > now);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<InviteTokenUsageLog>> GetTokenUsageLogsAsync(Guid tokenId)
    {
        return await _context.Set<InviteTokenUsageLog>()
            .Where(l => l.InviteTokenId == tokenId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredTokensAsync(int daysOld = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);

        var expiredTokens = await _context.Set<InviteToken>()
            .Where(t => t.ExpiresAt < cutoff || (t.RevokedAt.HasValue && t.RevokedAt < cutoff))
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.Set<InviteToken>().RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired/revoked tokens older than {Days} days",
                expiredTokens.Count, daysOld);
        }

        return expiredTokens.Count;
    }

    // ============ Private Helpers ============

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private async Task LogUsageAsync(Guid tokenId, RedeemTokenRequest request, string result)
    {
        var log = new InviteTokenUsageLog
        {
            Id = Guid.NewGuid(),
            InviteTokenId = tokenId,
            UsedByEmail = request.Email,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent?.Length > 500 
                ? request.UserAgent.Substring(0, 500) 
                : request.UserAgent,
            Result = result,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<InviteTokenUsageLog>().Add(log);
        // SaveChanges is called by the caller
    }
}
