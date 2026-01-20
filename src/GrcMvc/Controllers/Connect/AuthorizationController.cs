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
/// OpenID Connect Authorization Endpoint
/// Handles OAuth2/OIDC authorization requests
/// </summary>
[ApiController, Route("connect")]
public class AuthorizationController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IAuthenticationAuditService? _authAuditService;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        GrcDbContext dbContext,
        ILogger<AuthorizationController> logger,
        IAuthenticationAuditService? authAuditService = null)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _logger = logger;
        _authAuditService = authAuditService;
    }

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(new { error = "invalid_request" });
        }

        // Try to retrieve the user principal from the authentication cookie
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            // If the user is not authenticated, redirect to login page
            var returnUrl = Request.Path + Request.QueryString;
            return Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Inactive user attempted authorization: {UserId}", user.Id);
            return Forbid();
        }

        // Retrieve the application details from the database
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (application == null)
        {
            return BadRequest(new { error = "invalid_client" });
        }

        // Retrieve the permanent authorizations associated with the user and the calling application
        var authorizations = await _authorizationManager.FindAsync(
            subject: user.Id.ToString(),
            client: await _applicationManager.GetIdAsync(application) ?? "",
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var authorization = authorizations.FirstOrDefault();

        try
        {
            // Create the claims-based identity that will be used by OpenIddict to create tokens
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            // Add the subject claim
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()));

            // Add standard claims
            if (!string.IsNullOrEmpty(user.UserName))
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, user.UserName));
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.PreferredUsername, user.UserName));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Email, user.Email));
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.EmailVerified, user.EmailConfirmed ? "true" : "false"));
            }

            // Add tenant information
            var tenantUser = await _dbContext.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            if (tenantUser?.Tenant != null)
            {
                identity.AddClaim(new Claim("tenant_id", tenantUser.TenantId.ToString()));
                identity.AddClaim(new Claim("tenant_slug", tenantUser.Tenant.TenantSlug));
                identity.AddClaim(new Claim("tenant_name", tenantUser.Tenant.Name));
            }

            // Add roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));
            }

            // Add custom claims
            identity.AddClaim(new Claim("first_name", user.FirstName ?? ""));
            identity.AddClaim(new Claim("last_name", user.LastName ?? ""));
            identity.AddClaim(new Claim("department", user.Department ?? ""));

            // Create a new authorization or reuse the existing one
            var principal = new ClaimsPrincipal(identity);

            if (authorization == null)
            {
                authorization = await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: user.Id.ToString(),
                    client: await _applicationManager.GetIdAsync(application) ?? "",
                    type: OpenIddictConstants.AuthorizationTypes.Permanent,
                    scopes: request.GetScopes());
            }

            // Log authorization event
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
            {
                UserId = user.Id,
                Email = user.Email,
                EventType = "OAuthAuthorization",
                Success = true,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Message = $"OAuth authorization granted for client {request.ClientId}",
                Severity = "Info",
                Details = new Dictionary<string, object>
                {
                    ["ClientId"] = request.ClientId ?? "unknown",
                    ["Scopes"] = string.Join(" ", request.GetScopes()),
                    ["TenantId"] = tenantUser?.TenantId.ToString() ?? "N/A"
                }
            });

            // Set the list of scopes granted to the client
            principal.SetScopes(request.GetScopes());

            // Set authorization id
            var authorizationId = await _authorizationManager.GetIdAsync(authorization);
            if (!string.IsNullOrEmpty(authorizationId))
            {
                principal.SetAuthorizationId(authorizationId);
            }

            // Sign the OpenIddict token
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authorization for user {UserId}", user.Id);
            return Forbid();
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        // Retrieve the user principal from the authentication cookie
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result.Succeeded)
        {
            var user = await _userManager.GetUserAsync(result.Principal);
            if (user != null)
            {
                // Log logout event
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                _ = _authAuditService?.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    EventType = "OAuthLogout",
                    Success = true,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Message = "User logged out via OpenID Connect",
                    Severity = "Info"
                });

                // Sign out the user
                await _signInManager.SignOutAsync();
            }
        }

        // Redirect to the post-logout redirect URI
        var redirectUri = request?.PostLogoutRedirectUri ?? "/";
        return Redirect(redirectUri);
    }
}
