# Shahin AI GRC Platform – AI Agent Instructions

**For**: GitHub Copilot, Claude, Cursor, Windsurf, Continue  
**Stack**: .NET 8.0 MVC | PostgreSQL 15 | ABP Multi-Tenant | 12 Claude AI Agents  
**Comprehensive Docs**: See `CLAUDE.md` (867 lines), `AUDIT_REPORT.md`, `ECOSYSTEM_ROADMAP_INDEX.md`

---

## 🚨 Critical ABP Multi-Tenant Rules (NON-NEGOTIABLE)

1. **NEVER bypass tenant isolation** – All data queries MUST filter by `TenantId` via `ITenantContextService.GetCurrentTenantId()`
2. **Dual DB architecture** – `GrcDbContext` (app data) + `GrcAuthDbContext` (auth/identity isolation for security)
3. **All entities extend `BaseEntity`** – Auto-provides: `Id`, `TenantId`, `CreatedDate/By`, `ModifiedDate/By`, `IsDeleted`
4. **DTO naming convention** – `{Entity}Dto` (read), `Create{Entity}Dto`, `Update{Entity}Dto` – NEVER mix purposes
5. **Service pattern** – Interface `I{Name}Service` in `Services/Interfaces/`, impl in `Services/Implementations/{Name}Service`
6. **Constructor injection ONLY** – Use `IUnitOfWork`, `IMapper`, `ILogger<T>`, `ITenantContextService` – NEVER instantiate directly
7. **API responses standardized** – `ApiResponse<T>.SuccessResponse(data, "msg")` or `.ErrorResponse("error")`
8. **Global query filters enforced** – `GrcDbContext.OnModelCreating` applies `GetCurrentTenantId()` filter to 100+ entities
9. **Secrets via environment ONLY** – NEVER hardcode keys/passwords; use `.env` → `IConfiguration` → strongly-typed options
10. **Validators = FluentValidation** – Located in `Validators/`, auto-registered via `AddValidatorsFromAssemblyContaining<>`

---

## 🏗️ Architecture Patterns (Quick Reference)

### Layered Flow
```
Controllers (MVC/API) → Services (Business Logic) → IUnitOfWork → Repositories → DbContext (EF Core) → PostgreSQL
```

### Key Components
| Layer | Pattern | Example |
|-------|---------|---------|
| **Entity** | Extends `BaseEntity`, has `TenantId` | `Risk.cs`, `Control.cs`, `Audit.cs` |
| **DTO** | Read/Create/Update separation | `RiskDto`, `CreateRiskDto`, `UpdateRiskDto` |
| **Service** | Interface + Impl, inject `IUnitOfWork` | `IRiskService` → `RiskService` |
| **Controller** | MVC (`Controllers/`) + API (`Controllers/Api/`) | `RiskController`, `RiskApiController` |
| **Repository** | Via `IUnitOfWork.{Entity}` | `_unitOfWork.Risks.GetByIdAsync()` |
| **Validation** | FluentValidation in `Validators/` | `CreateRiskDtoValidator` |
| **Mapping** | AutoMapper profile in `Mappings/` | `RiskDto` ↔ `Risk` |

### Multi-Tenant Enforcement (Example)
```csharp
// ❌ WRONG - Bypasses tenant isolation
var risks = _unitOfWork.Risks.GetAllAsync();

// ✅ CORRECT - Tenant-scoped query
var tenantId = _tenantContextService.GetCurrentTenantId();
var risks = await _unitOfWork.Risks
    .AsQueryable()
    .Where(r => r.TenantId == tenantId)
    .ToListAsync();
```

### Service Constructor (Standard Pattern)
```csharp
public class RiskService : IRiskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RiskService> _logger;
    private readonly ITenantContextService _tenantContext;

    public RiskService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RiskService> logger,
        ITenantContextService tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _tenantContext = tenantContext;
    }
}
```

---

## 🔧 Integration Points (External Systems)

| Service | Purpose | Location |
|---------|---------|----------|
| **Camunda BPM** | BPMN workflow execution | External at `:8081` |
| **Hangfire** | Background jobs | Embedded, dashboard at `/hangfire` |
| **Kafka** | Event streaming | External at `:9092` |
| **RabbitMQ** | Message queue | External at `:5672` |
| **Redis** | Caching (optional) | External at `:6379` |
| **ClickHouse** | OLAP analytics | External at `:8123` |
| **Stripe** | Payment gateway | Via `IPaymentGatewayService` |
| **Microsoft Graph** | Email integration | Via `IEmailIntegrationService` |
| **Claude AI** | 12 specialized agents | Via `Services/Implementations/AI/` |

---

## 📋 Common Workflows

### Adding a New Entity
1. Create entity in `Models/Entities/{Entity}.cs` extending `BaseEntity`
2. Create DTOs: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto` in `Models/DTOs/`
3. Add `DbSet<{Entity}>` to `Data/GrcDbContext.cs`
4. Add to `IUnitOfWork` interface and `UnitOfWork` implementation
5. Create AutoMapper profile in `Mappings/AutoMapperProfile.cs`
6. Create `{Entity}Validators.cs` using FluentValidation
7. Create `I{Entity}Service` interface + `{Entity}Service` implementation
8. Register service in `Program.cs`: `builder.Services.AddScoped<I{Entity}Service, {Entity}Service>();`
9. Create MVC controller in `Controllers/` and/or API controller in `Controllers/Api/`
10. Run migration: `dotnet ef migrations add Add{Entity} && dotnet ef database update`

### Querying with Tenant Isolation
```csharp
// Always inject ITenantContextService
private readonly ITenantContextService _tenantContext;

// Get current tenant (returns null for public endpoints, migrations, seeding)
var tenantId = _tenantContext.GetCurrentTenantId();

// Query with explicit filter (defensive programming)
var data = await _unitOfWork.Risks
    .AsQueryable()
    .Where(r => r.TenantId == tenantId)
    .ToListAsync();

// Trust global query filters ONLY after verification
// (filters defined in GrcDbContext.OnModelCreating)
```

### Handling Background Jobs
```csharp
// Located in BackgroundJobs/ folder
// Registered in Program.cs via Hangfire
// Examples: EscalationJob, SlaMonitorJob, NotificationDeliveryJob, CodeQualityMonitorJob

// Access dashboard: http://localhost:5008/hangfire
// Manual trigger: BackgroundJob.Enqueue(() => service.Method());
```

---

## 🔒 Security Essentials

- **Authentication**: ASP.NET Core Identity (cookies for MVC) + JWT Bearer (API endpoints)
- **Password Policy**: 12+ chars, upper/lower/digit/special, lockout after 3 failed attempts (15 min)
- **RBAC**: 214+ permissions, 15 roles, claims-based → database fallback → admin bypass for Admin/Owner/PlatformAdmin
- **Middleware**: `SecurityHeadersMiddleware` (OWASP headers), `TenantResolutionMiddleware`, `RequestLoggingMiddleware`
- **Rate Limiting**: Global 100 req/min, API 30 req/min, Auth 5 req/5min (anti-brute-force)
- **Secrets**: Environment variables ONLY (`.env` → `IConfiguration` → `IOptions<T>` pattern)

---

## 🌍 Localization (Bilingual EN/AR)

- **Default**: Arabic (ar), Secondary: English (en)
- **Resources**: `Resources/*.resx` (2,495 EN + 2,399 AR strings)
- **RTL Support**: Automatic via culture middleware (`Program.cs` lines 366-387)
- **Culture Preference**: Stored in cookie, defaults to Accept-Language header
- **Usage**: `IStringLocalizer<SharedResource>` injection in controllers/services

---

## 🧪 Testing Conventions

- **Location**: `tests/GrcMvc.Tests/` mirrors `src/GrcMvc/` structure
- **Framework**: xUnit + Moq + FluentAssertions
- **Mocking**: Mock `IUnitOfWork`, `ITenantContextService`, external services (Email, LLM, Payment)
- **Tenant Tests**: Always verify tenant isolation in data access tests
- **Command**: `dotnet test tests/GrcMvc.Tests/GrcMvc.Tests.csproj`

---

## 📚 Reference Files (Quick Lookup)

| File | Lines | Purpose |
|------|-------|---------|
| `Program.cs` | 1,400+ | DI, middleware, Hangfire, localization, JWT, CORS, rate limiting |
| `GrcDbContext.cs` | 2,131 | 230+ DbSets, tenant filters, ABP integration |
| `GrcAuthDbContext.cs` | — | Separate Identity DB for security isolation |
| `AutoMapperProfile.cs` | ~150 | Entity ↔ DTO mappings |
| `BaseEntity.cs` | 43 | Multi-tenant base with audit fields |
| `IUnitOfWork.cs` | — | Repository abstraction (200+ entities) |
| `CLAUDE.md` | 867 | Comprehensive project instructions |
| `AUDIT_REPORT.md` | 461 | Security audit (B+ overall, A auth/multi-tenant) |
| `ECOSYSTEM_ROADMAP_INDEX.md` | — | 24-week $750K development roadmap |

---

## 🚀 Development Quick Commands

```bash
# Run app (Docker recommended)
docker-compose up -d                # Full stack (DB, Redis, Kafka, Camunda, app)
dotnet run --project src/GrcMvc    # Manual run (requires separate DB setup)

# Database
dotnet ef migrations add MigrationName --project src/GrcMvc
dotnet ef database update --project src/GrcMvc

# Tests
dotnet test tests/GrcMvc.Tests

# Build
dotnet build GrcMvc.sln

# Logs
docker-compose logs -f grcmvc

# Access Points
# - App: http://localhost:8888 (configurable via APP_PORT in .env)
# - Hangfire: http://localhost:8888/hangfire
# - Swagger: http://localhost:8888/swagger (if enabled)
# - Camunda: http://localhost:8081/camunda
```

---

**Last Updated**: January 2026  
**Maintained By**: Shahin AI Engineering Team  
**Compliance**: KSA (NCA, SAMA, PDPL, CITC) + International (ISO 27001, SOC 2, NIST, GDPR)
