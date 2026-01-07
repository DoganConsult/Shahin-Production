# EvidenceService Fix Plan - Complete Migration to IDbContextFactory

**Date:** 2025-01-06  
**Status:** 🔴 CRITICAL - Compilation Errors (25 errors)  
**Priority:** P0 - Blocks Build

---

## Problem Analysis

### Current State
- ✅ Constructor correctly uses `IDbContextFactory<GrcDbContext>`
- ✅ `GetAllAsync()` correctly uses factory pattern
- ❌ **11 methods still use `_context` (doesn't exist)** - causing 25 compilation errors

### Error Summary
1. **Missing `_context` references** (lines 65, 115, 116, 141, 154, 155, 174, 181, 182, 200, 223, 247, 270, 293)
2. **Methods affected:**
   - `GetByIdAsync()` - line 65
   - `CreateAsync()` - lines 115-116
   - `UpdateAsync()` - lines 141, 154-155
   - `DeleteAsync()` - lines 174, 181-182
   - `GetByTypeAsync()` - line 200
   - `GetByClassificationAsync()` - line 223
   - `GetExpiringEvidencesAsync()` - line 247
   - `GetByAuditIdAsync()` - line 270
   - `GetStatisticsAsync()` - line 293

---

## Fix Strategy

### Pattern to Follow (from `GetAllAsync()`)
```csharp
public async Task<IEnumerable<EvidenceDto>> GetAllAsync()
{
    try
    {
        await using var context = _contextFactory.CreateDbContext();
        var evidences = await context.Evidences
            .AsNoTracking()
            .Select(e => MapToDto(e))
            .ToListAsync();
        
        _logger.LogInformation($"Retrieved {evidences.Count} evidences from database");
        return evidences;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error getting all evidences: {ex.Message}");
        throw;
    }
}
```

### Key Principles
1. **Always use `await using var context = _contextFactory.CreateDbContext();`**
2. **For read operations:** Use `AsNoTracking()` for performance
3. **For write operations:** No `AsNoTracking()`, use `context.SaveChangesAsync()`
4. **Proper disposal:** `await using` ensures context is disposed
5. **Error handling:** Keep existing try-catch blocks
6. **Logging:** Keep existing logging statements

---

## Detailed Fix Plan

### 1. `GetByIdAsync()` - Read Operation
**Current (BROKEN):**
```csharp
var evidence = await _context.Evidences
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == id);
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidence = await context.Evidences
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == id);
```

---

### 2. `CreateAsync()` - Write Operation
**Current (BROKEN):**
```csharp
_context.Evidences.Add(evidence);
await _context.SaveChangesAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
context.Evidences.Add(evidence);
await context.SaveChangesAsync();
```

**Note:** Policy enforcement happens BEFORE context creation (correct as-is)

---

### 3. `UpdateAsync()` - Write Operation
**Current (BROKEN):**
```csharp
var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == id);
// ... update properties ...
_context.Evidences.Update(evidence);
await _context.SaveChangesAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidence = await context.Evidences.FirstOrDefaultAsync(e => e.Id == id);
// ... update properties ...
context.Evidences.Update(evidence);
await context.SaveChangesAsync();
```

---

### 4. `DeleteAsync()` - Write Operation
**Current (BROKEN):**
```csharp
var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == id);
_context.Evidences.Remove(evidence);
await _context.SaveChangesAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidence = await context.Evidences.FirstOrDefaultAsync(e => e.Id == id);
context.Evidences.Remove(evidence);
await context.SaveChangesAsync();
```

---

### 5. `GetByTypeAsync()` - Read Operation
**Current (BROKEN):**
```csharp
var evidences = await _context.Evidences
    .AsNoTracking()
    .Where(e => e.Type == type)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidences = await context.Evidences
    .AsNoTracking()
    .Where(e => e.Type == type)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

---

### 6. `GetByClassificationAsync()` - Read Operation
**Current (BROKEN):**
```csharp
var evidences = await _context.Evidences
    .AsNoTracking()
    .Where(e => e.Type == classification)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidences = await context.Evidences
    .AsNoTracking()
    .Where(e => e.Type == classification)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

---

### 7. `GetExpiringEvidencesAsync()` - Read Operation
**Current (BROKEN):**
```csharp
var expiryDate = DateTime.UtcNow.AddDays(days);
var evidences = await _context.Evidences
    .AsNoTracking()
    .Where(e => e.VerificationDate.HasValue && e.VerificationDate <= expiryDate)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var expiryDate = DateTime.UtcNow.AddDays(days);
var evidences = await context.Evidences
    .AsNoTracking()
    .Where(e => e.VerificationDate.HasValue && e.VerificationDate <= expiryDate)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

---

### 8. `GetByAuditIdAsync()` - Read Operation
**Current (BROKEN):**
```csharp
var evidences = await _context.Evidences
    .AsNoTracking()
    .Where(e => e.AuditId == auditId)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidences = await context.Evidences
    .AsNoTracking()
    .Where(e => e.AuditId == auditId)
    .Select(e => MapToDto(e))
    .ToListAsync();
```

---

### 9. `GetStatisticsAsync()` - Read Operation
**Current (BROKEN):**
```csharp
var evidences = await _context.Evidences.AsNoTracking().ToListAsync();
```

**Fixed:**
```csharp
await using var context = _contextFactory.CreateDbContext();
var evidences = await context.Evidences.AsNoTracking().ToListAsync();
```

---

## Implementation Checklist

- [ ] Fix `GetByIdAsync()` - Add context creation
- [ ] Fix `CreateAsync()` - Add context creation before Add/SaveChanges
- [ ] Fix `UpdateAsync()` - Add context creation before query/update
- [ ] Fix `DeleteAsync()` - Add context creation before query/remove
- [ ] Fix `GetByTypeAsync()` - Add context creation
- [ ] Fix `GetByClassificationAsync()` - Add context creation
- [ ] Fix `GetExpiringEvidencesAsync()` - Add context creation
- [ ] Fix `GetByAuditIdAsync()` - Add context creation
- [ ] Fix `GetStatisticsAsync()` - Add context creation
- [ ] Verify build compiles (0 errors)
- [ ] Verify all methods follow consistent pattern
- [ ] Verify proper disposal (`await using`)
- [ ] Verify read operations use `AsNoTracking()`
- [ ] Verify write operations don't use `AsNoTracking()`

---

## Testing Strategy

### Unit Tests (After Fix)
1. **GetByIdAsync** - Verify returns correct evidence
2. **CreateAsync** - Verify creates evidence and enforces policies
3. **UpdateAsync** - Verify updates evidence correctly
4. **DeleteAsync** - Verify deletes evidence correctly
5. **GetByTypeAsync** - Verify filters by type
6. **GetByClassificationAsync** - Verify filters by classification
7. **GetExpiringEvidencesAsync** - Verify date filtering
8. **GetByAuditIdAsync** - Verify audit ID filtering
9. **GetStatisticsAsync** - Verify statistics calculation

### Integration Tests
1. Verify tenant isolation (each tenant sees only their data)
2. Verify context disposal (no connection leaks)
3. Verify concurrent operations (multiple tenants)

---

## Risk Assessment

### Low Risk ✅
- **Pattern is established** - `GetAllAsync()` already works correctly
- **No logic changes** - Only adding context creation
- **Backward compatible** - Same method signatures
- **No breaking changes** - Interface unchanged

### Mitigation
- Follow exact pattern from `GetAllAsync()`
- Test each method after fix
- Verify build before committing

---

## Success Criteria

1. ✅ **Build succeeds** (0 compilation errors)
2. ✅ **All 11 methods use factory pattern**
3. ✅ **Consistent code style** (matches `GetAllAsync()`)
4. ✅ **Proper resource disposal** (`await using`)
5. ✅ **No performance regression** (AsNoTracking for reads)
6. ✅ **Tenant isolation maintained** (via factory)

---

## Post-Fix Validation

```bash
# 1. Build verification
dotnet build src/GrcMvc/GrcMvc.csproj

# 2. Run tests
dotnet test tests/GrcMvc.Tests/

# 3. Verify no _context references remain
grep -r "_context\." src/GrcMvc/Services/Implementations/EvidenceService.cs
# Should return 0 results

# 4. Verify all methods use factory
grep -c "_contextFactory.CreateDbContext()" src/GrcMvc/Services/Implementations/EvidenceService.cs
# Should return 10 (9 fixes + 1 existing)
```

---

**Next Step:** Implement fixes following this plan
