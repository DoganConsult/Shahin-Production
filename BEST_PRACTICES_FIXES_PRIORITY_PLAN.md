# Best Practices Violations - Fix Priority Plan

## Executive Summary

**Total Issues Identified:** 20 categories across multiple severity levels
**Critical Issues:** 5 (Security vulnerabilities, production blockers)
**High Priority:** 9 (Maintainability, performance, security risks)
**Medium Priority:** 6 (Code quality, best practices)

---

## ✅ IMMEDIATE FIXES (Starting Now)

### 1. 🔴 CRITICAL: AuthenticationService Mock Implementation
**Status:** In Progress
**File:** `src/GrcMvc/Services/Implementations/AuthenticationService.cs`
**Issue:** 
- Mock users hardcoded
- Base64 token generation (insecure)
- No password validation
- In-memory token store

**Fix:** Replace with Identity-based implementation using:
- `UserManager<ApplicationUser>` for user management
- `SignInManager<ApplicationUser>` for authentication
- `JwtSecurityTokenHandler` for proper JWT generation
- Database-backed token storage

**Reference:** AccountController already has proper Identity implementation (lines 900-960)

### 2. 🔴 CRITICAL: Missing CSRF Protection
**Status:** Pending
**Files:** ~65 [HttpPost] actions missing [ValidateAntiForgeryToken]
**Priority:** Start with authentication endpoints

**Fix Plan:**
1. AccountController - Already has most (verify all)
2. TrialController - Add to Register POST
3. OnboardingController - Add to all POST actions
4. LandingController - Add to contact form POST
5. API controllers - Note: API endpoints use JWT, not CSRF tokens

### 3. 🔴 CRITICAL: Rate Limiting Coverage
**Status:** Partial (3 endpoints only)
**Current:** AccountController Login, AccountControllerV2 Login
**Needed:** All authentication endpoints

**Fix Plan:**
- Add `[EnableRateLimiting("auth")]` to:
  - AccountController: Login, Register, ForgotPassword, ResetPassword
  - TrialController: Register
  - OnboardingController: All POST actions

---

## ⏳ HIGH PRIORITY (Next Phase)

### 4. 🟠 HIGH: Generic Exception Handling
**Status:** Pending
**Count:** 1218 catch (Exception) blocks across 128 files
**Strategy:** Fix high-traffic controllers first

**Priority Files:**
1. AccountController.cs - 7 instances
2. RiskApiController.cs - 29 instances
3. SeedController.cs - 26 instances
4. ApiController.cs - 63 instances

**Fix Pattern:**
```csharp
// Before
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    return BadRequest();
}

// After
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database update failed");
    return StatusCode(500, "Database error");
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning(ex, "Unauthorized access");
    return Unauthorized();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500, "Internal server error");
}
```

### 5. 🟠 HIGH: Sync-over-Async Anti-Pattern
**Status:** Pending
**Count:** 34 instances in 9 files
**Files:**
- VendorService.cs
- LandingController.cs
- ObjectExtensions.cs
- Guard.cs
- ResultExtensions.cs

**Fix Pattern:**
```csharp
// Before
var result = SomeAsyncMethod().Result;

// After
var result = await SomeAsyncMethod();
```

### 6. 🟠 HIGH: DbContext in Controllers
**Status:** Pending
**Count:** 66 controllers
**Strategy:** Create Application Services layer incrementally

**Priority Controllers:**
1. AccountController - Already uses services, but still has _context
2. RiskApiController - High traffic
3. DashboardApiController - High traffic

**Fix Pattern:**
```csharp
// Before
public class RiskController : Controller
{
    private readonly GrcDbContext _context;
    
    public async Task<IActionResult> GetRisks()
    {
        var risks = await _context.Risks.ToListAsync();
        return Ok(risks);
    }
}

// After
public class RiskController : Controller
{
    private readonly IRiskAppService _riskAppService;
    
    public async Task<IActionResult> GetRisks()
    {
        var risks = await _riskAppService.GetAllAsync();
        return Ok(risks);
    }
}
```

---

## 📋 MEDIUM PRIORITY (Future Work)

### 7. 🟡 MEDIUM: Raw SQL Queries
**Count:** 71 instances in 31 files
**Risk:** SQL injection potential
**Fix:** Use EF Core LINQ or parameterized queries

### 8. 🟡 MEDIUM: Generic Exception Throwing
**Count:** 31 instances in 6 files
**Fix:** Create custom domain exceptions

### 9. 🟡 MEDIUM: async void Methods
**Count:** 2 instances
**Fix:** Return Task instead

### 10. 🟡 MEDIUM: TODO/FIXME Comments
**Count:** 7 instances
**Fix:** Resolve or create tickets

### 11. 🟡 MEDIUM: Backup Files in Codebase
**Count:** ~50 .backup-* files
**Fix:** Remove from source control, add to .gitignore

---

## 🎯 IMPLEMENTATION STRATEGY

### Phase 1: Critical Security (Now)
1. ✅ Fix AuthenticationService (Identity-based)
2. ⏳ Add CSRF protection to auth endpoints
3. ⏳ Expand rate limiting coverage

### Phase 2: High Priority (Next)
1. Fix exception handling in top 5 controllers
2. Fix sync-over-async in critical services
3. Start Application Services layer (3-5 controllers)

### Phase 3: Medium Priority (Future)
1. Raw SQL → EF Core migration
2. Custom exception types
3. Code cleanup (TODOs, backup files)

---

## 📊 PROGRESS TRACKING

| Category | Total | Fixed | Remaining | % Complete |
|----------|-------|-------|-----------|------------|
| 🔴 CRITICAL | 5 | 0 | 5 | 0% |
| 🟠 HIGH | 9 | 0 | 9 | 0% |
| 🟡 MEDIUM | 6 | 0 | 6 | 0% |
| **TOTAL** | **20** | **0** | **20** | **0%** |

---

## NOTES

- **AuthenticationService:** AccountController already has proper Identity implementation - can be used as reference
- **CSRF:** API endpoints using JWT don't need CSRF tokens (different security model)
- **DbContext:** Large refactoring - should be done incrementally with tests
- **Exception Handling:** Massive scope - prioritize by traffic/risk
