# SerialCodeService.cs Refactoring Plan

**Created**: 2026-01-14
**Status**: Planning Phase
**Priority**: P0 - CRITICAL
**Estimated Time**: 8 hours

---

## 📋 Overview

Refactor SerialCodeService.cs to use Result<T> pattern instead of throwing exceptions for business logic errors.

**Total Errors to Fix**: 13
- ArgumentException: 7 instances
- InvalidOperationException: 6 instances

---

## 🎯 Errors to Fix

### 1. Line 46: GenerateAsync - ArgumentException
**Current Code**:
```csharp
if (!SerialCodePrefixes.IsValidTenantCode(request.TenantCode))
{
    throw new ArgumentException($"Invalid tenant code: {request.TenantCode}...");
}
```

**Fix**:
```csharp
if (!SerialCodePrefixes.IsValidTenantCode(request.TenantCode))
{
    return Result<SerialCodeResult>.Failure(
        Error.Validation($"Invalid tenant code: {request.TenantCode}. Must be 3-6 uppercase alphanumeric characters."));
}
```

---

### 2. Line 217: Parse - ArgumentException
**Current Code**:
```csharp
public ParsedSerialCode Parse(string code)
{
    var result = Validate(code);
    if (result.Parsed == null || !result.IsValid)
    {
        throw new ArgumentException($"Invalid serial code: {code}. Errors: {string.Join(", ", result.Errors)}");
    }
    return result.Parsed;
}
```

**Fix**: Change return type to `Result<ParsedSerialCode>`
```csharp
public Result<ParsedSerialCode> Parse(string code)
{
    var result = Validate(code);
    if (result.Parsed == null || !result.IsValid)
    {
        return Result<ParsedSerialCode>.Failure(
            Error.Validation($"Invalid serial code: {code}", 
                $"Errors: {string.Join(", ", result.Errors)}"));
    }
    return Result<ParsedSerialCode>.Success(result.Parsed);
}
```

---

### 3. Line 291: CreateNewVersionAsync - ArgumentException
**Current Code**:
```csharp
if (current == null)
{
    throw new ArgumentException($"Serial code not found or not active: {baseCode}");
}
```

**Fix**:
```csharp
if (current == null)
{
    return Result<SerialCodeResult>.Failure(
        Error.NotFound("SerialCode", baseCode));
}
```

---

### 4. Line 298: CreateNewVersionAsync - InvalidOperationException
**Current Code**:
```csharp
if (newVersion > 99)
{
    throw new InvalidOperationException($"Maximum version (99) reached for: {baseCode}");
}
```

**Fix**:
```csharp
if (newVersion > 99)
{
    return Result<SerialCodeResult>.Failure(
        Error.InvalidOperation("Maximum version (99) reached", 
            $"Code: {baseCode}"));
}
```

---

### 5. Line 510: ConfirmReservationAsync - ArgumentException (Invalid ID)
**Current Code**:
```csharp
if (!Guid.TryParse(reservationId, out var id))
{
    throw new ArgumentException("Invalid reservation ID");
}
```

**Fix**:
```csharp
if (!Guid.TryParse(reservationId, out var id))
{
    return Result<SerialCodeResult>.Failure(
        Error.Validation("Invalid reservation ID format", 
            $"Provided ID: {reservationId}"));
}
```

---

### 6. Line 518: ConfirmReservationAsync - ArgumentException (Not Found)
**Current Code**:
```csharp
if (reservation == null)
{
    throw new ArgumentException($"Reservation not found: {reservationId}");
}
```

**Fix**:
```csharp
if (reservation == null)
{
    return Result<SerialCodeResult>.Failure(
        Error.NotFound("Reservation", reservationId));
}
```

---

### 7. Line 523: ConfirmReservationAsync - InvalidOperationException (Wrong Status)
**Current Code**:
```csharp
if (reservation.Status != "reserved")
{
    throw new InvalidOperationException($"Reservation is not in 'reserved' status: {reservation.Status}");
}
```

**Fix**:
```csharp
if (reservation.Status != "reserved")
{
    return Result<SerialCodeResult>.Failure(
        Error.InvalidOperation("Reservation is not in 'reserved' status", 
            $"Current status: {reservation.Status}"));
}
```

---

### 8. Line 530: ConfirmReservationAsync - InvalidOperationException (Expired)
**Current Code**:
```csharp
if (reservation.ExpiresAt < DateTime.UtcNow)
{
    reservation.Status = "expired";
    await _context.SaveChangesAsync();
    throw new InvalidOperationException($"Reservation has expired at {reservation.ExpiresAt}");
}
```

**Fix**:
```csharp
if (reservation.ExpiresAt < DateTime.UtcNow)
{
    reservation.Status = "expired";
    await _context.SaveChangesAsync();
    return Result<SerialCodeResult>.Failure(
        Error.InvalidOperation("Reservation has expired", 
            $"Expired at: {reservation.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC"));
}
```

---

### 9. Line 579: CancelReservationAsync - ArgumentException (Invalid ID)
**Current Code**:
```csharp
if (!Guid.TryParse(reservationId, out var id))
{
    throw new ArgumentException("Invalid reservation ID");
}
```

**Fix**:
```csharp
if (!Guid.TryParse(reservationId, out var id))
{
    return Result.Failure(
        Error.Validation("Invalid reservation ID format", 
            $"Provided ID: {reservationId}"));
}
```

---

### 10. Line 587: CancelReservationAsync - ArgumentException (Not Found)
**Current Code**:
```csharp
if (reservation == null)
{
    throw new ArgumentException($"Reservation not found: {reservationId}");
}
```

**Fix**:
```csharp
if (reservation == null)
{
    return Result.Failure(
        Error.NotFound("Reservation", reservationId));
}
```

---

### 11. Line 592: CancelReservationAsync - InvalidOperationException
**Current Code**:
```csharp
if (reservation.Status != "reserved")
{
    throw new InvalidOperationException($"Cannot cancel reservation in status: {reservation.Status}");
}
```

**Fix**:
```csharp
if (reservation.Status != "reserved")
{
    return Result.Failure(
        Error.InvalidOperation("Cannot cancel reservation", 
            $"Current status: {reservation.Status}"));
}
```

---

### 12. Line 626: VoidAsync - ArgumentException
**Current Code**:
```csharp
if (registry == null)
{
    throw new ArgumentException($"Serial code not found: {code}");
}
```

**Fix**:
```csharp
if (registry == null)
{
    return Result.Failure(
        Error.NotFound("SerialCode", code));
}
```

---

### 13. Line 631: VoidAsync - InvalidOperationException
**Current Code**:
```csharp
if (registry.Status == "void")
{
    throw new InvalidOperationException($"Serial code is already void: {code}");
}
```

**Fix**:
```csharp
if (registry.Status == "void")
{
    return Result.Failure(
        Error.InvalidOperation("Serial code is already void", 
            $"Code: {code}"));
}
```

---

## 📝 Method Signature Changes

### Methods Requiring Return Type Changes:

1. **GenerateAsync**
   - From: `Task<SerialCodeResult>`
   - To: `Task<Result<SerialCodeResult>>`

2. **Parse**
   - From: `ParsedSerialCode`
   - To: `Result<ParsedSerialCode>`

3. **CreateNewVersionAsync**
   - From: `Task<SerialCodeResult>`
   - To: `Task<Result<SerialCodeResult>>`

4. **ConfirmReservationAsync**
   - From: `Task<SerialCodeResult>`
   - To: `Task<Result<SerialCodeResult>>`

5. **CancelReservationAsync**
   - From: `Task`
   - To: `Task<Result>`

6. **VoidAsync**
   - From: `Task`
   - To: `Task<Result>`

7. **GetTraceabilityReportAsync**
   - From: `Task<SerialCodeTraceabilityReport>`
   - To: `Task<Result<SerialCodeTraceabilityReport>>`
   - Note: Line 645 has `throw new ArgumentException($"Serial code not found: {code}");`

---

## 🔄 Dependent Changes

### Interface Updates Required:
File: `ISerialCodeService.cs`

Update method signatures to match:
```csharp
Task<Result<SerialCodeResult>> GenerateAsync(SerialCodeRequest request);
Result<ParsedSerialCode> Parse(string code);
Task<Result<SerialCodeResult>> CreateNewVersionAsync(string baseCode, string? changeReason = null);
Task<Result<SerialCodeResult>> ConfirmReservationAsync(string reservationId, Guid entityId);
Task<Result> CancelReservationAsync(string reservationId);
Task<Result> VoidAsync(string code, string reason);
Task<Result<SerialCodeTraceabilityReport>> GetTraceabilityReportAsync(string code);
```

### Methods That Call These (Need Updates):

1. **GenerateBatchAsync** - Calls `GenerateAsync`
   - Update to handle `Result<SerialCodeResult>`
   
2. **GetHistoryAsync** - Calls `Parse`
   - Update to handle `Result<ParsedSerialCode>`
   
3. **GetLatestVersionAsync** - Calls `Parse`
   - Update to handle `Result<ParsedSerialCode>`

---

## ✅ Implementation Steps

### Step 1: Update Interface (ISerialCodeService.cs)
- [ ] Update method signatures to return Result<T>
- [ ] Add using statement for Result types

### Step 2: Update Implementation (SerialCodeService.cs)
- [ ] Add using statement: `using GrcMvc.Common.Results;`
- [ ] Fix all 13 exception throws
- [ ] Update method signatures
- [ ] Update methods that call changed methods

### Step 3: Update Callers (Controllers)
- [ ] Search for controllers using SerialCodeService
- [ ] Update to use `.ToActionResult()` pattern
- [ ] Handle Result<T> returns properly

### Step 4: Testing
- [ ] Build solution to check for compilation errors
- [ ] Update unit tests if they exist
- [ ] Manual testing of key scenarios

---

## 🧪 Testing Scenarios

### Test Cases to Verify:

1. **Invalid Tenant Code**
   - Input: Invalid tenant code format
   - Expected: Validation error, not exception

2. **Parse Invalid Code**
   - Input: Malformed serial code
   - Expected: Validation error with details

3. **Version Limit Reached**
   - Input: Code at version 99
   - Expected: InvalidOperation error

4. **Expired Reservation**
   - Input: Expired reservation ID
   - Expected: InvalidOperation error

5. **Already Void Code**
   - Input: Code already marked void
   - Expected: InvalidOperation error

---

## 📊 Success Criteria

- [ ] All 13 exception throws replaced with Result<T> pattern
- [ ] Interface updated with new signatures
- [ ] Implementation compiles without errors
- [ ] All dependent methods updated
- [ ] Controllers updated to use ToActionResult()
- [ ] No regression in functionality
- [ ] Error messages remain clear and informative

---

## 🚀 Next Steps After Completion

1. Update TODO.md to mark SerialCodeService as complete
2. Update progress tracking (13 items → 0 items)
3. Move to next service: SyncExecutionService.cs (8 errors)
4. Document lessons learned

---

**Estimated Completion Time**: 8 hours
**Actual Time**: TBD
**Status**: Ready to implement
