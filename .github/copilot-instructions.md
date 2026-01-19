# GitHub Copilot Instructions for Shahin AI GRC Platform

> **AI Agents**: Follow these patterns for code generation, refactoring, and debugging  
> **Updated**: January 19, 2026  
> **Stack**: ASP.NET Core 8.0 MVC + PostgreSQL + Multi-Tenant ABP + 12 Claude AI Agents

## 🎯 Big Picture: Why This Architecture

**Shahin GRC** is a **multi-tenant, AI-powered compliance automation platform** serving enterprise organizations (KSA-focused: NCA, SAMA, PDPL, CITC). The system employs 12 specialized Claude Sonnet agents for compliance, risk, audit, workflow, analytics, and evidence collection. 

**Architecture Decision**: Single MVC monolith (not microservices) to reduce complexity while supporting 230+ DbSets, 100+ controllers, 373 Razor views, and 12 agent types. Multi-tenancy is enforced at query filters (not separate databases) for operational simplicity and cost efficiency.

**Data Flow**: 
1. User → Tenant context via `ITenantContextService` (never defaults)
2. Controllers invoke Services (business logic, tenant validation)
3. Services call EF Core DbSets with automatic tenant filters
4. Complex decisions → Claude agents (policy, risk, workflow recommendations)
5. Events trigger background jobs (Hangfire) and Kafka streams (MassTransit)
6. Dashboards consume event projections (real-time, not raw queries)

### Essential Commands
```bash
docker-compose up -d                    # Start PostgreSQL, Redis, Kafka, Camunda
dotnet run --project src/GrcMvc         # http://localhost:5010
dotnet ef migrations add MigrationName  # Add to src/GrcMvc/Migrations/Grc
dotnet ef database update               # Apply migrations
dotnet test tests/GrcMvc.Tests          # Run full test suite
```

## 🏗️ Multi-Tenancy: The Core Constraint

**Golden Rule**: Every query **must** be tenant-scoped. No cross-tenant leaks allowed.

### How It Works
- Every entity inherits `BaseEntity` (`TenantId` as Guid? + `WorkspaceId` for team collaboration)
- `GrcDbContext.OnModelCreating()` adds global query filters: `.HasQueryFilter(e => e.TenantId == tenantId)`
- Tenant context injected via `ITenantContextService.GetCurrentTenantId()` (throws if missing)
- **Never skip the filter**: If you manually write `.FromSqlInterpolated()`, you **must** add `WHERE TenantId = {currentTenantId}`

### Adding a New Entity (Tenant-Safe)
```csharp
// 1. Entity in Models/Entities/MyEntity.cs
public class MyControl : BaseEntity  // Inherits TenantId, WorkspaceId, audit fields
{
    public string Name { get; set; }
    // TenantId, WorkspaceId, IsDeleted, CreatedAt, etc. inherited from BaseEntity
}

// 2. Add DbSet in GrcDbContext.cs
public DbSet<MyControl> MyControls { get; set; }

// 3. Query (automatic tenant filtering via global filter)
var myControls = _dbContext.MyControls.ToList();  // Already filtered by TenantId
```

## 🤖 12 AI Agents: When and How

**All agents** use `ClaudeAgentService` (unified handler) or specialized services. Tenant ID is **always** passed.

| Agent | Service | Purpose |
|-------|---------|---------|
| **Compliance** | `ClaudeAgentService` | Framework selection, control recommendations |
| **Risk Analysis** | `ILlmService.GenerateRiskInsights()` | Risk scoring, mitigation strategies |
| **Audit** | `AuditRecommendationAgent` | Audit planning, finding prioritization |
| **Policy** | `PolicyAgent` | Policy generation, gap analysis |
| **Workflow** | `WorkflowAgent` | Task recommendations, escalation logic |
| **Evidence** | `EvidenceCollectionAgent` | Evidence req identification, automation suggestions |
| **Diagnostic** | `DiagnosticAgentService` | System health, troubleshooting |
| **Support** | `SupportAgentService` | User help, FAQ generation |

### Calling an Agent (Correct Pattern)
```csharp
// ✅ CORRECT: Tenant ID passed, response validated
var tenantId = _tenantContextService.GetCurrentTenantId();  // Throws if missing
var risks = await _llmService.GenerateRiskInsights(
    new RiskAnalysisRequest 
    { 
        TenantId = tenantId,
        ControlIds = new[] { /* ... */ },
        Tone = "Professional"
    }
);

// ❌ WRONG: No tenant validation, hardcoded config
var risksBad = await _llmService.GenerateRiskInsights(defaultRequest);
```

**Key Files**:
- `Services/Implementations/ClaudeAgentService.cs` (498 lines)
- `Services/Implementations/LlmService.cs` (config per tenant)
- `Services/Implementations/DiagnosticAgentService.cs`
- `Services/Implementations/SupportAgentService.cs`

## ⚙️ Workflow Engine: State Machines That Work

**Why**: 10 compliance workflows (Control Implementation, Evidence Collection, Remediation, Approval, etc.) require explicit state management.

**Pattern**: 
- Entity: `WorkflowInstance` + `WorkflowTask` (state, owner, due date, sla status)
- Service: `WorkflowService.CreateWorkflow()`, `.AdvanceTask()`, `.EscalateOverdue()`
- Auto-escalation: `EscalationJob` runs hourly via Hangfire

### Example: Starting an Evidence Collection Workflow
```csharp
var workflowId = await _workflowService.CreateWorkflow(
    WorkflowType.EvidenceCollection,
    tenantId,
    new { ControlIds = controlIds, Deadline = DateTime.UtcNow.AddDays(14) }
);
// Auto-creates tasks, assigns to control owners, triggers Hangfire reminders
```

## 🎨 Code Conventions: Consistency Wins

| What | Pattern | Example | Location |
|------|---------|---------|----------|
| **Entities** | Singular, PascalCase, inherit `BaseEntity` | `Risk`, `Control`, `AuditFinding` | `Models/Entities/` |
| **Controllers** | `{Entity}Controller` (MVC), `{Entity}ApiController` (API) | `RiskController.cs`, `RiskApiController.cs` | `Controllers/` |
| **Services** | `I{Entity}Service` interface + `{Entity}Service` implementation | `IRiskService`, `RiskService` | `Services/{Interface,Implementations}/` |
| **DTOs** | `{Entity}ReadDto`, `{Entity}CreateDto`, `{Entity}UpdateDto` | `RiskReadDto`, `RiskCreateDto` | `Models/DTOs/` |
| **Validators** | `{Entity}Validators.cs` with `{Entity}CreateValidator`, `{Entity}UpdateValidator` | `RiskCreateValidator : AbstractValidator<RiskCreateDto>` | `Validators/` |
| **Migrations** | `Add{EntityName}` or `{ChangeDescription}` | `AddRiskTable`, `AddTenantIdToEvidence` | `Migrations/Grc/` |

### Naming Golden Rules
- ✅ `var risks = await _riskService.GetAllAsync(tenantId);`
- ❌ `var r = await GetData();` (cryptic, no tenant context)
- ✅ `public async Task<IEnumerable<RiskReadDto>> GetAllAsync(Guid tenantId) { ... }`
- ❌ `public List<Risk> Get() { ... }` (not async, no DTO, no tenant context)

## 🔌 Integration Points & External Services

| Integration | Purpose | Config | File |
|---|---|---|---|
| **Claude Sonnet 4.5** | AI agents (compliance, risk, audit, policy, workflow) | `appsettings.json` → `ClaudeAgents:ApiKey` | `Services/Implementations/LlmService.cs` |
| **Microsoft Graph** | OAuth email, teams integration | `appsettings.json` → `Graph:*` | `Controllers/GraphSubscriptionsController.cs` |
| **SMTP** | Email delivery (dual: real vs stub) | `Email:Smtp:*` or `Email:UseStub: true` | `Services/Implementations/GrcEmailService.cs` |
| **Camunda BPM** | Complex workflow orchestration (optional) | Docker: `camunda:7.20` | `docker-compose.yml` |
| **Kafka + RabbitMQ** | Event streaming, MassTransit | `MassTransit:*`, `MessageBroker:*` | `Program.cs` DI configuration |
| **PostgreSQL** | Primary data store | `ConnectionStrings:DefaultConnection` | Migrations run on startup |
| **Redis** | Optional caching | `Redis:ConnectionString` | `Program.cs` service registration |

## 🚀 Critical Workflow: Adding a New Feature

### Step 1: Entity + Migration
```csharp
// Models/Entities/MyEntity.cs
public class MyEntity : BaseEntity
{
    public string Title { get; set; }
    public Guid? ParentId { get; set; }
}

// Add DbSet in GrcDbContext.cs
public DbSet<MyEntity> MyEntities { get; set; }

// Create migration
dotnet ef migrations add AddMyEntity
```

### Step 2: DTOs + Service
```csharp
// Models/DTOs/MyEntity*.cs
public class MyEntityReadDto { public Guid Id { get; set; } /* ... */ }
public class MyEntityCreateDto { public string Title { get; set; } /* ... */ }

// Services/IMyEntityService.cs + Implementations/MyEntityService.cs
public interface IMyEntityService 
{
    Task<IEnumerable<MyEntityReadDto>> GetAllAsync(Guid tenantId);
    Task<MyEntityReadDto> CreateAsync(Guid tenantId, MyEntityCreateDto dto);
}
```

### Step 3: Controller + Validation
```csharp
// Controllers/MyEntityController.cs (MVC)
public class MyEntityController : BaseController
{
    public async Task<IActionResult> Index()
    {
        var entities = await _myEntityService.GetAllAsync(_tenantId);
        return View(entities);
    }
}

// Controllers/Api/MyEntityApiController.cs (API)
[ApiController]
[Route("api/[controller]")]
public class MyEntityApiController : ControllerBase { /* ... */ }

// Validators/MyEntityValidators.cs
public class MyEntityCreateValidator : AbstractValidator<MyEntityCreateDto>
{
    public MyEntityCreateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
    }
}
```

### Step 4: Register in DI (Program.cs)
```csharp
builder.Services.AddScoped<IMyEntityService, MyEntityService>();
builder.Services.AddScoped<MyEntityCreateValidator>();
```

## 🛡️ Security & Multi-Tenancy Guardrails

**Always validate tenant context**:
```csharp
var tenantId = _tenantContextService.GetCurrentTenantId();  // Throws if missing
if (tenantId == Guid.Empty) throw new SecurityException("Invalid tenant");

var entity = await _dbContext.Risks.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
if (entity == null) throw new NotFoundException("Not found in your tenant");
```

**Background job tenant safety**:
```csharp
// ✅ CORRECT: Store tenantId in job data
await _backgroundJobClient.EnqueueAsync<MyJob>(x => x.Execute(tenantId));

// ❌ WRONG: Assuming ambient context persists in background job
await _backgroundJobClient.EnqueueAsync<MyJob>(x => x.Execute());  // tenantId lost!
```

## 📚 Key Files & Ratios

| File | Lines | Purpose |
|------|-------|---------|
| `Program.cs` | 1,749 | DI, middleware, Hangfire, MassTransit, auth, localization |
| `GrcDbContext.cs` | 1,697 | 230+ DbSets, query filters, multi-tenant scoping |
| `ClaudeAgentService.cs` | 498 | Unified handler for 12 AI agents |
| `DashboardService.cs` | 31KB | Event-driven projections for dashboards |
| `AdminCatalogService.cs` | 36KB | Control/framework catalog management |
| `OnboardingWizard.cs` | 25KB | Multi-tenant onboarding flow data |

**Test Coverage**: 34 test files for 833 source files (~4% ratio). Priority: Tenant isolation, workflow state, agent output validation.

---

**Roadmap**: 24-week ecosystem expansion available in `ECOSYSTEM_ROADMAP_INDEX.md`  
**Last Updated**: January 19, 2026
- API endpoints: 30 req/min
- Auth endpoints: 5 req/5min (brute force protection)

**Authentication**:
- Primary: ASP.NET Core Identity + cookie auth (MVC views)
- Secondary: JWT Bearer tokens (API endpoints)
- Password: 12+ chars, upper/lower/digit/special
- Lockout: 3 failed attempts → 15 min lockout

### 4. Localization (Arabic RTL)
Default culture: Arabic (ar), Secondary: English (en). Culture preference stored in cookie.

### 5. Email Services (Dual Mode)
- `SmtpEmailService` — Production SMTP
- `StubEmailService` — Development (no actual sending)
- Toggle in `Program.cs` based on environment

### 6. Resilience Pattern
`ResilienceService` wraps Polly retry policies for database transients and external API failures.

### 7. Request Logging
`RequestLoggingMiddleware` logs HTTP requests/responses with performance timing via Serilog.

## All 39 Services (Complete Reference)

| Service | Purpose | Interface |
|---------|---------|-----------|
| RiskService | Risk CRUD, statistics | IRiskService |
| ControlService | Control management | IControlService |
| AssessmentService | Assessment tracking | IAssessmentService |
| AuditService | Audit planning, findings | IAuditService |
| PolicyService | Policy enforcement | IPolicyService |
| EvidenceService | Evidence collection | IEvidenceService |
| WorkflowService | Workflow orchestration | IWorkflowService |
| WorkflowEngineService | BPMN execution | IWorkflowEngineService |
| EscalationService | Auto-escalation rules | IEscalationService |
| WorkflowAuditService | Workflow audit trail | IWorkflowAuditService |
| Phase1FrameworkService | Framework reference data | IFrameworkService |
| HRISService | HR integration | IHRISService |
| AuditTrailService | Audit logging | IAuditTrailService |
| Phase1RulesEngineService | Business rule engine | IRulesEngineService |
| TenantService | Tenant management | ITenantService |
| OnboardingService | Tenant onboarding | IOnboardingService |
| AuditEventService | Audit events | IAuditEventService |
| PlanService | Planning service | IPlanService |
| PermissionService | RBAC permissions | IPermissionService |
| FeatureService | Feature flags | IFeatureService |
| TenantRoleConfigurationService | Role config | ITenantRoleConfigurationService |
| UserRoleAssignmentService | User-role mapping | IUserRoleAssignmentService |
| AccessControlService | Access checking | IAccessControlService |
| RbacSeederService | RBAC seed data | IRbacSeederService |
| UserProfileService | 14 user profile types | IUserProfileService |
| TenantContextService | Multi-tenant resolver | ITenantContextService |
| LlmService | AI-powered insights | ILlmService |
| ResilienceService | Fault tolerance | IResilienceService |
| NotificationService | Alert notifications | INotificationService |
| FileUploadService | Secure file handling | IFileUploadService |
| LocalFileStorageService | Local file storage | IFileStorageService |
| SmtpEmailService | SMTP email sender | ISmtpEmailService |
| StubEmailService | Dev email stub | IEmailService |
| AuthenticationService | JWT token ops | IAuthenticationService |
| AuthorizationService | Permission checks | IAuthorizationService |
| CurrentUserService | Context user data | ICurrentUserService |
| MenuService | Dynamic menu | IMenuService |
| DashboardService | Dashboard data | IDashboardService |
| EvidenceLifecycleService | Evidence workflow | IEvidenceLifecycleService |

## Build & Run Commands

```bash
# Development (Docker Compose - RECOMMENDED)
docker-compose up -d

# Manual run
cd src/GrcMvc && dotnet run

# Database migrations
cd src/GrcMvc
dotnet ef migrations add MigrationName
dotnet ef database update

# Run tests
dotnet test tests/GrcMvc.Tests/GrcMvc.Tests.csproj

# Build solution
dotnet build GrcMvc.sln

# View logs (Docker)
docker-compose logs -f grcmvc

# Access Hangfire dashboard (background jobs)
# http://localhost:8080/hangfire
```

## Environment Configuration

All secrets via environment variables (never hardcode). Key variables in `.env`:
- `CONNECTION_STRING` — PostgreSQL connection
- `JWT_SECRET` — Minimum 32 characters (generate: `openssl rand -base64 32`)
- `ALLOWED_HOSTS` — Semicolon-separated list for production

## Domain Modules

| Module | Entity | Controller | Service |
|--------|--------|------------|---------|
| Risk | `Risk.cs` | `RiskController` / `RiskApiController` | `IRiskService` |
| Control | `Control.cs` | `ControlController` / `ControlApiController` | `IControlService` |
| Audit | `Audit.cs`, `AuditFinding.cs` | `AuditController` | `IAuditService` |
| Policy | `Policy.cs`, `PolicyViolation.cs` | `PolicyController` | `IPolicyService` |
| Assessment | `Assessment.cs` | `AssessmentController` | `IAssessmentService` |
| Evidence | `Evidence.cs` | `EvidenceController` | `IEvidenceService` |
| Workflow | `WorkflowInstance.cs`, `WorkflowTask.cs` | `WorkflowController` | `IWorkflowService`, `IWorkflowEngineService` |

## RBAC System

**Location**: `Services/Implementations/RBAC/` and `Services/Interfaces/RBAC/`
- `Permission`, `Feature`, `RolePermission`, `RoleFeature` entities
- Use `IAuthorizationService` for permission checks
- Role profiles seeded in `Data/Seeds/RoleProfileSeeds.cs`

## Validation

FluentValidation in `Validators/` directory:
```csharp
public class CreateRiskDtoValidator : AbstractValidator<CreateRiskDto>
{
    public CreateRiskDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Probability).InclusiveBetween(1, 5);
    }
}
```

## Common Workflows

### Adding a New Entity
1. Create entity in `Models/Entities/{Entity}.cs`, extend `BaseEntity`
2. Create DTOs in `Models/DTOs/{Entity}Dto.cs` (Read, Create, Update variants)
3. Add `DbSet<Entity>` to `src/GrcMvc/Data/GrcDbContext.cs`
4. Add property to `Data/IUnitOfWork.cs` and `UnitOfWork.cs`
5. Create mapping in `Mappings/AutoMapperProfile.cs`
6. Create validator in `Validators/{Entity}Validators.cs` using FluentValidation
7. Create service interface in `Services/Interfaces/`, implementation in `Services/Implementations/`
8. Add DI in `Program.cs`
9. Create MVC controller in `Controllers/{Entity}Controller.cs` and API controller if needed
10. Run migration: `dotnet ef migrations add Add{Entity} && dotnet ef database update`

### Calling External Services (LLM, Email, File Storage)
Always **inject via constructor**, never instantiate directly:
```csharp
public class RiskService : IRiskService
{
    private readonly ILlmService _llmService;
    private readonly IEmailService _emailService;

    public RiskService(ILlmService llmService, IEmailService emailService, ...)
    {
        _llmService = llmService;
        _emailService = emailService;
    }
}
```

### Ensuring Tenant Isolation
Every query must filter by `TenantId`. Use `ITenantContextService`:
```csharp
var tenantId = _tenantContextService.GetCurrentTenantId();
var entities = _unitOfWork.Risks
    .AsQueryable()
    .Where(r => r.TenantId == tenantId)
    .ToList();
```

### Handling Domain Events
Implement `IGrcEvent` and register handler in event bus. `DashboardProjector` is the primary example.

## Testing Conventions
- Unit tests in `Tests/` mirror source structure
- Use xUnit + Moq
- Mock repositories and external services (LLM, Email)
- Test tenant context isolation in data access tests

## Key Files Quick Reference

| File | Purpose | Lines |
|------|---------|-------|
| `src/GrcMvc/Program.cs` | DI registration, JWT, CORS, rate limiting, Hangfire, Serilog, localization | 1165 |
| `src/GrcMvc/Data/GrcDbContext.cs` | EF Core DbContext, 50+ DbSets | — |
| `src/GrcMvc/Mappings/AutoMapperProfile.cs` | Entity↔DTO mappings | ~150 |
| `src/GrcMvc/Models/Entities/BaseEntity.cs` | Abstract base with multi-tenant support | ~43 |
| `src/GrcMvc/Services/Implementations/LlmService.cs` | AI-powered insights | 498 |
| `src/GrcMvc/Services/Analytics/DashboardProjector.cs` | Event-driven analytics | — |
| `src/GrcMvc/Middleware/SecurityHeadersMiddleware.cs` | OWASP security headers | — |
| `src/GrcMvc/Models/ApiResponse.cs` | Standardized API response wrapper | — |
| `docker-compose.yml` | PostgreSQL + GrcMvc containers | — |

---

**Last Updated**: January 2026
**Status**: GrcMvc Phase-based development with ecosystem roadmap (24 weeks)
