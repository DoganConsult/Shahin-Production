# Tenant & Tenant Admin Creation in ABP Framework

This document outlines how tenants and tenant admin users are created in the GRC platform, including the different flows and their ABP multi-tenancy integration.

---

## Summary Table

| **Flow** | **Entry Point** | **Tenant Creation** | **Admin User Creation** | **Role Assignment** | **FirstAdminUserId** | **Multi-Tenancy Context** | **ABP Services Used** |
|----------|----------------|---------------------|------------------------|---------------------|---------------------|-------------------------|----------------------|
| **Trial Signup** | `POST /api/trial/provision` | ✅ Direct `DbContext`<br>`_context.Tenants.Add(tenant)` | ✅ `IdentityUserManager.CreateAsync()`<br>Creates `ApplicationUser` | ✅ `AddToRoleAsync("TenantAdmin")` | ✅ Set before role assignment<br>`tenant.FirstAdminUserId = applicationUser.Id` | ⚠️ Uses direct `DbContext`<br>(bypasses tenant-aware context) | `UserManager<IdentityUser>`<br>`RoleManager<IdentityRole>` |
| **Owner-Generated Tenant** | `POST /api/owner/tenants/{id}/generate-admin` | ❌ Tenant must exist<br>(created separately) | ✅ `IdentityUserManager.CreateAsync()`<br>Generates secure password | ✅ `AddToRoleAsync("Admin")`<br>Creates role if missing | ✅ Set via `TenantUser` record<br>`tenant.AdminAccountGenerated = true` | ✅ Uses `IUnitOfWork`<br>(tenant-aware) | `UserManager<IdentityUser>`<br>`RoleManager<IdentityRole>`<br>`IUnitOfWork` |
| **Platform Admin Creates Tenant** | `POST /api/admin/tenants`<br>via `TenantService.CreateTenantAsync()` | ✅ Direct `DbContext`<br>`dbContext.Tenants.Add(tenant)` | ❌ **Not created automatically**<br>Admin must be created separately | ❌ N/A | ❌ Not set automatically | ⚠️ Uses direct `DbContext`<br>Then calls `ProvisionTenantAsync()` | Custom `TenantService`<br>`IProvisioningService` |
| **User Registration** | `POST /Account/Register`<br>via `RegisterController` | ❌ Tenant must exist<br>(user registers with existing tenant) | ✅ `IdentityUserManager.CreateAsync()`<br>No password (set via verification) | ❌ Not assigned immediately<br>Status = "PendingVerification" | ✅ Set if `tenant.FirstAdminUserId` is null<br>`tenant.FirstAdminUserId = user.Id` | ✅ Uses tenant-aware `DbContext`<br>(via `ICurrentTenant`) | `UserManager<IdentityUser>`<br>`ICurrentTenant` |

---

## Detailed Flow Analysis

### 1. Trial Signup Flow (`TrialLifecycleService.ProvisionTrialAsync`)

**Purpose**: Self-service trial provisioning for new organizations.

**Steps**:
1. Validates `TrialSignup` record exists and is in "pending" status
2. **Creates Tenant**:
   - Direct `DbContext` insertion (not tenant-aware)
   - Generates unique `tenantSlug` from organization name
   - Sets `Status = "trial"`, `TrialEndsAt = DateTime.UtcNow.AddDays(14)`
3. **Creates ApplicationUser**:
   - Uses `UserManager<IdentityUser>.CreateAsync(user, password)`
   - Email confirmed automatically (`EmailConfirmed = true`)
   - Sets `MustChangePassword = false` (trial users keep provided password)
4. **Assigns Role**:
   - Calls `AddToRoleAsync(applicationUser, "TenantAdmin")`
   - Role must exist (created by seed data)
5. **Links to Tenant**:
   - Sets `tenant.FirstAdminUserId = applicationUser.Id`
   - Creates `TenantUser` record with `RoleCode = "TenantAdmin"`, `Status = "Active"`
6. **Updates Signup Record**:
   - Sets `signup.Status = "provisioned"`, `signup.TenantId = tenant.Id`

**ABP Multi-Tenancy**: ⚠️ **Bypasses ABP's tenant-aware context** - uses direct `DbContext` for tenant creation, then switches context for user creation.

**File**: `src/GrcMvc/Services/Implementations/TrialLifecycleService.cs` (lines 130-280)

---

### 2. Owner-Generated Tenant Flow (`OwnerTenantService.GenerateTenantAdminAccountAsync`)

**Purpose**: Platform admin creates full-feature tenant and generates secure admin credentials.

**Steps**:
1. Validates tenant exists and `AdminAccountGenerated = false`
2. **Tenant Creation**: ❌ **Not done here** - tenant must be created separately via `TenantService.CreateTenantAsync()`
3. **Generates Admin User**:
   - Generates unique username: `{tenantSlug}_admin_{random}`
   - Generates secure 16-character password
   - Uses `UserManager<IdentityUser>.CreateAsync(user, password)`
   - Sets `EmailConfirmed = true`
4. **Assigns Role**:
   - Ensures "Admin" role exists (creates if missing)
   - Calls `AddToRoleAsync(user, "Admin")`
5. **Creates TenantUser**:
   - `RoleCode = "Admin"`, `TitleCode = "TENANT_ADMIN"`
   - Sets `MustChangePasswordOnFirstLogin = true`
   - Sets `CredentialExpiresAt` (default 14 days)
   - Marks `IsOwnerGenerated = true`
6. **Updates Tenant**:
   - Sets `tenant.AdminAccountGenerated = true`
   - Sets `tenant.AdminAccountGeneratedAt = DateTime.UtcNow`
   - Sets `tenant.CredentialExpiresAt = expirationDate`
7. **Audit Trail**:
   - Creates `OwnerTenantCreation` record
   - Logs `TenantAdminAccountGenerated` event

**ABP Multi-Tenancy**: ✅ **Uses tenant-aware context** - all operations use `IUnitOfWork` which respects `ICurrentTenant`.

**File**: `src/GrcMvc/Services/Implementations/OwnerTenantService.cs` (lines 100-250)

---

### 3. Platform Admin Creates Tenant (`TenantService.CreateTenantAsync`)

**Purpose**: Platform admin manually creates a tenant (without auto-generating admin).

**Steps**:
1. Uses **direct `DbContext`** (bypasses tenant-aware context)
2. **Creates Tenant**:
   - Validates `tenantSlug` uniqueness
   - Creates `Tenant` entity with `Status = "pending"`
3. **Calls Provisioning Service**:
   - Immediately calls `_provisioningService.ProvisionTenantAsync(tenant.Id)`
   - This sets up tenant's isolated database (if using DB-per-tenant)
4. **Sends Activation Email**:
   - Email sent to `adminEmail` with activation link
5. **Admin User Creation**: ❌ **Not created automatically**
   - Admin must be created separately via:
     - `OwnerTenantService.GenerateTenantAdminAccountAsync()` (recommended)
     - Manual registration by user clicking activation link
     - Platform admin creating user via UI

**ABP Multi-Tenancy**: ⚠️ **Bypasses tenant-aware context** for tenant creation, then provisions tenant database.

**File**: `src/GrcMvc/Services/Implementations/TenantService.cs` (lines 50-150)

---

### 4. User Registration Flow (`RegisterController.Register`)

**Purpose**: User self-registers with an existing tenant (via email domain or invitation).

**Steps**:
1. Validates tenant exists (by email domain or invitation token)
2. **Creates ApplicationUser**:
   - Uses `UserManager<IdentityUser>.CreateAsync(user)` (no password)
   - Password set via email verification link
   - Sets `EmailConfirmed = false`
3. **Links to Tenant**:
   - If `tenant.FirstAdminUserId` is null, sets `tenant.FirstAdminUserId = user.Id`
   - Creates `TenantUser` with `Status = "PendingVerification"`
4. **Role Assignment**: ❌ **Not assigned immediately**
   - Role assigned after email verification
   - Typically becomes "TenantAdmin" if `FirstAdminUserId` was set

**ABP Multi-Tenancy**: ✅ **Uses tenant-aware context** - `ICurrentTenant` is set before user creation.

**File**: `src/GrcMvc/Controllers/RegisterController.cs` (lines 200-250)

---

## ABP Multi-Tenancy Integration

### Current Implementation Status

| **Component** | **ABP Pattern** | **Status** | **Notes** |
|--------------|----------------|-----------|-----------|
| **Tenant Entity** | `Volo.Abp.MultiTenancy.ITenant` | ✅ Implemented | Custom `Tenant` entity with `TenantId` |
| **Tenant Context** | `ICurrentTenant` | ✅ Used | Set via middleware or `ICurrentTenant.Change()` |
| **Tenant-Aware DbContext** | `MultiTenantDbContext` | ⚠️ Partial | Some services use direct `DbContext` |
| **Tenant Repository** | `ITenantRepository` | ❌ Not used | Direct `DbContext.Tenants.Add()` |
| **User-Tenant Linking** | `TenantUser` entity | ✅ Custom | Custom `TenantUser` table (not ABP's built-in) |
| **Role Assignment** | `IdentityUserManager` | ✅ ABP | Uses ASP.NET Core Identity (ABP-compatible) |

### Recommended ABP Pattern (Not Currently Used)

ABP provides `ITenantAppService.CreateAsync()` which can automate tenant + admin creation:

```csharp
// ABP's built-in approach (not currently used)
var tenant = await _tenantAppService.CreateAsync(new TenantCreateDto
{
    Name = organizationName,
    AdminEmailAddress = adminEmail,
    AdminPassword = password
});
// This automatically:
// 1. Creates tenant
// 2. Creates admin user
// 3. Assigns default role
// 4. Links user to tenant
```

**Why Not Used**: Custom business logic requirements (trial periods, onboarding flow, custom roles, audit trails).

---

## Database Schema

### Tenant Table
```sql
Tenants (
    Id UUID PRIMARY KEY,
    OrganizationName VARCHAR(255),
    TenantSlug VARCHAR(100) UNIQUE,
    AdminEmail VARCHAR(255),
    FirstAdminUserId VARCHAR(255),  -- Links to ApplicationUser.Id
    Status VARCHAR(50),             -- pending, active, trial, suspended
    TrialEndsAt TIMESTAMP,
    AdminAccountGenerated BOOLEAN,
    AdminAccountGeneratedAt TIMESTAMP,
    CreatedDate TIMESTAMP
)
```

### ApplicationUser Table (ASP.NET Identity)
```sql
AspNetUsers (
    Id VARCHAR(255) PRIMARY KEY,     -- UUID as string
    UserName VARCHAR(256),
    Email VARCHAR(256),
    EmailConfirmed BOOLEAN,
    FirstName VARCHAR(255),
    LastName VARCHAR(255),
    IsActive BOOLEAN,
    MustChangePassword BOOLEAN,
    CreatedDate TIMESTAMP
)
```

### TenantUser Table (Custom Linking)
```sql
TenantUsers (
    Id UUID PRIMARY KEY,
    TenantId UUID REFERENCES Tenants(Id),
    UserId VARCHAR(255) REFERENCES AspNetUsers(Id),
    RoleCode VARCHAR(100),           -- TenantAdmin, Admin, ComplianceManager, etc.
    TitleCode VARCHAR(100),
    Status VARCHAR(50),              -- Active, PendingVerification, Suspended
    IsOwnerGenerated BOOLEAN,
    GeneratedByOwnerId UUID,
    CredentialExpiresAt TIMESTAMP,
    MustChangePasswordOnFirstLogin BOOLEAN,
    ActivatedAt TIMESTAMP,
    CreatedDate TIMESTAMP
)
```

### AspNetUserRoles Table (ASP.NET Identity)
```sql
AspNetUserRoles (
    UserId VARCHAR(255) REFERENCES AspNetUsers(Id),
    RoleId VARCHAR(255) REFERENCES AspNetRoles(Id),
    PRIMARY KEY (UserId, RoleId)
)
```

---

## Onboarding Flow Integration

All tenant creation flows can trigger onboarding:

1. **OnboardingRedirectMiddleware** checks:
   - `tenant.OnboardingStatus != "Completed"`
   - `user.Id == tenant.FirstAdminUserId`
   - Redirects to `/onboarding/wizard/fast-start`

2. **OnboardingWizard** tracks:
   - `WizardStatus` (NotStarted, InProgress, Completed)
   - `CurrentStep` (StepA through StepL)
   - Links to `TenantId`

**File**: `src/GrcMvc/Middleware/OnboardingRedirectMiddleware.cs`

---

## Security Considerations

1. **Password Generation**:
   - Trial: User-provided password (min 12 chars, complexity rules)
   - Owner-generated: System-generated 16-char secure password
   - Registration: Password set via email verification link

2. **Role Hierarchy**:
   - `TenantAdmin`: Tenant-level administrator (trial flow)
   - `Admin`: Full tenant admin (owner-generated flow)
   - `PlatformAdmin`: Cross-tenant administrator (system-level)

3. **First Login Behavior**:
   - Trial: No password change required
   - Owner-generated: `MustChangePasswordOnFirstLogin = true`
   - Registration: Password set during verification

---

## Missing / Recommended Improvements

1. **❌ Use ABP's `ITenantAppService`** for tenant creation (currently uses direct `DbContext`)
2. **❌ Use ABP's `ITenantRepository`** instead of direct `DbContext.Tenants`
3. **✅ Add Transaction Wrapping** for tenant + user creation (atomicity)
4. **✅ Standardize Role Names** (currently "TenantAdmin" vs "Admin" inconsistency)
5. **✅ Add Tenant Validation** before user creation (ensure tenant exists and is active)
6. **✅ Implement `ICurrentTenant.Change()`** consistently in all flows

---

## Related Files

- `src/GrcMvc/Services/Implementations/TrialLifecycleService.cs`
- `src/GrcMvc/Services/Implementations/OwnerTenantService.cs`
- `src/GrcMvc/Services/Implementations/TenantService.cs`
- `src/GrcMvc/Controllers/Api/TrialApiController.cs`
- `src/GrcMvc/Controllers/RegisterController.cs`
- `src/GrcMvc/Middleware/OnboardingRedirectMiddleware.cs`
- `src/GrcMvc/Models/Entities/Tenant.cs`
- `src/GrcMvc/Models/Entities/TenantUser.cs`
- `src/GrcMvc/Models/Entities/ApplicationUser.cs`
