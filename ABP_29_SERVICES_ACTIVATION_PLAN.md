# ABP 29 Services Activation Plan

**Date:** 2026-01-19  
**Status:** Planning Phase  
**Objective:** Replace custom implementations with ABP services across all 29 modules

---

## 📊 Current State Analysis

### ✅ **ABP Modules Status**
- **29 modules:** All enabled in `GrcMvcAbpModule.cs`
- **37 total services:** Available from all modules
- **7 services:** Currently used (ICurrentUser, ICurrentTenant, IPermissionChecker, IFeatureChecker, IAuditingManager, IRepository<T>, IBackgroundWorkerManager)
- **28 services:** Available but not used (can replace custom implementations)
- **2 services:** Disabled (IBackgroundJobManager - Hangfire used instead)

### ⚠️ **Custom Services Still in Use**
1. **TenantService** (custom) → Replace with `ITenantAppService`
2. **UserManager<ApplicationUser>** → Replace with `IIdentityUserAppService`
3. **IUnitOfWork** (custom) → Replace with ABP's `IUnitOfWork` + `IRepository<T>`
4. **IGenericRepository<T>** (custom) → Replace with ABP's `IRepository<T>`
5. **ISettingService** (custom) → Replace with `ISettingManager`
6. **AccountController** (custom) → Use `IAccountAppService`
7. **OpenIddict services** (not used) → Activate for OAuth/OIDC

---

## 🎯 Activation Strategy: Phased Approach

### **Phase 1: Identity & Tenant Services (High Priority)**
**Impact:** Core user and tenant management  
**Risk:** Medium (affects authentication and tenant operations)  
**Timeline:** 2-3 days

#### 1.1 Replace UserManager with IIdentityUserAppService

**Files to Update:**
- `Controllers/AccountController.cs`
- `Controllers/RegisterController.cs`
- `Controllers/OnboardingController.cs`
- `Controllers/TrialController.cs`
- `Services/Implementations/OwnerTenantService.cs`
- `Controllers/Api/TenantsApiController.cs`

**Migration Steps:**

1. **Inject IIdentityUserAppService** in controllers:
```csharp
private readonly IIdentityUserAppService _userAppService;
private readonly IIdentityRoleAppService _roleAppService;

public AccountController(
    IIdentityUserAppService userAppService,
    IIdentityRoleAppService roleAppService,
    // ... other dependencies
)
{
    _userAppService = userAppService;
    _roleAppService = roleAppService;
}
```

2. **Replace UserManager calls:**

**Before:**
```csharp
var user = new ApplicationUser { UserName = email, Email = email };
var result = await _userManager.CreateAsync(user, password);
await _userManager.AddToRoleAsync(user, "TenantAdmin");
```

**After:**
```csharp
var createDto = new IdentityUserCreateDto
{
    UserName = email,
    Email = email,
    Password = password,
    RoleNames = new[] { "TenantAdmin" }
};
var userDto = await _userAppService.CreateAsync(createDto);
```

3. **Update Tenant Context Switching:**
```csharp
// Use ABP's ICurrentTenant.Change() for tenant context
using (_currentTenant.Change(tenantId))
{
    var userDto = await _userAppService.CreateAsync(createDto);
}
```

**Testing Checklist:**
- [ ] User registration works
- [ ] User login works
- [ ] Role assignment works
- [ ] Tenant context switching works
- [ ] Password reset works
- [ ] Email verification works

---

#### 1.2 Replace TenantService with ITenantAppService

**Files to Update:**
- `Controllers/TrialController.cs` (Register method)
- `Controllers/Api/TenantsApiController.cs`
- `Controllers/PlatformTenantsController.cs`
- `Services/Implementations/OwnerTenantService.cs`

**Migration Steps:**

1. **Inject ITenantAppService:**
```csharp
private readonly ITenantAppService _tenantAppService;
private readonly ITenantManager _tenantManager;

public TrialController(
    ITenantAppService tenantAppService,
    ITenantManager tenantManager,
    // ... other dependencies
)
```

2. **Replace TenantService.CreateTenantAsync:**

**Before:**
```csharp
var tenant = await _tenantService.CreateTenantAsync(
    organizationName, 
    adminEmail, 
    tenantSlug
);
```

**After:**
```csharp
var createDto = new TenantCreateDto
{
    Name = tenantSlug,
    AdminEmailAddress = adminEmail,
    AdminPassword = password,
    // ABP will auto-create admin user
};
var tenantDto = await _tenantAppService.CreateAsync(createDto);
```

3. **Update Tenant Queries:**

**Before:**
```csharp
var tenant = await _unitOfWork.Tenants
    .Query()
    .FirstOrDefaultAsync(t => t.TenantSlug == slug);
```

**After:**
```csharp
var tenantDto = await _tenantAppService.GetByNameAsync(slug);
// Or use ITenantRepository for complex queries
var tenant = await _tenantRepository.FindByNameAsync(slug);
```

**Testing Checklist:**
- [ ] Tenant creation works
- [ ] Tenant lookup by slug works
- [ ] Tenant activation works
- [ ] Tenant admin user auto-created
- [ ] Tenant context isolation works

---

### **Phase 2: Data Access Layer (Medium Priority)**
**Impact:** All services using IUnitOfWork  
**Risk:** High (affects all CRUD operations)  
**Timeline:** 3-5 days

#### 2.1 Migrate from IUnitOfWork to ABP IRepository<T>

**Files to Update:**
- All service implementations using `IUnitOfWork`
- `Services/Implementations/ControlService.cs`
- `Services/Implementations/RiskService.cs`
- `Services/Implementations/EvidenceService.cs`
- `Services/Implementations/AssessmentService.cs`
- `Services/Implementations/AuditService.cs`
- `Services/Implementations/PolicyService.cs`
- `Services/Implementations/PlanService.cs`
- And 20+ more services...

**Migration Strategy:**

1. **Create Adapter Pattern (Temporary):**
```csharp
// Keep IUnitOfWork interface but implement using ABP repositories
public class AbpUnitOfWorkAdapter : IUnitOfWork
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private IUnitOfWork? _currentUnitOfWork;

    public AbpUnitOfWorkAdapter(IUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    public IGenericRepository<T> GetRepository<T>() where T : class, IEntity
    {
        _currentUnitOfWork ??= _unitOfWorkManager.Begin();
        var abpRepo = _currentUnitOfWork.ServiceProvider
            .GetRequiredService<IRepository<T, Guid>>();
        return new AbpRepositoryAdapter<T>(abpRepo);
    }

    public async Task SaveChangesAsync()
    {
        if (_currentUnitOfWork != null)
        {
            await _currentUnitOfWork.CompleteAsync();
        }
    }
}
```

2. **Gradual Migration (Service by Service):**

**Before:**
```csharp
public class ControlService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ControlDto> GetByIdAsync(Guid id)
    {
        var control = await _unitOfWork.Controls.GetByIdAsync(id);
        return _mapper.Map<ControlDto>(control);
    }
}
```

**After:**
```csharp
public class ControlService
{
    private readonly IRepository<Control, Guid> _controlRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public async Task<ControlDto> GetByIdAsync(Guid id)
    {
        using var uow = _unitOfWorkManager.Begin();
        var control = await _controlRepository.GetAsync(id);
        await uow.CompleteAsync();
        return _mapper.Map<ControlDto>(control);
    }
}
```

**Testing Checklist:**
- [ ] All CRUD operations work
- [ ] Tenant filtering works
- [ ] Workspace filtering works
- [ ] Transactions work correctly
- [ ] Query performance acceptable

---

### **Phase 3: Settings & Configuration (Low Priority)**
**Impact:** Application settings management  
**Risk:** Low (non-critical functionality)  
**Timeline:** 1-2 days

#### 3.1 Replace Custom Settings with ISettingManager

**Files to Update:**
- Any service using `IConfiguration` for app settings
- Settings controllers/APIs

**Migration Steps:**

1. **Use ISettingManager for tenant-specific settings:**
```csharp
private readonly ISettingManager _settingManager;

// Get setting
var value = await _settingManager.GetOrNullAsync(
    "App.Settings.MaxUsers", 
    TenantManagementConsts.DefaultTenantName
);

// Set setting
await _settingManager.SetForTenantAsync(
    tenantId,
    "App.Settings.MaxUsers",
    "100"
);
```

2. **Use ISettingProvider for default values:**
```csharp
private readonly ISettingProvider _settingProvider;

var maxUsers = await _settingProvider.GetOrNullAsync("App.Settings.MaxUsers");
```

**Testing Checklist:**
- [ ] Settings read correctly
- [ ] Settings write correctly
- [ ] Tenant-specific settings work
- [ ] Default values work

---

### **Phase 4: Account & Authentication (Medium Priority)**
**Impact:** Login/register flows  
**Risk:** Medium (affects user authentication)  
**Timeline:** 2-3 days

#### 4.1 Use IAccountAppService for Authentication

**Files to Update:**
- `Controllers/AccountController.cs`
- `Controllers/RegisterController.cs`

**Migration Steps:**

1. **Replace Login method:**
```csharp
private readonly IAccountAppService _accountAppService;

public async Task<IActionResult> Login(LoginInput input)
{
    var loginDto = new LoginInput
    {
        UserNameOrEmailAddress = input.Email,
        Password = input.Password,
        RememberMe = input.RememberMe
    };

    var result = await _accountAppService.LoginAsync(loginDto);
    
    if (result.Result == LoginResultType.Success)
    {
        // ABP handles sign-in automatically
        return RedirectToLocal(input.ReturnUrl);
    }
    
    // Handle errors
    ModelState.AddModelError("", "Invalid login attempt");
    return View(input);
}
```

2. **Replace Register method:**
```csharp
public async Task<IActionResult> Register(RegisterInput input)
{
    var registerDto = new RegisterInput
    {
        UserName = input.Email,
        EmailAddress = input.Email,
        Password = input.Password,
        AppName = "GrcMvc"
    };

    await _accountAppService.RegisterAsync(registerDto);
    return RedirectToAction("Login");
}
```

**Testing Checklist:**
- [ ] Login works
- [ ] Register works
- [ ] Password reset works
- [ ] Email verification works
- [ ] Two-factor authentication works (if enabled)

---

### **Phase 5: OpenIddict OAuth/OIDC (Low Priority)**
**Impact:** SSO and API authentication  
**Risk:** Low (optional feature)  
**Timeline:** 2-3 days

#### 5.1 Activate OpenIddict Services

**Files to Create/Update:**
- `Controllers/Api/OAuthController.cs` (new)
- `Services/Implementations/OAuthManagementService.cs` (new)

**Implementation Steps:**

1. **Create OAuth Application:**
```csharp
private readonly IOpenIddictApplicationManager _applicationManager;

public async Task<IActionResult> CreateApplication(CreateApplicationDto dto)
{
    var application = new OpenIddictApplicationDescriptor
    {
        ClientId = dto.ClientId,
        DisplayName = dto.DisplayName,
        Type = ClientType.Confidential,
        Permissions =
        {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code
        },
        RedirectUris = { new Uri(dto.RedirectUri) }
    };

    await _applicationManager.CreateAsync(application);
    return Ok();
}
```

2. **Manage Tokens:**
```csharp
private readonly IOpenIddictTokenManager _tokenManager;

public async Task<IActionResult> RevokeToken(string tokenId)
{
    var token = await _tokenManager.FindByIdAsync(tokenId);
    if (token != null)
    {
        await _tokenManager.DeleteAsync(token);
    }
    return Ok();
}
```

**Testing Checklist:**
- [ ] OAuth application creation works
- [ ] Authorization code flow works
- [ ] Token generation works
- [ ] Token validation works
- [ ] Token revocation works

---

## 📋 Implementation Checklist

### **Phase 1: Identity & Tenant (Priority 1)**
- [ ] **1.1** Replace UserManager in AccountController
- [ ] **1.2** Replace UserManager in TrialController
- [ ] **1.3** Replace UserManager in RegisterController
- [ ] **1.4** Replace UserManager in OnboardingController
- [ ] **1.5** Replace UserManager in OwnerTenantService
- [ ] **1.6** Replace TenantService with ITenantAppService in TrialController
- [ ] **1.7** Replace TenantService in TenantsApiController
- [ ] **1.8** Replace TenantService in PlatformTenantsController
- [ ] **1.9** Test all user operations
- [ ] **1.10** Test all tenant operations

### **Phase 2: Data Access (Priority 2)**
- [ ] **2.1** Create AbpUnitOfWorkAdapter
- [ ] **2.2** Migrate ControlService to IRepository
- [ ] **2.3** Migrate RiskService to IRepository
- [ ] **2.4** Migrate EvidenceService to IRepository
- [ ] **2.5** Migrate AssessmentService to IRepository
- [ ] **2.6** Migrate AuditService to IRepository
- [ ] **2.7** Migrate PolicyService to IRepository
- [ ] **2.8** Migrate PlanService to IRepository
- [ ] **2.9** Migrate remaining services (15+ services)
- [ ] **2.10** Test all CRUD operations
- [ ] **2.11** Test tenant isolation
- [ ] **2.12** Test workspace isolation

### **Phase 3: Settings (Priority 3)**
- [ ] **3.1** Identify all settings usage
- [ ] **3.2** Replace with ISettingManager
- [ ] **3.3** Test settings read/write
- [ ] **3.4** Test tenant-specific settings

### **Phase 4: Account (Priority 2)**
- [ ] **4.1** Replace AccountController.Login
- [ ] **4.2** Replace AccountController.Register
- [ ] **4.3** Replace AccountController.ResetPassword
- [ ] **4.4** Test authentication flows

### **Phase 5: OpenIddict (Priority 3)**
- [ ] **5.1** Create OAuth management service
- [ ] **5.2** Create OAuth API controller
- [ ] **5.3** Test OAuth flows
- [ ] **5.4** Test token management

---

## 🚨 Risk Mitigation

### **Backward Compatibility**
- Keep custom services as adapters initially
- Gradual migration service-by-service
- Feature flags for ABP vs custom implementations

### **Testing Strategy**
- Unit tests for each migrated service
- Integration tests for critical flows
- Manual testing of all user-facing features

### **Rollback Plan**
- Keep custom implementations until migration verified
- Use feature flags to switch between implementations
- Database migration can be rolled back if needed

---

## 📊 Success Metrics

### **Phase 1 Success Criteria**
- ✅ All user management operations use ABP services
- ✅ All tenant management operations use ABP services
- ✅ Zero UserManager dependencies in controllers
- ✅ Zero TenantService dependencies in controllers

### **Phase 2 Success Criteria**
- ✅ All services use ABP IRepository<T>
- ✅ Zero IUnitOfWork dependencies
- ✅ Zero IGenericRepository dependencies
- ✅ All queries respect tenant/workspace filters

### **Phase 3-5 Success Criteria**
- ✅ Settings use ISettingManager
- ✅ Authentication uses IAccountAppService
- ✅ OAuth uses OpenIddict services

---

## 🎯 Next Immediate Steps

1. **Start with Phase 1.1:** Replace UserManager in TrialController
   - This is the most critical path (trial registration)
   - Low risk (can test in isolation)
   - High impact (enables tenant creation flow)

2. **Create Migration Branch:**
   ```bash
   git checkout -b feature/abp-services-activation
   ```

3. **Begin TrialController Migration:**
   - Inject `IIdentityUserAppService` and `ITenantAppService`
   - Replace `UserManager` calls
   - Replace `TenantService` calls
   - Test trial registration end-to-end

4. **Document Changes:**
   - Update `ABP_PLAN_VS_ACTUAL_EVALUATION.md` with migration progress
   - Create migration log for each service replaced

---

**Last Updated:** 2026-01-19  
**Status:** 📋 **PLANNING COMPLETE** | 🚀 **READY TO START PHASE 1**
