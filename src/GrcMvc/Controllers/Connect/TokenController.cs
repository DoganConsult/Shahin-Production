using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Claim = System.Security.Claims.Claim;
using ClaimsIdentity = System.Security.Claims.ClaimsIdentity;
using ClaimsPrincipal = System.Security.Claims.ClaimsPrincipal;

namespace GrcMvc.Controllers.Connect;

/// <summary>
/// OpenID Connect Token Endpoint
/// Handles OAuth2/OIDC token requests
/// </summary>
[ApiController, Route("connect")]
public class TokenController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<TokenController> _logger;
    private readonly IAuthenticationAuditService? _authAuditService;

    public TokenController(
        IOpenIddictApplicationManager applicationManager,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        GrcDbContext dbContext,
        ILogger<TokenController> logger,
        IAuthenticationAuditService? authAuditService = null)
    {
        _applicationManager = applicationManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _logger = logger;
        _authAuditService = authAuditService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(new { error = "invalid_request" });
        }

        if (request.IsAuthorizationCodeGrantType())
        {
            // Handle authorization code grant
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result.Succeeded && result.Principal != null)
            {
                var user = await _userManager.GetUserAsync(result.Principal);
                if (user != null)
                {
                    // Log token issuance
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                    _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        EventType = "TokenIssued",
                        Success = true,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        Message = "Access token issued via authorization code",
                        Severity = "Info",
                        Details = new Dictionary<string, object>
                        {
                            ["GrantType"] = request.GrantType ?? "authorization_code",
                            ["ClientId"] = request.ClientId ?? "unknown",
                            ["Scopes"] = request.Scope ?? ""
                        }
                    });

                    return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }
            }
        }

        if (request.IsRefreshTokenGrantType())
        {
            // Handle refresh token grant
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result.Succeeded && result.Principal != null)
            {
                var user = await _userManager.GetUserAsync(result.Principal);
                if (user != null && user.IsActive)
                {
                    // Log token refresh
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        EventType = "TokenRefreshed",
                        Success = true,
                        IpAddress = ipAddress,
                        Message = "Access token refreshed",
                        Severity = "Info"
                    });

                    return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }
            }
        }

        if (request.IsPasswordGrantType())
        {
            // Handle password grant (for legacy support)
            var email = request.GetParameter("username")?.ToString();
            var password = request.GetParameter("password")?.ToString();

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && user.IsActive)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        // Create claims principal
                        var identity = new ClaimsIdentity(
                            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                            nameType: OpenIddictConstants.Claims.Name,
                            roleType: OpenIddictConstants.Claims.Role);

                        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()));
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Email, user.Email));
                        }
                        identity.AddClaim(new Claim(OpenIddictConstants.Claims.EmailVerified, user.EmailConfirmed ? "true" : "false"));

                        // Add tenant info
                        var tenantUser = await _dbContext.TenantUsers
                            .Include(tu => tu.Tenant)
                            .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

                        if (tenantUser?.Tenant != null)
                        {
                            identity.AddClaim(new Claim("tenant_id", tenantUser.TenantId.ToString()));
                            identity.AddClaim(new Claim("tenant_slug", tenantUser.Tenant.TenantSlug));
                        }

                        // Add roles
                        var roles = await _userManager.GetRolesAsync(user);
                        foreach (var role in roles)
                        {
                            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));
                        }

                        // Log token issuance
                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                        {
                            UserId = user.Id,
                            Email = user.Email,
                            EventType = "TokenIssued",
                            Success = true,
                            IpAddress = ipAddress,
                            Message = "Access token issued via password grant",
                            Severity = "Info",
                            Details = new Dictionary<string, object>
                            {
                                ["GrantType"] = "password",
                                ["ClientId"] = request.ClientId ?? "unknown"
                            }
                        });

                        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    }
                }
            }
        }

        // Return error for unsupported grant types
        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserInfo()
    {
        var userId = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return Forbid();
        }

        // Get tenant information
        var tenantUser = await _dbContext.TenantUsers
            .Include(tu => tu.Tenant)
            .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

        var userInfo = new Dictionary<string, object>
        {
            [OpenIddictConstants.Claims.Subject] = user.Id.ToString(),
            [OpenIddictConstants.Claims.Name] = user.UserName ?? "",
            [OpenIddictConstants.Claims.PreferredUsername] = user.UserName ?? "",
            [OpenIddictConstants.Claims.Email] = user.Email ?? "",
            [OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed
        };

        // Add tenant claims if available
        if (tenantUser?.Tenant != null)
        {
            userInfo["tenant_id"] = tenantUser.TenantId.ToString();
            userInfo["tenant_slug"] = tenantUser.Tenant.TenantSlug;
            userInfo["tenant_name"] = tenantUser.Tenant.Name;
        }

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        userInfo[OpenIddictConstants.Claims.Role] = roles;

        // Add custom claims
        userInfo["first_name"] = user.FirstName ?? "";
        userInfo["last_name"] = user.LastName ?? "";
        userInfo["department"] = user.Department ?? "";

        return Ok(userInfo);
    }
}
