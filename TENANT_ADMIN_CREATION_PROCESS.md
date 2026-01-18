# 🏢 Tenant & Admin Creation Process - Complete Guide

**Generated:** 2026-01-12  
**Platform:** Shahin AI GRC Platform (ABP Framework)  
**Status:** ✅ **4 Different Flows Available**

---

## 📊 Quick Reference Table

| **Method** | **Who Can Use** | **Entry Point** | **Creates Tenant?** | **Creates Admin?** | **Auto-Login?** | **Onboarding Redirect?** |
|------------|----------------|-----------------|---------------------|-------------------|----------------|------------------------|
| **1. Trial Signup** | Public (Self-Service) | `/trial` or `/api/trial/provision` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes → `/onboarding/wizard/fast-start` |
| **2. Platform Admin UI** | Platform Admin | `/admin/tenants/create` | ✅ Yes | ❌ No (separate step) | ❌ No | ⚠️ Manual redirect |
| **3. Owner-Generated** | Platform Owner | `/api/owner/tenants/{id}/generate-admin` | ❌ No (tenant must exist) | ✅ Yes | ❌ No | ⚠️ Manual redirect |
| **4. User Self-Registration** | Public (with tenant) | `/Account/Register` | ❌ No (tenant must exist) | ✅ Yes (if first) | ❌ No | ✅ Yes (if FirstAdminUserId) |

---

## 🔄 Method 1: Trial Signup (Self-Service) ⭐ RECOMMENDED

### **Purpose**
Public self-service trial registration. Creates tenant + admin + auto-login + onboarding redirect.

### **Entry Points**
- **UI:** `GET /trial` → Fill form → Submit
- **API:** `POST /api/trial/provision`

### **Process Flow**

```
1. User fills form:
   ├── Organization Name
   ├── Admin Email
   ├── Password (min 12 chars, complexity)
   └── Accept Terms ✅

2. System creates Tenant:
   ├── Generate unique TenantSlug
   ├── Status = "trial"
   ├── TrialEndsAt = Now + 14 days
   └── OnboardingStatus = "NotStarted"

3. System creates ApplicationUser:
   ├── UserName = Email
   ├── EmailConfirmed = true
   ├── MustChangePassword = false
   └── IsActive = true

4. System assigns role:
   └── AddToRoleAsync(user, "TenantAdmin")

5. System links to tenant:
   ├── tenant.FirstAdminUserId = user.Id
   └── Create TenantUser record (Status = "Active")

6. System auto-signs in:
   └── SignInManager.SignInAsync(user)

7. System redirects:
   └── /onboarding/wizard/fast-start
```

### **Implementation Files**
- `TrialLifecycleService.ProvisionTrialAsync()`
- `TrialApiController` or `TrialController`

### **Database Changes**
```sql
-- Tenant created
INSERT INTO Tenants (Id, OrganizationName, TenantSlug, AdminEmail, Status, FirstAdminUserId, OnboardingStatus)
VALUES (uuid, 'Acme Corp', 'acme-corp', 'admin@acme.com', 'trial', user_id, 'NotStarted');

-- User created
INSERT INTO AspNetUsers (Id, UserName, Email, EmailConfirmed, IsActive)
VALUES (uuid, 'admin@acme.com', 'admin@acme.com', true, true);

-- Role assigned
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (user_id, 'TenantAdmin');

-- TenantUser link
INSERT INTO TenantUsers (Id, TenantId, UserId, RoleCode, Status)
VALUES (uuid, tenant_id, user_id, 'TenantAdmin', 'Active');
```

---

## 🔄 Method 2: Platform Admin Creates Tenant (Manual)

### **Purpose**
Platform admin manually creates tenant for enterprise onboarding.

### **Entry Points**
- **UI:** `/admin/tenants/create`
- **API:** `POST /api/admin/tenants`

### **Process Flow**

```
1. Platform Admin fills form:
   ├── Organization Name
   ├── Admin Email
   ├── Tenant Slug (optional, auto-generated)
   └── Subscription Tier

2. System creates Tenant:
   ├── Status = "Pending"
   ├── Sends activation email
   └── OnboardingStatus = "NotStarted"

3. Admin User Creation: ❌ NOT AUTOMATIC
   └── Must be created separately via:
       ├── Method 3 (Owner-Generated)
       ├── Method 4 (User Self-Registration)
       └── Manual UI creation

4. First Login:
   └── User clicks activation link → Sets password → Redirects to onboarding
```

### **Implementation Files**
- `TenantService.CreateTenantAsync()`
- `PlatformAdminController.CreateTenant()`
- `TenantProvisioningService.ProvisionTenantAsync()`

### **Database Changes**
```sql
-- Only tenant created
INSERT INTO Tenants (Id, OrganizationName, TenantSlug, AdminEmail, Status, OnboardingStatus)
VALUES (uuid, 'Enterprise Corp', 'enterprise-corp', 'admin@enterprise.com', 'Pending', 'NotStarted');

-- Admin user NOT created yet
-- Must use Method 3 or 4 to create admin
```

---

## 🔄 Method 3: Owner-Generated Admin Account

### **Purpose**
Platform owner generates secure admin credentials for existing tenant.

### **Entry Points**
- **API:** `POST /api/owner/tenants/{tenantId}/generate-admin`

### **Process Flow**

```
1. Tenant must exist (created via Method 2)

2. System generates admin:
   ├── Username: admin-{tenant-slug}
   ├── Password: 16-char secure random
   ├── EmailConfirmed = true
   └── MustChangePasswordOnFirstLogin = true

3. System assigns role:
   └── AddToRoleAsync(user, "Admin")

4. System links to tenant:
   ├── tenant.AdminAccountGenerated = true
   ├── tenant.FirstAdminUserId = user.Id
   └── Create TenantUser (IsOwnerGenerated = true)

5. System returns credentials:
   └── Username + Password (must be delivered securely)

6. First Login:
   └── User logs in → Must change password → Redirects to onboarding
```

### **Implementation Files**
- `OwnerTenantService.GenerateTenantAdminAccountAsync()`
- `OwnerController.GenerateAdmin()`

### **Database Changes**
```sql
-- User created
INSERT INTO AspNetUsers (Id, UserName, Email, EmailConfirmed, MustChangePassword)
VALUES (uuid, 'admin-enterprise-corp', 'admin@enterprise.com', true, true);

-- Role assigned
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (user_id, 'Admin');

-- TenantUser link
INSERT INTO TenantUsers (Id, TenantId, UserId, RoleCode, IsOwnerGenerated, MustChangePasswordOnFirstLogin)
VALUES (uuid, tenant_id, user_id, 'Admin', true, true);

-- Tenant updated
UPDATE Tenants 
SET AdminAccountGenerated = true, FirstAdminUserId = user_id
WHERE Id = tenant_id;
```

---

## 🔄 Method 4: User Self-Registration (With Existing Tenant)

### **Purpose**
User registers with existing tenant (via email domain or invitation).

### **Entry Points**
- **UI:** `/Account/Register`
- **API:** `POST /Account/Register`

### **Process Flow**

```
1. User fills form:
   ├── Full Name
   ├── Email (must match tenant domain or invitation)
   ├── Phone Number
   └── Organization Name (if new tenant)

2. System validates:
   ├── Tenant exists (by email domain or invitation token)
   └── Email not already registered

3. System creates ApplicationUser:
   ├── EmailConfirmed = false
   ├── MustChangePassword = true
   └── No password yet (set via verification)

4. System links to tenant:
   ├── If tenant.FirstAdminUserId is NULL:
   │   └── tenant.FirstAdminUserId = user.Id (becomes admin)
   └── Create TenantUser (Status = "PendingVerification")

5. System sends verification email:
   └── Link: /Account/VerifyAndSetPassword?tenantId={id}&token={token}

6. User clicks link:
   ├── Sets password
   ├── EmailConfirmed = true
   ├── TenantUser.Status = "Active"
   └── If FirstAdminUserId → Redirects to onboarding
```

### **Implementation Files**
- `RegisterController.Register()`
- `AccountController.VerifyAndSetPassword()`

### **Database Changes**
```sql
-- User created (no password)
INSERT INTO AspNetUsers (Id, UserName, Email, EmailConfirmed, MustChangePassword)
VALUES (uuid, 'user@acme.com', 'user@acme.com', false, true);

-- TenantUser link (pending)
INSERT INTO TenantUsers (Id, TenantId, UserId, RoleCode, Status)
VALUES (uuid, tenant_id, user_id, 'TENANT_ADMIN', 'PendingVerification');

-- If first admin:
UPDATE Tenants 
SET FirstAdminUserId = user_id
WHERE Id = tenant_id AND FirstAdminUserId IS NULL;
```

---

## 🎯 Onboarding Redirect Logic

### **When Does Onboarding Redirect Happen?**

All methods can trigger onboarding redirect via `OnboardingRedirectMiddleware`:

```csharp
if (User.IsAuthenticated)
{
    var user = CurrentUserService.GetUser();
    var tenant = TenantContextService.CurrentTenant;

    // Check if user is first admin AND onboarding not completed
    if (user.Id == tenant.FirstAdminUserId 
        && tenant.OnboardingStatus != "Completed")
    {
        context.Response.Redirect("/onboarding/wizard/fast-start");
        return;
    }
}
```

### **Onboarding Status Flow**

```
NotStarted → InProgress → Completed
     ↓            ↓            ↓
  Redirect    Redirect    Dashboard
```

---

## 🔐 Role Hierarchy

| **Role** | **Scope** | **Can Create Tenants?** | **Can Create Users?** | **Used In** |
|----------|-----------|------------------------|----------------------|-------------|
| **PlatformAdmin** | System-wide | ✅ Yes | ✅ Yes (all tenants) | Platform management |
| **Admin** | Single tenant | ❌ No | ✅ Yes (own tenant) | Owner-generated flow |
| **TenantAdmin** | Single tenant | ❌ No | ✅ Yes (own tenant) | Trial signup flow |
| **ComplianceManager** | Single tenant | ❌ No | ❌ No | Post-onboarding |

---

## 📋 Step-by-Step: Create Tenant + Admin (All Methods)

### **Method 1: Trial Signup (Recommended for Public)**

```bash
# 1. User visits
GET /trial

# 2. User submits form
POST /trial
{
  "organizationName": "Acme Corp",
  "adminEmail": "admin@acme.com",
  "password": "SecurePass123!",
  "acceptTerms": true
}

# 3. System automatically:
#    ✅ Creates tenant
#    ✅ Creates admin user
#    ✅ Assigns TenantAdmin role
#    ✅ Auto-signs in
#    ✅ Redirects to /onboarding/wizard/fast-start
```

### **Method 2: Platform Admin (Manual 2-Step)**

```bash
# Step 1: Create tenant
POST /api/admin/tenants
{
  "organizationName": "Enterprise Corp",
  "adminEmail": "admin@enterprise.com",
  "subscriptionTier": "Enterprise"
}
# Result: Tenant created, activation email sent

# Step 2: Create admin (choose one):
# Option A: Owner generates
POST /api/owner/tenants/{tenantId}/generate-admin
# Result: Secure credentials generated

# Option B: User self-registers
POST /Account/Register
{
  "email": "admin@enterprise.com",
  "fullName": "Admin User"
}
# Result: Verification email sent, user sets password
```

### **Method 3: Owner-Generated (Tenant Must Exist)**

```bash
# Prerequisite: Tenant must exist (created via Method 2)

# Generate admin
POST /api/owner/tenants/{tenantId}/generate-admin
{
  "ownerId": "owner-uuid",
  "expirationDays": 14
}

# Response:
{
  "username": "admin-enterprise-corp",
  "password": "SecureRandom16Chars!",
  "expiresAt": "2026-01-26T00:00:00Z"
}
```

### **Method 4: Self-Registration (Tenant Must Exist)**

```bash
# Prerequisite: Tenant must exist

# User registers
POST /Account/Register
{
  "fullName": "John Doe",
  "email": "john@acme.com",
  "phoneNumber": "+1234567890",
  "organizationName": "Acme Corp"  # Optional if tenant exists
}

# System sends verification email
# User clicks link → Sets password → Redirects to onboarding (if first admin)
```

---

## 🗄️ Database Schema Reference

### **Tenants Table**
```sql
CREATE TABLE Tenants (
    Id UUID PRIMARY KEY,
    OrganizationName VARCHAR(255) NOT NULL,
    TenantSlug VARCHAR(100) UNIQUE NOT NULL,
    AdminEmail VARCHAR(255) NOT NULL,
    FirstAdminUserId VARCHAR(255),  -- Links to AspNetUsers.Id
    Status VARCHAR(50),             -- pending, active, trial, suspended
    SubscriptionTier VARCHAR(50),   -- Trial, Professional, Enterprise
    OnboardingStatus VARCHAR(50),    -- NotStarted, InProgress, Completed
    TrialEndsAt TIMESTAMP,
    AdminAccountGenerated BOOLEAN DEFAULT false,
    AdminAccountGeneratedAt TIMESTAMP,
    EmailVerificationTokenHash VARCHAR(255),
    EmailVerificationTokenExpiresAt TIMESTAMP,
    IsEmailVerified BOOLEAN DEFAULT false,
    CreatedDate TIMESTAMP NOT NULL,
    IsDeleted BOOLEAN DEFAULT false
);
```

### **AspNetUsers Table (ASP.NET Identity)**
```sql
CREATE TABLE AspNetUsers (
    Id VARCHAR(255) PRIMARY KEY,     -- UUID as string
    UserName VARCHAR(256) UNIQUE NOT NULL,
    Email VARCHAR(256),
    EmailConfirmed BOOLEAN DEFAULT false,
    FirstName VARCHAR(255),
    LastName VARCHAR(255),
    IsActive BOOLEAN DEFAULT true,
    MustChangePassword BOOLEAN DEFAULT false,
    PhoneNumber VARCHAR(50),
    CreatedDate TIMESTAMP NOT NULL
);
```

### **TenantUsers Table (Custom Linking)**
```sql
CREATE TABLE TenantUsers (
    Id UUID PRIMARY KEY,
    TenantId UUID REFERENCES Tenants(Id),
    UserId VARCHAR(255) REFERENCES AspNetUsers(Id),
    RoleCode VARCHAR(100),           -- TenantAdmin, Admin, ComplianceManager
    TitleCode VARCHAR(100),
    Status VARCHAR(50),               -- Active, PendingVerification, Suspended
    IsOwnerGenerated BOOLEAN DEFAULT false,
    GeneratedByOwnerId UUID,
    CredentialExpiresAt TIMESTAMP,
    MustChangePasswordOnFirstLogin BOOLEAN DEFAULT false,
    ActivatedAt TIMESTAMP,
    CreatedDate TIMESTAMP NOT NULL,
    IsDeleted BOOLEAN DEFAULT false,
    UNIQUE(TenantId, UserId)
);
```

---

## ✅ Best Practices

### **1. Use Method 1 (Trial Signup) For:**
- ✅ Public self-service registration
- ✅ Quick onboarding
- ✅ Automated tenant + admin creation
- ✅ Auto-login and redirect

### **2. Use Method 2 + 3 (Platform Admin + Owner-Generated) For:**
- ✅ Enterprise onboarding
- ✅ Secure credential generation
- ✅ Manual approval workflow
- ✅ Audit trail requirements

### **3. Use Method 4 (Self-Registration) For:**
- ✅ Existing tenant invites
- ✅ Email domain-based registration
- ✅ Password set via verification link

### **4. Always Set:**
- ✅ `tenant.FirstAdminUserId` when creating first admin
- ✅ `tenant.OnboardingStatus = "NotStarted"` for new tenants
- ✅ `TenantUser.Status = "Active"` after verification
- ✅ `TenantUser.RoleCode` based on flow (TenantAdmin vs Admin)

---

## 🚨 Common Issues & Solutions

### **Issue 1: Admin Not Created Automatically**
**Problem:** Method 2 creates tenant but no admin.  
**Solution:** Use Method 3 or 4 to create admin after tenant creation.

### **Issue 2: Onboarding Not Redirecting**
**Problem:** User logs in but doesn't see onboarding.  
**Solution:** Check:
- `tenant.FirstAdminUserId == user.Id`
- `tenant.OnboardingStatus != "Completed"`
- `OnboardingRedirectMiddleware` is registered in `Program.cs`

### **Issue 3: Role Not Assigned**
**Problem:** User created but no role.  
**Solution:** Ensure role exists:
```csharp
if (!await _roleManager.RoleExistsAsync("TenantAdmin"))
{
    await _roleManager.CreateAsync(new IdentityRole("TenantAdmin"));
}
```

### **Issue 4: Tenant Context Not Set**
**Problem:** User created but not linked to tenant.  
**Solution:** Use `ICurrentTenant.Change(tenantId)` before user creation:
```csharp
using (_currentTenant.Change(tenant.Id))
{
    await _userManager.CreateAsync(user, password);
}
```

---

## 📝 Related Files

- `src/GrcMvc/Services/Implementations/TrialLifecycleService.cs`
- `src/GrcMvc/Services/Implementations/TenantService.cs`
- `src/GrcMvc/Services/Implementations/OwnerTenantService.cs`
- `src/GrcMvc/Controllers/RegisterController.cs`
- `src/GrcMvc/Middleware/OnboardingRedirectMiddleware.cs`
- `src/GrcMvc/Models/Entities/Tenant.cs`
- `src/GrcMvc/Models/Entities/TenantUser.cs`
- `docs/production/tenant-admin-creation-table.md`

---

**Last Updated:** 2026-01-12  
**Status:** ✅ Production Ready
