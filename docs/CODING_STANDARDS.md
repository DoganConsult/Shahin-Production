# GRC Platform Coding Standards

**Version:** 1.0  
**Last Updated:** January 2026  
**Status:** 🟢 **ACTIVE**

---

## 📋 Table of Contents

1. [General Principles](#general-principles)
2. [C# Coding Standards](#c-coding-standards)
3. [Architecture Standards](#architecture-standards)
4. [Module Standards](#module-standards)
5. [Database Standards](#database-standards)
6. [API Standards](#api-standards)
7. [Testing Standards](#testing-standards)
8. [Documentation Standards](#documentation-standards)

---

## 🎯 General Principles

### SOLID Principles

**Single Responsibility Principle**
```csharp
// ✅ GOOD - Single responsibility
public class AssessmentService : IAssessmentService
{
    // Only handles assessment business logic
}

// ❌ BAD - Multiple responsibilities  
public class AssessmentService
{
    // Assessment logic
    // Email sending
    // Database operations
    // Logging
}
```

**Open/Closed Principle**
```csharp
// ✅ GOOD - Open for extension, closed for modification
public abstract class NotificationChannel
{
    public abstract Task SendAsync(Message message);
}

public class EmailChannel : NotificationChannel { }
public class SmsChannel : NotificationChannel { }
```

**Liskov Substitution Principle**
```csharp
// ✅ GOOD - Subtypes are substitutable
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
}

public class AssessmentRepository : IRepository<Assessment> { }
```

**Interface Segregation**
```csharp
// ✅ GOOD - Segregated interfaces
public interface IReadRepository<T> 
{
    Task<T> GetByIdAsync(Guid id);
    IQueryable<T> Query();
}

public interface IWriteRepository<T>
{
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// ❌ BAD - Fat interface
public interface IRepository<T>
{
    // Too many methods in one interface
}
```

**Dependency Inversion**
```csharp
// ✅ GOOD - Depends on abstractions
public class RiskService
{
    private readonly IRepository<Risk> _repository;
    private readonly INotificationService _notifications;
    
    public RiskService(IRepository<Risk> repository, INotificationService notifications)
    {
        _repository = repository;
        _notifications = notifications;
    }
}
```

---

## 💻 C# Coding Standards

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| **Classes** | PascalCase | `AssessmentService` |
| **Interfaces** | I + PascalCase | `IAssessmentService` |
| **Methods** | PascalCase | `GetAssessmentAsync()` |
| **Properties** | PascalCase | `AssessmentId` |
| **Parameters** | camelCase | `assessmentId` |
| **Private Fields** | _camelCase | `_repository` |
| **Constants** | UPPER_CASE | `MAX_RETRY_COUNT` |
| **Enums** | PascalCase | `AssessmentStatus` |

### Code Organization

```csharp
namespace GrcMvc.Modules.Assessment.Services
{
    // Using statements (sorted alphabetically)
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Assessment service implementation
    /// </summary>
    public class AssessmentService : IAssessmentService
    {
        // Constants
        private const int MAX_BATCH_SIZE = 100;
        
        // Fields
        private readonly IRepository<Assessment> _repository;
        private readonly ILogger<AssessmentService> _logger;
        
        // Constructor
        public AssessmentService(
            IRepository<Assessment> repository,
            ILogger<AssessmentService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        // Public methods
        public async Task<Assessment> GetByIdAsync(Guid id)
        {
            // Implementation
        }
        
        // Private methods
        private void ValidateAssessment(Assessment assessment)
        {
            // Implementation
        }
    }
}
```

### Async/Await Patterns

```csharp
// ✅ GOOD - Async all the way
public async Task<AssessmentDto> GetAssessmentAsync(Guid id)
{
    var assessment = await _repository.GetByIdAsync(id);
    return await MapToDto(assessment);
}

// ❌ BAD - Blocking async code
public AssessmentDto GetAssessment(Guid id)
{
    var assessment = _repository.GetByIdAsync(id).Result; // Don't do this!
    return MapToDto(assessment).Result;
}
```

### Exception Handling

```csharp
// ✅ GOOD - Specific exception handling
public async Task<Result<Assessment>> CreateAssessmentAsync(CreateAssessmentDto dto)
{
    try
    {
        // Validation
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
            
        // Business logic
        var assessment = await _service.CreateAsync(dto);
        return Result<Assessment>.Success(assessment);
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning(ex, "Validation failed for assessment creation");
        return Result<Assessment>.Failure(ex.Message);
    }
    catch (EntityExistsException ex)
    {
        _logger.LogWarning(ex, "Assessment already exists");
        return Result<Assessment>.Failure("Assessment already exists");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating assessment");
        return Result<Assessment>.Failure("An error occurred");
    }
}
```

---

## 🏗️ Architecture Standards

### Layer Separation

```
┌─────────────────────────────────┐
│     Presentation Layer          │ ← Controllers, Views, APIs
├─────────────────────────────────┤
│     Application Layer           │ ← Services, Commands, Queries
├─────────────────────────────────┤
│       Domain Layer              │ ← Entities, Value Objects, Events
├─────────────────────────────────┤
│    Infrastructure Layer         │ ← Data Access, External Services
└─────────────────────────────────┘
```

### Dependency Rules
- Dependencies only flow downward
- Domain layer has no dependencies
- Infrastructure implements domain interfaces
- Presentation depends on Application

---

## 📦 Module Standards

### Module Structure

```
GrcMvc.Modules.{ModuleName}/
├── Module.cs                     # Module definition
├── {ModuleName}Module.csproj     # Project file
│
├── Domain/                       # Domain layer
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Exceptions/
│   └── Interfaces/
│
├── Application/                  # Application layer
│   ├── Services/
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   ├── Validators/
│   └── Mappings/
│
├── Infrastructure/              # Infrastructure layer
│   ├── Persistence/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── Migrations/
│   └── Services/
│
├── Presentation/                # Presentation layer
│   ├── Controllers/
│   ├── ViewModels/
│   └── Views/
│
└── Tests/                       # Tests
    ├── Unit/
    ├── Integration/
    └── Fixtures/
```

### Module Registration

```csharp
public class RiskModule : ModuleBase
{
    public override string Id => "GrcMvc.Modules.Risk";
    public override string Name => "Risk Management";
    public override string Version => "1.0.0";
    
    public override void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddScoped<IRiskService, RiskService>();
        services.AddScoped<IRiskRepository, RiskRepository>();
        
        // Register validators
        services.AddScoped<IValidator<Risk>, RiskValidator>();
        
        // Register event handlers
        services.AddScoped<IEventHandler<RiskCreatedEvent>, RiskCreatedEventHandler>();
    }
}
```

---

## 🗄️ Database Standards

### Entity Configuration

```csharp
public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        // Table and schema
        builder.ToTable("Assessments", "assessment");
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasMaxLength(2000);
            
        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => new { e.TenantId, e.Status });
        
        // Relationships
        builder.HasOne(e => e.Control)
            .WithMany(c => c.Assessments)
            .HasForeignKey(e => e.ControlId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### Repository Pattern

```csharp
public interface IAssessmentRepository : IRepository<Assessment>
{
    Task<IEnumerable<Assessment>> GetUpcomingAsync(int days);
    Task<IEnumerable<Assessment>> GetByStatusAsync(AssessmentStatus status);
}

public class AssessmentRepository : RepositoryBase<Assessment>, IAssessmentRepository
{
    public AssessmentRepository(DbContext context) : base(context) { }
    
    public async Task<IEnumerable<Assessment>> GetUpcomingAsync(int days)
    {
        return await Query()
            .Where(a => a.DueDate <= DateTime.UtcNow.AddDays(days))
            .Where(a => a.Status != AssessmentStatus.Completed)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }
}
```

---

## 🌐 API Standards

### RESTful Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    // GET api/assessments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAll()
    
    // GET api/assessments/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssessmentDto>> GetById(Guid id)
    
    // POST api/assessments
    [HttpPost]
    public async Task<ActionResult<AssessmentDto>> Create(CreateAssessmentDto dto)
    
    // PUT api/assessments/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateAssessmentDto dto)
    
    // DELETE api/assessments/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
}
```

### Response Format

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

---

## 🧪 Testing Standards

### Unit Tests

```csharp
[TestFixture]
public class AssessmentServiceTests
{
    private IAssessmentService _service;
    private Mock<IRepository<Assessment>> _repositoryMock;
    
    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IRepository<Assessment>>();
        _service = new AssessmentService(_repositoryMock.Object);
    }
    
    [Test]
    public async Task GetById_WhenAssessmentExists_ReturnsAssessment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var assessment = new Assessment { Id = id };
        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(assessment);
        
        // Act
        var result = await _service.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(id, result.Id);
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class AssessmentApiTests : IntegrationTestBase
{
    [Test]
    public async Task CreateAssessment_ValidData_Returns201()
    {
        // Arrange
        var dto = new CreateAssessmentDto
        {
            Name = "Test Assessment",
            ControlId = Guid.NewGuid()
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/assessments", dto);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

## 📝 Documentation Standards

### XML Documentation

```csharp
/// <summary>
/// Service for managing assessments
/// </summary>
public interface IAssessmentService
{
    /// <summary>
    /// Gets an assessment by its unique identifier
    /// </summary>
    /// <param name="id">The assessment identifier</param>
    /// <returns>The assessment if found; otherwise, null</returns>
    /// <exception cref="ArgumentException">Thrown when id is empty</exception>
    Task<Assessment> GetByIdAsync(Guid id);
}
```

### README Structure

```markdown
# Module Name

## Overview
Brief description of the module

## Features
- Feature 1
- Feature 2

## Installation
```bash
dotnet add package GrcMvc.Modules.ModuleName
```

## Configuration
```json
{
  "Modules": {
    "ModuleName": {
      "Enabled": true
    }
  }
}
```

## Usage
Code examples

## API Reference
Link to API documentation

## License
License information
```

---

## ✅ Code Review Checklist

- [ ] Follows naming conventions
- [ ] Includes XML documentation
- [ ] Has unit tests (>80% coverage)
- [ ] No hardcoded values
- [ ] Proper exception handling
- [ ] Async/await used correctly
- [ ] No memory leaks
- [ ] SQL injection prevention
- [ ] Input validation
- [ ] Logging implemented
- [ ] Performance optimized
- [ ] Security best practices

---

**Last Updated:** January 2026  
**Next Review:** March 2026
