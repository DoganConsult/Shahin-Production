# GRC System - Copilot Instructions

## Project Overview

Enterprise **Governance, Risk, and Compliance (GRC)** platform built with **ASP.NET Core 8.0 MVC**, **Entity Framework Core 8.0.8**, and **PostgreSQL**. Single MVC application with RBAC, workflow engine, LLM integration, multi-tenancy, and modular domain areas (Risk, Audit, Control, Policy, Assessment, Evidence, Workflow).

**Key Stats**: 39 Services, 50+ Entities, 4 Background Jobs, 2 Middleware, Serilog logging, Rate limiting, JWT + Identity authentication.

## Architecture Patterns

### Layered Architecture
```
Controllers → Services → UnitOfWork → GenericRepository → GrcDbContext (EF Core)
```
- **Controllers**: MVC views (`{Module}Controller.cs`) + REST APIs (`{Module}ApiController.cs`)
- **Services**: Interface in `Services/Interfaces/`, implementation in `Services/Implementations/`
- **Repositories**: Generic pattern via `IUnitOfWork` in [Data/UnitOfWork.cs](src/GrcMvc/Data/UnitOfWork.cs)
- **DTOs**: Separate Create/Update/Read DTOs in `Models/DTOs/`

### Key Conventions

**Entities** extend `BaseEntity` (provides `Id`, `TenantId`, `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy`, `IsDeleted`):
```csharp
public class Risk : BaseEntity { /* domain properties */ }
```

**DTOs** follow naming pattern:
- `RiskDto` - Read operations
- `CreateRiskDto` - Creation with validation
- `UpdateRiskDto` - Updates with Id

**Services** use constructor injection with `IUnitOfWork`, `IMapper`, `ILogger<T>`:
```csharp
public RiskService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RiskService> logger)
```

**API responses** use standardized wrapper:
```csharp
return Ok(ApiResponse<T>.SuccessResponse(data, "Message"));
return BadRequest(ApiResponse<T>.ErrorResponse("Error message"));
```

## Adding New Entities

1. **Entity**: `Models/Entities/{Entity}.cs` extending `BaseEntity`
2. **DTOs**: `Models/DTOs/{Entity}Dto.cs` with Create/Update/Read variants
3. **DbSet**: Add to `Data/GrcDbContext.cs`
4. **Repository**: Property in `Data/IUnitOfWork.cs` and `UnitOfWork.cs`
5. **Mapping**: Add to `Mappings/AutoMapperProfile.cs`
6. **Validator**: `Validators/{Entity}Validators.cs` using FluentValidation
7. **Service**: Interface in `Services/Interfaces/`, impl in `Services/Implementations/`
8. **Controller**: MVC in `Controllers/{Entity}Controller.cs`, API in `{Entity}ApiController.cs`
9. **Migration**: `dotnet ef migrations add Add{Entity} --project src/GrcMvc`

## Build & Run Commands

```bash
# Development (Docker Compose - recommended)
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
```

## Environment Configuration

All secrets via environment variables (never hardcode). Key variables in `.env`:
- `CONNECTION_STRING` - PostgreSQL connection
- `JWT_SECRET` - Minimum 32 characters (generate: `openssl rand -base64 32`)
- `ALLOWED_HOSTS` - Semicolon-separated list for production

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

## Multi-Tenancy

- All entities have optional `TenantId` property from `BaseEntity`
- Tenant context resolved via `ITenantContextService`
- User-tenant mapping in `TenantUsers` table

## RBAC System

Located in `Services/Implementations/RBAC/` and `Services/Interfaces/RBAC/`:
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

## Workflow Type Services (10 Specialized)
Auto-registered in Program.cs:
- ControlImplementationWorkflow
- RiskAssessmentWorkflow
- ApprovalWorkflow
- EvidenceCollectionWorkflow
- ComplianceTestingWorkflow
- RemediationWorkflow
- PolicyReviewWorkflow
- TrainingAssignmentWorkflow
- AuditWorkflow
- ExceptionHandlingWorkflow



## Advanced Features (Often Overlooked)

### 1. Hangfire Background Jobs (`Services/BackgroundJobs/`)
Scheduled/recurring jobs for async processing:
- `CodeQualityMonitorJob` - Code analysis
- `EscalationJob` - Auto-escalate overdue tasks
- `NotificationDeliveryJob` - Send batch notifications
- `SlaMonitorJob` - Track SLA violations
```csharp
// Register in Program.cs (already done)
builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(connectionString));
app.UseHangfireServer();
app.UseHangfireDashboard();
```

### 2. LLM Service - Enterprise AI Integration (`Services/LlmService.cs`)
Tenant-specific LLM configuration with insights generation:
```csharp
public interface ILlmService
{
    Task<string> GenerateWorkflowInsightAsync(Guid workflowInstanceId, string context);
    Task<string> GenerateRiskAnalysisAsync(Guid riskId, string riskDescription);
    Task<string> GenerateComplianceRecommendationAsync(Guid assessmentId, string findings);
    Task<LlmConfiguration> GetTenantLlmConfigAsync(Guid tenantId);
}
```

### 3. Security & Middleware

**SecurityHeadersMiddleware** (`Middleware/SecurityHeadersMiddleware.cs`):
- Adds OWASP security headers (CSP, HSTS, X-Frame-Options, etc.)
- Removes server fingerprint

**Rate Limiting** (in Program.cs):
- Global: 100 req/min per user/IP
- API endpoints: 30 req/min
- Auth endpoints: 5 req/5min (brute force protection)

**Authentication**:
- Primary: ASP.NET Core Identity + cookie auth (MVC)
- Secondary: JWT Bearer tokens (API endpoints)
- Email confirmation in production
- Password: 12 chars, uppercase, lowercase, digit, special char
- Lockout: 3 failed attempts → 15 min lockout

### 4. Localization (Arabic RTL Default)
Default culture: Arabic (ar), Secondary: English (en)
Culture preference stored in cookie.

### 5. Email Services (Dual Mode)
- `SmtpEmailService` - Production SMTP sender
- `StubEmailService` - Development (returns success without sending)

### 6. Resilience Pattern
`ResilienceService` - Polly retry policies for fault tolerance.

### 7. Request Logging
`RequestLoggingMiddleware` - Logs all HTTP requests/responses with performance timing.

## Key Files Reference

- [Program.cs](src/GrcMvc/Program.cs) - 725 lines: DI registration, JWT, CORS, rate limiting, Hangfire, Serilog
- [GrcDbContext.cs](src/GrcMvc/Data/GrcDbContext.cs) - 50+ DbSets for all entities
- [AutoMapperProfile.cs](src/GrcMvc/Mappings/AutoMapperProfile.cs) - 148 lines of entity↔DTO mappings
- [BaseEntity.cs](src/GrcMvc/Models/Entities/BaseEntity.cs) - Abstract base with multi-tenant support
- [LlmService.cs](src/GrcMvc/Services/LlmService.cs) - 498 lines: AI-powered insights
- [SecurityHeadersMiddleware.cs](src/GrcMvc/Middleware/SecurityHeadersMiddleware.cs) - OWASP compliance
- [ApiResponse.cs](src/GrcMvc/Models/ApiResponse.cs) - Standardized API response wrapper
- [docker-compose.yml](docker-compose.yml) - PostgreSQL + GrcMvc containers
