# Result<T> Pattern - Usage Guide

**Purpose**: Replace exceptions with explicit error handling using the Result<T> pattern
**Created**: 2026-01-14

---

## 📚 Quick Reference

### Basic Usage

```csharp
// Instead of throwing exceptions:
❌ throw new KeyNotFoundException($"Risk {id} not found");

// Use Result<T> pattern:
✅ return Result<RiskDto>.Failure(Error.NotFound("Risk", id));
```

---

## 🎯 Common Patterns

### Pattern 1: Entity Not Found

**Before (Exception-based):**
```csharp
public async Task<RiskDto> GetByIdAsync(Guid id)
{
    var risk = await _context.Risks.FindAsync(id);
    if (risk == null)
    {
        throw new KeyNotFoundException($"Risk {id} not found");
    }
    return _mapper.Map<RiskDto>(risk);
}
```

**After (Result-based):**
```csharp
public async Task<Result<RiskDto>> GetByIdAsync(Guid id)
{
    var risk = await _context.Risks.FindAsync(id);
    if (risk == null)
    {
        return Result<RiskDto>.Failure(Error.NotFound("Risk", id));
    }
    return Result<RiskDto>.Success(_mapper.Map<RiskDto>(risk));
}
```

**Using Guard Helper:**
```csharp
public async Task<Result<RiskDto>> GetByIdAsync(Guid id)
{
    var risk = await _context.Risks.FindAsync(id);
    var result = Guard.NotNull(risk, "Risk", id);
    
    if (result.IsFailure)
        return Result<RiskDto>.Failure(result.Error!);
    
    return Result<RiskDto>.Success(_mapper.Map<RiskDto>(risk));
}
```

---

### Pattern 2: Validation Errors

**Before (Exception-based):**
```csharp
public async Task<SerialCode> CreateAsync(string prefix)
{
    if (string.IsNullOrWhiteSpace(prefix))
    {
        throw new ArgumentException("Prefix cannot be empty");
    }
    
    if (prefix.Length > 10)
    {
        throw new ArgumentException("Prefix cannot exceed 10 characters");
    }
    
    // Create serial code...
}
```

**After (Result-based):**
```csharp
public async Task<Result<SerialCode>> CreateAsync(string prefix)
{
    if (string.IsNullOrWhiteSpace(prefix))
    {
        return Result<SerialCode>.Failure(
            Error.Validation("Prefix cannot be empty"));
    }
    
    if (prefix.Length > 10)
    {
        return Result<SerialCode>.Failure(
            Error.Validation("Prefix cannot exceed 10 characters", 
                $"Provided prefix length: {prefix.Length}"));
    }
    
    // Create serial code...
    return Result<SerialCode>.Success(serialCode);
}
```

---

### Pattern 3: Invalid Operation

**Before (Exception-based):**
```csharp
public async Task DeleteAsync(Guid id)
{
    var risk = await _context.Risks.FindAsync(id);
    if (risk == null)
    {
        throw new KeyNotFoundException($"Risk {id} not found");
    }
    
    if (risk.Status == RiskStatus.Active)
    {
        throw new InvalidOperationException("Cannot delete active risk");
    }
    
    _context.Risks.Remove(risk);
    await _context.SaveChangesAsync();
}
```

**After (Result-based):**
```csharp
public async Task<Result> DeleteAsync(Guid id)
{
    var risk = await _context.Risks.FindAsync(id);
    if (risk == null)
    {
        return Result.Failure(Error.NotFound("Risk", id));
    }
    
    if (risk.Status == RiskStatus.Active)
    {
        return Result.Failure(
            Error.InvalidOperation("Cannot delete active risk",
                $"Risk status: {risk.Status}"));
    }
    
    _context.Risks.Remove(risk);
    await _context.SaveChangesAsync();
    
    return Result.Success();
}
```

---

### Pattern 4: Controller Usage

**Before (Exception-based):**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    try
    {
        var risk = await _riskService.GetByIdAsync(id);
        return Ok(risk);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting risk");
        return StatusCode(500, new { error = "Internal server error" });
    }
}
```

**After (Result-based):**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var result = await _riskService.GetByIdAsync(id);
    return result.ToActionResult();
}
```

**With Logging:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var result = await _riskService.GetByIdAsync(id);
    
    result.OnFailure(error => 
        _logger.LogWarning("Failed to get risk {RiskId}: {Error}", id, error));
    
    return result.ToActionResult();
}
```

---

### Pattern 5: Chaining Operations

**Using Map:**
```csharp
public async Task<Result<RiskSummaryDto>> GetSummaryAsync(Guid id)
{
    var result = await GetByIdAsync(id);
    
    return result.Map(risk => new RiskSummaryDto
    {
        Id = risk.Id,
        Title = risk.Title,
        Severity = risk.Severity
    });
}
```

**Using BindAsync:**
```csharp
public async Task<Result<RiskDto>> UpdateAndGetAsync(Guid id, UpdateRiskDto dto)
{
    var updateResult = await UpdateAsync(id, dto);
    
    return await updateResult.BindAsync(async _ => 
        await GetByIdAsync(id));
}
```

**Using Ensure:**
```csharp
public async Task<Result<RiskDto>> GetActiveRiskAsync(Guid id)
{
    var result = await GetByIdAsync(id);
    
    return result.Ensure(
        risk => risk.Status == RiskStatus.Active,
        Error.InvalidOperation("Risk is not active"));
}
```

---

### Pattern 6: Multiple Validations

**Sequential Validation:**
```csharp
public async Task<Result<SerialCode>> CreateAsync(CreateSerialCodeDto dto)
{
    // Validate prefix
    if (string.IsNullOrWhiteSpace(dto.Prefix))
    {
        return Result<SerialCode>.Failure(
            Error.Validation("Prefix is required"));
    }
    
    // Validate length
    if (dto.Prefix.Length > 10)
    {
        return Result<SerialCode>.Failure(
            Error.Validation("Prefix too long"));
    }
    
    // Check for duplicates
    var exists = await _context.SerialCodes
        .AnyAsync(s => s.Prefix == dto.Prefix);
    
    if (exists)
    {
        return Result<SerialCode>.Failure(
            Error.Conflict("Serial code prefix already exists", 
                $"Prefix: {dto.Prefix}"));
    }
    
    // Create entity
    var serialCode = new SerialCode { Prefix = dto.Prefix };
    _context.SerialCodes.Add(serialCode);
    await _context.SaveChangesAsync();
    
    return Result<SerialCode>.Success(serialCode);
}
```

---

## 🛠️ Error Types Reference

### Error.NotFound(entityName, id)
Use when an entity is not found in the database.

```csharp
return Result<RiskDto>.Failure(Error.NotFound("Risk", id));
// Returns: [NOT_FOUND] Risk not found - No Risk found with ID: {id}
```

### Error.Validation(message, details)
Use for input validation failures.

```csharp
return Result.Failure(Error.Validation(
    "Invalid email format", 
    $"Provided email: {email}"));
// Returns: [VALIDATION_ERROR] Invalid email format - Provided email: {email}
```

### Error.Unauthorized(message)
Use when authentication is required.

```csharp
return Result.Failure(Error.Unauthorized("Login required"));
// Returns: [UNAUTHORIZED] Login required
```

### Error.Forbidden(message)
Use when user lacks permissions.

```csharp
return Result.Failure(Error.Forbidden("Insufficient permissions"));
// Returns: [FORBIDDEN] Insufficient permissions
```

### Error.Conflict(message, details)
Use for duplicate entries or concurrent modifications.

```csharp
return Result.Failure(Error.Conflict(
    "Email already exists", 
    $"Email: {email}"));
// Returns: [CONFLICT] Email already exists - Email: {email}
```

### Error.InvalidOperation(message, details)
Use when operation is not allowed in current state.

```csharp
return Result.Failure(Error.InvalidOperation(
    "Cannot delete active risk", 
    $"Status: {risk.Status}"));
// Returns: [INVALID_OPERATION] Cannot delete active risk - Status: Active
```

### Error.Internal(message, details)
Use for unexpected internal errors.

```csharp
return Result.Failure(Error.Internal(
    "Database connection failed", 
    ex.Message));
// Returns: [INTERNAL_ERROR] Database connection failed - {ex.Message}
```

---

## 🎨 HTTP Status Code Mapping

The `ToActionResult()` extension automatically maps error codes to HTTP status codes:

| Error Code | HTTP Status | Example |
|------------|-------------|---------|
| `NOT_FOUND` | 404 Not Found | Entity doesn't exist |
| `VALIDATION_ERROR` | 400 Bad Request | Invalid input |
| `UNAUTHORIZED` | 401 Unauthorized | Not authenticated |
| `FORBIDDEN` | 403 Forbidden | Insufficient permissions |
| `CONFLICT` | 409 Conflict | Duplicate entry |
| `BAD_REQUEST` | 400 Bad Request | Malformed request |
| `INVALID_OPERATION` | 500 Internal Server Error | Invalid state |
| `INTERNAL_ERROR` | 500 Internal Server Error | Unexpected error |

---

## ✅ Best Practices

### DO:
✅ Use `Result<T>` for operations that can fail
✅ Use `Result` (without T) for operations that don't return a value
✅ Provide detailed error messages with context
✅ Use predefined Error factory methods
✅ Log errors in controllers using `OnFailure()`
✅ Use `ToActionResult()` for consistent API responses

### DON'T:
❌ Don't throw exceptions for expected failures
❌ Don't use generic error messages
❌ Don't mix exceptions and Result<T> in the same service
❌ Don't forget to check `IsFailure` before accessing `Value`
❌ Don't return null from Result<T> methods

---

## 🔍 Migration Checklist

When refactoring a service method:

- [ ] Change return type from `T` to `Result<T>` (or `Task<T>` to `Task<Result<T>>`)
- [ ] Replace `throw new KeyNotFoundException` with `Result<T>.Failure(Error.NotFound(...))`
- [ ] Replace `throw new ArgumentException` with `Result<T>.Failure(Error.Validation(...))`
- [ ] Replace `throw new InvalidOperationException` with `Result<T>.Failure(Error.InvalidOperation(...))`
- [ ] Replace successful returns with `Result<T>.Success(value)`
- [ ] Update controller to use `result.ToActionResult()`
- [ ] Update unit tests to check `IsSuccess` and `Error` properties
- [ ] Update interface definition if needed

---

## 📝 Example: Complete Refactoring

### Before:
```csharp
// Service
public async Task<RiskDto> UpdateAsync(Guid id, UpdateRiskDto dto)
{
    var risk = await _context.Risks.FindAsync(id);
    if (risk == null)
        throw new KeyNotFoundException($"Risk {id} not found");
    
    if (string.IsNullOrWhiteSpace(dto.Title))
        throw new ArgumentException("Title is required");
    
    risk.Title = dto.Title;
    await _context.SaveChangesAsync();
    
    return _mapper.Map<RiskDto>(risk);
}

// Controller
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, UpdateRiskDto dto)
{
    try
    {
        var result = await _riskService.UpdateAsync(id, dto);
        return Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

### After:
```csharp
// Service
public async Task<Result<RiskDto>> UpdateAsync(Guid id, UpdateRiskDto dto)
{
    var risk = await _context.Risks.FindAsync(id);
    if (risk == null)
        return Result<RiskDto>.Failure(Error.NotFound("Risk", id));
    
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Result<RiskDto>.Failure(Error.Validation("Title is required"));
    
    risk.Title = dto.Title;
    await _context.SaveChangesAsync();
    
    return Result<RiskDto>.Success(_mapper.Map<RiskDto>(risk));
}

// Controller
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, UpdateRiskDto dto)
{
    var result = await _riskService.UpdateAsync(id, dto);
    return result.ToActionResult();
}
```

---

## 🧪 Testing Examples

### Unit Test - Success Case:
```csharp
[Fact]
public async Task GetByIdAsync_WhenRiskExists_ReturnsSuccess()
{
    // Arrange
    var riskId = Guid.NewGuid();
    // ... setup mock
    
    // Act
    var result = await _service.GetByIdAsync(riskId);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Id.Should().Be(riskId);
}
```

### Unit Test - Failure Case:
```csharp
[Fact]
public async Task GetByIdAsync_WhenRiskNotFound_ReturnsNotFoundError()
{
    // Arrange
    var riskId = Guid.NewGuid();
    // ... setup mock to return null
    
    // Act
    var result = await _service.GetByIdAsync(riskId);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().NotBeNull();
    result.Error.Code.Should().Be(ErrorCode.NotFound);
    result.Error.Message.Should().Contain("Risk not found");
}
```

---

**Last Updated**: 2026-01-14
**Next Review**: After completing first service refactoring
