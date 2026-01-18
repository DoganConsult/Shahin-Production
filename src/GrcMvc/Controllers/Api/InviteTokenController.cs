using System;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for managing invite tokens
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InviteTokenController : ControllerBase
{
    private readonly IInviteTokenService _tokenService;
    private readonly ILogger<InviteTokenController> _logger;

    public InviteTokenController(
        IInviteTokenService tokenService,
        ILogger<InviteTokenController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new invite token (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin")]
    public async Task<IActionResult> CreateToken([FromBody] CreateInviteTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.CreatedBy))
        {
            request.CreatedBy = User.Identity?.Name ?? "System";
        }

        var token = await _tokenService.CreateTokenAsync(request);

        return Ok(new
        {
            success = true,
            token = token.Token,
            inviteUrl = $"{Request.Scheme}://{Request.Host}/invite/{token.Token}",
            expiresAt = token.ExpiresAt,
            maxUses = token.MaxUses
        });
    }

    /// <summary>
    /// Validate a token (Public - used by invite landing page)
    /// </summary>
    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken(string token, [FromQuery] string? email = null)
    {
        var result = await _tokenService.ValidateTokenAsync(token, email);

        return Ok(new
        {
            isValid = result.IsValid,
            status = result.Status,
            message = result.Message,
            remainingUses = result.RemainingUses,
            timeUntilExpiry = result.TimeUntilExpiry?.TotalHours
        });
    }

    /// <summary>
    /// Redeem a token (Public - used during registration)
    /// </summary>
    [HttpPost("redeem/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> RedeemToken(string token, [FromBody] RedeemTokenApiRequest request)
    {
        var redeemRequest = new RedeemTokenRequest
        {
            Email = request.Email,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString()
        };

        var result = await _tokenService.RedeemTokenAsync(token, redeemRequest);

        if (!result.Success)
        {
            return BadRequest(new
            {
                success = false,
                status = result.Status,
                message = result.Message
            });
        }

        return Ok(new
        {
            success = true,
            redirectUrl = result.RedirectUrl,
            message = result.Message
        });
    }

    /// <summary>
    /// Revoke a single token (Admin only)
    /// </summary>
    [HttpPost("revoke/{token}")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin")]
    public async Task<IActionResult> RevokeToken(string token, [FromBody] RevokeTokenRequest? request = null)
    {
        var revokedBy = User.Identity?.Name ?? "System";
        var success = await _tokenService.RevokeTokenAsync(token, revokedBy, request?.Reason);

        if (!success)
        {
            return NotFound(new { success = false, message = "Token not found" });
        }

        return Ok(new { success = true, message = "Token revoked successfully" });
    }

    /// <summary>
    /// Revoke all tokens for a tenant (Admin only)
    /// </summary>
    [HttpPost("revoke-all/{tenantId:guid}")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin")]
    public async Task<IActionResult> RevokeAllForTenant(Guid tenantId, [FromBody] RevokeTokenRequest? request = null)
    {
        var revokedBy = User.Identity?.Name ?? "System";
        var count = await _tokenService.RevokeAllTokensForTenantAsync(tenantId, revokedBy, request?.Reason);

        return Ok(new
        {
            success = true,
            revokedCount = count,
            message = $"Revoked {count} tokens for tenant {tenantId}"
        });
    }

    /// <summary>
    /// Get all tokens for a tenant (Admin only)
    /// </summary>
    [HttpGet("tenant/{tenantId:guid}")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin")]
    public async Task<IActionResult> GetTokensForTenant(Guid tenantId, [FromQuery] bool includeExpired = false)
    {
        var tokens = await _tokenService.GetTokensForTenantAsync(tenantId, includeExpired);

        return Ok(new
        {
            success = true,
            count = tokens.Count,
            tokens = tokens.Select(t => new
            {
                t.Id,
                t.Token,
                t.InviteType,
                t.TargetEmail,
                t.AllowedDomain,
                t.CreatedBy,
                t.CreatedAt,
                t.ExpiresAt,
                t.MaxUses,
                t.UsedCount,
                t.IsValid,
                t.IsExpired,
                t.IsRevoked,
                t.IsExhausted,
                t.RemainingUses,
                inviteUrl = $"{Request.Scheme}://{Request.Host}/invite/{t.Token}"
            })
        });
    }

    /// <summary>
    /// Get token usage logs (Admin only)
    /// </summary>
    [HttpGet("{tokenId:guid}/logs")]
    [Authorize(Roles = "PlatformAdmin,TenantAdmin")]
    public async Task<IActionResult> GetTokenUsageLogs(Guid tokenId)
    {
        var logs = await _tokenService.GetTokenUsageLogsAsync(tokenId);

        return Ok(new
        {
            success = true,
            count = logs.Count,
            logs = logs.Select(l => new
            {
                l.Id,
                l.UsedByEmail,
                l.IpAddress,
                l.Result,
                l.Details,
                l.CreatedAt
            })
        });
    }
}

/// <summary>
/// Request to redeem a token via API
/// </summary>
public class RedeemTokenApiRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request to revoke a token
/// </summary>
public class RevokeTokenRequest
{
    public string? Reason { get; set; }
}
