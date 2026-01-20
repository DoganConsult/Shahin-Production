# 100% ABP OpenIddict Compliance Migration Plan

## Executive Summary

**Current State**: Hybrid OpenIddict (SSO) + JWT Bearer (API) authentication  
**Target State**: 100% ABP OpenIddict for both SSO and API authentication  
**Migration Complexity**: Medium (requires removing JWT, updating OpenIddict configuration)  
**Risk Level**: Low (OpenIddict already integrated, just needs proper ABP configuration)

---

## Current State Analysis

### ✅ What's Already Working (OpenIddict)

1. **ABP Modules Registered** (`GrcMvcAbpModule.cs`)
   - `AbpOpenIddictDomainModule` ✅
   - `AbpOpenIddictAspNetCoreModule` ✅
   - `AbpOpenIddictEntityFrameworkCoreModule` ✅

2. **Database Configuration** (`GrcDbContext.cs`)
   - `modelBuilder.ConfigureOpenIddict()` ✅

3. **OpenIddict Server Endpoints** (`GrcMvcAbpModule.cs` lines 176-214)
   - `/connect/authorize` ✅
   - `/connect/token` ✅
   - `/connect/userinfo` ✅
   - `/connect/logout` ✅
   - `/connect/introspect` ✅
   - `/connect/revocat` ✅

4. **Controllers Implemented**
   - `AuthorizationController.cs` - Handles authorization flow ✅
   - `TokenController.cs` - Handles token issuance ✅
   - `IntrospectionController.cs` - Handles token introspection ✅

5. **Grant Types Supported**
   - Authorization Code Flow ✅
   - Refresh Token Flow ✅
   - Client Credentials Flow ✅
   - Password Flow (legacy) ✅

6. **OpenIddict Validation Configured** (`GrcMvcAbpModule.cs` lines 210-214)
   - `UseLocalServer()` ✅
   - `UseAspNetCore()` ✅
   - **BUT**: Not set as default authentication scheme for APIs ❌

### ❌ What Needs to Be Fixed/Removed

1. **OpenIddict Configuration Method** (`GrcMvcAbpModule.cs` line 177)
   - **Current**: Uses direct `context.Services.AddOpenIddict()` 
   - **Issue**: Should use ABP's `AddAbpOpenIddict()` OR keep direct but ensure proper ABP integration
   - **Decision**: Keep direct `AddOpenIddict()` (it's already working) but ensure validation is properly configured

2. **OpenIddict Validation Not Default for APIs** (`Program.cs` lines 705-726)
   - **Current**: Default authentication scheme is `IdentityConstants.ApplicationScheme` (cookie)
   - **Issue**: API endpoints need OpenIddict validation as default, not JWT Bearer
   - **Fix**: Configure OpenIddict validation scheme and set it as default for API routes

3. **JWT Bearer Authentication** (`Program.cs` lines 694-726)
   - **Remove**: Entire JWT Bearer configuration
   - **Lines**: 694-726

4. **JWT Token Generation** (`AccountController.cs`)
   - **Remove**: `/api/account/token` endpoint that generates JWT tokens

5. **JWT Settings Configuration**
   - `Configuration/JwtSettings.cs` (entire file)
   - `Configuration/ConfigurationValidators.cs` (JwtSettingsValidator class)
   - `appsettings.json` (JwtSettings section)
   - `appsettings.Production.json` (JwtSettings section)
   - `Program.cs` lines 191-209, 515-516, 537 (JWT configuration)

6. **NuGet Package** (`GrcMvc.csproj`)
   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.8" />
   ```

7. **JWT Validation Code** (`AuthenticationService.Identity.cs`)
   - JWT token validation logic
   - `JwtSecurityTokenHandler` usage

8. **Swagger JWT Configuration** (`Program.cs` lines 443-467)
   - Remove JWT Bearer security definition
   - Add OpenIddict security definition

---

## Migration Steps

### Step 1: Fix Duplicate OpenIddict Validation Configuration

**File**: `GrcMvcAbpModule.cs`

**Issue**: Validation is configured twice:
- Once in `PreConfigureServices` (lines 54-60)
- Once in `ConfigureServices` (lines 210-214)

**Current Code**:

**PreConfigureServices** (lines 54-60):
```csharp
PreConfigure<OpenIddictBuilder>(builder =>
{
    builder.AddValidation(options =>
    {
        options.AddAudiences("GrcMvc");
        options.UseLocalServer();
        options.UseAspNetCore();
    });
});
```

**ConfigureServices** (lines 210-214):
```csharp
.AddValidation(options =>
{
    options.UseLocalServer();
    options.UseAspNetCore();
});
```

**Fix**: Remove duplicate validation from `ConfigureServices` and enhance `PreConfigureServices`:

**Updated PreConfigureServices**:
```csharp
public override void PreConfigureServices(ServiceConfigurationContext context)
{
    // Pre-configure OpenIddict validation (for API authentication)
    PreConfigure<OpenIddictBuilder>(builder =>
    {
        builder.AddValidation(options =>
        {
            // Set issuer (must match what server issues)
            var selfUrl = context.Configuration["App:SelfUrl"] ?? "https://grcsystem.com";
            options.SetIssuer(selfUrl);
            
            // Add audiences that API will accept
            options.AddAudiences("grc-api", "GrcMvc");
            
            // Use local server (same instance)
            options.UseLocalServer();
            
            // Enable ASP.NET Core integration
            options.UseAspNetCore();
            
            // Enable token entry validation (validates tokens from database)
            options.EnableTokenEntryValidation();
        });
    });
}
```

**Remove from ConfigureServices** (lines 210-214):
```csharp
// Remove this duplicate validation configuration:
// .AddValidation(options => { ... })
```

**Why**: 
- Single source of truth for validation configuration
- `SetIssuer()` ensures tokens are validated against the correct issuer
- `AddAudiences()` ensures tokens with these audiences are accepted
- `EnableTokenEntryValidation()` validates tokens stored in the database (required for OpenIddict)

---

### Step 2: Configure Authentication Schemes in Program.cs

**File**: `Program.cs`

**Current Code** (lines 705-726):
```csharp
builder.Services.AddAuthentication(options =>
{
    // Use cookie authentication as default for MVC web app
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options => { ... });
```

**Replace With**:
```csharp
builder.Services.AddAuthentication(options =>
{
    // Default scheme for MVC (cookie-based)
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    
    // OpenIddict validation is automatically registered by ABP
    // It uses scheme: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
    // API controllers can use: [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    // OR we can set it as default for /api/* routes (see Step 3)
});
// Remove .AddJwtBearer() completely
```

**Why**: 
- Keep cookie authentication for MVC pages
- OpenIddict validation is already registered by ABP module
- API controllers need to explicitly use OpenIddict validation scheme OR we configure route-based authentication

---

### Step 3: Configure Route-Based Authentication (API Routes Use OpenIddict)

**File**: `Program.cs` (after `app.UseAuthentication()`)

**Add**:
```csharp
// Configure route-based authentication: /api/* uses OpenIddict, everything else uses cookies
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        // For API routes, use OpenIddict validation as default
        context.Request.Headers["X-Authentication-Scheme"] = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    }
    await next();
});
```

**OR Better Approach**: Use policy-based authentication in controllers:

**File**: `Program.cs` (in `AddAuthorization` section, around line 729)

**Add**:
```csharp
builder.Services.AddAuthorization(options =>
{
    // ... existing policies ...
    
    // Default policy for API routes: use OpenIddict validation
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(
            OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
            IdentityConstants.ApplicationScheme) // Fallback to cookie for backward compatibility
        .RequireAuthenticatedUser()
        .Build();
});
```

**Why**: This ensures API endpoints automatically use OpenIddict validation when `[Authorize]` is used.

---

### Step 4: Remove JWT Bearer Authentication

**File**: `Program.cs`

**Remove** (lines 694-726):
```csharp
// Configure JWT Authentication (for API endpoints)
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings == null || !jwtSettings.IsValid())
{
    throw new InvalidOperationException(
        "JWT settings are invalid or missing. " +
        "Please set JwtSettings__Secret (min 32 chars) via environment variable or User Secrets.");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    // Use cookie authentication as default for MVC web app
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1) // Allow 1 minute clock skew
    };
});
```

**Replace With** (keep authentication configuration but remove JWT):
```csharp
// Configure Authentication (OpenIddict validation is configured in GrcMvcAbpModule)
builder.Services.AddAuthentication(options =>
{
    // Default scheme for MVC (cookie-based)
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    
    // OpenIddict validation is registered by ABP module
    // API controllers use: [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
});
```

**Why**: OpenIddict validation (configured in ABP module) will handle API authentication automatically.

---

### Step 5: Update Swagger Configuration for OpenIddict

**File**: `Program.cs` (around lines 443-467)

**Current Code** (JWT Bearer):
```csharp
// JWT Bearer authentication in Swagger
options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
{
    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
    Name = "Authorization",
    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
});
options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
{
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
});
```

**Replace With** (OpenIddict OAuth2):
```csharp
// OpenIddict OAuth2 authentication in Swagger
options.AddSecurityDefinition("OpenIddict", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
{
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
    Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
    {
        AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri("/connect/authorize", UriKind.Relative),
            TokenUrl = new Uri("/connect/token", UriKind.Relative),
            Scopes = new Dictionary<string, string>
            {
                { "openid", "OpenID Connect" },
                { "profile", "User profile" },
                { "email", "User email" },
                { "roles", "User roles" },
                { "offline_access", "Refresh token" }
            }
        },
        Password = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
        {
            TokenUrl = new Uri("/connect/token", UriKind.Relative),
            Scopes = new Dictionary<string, string>
            {
                { "openid", "OpenID Connect" },
                { "profile", "User profile" },
                { "email", "User email" },
                { "roles", "User roles" }
            }
        },
        ClientCredentials = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
        {
            TokenUrl = new Uri("/connect/token", UriKind.Relative),
            Scopes = new Dictionary<string, string>
            {
                { "openid", "OpenID Connect" }
            }
        }
    }
});
options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
{
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "OpenIddict"
            }
        },
        new[] { "openid", "profile", "email", "roles" }
    }
});
```

**Why**: Swagger UI will now use OpenIddict OAuth2 flows instead of JWT Bearer.

---

### Step 6: Remove JWT Token Generation Endpoint

**File**: `Controllers/AccountController.cs`

**Find and Remove** (search for `GenerateToken` method):
```csharp
[HttpPost]
[AllowAnonymous]
[Route("api/account/token")]
public async Task<IActionResult> GenerateToken([FromBody] LoginViewModel model)
{
    // ... JWT token generation logic using JwtSecurityTokenHandler ...
    // ... Remove entire method ...
}
```

**Replace With** (add comment):
```csharp
// JWT token generation removed - use OpenIddict token endpoint instead
// POST /connect/token
// Content-Type: application/x-www-form-urlencoded
// grant_type=password&username=user@example.com&password=password&client_id=grc-api
```

**Why**: OpenIddict's `/connect/token` endpoint handles all token generation (authorization code, refresh token, password, client credentials).

---

### Step 7: Update API Controllers to Use OpenIddict

**Current**: API controllers use `[Authorize]` which defaults to JWT Bearer  
**Target**: API controllers use `[Authorize]` which uses OpenIddict validation

**Action**: 
1. **No code changes needed for most controllers!** ✅
2. **For explicit OpenIddict validation**, update `[Authorize]` attributes:
   ```csharp
   // Option 1: Use default (after Step 3, default policy uses OpenIddict)
   [Authorize]
   
   // Option 2: Explicitly specify OpenIddict validation scheme
   [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
   ```

**Why**: Once JWT Bearer is removed and OpenIddict validation is configured as default (Step 3), `[Authorize]` will automatically use OpenIddict tokens.

**Verification**: Ensure OpenIddict validation is configured with correct audience:
```csharp
options.AddAudiences("grc-api", "GrcMvc");
```

---

### Step 8: Register OpenIddict API Clients

**File**: Create `Data/Seeds/OpenIddictClientSeeder.cs` OR add to existing seeder

**Purpose**: Register OpenIddict applications/clients for API authentication

**Code**:
```csharp
using OpenIddict.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.OpenIddict;

public class OpenIddictClientSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public OpenIddictClientSeeder(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Check if client already exists
        var existingClient = await _applicationManager.FindByClientIdAsync("grc-api");
        if (existingClient != null)
        {
            return; // Already seeded
        }

        // Create API client for service-to-service authentication
        await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "grc-api",
            ClientSecret = "your-secret-here", // Use secure secret in production
            DisplayName = "GRC API Client",
            Type = OpenIddictConstants.ClientTypes.Confidential,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Scopes.OpenId,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Roles
            }
        });

        // Create public client for browser-based apps (if needed)
        await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "grc-web",
            DisplayName = "GRC Web Application",
            Type = OpenIddictConstants.ClientTypes.Public,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Scopes.OpenId,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Roles
            },
            RedirectUris =
            {
                new Uri("https://grcsystem.com/callback"),
                new Uri("http://localhost:5000/callback") // For development
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://grcsystem.com/"),
                new Uri("http://localhost:5000/") // For development
            }
        });
    }
}
```

**Register Seeder** in `GrcMvcAbpModule.cs`:
```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    // ... existing code ...
    
    // Seed OpenIddict clients
    using (var scope = context.ServiceProvider.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<OpenIddictClientSeeder>();
        await seeder.SeedAsync(new DataSeedContext());
    }
}
```

**Why**: OpenIddict requires registered clients/applications to issue tokens. This seeder creates the necessary clients for API and web authentication.

**Security Note**: Store `grc-api` client secret securely (environment variable, secrets manager, etc.)

---

### Step 9: Remove JWT Configuration Files

**Files to Delete**:
1. `Configuration/JwtSettings.cs`
2. `Configuration/ConfigurationValidators.cs` (or remove `JwtSettingsValidator` class only)

**Files to Update**:
1. `appsettings.json` - Remove `JwtSettings` section
2. `appsettings.Production.json` - Remove `JwtSettings` section
3. `Program.cs` - Remove `JwtSettings` configuration and validation

---

### Step 10: Remove JWT NuGet Package

**File**: `GrcMvc.csproj`

**Remove**:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.8" />
```

---

### Step 11: Update AuthenticationService

**File**: `Services/Implementations/AuthenticationService.Identity.cs`

**Remove**:
- `JwtSettings` dependency injection
- JWT token validation code
- `JwtSecurityTokenHandler` usage

**Keep**:
- OpenIddict refresh token logic (if any)
- User authentication logic

---

### Step 12: Update Documentation

**Files to Update**:
1. `PRODUCTION_DEPLOYMENT_GUIDE.md` - Remove JWT environment variables, add OpenIddict configuration
2. `API_ENDPOINTS_REFERENCE.md` - Update authentication section to reference OpenIddict tokens
3. `API_IMPLEMENTATION_COMPLETE.md` - Update authentication examples

---

## Testing Checklist

### ✅ Pre-Migration Tests

- [ ] Verify OpenIddict SSO endpoints work (`/connect/authorize`, `/connect/token`)
- [ ] Verify OpenIddict userinfo endpoint works (`/connect/userinfo`)
- [ ] Verify JWT API authentication works (baseline)

### ✅ Post-Migration Tests

- [ ] Verify OpenIddict token endpoint works for API authentication
  ```bash
  POST /connect/token
  grant_type=client_credentials&client_id=grc-api&client_secret=secret
  ```
- [ ] Verify API endpoints accept OpenIddict tokens
  ```bash
  GET /api/control
  Authorization: Bearer <openiddict_token>
  ```
- [ ] Verify refresh token flow works
- [ ] Verify password grant type works (if needed)
- [ ] Verify authorization code flow works (SSO)
- [ ] Verify token introspection works (`/connect/introspect`)
- [ ] Verify token revocation works (`/connect/revocat`)

---

## Configuration Changes

### Environment Variables (Production)

**Remove**:
- `JWT_SECRET`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__ExpirationInHours`

**Add** (if not already present):
- `OpenIddict__EncryptionCertificate` (for production)
- `OpenIddict__SigningCertificate` (for production)

**Note**: Development certificates are auto-generated by ABP OpenIddict.

---

## API Client Migration Guide

### Before (JWT)

```bash
# Get JWT token
POST /api/account/token
{
  "email": "user@example.com",
  "password": "password"
}

# Use JWT token
GET /api/control
Authorization: Bearer <jwt_token>
```

### After (OpenIddict)

```bash
# Get OpenIddict token (Password Grant)
POST /connect/token
Content-Type: application/x-www-form-urlencoded
grant_type=password&username=user@example.com&password=password&client_id=grc-api

# Or use Client Credentials (for service-to-service)
POST /connect/token
Content-Type: application/x-www-form-urlencoded
grant_type=client_credentials&client_id=grc-api&client_secret=secret

# Use OpenIddict token
GET /api/control
Authorization: Bearer <openiddict_token>
```

---

## Rollback Plan

If issues occur, rollback steps:

1. **Restore JWT Configuration**
   - Re-add `JwtSettings.cs`
   - Re-add JWT Bearer authentication in `Program.cs`
   - Re-add JWT token endpoint in `AccountController.cs`

2. **Restore NuGet Package**
   - Re-add `Microsoft.AspNetCore.Authentication.JwtBearer` to `GrcMvc.csproj`

3. **Revert OpenIddict Changes**
   - Restore original `AddOpenIddict()` configuration in `GrcMvcAbpModule.cs`

---

## Success Criteria

✅ **Migration is successful when**:

1. All API endpoints authenticate using OpenIddict tokens
2. SSO continues to work via OpenIddict
3. No JWT-related code remains in the codebase
4. All tests pass
5. API clients can obtain and use OpenIddict tokens
6. Token refresh works correctly
7. Token introspection works correctly

---

## Next Steps

1. **Review this plan** with the team
2. **Create a feature branch**: `feature/100-percent-abp-openiddict`
3. **Execute migration steps** in order
4. **Run tests** after each step
5. **Update API client documentation**
6. **Deploy to staging** for validation
7. **Deploy to production** after approval

---

## Questions to Resolve

1. **OpenIddict Client Registration**: Do we need to register API clients in the database?
   - Answer: Yes, use `IOpenIddictApplicationManager` to create clients with appropriate grant types.

2. **Token Lifetime**: What should be the default token lifetime?
   - Answer: Configure in OpenIddict server options (default is usually 1 hour).

3. **Refresh Token Lifetime**: What should be the refresh token lifetime?
   - Answer: Configure in OpenIddict server options (default is usually 14 days).

4. **Client Credentials**: Should we use client credentials for service-to-service calls?
   - Answer: Yes, if needed. Register clients with `client_credentials` grant type.

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-12  
**Status**: Ready for Implementation
