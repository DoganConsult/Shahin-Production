# Workspace Integration - Setup & Configuration Required

**Date:** 2026-01-06

---

## ✅ Already Configured

### 1. Service Registration ✅
**Location:** `Program.cs` lines 428, 431

```csharp
builder.Services.AddScoped<IWorkspaceContextService, WorkspaceContextService>();
builder.Services.AddScoped<IWorkspaceManagementService, WorkspaceManagementService>();
```

**Status:** ✅ **CONFIGURED**

---

### 2. Session Configuration ✅
**Location:** `Program.cs` line 328

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
```

**Status:** ✅ **CONFIGURED**

---

### 3. HttpContextAccessor ✅
**Location:** `Program.cs` line 120

```csharp
builder.Services.AddHttpContextAccessor();
```

**Status:** ✅ **CONFIGURED** (Required for WorkspaceContextService)

---

### 4. Database Migrations ✅
**Migrations Created:**
- `20260106125113_WorkspaceInsideTenantModel.cs` ✅
- `20260106131413_AddWorkspaceIdToCoreEntities.cs` ✅

**Status:** ✅ **CREATED** (Need to verify if applied)

---

## ⚠️ REQUIRED SETUP

### 1. Enable Session Middleware (CRITICAL) ⚠️

**Issue:** Session is configured but middleware may not be enabled.

**Location:** `Program.cs` - Middleware pipeline (around line 780)

**Required:** Add `app.UseSession()` in the middleware pipeline

**Order:** Must be AFTER `app.UseRouting()` and BEFORE `app.UseAuthentication()`

**Example:**
```csharp
app.UseRouting();

// Add this line:
app.UseSession(); // Required for workspace context storage

app.UseCors("AllowSpecificOrigins");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
```

**Status:** ⚠️ **NEEDS VERIFICATION**

---

### 2. Verify Database Migrations Applied ⚠️

**Check if migrations are applied:**

```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet ef database update
```

**Or check migration status:**
```bash
dotnet ef migrations list
```

**Status:** ⚠️ **NEEDS VERIFICATION**

---

### 3. ViewComponent Auto-Discovery ✅

**Status:** ✅ **NO ACTION NEEDED**

ASP.NET Core automatically discovers ViewComponents in:
- `ViewComponents/` folder
- Namespace ending with `.ViewComponents`

**Current:** `WorkspaceSwitcherViewComponent` is in correct location ✅

---

## Setup Checklist

### Immediate Actions Required:

- [ ] **1. Verify `app.UseSession()` is in middleware pipeline**
  - Check `Program.cs` around line 780
  - If missing, add: `app.UseSession();`
  - Must be after `UseRouting()` and before `UseAuthentication()`

- [ ] **2. Apply database migrations**
  ```bash
  cd /home/dogan/grc-system/src/GrcMvc
  dotnet ef database update
  ```

- [ ] **3. Verify migrations applied**
  ```bash
  dotnet ef migrations list
  ```
  Should show both workspace migrations as "Applied"

### Optional Verification:

- [ ] **4. Test workspace creation**
  - Create a workspace via onboarding or admin
  - Verify user is added as member
  - Verify workspace appears in switcher

- [ ] **5. Test workspace switching**
  - Login as user with multiple workspaces
  - Click workspace switcher
  - Verify page reloads with new workspace context

---

## Verification Commands

### Check Session Middleware:
```bash
grep -n "UseSession" src/GrcMvc/Program.cs
```

### Check Service Registration:
```bash
grep -n "IWorkspaceContextService\|IWorkspaceManagementService" src/GrcMvc/Program.cs
```

### Check Migrations:
```bash
cd src/GrcMvc
dotnet ef migrations list
```

---

## Expected Behavior After Setup

1. **On Login:**
   - WorkspaceContextService resolves default workspace
   - WorkspaceId stored in session
   - Workspace switcher appears in header (if user has workspaces)

2. **On Workspace Switch:**
   - POST to `/api/workspace/switch`
   - Session updated with new WorkspaceId
   - Page reloads
   - All queries filtered by new WorkspaceId

3. **On Entity Creation:**
   - Services automatically set WorkspaceId
   - Entity saved with current workspace context
   - Query filters ensure isolation

---

## Troubleshooting

### Issue: Workspace switcher not appearing
**Check:**
- User has workspace memberships
- Session is enabled (`app.UseSession()`)
- WorkspaceContextService is registered

### Issue: Workspace switch not working
**Check:**
- API endpoint `/api/workspace/switch` is accessible
- User has access to target workspace
- Session is enabled

### Issue: Data not filtered by workspace
**Check:**
- Migrations applied (WorkspaceId columns exist)
- Query filters in DbContext are active
- WorkspaceId is set on entity creation

---

**Next Step:** Verify `app.UseSession()` is in middleware pipeline and apply migrations if needed.
