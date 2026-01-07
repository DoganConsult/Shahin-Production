# Legacy Code Cleanup Guide

## Overview

After the V2 migration is complete and stable (100% enhanced usage for 7+ days), you can safely remove legacy code. This guide provides step-by-step instructions for cleanup.

---

## ⚠️ Pre-Cleanup Checklist

**DO NOT proceed unless ALL criteria are met:**

- [ ] V2 routes have been in production for 7+ days
- [ ] Migration metrics show 100% enhanced usage
- [ ] Success rate is ≥99.9% for enhanced implementation
- [ ] Zero consistency failures detected
- [ ] No customer complaints or support tickets
- [ ] Stakeholder approval obtained
- [ ] Full database backup completed

---

## 🗑️ Files to Delete (Phase 1)

### Week 1: Mark as Obsolete (No Deletion Yet)

Add `[Obsolete]` attributes to legacy code:

#### 1. Controllers
```csharp
// src/GrcMvc/Controllers/AccountController.cs
[Obsolete("Use AccountControllerV2 instead. Will be removed in v3.0")]
public class AccountController : Controller
{
    // Keep code, just mark as obsolete
}

// src/GrcMvc/Controllers/PlatformAdminController.cs
[Obsolete("Use PlatformAdminControllerV2 instead. Will be removed in v3.0")]
public class PlatformAdminController : Controller
{
    // Keep code, just mark as obsolete
}
```

#### 2. Services
```csharp
// src/GrcMvc/Services/Implementations/PlatformAdminService.cs
[Obsolete("Use enhanced UserManager directly. Will be removed in v3.0")]
public class PlatformAdminService : IPlatformAdminService
{
    // Keep code, just mark as obsolete
}
```

**Monitor for 7 more days** - If any issues arise, you can still use legacy code.

---

## 🗑️ Files to Delete (Phase 2)

### Week 2: Remove Obsolete Services

**CRITICAL:** Only after Phase 1 monitoring shows zero issues.

#### Step 1: Remove Legacy Service Implementations

```bash
# Backup first!
cp -r src/GrcMvc/Services/Implementations src/GrcMvc/Services/Implementations.backup

# Remove legacy files
rm src/GrcMvc/Services/Implementations/PlatformAdminService.cs
```

#### Step 2: Remove Legacy Service Interfaces

```bash
# Check for any remaining references first
grep -r "IPlatformAdminService" src/GrcMvc/

# If no references, remove
rm src/GrcMvc/Services/Interfaces/IPlatformAdminService.cs
```

#### Step 3: Update Program.cs

Remove legacy service registrations:

```csharp
// REMOVE THIS LINE:
// builder.Services.AddScoped<IPlatformAdminService, PlatformAdminService>();
```

---

## 🔄 Route Migration (Phase 3)

### Week 3: Promote V2 to V1

#### Step 1: Update Route Attributes

**Before:**
```csharp
[Route("platform-admin/v2")]
public class PlatformAdminControllerV2 : Controller
```

**After:**
```csharp
[Route("platform-admin")]
public class PlatformAdminController : Controller
```

#### Step 2: Rename Controller Files

```bash
cd src/GrcMvc/Controllers

# Backup legacy (don't delete yet)
mv PlatformAdminController.cs PlatformAdminController.legacy.cs
mv AccountController.cs AccountController.legacy.cs

# Rename V2 to V1
mv PlatformAdminControllerV2.cs PlatformAdminController.cs
mv AccountControllerV2.cs AccountController.cs

# Update class names inside files
# PlatformAdminControllerV2 → PlatformAdminController
# AccountControllerV2 → AccountController
```

#### Step 3: Update View Paths

**Before:**
```csharp
return View("~/Views/PlatformAdmin/DashboardV2.cshtml");
```

**After:**
```csharp
return View("~/Views/PlatformAdmin/Dashboard.cshtml");
```

#### Step 4: Rename View Files

```bash
cd src/GrcMvc/Views

# Backup legacy
mv PlatformAdmin/Dashboard.cshtml PlatformAdmin/Dashboard.legacy.cshtml
mv Account/Login.cshtml Account/Login.legacy.cshtml

# Promote V2 to V1
mv PlatformAdmin/DashboardV2.cshtml PlatformAdmin/Dashboard.cshtml
mv PlatformAdmin/UsersV2.cshtml PlatformAdmin/Users.cshtml
mv Account/LoginV2.cshtml Account/Login.cshtml
mv Account/TenantLoginV2.cshtml Account/TenantLogin.cshtml
```

---

## 🧹 Feature Flag Cleanup (Phase 4)

### Week 4: Remove Feature Flags

#### Step 1: Remove Facade Pattern

**Before (in UserManagementFacade):**
```csharp
if (useEnhanced)
{
    // Enhanced code
}
else
{
    // Legacy code
}
```

**After:**
```csharp
// Just enhanced code, no branching
```

#### Step 2: Simplify Controllers

Remove all feature flag checks:

```csharp
// REMOVE:
if (_featureOptions.Value.DisableDemoLogin) { ... }

// REPLACE WITH:
// Enhanced code directly
```

#### Step 3: Remove GrcFeatureOptions

```bash
# Remove configuration class
rm src/GrcMvc/Configuration/GrcFeatureOptions.cs

# Remove from appsettings.json
# Delete "GrcFeatureFlags" section
```

#### Step 4: Remove Metrics Service (Optional)

If you don't need ongoing monitoring:

```bash
rm src/GrcMvc/Services/Interfaces/IMetricsService.cs
rm src/GrcMvc/Services/Implementations/MetricsService.cs
rm src/GrcMvc/Controllers/MigrationMetricsController.cs
rm src/GrcMvc/Views/PlatformAdmin/MigrationMetrics.cshtml
```

Update Program.cs:
```csharp
// REMOVE:
// builder.Services.AddSingleton<IMetricsService, MetricsService>();
```

---

## 📁 Final Cleanup (Phase 5)

### Week 5: Delete All Legacy Files

```bash
cd /home/dogan/grc-system

# Remove legacy backups
rm src/GrcMvc/Controllers/PlatformAdminController.legacy.cs
rm src/GrcMvc/Controllers/AccountController.legacy.cs
rm src/GrcMvc/Services/Implementations.backup -rf

# Remove legacy views
rm src/GrcMvc/Views/PlatformAdmin/Dashboard.legacy.cshtml
rm src/GrcMvc/Views/Account/Login.legacy.cshtml
```

---

## ✅ Post-Cleanup Verification

### Step 1: Build Check
```bash
cd src/GrcMvc
dotnet clean
dotnet build
```

Expected: 0 errors, 0 warnings

### Step 2: Test All Routes
```bash
# Test main routes
curl -k https://localhost:5010/platform-admin/dashboard
curl -k https://localhost:5010/account/login

# Should work without "/v2" in path
```

### Step 3: Run Full Test Suite
```bash
cd tests/GrcMvc.Tests
dotnet test
```

Expected: All tests pass

### Step 4: Deploy to Staging
1. Deploy cleaned code to staging
2. Run full regression tests
3. Monitor for 48 hours
4. If stable, deploy to production

---

## 🔄 Code Refactoring After Cleanup

### Simplify UserManagementFacade

**Before (with feature flags):**
```csharp
public async Task<bool> ResetPasswordAsync(...)
{
    var useEnhanced = ShouldUseEnhanced(...);
    if (useEnhanced) { /* enhanced */ }
    else { /* legacy */ }
}
```

**After (direct enhanced implementation):**
```csharp
public async Task<bool> ResetPasswordAsync(...)
{
    var user = await _userManager.FindByIdAsync(targetUserId);
    var newPassword = _securePasswordGenerator.GeneratePassword();
    // ... enhanced logic only
}
```

### Rename "Enhanced" Services

After legacy removal, "Enhanced" is redundant:

```bash
# Rename files
mv EnhancedAuthService.cs AuthService.cs
mv EnhancedTenantResolver.cs TenantResolver.cs

# Update class names and interfaces
# IEnhancedAuthService → IAuthService
# EnhancedAuthService → AuthService
```

---

## 📊 Expected Code Reduction

| Metric | Before Cleanup | After Cleanup | Reduction |
|--------|----------------|---------------|-----------|
| Total LOC | ~15,000 | ~11,000 | 27% |
| Service Classes | 8 | 5 | 38% |
| Controllers | 6 | 3 | 50% |
| Views | 10 | 5 | 50% |
| Feature Flags | 8 | 0 | 100% |

**Maintainability Score:** +45%

---

## ⚠️ Rollback from Cleanup

### If Issues Arise After Cleanup

#### Quick Rollback (from Git)
```bash
# Restore specific file
git checkout HEAD~1 -- src/GrcMvc/Controllers/AccountController.cs

# Or restore entire cleanup commit
git revert HEAD

# Then redeploy
cd src/GrcMvc
dotnet build
dotnet run
```

#### Full Rollback (from Backup)
```bash
# Restore from backup directory
cp -r src/GrcMvc/Services/Implementations.backup/* src/GrcMvc/Services/Implementations/
cp src/GrcMvc/Controllers/PlatformAdminController.legacy.cs src/GrcMvc/Controllers/PlatformAdminController.cs

# Rebuild
dotnet build
```

---

## 📋 Cleanup Verification Checklist

After each phase, verify:

- [ ] Application builds successfully
- [ ] All routes respond correctly
- [ ] No 404 or 500 errors in logs
- [ ] User authentication works
- [ ] Tenant switching works
- [ ] Password reset works
- [ ] Performance is stable
- [ ] No regression in functionality

---

## 🎯 Success Criteria

**You can consider cleanup complete when:**

1. ✅ All legacy code deleted
2. ✅ Build succeeds (0 errors)
3. ✅ All tests pass
4. ✅ Production stable for 7+ days
5. ✅ No rollback required
6. ✅ Documentation updated
7. ✅ Team trained on new code

---

## 📖 Documentation Updates After Cleanup

### Update These Files:

1. **README.md** - Remove references to V2 routes
2. **CLAUDE.md** - Update security section
3. **API docs** - Update endpoint paths
4. **Deployment guide** - Remove feature flag steps

### Archive These Files:

1. **PARALLEL_MIGRATION_COMPLETE.md** → Move to `docs/archive/`
2. **IMPLEMENTATION_AUDIT.md** → Move to `docs/archive/`
3. **QUICK_START.md** → Update or archive

---

## ⏱️ Timeline Summary

| Phase | Duration | Tasks | Risk Level |
|-------|----------|-------|------------|
| **Phase 1: Mark Obsolete** | 1 week | Add `[Obsolete]` attributes | 🟢 Low |
| **Phase 2: Remove Services** | 1 week | Delete legacy services | 🟡 Medium |
| **Phase 3: Promote Routes** | 1 week | Rename V2 → V1 | 🟠 Medium-High |
| **Phase 4: Remove Flags** | 1 week | Simplify code | 🟢 Low |
| **Phase 5: Final Cleanup** | 1 week | Delete backups | 🟢 Low |

**Total Cleanup Time:** 5 weeks

---

## 💡 Best Practices

1. **Always backup before deleting**
2. **Delete in small increments**
3. **Test after each phase**
4. **Monitor production closely**
5. **Keep rollback plan ready**
6. **Document what was removed**
7. **Update team on changes**

---

## 🎉 Expected Benefits After Cleanup

1. **Simpler Codebase** - 27% less code to maintain
2. **Faster Development** - No branching logic
3. **Better Performance** - No overhead from metrics/flags
4. **Cleaner Architecture** - Single code path
5. **Easier Onboarding** - Less to learn for new devs

---

**Cleanup Status:** 📋 **READY TO START**

Wait for V2 to be stable in production before starting cleanup!
