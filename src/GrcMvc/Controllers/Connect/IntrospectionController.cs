using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Controllers.Connect;

/// <summary>
/// OpenID Connect Introspection Endpoint
/// Validates access tokens for resource servers
/// </summary>
[ApiController, Route("connect")]
public class IntrospectionController : ControllerBase
{
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly ILogger<IntrospectionController> _logger;
    private readonly IAuthenticationAuditService? _authAuditService;

    public IntrospectionController(
        IOpenIddictTokenManager tokenManager,
        ILogger<IntrospectionController> logger,
        IAuthenticationAuditService? authAuditService = null)
    {
        _tokenManager = tokenManager;
        _logger = logger;
        _authAuditService = authAuditService;
    }

    [HttpPost("introspect")]
    public async Task<IActionResult> Introspect()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(new { error = "invalid_request" });
        }

        // Try to validate the access token
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!result.Succeeded)
        {
            // Token is invalid - log the failed introspection
            _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
            {
                EventType = "TokenIntrospection",
                Success = false,
                IpAddress = clientIpAddress,
                Message = "Invalid token introspected",
                Severity = "Warning",
                Details = new Dictionary<string, object>
                {
                    ["ClientId"] = request.ClientId ?? "unknown"
                }
            });

            return Ok(new { active = false });
        }

        // Token is valid - log successful introspection
        var userId = result.Principal?.FindFirst(Claims.Subject)?.Value;

        _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
        {
            UserId = userId,
            EventType = "TokenIntrospection",
            Success = true,
            IpAddress = clientIpAddress,
            Message = "Token introspected successfully",
            Severity = "Info",
            Details = new Dictionary<string, object>
            {
                ["ClientId"] = request.ClientId ?? "unknown",
                ["UserId"] = userId ?? "unknown",
                ["Scopes"] = result.Principal?.FindFirst(Claims.Scope)?.Value ?? ""
            }
        });

        return Ok(new
        {
            active = true,
            scope = result.Principal?.FindFirst(Claims.Scope)?.Value,
            client_id = request.ClientId,
            username = result.Principal?.FindFirst(Claims.PreferredUsername)?.Value,
            sub = result.Principal?.FindFirst(Claims.Subject)?.Value,
            aud = result.Principal?.FindFirst(Claims.Audience)?.Value,
            iss = result.Principal?.FindFirst(Claims.Issuer)?.Value,
            iat = result.Principal?.FindFirst(Claims.IssuedAt)?.Value,
            exp = result.Principal?.FindFirst(Claims.ExpiresAt)?.Value,
            token_type = "Bearer"
        });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(new { error = "invalid_request" });
        }

        var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Try to authenticate the token
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        if (result.Succeeded && result.Principal != null)
        {
            // Get the token identifier from the principal
            var tokenId = result.Principal.FindFirst("oi_tok_id")?.Value;
            var userId = result.Principal.FindFirst(Claims.Subject)?.Value;

            if (!string.IsNullOrEmpty(tokenId))
            {
                // Find and revoke the token
                var token = await _tokenManager.FindByIdAsync(tokenId);
                if (token != null)
                {
                    await _tokenManager.TryRevokeAsync(token);

                    // Log token revocation
                    _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                    {
                        UserId = userId,
                        EventType = "TokenRevoked",
                        Success = true,
                        IpAddress = clientIpAddress,
                        Message = "Token revoked successfully",
                        Severity = "Info",
                        Details = new Dictionary<string, object>
                        {
                            ["ClientId"] = request.ClientId ?? "unknown",
                            ["TokenId"] = tokenId
                        }
                    });
                }
            }
        }

        // Always return success to avoid leaking information
        return Ok();
    }
}
