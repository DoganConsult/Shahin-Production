# 🔄 Tenant & Tenant Admin Creation Process - Step-by-Step Guide

**Generated:** 2026-01-19  
**Platform:** Shahin AI GRC Platform (ABP Framework)

---

## 📋 Overview

This document explains the **step-by-step process** for creating tenants and tenant admin users in both:
1. **Current Implementation** (Custom Services)
2. **ABP Way** (Recommended - Using ABP Services)

---

## 🎯 Scenario: Trial Signup via API

This is the most common scenario: a user signs up for a trial, and the system creates both the tenant and admin user automatically.

---

## 🔧 Current Implementation Process (Custom)

### **Step-by-Step Flow:**

```
1. API Request Received
   ↓
2. Validate Input (email, password, company name)
   ↓
3. Check for Existing Tenant/User
   ↓
4. Create Tenant Entity
   ↓
5. Create ApplicationUser (ASP.NET Identity)
   ↓
6. Assign TenantAdmin Role
   ↓
7. Create TenantUser Link
   ↓
8. Save All Changes
   ↓
9. Send Welcome Email
   ↓
10. Return Success Response
```

### **Detailed Code Flow:**

```csharp
// Step 1: API Endpoint Receives Request
[HttpPost("/api/agent/tenant/create")]
public async Task<IActionResult> CreateTenant([FromBody] TrialRequestDto dto)
{
    // Step 2: Call TrialLifecycleService
    var result = await _trialService.ProvisionTrialAsync(signupId, password);
}

// Step 3: Inside TrialLifecycleService.ProvisionTrialAsync()
public async Task<TrialProvisionResult> ProvisionTrialAsync(Guid signupId, string password)
{
    // Step 3.1: Load signup record
    var signup = await _context.TrialSignups.FindAsync(signupId);
    
    // Step 3.2: Generate tenant slug
    var tenantSlug = GenerateSlug(signup.CompanyName ?? "company");
    
    // Step 3.3: Create Tenant entity (CUSTOM)
    var tenant = new Tenant
    {
        Id = Guid.NewGuid(),
        OrganizationName = signup.CompanyName ?? "Company",
        TenantSlug = tenantSlug,
        Email = signup.Email,
        AdminEmail = signup.Email,
        Status = "trial",
        IsTrial = true,
        TrialStartsAt = DateTime.UtcNow,
        TrialEndsAt = DateTime.UtcNow.AddDays(7),
        OnboardingStatus = "NOT_STARTED",
        CreatedDate = DateTime.UtcNow,
        CreatedBy = "system"
    };
    _context.Tenants.Add(tenant);  // ⚠️ Custom entity, not ABP
    
    // Step 3.4: Create ApplicationUser (ASP.NET Identity)
    var applicationUser = new ApplicationUser
    {
        Id = Guid.NewGuid().ToString(),
        UserName = signup.Email,
        Email = signup.Email,
        EmailConfirmed = true,
        FirstName = signup.FirstName ?? "",
        LastName = signup.LastName ?? "",
        IsActive = true,
        MustChangePassword = false,
        CreatedDate = DateTime.UtcNow
    };
    
    // Step 3.5: Create user with password (UserManager)
    var createUserResult = await _userManager.CreateAsync(applicationUser, password);
    if (!createUserResult.Succeeded)
    {
        // Rollback tenant creation
        _context.Tenants.Remove(tenant);
        return new TrialProvisionResult { Success = false, Message = "Failed to create user" };
    }
    
    // Step 3.6: Assign TenantAdmin role
    var roleResult = await _userManager.AddToRoleAsync(applicationUser, "TenantAdmin");
    
    // Step 3.7: Create TenantUser link (CUSTOM)
    var tenantUser = new TenantUser
    {
        Id = Guid.NewGuid(),
        TenantId = tenant.Id,
        UserId = applicationUser.Id,
        RoleCode = "TenantAdmin",
        TitleCode = "TENANT_ADMIN",
        Status = "Active",
        InvitedAt = DateTime.UtcNow,
        ActivatedAt = DateTime.UtcNow,
        InvitedBy = "system",
        CreatedDate = DateTime.UtcNow
    };
    _context.TenantUsers.Add(tenantUser);  // ⚠️ Custom linking table
    
    // Step 3.8: Update signup record
    signup.Status = "provisioned";
    signup.TenantId = tenant.Id;
    signup.ProvisionedAt = DateTime.UtcNow;
    
    // Step 3.9: Save all changes (single transaction)
    await _context.SaveChangesAsync();
    
    // Step 3.10: Send welcome email
    await _emailService.SendWelcomeEmailAsync(applicationUser.Email, tenant.OrganizationName);
    
    return new TrialProvisionResult 
    { 
        Success = true, 
        TenantId = tenant.Id,
        UserId = applicationUser.Id 
    };
}
```

### **Database Changes (Current Implementation):**

```sql
-- Transaction 1: Create Tenant
BEGIN TRANSACTION;
INSERT INTO Tenants (Id, OrganizationName, TenantSlug, Email, AdminEmail, Status, IsTrial, TrialStartsAt, TrialEndsAt, OnboardingStatus, CreatedDate, CreatedBy)
VALUES (
    '550e8400-e29b-41d4-a716-446655440000',  -- tenant.Id
    'Acme Corp',                              -- OrganizationName
    'acme-corp',                              -- TenantSlug
    'admin@acme.com',                         -- Email
    'admin@acme.com',                         -- AdminEmail
    'trial',                                  -- Status
    true,                                     -- IsTrial
    '2026-01-19 10:00:00',                   -- TrialStartsAt
    '2026-01-26 10:00:00',                   -- TrialEndsAt
    'NOT_STARTED',                            -- OnboardingStatus
    '2026-01-19 10:00:00',                   -- CreatedDate
    'system'                                  -- CreatedBy
);
COMMIT;

-- Transaction 2: Create ApplicationUser (in default tenant context)
BEGIN TRANSACTION;
INSERT INTO AspNetUsers (Id, UserName, Email, EmailConfirmed, FirstName, LastName, IsActive, MustChangePassword, CreatedDate, LastPasswordChangedAt)
VALUES (
    '660e8400-e29b-41d4-a716-446655440000',  -- applicationUser.Id (string)
    'admin@acme.com',                         -- UserName
    'admin@acme.com',                         -- Email
    true,                                     -- EmailConfirmed
    'John',                                   -- FirstName
    'Doe',                                    -- LastName
    true,                                     -- IsActive
    false,                                    -- MustChangePassword
    '2026-01-19 10:00:00',                   -- CreatedDate
    '2026-01-19 10:00:00'                    -- LastPasswordChangedAt
);
COMMIT;

-- Transaction 3: Assign Role
BEGIN TRANSACTION;
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (
    '660e8400-e29b-41d4-a716-446655440000',  -- UserId
    'TenantAdmin'                             -- RoleId (from AspNetRoles)
);
COMMIT;

-- Transaction 4: Create TenantUser Link (CUSTOM)
BEGIN TRANSACTION;
INSERT INTO TenantUsers (Id, TenantId, UserId, RoleCode, TitleCode, Status, InvitedAt, ActivatedAt, InvitedBy, CreatedDate)
VALUES (
    '770e8400-e29b-41d4-a716-446655440000',  -- tenantUser.Id
    '550e8400-e29b-41d4-a716-446655440000',  -- TenantId
    '660e8400-e29b-41d4-a716-446655440000',  -- UserId (ApplicationUser.Id)
    'TenantAdmin',                            -- RoleCode
    'TENANT_ADMIN',                           -- TitleCode
    'Active',                                 -- Status
    '2026-01-19 10:00:00',                   -- InvitedAt
    '2026-01-19 10:00:00',                   -- ActivatedAt
    'system',                                 -- InvitedBy
    '2026-01-19 10:00:00'                    -- CreatedDate
);
COMMIT;
```

### **Issues with Current Implementation:**

1. ❌ **No Tenant Context Isolation:** User is created in default tenant context, not tenant-specific
2. ❌ **Manual Linking Required:** Must manually create `TenantUser` link
3. ❌ **Multiple Transactions:** 4 separate database transactions (not atomic)
4. ❌ **Custom Entities:** Uses custom `Tenant` and `ApplicationUser` instead of ABP entities
5. ❌ **No ABP Integration:** Doesn't use ABP's multi-tenancy features

---

## ✅ ABP Way Process (Recommended)

### **Step-by-Step Flow:**

```
1. API Request Received
   ↓
2. Validate Input
   ↓
3. Call ITenantAppService.CreateAsync()
   ↓
   [ABP AUTOMATICALLY:]
   3.1. Creates Tenant (ABP entity)
   3.2. Switches to Tenant Context
   3.3. Creates Admin User (in tenant context)
   3.4. Assigns Admin Role
   3.5. Links User to Tenant
   ↓
4. Return Success Response
```

### **Detailed Code Flow (ABP Way):**

```csharp
// Step 1: API Endpoint Receives Request
[HttpPost("/api/agent/tenant/create")]
[AllowAnonymous]
public async Task<IActionResult> CreateTenantViaApi([FromBody] TenantCreateApiRequest request)
{
    // Step 2: Validate input (optional, ABP also validates)
    if (string.IsNullOrEmpty(request.OrganizationName) || 
        string.IsNullOrEmpty(request.AdminEmail) || 
        string.IsNullOrEmpty(request.Password))
    {
        return BadRequest(new { error = "Missing required fields" });
    }
    
    // Step 3: Call ABP's ITenantAppService (ONE CALL DOES EVERYTHING)
    var tenantDto = await _tenantAppService.CreateAsync(new TenantCreateDto
    {
        Name = request.OrganizationName,        // Tenant name
        AdminEmailAddress = request.AdminEmail,  // Admin email
        AdminPassword = request.Password        // Admin password
    });
    
    // ✅ ABP AUTOMATICALLY:
    // 1. Creates AbpTenants record
    // 2. Switches to tenant context using ICurrentTenant.Change(tenantId)
    // 3. Creates AbpUsers record (in tenant context)
    // 4. Creates AbpUserRoles record (assigns admin role)
    // 5. Sets tenant.AdminUserId automatically
    // ALL IN A SINGLE TRANSACTION!
    
    // Step 4: Return success response
    return Ok(new
    {
        status = "created",
        tenantId = tenantDto.Id,
        adminUserId = tenantDto.AdminUserId,
        redirectUrl = $"/onboarding/wizard/fast-start?tenantId={tenantDto.Id}"
    });
}
```

### **What Happens Inside `ITenantAppService.CreateAsync()`:**

```csharp
// ABP Framework Internal Flow (simplified)
public async Task<TenantDto> CreateAsync(TenantCreateDto input)
{
    // Step 1: Create tenant using ITenantManager
    var tenant = await _tenantManager.CreateAsync(input.Name);
    
    // Step 2: Set tenant properties
    tenant.SetConnectionString(input.ConnectionString); // Optional
    
    // Step 3: Save tenant to database
    await _tenantRepository.InsertAsync(tenant);
    
    // Step 4: Switch to tenant context
    using (_currentTenant.Change(tenant.Id))
    {
        // Step 5: Create admin user IN TENANT CONTEXT
        var adminUser = await _identityUserManager.CreateAsync(
            new IdentityUser(
                GuidGenerator.Create(),
                input.AdminEmailAddress,
                input.AdminEmailAddress,
                tenant.Id  // ✅ User is created in tenant context
            )
        );
        
        // Step 6: Set password
        await _identityUserManager.AddPasswordAsync(adminUser, input.AdminPassword);
        
        // Step 7: Assign admin role
        await _identityUserManager.AddToRoleAsync(adminUser, "admin"); // ABP default admin role
        
        // Step 8: Link admin to tenant
        tenant.SetAdminUserId(adminUser.Id);
        await _tenantRepository.UpdateAsync(tenant);
    }
    
    // Step 9: Return DTO
    return ObjectMapper.Map<Tenant, TenantDto>(tenant);
}
```

### **Database Changes (ABP Way):**

```sql
-- SINGLE TRANSACTION: All operations atomic
BEGIN TRANSACTION;

-- Step 1: Create ABP Tenant
INSERT INTO AbpTenants (Id, Name, AdminUserId, ExtraProperties, CreationTime, CreatorId)
VALUES (
    '550e8400-e29b-41d4-a716-446655440000',  -- tenant.Id (Guid)
    'Acme Corp',                              -- Name
    NULL,                                     -- AdminUserId (set later)
    '{"OnboardingStatus":"NOT_STARTED"}',    -- ExtraProperties (JSON)
    '2026-01-19 10:00:00',                   -- CreationTime
    NULL                                      -- CreatorId
);

-- Step 2: Switch Tenant Context (ABP handles this automatically)
-- ABP sets ICurrentTenant.Id = '550e8400-e29b-41d4-a716-446655440000'

-- Step 3: Create ABP User (IN TENANT CONTEXT)
INSERT INTO AbpUsers (Id, TenantId, UserName, Email, EmailConfirmed, Name, Surname, IsActive, CreationTime, CreatorId)
VALUES (
    '660e8400-e29b-41d4-a716-446655440000',  -- adminUser.Id (Guid)
    '550e8400-e29b-41d4-a716-446655440000',  -- TenantId (✅ User is tenant-scoped!)
    'admin@acme.com',                         -- UserName
    'admin@acme.com',                         -- Email
    true,                                     -- EmailConfirmed
    'John',                                   -- Name
    'Doe',                                    -- Surname
    true,                                     -- IsActive
    '2026-01-19 10:00:00',                   -- CreationTime
    NULL                                      -- CreatorId
);

-- Step 4: Create Password Hash
INSERT INTO AbpUserTokens (UserId, LoginProvider, Name, Value, TenantId)
VALUES (
    '660e8400-e29b-41d4-a716-446655440000',  -- UserId
    'Password',                               -- LoginProvider
    'Password',                               -- Name
    'hashed_password_value',                  -- Value (hashed)
    '550e8400-e29b-41d4-a716-446655440000'   -- TenantId
);

-- Step 5: Assign Admin Role
INSERT INTO AbpUserRoles (UserId, RoleId, TenantId)
VALUES (
    '660e8400-e29b-41d4-a716-446655440000',  -- UserId
    'admin',                                  -- RoleId (ABP default admin role)
    '550e8400-e29b-41d4-a716-446655440000'   -- TenantId
);

-- Step 6: Update Tenant with AdminUserId
UPDATE AbpTenants
SET AdminUserId = '660e8400-e29b-41d4-a716-446655440000'
WHERE Id = '550e8400-e29b-41d4-a716-446655440000';

COMMIT;  -- ✅ ALL IN ONE TRANSACTION!
```

### **Benefits of ABP Way:**

1. ✅ **Tenant Context Isolation:** User is automatically created in tenant context
2. ✅ **Automatic Linking:** ABP handles user-tenant relationship automatically
3. ✅ **Single Transaction:** All operations are atomic (all-or-nothing)
4. ✅ **ABP Entities:** Uses ABP's `Tenant` and `IdentityUser` entities
5. ✅ **Built-in Features:** Automatic audit logging, permission checks, etc.
6. ✅ **Less Code:** One method call instead of 10+ lines of code

---

## 🔄 Comparison: Current vs ABP

| **Aspect** | **Current Implementation** | **ABP Way** |
|------------|---------------------------|-------------|
| **Code Lines** | ~50 lines | ~10 lines |
| **Database Transactions** | 4 separate transactions | 1 atomic transaction |
| **Tenant Context** | ❌ User created in default context | ✅ User created in tenant context |
| **Entity Types** | Custom `Tenant`, `ApplicationUser` | ABP `Tenant`, `IdentityUser` |
| **Manual Linking** | ✅ Must create `TenantUser` link | ❌ Automatic via `TenantId` |
| **Error Handling** | Manual rollback logic | ✅ Automatic rollback |
| **Audit Logging** | Manual `IAuditEventService` calls | ✅ Automatic via ABP |
| **Permission Checks** | Manual authorization | ✅ Automatic via ABP |

---

## 🎯 Next Steps

1. **Migrate `TrialLifecycleService`** to use `ITenantAppService.CreateAsync()`
2. **Update API Endpoints** to use ABP services
3. **Test** all creation flows with ABP services
4. **Remove** custom `ITenantService` after migration

---

**Last Updated:** 2026-01-19  
**Status:** ✅ ABP services available, migration in progress
