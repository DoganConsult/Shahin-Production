# 🎯 Onboarding Wizard - Access Guide

**Generated:** 2026-01-19  
**Purpose:** Complete guide on where the onboarding wizard is, how users reach it, and when they get redirected

---

## 📍 Where is the Onboarding Wizard?

### **Primary URL Routes**

| **Route** | **Controller** | **Purpose** | **Authentication** |
|-----------|---------------|-------------|-------------------|
| `/OnboardingWizard/Index?tenantId={guid}` | `OnboardingWizardController.Index()` | Entry point - redirects to current step | ✅ Required (`[Authorize]`) |
| `/OnboardingWizard/StepA/{tenantId}` | `OnboardingWizardController.StepA()` | Step A: Organization Identity | ✅ Required |
| `/OnboardingWizard/StepB/{tenantId}` | `OnboardingWizardController.StepB()` | Step B: Industry & Sector | ✅ Required |
| `/OnboardingWizard/StepC/{tenantId}` | `OnboardingWizardController.StepC()` | Step C: Regulatory Frameworks | ✅ Required |
| `/OnboardingWizard/StepD/{tenantId}` | `OnboardingWizardController.StepD()` | Step D: Data Classification | ✅ Required |
| `/OnboardingWizard/StepE/{tenantId}` | `OnboardingWizardController.StepE()` | Step E: Risk Management | ✅ Required |
| `/OnboardingWizard/StepF/{tenantId}` | `OnboardingWizardController.StepF()` | Step F: Compliance Requirements | ✅ Required |
| `/OnboardingWizard/StepG/{tenantId}` | `OnboardingWizardController.StepG()` | Step G: Security Controls | ✅ Required |
| `/OnboardingWizard/StepH/{tenantId}` | `OnboardingWizardController.StepH()` | Step H: Business Continuity | ✅ Required |
| `/OnboardingWizard/StepI/{tenantId}` | `OnboardingWizardController.StepI()` | Step I: Vendor Management | ✅ Required |
| `/OnboardingWizard/StepJ/{tenantId}` | `OnboardingWizardController.StepJ()` | Step J: Incident Response | ✅ Required |
| `/OnboardingWizard/StepK/{tenantId}` | `OnboardingWizardController.StepK()` | Step K: Training & Awareness | ✅ Required |
| `/OnboardingWizard/StepL/{tenantId}` | `OnboardingWizardController.StepL()` | Step L: Review & Complete | ✅ Required |
| `/OnboardingWizard/Summary/{tenantId}` | `OnboardingWizardController.Summary()` | Wizard progress summary | ✅ Required |

### **File Locations**

- **Controller:** `Shahin-Jan-2026/src/GrcMvc/Controllers/OnboardingWizardController.cs`
- **Views:** `Shahin-Jan-2026/src/GrcMvc/Views/OnboardingWizard/`
- **Middleware:** `Shahin-Jan-2026/src/GrcMvc/Middleware/OnboardingRedirectMiddleware.cs`
- **Service:** `Shahin-Jan-2026/src/GrcMvc/Services/Implementations/OnboardingWizardService.cs`

---

## 🚪 How Users Reach the Onboarding Wizard

### **Entry Point 1: After Login (Automatic Redirect)**

**Location:** `AccountController.Login()` (lines 430-442)

**Flow:**
```
1. User logs in successfully
2. AccountController checks onboarding status
3. If onboarding NOT completed → Redirect to /OnboardingWizard/Index?tenantId={guid}
4. If onboarding completed → Redirect to dashboard
```

**Code:**
```csharp
// Check if user is first admin
if (user.Id == tenant.FirstAdminUserId && !OnboardingStatus.IsCompleted(tenant.OnboardingStatus))
{
    return RedirectToAction("Index", "OnboardingWizard", new { tenantId = tenant.Id });
}

// For all users: check onboarding status
if (!OnboardingStatus.IsCompleted(tenant.OnboardingStatus))
{
    return RedirectToAction("Index", "OnboardingWizard", new { tenantId = tenant.Id });
}
```

**When:** Immediately after successful login

---

### **Entry Point 2: Middleware Redirect (Automatic Guard)**

**Location:** `OnboardingRedirectMiddleware.cs` (line 74)

**Flow:**
```
1. User is authenticated
2. User navigates to any page (except excluded routes)
3. Middleware checks tenant onboarding status
4. If onboarding NOT completed → Redirect to /OnboardingWizard/Index?tenantId={guid}
5. If onboarding completed → Allow request to continue
```

**Excluded Routes (No Redirect):**
- `/onboarding/*` - Onboarding routes (avoid loop)
- `/account/*` - Authentication routes
- `/owner/*` - Platform admin routes
- `/api/*` - API routes
- `/css/*`, `/js/*`, `/lib/*` - Static files
- `/health` - Health checks
- `/landing/*`, `/pricing/*`, `/features/*` - Public pages
- `/error/*` - Error pages
- `/favicon.ico` - Favicon
- `/` or `/home` - Root/home

**When:** On every authenticated request (except excluded routes)

**Middleware Registration:** `Program.cs` line 1712
```csharp
app.UseMiddleware<GrcMvc.Middleware.OnboardingRedirectMiddleware>();
```

---

### **Entry Point 3: After Tenant Creation (Manual/Auto)**

**Location:** Various tenant creation controllers

**Scenarios:**

#### **A. Trial Registration**
- **Controller:** `TrialController` (if exists) or `RegisterController`
- **Flow:** User registers → Tenant created → Auto-sign-in → Redirect to onboarding
- **URL:** `/trial` or `/register`

#### **B. Platform Admin Creates Tenant**
- **Controller:** `AbpTenantController.Create()` or `TenantsApiController.CreateTenant()`
- **Flow:** Admin creates tenant → Admin user created → First login → Redirect to onboarding
- **URL:** `/admin/tenants/create` or `POST /api/tenants`

#### **C. Agent/Bot Creates Tenant**
- **Controller:** `AgentTenantApiController.CreateTenantAndAdmin()`
- **Flow:** Bot creates tenant + admin → Returns redirect URL
- **URL:** `POST /api/agent/tenant/create`
- **Response includes:** `redirectUrl: "/onboarding/wizard/fast-start?tenantId={guid}"`

#### **D. Subscription Payment Flow**
- **Controller:** `SubscribeController.ProcessPayment()`
- **Flow:** Payment successful → Tenant + user created → Auto-sign-in → Redirect to onboarding
- **URL:** `/subscribe/payment/process`

---

### **Entry Point 4: Direct URL Access (Manual)**

**URL:** `/OnboardingWizard/Index?tenantId={guid}`

**Requirements:**
- ✅ User must be authenticated (`[Authorize]` attribute)
- ✅ User must be tenant admin (checked in `CheckTenantAdminAuthAsync()`)
- ✅ Tenant must exist

**If Not Authenticated:**
- Redirects to: `/Account/TenantAdminLogin?tenantId={guid}&returnUrl={currentPath}`

---

## ⏰ When Users Get Redirected

### **Trigger 1: Onboarding Status Check**

**Condition:** `tenant.OnboardingStatus != "Completed"`

**Status Values:**
- `"NotStarted"` → Redirect to onboarding
- `"InProgress"` → Redirect to onboarding (resume at current step)
- `"Completed"` → No redirect (go to dashboard)

**Check Locations:**
1. `AccountController.Login()` - After login
2. `OnboardingRedirectMiddleware` - On every request
3. `OnboardingWizardController.Index()` - Entry point validation

---

### **Trigger 2: First Admin User**

**Condition:** `user.Id == tenant.FirstAdminUserId`

**Logic:**
- First admin user is always redirected to onboarding (if not completed)
- Other users in the tenant are also redirected if onboarding is incomplete

**Code:**
```csharp
// Priority check for first admin
if (user.Id == tenant.FirstAdminUserId && !OnboardingStatus.IsCompleted(tenant.OnboardingStatus))
{
    return RedirectToAction("Index", "OnboardingWizard", new { tenantId = tenant.Id });
}
```

---

### **Trigger 3: Tenant Creation**

**When:** Immediately after tenant + admin user creation

**Flow:**
1. Tenant created with `OnboardingStatus = "NotStarted"`
2. Admin user created and signed in
3. Redirect to onboarding wizard

**Example from `AgentTenantApiController`:**
```csharp
return Ok(new
{
    success = true,
    tenantId = tenantDto.Id,
    redirectUrl = $"/onboarding/wizard/fast-start?tenantId={tenantDto.Id}",
    // ...
});
```

---

## 🔄 Onboarding Wizard Flow

### **Step Progression**

```
Index → StepA → StepB → StepC → StepD → StepE → StepF → 
StepG → StepH → StepI → StepJ → StepK → StepL → Summary → Complete
```

### **Current Step Tracking**

- Stored in: `OnboardingWizard.CurrentStep` (1-12)
- Updated when user completes each step
- Wizard resumes at current step if user returns

### **Completion**

**When:** User completes Step L (final step)

**Actions:**
1. `OnboardingWizard.WizardStatus = "Completed"`
2. `Tenant.OnboardingStatus = "Completed"`
3. User redirected to dashboard
4. No more redirects to onboarding

---

## 🛡️ Security & Access Control

### **Authentication Requirements**

- ✅ All wizard routes require `[Authorize]` attribute
- ✅ User must be authenticated (cookie or OpenIddict token)
- ✅ User must be tenant admin (checked via `CheckTenantAdminAuthAsync()`)

### **Tenant Isolation**

- ✅ Each tenant has its own onboarding wizard
- ✅ Users can only access their own tenant's wizard
- ✅ `tenantId` parameter is validated against user's tenant

### **Authorization Check**

**Method:** `OnboardingWizardController.CheckTenantAdminAuthAsync()`

**Checks:**
1. User is authenticated
2. User has tenant claim matching `tenantId`
3. User is tenant admin (role or tenant user record)

**If Failed:**
- Redirects to: `/Account/TenantAdminLogin?tenantId={guid}&returnUrl={currentPath}`

---

## 📊 Onboarding Status Constants

**File:** `Shahin-Jan-2026/src/GrcMvc/Constants/OnboardingStatus.cs`

**Values:**
- `"NotStarted"` - Onboarding not started
- `"InProgress"` - Onboarding in progress
- `"Completed"` - Onboarding completed

**Helper Method:**
```csharp
OnboardingStatus.IsCompleted(status) // Returns true if status == "Completed"
```

---

## 🎯 Summary Table

| **When** | **Where** | **How** | **Redirect To** |
|----------|-----------|---------|-----------------|
| After login | `AccountController.Login()` | Automatic | `/OnboardingWizard/Index?tenantId={guid}` |
| On any request | `OnboardingRedirectMiddleware` | Automatic | `/OnboardingWizard/Index?tenantId={guid}` |
| After tenant creation | Various controllers | Automatic | `/OnboardingWizard/Index?tenantId={guid}` |
| Direct access | User navigates | Manual | `/OnboardingWizard/Index?tenantId={guid}` |

**Condition for Redirect:**
- `tenant.OnboardingStatus != "Completed"`

**Stops Redirecting When:**
- `tenant.OnboardingStatus == "Completed"`

---

## 🔧 Configuration

### **Middleware Registration**

**File:** `Program.cs` line 1712

```csharp
app.UseMiddleware<GrcMvc.Middleware.OnboardingRedirectMiddleware>();
```

**Order:** After authentication middleware, before routing

### **Route Configuration**

**File:** `Program.cs` line 1934-1936

```csharp
name: "onboarding-wizard",
pattern: "OnboardingWizard/{action=Index}/{tenantId?}",
defaults: new { controller = "OnboardingWizard" }
```

---

## 📝 Notes

1. **Two Onboarding Systems:**
   - **Simplified 4-Step:** `OnboardingController` (legacy, `/Onboarding/*`)
   - **Comprehensive 12-Step:** `OnboardingWizardController` (current, `/OnboardingWizard/*`)

2. **Wizard Persistence:**
   - Progress saved after each step
   - User can exit and resume later
   - Current step tracked in database

3. **Completion Tracking:**
   - Both `OnboardingWizard.WizardStatus` and `Tenant.OnboardingStatus` are updated
   - Middleware checks `Tenant.OnboardingStatus` for redirects

4. **Error Handling:**
   - Middleware errors don't block the app (continues to next middleware)
   - Missing tenant → No redirect (handled elsewhere)
   - Missing wizard → Created automatically on first access

---

**Last Updated:** 2026-01-19  
**Status:** ✅ Active - All entry points functional
