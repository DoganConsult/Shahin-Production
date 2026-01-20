# ABP 19 Modules - Complete Replacement & Integration Guide

**Generated:** 2026-01-12  
**Purpose:** Detailed guide showing all 19 modules, what custom code they replace, and full integration steps

---

## Complete List of 19 Modules

### Category 1: Application Modules (5 modules) - CRITICAL

1. **AbpIdentityApplicationModule**
2. **AbpTenantManagementApplicationModule**
3. **AbpFeatureManagementApplicationModule**
4. **AbpPermissionManagementApplicationModule**
5. **AbpSettingManagementApplicationModule**

### Category 2: EntityFrameworkCore Modules (7 modules) - CRITICAL

6. **AbpIdentityEntityFrameworkCoreModule**
7. **AbpTenantManagementEntityFrameworkCoreModule**
8. **AbpFeatureManagementEntityFrameworkCoreModule**
9. **AbpPermissionManagementEntityFrameworkCoreModule**
10. **AbpAuditLoggingEntityFrameworkCoreModule**
11. **AbpSettingManagementEntityFrameworkCoreModule**
12. **AbpOpenIddictEntityFrameworkCoreModule**

### Category 3: Additional Modules (7 modules) - OPTIONAL

13. **AbpAccountWebModule**
14. **AbpBackgroundJobsDomainModule**
15. **AbpCmsKitWebModule**
16. **AbpDocsWebModule**
17. **AbpVirtualFileExplorerWebModule**
18. **AbpIdentityServerDomainModule** (NOT added - using OpenIddict)
19. **Localization** (Already included in Core - no separate module)

---

## Module 1: AbpIdentityApplicationModule

### What It Replaces

**Current Custom Code:**
- `UserManager<ApplicationUser>` usage in controllers/services
- `SignInManager<ApplicationUser>` usage
- Custom user creation logic in `IdentityAuthenticationService`
- Direct `DbContext` queries for users
- Custom user update/delete operations

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Services/Implementations/AuthenticationService.Identity.cs` (~556 lines)
- `Shahin-Jan-2026/src/GrcMvc/Controllers/AccountController.cs`
- `Shahin-Jan-2026/src/GrcMvc/Controllers/UserController.cs`
- `Shahin-Jan-2026/src/GrcMvc/Models/Entities/ApplicationUser.cs`

### Integration Steps

#### Step 1: Update ApplicationUser Entity

**File:** `Shahin-Jan-2026/src/GrcMvc/Models/Entities/ApplicationUser.cs`

**Before:**
```csharp
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    // ... custom properties
}
```

**After:**
```csharp
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    // ... keep all custom properties
    // ABP adds: TenantId, ExtraProperties, ConcurrencyStamp, etc.
}
```

#### Step 2: Replace UserManager with IIdentityUserAppService

**File:** `Shahin-Jan-2026/src/GrcMvc/Services/Implementations/AuthenticationService.Identity.cs`

**Before:**
```csharp
public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public async Task<AuthTokenDto?> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var result = await _signInManager.PasswordSignInAsync(user, password, ...);
        // ... custom logic
    }

    public async Task<ApplicationUser> CreateUserAsync(string email, string password)
    {
        var user = new ApplicationUser { Email = email, UserName = email };
        await _userManager.CreateAsync(user, password);
        return user;
    }
}
```

**After:**
```csharp
using Volo.Abp.Identity;
using Volo.Abp.Application.Services;

public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IPasswordHasher<IdentityUser> _passwordHasher;

    public async Task<AuthTokenDto?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null) return null;

        // Verify password using ABP's password hasher
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verificationResult != PasswordVerificationResult.Success) return null;

        // ... generate JWT token
    }

    public async Task<ApplicationUser> CreateUserAsync(string email, string password)
    {
        var user = await _identityUserAppService.CreateAsync(new IdentityUserCreateDto
        {
            UserName = email,
            Email = email,
            Password = password,
            // Map custom properties
        });
        return user; // ABP returns IdentityUserDto, map to ApplicationUser if needed
    }
}
```

#### Step 3: Update Controllers

**File:** `Shahin-Jan-2026/src/GrcMvc/Controllers/AccountController.cs`

**Before:**
```csharp
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var user = new ApplicationUser { Email = model.Email };
        await _userManager.CreateAsync(user, model.Password);
        // ...
    }
}
```

**After:**
```csharp
using Volo.Abp.Identity;

public class AccountController : Controller
{
    private readonly IIdentityUserAppService _identityUserAppService;

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var user = await _identityUserAppService.CreateAsync(new IdentityUserCreateDto
        {
            UserName = model.Email,
            Email = model.Email,
            Password = model.Password
        });
        // ...
    }
}
```

### Database Migration

```bash
dotnet ef migrations add MigrateIdentityToAbp --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `AbpUsers` (extends IdentityUser)
- `AbpUserRoles`
- `AbpUserClaims`
- `AbpUserLogins`
- `AbpUserTokens`
- `AbpRoles`
- `AbpRoleClaims`

### Integration Checklist

- [ ] Update `ApplicationUser` to inherit from `Volo.Abp.Identity.IdentityUser`
- [ ] Replace `UserManager<ApplicationUser>` with `IIdentityUserAppService` in all services
- [ ] Replace `SignInManager<ApplicationUser>` with ABP authentication
- [ ] Update all user CRUD operations to use `IIdentityUserAppService`
- [ ] Update password hashing to use ABP's `IPasswordHasher`
- [ ] Test user creation, login, update, delete
- [ ] Verify multi-tenant user isolation

---

## Module 2: AbpTenantManagementApplicationModule

### What It Replaces

**Current Custom Code:**
- `TenantService` class (~512 lines)
- `ITenantService` interface
- Custom tenant creation logic
- Custom tenant activation/suspension logic
- Direct `DbContext` queries for tenants

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Services/Implementations/TenantService.cs` (~512 lines)
- `Shahin-Jan-2026/src/GrcMvc/Services/Interfaces/ITenantService.cs`
- `Shahin-Jan-2026/src/GrcMvc/Models/Entities/Tenant.cs`
- `Shahin-Jan-2026/src/GrcMvc/Controllers/TenantController.cs`

### Integration Steps

#### Step 1: Update Tenant Entity

**File:** `Shahin-Jan-2026/src/GrcMvc/Models/Entities/Tenant.cs`

**Before:**
```csharp
public class Tenant : BaseEntity
{
    public string TenantSlug { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    // ... custom properties
}
```

**After:**
```csharp
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

public class Tenant : Volo.Abp.TenantManagement.Tenant
{
    // ABP provides: Id, Name, NormalizedName, ConcurrencyStamp, ExtraProperties
    
    // Keep custom properties
    public string TenantSlug { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    
    // Map ABP's Name to OrganizationName if needed
    // Or use Name property from ABP base class
}
```

#### Step 2: Replace TenantService with ITenantAppService

**File:** `Shahin-Jan-2026/src/GrcMvc/Services/Implementations/TenantService.cs`

**Before:**
```csharp
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Tenant> CreateTenantAsync(string organizationName, string adminEmail, string tenantSlug)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            OrganizationName = organizationName,
            AdminEmail = adminEmail,
            TenantSlug = tenantSlug
        };
        await _unitOfWork.Tenants.AddAsync(tenant);
        await _unitOfWork.SaveChangesAsync();
        return tenant;
    }
}
```

**After:**
```csharp
using Volo.Abp.TenantManagement;
using Volo.Abp.Application.Services;

public class TenantService : ApplicationService, ITenantService
{
    private readonly ITenantAppService _tenantAppService;

    public async Task<Tenant> CreateTenantAsync(string organizationName, string adminEmail, string tenantSlug)
    {
        var tenantDto = await _tenantAppService.CreateAsync(new TenantCreateDto
        {
            Name = tenantSlug, // Or use organizationName
            AdminEmailAddress = adminEmail,
            // Map TenantSlug to a custom property or use Name
        });

        // Map DTO back to Tenant entity if needed
        // Or work with TenantDto directly
        return MapToTenantEntity(tenantDto);
    }
}
```

#### Step 3: Update Controllers

**File:** `Shahin-Jan-2026/src/GrcMvc/Controllers/TenantController.cs`

**Before:**
```csharp
public class TenantController : Controller
{
    private readonly ITenantService _tenantService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantViewModel model)
    {
        var tenant = await _tenantService.CreateTenantAsync(
            model.OrganizationName, 
            model.AdminEmail, 
            model.TenantSlug
        );
        return Ok(tenant);
    }
}
```

**After:**
```csharp
using Volo.Abp.TenantManagement;

public class TenantController : Controller
{
    private readonly ITenantAppService _tenantAppService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantViewModel model)
    {
        var tenant = await _tenantAppService.CreateAsync(new TenantCreateDto
        {
            Name = model.TenantSlug,
            AdminEmailAddress = model.AdminEmail
        });
        return Ok(tenant);
    }
}
```

### Database Migration

```bash
dotnet ef migrations add MigrateTenantToAbp --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `AbpTenants` (extends Tenant)
- `AbpTenantConnectionStrings`

### Integration Checklist

- [ ] Update `Tenant` to inherit from `Volo.Abp.TenantManagement.Tenant`
- [ ] Replace `ITenantService` with `ITenantAppService` in all controllers
- [ ] Update tenant CRUD operations to use ABP DTOs
- [ ] Map custom properties (TenantSlug, OrganizationName) to ABP properties
- [ ] Test tenant creation, activation, suspension
- [ ] Verify tenant connection string management

---

## Module 3: AbpFeatureManagementApplicationModule

### What It Replaces

**Current Custom Code:**
- `FeatureCheckService` class (~116 lines)
- `IFeatureCheckService` interface
- Custom feature checking logic based on subscription/edition
- Custom feature definition logic

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Abp/FeatureCheckService.cs` (~116 lines)
- `Shahin-Jan-2026/src/GrcMvc/Abp/IFeatureCheckService.cs`
- All controllers using `IFeatureCheckService`

### Integration Steps

#### Step 1: Replace IFeatureCheckService with IFeatureChecker

**File:** `Shahin-Jan-2026/src/GrcMvc/Abp/GrcMvcAbpModule.cs`

**Before:**
```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    // Keep custom FeatureCheckService for backward compatibility
    context.Services.AddSingleton<IFeatureCheckService, FeatureCheckService>();
}
```

**After:**
```csharp
using Volo.Abp.FeatureManagement;

public override void ConfigureServices(ServiceConfigurationContext context)
{
    // Remove custom service - use ABP's IFeatureChecker instead
    // context.Services.AddSingleton<IFeatureCheckService, FeatureCheckService>(); // REMOVE
}
```

#### Step 2: Update Controllers

**File:** Any controller using `IFeatureCheckService`

**Before:**
```csharp
public class RiskController : Controller
{
    private readonly IFeatureCheckService _featureCheck;

    public async Task<IActionResult> Index()
    {
        var canUseAI = await _featureCheck.IsEnabledAsync(GrcFeatures.AI.Enabled);
        if (!canUseAI) return Forbid();
        // ...
    }
}
```

**After:**
```csharp
using Volo.Abp.FeatureManagement;

public class RiskController : Controller
{
    private readonly IFeatureChecker _featureChecker;

    public async Task<IActionResult> Index()
    {
        var canUseAI = await _featureChecker.IsEnabledAsync("GRC.AI.Enabled");
        if (!canUseAI) return Forbid();
        // ...
    }
}
```

#### Step 3: Update Feature Definition Provider

**File:** `Shahin-Jan-2026/src/GrcMvc/Abp/GrcFeatureDefinitionProvider.cs`

**Verify it extends ABP base class:**

```csharp
using Volo.Abp.FeatureManagement;

public class GrcFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        // Define features using ABP pattern
        context.Add(new FeatureDefinition(
            "GRC.AI.Enabled",
            defaultValue: "false",
            displayName: new LocalizableString("AI Features", "GrcResource")
        ));
    }
}
```

### Database Migration

```bash
dotnet ef migrations add AddFeatureManagement --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `AbpFeatures` (feature values per tenant/user)

### Integration Checklist

- [ ] Remove `IFeatureCheckService` registration
- [ ] Replace `IFeatureCheckService` with `IFeatureChecker` in all controllers
- [ ] Update feature check calls to use ABP feature names
- [ ] Verify `GrcFeatureDefinitionProvider` extends `FeatureDefinitionProvider`
- [ ] Test feature checks work correctly
- [ ] Verify tenant-scoped features work

---

## Module 4: AbpPermissionManagementApplicationModule

### What It Replaces

**Current Custom Code:**
- Custom `PermissionCatalog` entity
- Custom `PermissionAuthorizationHandler` (~150 lines)
- Custom permission checking logic
- Custom permission assignment logic

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Authorization/PermissionAuthorizationHandler.cs` (~150 lines)
- `Shahin-Jan-2026/src/GrcMvc/Application/Permissions/PermissionDefinitionProvider.cs`
- `Shahin-Jan-2026/src/GrcMvc/Models/Entities/RbacModels.cs` (Permission entity)

### Integration Steps

#### Step 1: Update Permission Definition Provider

**File:** `Shahin-Jan-2026/src/GrcMvc/Application/Permissions/PermissionDefinitionProvider.cs`

**Before:**
```csharp
public class PermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public void Define(IPermissionDefinitionContext context)
    {
        // Custom permission definitions
    }
}
```

**After:**
```csharp
using Volo.Abp.Authorization.Permissions;

public class GrcPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var grcGroup = context.AddGroup("Grc", "GRC Permissions");

        var assessments = grcGroup.AddPermission("Grc.Assessments.View", "View Assessments");
        assessments.AddChild("Grc.Assessments.Create", "Create Assessments");
        assessments.AddChild("Grc.Assessments.Update", "Update Assessments");
        // ... all 60+ permissions
    }
}
```

#### Step 2: Replace Custom Authorization Handler

**File:** `Shahin-Jan-2026/src/GrcMvc/Authorization/PermissionAuthorizationHandler.cs`

**Before:**
```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Custom permission checking logic
        var hasPermission = await CheckDatabasePermissionAsync(userId, permission);
        if (hasPermission) context.Succeed(requirement);
    }
}
```

**After:**
```csharp
using Volo.Abp.Authorization.Permissions;

// ABP provides built-in permission checking
// Remove custom handler, use ABP's [Authorize("PermissionName")] attribute
```

#### Step 3: Update Controllers to Use ABP Permissions

**File:** `Shahin-Jan-2026/src/GrcMvc/Controllers/RiskController.cs`

**Before:**
```csharp
[Authorize(GrcPermissions.Risks.View)]
public class RiskController : Controller
{
    // Custom permission checking
}
```

**After:**
```csharp
using Volo.Abp.AspNetCore.Mvc.Authorization;

[Authorize("Grc.Risks.View")]
public class RiskController : Controller
{
    // ABP automatically checks permissions
}
```

### Database Migration

```bash
dotnet ef migrations add AddPermissionManagement --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `AbpPermissionGrants` (permission assignments per tenant/user/role)

### Integration Checklist

- [ ] Update `PermissionDefinitionProvider` to extend ABP base class
- [ ] Remove custom `PermissionAuthorizationHandler`
- [ ] Update all `[Authorize]` attributes to use ABP permission format
- [ ] Migrate permission assignments from custom tables to `AbpPermissionGrants`
- [ ] Test permission checking works
- [ ] Verify tenant-scoped permissions work

---

## Module 5: AbpSettingManagementApplicationModule

### What It Replaces

**Current Custom Code:**
- Direct `ISettingManager` usage (may already be using ABP)
- Custom setting storage logic
- Custom setting retrieval logic

**Files Affected:**
- Services using `ISettingManager` (may already be ABP-compliant)

### Integration Steps

#### Step 1: Use ISettingAppService for UI

**File:** Controllers that need setting management UI

**Before:**
```csharp
public class SettingsController : Controller
{
    private readonly ISettingManager _settingManager;

    public async Task<IActionResult> GetSetting(string name)
    {
        var value = await _settingManager.GetOrNullAsync(name);
        return Ok(value);
    }
}
```

**After:**
```csharp
using Volo.Abp.SettingManagement;

public class SettingsController : Controller
{
    private readonly ISettingAppService _settingAppService;

    public async Task<IActionResult> GetSetting(string name)
    {
        var setting = await _settingAppService.GetAsync(name);
        return Ok(setting);
    }
}
```

### Database Migration

**Already configured** - Settings tables should already exist.

### Integration Checklist

- [ ] Replace `ISettingManager` with `ISettingAppService` in controllers (if UI needed)
- [ ] Keep `ISettingManager` in services (for programmatic access)
- [ ] Test setting retrieval and updates
- [ ] Verify tenant-scoped settings work

---

## Module 6: AbpIdentityEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual Identity table configuration
- Custom Identity DbContext setup

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

#### Step 1: Verify DbContext Configuration

**File:** `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

**Already Done (verify):**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ABP Identity configuration
    modelBuilder.ConfigureIdentity();
    
    // ... other configurations
}
```

**Integration:** Module automatically configures Identity tables when added to [DependsOn].

### Integration Checklist

- [ ] Verify `ConfigureIdentity()` is called in `GrcDbContext`
- [ ] Verify module is in [DependsOn]
- [ ] Test Identity tables are created
- [ ] Verify Identity queries work

---

## Module 7: AbpTenantManagementEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual tenant table configuration
- Custom tenant DbContext setup

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

**Already Done (verify):**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ABP Tenant Management configuration
    modelBuilder.ConfigureTenantManagement();
}
```

### Integration Checklist

- [ ] Verify `ConfigureTenantManagement()` is called
- [ ] Verify module is in [DependsOn]
- [ ] Test tenant tables are created

---

## Module 8: AbpFeatureManagementEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual feature table configuration

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

**Already Done (verify):**
```csharp
modelBuilder.ConfigureFeatureManagement();
```

### Integration Checklist

- [ ] Verify `ConfigureFeatureManagement()` is called
- [ ] Verify module is in [DependsOn]
- [ ] Test feature tables are created

---

## Module 9: AbpPermissionManagementEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual permission grant table configuration

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

**Already Done (verify):**
```csharp
modelBuilder.ConfigurePermissionManagement();
```

### Integration Checklist

- [ ] Verify `ConfigurePermissionManagement()` is called
- [ ] Verify module is in [DependsOn]
- [ ] Test permission tables are created

---

## Module 10: AbpAuditLoggingEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual audit log table configuration

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

**Already Done (verify):**
```csharp
modelBuilder.ConfigureAuditLogging();
```

### Integration Checklist

- [ ] Verify `ConfigureAuditLogging()` is called
- [ ] Verify module is in [DependsOn]
- [ ] Test audit log tables are created

---

## Module 11: AbpSettingManagementEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual setting table configuration

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

**Already Done (verify):**
```csharp
modelBuilder.ConfigureSettingManagement();
```

### Integration Checklist

- [ ] Verify `ConfigureSettingManagement()` is called
- [ ] Verify module is in [DependsOn]
- [ ] Test setting tables are created

---

## Module 12: AbpOpenIddictEntityFrameworkCoreModule

### What It Replaces

**Current Custom Code:**
- Manual OpenIddict table configuration

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Data/GrcDbContext.cs`

### Integration Steps

**Already Done (verify):**
```csharp
modelBuilder.ConfigureOpenIddict();
```

### Integration Checklist

- [ ] Verify `ConfigureOpenIddict()` is called
- [ ] Verify module is in [DependsOn]
- [ ] Test OpenIddict tables are created

---

## Module 13: AbpAccountWebModule

### What It Replaces

**Current Custom Code:**
- Custom login/register pages
- Custom authentication UI
- Custom password reset UI

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/Views/Account/Login.cshtml`
- `Shahin-Jan-2026/src/GrcMvc/Views/Account/Register.cshtml`
- `Shahin-Jan-2026/src/GrcMvc/Controllers/AccountController.cs`

### Integration Steps

#### Step 1: Install Account Module Packages

```bash
dotnet add package Volo.Abp.Account.Application --version 8.2.2
dotnet add package Volo.Abp.Account.Application.Contracts --version 8.2.2
dotnet add package Volo.Abp.Account.HttpApi --version 8.2.2
dotnet add package Volo.Abp.Account.Web --version 8.2.2
```

#### Step 2: Add Module to [DependsOn]

```csharp
[DependsOn(
    // ... existing modules ...
    typeof(AbpAccountWebModule)
)]
```

#### Step 3: Configure Account Module

**File:** `Shahin-Jan-2026/src/GrcMvc/Abp/GrcMvcAbpModule.cs`

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    // Configure Account module
    Configure<AbpAccountOptions>(options =>
    {
        options.EnableLocalLogin = true;
        options.EnableSelfRegistration = true;
        // ... other options
    });
}
```

#### Step 4: Use ABP Account Pages (Optional)

**Option A:** Keep custom pages, use ABP services
**Option B:** Replace with ABP's pre-built pages

**If replacing:**
- Remove custom `AccountController` actions
- Use ABP's account pages at `/Account/Login`, `/Account/Register`
- Customize ABP pages if needed

### Integration Checklist

- [ ] Install Account packages
- [ ] Add module to [DependsOn]
- [ ] Configure Account options
- [ ] Decide: Keep custom UI or use ABP pages
- [ ] Test login/register flows
- [ ] Test password reset

---

## Module 14: AbpBackgroundJobsDomainModule

### What It Replaces

**Current Custom Code:**
- Hangfire jobs (can coexist)
- Custom job queue logic

**Files Affected:**
- `Shahin-Jan-2026/src/GrcMvc/BackgroundJobs/EscalationJob.cs`
- `Shahin-Jan-2026/src/GrcMvc/BackgroundJobs/NotificationDeliveryJob.cs`
- `Shahin-Jan-2026/src/GrcMvc/BackgroundJobs/SlaMonitorJob.cs`

### Integration Steps

#### Step 1: Install Background Jobs Packages

```bash
dotnet add package Volo.Abp.BackgroundJobs.Domain --version 8.2.2
dotnet add package Volo.Abp.BackgroundJobs.EntityFrameworkCore --version 8.2.2
```

#### Step 2: Add Module to [DependsOn]

```csharp
[DependsOn(
    // ... existing modules ...
    typeof(AbpBackgroundJobsDomainModule)
)]
```

#### Step 3: Migrate Hangfire Jobs to ABP Background Jobs (Optional)

**File:** `Shahin-Jan-2026/src/GrcMvc/BackgroundJobs/EscalationJob.cs`

**Before (Hangfire):**
```csharp
public class EscalationJob
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync()
    {
        // Job logic
    }
}
```

**After (ABP Background Jobs):**
```csharp
using Volo.Abp.BackgroundJobs;

public class EscalationJob : AsyncBackgroundJob<EscalationJobArgs>
{
    protected override async Task ExecuteAsync(EscalationJobArgs args)
    {
        // Job logic
    }
}

// Enqueue job
await _backgroundJobManager.EnqueueAsync(new EscalationJobArgs { ... });
```

**Note:** Can keep Hangfire for complex workflows, use ABP for simple jobs.

### Database Migration

```bash
dotnet ef migrations add AddBackgroundJobs --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `AbpBackgroundJobs` (job queue)

### Integration Checklist

- [ ] Install Background Jobs packages
- [ ] Add module to [DependsOn]
- [ ] Decide: Migrate jobs or keep Hangfire
- [ ] Test job execution
- [ ] Verify job retry logic

---

## Module 15: AbpCmsKitWebModule

### What It Replaces

**Current Custom Code:**
- Custom blog/content management (if exists)
- Custom page management (if exists)

**Files Affected:**
- None (new functionality)

### Integration Steps

#### Step 1: Install CMS Kit Packages

```bash
dotnet add package Volo.Abp.CmsKit.Domain --version 8.2.2
dotnet add package Volo.Abp.CmsKit.Application --version 8.2.2
dotnet add package Volo.Abp.CmsKit.Application.Contracts --version 8.2.2
dotnet add package Volo.Abp.CmsKit.EntityFrameworkCore --version 8.2.2
dotnet add package Volo.Abp.CmsKit.Web --version 8.2.2
```

#### Step 2: Add Module to [DependsOn]

```csharp
[DependsOn(
    // ... existing modules ...
    typeof(AbpCmsKitWebModule)
)]
```

#### Step 3: Configure CMS Kit

**File:** `Shahin-Jan-2026/src/GrcMvc/Abp/GrcMvcAbpModule.cs`

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    // CMS Kit is configured automatically
    // Add custom configuration if needed
}
```

#### Step 4: Use CMS Kit Features

**New Features Available:**
- Blog posts
- Pages
- Comments
- Reactions (like/dislike)
- Ratings
- Tags
- Media

**Usage:**
```csharp
using Volo.CmsKit.Public.Blogs;
using Volo.CmsKit.Public.Pages;

public class ContentController : Controller
{
    private readonly IBlogPostPublicAppService _blogService;
    private readonly IPagePublicAppService _pageService;

    // Use CMS Kit services
}
```

### Database Migration

```bash
dotnet ef migrations add AddCmsKit --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `CmsBlogs`
- `CmsBlogPosts`
- `CmsPages`
- `CmsComments`
- `CmsReactions`
- `CmsRatings`
- `CmsTags`
- `CmsMedia`

### Integration Checklist

- [ ] Install CMS Kit packages
- [ ] Add module to [DependsOn]
- [ ] Create migration
- [ ] Test blog/page creation
- [ ] Test comments/reactions
- [ ] Customize CMS Kit UI if needed

---

## Module 16: AbpDocsWebModule

### What It Replaces

**Current Custom Code:**
- Custom documentation site (if exists)

**Files Affected:**
- None (new functionality)

### Integration Steps

#### Step 1: Install Docs Packages

```bash
dotnet add package Volo.Abp.Docs.Domain --version 8.2.2
dotnet add package Volo.Abp.Docs.Application --version 8.2.2
dotnet add package Volo.Abp.Docs.Application.Contracts --version 8.2.2
dotnet add package Volo.Abp.Docs.EntityFrameworkCore --version 8.2.2
dotnet add package Volo.Abp.Docs.Web --version 8.2.2
```

#### Step 2: Add Module to [DependsOn]

```csharp
[DependsOn(
    // ... existing modules ...
    typeof(AbpDocsWebModule)
)]
```

#### Step 3: Configure Docs Module

**File:** `Shahin-Jan-2026/src/GrcMvc/Abp/GrcMvcAbpModule.cs`

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    // Docs module is configured automatically
    // Access docs at /docs
}
```

### Database Migration

```bash
dotnet ef migrations add AddDocs --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates Tables:**
- `DocsProjects`
- `DocsDocuments`
- `DocsDocumentContributors`

### Integration Checklist

- [ ] Install Docs packages
- [ ] Add module to [DependsOn]
- [ ] Create migration
- [ ] Test documentation site
- [ ] Import existing docs if needed

---

## Module 17: AbpVirtualFileExplorerWebModule

### What It Replaces

**Current Custom Code:**
- Custom file management UI (if exists)

**Files Affected:**
- None (new functionality)

### Integration Steps

#### Step 1: Install Virtual File Explorer Package

```bash
dotnet add package Volo.Abp.VirtualFileExplorer.Web --version 8.2.2
```

#### Step 2: Add Module to [DependsOn]

```csharp
[DependsOn(
    // ... existing modules ...
    typeof(AbpVirtualFileExplorerWebModule)
)]
```

#### Step 3: Access Virtual File Explorer

**URL:** `/VirtualFileExplorer` (automatically available)

**Features:**
- Browse embedded resources
- View virtual file system
- Useful for debugging and development

### Integration Checklist

- [ ] Install package
- [ ] Add module to [DependsOn]
- [ ] Test virtual file explorer UI
- [ ] Restrict access to admin users if needed

---

## Module 18: AbpIdentityServerDomainModule

### Status: NOT ADDED

**Reason:** Using OpenIddict instead (Module 12)

**Decision:** Do NOT install IdentityServer module if using OpenIddict.

---

## Module 19: Localization

### Status: ALREADY INCLUDED

**Reason:** Localization is included in `Volo.Abp.Core`

**Current Implementation:**
- Resource files: `SharedResource.resx`, `SharedResource.ar.resx`
- Localization configured in `Program.cs`
- `IStringLocalizer<SharedResource>` used throughout

**No Action Needed:** Localization is already working.

---

## Complete Integration Summary

### Files to Modify

1. **GrcMvc.csproj** - Add 29 packages
2. **GrcMvcAbpModule.cs** - Add 19 modules to [DependsOn], add configurations
3. **ApplicationUser.cs** - Change base class
4. **Tenant.cs** - Change base class
5. **GrcDbContext.cs** - Verify all Configure*() methods
6. **AuthenticationService.Identity.cs** - Replace UserManager with IIdentityUserAppService
7. **TenantService.cs** - Replace with ITenantAppService
8. **FeatureCheckService.cs** - Remove, use IFeatureChecker
9. **PermissionAuthorizationHandler.cs** - Remove, use ABP's built-in
10. **All Controllers** - Update to use ABP services and attributes

### Database Migrations Needed

```bash
dotnet ef migrations add AddAll19AbpModules --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

**Creates ~50+ new tables** for all ABP modules.

### Testing Requirements

For each module:
- [ ] Unit tests for service replacements
- [ ] Integration tests for API endpoints
- [ ] Manual testing of UI flows
- [ ] Multi-tenant isolation testing
- [ ] Permission/feature checking tests

---

**Total Modules:** 19 modules  
**Total Packages:** 29 packages  
**Total Files to Modify:** ~50+ files  
**Estimated Time:** 3-5 days for complete integration
