# Single MVC Application Migration Plan

## Current Architecture (9 Projects)
- Grc.Domain.Shared
- Grc.Domain
- Grc.Application.Contracts
- Grc.Application
- Grc.EntityFrameworkCore
- Grc.HttpApi
- Grc.HttpApi.Host
- Grc.Blazor
- Grc.DbMigrator

## New Simplified Architecture (1 Project)
Single MVC application: **Grc.MvcApp**

## Folder Structure for Single App

```
Grc.MvcApp/
├── Controllers/           # MVC Controllers
├── Models/               # Domain models & DTOs
│   ├── Entities/        # Domain entities
│   ├── DTOs/            # Data transfer objects
│   └── ViewModels/      # View models
├── Data/                # Entity Framework
│   ├── GrcDbContext.cs
│   ├── Migrations/
│   └── Repositories/
├── Services/            # Business logic
│   ├── Interfaces/
│   └── Implementations/
├── Views/               # MVC Views
├── wwwroot/            # Static files
├── Areas/              # Feature areas
│   ├── Admin/
│   ├── Assessment/
│   ├── Audit/
│   ├── Risk/
│   └── Workflow/
├── Configuration/      # App configuration
├── Middleware/         # Custom middleware
├── Extensions/         # Extension methods
└── Program.cs         # Application entry point

## Benefits of Single App
1. **Simplified Deployment** - One deployment unit
2. **Reduced Complexity** - No inter-project dependencies
3. **Easier Debugging** - All code in one place
4. **Faster Development** - No need to switch between projects
5. **Simplified Configuration** - One appsettings.json
6. **Better Performance** - No inter-process communication

## Migration Steps

### Phase 1: Create New MVC Project
1. Create ASP.NET Core MVC project with Entity Framework
2. Configure authentication (Identity)
3. Setup Entity Framework with SQL Server

### Phase 2: Migrate Domain Models
1. Copy all entities from Grc.Domain to Models/Entities
2. Copy DTOs from Application.Contracts to Models/DTOs
3. Create ViewModels as needed

### Phase 3: Migrate Data Access
1. Copy GrcDbContext to Data/
2. Migrate existing migrations
3. Create repository pattern implementations

### Phase 4: Migrate Business Logic
1. Convert Application services to Service layer
2. Simplify dependency injection
3. Remove ABP-specific abstractions

### Phase 5: Migrate UI
1. Convert Blazor pages to MVC Views
2. Create Controllers for each feature area
3. Migrate client-side logic to JavaScript/jQuery

### Phase 6: Authentication & Authorization
1. Replace OpenIddict with ASP.NET Core Identity
2. Implement role-based authorization
3. Configure JWT for API endpoints if needed

## Key Changes

### From ABP to Standard ASP.NET Core
- Replace `IApplicationService` with regular service interfaces
- Replace `IRepository<T>` with custom repository pattern
- Remove ABP modules and use standard DI
- Replace ABP permissions with ASP.NET Core policies

### Database Access
- Keep Entity Framework Core
- Simplify to DbContext + Repository pattern
- Remove unit of work abstraction

### Authentication
- Use ASP.NET Core Identity instead of OpenIddict
- Simpler cookie-based auth for MVC
- JWT for API endpoints if needed

## Sample Code Structure

```csharp
// Models/Entities/Risk.cs
public class Risk
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Severity { get; set; }
    public DateTime CreatedDate { get; set; }
}

// Data/Repositories/IRiskRepository.cs
public interface IRiskRepository
{
    Task<Risk> GetByIdAsync(Guid id);
    Task<List<Risk>> GetAllAsync();
    Task<Risk> CreateAsync(Risk risk);
    Task UpdateAsync(Risk risk);
    Task DeleteAsync(Guid id);
}

// Services/IRiskService.cs
public interface IRiskService
{
    Task<RiskDto> GetRiskAsync(Guid id);
    Task<List<RiskDto>> GetAllRisksAsync();
    Task<RiskDto> CreateRiskAsync(CreateRiskDto input);
}

// Controllers/RiskController.cs
public class RiskController : Controller
{
    private readonly IRiskService _riskService;

    public RiskController(IRiskService riskService)
    {
        _riskService = riskService;
    }

    public async Task<IActionResult> Index()
    {
        var risks = await _riskService.GetAllRisksAsync();
        return View(risks);
    }
}
```

## Estimated Timeline
- Phase 1: 1 day - Project setup
- Phase 2: 2 days - Model migration
- Phase 3: 2 days - Data layer
- Phase 4: 3 days - Business logic
- Phase 5: 4 days - UI migration
- Phase 6: 2 days - Authentication

Total: ~2 weeks for complete migration