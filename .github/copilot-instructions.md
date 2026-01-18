# GitHub Copilot Instructions for Shahin AI GRC Platform

> **AI Agents**: Follow these patterns for code generation, refactoring, and debugging  
> **Updated**: January 2026  
> **Stack**: ASP.NET Core 8.0 MVC + PostgreSQL + ABP Framework Multi-Tenancy + 12 Claude AI Agents

---

## 🎯 Quick Start for AI Agents

**Project Type**: Enterprise GRC SaaS with 12 specialized AI agents (Claude Sonnet 4.5)  
**Active Codebase**: `src/GrcMvc/` (monolithic MVC app, 200+ entities, 100+ controllers)  
**Database**: PostgreSQL 15 via EF Core 8.0.8 (multi-tenant with query filters)  
**Key Dependencies**: ABP Framework, Hangfire, MassTransit, Camunda BPM, Serilog, QuestPDF

### Essential Commands
```bash
# Development
docker-compose up -d               # Start PostgreSQL, Redis, Kafka, Camunda, RabbitMQ
dotnet run --project src/GrcMvc   # Run app → http://localhost:5010

# Testing & Debugging
dotnet test tests/GrcMvc.Tests
dotnet ef migrations add MigrationName --project src/GrcMvc  # EF Core migrations

# Background jobs: http://localhost:5010/hangfire
# Camunda BPM: http://localhost:8080/camunda (if enabled)
# API docs: http://localhost:5010/swagger
```

---

## 🏗️ Core Architecture Patterns

### Multi-Tenancy + Workspace Isolation
- All entities extend `BaseEntity` with `TenantId` (Guid?) and optional `WorkspaceId`
- Global query filters in `GrcDbContext.OnModelCreating()` auto-scope queries
- Tenant context via `ITenantContextService.GetCurrentTenantId()` (never throws)
- Workspace context via `IWorkspaceContextService` for team collaboration
- Separate auth DB (`GrcAuthDbContext`) for security isolation

### 12 AI Agents (Claude-Based)
- Unified `ClaudeAgentService` handles 7+ agent types: Compliance, Risk, Audit, Policy, Analytics, Report, Workflow
- Agents: DiagnosticAgentService, SupportAgentService, EmailAiService
- Configuration in `appsettings.json` under `ClaudeAgents` (API key required)
- Governance: `AgentApprovalGate`, `AgentSoDRule`, `AgentConfidenceScore`
- Example: Risk analysis via `ILlmService.GenerateRiskInsights()`

### Workflow Engine (Custom + Camunda BPM)
- 10 workflow types: Control Implementation, Risk Assessment, Approval, Evidence Collection, etc.
- Custom engine: `WorkflowInstance`/`WorkflowTask` entities, state transitions via task completion
- Camunda integration: BPMN processes for complex orchestrations
- Auto-escalation via `EscalationJob` (Hangfire)

### Event-Driven Analytics
- `DashboardProjector` listens to `IGrcEvent` handlers for real-time projections
- Projections: compliance scores, risk heatmaps, task metrics (no raw data queries)
- MassTransit for event streaming (Kafka/RabbitMQ)

### Localization & Security
- Bilingual: English (2,495 strings) + Arabic (2,399 strings) in `Resources/`
- RTL support via `rtl.css`
- Security: OWASP headers (`SecurityHeadersMiddleware`), rate limiting (100 req/min), JWT (32+ char secret)

## Key Conventions & Patterns

### Naming & Code Style
- Entities: PascalCase singular (e.g., `Risk`, `Control`)
- Controllers: `{Entity}Controller` (MVC), `{Entity}ApiController` (API)
- Services: `{Entity}Service` implementing `I{Entity}Service`
- Async/await everywhere; DI via constructor; `ILogger<T>` for logging
- FluentValidation for DTOs; AutoMapper for mappings

### Database Patterns
- Soft delete: `IsDeleted` flag on all entities (`BaseEntity`)
- Audit: `CreatedBy`, `CreatedAt`, `ModifiedBy`, `ModifiedAt`, `RowVersion`
- Concurrency: Optimistic via `RowVersion`
- Migrations: 60+ files; run `dotnet ef database update`

### Adding New Entities
1. Extend `BaseEntity` in `Models/Entities/{Entity}.cs`
2. Add `DbSet<Entity>` to `GrcDbContext.cs`
3. Create DTOs in `Models/DTOs/` (Read/Create/Update variants)
4. Service: `I{Entity}Service` + implementation
5. Controller: `{Entity}Controller.cs`
6. Validator: `Validators/{Entity}Validators.cs`
7. Migration: `dotnet ef migrations add Add{Entity}`

### Tenant-Aware Queries
```csharp
var tenantId = _tenantContextService.GetCurrentTenantId();
var entities = _unitOfWork.Risks
    .Where(r => r.TenantId == tenantId)
    .ToList();
```

### AI Agent Calls
```csharp
var insights = await _llmService.GenerateInsights(request, tenantConfig);
```

## Integration Points
- Claude AI: 12 agents via `ClaudeAgentService`
- Microsoft Graph: OAuth2 email via `GraphSubscriptionsController`
- SMTP: Dual mode (`SmtpEmailService` vs `StubEmailService`)
- Camunda BPM: Workflow orchestration
- Kafka/RabbitMQ: Event streaming via MassTransit
- Redis: Caching (optional)

## Active Roadmap Reminder
- **24-week ecosystem roadmap** available in workspace
- **Current Phase**: Phase 1 - Foundation (Weeks 1-6)
- **Start Here**: `ECOSYSTEM_ROADMAP_INDEX.md`
- **Investment**: $750K over 24 weeks

## Key Files Reference
- `Program.cs`: DI, middleware, Hangfire, MassTransit, localization (2,445 lines)
- `GrcDbContext.cs`: 230+ DbSets, query filters (2,361 lines)
- `AutoMapperProfile.cs`: Entity↔DTO mappings (~150 lines)
- `BaseEntity.cs`: Multi-tenant base with audit fields
- `ClaudeAgentService.cs`: Unified AI agent handler (498 lines)
- `DashboardProjector.cs`: Event-driven analytics
- `SecurityHeadersMiddleware.cs`: OWASP headers
- `docker-compose.yml`: 11 services orchestration

---

**Last Updated**: January 16, 2026  
**Status**: GRC Phase-based development with 12 AI agents + Camunda BPM

### 1. Hangfire Background Jobs
**Location**: `src/GrcMvc/BackgroundJobs/`
- **CodeQualityMonitorJob** — Code analysis
- **EscalationJob** — Auto-escalate overdue tasks
- **NotificationDeliveryJob** — Batch email sending
- **SlaMonitorJob** — Track SLA violations
- **Dashboard**: http://localhost:8080/hangfire

### 2. LLM Service (Tenant-Configurable AI)
`src/GrcMvc/Services/Implementations/LlmService.cs` (498 lines)
- Generate workflow insights, risk analysis, compliance recommendations
- Tenant-specific configuration (API keys, models, tone)
- Never hardcode — All LLM config via environment or database

### 3. Security & Middleware

**SecurityHeadersMiddleware** — OWASP security headers (CSP, HSTS, X-Frame-Options), server fingerprint removal

**Rate Limiting**:
- Global: 100 req/min per user/IP
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
