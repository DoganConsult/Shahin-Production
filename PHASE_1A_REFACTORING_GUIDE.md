# Phase 1A: Service Refactoring Guide

**Created**: 2026-01-10
**Purpose**: Detailed guide for refactoring services to use Result<T> pattern
**Status**: 3/100 errors fixed (3% complete)

---

## ✅ Completed (3 items)

### Infrastructure Created
1. ✅ **Result<T> Pattern** - `/src/GrcMvc/Common/Results/`
   - Error.cs
   - ErrorCodes.cs
   - Result.cs
   - ResultT.cs
   - ResultExtensions.cs

2. ✅ **Guard Helpers** - `/src/GrcMvc/Common/Guards/`
   - Guard.cs
   - ObjectExtensions.cs

3. ✅ **RiskService.cs** - Already using Result<T> pattern correctly
   - No KeyNotFoundException found
   - Proper error handling in place

---

## 🔴 Pending Refactoring (97 errors)

### 1. SerialCodeService.cs (13 errors) 🔴

**File**: [src/GrcMvc/Services/Implementations/SerialCodeService.cs](src/GrcMvc/Services/Implementations/SerialCodeService.cs)

#### Errors to Fix:

| Line | Current Code | Error Type | Fix Required |
|------|--------------|------------|--------------|
| 46 | `throw new ArgumentException($"Invalid tenant code...")` | Validation | Return `Result<SerialCodeResult>.Failure()` |
| 217 | `throw new ArgumentException($"Invalid serial code...")` | Parsing | Change `Parse()` to return `Result<ParsedSerialCode>` |
| 291 | `throw new ArgumentException($"Serial code not found...")` | Not Found | Return `Result<SerialCodeResult>.Failure()` |
| 298 | `throw new InvalidOperationException($"Maximum version...")` | Business Rule | Return `Result<SerialCodeResult>.Failure()` |
| 510 | `throw new ArgumentException("Invalid reservation ID")` | Validation | Return `Result.Failure()` |
| 518 | `throw new ArgumentException($"Reservation not found...")` | Not Found | Return `Result.Failure()` |
| 523 | `throw new InvalidOperationException($"Reservation is not...")` | State Error | Return `Result.Failure()` |
| 530 | `throw new InvalidOperationException($"Reservation has expired...")` | Expired | Return `Result.Failure()` |
| 579 | `throw new ArgumentException("Invalid reservation ID")` | Validation | Return `Result.Failure()` |
| 587 | `throw new ArgumentException($"Reservation not found...")` | Not Found | Return `Result.Failure()` |
| 592 | `throw new InvalidOperationException($"Cannot cancel reservation...")` | State Error | Return `Result.Failure()` |
| 626 | `throw new ArgumentException($"Serial code not found...")` | Not Found | Return `Result.Failure()` |
| 631 | `throw new InvalidOperationException($"Serial code is already void...")` | Duplicate Op | Return `Result.Failure()` |

#### Refactoring Strategy:

**Step 1**: Update method signatures to return `Result<T>`

**Before**:
```csharp
public async Task<SerialCodeResult> GenerateAsync(SerialCodeRequest request)
{
    if (!SerialCodePrefixes.IsValidTenantCode(request.TenantCode))
    {
        throw new ArgumentException($"Invalid tenant code: {request.TenantCode}...");
    }
    // ...
}
```

**After**:
```csharp
public async Task<Result<SerialCodeResult>> GenerateAsync(SerialCodeRequest request)
{
    if (!SerialCodePrefixes.IsValidTenantCode(request.TenantCode))
    {
        _logger.LogWarning("Invalid tenant code: {TenantCode}", request.TenantCode);
        return Result<SerialCodeResult>.Failure(
            ErrorCodes.ValidationFailed,
            "Invalid tenant code",
            $"Tenant code '{request.TenantCode}' must be 3-6 uppercase alphanumeric characters.");
    }
    // ...
    return Result<SerialCodeResult>.Success(result);
}
```

**Step 2**: Update `Parse()` method

**Before**:
```csharp
public ParsedSerialCode Parse(string code)
{
    var result = Validate(code);
    if (result.Parsed == null || !result.IsValid)
    {
        throw new ArgumentException($"Invalid serial code: {code}...");
    }
    return result.Parsed;
}
```

**After**:
```csharp
public Result<ParsedSerialCode> Parse(string code)
{
    var result = Validate(code);
    if (result.Parsed == null || !result.IsValid)
    {
        _logger.LogWarning("Invalid serial code format: {Code}", code);
        return Result<ParsedSerialCode>.Failure(
            ErrorCodes.InvalidFormat,
            "Invalid serial code format",
            $"Errors: {string.Join(", ", result.Errors)}");
    }
    return Result<ParsedSerialCode>.Success(result.Parsed);
}
```

**Step 3**: Update `CreateNewVersionAsync()` method

**Before**:
```csharp
public async Task<SerialCodeResult> CreateNewVersionAsync(string baseCode, string? changeReason = null)
{
    var current = await _context.Set<SerialCodeRegistry>()
        .Where(r => r.Code == baseCode && r.Status == "active")
        .FirstOrDefaultAsync();

    if (current == null)
    {
        throw new ArgumentException($"Serial code not found or not active: {baseCode}");
    }

    var newVersion = current.Version + 1;
    if (newVersion > 99)
    {
        throw new InvalidOperationException($"Maximum version (99) reached for: {baseCode}");
    }
    // ...
}
```

**After**:
```csharp
public async Task<Result<SerialCodeResult>> CreateNewVersionAsync(string baseCode, string? changeReason = null)
{
    var current = await _context.Set<SerialCodeRegistry>()
        .Where(r => r.Code == baseCode && r.Status == "active")
        .FirstOrDefaultAsync();

    if (current == null)
    {
        _logger.LogWarning("Serial code not found or not active: {BaseCode}", baseCode);
        return Result<SerialCodeResult>.Failure(
            ErrorCodes.EntityNotFound,
            "Serial code not found",
            $"Serial code '{baseCode}' not found or not active");
    }

    var newVersion = current.Version + 1;
    if (newVersion > 99)
    {
        _logger.LogWarning("Maximum version reached for serial code: {BaseCode}", baseCode);
        return Result<SerialCodeResult>.Failure(
            ErrorCodes.MaxLimitReached,
            "Maximum version reached",
            $"Cannot create version {newVersion}. Maximum allowed: 99");
    }
    // ...
    return Result<SerialCodeResult>.Success(result);
}
```

**Step 4**: Update reservation methods (Lines 510-631)

Apply same pattern to:
- `ConfirmReservation()`
- `CancelReservation()`
- `VoidSerialCode()`

**Estimated Time**: 8 hours

---

### 2. SyncExecutionService.cs (8 errors) 🔴

**File**: [src/GrcMvc/Services/Implementations/SyncExecutionService.cs](src/GrcMvc/Services/Implementations/SyncExecutionService.cs)

#### Errors to Fix:

| Line | Current Code | Error Type | Fix Required |
|------|--------------|------------|--------------|
| 49 | `throw new InvalidOperationException($"SyncJob {syncJobId} not found...")` | Not Found | Return `Result.Failure()` |
| 54 | `throw new InvalidOperationException($"SyncJob {syncJobId} is not active")` | State Error | Return `Result.Failure()` |
| 95 | `throw new InvalidOperationException($"Unknown sync direction...")` | Invalid Enum | Return `Result.Failure()` |
| 220 | `throw new InvalidOperationException($"Execution log {executionLogId} not found")` | Not Found | Return `Result.Failure()` |
| 225 | `throw new InvalidOperationException($"Cannot cancel sync job...")` | State Error | Return `Result.Failure()` |
| 244 | `throw new InvalidOperationException($"Execution log {executionLogId} not found")` | Not Found | Return `Result.Failure()` |
| 285 | `throw new InvalidOperationException($"Execution log {failedExecutionLogId} not found")` | Not Found | Return `Result.Failure()` |
| 290 | `throw new InvalidOperationException($"Can only retry failed sync jobs...")` | Business Rule | Return `Result.Failure()` |

#### Refactoring Strategy:

**Example** (Line 49-54):
```csharp
// Before:
public async Task<SyncExecutionResult> ExecuteAsync(Guid syncJobId)
{
    var syncJob = await _context.SyncJobs.FindAsync(syncJobId);
    if (syncJob == null || syncJob.IsDeleted)
    {
        throw new InvalidOperationException($"SyncJob {syncJobId} not found or deleted");
    }
    if (!syncJob.IsActive)
    {
        throw new InvalidOperationException($"SyncJob {syncJobId} is not active");
    }
    // ...
}

// After:
public async Task<Result<SyncExecutionResult>> ExecuteAsync(Guid syncJobId)
{
    var syncJob = await _context.SyncJobs.FindAsync(syncJobId);
    if (syncJob == null || syncJob.IsDeleted)
    {
        _logger.LogWarning("SyncJob {SyncJobId} not found or deleted", syncJobId);
        return Result<SyncExecutionResult>.Failure(
            ErrorCodes.EntityNotFound,
            "SyncJob not found",
            $"SyncJob {syncJobId} not found or deleted");
    }
    if (!syncJob.IsActive)
    {
        _logger.LogWarning("SyncJob {SyncJobId} is not active", syncJobId);
        return Result<SyncExecutionResult>.Failure(
            ErrorCodes.InvalidOperation,
            "SyncJob not active",
            $"SyncJob {syncJobId} is not active");
    }
    // ...
    return Result<SyncExecutionResult>.Success(result);
}
```

**Estimated Time**: 6 hours

---

### 3. VendorService.cs (3 errors) 🔴

**File**: [src/GrcMvc/Services/Implementations/VendorService.cs](src/GrcMvc/Services/Implementations/VendorService.cs)

#### Errors to Fix:

| Line | Current Code | Error Type | Fix Required |
|------|--------------|------------|--------------|
| 123 | `throw new KeyNotFoundException($"Vendor with ID {id} not found")` | Not Found | Return `Result<VendorDto>.Failure()` |
| 162 | `throw new KeyNotFoundException($"Vendor with ID {id} not found")` | Not Found | Return `Result<VendorDto>.Failure()` |
| 204 | `throw new KeyNotFoundException($"Vendor with ID {id} not found")` | Not Found | Return `Result.Failure()` |

#### Refactoring Strategy:

**Pattern to apply**:
```csharp
// Before:
public async Task<VendorDto> GetByIdAsync(Guid id)
{
    var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
    if (vendor == null)
    {
        throw new KeyNotFoundException($"Vendor with ID {id} not found");
    }
    return _mapper.Map<VendorDto>(vendor);
}

// After:
public async Task<Result<VendorDto>> GetByIdAsync(Guid id)
{
    var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
    if (vendor == null)
    {
        _logger.LogWarning("Vendor with ID {Id} not found", id);
        return Result<VendorDto>.Failure(
            ErrorCodes.EntityNotFound,
            "Vendor not found",
            $"Vendor with ID {id} does not exist");
    }
    return Result<VendorDto>.Success(_mapper.Map<VendorDto>(vendor));
}
```

**Estimated Time**: 4 hours

---

### 4. OnboardingService.cs (2 errors) 🔴

**File**: [src/GrcMvc/Services/Implementations/OnboardingService.cs](src/GrcMvc/Services/Implementations/OnboardingService.cs)

#### Errors to Fix:

| Line | Method | Error Type |
|------|--------|------------|
| 74 | `CreateTenantProfileAsync` | `throw new EntityNotFoundException("Tenant", tenantId)` |
| 150+ | Wizard validation | Similar entity not found errors |

**Pattern**: Replace with Result<T> pattern

**Estimated Time**: 4 hours

---

### 5. LlmService.cs / UnifiedAiService.cs (6 errors) 🔴

**Files**:
- [src/GrcMvc/Services/Implementations/LlmService.cs](src/GrcMvc/Services/Implementations/LlmService.cs)
- [src/GrcMvc/Services/Implementations/UnifiedAiService.cs](src/GrcMvc/Services/Implementations/UnifiedAiService.cs)

#### Errors to Fix:

| Line | Error | Type |
|------|-------|------|
| 268 | `throw new InvalidOperationException($"Unknown LLM provider...")` | Invalid Config |
| 372 | `throw new InvalidOperationException($"OpenAI API error...")` | API Error |
| 422 | `throw new InvalidOperationException($"Azure OpenAI API error...")` | API Error |
| 468 | `throw new InvalidOperationException($"Local LLM error...")` | API Error |
| 650 | `throw new InvalidOperationException("Azure OpenAI requires...")` | Missing Config |
| 764 | `throw new InvalidOperationException("Custom provider requires...")` | Missing Config |

**Pattern**:
- Configuration errors → `ErrorCodes.ConfigurationMissing`
- API errors → `ErrorCodes.ExternalApiFailure`

**Estimated Time**: 6 hours

---

### 6. UnitOfWork.cs (3 errors) 🔴

**File**: [src/GrcMvc/Data/UnitOfWork.cs](src/GrcMvc/Data/UnitOfWork.cs)

#### Errors to Fix:

| Line | Error | Type |
|------|-------|------|
| 272 | `throw new InvalidOperationException("A transaction is already in progress")` | Transaction State |
| 282 | `throw new InvalidOperationException("No transaction in progress to commit")` | Transaction State |
| 306 | `throw new InvalidOperationException("No transaction in progress to rollback")` | Transaction State |

**Pattern**: Change transaction methods to return `Result`

**Estimated Time**: 3 hours

---

### 7. Program.cs (4 errors) 🔴

**File**: [src/GrcMvc/Program.cs](src/GrcMvc/Program.cs)

#### Errors to Fix:

| Line | Error | Type |
|------|-------|------|
| 293 | `throw new InvalidOperationException(...)` | Missing Config |
| 319 | `throw new InvalidOperationException("Connection string not configured")` | Missing Config |
| 404 | `throw new InvalidOperationException(...)` | Missing Config |
| 949 | `throw new InvalidOperationException("Connection string not configured for Hangfire")` | Missing Config |

**Pattern**: Replace with startup validation using ConfigurationValidator

**Estimated Time**: 4 hours (covered in Action 1A.3)

---

### 8. Controller Null Checks (28 errors) 🔴

#### Controllers to Refactor:

1. **RiskAppetiteApiController.cs** (4 errors)
   - Lines: 105, 238, 299, 338
   - Pattern: `if (entity == null) return NotFound();`

2. **WorkspaceController.cs** (6 errors)
   - Lines: 101, 166, 247, 290, 298, 395

3. **WorkflowApiController.cs** (6 errors)
   - Lines: 120, 239, 287, 341, 411, 450

4. **TenantsApiController.cs** (5 errors)
   - Lines: 116, 139, 218, 253, 278

5. **WorkflowDataController.cs** (6 errors)
   - Lines: 266, 292, 405, 430, 605, 631

6. **GrcDbContext.cs** (2 errors)
   - Lines: 35, 56

**Refactoring Pattern**:
```csharp
// Before:
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var entity = await _service.GetByIdAsync(id);
    if (entity == null)
        return NotFound();
    return Ok(entity);
}

// After (Option 1: Service returns Result<T>):
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var result = await _service.GetByIdAsync(id);
    if (result.IsFailure)
        return NotFound(new { error = result.Error.Message, code = result.Error.Code });
    return Ok(result.Value);
}

// After (Option 2: Controller uses ToResult extension):
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var entity = await _service.GetByIdAsync(id);
    var result = entity.ToResult("Entity", id);
    if (result.IsFailure)
        return NotFound(new { error = result.Error.Message, code = result.Error.Code });
    return Ok(result.Value);
}
```

**Estimated Time**: 15 hours total

---

## Implementation Order (Recommended)

### Week 1: Core Services
1. ✅ Result<T> infrastructure (4h) - DONE
2. ✅ Guard helpers (2h) - DONE
3. ⏸️ SerialCodeService.cs (8h)
4. ⏸️ SyncExecutionService.cs (6h)
5. ⏸️ VendorService.cs (4h)

**Week 1 Total**: 24 hours (3 days)

### Week 2: Supporting Services & Controllers
6. ⏸️ OnboardingService.cs (4h)
7. ⏸️ LlmService.cs / UnifiedAiService.cs (6h)
8. ⏸️ UnitOfWork.cs (3h)
9. ⏸️ Controller refactoring (15h)

**Week 2 Total**: 28 hours (3.5 days)

### Week 3: Testing & Validation
10. ⏸️ Unit tests for Result<T> patterns (8h)
11. ⏸️ Integration tests (8h)
12. ⏸️ Update API documentation (4h)
13. ⏸️ Code review & fixes (4h)

**Week 3 Total**: 24 hours (3 days)

---

## Testing Strategy

### Unit Tests Required:

1. **Result<T> Infrastructure Tests**
```csharp
[Fact]
public void Result_Success_ShouldHaveValue()
{
    var result = Result<string>.Success("test");
    Assert.True(result.IsSuccess);
    Assert.Equal("test", result.Value);
    Assert.Null(result.Error);
}

[Fact]
public void Result_Failure_ShouldHaveError()
{
    var result = Result<string>.Failure("CODE", "Message");
    Assert.True(result.IsFailure);
    Assert.Null(result.Value);
    Assert.NotNull(result.Error);
}
```

2. **Service Tests**
```csharp
[Fact]
public async Task GetByIdAsync_NotFound_ShouldReturnFailure()
{
    var result = await _vendorService.GetByIdAsync(Guid.NewGuid());
    Assert.True(result.IsFailure);
    Assert.Equal(ErrorCodes.EntityNotFound, result.Error.Code);
}
```

---

## Success Criteria

- [ ] All 97 errors fixed
- [ ] No `KeyNotFoundException`, `ArgumentException`, or `InvalidOperationException` thrown in business logic
- [ ] All services return `Result<T>` or `Result`
- [ ] All controllers handle Result failures properly
- [ ] Unit test coverage >80%
- [ ] Integration tests passing
- [ ] API documentation updated

---

## Next Steps

**Option 1: Continue Automated Refactoring**
- I can continue refactoring SerialCodeService.cs and other services
- Estimated time: 40 hours of refactoring work

**Option 2: Create Example Refactoring**
- I create one fully refactored example (SerialCodeService.cs)
- You review and approve the pattern
- I proceed with remaining services

**Option 3: Parallel Work**
- I start Phase 1B (SSL certificates, env vars) while you review this guide
- We can refactor services in parallel with infrastructure work

**Which option would you prefer?**

---

**Document Status**: Ready for Review
**Created**: 2026-01-10
**Last Updated**: 2026-01-10
