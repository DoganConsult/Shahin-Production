# Phase 0 & Phase 1 Fixes - Implementation Plan

**Created**: 2026-01-14
**Status**: 🔴 In Progress
**Priority**: P0 - CRITICAL

---

## 🔴 PHASE 0: Critical Security Fixes (IMMEDIATE)

### Security Issue 1: Hardcoded Database Credentials
**File**: `scripts/create-user.cs:15`
**Status**: ✅ COMPLETED
- [x] Remove hardcoded fallback credentials
- [x] Add validation to require CONNECTION_STRING environment variable
- [x] Update documentation with required environment variables

### Security Issue 2: SQL Injection Vulnerability
**File**: `src/GrcMvc/Services/Implementations/TenantDatabaseResolver.cs:109`
**Status**: ✅ COMPLETED
- [x] Implement database name validation/sanitization
- [x] Add whitelist pattern for database names (regex validation)
- [x] Use quoted identifiers for defense in depth
- [x] Add validation before database creation

### Security Issue 3: Input Validation
**Status**: 🟡 IN PROGRESS
- [x] Add input sanitization helpers (ModelStateValidator.cs)
- [x] Create validation attribute library (ValidationAttributes.cs)
- [ ] Add ModelState validation to all API endpoints
- [ ] Implement schema validation for webhook endpoints

---

## 🔴 PHASE 1A: Critical Error Fixes

### 1A.1: Result<T> Pattern Infrastructure (4 hours)
**Status**: ✅ COMPLETED
- [x] Create `src/GrcMvc/Common/Results/Result.cs`
- [x] Create `src/GrcMvc/Common/Results/ResultT.cs`
- [x] Create `src/GrcMvc/Common/Results/Error.cs`
- [x] Create `src/GrcMvc/Common/Results/ErrorCode.cs`
- [x] Create `src/GrcMvc/Common/Results/ResultExtensions.cs`
- [x] Create `src/GrcMvc/Common/Guards/Guard.cs`

### 1A.2: Fix RiskService.cs (6 hours)
**Status**: ✅ COMPLETE (Already using Result<T> pattern)
**Note**: RiskService.cs already properly implements Result<T> pattern throughout
- [x] All methods return Result<T> or Result
- [x] Guard clauses used for null checks
- [x] No KeyNotFoundException thrown
- [x] Proper error handling with Result pattern

### 1A.3: Fix SerialCodeService.cs (8 hours)
**Status**: 🔴 NEEDS REFACTORING
**Errors**: 13 validation/state errors (ArgumentException, InvalidOperationException)
**Locations to fix**:
- [ ] Line 46: GenerateAsync - ArgumentException for invalid tenant code
- [ ] Line 217: Parse - ArgumentException for invalid code
- [ ] Line 291: CreateNewVersionAsync - ArgumentException for code not found
- [ ] Line 298: CreateNewVersionAsync - InvalidOperationException for max version
- [ ] Line 510: ConfirmReservationAsync - ArgumentException for invalid ID
- [ ] Line 518: ConfirmReservationAsync - ArgumentException for not found
- [ ] Line 523: ConfirmReservationAsync - InvalidOperationException for wrong status
- [ ] Line 530: ConfirmReservationAsync - InvalidOperationException for expired
- [ ] Line 579: CancelReservationAsync - ArgumentException for invalid ID
- [ ] Line 587: CancelReservationAsync - ArgumentException for not found
- [ ] Line 592: CancelReservationAsync - InvalidOperationException for wrong status
- [ ] Line 626: VoidAsync - ArgumentException for not found
- [ ] Line 631: VoidAsync - InvalidOperationException for already void

### 1A.4: Fix SyncExecutionService.cs (6 hours)
**Status**: ⏸️ Not Started
**Errors**: 8 workflow state errors
- [ ] Refactor all 8 error locations to use Result<T> pattern

### 1A.5: Fix VendorService.cs (4 hours)
**Status**: ⏸️ Not Started
**Errors**: 3 entity not found errors
- [ ] Refactor all 3 error locations to use Result<T> pattern

### 1A.6: Fix Null Reference Risks (16 hours)
**Status**: ⏸️ Not Started
**Errors**: 28 null reference warnings
- [ ] Create Guard.cs helper class
- [ ] Fix RiskAppetiteApiController.cs (4 errors)
- [ ] Fix WorkspaceController.cs (6 errors)
- [ ] Fix WorkflowApiController.cs (6 errors)
- [ ] Fix TenantsApiController.cs (5 errors)
- [ ] Fix WorkflowDataController.cs (6 errors)
- [ ] Fix GrcDbContext.cs (2 errors)

### 1A.7: Fix Configuration Validation (8 hours)
**Status**: ⏸️ Not Started
- [ ] Create ConfigurationValidator.cs
- [ ] Fix Program.cs configuration errors (4 locations)
- [ ] Add startup validation

---

## 🔴 PHASE 1B: Production Blockers

### 1B.1: SSL Certificates (2 hours)
**Status**: ⏸️ Not Started
- [ ] Create certificates directory
- [ ] Generate development SSL certificate
- [ ] Configure Kestrel for HTTPS
- [ ] Test HTTPS functionality

### 1B.2: Critical Environment Variables (6 hours)
**Status**: ⏸️ Not Started
- [ ] Document all required environment variables
- [ ] Create .env.template file
- [ ] Add validation for required variables
- [ ] Update configuration documentation

### 1B.3: Database Backups (9 hours)
**Status**: ⏸️ Not Started
- [ ] Create backup script
- [ ] Configure backup schedule
- [ ] Test backup and restore
- [ ] Document backup procedures

### 1B.4: Critical Monitoring (7 hours)
**Status**: ⏸️ Not Started
- [ ] Configure Application Insights
- [ ] Setup centralized logging
- [ ] Configure alerting rules
- [ ] Test monitoring

---

## Progress Tracking

| Phase | Total Items | Completed | Remaining | % Complete |
|-------|-------------|-----------|-----------|------------|
| Phase 0 | 3 | 2 | 1 | 67% |
| Phase 1A | 73 | 11 | 62 | 15% |
| Phase 1B | 28 | 0 | 28 | 0% |
| **TOTAL** | **104** | **13** | **91** | **13%** |

## Completed Items (13)

### Phase 0 - Security Fixes (2/3)
1. ✅ Fixed hardcoded credentials in create-user.cs
2. ✅ Fixed SQL injection in TenantDatabaseResolver.cs

### Phase 1A - Infrastructure & Validation (11/73)
1. ✅ Created Result.cs
2. ✅ Created ResultT.cs
3. ✅ Created Error.cs
4. ✅ Created ErrorCode.cs
5. ✅ Created ResultExtensions.cs
6. ✅ Created Guard.cs
7. ✅ Created ModelStateValidator.cs
8. ✅ Created ValidationAttributes.cs (10 custom attributes)
9. ✅ Verified RiskService.cs (already using Result<T>)
10. ✅ Documented SerialCodeService.cs refactoring needs
11. ✅ Created comprehensive documentation (3 files)

---

## Next Steps

1. Start with Phase 0 security fixes (CRITICAL)
2. Implement Result<T> pattern infrastructure
3. Begin refactoring services to use Result<T>
4. Fix null reference warnings
5. Setup production infrastructure

**Last Updated**: 2026-01-14 (Updated)
**Current Focus**: Phase 1A - Refactoring Services
**Next Task**: Refactor RiskService.cs to use Result<T> pattern
