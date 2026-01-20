# Copilot Instructions for Shahin GRC Platform

**Last Updated:** 2026-01-20 | **For detailed patterns:** [COPILOT_INSTRUCTIONS_GRC_AGENTS.md](../COPILOT_INSTRUCTIONS_GRC_AGENTS.md)

## The System at a Glance

**Shahin AI GRC** is an enterprise **multi-tenant SaaS** platform for Governance, Risk, and Compliance automation. It orchestrates compliance via 12 AI agents, supports 6-stage GRC lifecycle (MAP→APPLY→PROVE→WATCH→FIX→VAULT), and enforces strict multi-tenancy + RBAC.

**Stack:** ASP.NET Core 8.0 MVC | ABP Framework | EF Core 8.0.8 | PostgreSQL 15 | OpenIddict SSO | Hangfire jobs | Claude Sonnet 4.5 agents

**Your #1 job:** **Every tenant operation must validate `TenantId`** via `ITenantContextService`. Non-negotiable—data isolation is the compliance foundation.

---

## Critical Architecture: Multi-Tenant Enforcement

**Every operation must validate `TenantId`.** This is not optional. Here's why and how:

### Why Multi-Tenancy Matters
- **Security:** Tenants cannot see each other's data
- **Compliance:** GRC regulations require data isolation
- **Architecture:** Built into every DbSet, service, controller

### The Pattern
```csharp
// ✅ CORRECT: Service validates tenant before querying
public class RiskService : IRiskService
{
    private readonly IRepository<Risk> _repo;
    private readonly ITenantContextService _tenantService;

    public async Task<Risk> GetAsync(Guid id)
    {
        var tenantId = _tenantService.TenantId; // Always from context
        if (string.IsNullOrEmpty(tenantId))
            throw new SecurityException("Tenant context missing");
        
        return await _repo.FirstOrDefaultAsync(r => 
            r.Id == id && r.TenantId == tenantId); // Always filter
    }
}

// ❌ WRONG: Cross-tenant data leak
var risk = await _dbContext.Risks.FirstOrDefaultAsync(r => r.Id == id);
```

### Why ABP Framework?
ABP is the **glue** for multi-tenancy, permissions, and auditing—not decorative:
- **Service registration:** Don't use `Program.cs` → use `GrcMvcAbpModule.cs`
- **Repositories:** Use `IRepository<T>` (adds automatic tenant filtering) → not raw DbContext
- **Permissions:** Dynamic resolution via `PermissionPolicyProvider.cs` (e.g., `[Authorize("Grc.Control.View")]` auto-resolves)

---

## Onboarding Flow: The Core Workflow

When an admin logs in **first time**, the system enforces this exact sequence:

```
[OnboardingRedirectMiddleware]
  → Checks: FirstAdminUserId + OnboardingStatus != "Completed"
  → If true: Redirect to Fast Start

[Fast Start] Org profile → select frameworks → generate baseline
  ↓
[Mission 1] Set up teams → assign roles → RACI mapping
  ↓
[Mission 2] Select controls → define evidence requirements
  ↓
[Mission 3] Connect integrations → auto-collect evidence
  ↓
[Update] tenant.OnboardingStatus = "Completed"
  ↓
[Dashboard] User can now access app normally
```

**Key files:** `OnboardingRedirectMiddleware.cs` | `OnboardingWizardProgressService.cs` | `SmartOnboardingService.cs`

**Critical rule:** Breaking sequence = `InvalidOperationException`. No skipping steps.

---

## Agent Governance (Mandatory Compliance Pattern)

These agents **orchestrate the entire system.** They are **whitelisted only**:
- `OnboardingAgent` — captures org profile, triggers rules
- `RulesEngineAgent` — determines applicable frameworks/controls
- `PlanAgent` — generates GRC implementation plan
- `WorkflowAgent` — assigns tasks, tracks completion
- `EvidenceAgent` — collects/validates compliance artifacts
- `DashboardAgent` — real-time KPI aggregation

**Every agent action MUST:**
1. ✅ Validate multi-tenant context (throw `SecurityException` if missing)
2. ✅ Log to `AuditReplayEvent` for audit trail
3. ✅ Return deterministic JSON with `Rationale` field

```csharp
// ✅ Template
AgentGovernancePolicy.EnforceMultiTenantContext(_tenantService.TenantId);
var result = await DoWork();
await LogAuditEventAsync("AgentName", result);
return new AgentOutputDto { 
    Result = result, 
    Rationale = JsonSerializer.Serialize(new { reason = "...", confidence = 0.95 }) 
};
```

## Common Patterns

### Middleware Template
```csharp
[RequireTenant]
public class YourMiddleware
{
    private readonly ITenantContextService _tenantService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(_tenantService.TenantId))
            throw new SecurityException("Tenant context missing");
        
        await _next(context);
    }
}
```

### Service Template
```csharp
public class YourService : IYourService
{
    private readonly ITenantContextService _tenantService;
    private readonly IRepository<YourEntity> _repo;
    
    public async Task<ResultDto> HandleAsync(InputDto input)
    {
        if (input.TenantId != _tenantService.TenantId)
            throw new SecurityException("Tenant mismatch");
        
        var entities = await _repo.GetListAsync(e => 
            e.TenantId == _tenantService.TenantId);
        return Map(entities);
---

## Developer Workflows

### Build & Run
```bash
# From workspace root
cd Shahin-Jan-2026/src/GrcMvc
dotnet build
dotnet run
# App at https://localhost:5010; database auto-migrates on startup
```

### Debug Multi-Tenant Issues
1. **Check tenant extraction:** `TenantResolutionMiddleware.cs` (resolves from subdomain/query param)
2. **Verify context is set:** `TenantContextService.TenantId` should NOT be null/empty
3. **Verify DbContext filters:** All entities have `HasQueryFilter(e => e.TenantId == __tenantId)`

### Key File Locations
| Path | Purpose |
|------|---------|
| `Program.cs` (~1700 lines) | Startup config, DI, middleware pipeline, Hangfire, OpenIddict |
| `Abp/GrcMvcAbpModule.cs` | ABP module registration (services, DbContext, permissions) |
| `Middleware/` | TenantResolution, OnboardingRedirect, SecurityHeaders |
| `Services/Agents/` | Agent services (OnboardingAgent, DashboardAgent, etc.) |
| `Data/GrcDbContext.cs` | 230 DbSets with global tenant filters |
| `Authorization/PermissionPolicyProvider.cs` | Dynamic `[Authorize("Grc.*")]` resolution |

---

## Common Pitfalls & Quick Fixes

| Problem | Fix |
|---------|-----|
| `TenantId` is null in service | Inject `ITenantContextService`; check middleware validates tenant |
| Query returns cross-tenant data | Add `&& e.TenantId == _tenantService.TenantId` to all queries |
| Permission check fails | Check `PermissionPolicyProvider.cs` + permission seeding in `GrcMvcAbpModule.cs` |
| Onboarding skips steps | Inspect `OnboardingRedirectMiddleware.cs` guard logic |
| Agent runs without audit trail | Always call `await LogAuditEventAsync()` before return |
| Services not injected | Register in `GrcMvcAbpModule.ConfigureServices()`, NOT in `Program.cs` |
| Tenant filter not applied to DbSet | Add `HasQueryFilter()` in `GrcDbContext.OnModelBuilding()` |
---

## What NOT to Do

❌ Register services in `Program.cs` (use ABP module)  
❌ Query without tenant filter (data leak!)  
❌ Create new agents outside whitelist (breaks audit + governance)  
❌ Skip onboarding steps (compliance violation)  
❌ Use raw SQL without `TenantId` WHERE clause  
❌ Emit agent output without `Rationale` JSON field  

---

## Questions?

1. **"How do I add a service?"** → Register in `GrcMvcAbpModule.ConfigureServices()`
2. **"How do I add a permission?"** → Define in `Permissions/GrcPermissionDefinitionProvider.cs`
3. **"How do I support a new entity?"** → Add DbSet, `HasQueryFilter()`, and ensure all queries filter by `TenantId`
4. **"Why is my agent not running?"** → Check `AgentGovernancePolicy.RequireAgentWhitelisted()` + audit logs
5. **For layer-by-layer patterns:** Read [COPILOT_INSTRUCTIONS_GRC_AGENTS.md](../COPILOT_INSTRUCTIONS_GRC_AGENTS.md) (395 lines, comprehensive reference)

---

**Updated**: January 2026 | **Version**: 2.0 (consolidated from fragmented docs)  
**Audience**: AI code agents, developers  
**Related**: [COPILOT_INSTRUCTIONS_GRC_AGENTS.md](../COPILOT_INSTRUCTIONS_GRC_AGENTS.md) (detailed), [CLAUDE.md](../CLAUDE.md) (project overview) multi-tenant entity
        AgentGovernancePolicy.EnforceMultiTenantContext(_tenantService.TenantId);
        AgentGovernancePolicy.RequireAgentWhitelisted(nameof(YourAgentService));
        
        var result = await DoWork(input);
        await LogAuditEventAsync("AgentExecuted", result);
        
        return new AgentOutputDto 
        { 
            Result = result,
            Rationale = JsonSerializer.Serialize(new { reason = "...", confidence = 0.95 })
        };
    }
}
```

## Validation Checklist

Before submitting code, verify:
- ✅ **Tenant check**: All queries filter by TenantId
- ✅ **ABP patterns**: Services registered in modules, not Program.cs
- ✅ **Controllers**: Have `[RequireTenant]` attribute
- ✅ **Agents**: Listed in whitelisted agents, emit audit logs
- ✅ **Errors**: No tenant info leaked in error messages
- ✅ **DTOs**: Controllers use DTOs, not entities
- ✅ **Tests**: Include unit & integration test stubs

## Useful Links

| Resource | Purpose |
|----------|---------|
| [COPILOT_INSTRUCTIONS_GRC_AGENTS.md](../COPILOT_INSTRUCTIONS_GRC_AGENTS.md) | Detailed patterns by layer |
| [CLAUDE.md](../CLAUDE.md) | Project overview & quick start |
| [Program.cs](../Shahin-Jan-2026/src/GrcMvc/Program.cs) | Startup configuration |
| [GrcMvcAbpModule.cs](../Shahin-Jan-2026/src/GrcMvc/Abp/GrcMvcAbpModule.cs) | Module definition |
| [PermissionPolicyProvider.cs](../Shahin-Jan-2026/src/GrcMvc/Authorization/PermissionPolicyProvider.cs) | Dynamic permission policies |
| [TenantContextService.cs](../Shahin-Jan-2026/src/GrcMvc/Services/Security/TenantContextService.cs) | Multi-tenant context |

## Quick Commands

```bash
# Start PostgreSQL + Redis
docker-compose up -d db redis

# Build and run
dotnet build && dotnet run

# Run tests
dotnet test --filter "Category=Governance"

# Create migration
dotnet ef migrations add YourMigrationName

# Apply migrations
dotnet ef database update
```

## Getting Help

- **Architecture questions**: See COPILOT_INSTRUCTIONS_GRC_AGENTS.md (Section 2-5)
- **Permission issues**: Check Authorization folder + PermissionPolicyProvider.cs
- **Onboarding flow**: Review OnboardingRedirectMiddleware + OnboardingWizardProgressService
- **Agents**: Check Agents folder + AgentGovernancePolicy.cs
- **Database**: See Migrations folder + DbContext classes

---

**Last Updated**: 2026-01-19  
**Version**: 1.0  
**Audience**: AI code agents, developers, code reviewers
