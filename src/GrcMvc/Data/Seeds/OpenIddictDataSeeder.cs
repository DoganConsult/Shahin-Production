using OpenIddict.Abstractions;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds OpenIddict applications and scopes for OAuth2/OpenID Connect authentication
/// </summary>
public class OpenIddictDataSeeder
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenIddictDataSeeder> _logger;

    public OpenIddictDataSeeder(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IConfiguration configuration,
        ILogger<OpenIddictDataSeeder> logger)
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Seed OpenIddict applications and scopes
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding OpenIddict data...");

        await SeedScopesAsync();
        await SeedApplicationsAsync();

        _logger.LogInformation("OpenIddict data seeding completed");
    }

    /// <summary>
    /// Seed OAuth2 scopes
    /// </summary>
    private async Task SeedScopesAsync()
    {
        // Seed standard scopes
        var scopes = new[]
        {
            new { Name = OpenIddictConstants.Scopes.OpenId, DisplayName = "OpenID", Resources = Array.Empty<string>() },
            new { Name = OpenIddictConstants.Scopes.Profile, DisplayName = "Profile information", Resources = Array.Empty<string>() },
            new { Name = OpenIddictConstants.Scopes.Email, DisplayName = "Email address", Resources = Array.Empty<string>() },
            new { Name = OpenIddictConstants.Scopes.Roles, DisplayName = "User roles", Resources = Array.Empty<string>() },
            new { Name = "GrcMvc", DisplayName = "GRC Platform API", Resources = new[] { "GrcMvc" } }
        };

        foreach (var scope in scopes)
        {
            if (await _scopeManager.FindByNameAsync(scope.Name) == null)
            {
                await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = scope.Name,
                    DisplayName = scope.DisplayName,
                    Resources = { scope.Resources.Length > 0 ? scope.Resources[0] : string.Empty }
                });
                _logger.LogInformation("Created OpenIddict scope: {ScopeName}", scope.Name);
            }
        }
    }

    /// <summary>
    /// Seed OAuth2 client applications
    /// </summary>
    private async Task SeedApplicationsAsync()
    {
        // 1. GrcMvc API - Password grant for direct API access
        await CreateApplicationIfNotExistsAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "GrcMvc",
            DisplayName = "Shahin GRC Platform API",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                // Grant types
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                // Endpoints
                OpenIddictConstants.Permissions.Endpoints.Token,

                // Scopes
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + "GrcMvc"
            }
        });

        // 2. Swagger UI - For API documentation
        var swaggerRedirectUri = _configuration["AuthServer:SwaggerRedirectUri"]
            ?? "https://localhost:5001/swagger/oauth2-redirect.html";

        await CreateApplicationIfNotExistsAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "Grc_Swagger",
            ClientSecret = _configuration["AuthServer:SwaggerClientSecret"] ?? "swagger-secret-key-development",
            DisplayName = "GRC Swagger Documentation",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            RedirectUris = { new Uri(swaggerRedirectUri) },
            Permissions =
            {
                // Grant types
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                // Endpoints
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                // Response types
                OpenIddictConstants.Permissions.ResponseTypes.Code,

                // Scopes
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + "GrcMvc"
            }
        });

        // 3. GRC Frontend (Next.js) - For SPA authentication
        var frontendUrl = _configuration["App:ClientUrl"] ?? "http://localhost:3000";

        await CreateApplicationIfNotExistsAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "Grc_Frontend",
            DisplayName = "GRC Frontend Application",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            RedirectUris = { new Uri($"{frontendUrl}/api/auth/callback") },
            PostLogoutRedirectUris = { new Uri($"{frontendUrl}/") },
            Permissions =
            {
                // Grant types
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                // Endpoints
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Logout,

                // Response types
                OpenIddictConstants.Permissions.ResponseTypes.Code,

                // Scopes
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + "GrcMvc"
            }
        });

        // 4. Service-to-Service client (for backend integrations)
        await CreateApplicationIfNotExistsAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "Grc_Service",
            ClientSecret = _configuration["AuthServer:ServiceClientSecret"] ?? "service-secret-key-change-in-production",
            DisplayName = "GRC Service Client",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            Permissions =
            {
                // Grant types
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,

                // Endpoints
                OpenIddictConstants.Permissions.Endpoints.Token,

                // Scopes
                OpenIddictConstants.Permissions.Prefixes.Scope + "GrcMvc"
            }
        });
    }

    /// <summary>
    /// Create an OpenIddict application if it doesn't exist
    /// </summary>
    private async Task CreateApplicationIfNotExistsAsync(OpenIddictApplicationDescriptor descriptor)
    {
        if (await _applicationManager.FindByClientIdAsync(descriptor.ClientId!) == null)
        {
            await _applicationManager.CreateAsync(descriptor);
            _logger.LogInformation("Created OpenIddict application: {ClientId}", descriptor.ClientId);
        }
        else
        {
            _logger.LogDebug("OpenIddict application already exists: {ClientId}", descriptor.ClientId);
        }
    }
}
