# Golden Path & Admin Path Testing Guide

## 🎯 Overview

This guide helps you test both the **Golden Path** (trial registration → onboarding) and **Admin Path** (platform/tenant admin access).

---

## 🟢 GOLDEN PATH: Trial Registration → Onboarding

### Flow Overview

```
1. Trial Registration (/trial or /api/trial/provision)
   ↓
2. Tenant + Admin User Created
   ↓
3. Auto Sign-In
   ↓
4. Redirect to /Onboarding/Start/{tenantSlug}
   ↓
5. Onboarding Wizard (12 steps)
   ↓
6. Onboarding Complete
   ↓
7. Redirect to Dashboard
```

### Step-by-Step Testing

#### Step 1: Trial Registration

**Option A: Via UI**
- Navigate to: `http://localhost:5010/trial` or `http://localhost:5010/grc-free-trial`
- Fill form:
  - Organization Name
  - Admin Email
  - Password (min 12 chars)
  - Accept Terms
- Submit

**Option B: Via API**
```bash
POST http://localhost:5010/api/trial/provision
Content-Type: application/json

{
  "organizationName": "Test Company",
  "adminEmail": "admin@testcompany.com",
  "password": "TestPassword123!",
  "acceptTerms": true
}
```

**Expected Result:**
- Tenant created with `Status = "trial"`
- Admin user created with `TenantAdmin` role
- User auto-signed in
- Redirect to `/Onboarding/Start/{tenantSlug}`

#### Step 2: Verify Onboarding Start

**URL:** `http://localhost:5010/Onboarding/Start/{tenantSlug}`

**Expected:**
- Shows onboarding wizard
- TenantId stored in TempData
- Can proceed through steps

#### Step 3: Complete Onboarding

**URL:** `http://localhost:5010/OnboardingWizard/Index?tenantId={guid}`

**Steps:**
- Step A: Organization Identity
- Step B: Assurance Objective
- Step C: Regulatory Applicability
- Step D: Scope Definition (with filtered dropdowns)
- ... through Step L

**Expected:**
- Progress saved automatically
- Sidebar shows progress
- Can navigate between steps

#### Step 4: Verify Completion

After completing all steps:
- `OnboardingStatus` = "COMPLETED"
- Redirect to dashboard on next login

#### Step 5: Test Login After Completion

**URL:** `http://localhost:5010/Account/Login`

**Login with:**
- Email: `admin@testcompany.com`
- Password: `TestPassword123!`

**Expected:**
- ✅ Login succeeds
- ✅ Redirects to dashboard (NOT onboarding)
- ✅ Can access tenant features

---

## 🔐 ADMIN PATH: Platform Admin & Tenant Admin

### Platform Admin Flow

#### Step 1: Platform Admin Login

**URL:** `http://localhost:5010/admin/login`

**Credentials:**
- Email: `Dooganlap@gmail.com`
- Password: (Your platform admin password)

**Expected:**
- Login succeeds
- Redirects to `/admin/dashboard`

#### Step 2: Platform Admin Dashboard

**URL:** `http://localhost:5010/admin/dashboard`

**Features Available:**
- View all tenants
- Create new tenants
- Manage tenant subscriptions
- View platform analytics
- Manage platform admins

#### Step 3: Create Tenant (Platform Admin)

**URL:** `http://localhost:5010/admin/tenants/create`

**Or via API:**
```bash
POST http://localhost:5010/api/owner/tenants
Authorization: Bearer {token}
Content-Type: application/json

{
  "organizationName": "New Tenant",
  "adminEmail": "admin@newtenant.com",
  "subscriptionTier": "Enterprise"
}
```

### Tenant Admin Flow

#### Step 1: Tenant Admin Login

**URL:** `http://localhost:5010/Account/Login`

**Credentials:**
- Email: (Tenant admin email)
- Password: (Tenant admin password)

**Expected:**
- If onboarding incomplete → Redirects to `/OnboardingWizard/Index?tenantId={guid}`
- If onboarding complete → Redirects to `/TenantAdmin/Dashboard`

#### Step 2: Tenant Admin Dashboard

**URL:** `http://localhost:5010/TenantAdmin/Dashboard`

**Features Available:**
- View tenant compliance status
- Manage users
- View risks and controls
- Manage assessments
- View reports

---

## 🧪 Test Endpoints

### Base URL: `/api/test/paths`

All endpoints require `PlatformAdmin` role.

### 1. Verify Golden Path
**GET** `/api/test/paths/golden-path/verify`

Returns:
- Trial endpoints status
- Onboarding endpoints status
- Recent trial tenants
- Recent onboarding wizards
- Endpoint URLs

### 2. Verify Admin Path
**GET** `/api/test/paths/admin-path/verify`

Returns:
- Platform admins list
- Tenant admins list
- Admin endpoints
- Controller availability

### 3. Full Report
**GET** `/api/test/paths/full-report`

Returns complete test report for both paths.

### 4. Test Login Flow
**POST** `/api/test/paths/test-login-flow`

**Body:**
```json
{
  "email": "admin@testcompany.com"
}
```

**Returns:**
- User information
- Tenant information
- Expected redirect path
- Flow steps

---

## 📋 Testing Checklist

### Golden Path ✅
- [ ] Trial registration form accessible
- [ ] Trial registration creates tenant + user
- [ ] Auto sign-in works
- [ ] Redirects to onboarding
- [ ] Onboarding wizard loads
- [ ] Steps can be completed
- [ ] Progress saves automatically
- [ ] Filtered dropdowns work in Step D
- [ ] Onboarding completion updates status
- [ ] Login after completion redirects to dashboard

### Admin Path ✅
- [ ] Platform admin login works
- [ ] Platform admin dashboard accessible
- [ ] Can view tenants
- [ ] Can create tenants
- [ ] Tenant admin login works
- [ ] Tenant admin dashboard accessible
- [ ] Onboarding redirect works for tenant admins
- [ ] Role-based redirects work correctly

---

## 🔍 Log Monitoring

### Golden Path Logs
Filter logs by: `[GOLDEN_PATH]`

**Key Logs:**
```
[GOLDEN_PATH] Login form submitted. Email={Email}
[GOLDEN_PATH] ProcessPostLoginAsync started. UserId={UserId}
[GOLDEN_PATH] Tenant entity retrieved. OnboardingStatus={Status}
[GOLDEN_PATH] ✅ REDIRECT DECISION: User → OnboardingWizard/Index
```

### Admin Path Logs
Filter logs by: `PlatformAdmin` or `TenantAdmin`

**Key Logs:**
```
Platform Admin login: {Email}
Tenant Admin login: {Email}
Redirect to: {Path}
```

---

## 🚀 Quick Test Commands

### Test Golden Path
```bash
# 1. Verify endpoints
GET http://localhost:5010/api/test/paths/golden-path/verify

# 2. Test login flow
POST http://localhost:5010/api/test/paths/test-login-flow
Content-Type: application/json
{
  "email": "admin@testcompany.com"
}

# 3. Register trial (if needed)
POST http://localhost:5010/api/trial/provision
Content-Type: application/json
{
  "organizationName": "Test Company",
  "adminEmail": "test@test.com",
  "password": "TestPassword123!",
  "acceptTerms": true
}
```

### Test Admin Path
```bash
# 1. Verify admin endpoints
GET http://localhost:5010/api/test/paths/admin-path/verify

# 2. Get full report
GET http://localhost:5010/api/test/paths/full-report
```

---

## 📊 Expected Results

### Golden Path
- ✅ Trial registration creates tenant
- ✅ User auto-signed in
- ✅ Redirects to onboarding
- ✅ Onboarding wizard functional
- ✅ Progress saves
- ✅ Completion updates status
- ✅ Login redirects to dashboard

### Admin Path
- ✅ Platform admin can login
- ✅ Platform admin dashboard accessible
- ✅ Can manage tenants
- ✅ Tenant admin can login
- ✅ Tenant admin dashboard accessible
- ✅ Onboarding redirect works

---

## 🔗 Key URLs

### Golden Path URLs
- Trial Registration: `/trial` or `/grc-free-trial`
- Trial API: `/api/trial/provision`
- Onboarding Start: `/Onboarding/Start/{tenantSlug}`
- Onboarding Wizard: `/OnboardingWizard/Index?tenantId={guid}`
- Login: `/Account/Login`

### Admin Path URLs
- Platform Admin Login: `/admin/login`
- Platform Admin Dashboard: `/admin/dashboard`
- Owner Portal: `/owner`
- Tenant Admin Login: `/Account/Login`
- Tenant Admin Dashboard: `/TenantAdmin/Dashboard`

---

## ⚠️ Common Issues

### Golden Path Issues
1. **Trial registration fails**
   - Check database connection
   - Verify ABP tenant creation
   - Check logs for errors

2. **Onboarding not redirecting**
   - Check `OnboardingStatus` in database
   - Verify middleware is configured
   - Check `OnboardingRedirectMiddleware` logs

3. **Progress not saving**
   - Check `AutoSave` endpoint
   - Verify JavaScript is loaded
   - Check browser console for errors

### Admin Path Issues
1. **Platform admin can't login**
   - Verify user has `PlatformAdmin` role
   - Check `PlatformAdmin.IsActive = true`
   - Verify `ActivePlatformAdmin` policy

2. **Tenant admin redirected incorrectly**
   - Check `OnboardingStatus`
   - Verify tenant exists
   - Check role assignments

---

**Last Updated:** 2026-01-12
**Status:** ✅ Ready for testing
