# Quick Reference - Essential Patterns

> For AI agents: Copy-paste templates for common tasks

---

## 1. Add New Domain Service (Copy-Paste Template)

### Step 1: Create Interface
```csharp
// Services/Interfaces/IMyFeatureService.cs
namespace GrcMvc.Services.Interfaces
{
    public interface IMyFeatureService
    {
        Task<MyFeatureDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<MyFeatureDto>> GetAllAsync();
        Task<MyFeatureDto> CreateAsync(CreateMyFeatureDto dto);
        Task<MyFeatureDto> UpdateAsync(Guid id, UpdateMyFeatureDto dto);
        Task DeleteAsync(Guid id);
    }
}
```

### Step 2: Create Implementation
```csharp
// Services/Implementations/MyFeatureService.cs
namespace GrcMvc.Services.Implementations
{
    public class MyFeatureService : IMyFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MyFeatureService> _logger;

        public MyFeatureService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MyFeatureService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MyFeatureDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.MyFeatures.GetByIdAsync(id);
                return _mapper.Map<MyFeatureDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MyFeature with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MyFeatureDto>> GetAllAsync()
        {
            try
            {
                var entities = await _unitOfWork.MyFeatures.GetAllAsync();
                return _mapper.Map<IEnumerable<MyFeatureDto>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all MyFeatures");
                throw;
            }
        }

        public async Task<MyFeatureDto> CreateAsync(CreateMyFeatureDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var entity = _mapper.Map<MyFeature>(dto);
                entity.Id = Guid.NewGuid();
                
                var created = await _unitOfWork.MyFeatures.AddAsync(entity);
                return _mapper.Map<MyFeatureDto>(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MyFeature");
                throw;
            }
        }

        // Implement UpdateAsync and DeleteAsync similarly
    }
}
```

### Step 3: Add DTOs
```csharp
// Models/DTOs/MyFeatureDto.cs
namespace GrcMvc.Models.DTOs
{
    public class MyFeatureDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Add other properties
        public DateTime CreatedDate { get; set; }
    }

    public class CreateMyFeatureDto
    {
        public string Name { get; set; } = string.Empty;
        // Add properties needed for creation
    }

    public class UpdateMyFeatureDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Add properties that can be updated
    }
}
```

### Step 4: Add Entity
```csharp
// Models/Entities/MyFeature.cs
namespace GrcMvc.Models.Entities
{
    public class MyFeature : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        // Add domain properties
        
        // Optional: Navigation properties
        public ICollection<RelatedEntity> RelatedEntities { get; set; } = [];
    }
}
```

### Step 5: Add to DbContext
```csharp
// In Data/GrcDbContext.cs
public DbSet<MyFeature> MyFeatures { get; set; } = null!;
```

### Step 6: Add to UnitOfWork
```csharp
// In Data/IUnitOfWork.cs
IGenericRepository<MyFeature> MyFeatures { get; }

// In Data/UnitOfWork.cs (in constructor and Dispose)
public IGenericRepository<MyFeature> MyFeatures { get; }

// In constructor:
MyFeatures = new GenericRepository<MyFeature>(context);
```

### Step 7: Add Mapping
```csharp
// In Mappings/AutoMapperProfile.cs
CreateMap<MyFeature, MyFeatureDto>();
CreateMap<CreateMyFeatureDto, MyFeature>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
    .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());
CreateMap<UpdateMyFeatureDto, MyFeature>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
```

### Step 8: Add Validators
```csharp
// Validators/MyFeatureValidators.cs
using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    public class CreateMyFeatureDtoValidator : AbstractValidator<CreateMyFeatureDto>
    {
        public CreateMyFeatureDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
        }
    }
}
```

### Step 9: Add Service Registration
```csharp
// In Program.cs (around line 400-430)
builder.Services.AddScoped<IMyFeatureService, MyFeatureService>();
```

### Step 10: Create Controllers
```csharp
// Controllers/MyFeatureController.cs (MVC)
[Authorize]
public class MyFeatureController : Controller
{
    private readonly IMyFeatureService _service;
    
    public MyFeatureController(IMyFeatureService service) => _service = service;
    
    public async Task<IActionResult> Index() 
    {
        var items = await _service.GetAllAsync();
        return View(items);
    }
    // Add Create, Edit, Delete, Details actions
}

// Controllers/MyFeatureApiController.cs (REST API)
[Route("api/myfeatures")]
[ApiController]
[Authorize]
public class MyFeatureApiController : ControllerBase
{
    private readonly IMyFeatureService _service;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var items = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<MyFeatureDto>>.SuccessResponse(items));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
    // Add GetById, Create, Update, Delete endpoints
}
```

### Step 11: Add Migration
```bash
cd src/GrcMvc
dotnet ef migrations add AddMyFeature -s GrcMvc.csproj
dotnet ef database update
```

---

## 2. Use LLM Service (AI Insights)

```csharp
// Inject the service
private readonly ILlmService _llmService;

// Generate insights
var insight = await _llmService.GenerateRiskAnalysisAsync(riskId, "High financial risk due to...");

// Check if LLM enabled for tenant
if (await _llmService.IsLlmEnabledAsync(tenantId))
{
    var recommendation = await _llmService.GenerateComplianceRecommendationAsync(
        assessmentId, 
        "3 control gaps found in audit");
}
```

---

## 3. Hangfire Background Job

```csharp
// Services/BackgroundJobs/MyCustomJob.cs
namespace GrcMvc.BackgroundJobs
{
    public class MyCustomJob
    {
        private readonly IMyFeatureService _service;
        private readonly ILogger<MyCustomJob> _logger;

        public MyCustomJob(IMyFeatureService service, ILogger<MyCustomJob> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Executing MyCustomJob");
            try
            {
                // Do async work
                var items = await _service.GetAllAsync();
                // Process items
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MyCustomJob");
                throw; // Hangfire will retry
            }
        }
    }
}

// Register in Program.cs
builder.Services.AddScoped<MyCustomJob>();

// Schedule in startup
app.UseHangfireServer();
RecurringJob.AddOrUpdate<MyCustomJob>(
    "my-custom-job",
    x => x.ExecuteAsync(),
    Cron.Daily);
```

---

## 4. Use RBAC Permission Check

```csharp
// Inject authorization service
private readonly IAuthorizationService _authService;

// Check permission before action
var hasPermission = await _authService.HasPermissionAsync(
    userId, 
    tenantId, 
    "PermissionKey");

if (!hasPermission)
{
    return Forbid("You don't have permission to perform this action");
}

// Or use attribute on controller
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> AdminAction() { }
```

---

## 5. Multi-Tenant Query

```csharp
// Get current tenant context
var tenantId = await _tenantContextService.GetTenantIdAsync();

// Filter by tenant
var userTenantItems = (await _service.GetAllAsync())
    .Where(x => x.TenantId == tenantId)
    .ToList();

// Service should handle tenant filtering internally:
// All repositories have access to GrcDbContext which includes TenantId
```

---

## 6. Validation Error Response

```csharp
// Automatically handled by FluentValidation middleware
// But for manual validation:
var validator = new CreateMyFeatureDtoValidator();
var result = await validator.ValidateAsync(dto);

if (!result.IsValid)
{
    var errors = result.Errors.ToDictionary(
        e => e.PropertyName,
        e => e.ErrorMessage);
    return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed"));
}
```

---

## 7. Send Email

```csharp
// Development (returns success, no email sent)
// Production (sends via SMTP)
var emailSender = app.Services.GetRequiredService<IEmailService>();

await emailSender.SendEmailAsync(
    to: "user@example.com",
    subject: "Action Required",
    body: "Please review the attached document");
```

---

## 8. Get Current User

```csharp
private readonly ICurrentUserService _userService;

var userId = await _userService.GetCurrentUserIdAsync();
var user = await _userService.GetCurrentUserAsync();
var userName = await _userService.GetCurrentUserNameAsync();
```

---

## 9. Resilience Pattern (Retry Logic)

```csharp
// Already configured in ResilienceService
// Use it for external service calls:
var policy = Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: /* exponential backoff */);

var response = await policy.ExecuteAsync(() => httpClient.GetAsync(url));
```

---

## 10. API Response Pattern

```csharp
// Success
return Ok(ApiResponse<RiskDto>.SuccessResponse(
    data: riskDto,
    message: "Risk created successfully"));

// Error with status code
return BadRequest(ApiResponse<object>.ErrorResponse(
    message: "Invalid risk category",
    statusCode: 400));

// Unauthorized
return Unauthorized(ApiResponse<object>.ErrorResponse(
    message: "User is not authorized"));

// Conflict
return Conflict(ApiResponse<object>.ErrorResponse(
    message: "Risk already exists",
    statusCode: 409));
```

---

## 11. Seed Data (Add to Database)

```csharp
// Data/Seeds/MyFeatureSeeds.cs
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds
{
    public class MyFeatureSeeds
    {
        public static async Task SeedAsync(GrcDbContext context)
        {
            if (await context.MyFeatures.AnyAsync())
                return; // Already seeded

            var features = new List<MyFeature>
            {
                new MyFeature { Id = Guid.NewGuid(), Name = "Feature 1" },
                new MyFeature { Id = Guid.NewGuid(), Name = "Feature 2" }
            };

            await context.MyFeatures.AddRangeAsync(features);
            await context.SaveChangesAsync();
        }
    }
}

// Call in ApplicationInitializer.cs or Program.cs
await MyFeatureSeeds.SeedAsync(context);
```

