# ABP Service Adoption Plan - Replace Custom Implementations

## Architecture: ABP = Core Foundation

```
┌─────────────────────────────────────────────────────────────┐
│                  CUSTOM BUSINESS LOGIC                       │
│   (TenantService, AuthenticationService, RiskService, etc.) │
├─────────────────────────────────────────────────────────────┤
│                 ABP SERVICE ADAPTERS                         │
│   IAbpIdentityServiceAdapter, IAbpTenantServiceAdapter,     │
│   IAbpSettingsServiceAdapter                                 │
├─────────────────────────────────────────────────────────────┤
│                  ABP CORE MODULES (29)                       │
│   Identity, TenantManagement, PermissionManagement,         │
│   FeatureManagement, AuditLogging, SettingManagement,       │
│   OpenIddict, Account, BackgroundJobs                        │
├─────────────────────────────────────────────────────────────┤
│                 ABP FRAMEWORK CORE                           │
│   Multi-tenancy, Auditing, EF Core, PostgreSQL              │
└─────────────────────────────────────────────────────────────┘
```

## Current Status
- **29 ABP Modules**: All enabled in `GrcMvcAbpModule.cs`
- **37 Total Services**: Available from all modules
- **10 Services**: Now actively used (7 direct + 3 via adapters)
- **27 Services**: Available for future adoption

---

## Currently Used ABP Services (7)

| Service | Usage Location | Status |
|---------|----------------|--------|
| `ICurrentUser` | `AbpCurrentUserAdapter` | ✅ Active |
| `ICurrentTenant` | `TenantContextService`, middleware | ✅ Active |
| `IPermissionChecker` | `PermissionAuthorizationHandler` | ✅ Active |
| `IFeatureChecker` | `FeatureCheckService` | ✅ Active |
| `IAuditingManager` | `AuditEventService` | ✅ Active |
| `IRepository<T>` | Registered (custom `IUnitOfWork` used) | ⚠️ Partial |
| `IBackgroundWorkerManager` | Available for workers | ✅ Active |

---

## Phase 1: Identity Services (Priority: HIGH)

### 1.1 Replace UserManager with IIdentityUserAppService

**Current**: `UserManager<ApplicationUser>` in `AuthenticationService.Identity.cs` (~556 lines)

**Target**: `IIdentityUserAppService` from ABP

**Files to Modify**:
- `Services/Implementations/AuthenticationService.Identity.cs`
- `Services/Implementations/UserManagementFacade.cs`
- `Services/Implementations/PlatformAdminService.cs`

**Code Change**:
```csharp
// Before
private readonly UserManager<ApplicationUser> _userManager;

// After
private readonly IIdentityUserAppService _identityUserAppService;
private readonly IIdentityUserRepository _identityUserRepository;
```

**Method Mappings**:
| Custom Method | ABP Method |
|---------------|------------|
| `_userManager.CreateAsync(user, password)` | `_identityUserAppService.CreateAsync(dto)` |
| `_userManager.FindByEmailAsync(email)` | `_identityUserAppService.FindByEmailAsync(email)` |
| `_userManager.GetRolesAsync(user)` | `_identityUserAppService.GetRolesAsync(userId)` |
| `_userManager.AddToRoleAsync(user, role)` | `_identityUserAppService.UpdateRolesAsync(userId, dto)` |

### 1.2 Add IIdentityRoleAppService

**Purpose**: Role management using ABP's built-in service

**Injection**:
```csharp
private readonly IIdentityRoleAppService _roleAppService;
```

### 1.3 Add IProfileAppService

**Purpose**: User profile management (self-service)

**Injection**:
```csharp
private readonly IProfileAppService _profileAppService;
```

---

## Phase 2: Tenant Services (Priority: HIGH)

### 2.1 Replace ITenantService with ITenantAppService

**Current**: Custom `TenantService.cs` (~512 lines)

**Target**: `ITenantAppService` from ABP + custom extensions

**Strategy**: Hybrid approach - use ABP for CRUD, keep custom logic for:
- Activation email workflow
- Tenant provisioning
- Custom slug-based resolution

**Files to Modify**:
- `Services/Implementations/TenantService.cs`
- `Services/Interfaces/ITenantService.cs`

**Code Change**:
```csharp
// Add ABP service alongside custom logic
private readonly ITenantAppService _abpTenantAppService;
private readonly ITenantRepository _tenantRepository;
private readonly ITenantManager _tenantManager;
```

### 2.2 Add ITenantManager

**Purpose**: Tenant business logic (validation, slug uniqueness)

```csharp
// ABP's ITenantManager handles:
// - Tenant name validation
// - Connection string management
// - Tenant normalization
```

---

## Phase 3: Settings Services (Priority: MEDIUM)

### 3.1 Add ISettingManager

**Purpose**: Application settings with tenant/user scope

**Injection**:
```csharp
private readonly ISettingManager _settingManager;
```

**Usage**:
```csharp
// Get setting for current tenant
var value = await _settingManager.GetOrNullForCurrentTenantAsync("App.Theme");

// Set setting for tenant
await _settingManager.SetForTenantAsync(tenantId, "App.Theme", "dark");
```

### 3.2 Add ISettingAppService

**Purpose**: Settings management UI

### 3.3 Add ISettingProvider

**Purpose**: Read-only setting access

---

## Phase 4: Permission & Feature AppServices (Priority: MEDIUM)

### 4.1 Add IPermissionAppService

**Purpose**: Permission management UI

```csharp
private readonly IPermissionAppService _permissionAppService;

// Get permissions for role
var permissions = await _permissionAppService.GetAsync("R", roleId.ToString());

// Update permissions
await _permissionAppService.UpdateAsync("R", roleId.ToString(), updateDto);
```

### 4.2 Add IFeatureAppService

**Purpose**: Feature management UI

```csharp
private readonly IFeatureAppService _featureAppService;

// Get features for tenant
var features = await _featureAppService.GetAsync("T", tenantId.ToString());

// Update features
await _featureAppService.UpdateAsync("T", tenantId.ToString(), updateDto);
```

---

## Phase 5: OpenIddict Services (Priority: LOW)

### 5.1 Add IOpenIddictApplicationManager

**Purpose**: OAuth application management

```csharp
private readonly IOpenIddictApplicationManager _applicationManager;

// Create OAuth client
await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
{
    ClientId = "my-client",
    ClientSecret = "secret",
    Permissions = { ... }
});
```

### 5.2 Add IOpenIddictTokenManager

**Purpose**: Token management (revocation, cleanup)

### 5.3 Add IOpenIddictScopeManager

**Purpose**: OAuth scope management

### 5.4 Add IOpenIddictAuthorizationManager

**Purpose**: Authorization management

---

## Phase 6: Account Services (Priority: LOW)

### 6.1 Add IAccountAppService

**Purpose**: Login/register/profile operations

```csharp
private readonly IAccountAppService _accountAppService;

// Register user
var result = await _accountAppService.RegisterAsync(new RegisterDto
{
    UserName = email,
    EmailAddress = email,
    Password = password
});
```

---

## Implementation Priority

| Phase | Services | Impact | Risk | Days |
|-------|----------|--------|------|------|
| 1 | Identity (3 services) | HIGH | MEDIUM | 3-5 |
| 2 | Tenant (3 services) | HIGH | MEDIUM | 2-3 |
| 3 | Settings (3 services) | MEDIUM | LOW | 1-2 |
| 4 | Permission/Feature (2 services) | MEDIUM | LOW | 1-2 |
| 5 | OpenIddict (4 services) | LOW | LOW | 2-3 |
| 6 | Account (2 services) | LOW | LOW | 1-2 |

**Total Estimated Time**: 10-17 days

---

## Migration Strategy

### Approach: Hybrid Wrapper Pattern

Instead of replacing custom services entirely, create **wrapper services** that:
1. Use ABP services internally
2. Maintain backward-compatible API
3. Add custom business logic on top

**Example**:
```csharp
public class AbpTenantServiceAdapter : ITenantService
{
    private readonly ITenantAppService _abpTenantAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IEmailService _emailService; // Custom
    
    public async Task<Tenant> CreateTenantAsync(string name, string email, string slug)
    {
        // Use ABP for CRUD
        var tenant = await _abpTenantAppService.CreateAsync(new TenantCreateDto
        {
            Name = name,
            AdminEmailAddress = email
        });
        
        // Custom logic for slug, activation email, etc.
        await SendActivationEmailAsync(tenant);
        
        return MapToCustomTenant(tenant);
    }
}
```

---

## Database Migration Required

After integrating new ABP services, run:

```bash
cd Shahin-Jan-2026/src/GrcMvc
dotnet ef migrations add IntegrateAbpServices --context GrcDbContext
dotnet ef database update --context GrcDbContext
```

---

## Verification Checklist

- [ ] All 29 modules in `[DependsOn]`
- [ ] All `Configure*()` methods in `OnModelCreating()`
- [ ] Build succeeds with zero errors
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Multi-tenant isolation verified
- [ ] Audit logging working
- [ ] Permission checks working
- [ ] Feature checks working

---

## Next Steps

1. **Immediate**: Verify current build compiles
2. **Phase 1**: Start with Identity services (highest impact)
3. **Phase 2**: Migrate tenant management
4. **Ongoing**: Add remaining services as needed

---

*Created: 2026-01-20*
*Last Updated: 2026-01-20*
