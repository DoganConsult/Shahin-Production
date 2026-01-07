# Access Control Analysis - Platform Admin, Tenant Admin, and Regular Users

**Date:** 2025-01-22  
**Status:** Analysis Complete

---

## 🔍 Current Access Control Status

### ✅ **Platform Admin Access**

**Authorization Policy:** `ActivePlatformAdmin`

**Requirements:**
1. ✅ User must have `PlatformAdmin` role
2. ✅ User must have an active `PlatformAdmin` record in database
3. ✅ `PlatformAdmin.Status` must be `"Active"`
4. ✅ `PlatformAdmin.IsDeleted` must be `false`

**Controllers Protected:**
- `PlatformAdminController` - Uses `[Authorize(Policy = "ActivePlatformAdmin")]`
- `Api/PlatformAdminController` - Uses `[Authorize(Policy = "ActivePlatformAdmin")]`

**Potential Obstacles:**
- ⚠️ **If PlatformAdmin record is deleted/suspended** → Access denied even if role exists
- ⚠️ **If Status != "Active"** → Access denied
- ⚠️ **If user ID not in claims** → Access denied

**Status:** ✅ **SECURE** - Multiple layers of verification

---

### ⚠️ **Tenant Admin Access**

**Current Implementation:**
- ✅ Role exists: `TenantAdmin`
- ✅ Route exists: `tenant/{slug}/admin/{controller=Dashboard}/{action=Index}/{id?}`
- ⚠️ **NO dedicated authorization policy** (unlike PlatformAdmin)
- ⚠️ **NO dedicated TenantAdmin controller** found
- ⚠️ **NO active verification** of TenantAdmin record

**Potential Obstacles:**
- ❌ **No `ActiveTenantAdmin` policy** - Only role-based check
- ❌ **No verification** that TenantAdmin record exists and is active
- ❌ **No tenant context validation** - Could access wrong tenant data
- ❌ **No tenant slug validation** - Route allows any slug

**Status:** ⚠️ **INCOMPLETE** - Missing security layers

---

### ⚠️ **Regular User Access**

**Current Implementation:**
- ✅ All controllers use `[Authorize]` attribute (requires authentication)
- ✅ Permission-based access via `GrcPermissions`
- ✅ Menu items use `.RequirePermissions()` for visibility
- ⚠️ **No tenant context enforcement** on all controllers
- ⚠️ **No workspace context enforcement** on all controllers

**Potential Obstacles:**
- ⚠️ **Cross-tenant data access** - If tenant context not properly set
- ⚠️ **Cross-workspace data access** - If workspace context not properly set
- ⚠️ **Permission gaps** - Some actions may not have permission checks

**Status:** ⚠️ **PARTIALLY SECURE** - Needs tenant/workspace isolation

---

## 🔒 Security Gaps Identified

### 1. **Tenant Admin - Missing Authorization Policy**

**Issue:** No `ActiveTenantAdmin` policy similar to `ActivePlatformAdmin`

**Risk:** TenantAdmin role could be assigned but user might not have active TenantAdmin record

**Recommendation:**
```csharp
// Create ActiveTenantAdminRequirement.cs
public class ActiveTenantAdminRequirement : IAuthorizationRequirement { }

// Create ActiveTenantAdminHandler.cs
// Verify: Role = TenantAdmin AND TenantAdmin record exists AND Status = Active

// Register in Program.cs
options.AddPolicy("ActiveTenantAdmin", policy =>
    policy.RequireRole("TenantAdmin")
          .AddRequirements(new ActiveTenantAdminRequirement()));
```

---

### 2. **Tenant Context Isolation**

**Issue:** Controllers don't consistently enforce tenant context

**Risk:** Users could access data from other tenants

**Current State:**
- ✅ `GrcDbContext` has tenant query filters (global)
- ⚠️ **But** - Controllers don't validate tenant context before operations
- ⚠️ **But** - Services don't always check tenant context

**Recommendation:**
- Add `[RequireTenant]` attribute to all tenant-scoped controllers
- Validate tenant context in service layer
- Add tenant ID validation in all CRUD operations

---

### 3. **Workspace Context Isolation**

**Issue:** Workspace context not enforced on all controllers

**Risk:** Users could access data from other workspaces

**Current State:**
- ✅ Some services check `_workspaceContext.HasWorkspaceContext()`
- ⚠️ **But** - Not all controllers/services enforce workspace isolation
- ⚠️ **But** - No `[RequireWorkspace]` attribute

**Recommendation:**
- Add workspace validation in all workspace-scoped operations
- Ensure workspace ID is validated before data access

---

### 4. **Permission Coverage Gaps**

**Issue:** Not all actions have permission checks

**Current State:**
- ✅ Most controllers have `[Authorize(GrcPermissions.X.Action)]`
- ⚠️ **But** - Some actions may be missing permission attributes
- ⚠️ **But** - Some services don't check permissions before operations

**Recommendation:**
- Audit all controller actions for permission attributes
- Add permission checks in service layer as backup
- Ensure all menu items have `.RequirePermissions()`

---

## 📊 Access Control Matrix

| User Type | Authentication | Role Check | Record Verification | Tenant Context | Workspace Context | Status |
|-----------|---------------|------------|-------------------|----------------|-------------------|--------|
| **Platform Admin** | ✅ Required | ✅ PlatformAdmin role | ✅ Active record check | ❌ Not enforced | ❌ Not enforced | ✅ Secure |
| **Tenant Admin** | ✅ Required | ✅ TenantAdmin role | ❌ **Missing** | ⚠️ Partial | ⚠️ Partial | ⚠️ Incomplete |
| **Regular User** | ✅ Required | ✅ Role-based | ❌ Not applicable | ⚠️ Partial | ⚠️ Partial | ⚠️ Partial |

---

## 🛠️ Recommended Fixes

### Priority 1: Tenant Admin Security

1. **Create `ActiveTenantAdminRequirement`**
   ```csharp
   // File: src/GrcMvc/Authorization/ActiveTenantAdminRequirement.cs
   public class ActiveTenantAdminRequirement : IAuthorizationRequirement { }
   
   public class ActiveTenantAdminHandler : AuthorizationHandler<ActiveTenantAdminRequirement>
   {
       // Verify: Role = TenantAdmin AND TenantAdmin record exists AND Status = Active
   }
   ```

2. **Register Policy in Program.cs**
   ```csharp
   options.AddPolicy("ActiveTenantAdmin", policy =>
       policy.RequireRole("TenantAdmin")
             .AddRequirements(new ActiveTenantAdminRequirement()));
   ```

3. **Create TenantAdminController**
   ```csharp
   [Authorize(Policy = "ActiveTenantAdmin")]
   public class TenantAdminController : Controller
   {
       // Tenant admin operations
   }
   ```

---

### Priority 2: Tenant Context Enforcement

1. **Create `RequireTenantAttribute`**
   ```csharp
   public class RequireTenantAttribute : AuthorizeAttribute
   {
       // Validate tenant context before action execution
   }
   ```

2. **Add to all tenant-scoped controllers**
   ```csharp
   [Authorize]
   [RequireTenant]
   public class RiskController : Controller { }
   ```

---

### Priority 3: Workspace Context Enforcement

1. **Create `RequireWorkspaceAttribute`**
   ```csharp
   public class RequireWorkspaceAttribute : AuthorizeAttribute
   {
       // Validate workspace context before action execution
   }
   ```

2. **Add to workspace-scoped operations**
   ```csharp
   [Authorize]
   [RequireWorkspace]
   public class WorkspaceController : Controller { }
   ```

---

### Priority 4: Permission Audit

1. **Audit all controller actions**
   - Verify every action has `[Authorize(GrcPermissions.X.Action)]`
   - Document any missing permissions

2. **Add service-layer permission checks**
   - Backup validation in service methods
   - Throw `UnauthorizedAccessException` if permission missing

---

## ✅ Current Strengths

1. ✅ **Platform Admin** - Well secured with multiple verification layers
2. ✅ **Authentication Required** - All controllers require `[Authorize]`
3. ✅ **Permission-Based Access** - Most controllers use `GrcPermissions`
4. ✅ **Policy Enforcement** - Policy engine enforces governance rules
5. ✅ **Menu Permissions** - Menu items respect permissions

---

## ⚠️ Current Weaknesses

1. ❌ **Tenant Admin** - Missing active record verification
2. ❌ **Tenant Isolation** - Not consistently enforced
3. ❌ **Workspace Isolation** - Not consistently enforced
4. ⚠️ **Permission Coverage** - May have gaps in some actions
5. ⚠️ **Tenant Slug Validation** - Route allows any slug

---

## 📝 Summary

### Platform Admin
- ✅ **Status:** SECURE
- ✅ **Obstacles:** Properly implemented (role + active record check)
- ✅ **Access:** Controlled via `ActivePlatformAdmin` policy

### Tenant Admin
- ⚠️ **Status:** INCOMPLETE
- ❌ **Obstacles:** Missing active record verification
- ⚠️ **Access:** Only role-based, no record validation

### Regular Users
- ⚠️ **Status:** PARTIALLY SECURE
- ⚠️ **Obstacles:** Tenant/workspace isolation not fully enforced
- ✅ **Access:** Permission-based, but context isolation needs improvement

---

## 🎯 Next Steps

1. **Implement `ActiveTenantAdmin` policy** (Priority 1)
2. **Add tenant context validation** to all controllers (Priority 2)
3. **Add workspace context validation** to workspace-scoped operations (Priority 3)
4. **Audit and fix permission gaps** (Priority 4)
5. **Add tenant slug validation** in routing (Priority 4)

---

**Recommendation:** Implement Priority 1 (Tenant Admin security) immediately to match Platform Admin security level.
