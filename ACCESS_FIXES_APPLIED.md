# Access Fixes Applied
**Date:** 2025-01-22
**Scope:** Fixes for access issues identified in ACCESS_DEBUG_REPORT.md

---

## Fixes Applied

### Fix 1: Dashboard Permission Check ✅

**File:** `src/GrcMvc/Controllers/DashboardController.cs`

**Issue:** Dashboard controller was missing permission check, only had `[Authorize]` and `[RequireTenant]`.

**Fix Applied:**
```csharp
// BEFORE:
[Authorize]
[RequireTenant]
public class DashboardMvcController : Controller

// AFTER:
[Authorize(GrcPermissions.Dashboard.Default)]
[RequireTenant]
public class DashboardMvcController : Controller
```

**Also Added:**
```csharp
using GrcMvc.Application.Permissions;
```

**Status:** ✅ **FIXED**

---

### Fix 2: EvidenceOfficer Missing Assessments Feature ✅

**File:** `src/GrcMvc/Data/Seeds/RbacSeeds.cs` - `MapRolesToFeaturesAsync`

**Issue:** EvidenceOfficer role had `GrcPermissions.Assessments.View` permission but no Assessments feature in RoleFeatures mapping, causing menu to be hidden.

**Fix Applied:**
```csharp
// BEFORE:
// EvidenceOfficer - Evidence, Dashboard
if (roles.ContainsKey("EvidenceOfficer"))
{
    var evidenceFeatures = new[] { "Home", "Dashboard", "Evidence" };

// AFTER:
// EvidenceOfficer - Evidence, Assessments (view), Dashboard
if (roles.ContainsKey("EvidenceOfficer"))
{
    var evidenceFeatures = new[] { "Home", "Dashboard", "Evidence", "Assessments" };
```

**Status:** ✅ **FIXED**

---

### Fix 3: RiskManager Missing Assessments Feature ✅

**File:** `src/GrcMvc/Data/Seeds/RbacSeeds.cs` - `MapRolesToFeaturesAsync`

**Issue:** RiskManager role had `GrcPermissions.Assessments.View` permission but no Assessments feature in RoleFeatures mapping, causing menu to be hidden.

**Fix Applied:**
```csharp
// BEFORE:
// RiskManager - Risks, ActionPlans, Reports
if (roles.ContainsKey("RiskManager"))
{
    var riskFeatures = new[] { "Home", "Dashboard", "Risks", "ActionPlans", "Reports" };

// AFTER:
// RiskManager - Risks, ActionPlans, Reports, Assessments (view)
if (roles.ContainsKey("RiskManager"))
{
    var riskFeatures = new[] { "Home", "Dashboard", "Risks", "ActionPlans", "Reports", "Assessments" };
```

**Status:** ✅ **FIXED**

---

### Fix 4: Viewer Role Verification ✅

**File:** `src/GrcMvc/Data/Seeds/RbacSeeds.cs` - `MapRolesToFeaturesAsync`

**Verification Result:** Viewer role already has ALL features mapped (line 1311-1320):
```csharp
// Viewer - All features visible (read-only)
if (roles.ContainsKey("Viewer"))
{
    foreach (var feature in features.Values)
    {
        roleFeatures.Add(new RoleFeature
        {
            RoleId = roles["Viewer"].Id,
            FeatureId = feature.Id,
            TenantId = tenantId,
            IsVisible = true,
            AssignedBy = "System"
        });
    }
}
```

**Status:** ✅ **VERIFIED - NO FIX NEEDED** (Viewer already has all features including Assessments)

---

## Summary of Changes

### Files Modified
1. ✅ `src/GrcMvc/Controllers/DashboardController.cs`
   - Added `[Authorize(GrcPermissions.Dashboard.Default)]`
   - Added `using GrcMvc.Application.Permissions;`

2. ✅ `src/GrcMvc/Data/Seeds/RbacSeeds.cs`
   - Added "Assessments" to EvidenceOfficer features array
   - Added "Assessments" to RiskManager features array
   - Verified Viewer has all features (no change needed)

### Impact
- ✅ Dashboard now properly enforces `GrcPermissions.Dashboard.Default` permission
- ✅ EvidenceOfficer role can now see Assessments menu item (if has permission)
- ✅ RiskManager role can now see Assessments menu item (if has permission)
- ✅ Viewer role already had Assessments feature (verified)

---

## Testing Required

### Test Scenarios

1. **Dashboard Access Test**
   - ✅ User with `GrcPermissions.Dashboard.Default` should access dashboard
   - ❌ User without `GrcPermissions.Dashboard.Default` should be denied

2. **EvidenceOfficer Assessment Access Test**
   - ✅ EvidenceOfficer with `GrcPermissions.Assessments.View` should see Assessments menu
   - ✅ EvidenceOfficer should be able to access `/assessments` page
   - ❌ EvidenceOfficer without permission should not see menu

3. **RiskManager Assessment Access Test**
   - ✅ RiskManager with `GrcPermissions.Assessments.View` should see Assessments menu
   - ✅ RiskManager should be able to access `/assessments` page
   - ❌ RiskManager without permission should not see menu

4. **Viewer Assessment Access Test**
   - ✅ Viewer should see Assessments menu (has all features)
   - ✅ Viewer should be able to access `/assessments` page (has View permission)
   - ❌ Viewer should NOT be able to create assessments (no Create permission)

---

## Database Seeding Impact

**Important:** The fixes to `RbacSeeds.cs` only affect NEW role-feature mappings. If the system has already been seeded, you may need to:

1. **Option A: Re-run seeding** (if safe to do so in your environment)
   ```csharp
   // The seeding method checks if mappings exist:
   if (await context.RoleFeatures.AnyAsync(rf => rf.TenantId == tenantId))
   {
       logger.LogInformation("Role-Feature mappings already exist. Skipping.");
       return;
   }
   ```
   
   **To force re-seeding:** You may need to temporarily clear existing mappings or modify the check.

2. **Option B: Manual database update** (safer for production)
   ```sql
   -- Add Assessments feature to EvidenceOfficer role
   INSERT INTO RoleFeatures (RoleId, FeatureId, TenantId, IsVisible, AssignedBy)
   SELECT r.Id, f.Id, t.Id, 1, 'System'
   FROM AspNetRoles r
   CROSS JOIN Features f
   CROSS JOIN Tenants t
   WHERE r.Name = 'EvidenceOfficer'
     AND f.Code = 'Assessments'
     AND NOT EXISTS (
         SELECT 1 FROM RoleFeatures rf 
         WHERE rf.RoleId = r.Id AND rf.FeatureId = f.Id AND rf.TenantId = t.Id
     );

   -- Add Assessments feature to RiskManager role
   INSERT INTO RoleFeatures (RoleId, FeatureId, TenantId, IsVisible, AssignedBy)
   SELECT r.Id, f.Id, t.Id, 1, 'System'
   FROM AspNetRoles r
   CROSS JOIN Features f
   CROSS JOIN Tenants t
   WHERE r.Name = 'RiskManager'
     AND f.Code = 'Assessments'
     AND NOT EXISTS (
         SELECT 1 FROM RoleFeatures rf 
         WHERE rf.RoleId = r.Id AND rf.FeatureId = f.Id AND rf.TenantId = t.Id
     );
   ```

---

## Next Steps

1. ✅ **Apply fixes to code** - COMPLETED
2. ⏳ **Verify compilation** - Run `dotnet build`
3. ⏳ **Run unit tests** - Verify no breaking changes
4. ⏳ **Update database** - Either re-seed or run SQL script
5. ⏳ **Test access scenarios** - Verify all access paths work correctly
6. ⏳ **Update documentation** - Reflect changes in system documentation

---

## Rollback Plan

If issues occur, revert changes:

1. **DashboardController.cs:**
   ```csharp
   // Revert to:
   [Authorize]
   [RequireTenant]
   ```

2. **RbacSeeds.cs:**
   ```csharp
   // Revert EvidenceOfficer:
   var evidenceFeatures = new[] { "Home", "Dashboard", "Evidence" };
   
   // Revert RiskManager:
   var riskFeatures = new[] { "Home", "Dashboard", "Risks", "ActionPlans", "Reports" };
   ```

---

**End of Fixes Summary**
