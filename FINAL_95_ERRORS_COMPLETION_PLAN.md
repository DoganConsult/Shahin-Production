# Final 95 Errors Completion Plan

**OUTSTANDING PROGRESS:** 252 → 95 errors (62% reduction!)  
**Target:** 0 build errors for 100% completion  

---

## 🎯 Error Categories (95 Remaining)

### **1. Property Setter Issues (35 errors)**
**Pattern:** `user.UserName = email;` `user.Email = email;` `user.EmailConfirmed = true;`  
**Files:** UserInvitationService.cs, AuthenticationService.Identity.cs, OwnerTenantService.cs  
**Fix:** Use `UserManager.SetUserNameAsync()`, `SetEmailAsync()`, `ConfirmEmailAsync()`  

### **2. ID Conversion Issues (40 errors)**
**Pattern:** `method(user.Id)` where method expects string  
**Files:** Various service files  
**Fix:** Add `.ToString()` → `method(user.Id.ToString())`

### **3. Entity ID Assignment Issues (10 errors)**  
**Pattern:** `entity.Id = Guid.NewGuid();`  
**Files:** TrialLifecycleService.cs, PasswordHistoryService.cs, ShahinApiController.cs  
**Fix:** Remove manual ID assignment (ABP auto-generates)

### **4. DbContext Property Issues (5 errors)**
**Pattern:** `context.UserRoles`, `context.Roles` (don't exist in ABP)
**Files:** OwnerSetupService.cs, UserDirectoryService.cs
**Fix:** Use `context.Set<IdentityUserRole>()` or ABP repositories

### **5. Missing Property Issues (5 errors)**
**Pattern:** `tenant.Labels` (property doesn't exist in ABP Tenant)
**Files:** SerialNumberService.cs
**Fix:** Add property to Tenant entity or update logic

---

## 🚀 SYSTEMATIC COMPLETION PLAN

### **Phase 1: Service Property Setters (15 minutes)**
Fix property setter issues in services:
- ✅ UserInvitationService.cs
- ✅ AuthenticationService.Identity.cs  
- ✅ OwnerTenantService.cs
- ✅ TrialLifecycleService.cs

### **Phase 2: Service ID Conversions (15 minutes)**
Fix ID conversion issues in services:
- ✅ Add `.ToString()` to all `user.Id` parameters
- ✅ Fix comparison operations
- ✅ Fix method arguments

### **Phase 3: Entity ID Assignments (10 minutes)**
Fix entity ID assignments:
- ✅ Remove `Id = Guid.NewGuid()` from entity creation
- ✅ Let ABP auto-generate IDs

### **Phase 4: DbContext Issues (10 minutes)**
Fix DbContext property access:
- ✅ Replace `context.UserRoles` with `context.Set<IdentityUserRole>()`
- ✅ Replace `context.Roles` with `context.Set<IdentityRole>()`

### **Phase 5: Add Missing Properties (5 minutes)**
Add missing properties to Tenant:
- ✅ Add `Labels` property if needed

---

## ⏱️ **TOTAL ESTIMATED TIME: 1 Hour**

**This is the final sprint!** 🏁

Ready to complete the remaining 95 errors systematically?