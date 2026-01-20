# Architectural Compliance Audit

## Guidelines

1. ✅ **Always inject and validate ITenantContextService in services**
2. ✅ **Register new services in GrcMvcAbpModule.cs, not in Program.cs**
3. ✅ **Use [RequireTenant] on controllers**
4. ✅ **Use DTOs for controller/service boundaries**
5. ✅ **For agent code, always log to AuditReplayEvent and return deterministic JSON with a Rationale field**

---

## 🔍 Audit Results

### ✅ COMPLIANT: Controllers Using [RequireTenant]

**26 controllers** are using `[RequireTenant]` attribute:
- ✅ TenantAdminController
- ✅ VendorsController
- ✅ DashboardController
- ✅ AdminController
- ✅ BenchmarkingController
- ✅ ExcellenceController
- ✅ KPIsController
- ✅ SustainabilityController
- ✅ WorkflowController
- ✅ TrendsController
- ✅ RoadmapController
- ✅ ProgramsController
- ✅ RegulatorsController
- ✅ ResilienceController
- ✅ RiskController
- ✅ PolicyController
- ✅ InitiativesController
- ✅ FrameworksController
- ✅ EvidenceController
- ✅ ControlController
- ✅ ComplianceCalendarController
- ✅ CertificationController
- ✅ AuditController
- ✅ AssessmentExecutionController
- ✅ AssessmentController
- ✅ ActionPlansController

---

### ❌ VIOLATION: Services Registered in Program.cs

**Services that should be moved to GrcMvcAbpModule.cs:**

Located in `Program.cs` (lines 864-875):
```csharp
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IControlService, ControlService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAssessmentExecutionService, AssessmentExecutionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IActionPlanService, ActionPlanService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IRegulatorService, RegulatorService>();
builder.Services.AddScoped<IComplianceCalendarService, ComplianceCalendarService>();
```

**Other services in Program.cs that should be reviewed:**
- `ISiteSettingsService` (line 601)
- `ITenantDatabaseResolver` (line 641)
- `IDbContextFactory<GrcDbContext>` (line 644)
- `IGenericRepository<>` (line 851)
- `IUnitOfWork` (line 854)
- `IAppInfoService` (line 859)

---

### ⚠️ PARTIAL: ITenantContextService Injection

**Services that DO inject ITenantContextService:**
- ✅ CertificationService
- ✅ IncidentResponseService
- ✅ WorkspaceContextService
- ✅ WorkspaceManagementService
- ✅ ControlTestService
- ✅ SupportTicketService
- ✅ UserNotificationDispatcher

**Services that DO NOT inject ITenantContextService:**
- ❌ VendorService (uses IWorkspaceContextService instead)
- ❌ SerialCodeService (no tenant context)
- ❌ TenantService (manages tenants, doesn't need context)
- ❌ RiskService (needs audit)
- ❌ ControlService (needs audit)
- ❌ AssessmentService (needs audit)
- ❌ AssessmentExecutionService (needs audit)
- ❌ AuditService (needs audit)
- ❌ PolicyService (needs audit)
- ❌ WorkflowService (needs audit)
- ❌ FileUploadService (needs audit)
- ❌ ActionPlanService (needs audit)
- ❌ RegulatorService (needs audit)
- ❌ ComplianceCalendarService (needs audit)

---

## 📋 Migration Plan

### Phase 1: Move Service Registrations to GrcMvcAbpModule.cs

**Target**: `GrcMvcAbpModule.cs` → `ConfigureServices` method

**Services to move:**
1. Business Logic Services (12 services)
2. Repository Services (2 services)
3. Infrastructure Services (3 services)

### Phase 2: Add ITenantContextService to Services

**Services requiring ITenantContextService:**
1. RiskService
2. ControlService
3. AssessmentService
4. AssessmentExecutionService
5. AuditService
6. PolicyService
7. WorkflowService
8. FileUploadService
9. ActionPlanService
10. RegulatorService
11. ComplianceCalendarService

**Validation pattern:**
```csharp
private readonly ITenantContextService _tenantContext;

public MyService(..., ITenantContextService tenantContext)
{
    _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    
    // Validate tenant context
    if (!_tenantContext.HasTenantContext())
    {
        throw new InvalidOperationException("Tenant context is required for this operation");
    }
}
```

### Phase 3: Verify DTO Usage

**Check all controllers:**
- ✅ All controller actions should accept DTOs, not entities
- ✅ All service methods should return DTOs, not entities
- ✅ No direct entity exposure in API responses

### Phase 4: Agent Code Audit

**For agent services (EvidenceAgentService, etc.):**
- ✅ Log to AuditReplayEvent
- ✅ Return deterministic JSON with Rationale field
- ✅ Ensure consistent response format

---

## 🎯 Priority Actions

### High Priority
1. **Move service registrations** from Program.cs to GrcMvcAbpModule.cs
2. **Add ITenantContextService** to all business logic services
3. **Validate tenant context** in service constructors

### Medium Priority
4. **Audit DTO usage** across all controllers
5. **Verify [RequireTenant] coverage** for all tenant-scoped controllers

### Low Priority
6. **Agent code compliance** audit and fixes
7. **Documentation updates** for architectural patterns

---

## 📝 Reference Files

- `Program.cs` - Startup, DI, middleware
- `GrcMvcAbpModule.cs` - ABP module/service registration
- `PermissionPolicyProvider.cs` - Dynamic permission policies
- `Services/Security/TenantContextService.cs` - Tenant context enforcement

---

**Last Updated**: 2026-01-20
