# GRC Agent-Driven Onboarding System – Copilot Instructions

## Overview
This document defines how Copilot should generate, review, and validate code for the **Shahin GRC Platform's Agent-Driven Onboarding System**. The system is built on:
- **.NET 8.0** with **ASP.NET Core**
- **ABP Framework** (multi-tenant, RBAC, module-based)
- **EF Core 8.0.8** with **PostgreSQL**
- **Agent orchestration** via OnboardingAgent, DashboardAgent, EvidenceAgent, WorkflowAgent
- **Multi-tenant isolation** with global tenant filters

---

## 1. Critical Constraints (Non-Negotiable)

### 1.1 ABP-Only Context
- **Every code change must preserve ABP patterns**: modules, permissions, features, dependency injection.
- **Multi-tenant isolation is mandatory**: All DbSets must have `HasQueryFilter()` enforcing `TenantId`.
- **No fallback contexts**: Services must always have `ITenantContextService` injected and validated.
- **Fail-fast on tenant violations**: Throw `SecurityException` or `UnauthorizedAccessException` immediately.

### 1.2 Agent Governance
- **Whitelisted agents only**: OnboardingAgent, DashboardAgent, EvidenceAgent, WorkflowAgent, RulesEngineAgent.
- **Agents emit deterministic outputs**: JSON contracts with schema validation.
- **No dynamic agent registration**: All agents are pre-defined in `GrcMvcAbpModule.cs`.
- **Audit trail required**: Every agent action must log to `AuditReplayEvent`.

### 1.3 Onboarding Flow Sequencing
```
Login (FirstAdminUserId + OnboardingStatus != "Completed") 
  → OnboardingRedirectMiddleware
  → Fast Start (Org Profile, Frameworks)
  → Mission 1 (Team Setup)
  → Mission 2 (Framework/Control Setup)
  → Mission 3 (Integration & Evidence)
  → tenant.OnboardingStatus = "Completed"
  → Dashboard
```
**Violating order**: Throw `InvalidOperationException`.

### 1.4 No Cross-Tenant Operations
- Services must validate `TenantContextService.TenantId` before any query/mutation.
- Controllers must use `[RequireTenant]` attribute.
- Views must only read from `CurrentUser` + `CurrentTenant` (injected).

---

## 2. Code Generation Rules by Layer

### 2.1 Middleware
```csharp
// REQUIRED STRUCTURE
public class YourMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantContextService _tenantService;

    public YourMiddleware(RequestDelegate next, 
        ICurrentTenant currentTenant,
        ITenantContextService tenantService)
    {
        _next = next;
        _currentTenant = currentTenant;
        _tenantService = tenantService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(_currentTenant?.Id))
            throw new SecurityException("Tenant context missing");

        // Your logic
        await _next(context);
    }
}
```

### 2.2 Controllers (MVC & API)
```csharp
[RequireTenant]  // MANDATORY for protected routes
[Route("[controller]")]
public class YourController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly IYourService _service;

    public YourController(
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IYourService service)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> DoSomething(CreateDto dto)
    {
        // DTOs only, no direct entity manipulation
        var result = await _service.HandleAsync(_currentTenant.Id, dto);
        return Ok(result);
    }
}
```

### 2.3 Services
```csharp
public class YourService : IYourService
{
    private readonly ITenantContextService _tenantService;
    private readonly IRepository<YourEntity> _repository;
    private readonly IObjectMapper _mapper;

    public YourService(
        ITenantContextService tenantService,
        IRepository<YourEntity> repository,
        IObjectMapper mapper)
    {
        _tenantService = tenantService;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResultDto> HandleAsync(string tenantId, InputDto input)
    {
        // Validate tenant
        if (tenantId != _tenantService.TenantId)
            throw new SecurityException("Tenant mismatch");

        // Service logic with repository
        var entities = await _repository.GetListAsync(e => e.TenantId == tenantId);
        return _mapper.Map<ResultDto>(entities);
    }
}
```

### 2.4 DbContext Entities
```csharp
// In OnModelBuilding
modelBuilder.Entity<YourEntity>(b =>
{
    b.HasKey(e => new { e.Id, e.TenantId });
    
    // REQUIRED: Tenant filter on every entity
    b.HasQueryFilter(e => e.TenantId == EF.Property<string>(typeof(YourEntity), "TenantId") 
        || EF.Property<Guid>("__EF_FilterValue__TenantId") == default);
    
    b.Property(e => e.TenantId).IsRequired();
});
```

### 2.5 Agents
```csharp
public class YourAgentService : IYourAgentService
{
    private readonly ITenantContextService _tenantService;
    private readonly ILogger<YourAgentService> _logger;

    public async Task<AgentOutputDto> ExecuteAsync(AgentInputDto input)
    {
        // Validate tenant context
        AgentGovernancePolicy.EnforceMultiTenantContext(_tenantService.TenantId);
        AgentGovernancePolicy.RequireAgentWhitelisted(nameof(YourAgentService));

        try
        {
            // Agent logic
            var result = await DoWork(input);

            // Log to AuditReplayEvent
            await LogAuditEventAsync("AgentExecuted", result);

            return new AgentOutputDto 
            { 
                Result = result,
                Rationale = GenerateRationale(result),
                TraceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Agent failed: {ex.Message}");
            throw;
        }
    }

    private string GenerateRationale(object result) =>
        JsonSerializer.Serialize(new { 
            reason = "...", 
            confidence = 0.95, 
            timestamp = DateTime.UtcNow 
        });
}
```

---

## 3. Onboarding-Specific Patterns

### 3.1 Redirect Middleware
```csharp
public class OnboardingRedirectMiddleware
{
    public async Task InvokeAsync(HttpContext context, 
        ICurrentUser currentUser, 
        ITenantContextService tenantService,
        IRepository<Tenant> tenantRepository)
    {
        if (!currentUser.IsAuthenticated) 
        {
            await _next(context);
            return;
        }

        var tenant = await tenantRepository.FirstOrDefaultAsync(t => 
            t.Id == tenantService.TenantId);

        // Redirect if FirstAdminUserId AND onboarding incomplete
        if (tenant?.FirstAdminUserId == currentUser.Id && 
            tenant.OnboardingStatus != "Completed" &&
            !context.Request.Path.StartsWithSegments("/onboarding"))
        {
            context.Response.Redirect($"/onboarding/wizard/fast-start?returnUrl={context.Request.Path}");
            return;
        }

        await _next(context);
    }
}
```

### 3.2 Progress Tracking Service
```csharp
public class OnboardingWizardProgressService : IOnboardingWizardProgressService
{
    public async Task<OnboardingProgressDto> GetProgressAsync(string tenantId, string adminUserId)
    {
        var wizard = await _wizardRepository.FirstOrDefaultAsync(w => 
            w.TenantId == tenantId && w.AdminUserId == adminUserId);

        return new OnboardingProgressDto
        {
            PercentComplete = CalculateProgress(wizard),
            CurrentMission = DetermineNextMission(wizard),
            CompletedMissions = wizard?.CompletedMissions ?? new List<string>(),
            IsBlocked = HasBlockingIssue(wizard)
        };
    }

    private int CalculateProgress(OnboardingWizard wizard) => wizard switch
    {
        null => 0,
        { FastStartComplete = false } => 25,
        { Mission1Complete = false } => 50,
        { Mission2Complete = false } => 75,
        { Mission3Complete = false } => 90,
        _ => 100
    };
}
```

### 3.3 Tenant Creation (Agent-Backed)
```csharp
[ApiController]
[Route("api/[controller]")]
public class OnboardingAgentController : ControllerBase
{
    [HttpPost("create-tenant")]
    public async Task<IActionResult> CreateTenant(CreateTenantAgentRequest req)
    {
        // AgentGovernancePolicy validates agent context
        AgentGovernancePolicy.EnforceMultiTenantContext(req.TenantId ?? Guid.NewGuid().ToString());

        var result = await _onboardingService.CreateTenantWithAdminAsync(
            req.OrgName, req.AdminEmail, req.Industry, req.Frameworks);

        return Ok(new { tenantId = result.TenantId, onboardingUrl = $"/onboarding/wizard?t={result.TenantId}" });
    }
}
```

---

## 4. Permission & Feature Patterns

### 4.1 Permission Requirements
```csharp
// In controller actions
[Authorize("Grc.Onboarding.Manage")]
public async Task<IActionResult> UpdateOrgProfile(UpdateProfileDto dto) => ...;

[Authorize("Grc.Dashboard.View")]
public IActionResult Dashboard() => ...;
```

### 4.2 Feature Checks
```csharp
// In service
if (!await _featureCheckService.IsEnabledAsync("AutoEvidence", _currentTenant.Id))
{
    // Fallback behavior
}
```

---

## 5. Testing Requirements

### 5.1 Unit Tests
```csharp
[Fact]
public void OnboardingRedirectMiddleware_Throws_When_TenantId_Missing()
{
    var middleware = new OnboardingRedirectMiddleware(_next, _currentUser, _tenantService);
    Assert.Throws<SecurityException>(() => middleware.InvokeAsync(httpContext));
}

[Fact]
public async Task ProgressService_Returns_Correct_Percent()
{
    var result = await _service.GetProgressAsync(_tenantId, _adminId);
    Assert.InRange(result.PercentComplete, 0, 100);
}
```

### 5.2 Integration Tests
```csharp
[Fact]
public async Task FirstLoginRedirects_To_Onboarding()
{
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/dashboard");
    
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Contains("/onboarding", response.Headers.Location?.ToString());
}
```

---

## 6. CI/CD Rules

### 6.1 Build Validation
```bash
dotnet build
dotnet test --filter "Category=Governance"
```

### 6.2 Pre-Commit Checks
- ✅ No hardcoded tenant IDs
- ✅ All DbSets have tenant filter
- ✅ All controllers have `[RequireTenant]`
- ✅ All agents logged to audit trail

---

## 7. Common Pitfalls to Avoid

| Pitfall | Correct Way |
|---------|------------|
| `var tenant = _db.Tenants.FirstOrDefault()` | `var tenant = _db.Tenants.FirstOrDefaultAsync(t => t.Id == _tenantService.TenantId)` |
| `new TenantService()` | Inject via constructor (ABP DI) |
| Direct entity mapping in controller | Use IObjectMapper in service |
| No error logging in agent | Must log to AuditReplayEvent |
| Skipping onboarding middleware | Add to `Program.cs` early in middleware pipeline |
| Agent without whitelist check | `AgentGovernancePolicy.RequireAgentWhitelisted(...)` |

---

## 8. Copilot Response Checklist

When generating code for this platform, ensure:
- [ ] ABP patterns followed (modules, DI, permissions)
- [ ] Multi-tenant validation present
- [ ] Audit trail logging included
- [ ] Error messages are security-conscious (no tenant leaks)
- [ ] DTOs used (no direct entity exposure)
- [ ] Unit & integration test stubs included
- [ ] Comments explain "why", not "what"
- [ ] No dynamic agent registration
- [ ] Onboarding flow order enforced
- [ ] CI/CD compatible (no external dependencies)

---

## References
- **ABP Framework**: https://docs.abp.io
- **Permission Policy Provider**: `Authorization/PermissionPolicyProvider.cs`
- **Multi-Tenant Services**: `Services/Security/TenantContextService.cs`
- **Onboarding Controller**: `Controllers/OnboardingWizardController.cs`
- **Agent Base Service**: `Services/Base/IAgentService.cs`
- **Audit Trail**: `Entities/AuditReplayEvent.cs`
