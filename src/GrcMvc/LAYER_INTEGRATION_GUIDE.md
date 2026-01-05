# Layer Integration Guide - GRC MVC Application

## Complete Architecture with All Layers Integrated

This document shows how all application layers are properly integrated and work together.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                       │
│                  Controllers / Razor Views                   │
│                          Areas                               │
└─────────────────┬───────────────────────────┬───────────────┘
                  │                           │
                  ▼                           ▼
┌─────────────────────────────┐ ┌─────────────────────────────┐
│      VALIDATION LAYER       │ │      MAPPING LAYER          │
│      FluentValidation       │ │       AutoMapper            │
└─────────────────┬───────────┘ └─────────────┬───────────────┘
                  │                           │
                  ▼                           ▼
┌─────────────────────────────────────────────────────────────┐
│                     SERVICE LAYER                            │
│               Business Logic & Orchestration                 │
│                  Service Implementations                     │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   UNIT OF WORK PATTERN                       │
│               Transaction Management                          │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    REPOSITORY LAYER                          │
│               Generic Repository Pattern                      │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                     DATA LAYER                               │
│            Entity Framework Core / SQL Server                │
└─────────────────────────────────────────────────────────────┘
```

## Layer Integration Details

### 1. Controller → Service Layer

**Example: RiskController**
```csharp
// Controller receives DTO from client
[HttpPost]
public async Task<IActionResult> Create(CreateRiskDto dto)
{
    // FluentValidation automatically validates
    if (!ModelState.IsValid)
        return View(dto);

    // Controller calls service
    var risk = await _riskService.CreateAsync(dto);

    return RedirectToAction(nameof(Details), new { id = risk.Id });
}
```

### 2. Service → Repository/UnitOfWork

**Example: RiskService**
```csharp
public class RiskService : IRiskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
    {
        // Map DTO to Entity
        var risk = _mapper.Map<Risk>(dto);

        // Use Unit of Work for data access
        await _unitOfWork.Risks.AddAsync(risk);
        await _unitOfWork.SaveChangesAsync();

        // Map Entity back to DTO
        return _mapper.Map<RiskDto>(risk);
    }
}
```

### 3. Repository → Entity Framework

**Example: GenericRepository**
```csharp
public class GenericRepository<T> : IGenericRepository<T>
{
    private readonly GrcDbContext _context;

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        // SaveChanges handled by UnitOfWork
        return entity;
    }
}
```

## Dependency Injection Configuration

**Program.cs - Complete Service Registration**
```csharp
// Database
builder.Services.AddDbContext<GrcDbContext>(options =>
    options.UseSqlServer(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// FluentValidation
builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<Program>();
});

// Unit of Work & Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Services
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Validators
builder.Services.AddScoped<IValidator<CreateRiskDto>, CreateRiskDtoValidator>();
```

## Complete Data Flow Example

### Creating a New Risk - End to End

#### 1. User submits form → Controller
```csharp
POST /Risk/Risk/Create
Body: {
    "Name": "Data Breach Risk",
    "Description": "Risk of unauthorized data access",
    "Likelihood": 3,
    "Impact": 5,
    "Owner": "John Doe"
}
```

#### 2. Controller receives and validates
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateRiskDto dto)
{
    // FluentValidation runs automatically
    // Checks: Name required, Likelihood 1-5, etc.
}
```

#### 3. Service orchestrates business logic
```csharp
public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
{
    // AutoMapper: DTO → Entity
    var risk = _mapper.Map<Risk>(dto);

    // Add audit fields
    risk.CreatedBy = GetCurrentUser();
    risk.CreatedDate = DateTime.UtcNow;

    // Calculate risk score
    risk.RiskScore = risk.Likelihood * risk.Impact;
}
```

#### 4. Unit of Work manages transaction
```csharp
// Begin transaction if complex operation
await _unitOfWork.BeginTransactionAsync();

try
{
    var risk = await _unitOfWork.Risks.AddAsync(entity);

    // If related operations needed
    await _unitOfWork.Controls.AddAsync(control);

    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

#### 5. Repository executes database operation
```csharp
public async Task<T> AddAsync(T entity)
{
    await _dbSet.AddAsync(entity);
    // Entity tracked by EF Core
}
```

#### 6. Entity Framework generates SQL
```sql
INSERT INTO Risks (Id, Name, Description, Likelihood, Impact, ...)
VALUES (@p0, @p1, @p2, @p3, @p4, ...)
```

#### 7. Response flows back up
```
Database → EF Core → Repository → UnitOfWork → Service → Controller → View
```

## Cross-Cutting Concerns

### Logging (Integrated at all layers)
```csharp
public class RiskService
{
    private readonly ILogger<RiskService> _logger;

    public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
    {
        _logger.LogInformation("Creating risk: {Name}", dto.Name);
        // ... business logic
        _logger.LogInformation("Risk created with ID: {Id}", risk.Id);
    }
}
```

### Authentication/Authorization
```csharp
[Authorize(Policy = "RiskManager")]
public async Task<IActionResult> Create(CreateRiskDto dto)
{
    var currentUser = User.Identity.Name; // Available via HttpContext
}
```

### Validation Pipeline
```
1. Client-side validation (JavaScript)
2. Model binding validation
3. FluentValidation (automatic)
4. Business rule validation (in service)
5. Database constraints
```

### Error Handling Flow
```csharp
Controller (try-catch)
  → Service (logs & rethrows)
    → Repository (handles DB exceptions)
      → Global Exception Handler
        → User-friendly error page
```

## Testing the Integration

### 1. Unit Tests (Mock dependencies)
```csharp
[Test]
public async Task CreateRisk_ValidDto_ReturnsRiskDto()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockMapper = new Mock<IMapper>();
    var service = new RiskService(mockUnitOfWork.Object, mockMapper.Object);

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.NotNull(result);
}
```

### 2. Integration Tests (Real database)
```csharp
[Test]
public async Task CreateRisk_EndToEnd_SavesToDatabase()
{
    // Use test database
    using var context = new GrcDbContext(testOptions);
    var unitOfWork = new UnitOfWork(context);
    var service = new RiskService(unitOfWork, mapper);

    // Create risk
    var result = await service.CreateAsync(dto);

    // Verify in database
    var saved = await context.Risks.FindAsync(result.Id);
    Assert.NotNull(saved);
}
```

## Common Integration Patterns

### 1. Query with Includes
```csharp
public async Task<RiskDto> GetRiskWithControlsAsync(Guid id)
{
    var risk = await _unitOfWork.Risks
        .Query()
        .Include(r => r.Controls)
        .FirstOrDefaultAsync(r => r.Id == id);

    return _mapper.Map<RiskDto>(risk);
}
```

### 2. Bulk Operations
```csharp
public async Task UpdateMultipleRisksAsync(List<UpdateRiskDto> risks)
{
    await _unitOfWork.BeginTransactionAsync();

    foreach(var dto in risks)
    {
        var entity = await _unitOfWork.Risks.GetByIdAsync(dto.Id);
        _mapper.Map(dto, entity);
    }

    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
```

### 3. Complex Business Logic
```csharp
public async Task<AssessmentDto> CompleteAssessmentAsync(Guid id)
{
    var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);

    // Calculate compliance score
    var controls = await _unitOfWork.Controls
        .FindAsync(c => c.AssessmentId == id);

    assessment.ComplianceScore = CalculateScore(controls);
    assessment.Status = "Completed";

    // Create notifications
    await NotifyStakeholders(assessment);

    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<AssessmentDto>(assessment);
}
```

## Performance Optimizations

### 1. Async/Await Throughout
✅ All database operations are async
✅ Controllers return Task<IActionResult>
✅ Services return Task<T>

### 2. Proper Disposal
✅ DbContext scoped per request
✅ UnitOfWork implements IDisposable
✅ Using statements for transactions

### 3. Query Optimization
✅ Use projections for read-only operations
✅ Include related data to avoid N+1
✅ Use compiled queries for hot paths

## Troubleshooting Common Issues

### Issue: "No service registered"
**Solution**: Check Program.cs registration
```csharp
builder.Services.AddScoped<IYourService, YourService>();
```

### Issue: "Circular dependency"
**Solution**: Use factory pattern or lazy injection
```csharp
builder.Services.AddScoped<Func<IService>>(x => () => x.GetService<IService>());
```

### Issue: "Transaction deadlock"
**Solution**: Keep transactions short, use proper isolation levels
```csharp
await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
```

## Summary

The application uses a **clean layered architecture** with:
- ✅ **Clear separation of concerns**
- ✅ **Dependency injection throughout**
- ✅ **Consistent data flow patterns**
- ✅ **Comprehensive validation**
- ✅ **Transaction management**
- ✅ **Proper async/await usage**
- ✅ **Testable architecture**

Each layer has a **single responsibility** and communicates through **well-defined interfaces**, making the application maintainable, testable, and scalable.