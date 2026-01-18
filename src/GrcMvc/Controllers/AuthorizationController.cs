using System.Collections.Immutable;
using System.Security.Claims;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GrcMvc.Controllers;

/// <summary>
/// OpenIddict Authorization Controller
/// Handles OAuth2/OpenID Connect token requests at /connect/token
/// This is the recommended authentication endpoint for API clients
/// </summary>
public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GrcDbContext _context;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        GrcDbContext context,
        ILogger<AuthorizationController> logger)
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Token endpoint - handles OAuth2 token requests
    /// Supports: password grant, refresh_token grant, client_credentials grant
    /// </summary>
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            return await HandlePasswordGrantAsync(request);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenGrantAsync();
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsGrantAsync(request);
        }

        _logger.LogWarning("Unsupported grant type requested: {GrantType}", request.GrantType);
        return Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.UnsupportedGrantType,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified grant type is not supported."
            }));
    }

    /// <summary>
    /// Handle Resource Owner Password Credentials grant (password grant)
    /// </summary>
    private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username!)
            ?? await _userManager.FindByEmailAsync(request.Username!);

        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for {Username}", request.Username);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
                }));
        }

        // Validate password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed for {Username}: {Result}", request.Username, result);

            var errorDescription = result.IsLockedOut
                ? "The account is locked out. Please try again later."
                : result.IsNotAllowed
                    ? "The account is not allowed to sign in."
                    : "The username/password couple is invalid.";

            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = errorDescription
                }));
        }

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} authenticated successfully via OpenIddict", user.Email);

        // Create claims principal
        var principal = await CreateClaimsPrincipalAsync(user, request);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle Refresh Token grant
    /// </summary>
    private async Task<IActionResult> HandleRefreshTokenGrantAsync()
    {
        // Retrieve the claims principal stored in the refresh token
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                }));
        }

        // Get the user from the claims
        var userId = result.Principal.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                }));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user associated with the refresh token no longer exists."
                }));
        }

        // Ensure the user can still sign in
        if (!await _signInManager.CanSignInAsync(user))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }));
        }

        _logger.LogInformation("Refresh token used for user {Email}", user.Email);

        // Create new claims principal with fresh data
        var request = HttpContext.GetOpenIddictServerRequest()!;
        var principal = await CreateClaimsPrincipalAsync(user, request);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle Client Credentials grant (for service-to-service authentication)
    /// </summary>
    private async Task<IActionResult> HandleClientCredentialsGrantAsync(OpenIddictRequest request)
    {
        // For client credentials, create a principal with the client as the subject
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
            ?? throw new InvalidOperationException("The application details cannot be found.");

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add the client_id as the subject
        identity.AddClaim(Claims.Subject, request.ClientId!);
        identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application) ?? request.ClientId!);

        var principal = new ClaimsPrincipal(identity);

        principal.SetScopes(request.GetScopes());

        _logger.LogInformation("Client credentials authentication successful for {ClientId}", request.ClientId);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Create a ClaimsPrincipal for the authenticated user
    /// </summary>
    private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user, OpenIddictRequest request)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add standard claims
        identity.AddClaim(Claims.Subject, user.Id.ToString());
        identity.AddClaim(Claims.Email, user.Email ?? string.Empty);
        identity.AddClaim(Claims.Name, user.UserName ?? string.Empty);
        identity.AddClaim(Claims.PreferredUsername, user.UserName ?? string.Empty);

        // Add full name
        if (!string.IsNullOrEmpty(user.FullName))
        {
            identity.AddClaim("full_name", user.FullName);
        }

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(Claims.Role, role);
        }

        // Add tenant ID if user has tenant membership
        var tenantUser = await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .FirstOrDefaultAsync(tu => tu.UserId == user.Id.ToString() && !tu.IsDeleted);

        if (tenantUser != null)
        {
            identity.AddClaim("tenant_id", tenantUser.TenantId.ToString());
            identity.AddClaim("tenant_role", tenantUser.RoleCode ?? string.Empty);

            if (tenantUser.Tenant != null)
            {
                identity.AddClaim("tenant_name", tenantUser.Tenant.OrganizationName ?? string.Empty);
            }
        }

        var principal = new ClaimsPrincipal(identity);

        // Set the requested scopes
        principal.SetScopes(request.GetScopes());

        // Set destinations for claims (which tokens they appear in)
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        return principal;
    }

    /// <summary>
    /// Determine which tokens (access_token, id_token) a claim should be included in
    /// </summary>
    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name:
            case Claims.PreferredUsername:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;
                yield break;

            case "tenant_id":
            case "tenant_role":
            case "tenant_name":
            case "full_name":
                yield return Destinations.AccessToken;
                yield break;

            // Never include the security stamp in tokens
            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
