# Missing Items Summary - GRC Policy Enforcement

## 🔴 CRITICAL (Must implement for production)

### 1. IGovernedResource Interface + BaseEntity Update
**Status:** ❌ NOT IMPLEMENTED  
**Files Missing:**
- `src/GrcMvc/Models/Interfaces/IGovernedResource.cs` - NEW
- `src/GrcMvc/Models/Entities/BaseEntity.cs` - NEEDS UPDATE

**What's needed:**
```csharp
public interface IGovernedResource
{
    string ResourceType { get; }
    string? Owner { get; set; }
    string? DataClassification { get; set; }
    Dictionary<string, string> Labels { get; }
}

public abstract class BaseEntity : IGovernedResource
{
    // Add these properties:
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    [NotMapped] public abstract string ResourceType { get; }
}
```

---

### 2. Controller Integration (PolicyEnforcer)
**Status:** ❌ NOT IMPLEMENTED (0% integration)  
**Critical Finding:** Policy engine exists but NO controllers are using it.

**Files that NEED updates:**
- `src/GrcMvc/Controllers/EvidenceController.cs`
- `src/GrcMvc/Controllers/AssessmentController.cs`
- `src/GrcMvc/Controllers/PolicyController.cs`
- `src/GrcMvc/Controllers/RiskController.cs`
- `src/GrcMvc/Controllers/AuditController.cs`
- `src/GrcMvc/Controllers/ControlController.cs`
- And 12 more module controllers...

**Required changes per controller:**
1. Inject `PolicyEnforcementHelper`
2. Add `await _policyHelper.EnforceCreateAsync()` before service calls
3. Add try/catch for `PolicyViolationException`
4. Update `[Authorize]` to use `GrcPermissions.{Module}.{Action}`

---

## 🟡 HIGH PRIORITY (Required for confidence)

### 3. Role Seeding
**Status:** ❌ NOT IMPLEMENTED  
**File Missing:** `src/GrcMvc/Data/Seed/GrcRoleDataSeedContributor.cs`

**What's needed:**
- Create default roles: SuperAdmin, TenantAdmin, ComplianceManager, RiskManager, Auditor, EvidenceOfficer, VendorManager, Viewer
- Grant permissions to roles
- Register in `ApplicationInitializer`

---

### 4. Unit Tests (Policy Engine Internals)
**Status:** ⚠️ PARTIAL (1/4 files exist)

**Files Missing:**
- `tests/GrcMvc.Tests/Unit/DotPathResolverTests.cs` - ❌ MISSING (need 8 tests)
- `tests/GrcMvc.Tests/Unit/MutationApplierTests.cs` - ❌ MISSING (need 6 tests)
- `tests/GrcMvc.Tests/Unit/PolicyEnforcerTests.cs` - ❌ MISSING (need 10 tests)
- `tests/GrcMvc.Tests/Unit/PolicyStoreTests.cs` - ❌ MISSING (need 5 tests)

**Existing (may need expansion):**
- `tests/GrcMvc.Tests/Unit/PolicyEngineTests.cs` - ✅ EXISTS (scope unknown)

---

### 5. Integration Tests (End-to-End Policy Enforcement)
**Status:** ❌ NOT IMPLEMENTED

**File Missing:** `tests/GrcMvc.Tests/Integration/PolicyEnforcementIntegrationTests.cs`

**Required test cases:**
1. Evidence create denied if dataClassification missing
2. Evidence create denied if owner missing
3. Evidence restricted in prod requires approvedForProd=true
4. Exception in dev allows restricted without approval
5. Mutation normalizes empty owner to null
6. Policy audit log records decisions
7. DenyOverrides conflict resolution works
8. AllowOverrides conflict resolution works

---

## 🟢 MEDIUM PRIORITY (UX enhancements)

### 6. Blazor UI Policy Guards
**Status:** ❌ NOT IMPLEMENTED

**Files Missing:**
- `src/GrcMvc/Views/Shared/Components/PolicyViolationDialog.cshtml` - NEW
- `src/GrcMvc/wwwroot/js/policy-error-handler.js` - NEW

**What's needed:**
- Display policy violation reasons to user
- Show remediation hints
- Catch `Grc:PolicyViolation` error code from API

---

## 🟢 LOW PRIORITY (Documentation)

### 7. Policy Enforcement Guide
**Status:** ⚠️ PARTIAL (V2 migration docs exist, policy docs missing)

**File Missing:** `POLICY_ENFORCEMENT_GUIDE.md`

**Existing (not policy-focused):**
- ✅ `LEGACY_CLEANUP_GUIDE.md`
- ✅ `PARALLEL_MIGRATION_COMPLETE.md`
- ✅ `QUICK_START.md`

---

## Summary Statistics

| Category | Complete | Partial | Missing | Total |
|----------|----------|---------|---------|-------|
| Backend Core | 7 | 0 | 0 | 7 |
| Service Integration | 0 | 0 | 18 | 18 |
| Entity Metadata | 0 | 0 | 2 | 2 |
| Role Seeding | 0 | 0 | 1 | 1 |
| Unit Tests | 1 | 0 | 4 | 5 |
| Integration Tests | 0 | 0 | 1 | 1 |
| UI Guards | 0 | 0 | 2 | 2 |
| Documentation | 3 | 0 | 1 | 4 |
| **TOTAL** | **11** | **0** | **29** | **40** |

**Overall Completion: ~27.5% (11/40)**

---

## Production Readiness

### ✅ Ready for Production:
- Policy Engine Core (PolicyEnforcer, PolicyStore, DotPathResolver, etc.)
- Permissions System (GrcPermissions, PermissionDefinitionProvider)
- Menu Contributor (Arabic navigation with RBAC)

### ❌ NOT Ready for Production:
- **Controller Integration** (0% - CRITICAL BLOCKER)
- **Entity Metadata** (0% - CRITICAL BLOCKER)
- **Role Seeding** (0% - HIGH PRIORITY)
- **Test Coverage** (20% - HIGH PRIORITY)
- **UI Error Handling** (0% - MEDIUM)

---

## Recommended Implementation Order

1. **IGovernedResource + BaseEntity** (1 hour) - Unblocks policy evaluation
2. **Controller Integration** (6 hours) - Core 6 controllers (Evidence, Assessment, Policy, Risk, Audit, Control)
3. **Role Seeding** (2 hours) - Enables permission testing
4. **Unit Tests** (4 hours) - Confidence in policy engine internals
5. **Integration Tests** (2 hours) - End-to-end validation
6. **UI Guards** (2 hours) - UX improvement
7. **Documentation** (1 hour) - Knowledge transfer

**Total Estimated Time: ~18 hours**

---

**See `IMPLEMENTATION_STATUS_REPORT.md` for detailed analysis and code examples.**
