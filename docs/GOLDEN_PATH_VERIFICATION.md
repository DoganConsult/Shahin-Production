# Golden Path Verification: Trial Registration → Login → Onboarding

## ✅ Status: INSTRUMENTED FOR VERIFICATION

All critical points in the golden path have been instrumented with `[GOLDEN_PATH]` logging tags.

---

## 🔍 Golden Path Flow

### Step 1: Trial Registration
**Entry Point:** `/trial` or `/grc-free-trial`
- User fills trial registration form
- `TrialLifecycleService.ProvisionTrialAsync()` creates:
  - ABP Tenant
  - Admin User (with TenantAdmin role)
  - Custom Tenant entity with `OnboardingStatus = "NOT_STARTED"`
- User is automatically signed in
- Redirects to `/Onboarding/Start/{tenantSlug}`

### Step 2: First Login (After Trial)
**Entry Point:** `/Account/Login`
- User submits login form (Email + Password)
- Form posts to `AccountController.Login` (POST)
- On success, calls `ProcessPostLoginAsync()`

### Step 3: Onboarding Check (ProcessPostLoginAsync)
**Location:** `AccountController.ProcessPostLoginAsync()`
- Retrieves `TenantUser` and `Tenant` entities
- Adds `TenantId` claim to user
- **Checks `tenant.OnboardingStatus`**
- If NOT completed → Redirects to `/OnboardingWizard/Index?tenantId={tenantId}`
- If completed → Redirects to role-based dashboard

### Step 4: Middleware Guard (Secondary Check)
**Location:** `OnboardingRedirectMiddleware`
- Runs on every authenticated request (after auth middleware)
- Checks if path should be skipped (onboarding routes, API, static files, etc.)
- Extracts `TenantId` from claims
- Queries tenant from database
- If onboarding NOT completed → Redirects to `/OnboardingWizard/Index?tenantId={tenantId}`
- If completed → Allows request to proceed

---

## 📊 Log Tags to Monitor

All golden path logs are prefixed with `[GOLDEN_PATH]` for easy filtering.

### Login Form Submission
```
[GOLDEN_PATH] Login form submitted. Email={Email}, ReturnUrl={ReturnUrl}, ModelStateValid={IsValid}
[GOLDEN_PATH] SignIn result. Email={Email}, Succeeded={Succeeded}, IsLockedOut={IsLockedOut}
[GOLDEN_PATH] User {Email} (ID: {UserId}) logged in successfully...
```

### Post-Login Processing
```
[GOLDEN_PATH] ProcessPostLoginAsync started. UserId={UserId}, Email={Email}
[GOLDEN_PATH] TenantUser found. TenantId={TenantId}, RoleCode={RoleCode}
[GOLDEN_PATH] Tenant entity retrieved. TenantId={TenantId}, OnboardingStatus={Status}, Role={Role}
[GOLDEN_PATH] Onboarding check. IsAdmin={IsAdmin}, IsCompleted={IsCompleted}, Status={Status}
[GOLDEN_PATH] ✅ REDIRECT DECISION: User {Email} → OnboardingWizard/Index (TenantId: {TenantId}, Status: {Status})
```

### Middleware Guard
```
[GOLDEN_PATH] OnboardingRedirectMiddleware: TenantId extracted. TenantId={TenantId}, Path={Path}
[GOLDEN_PATH] OnboardingRedirectMiddleware: Tenant found. TenantId={TenantId}, Status={Status}, IsCompleted={IsCompleted}, Path={Path}
[GOLDEN_PATH] ✅ MIDDLEWARE REDIRECT: User → OnboardingWizard/Index. TenantId={TenantId}, Status={Status}, Path={Path}
```

---

## 🧪 How to Test the Golden Path

### Test Scenario 1: New Trial User
1. **Register for trial** at `/trial` or `/grc-free-trial`
2. **Check logs** for tenant creation and `OnboardingStatus = "NOT_STARTED"`
3. **Login** with the created admin credentials
4. **Verify logs** show:
   - `[GOLDEN_PATH] Login form submitted`
   - `[GOLDEN_PATH] ProcessPostLoginAsync started`
   - `[GOLDEN_PATH] ✅ REDIRECT DECISION: ... → OnboardingWizard/Index`
5. **Verify browser** redirects to `/OnboardingWizard/Index?tenantId={guid}`

### Test Scenario 2: Existing User with Incomplete Onboarding
1. **Login** with existing tenant admin credentials
2. **Verify logs** show onboarding status check
3. **Verify redirect** to onboarding wizard

### Test Scenario 3: User with Completed Onboarding
1. **Complete onboarding** wizard
2. **Login** again
3. **Verify logs** show: `[GOLDEN_PATH] ✅ Onboarding completed. User {Email} → LoginRedirect`
4. **Verify browser** redirects to dashboard (not onboarding)

---

## 🔧 Frontend Connection Verification

### Login Form (Login.cshtml)
✅ **Form Action:** `asp-action="Login"` → Posts to `AccountController.Login`
✅ **Method:** `method="post"`
✅ **Anti-Forgery Token:** `@Html.AntiForgeryToken()` present
✅ **Email Field:** `asp-for="Email"` properly bound
✅ **Password Field:** `asp-for="Password"` properly bound
✅ **Validation:** `asp-validation-summary` and field-level validation present

### Expected Behavior
- Form submits to `/Account/Login` (POST)
- On success: Redirects to onboarding or dashboard based on status
- On failure: Shows validation errors or login failure message

---

## 🚨 Common Issues to Check

### Issue 1: Form Not Submitting
**Symptoms:**
- No `[GOLDEN_PATH] Login form submitted` log
- Form stays on page after clicking submit

**Check:**
- Browser console for JavaScript errors
- Network tab for POST request to `/Account/Login`
- Anti-forgery token is present

### Issue 2: Login Succeeds But No Redirect
**Symptoms:**
- `[GOLDEN_PATH] SignIn result. Succeeded=true` appears
- But no `[GOLDEN_PATH] ProcessPostLoginAsync started` log

**Check:**
- `ProcessPostLoginAsync` is being called after successful login
- No exceptions in logs

### Issue 3: Tenant Not Found
**Symptoms:**
- `[GOLDEN_PATH] No tenant found for user {Email}` log

**Check:**
- `TenantUser` record exists for the user
- `TenantUser.TenantId` is not null
- Tenant entity exists in database

### Issue 4: Onboarding Status Not Set
**Symptoms:**
- `OnboardingStatus` is null or unexpected value
- Redirect logic doesn't trigger

**Check:**
- `TrialLifecycleService.ProvisionTrialAsync()` sets `OnboardingStatus = "NOT_STARTED"`
- Database has correct status value
- `OnboardingStatus.IsCompleted()` helper method logic

### Issue 5: Middleware Not Executing
**Symptoms:**
- No `[GOLDEN_PATH] OnboardingRedirectMiddleware` logs
- User can access dashboard without completing onboarding

**Check:**
- `OnboardingRedirectMiddleware` is registered in `Program.cs` (after `UseAuthentication()`)
- Middleware is not being skipped by `ShouldSkip()` method
- User is authenticated (has `TenantId` claim)

---

## 📝 Next Steps

1. **Run the application** and test the golden path
2. **Filter logs** by `[GOLDEN_PATH]` tag
3. **Verify each step** in the flow appears in logs
4. **Check browser** redirects match log decisions
5. **Report any missing logs** or unexpected redirects

---

## 🔗 Related Files

- `Controllers/AccountController.cs` - Login and post-login processing
- `Middleware/OnboardingRedirectMiddleware.cs` - Secondary onboarding guard
- `Views/Account/Login.cshtml` - Login form UI
- `Services/Implementations/TrialLifecycleService.cs` - Trial provisioning
- `Controllers/OnboardingWizardController.cs` - Onboarding wizard target

---

**Last Updated:** 2026-01-12
**Status:** ✅ Instrumented and ready for testing
