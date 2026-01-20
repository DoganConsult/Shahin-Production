# What Happens When Onboarding is Incomplete?

## Overview
When an organization's onboarding is **not complete**, the system implements multiple safeguards to ensure the **tenant admin** completes the onboarding wizard before **any users** (including team members) can access the main application.

## ⚠️ IMPORTANT: Only Tenant Admins Can Complete Onboarding

**Key Point**: The onboarding wizard is **ONLY accessible to tenant admins**. Regular users (team members) cannot complete onboarding themselves - they must wait for the tenant admin to complete it.

## Onboarding Status Values

The system uses these status values:
- `NOT_STARTED` - Onboarding hasn't started
- `IN_PROGRESS` - Onboarding is in progress (not complete)
- `COMPLETED` - Onboarding is fully complete ✅
- `FAILED` - Onboarding failed (rare)

**Only `COMPLETED` status allows full app access.**

## What Happens When Incomplete

### 1. **Automatic Redirect to Onboarding Wizard** 🔄

When **any user** logs in and onboarding is **NOT COMPLETED**:

#### A. Post-Login Redirect (AccountController)
```csharp
// After successful login, checks onboarding status for ALL users
if (!isCompleted)
{
    // Redirects to: /OnboardingWizard/Index?tenantId={tenantId}
    return RedirectToAction("Index", "OnboardingWizard", new { tenantId });
}
```

**Behavior by User Role:**
- **Tenant Admin**: Redirected to wizard → Can complete onboarding ✅
- **Regular User/Team Member**: Redirected to wizard → **Blocked** (not admin) ❌
  - Shows error: "You must be authenticated as a tenant admin to access onboarding"
  - Redirected to login page

#### B. Middleware Guard (OnboardingRedirectMiddleware)
Even if a user tries to access other routes, the middleware intercepts:

```csharp
// Checks every authenticated request for ALL users
if (!isCompleted)
{
    // Redirects to: /OnboardingWizard/Index?tenantId={tenantId}
    context.Response.Redirect($"/OnboardingWizard/Index?tenantId={tenantId}");
    return;
}
```

**Wizard Access Control:**
```csharp
// OnboardingWizardController checks admin status
var isAuthenticated = await CheckTenantAdminAuthAsync(tenantId);
if (!isAuthenticated)
{
    // Regular users are blocked from wizard
    TempData["Error"] = "You must be authenticated as a tenant admin to access onboarding.";
    return RedirectToAction("TenantAdminLogin", "Account");
}
```

**Routes that are ALWAYS accessible (even with incomplete onboarding):**
- `/OnboardingWizard/*` - The wizard itself
- `/Account/*` - Login, logout, registration
- `/api/*` - API endpoints (handle separately)
- Static files (CSS, JS, images)
- Health checks
- Error pages

### 2. **Abandonment Detection & Recovery Emails** 📧

The system runs a **daily background job** (`OnboardingAbandonmentWorker`) that:

#### A. Detects Incomplete Onboarding
- Finds all wizards with status ≠ `"Completed"` and ≠ `"Cancelled"`
- Calculates days since start and days since last activity

#### B. Sends Recovery Emails

**After 7+ days incomplete (with 7+ days inactive):**
- Sends **Abandonment Recovery Email**
- Includes:
  - Resume link: `https://portal.shahin-ai.com/OnboardingWizard/Index?tenantId={tenantId}`
  - Days incomplete count
  - Professional enterprise email template
  - Bilingual support (Arabic/English)

**After 3+ days inactive (but < 7 days total):**
- Sends **Progress Reminder Email**
- Includes:
  - Current step: `{currentStep} / 12`
  - Resume link
  - Days since last activity
  - Encouragement to continue

#### C. Email Timing
- **Skipped if < 2 days** - Too recent, no email sent
- **Reminder emails**: 3+ days inactive
- **Abandonment emails**: 7+ days incomplete + 7+ days inactive

### 3. **User Experience Flow**

```
User Logs In
    ↓
Check Onboarding Status
    ↓
┌─────────────────────────┐
│ Is COMPLETED?           │
└─────────────────────────┘
    │
    ├─ YES → Access Full App ✅
    │
    └─ NO → Redirect to Wizard
             ↓
        User Completes Steps
             ↓
        Mark as COMPLETED
             ↓
        Access Full App ✅
```

### 4. **What Users CANNOT Access (When Incomplete)**

When onboarding is incomplete, **ALL users** (including team members) are **blocked from**:
- ❌ Dashboard (`/Dashboard/*`)
- ❌ Workspace management (`/Workspace/*`)
- ❌ Tenant management (`/Tenant/*`)
- ❌ All main application features
- ❌ Any route except onboarding wizard and account routes

**Additional Restrictions for Regular Users:**
- ❌ **Cannot access onboarding wizard** (admin-only)
- ❌ **Cannot complete onboarding** (must wait for admin)
- ❌ **Blocked from entire app** until admin completes onboarding

### 5. **Resume Capability**

**Tenant Admins** can **resume** incomplete onboarding:
- Direct link in recovery emails (sent to admin email)
- Automatic redirect on login
- Wizard remembers current step (`CurrentStep` property)
- Progress is saved (`CompletedSectionsJson`)
- Can continue from where they left off

**Regular Users:**
- ❌ Cannot resume onboarding (admin-only)
- ❌ Must wait for tenant admin to complete
- ✅ Will gain access automatically once admin completes onboarding

### 6. **Background Worker Schedule**

The abandonment detection runs:
- **Frequency**: Daily (via ABP Background Worker)
- **Worker**: `OnboardingAbandonmentWorker`
- **Job**: `OnboardingAbandonmentJob`
- **Time**: Configured in `GrcMvcAbpModule.cs`

## Code Locations

### Key Files:
1. **`OnboardingRedirectMiddleware.cs`**
   - Middleware that checks and redirects incomplete onboarding
   - Runs after authentication, before controllers

2. **`AccountController.cs`**
   - Post-login redirect logic
   - Checks `OnboardingStatus.IsCompleted()`

3. **`OnboardingAbandonmentJob.cs`**
   - Background job for detection and email sending
   - Runs daily via ABP Background Worker

4. **`OnboardingAbandonmentWorker.cs`**
   - ABP Background Worker wrapper
   - Orchestrates the job execution

5. **`Constants/ClaimConstants.cs`**
   - `OnboardingStatus` class with status values
   - `IsCompleted()` helper method

## Logging

All onboarding-related actions are logged with `[GOLDEN_PATH]` markers:

```
[GOLDEN_PATH] OnboardingRedirectMiddleware: Tenant found. TenantId={TenantId}, Status={Status}, IsCompleted={IsCompleted}
[GOLDEN_PATH] ✅ MIDDLEWARE REDIRECT: User → OnboardingWizard/Index
[GOLDEN_PATH] Abandonment recovery email sent to {Email} for tenant {TenantId}
```

## Summary

**When onboarding is incomplete:**

### For Tenant Admins:
1. ✅ **Automatically redirected** to the wizard
2. ✅ **Can complete** the onboarding wizard
3. ✅ **Recovery emails** sent after 3-7 days of inactivity
4. ✅ Can **resume** from where they left off
5. ✅ **No data loss** - progress is saved
6. ✅ **Professional email templates** with resume links

### For Regular Users/Team Members:
1. ✅ **Automatically redirected** to the wizard (but blocked)
2. ❌ **Cannot access** the wizard (admin-only)
3. ❌ **Cannot complete** onboarding (must wait for admin)
4. ❌ **Blocked from entire app** until admin completes
5. ✅ Will **automatically gain access** once admin completes onboarding

**The system ensures ONLY tenant admins can complete onboarding, and ALL users are blocked until it's complete!** 🎯
