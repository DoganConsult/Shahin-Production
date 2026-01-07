# GRC Permissions & Policy Enforcement System

Complete implementation of GRC permissions, policy enforcement, and menu system.

## 📁 Folder Structure

```
grc-permissions-policy-system/
├── Permissions/          # Permission definitions and management
├── Policy/              # Policy enforcement engine
├── Menu/                 # Menu contributor system
└── Documentation/       # Documentation and guides
```

## 🔐 Permissions System

### Files:
- `GrcPermissions.cs` - All permission constants (19 modules)
- `PermissionDefinitionProvider.cs` - ABP-style permission provider
- `PermissionSeederService.cs` - Seeds permissions to database
- `PermissionHelper.cs` - Helper utilities
- `PermissionAwareComponent.cs` - Blazor component base

### Permission Structure:
```
Grc.Home
Grc.Dashboard
Grc.Subscriptions (View, Manage)
Grc.Admin (Access, Users, Roles, Tenants)
Grc.Frameworks (View, Create, Update, Delete, Import)
Grc.Regulators (View, Manage)
Grc.Assessments (View, Create, Update, Submit, Approve)
Grc.ControlAssessments (View, Manage)
Grc.Evidence (View, Upload, Update, Delete, Approve)
Grc.Risks (View, Manage, Accept)
Grc.Audits (View, Manage, Close)
Grc.ActionPlans (View, Manage, Assign, Close)
Grc.Policies (View, Manage, Approve, Publish)
Grc.ComplianceCalendar (View, Manage)
Grc.Workflow (View, Manage)
Grc.Notifications (View, Manage)
Grc.Vendors (View, Manage, Assess)
Grc.Reports (View, Export)
Grc.Integrations (View, Manage)
```

## 🛡️ Policy Enforcement System

### Core Files:
- `PolicyEnforcer.cs` - Main enforcement engine
- `PolicyStore.cs` - YAML policy loader with hot-reload
- `PolicyContext.cs` - Evaluation context
- `PolicyEnforcementHelper.cs` - Easy integration helper
- `PolicyViolationException.cs` - Exception for violations

### Supporting Files:
- `DotPathResolver.cs` - Resolves dot-path expressions
- `MutationApplier.cs` - Applies policy mutations
- `PolicyAuditLogger.cs` - Audit logging
- `PolicyValidationHelper.cs` - Validation utilities
- `PolicyResourceWrapper.cs` - Resource wrapper for evaluation

### Policy File:
- `grc-baseline.yml` - Baseline governance policies

### Policy Rules:
1. **REQUIRE_DATA_CLASSIFICATION** - All resources must have data classification
2. **REQUIRE_OWNER** - All resources must have owner
3. **PROD_RESTRICTED_MUST_HAVE_APPROVAL** - Restricted data in prod requires approval
4. **NORMALIZE_EMPTY_LABELS** - Normalizes invalid label values

## 📋 Menu System

### Files:
- `GrcMenuContributor.cs` - Menu contributor with Arabic menu (19 items)
- `MenuInterfaces.cs` - Menu system interfaces

### Menu Items (Arabic):
1. الصفحة الرئيسية (Home) - `/`
2. لوحة التحكم (Dashboard) - `/dashboard`
3. الاشتراكات (Subscriptions) - `/subscriptions`
4. الإدارة (Admin) - `/admin`
   - المستخدمون (Users) - `/admin/users`
   - الأدوار (Roles) - `/admin/roles`
   - العملاء (Tenants) - `/admin/tenants`
5. مكتبة الأطر التنظيمية (Frameworks) - `/frameworks`
6. الجهات التنظيمية (Regulators) - `/regulators`
7. التقييمات (Assessments) - `/assessments`
8. تقييمات الضوابط (Control Assessments) - `/control-assessments`
9. الأدلة (Evidence) - `/evidence`
10. إدارة المخاطر (Risks) - `/risks`
11. إدارة المراجعة (Audits) - `/audits`
12. خطط العمل (Action Plans) - `/action-plans`
13. إدارة السياسات (Policies) - `/policies`
14. تقويم الامتثال (Compliance Calendar) - `/compliance-calendar`
15. محرك سير العمل (Workflow) - `/workflow`
16. الإشعارات (Notifications) - `/notifications`
17. إدارة الموردين (Vendors) - `/vendors`
18. التقارير والتحليلات (Reports) - `/reports`
19. مركز التكامل (Integrations) - `/integrations`

## 🚀 Integration

### Program.cs Registration:
```csharp
// Permissions
builder.Services.AddSingleton<IPermissionDefinitionProvider, GrcPermissionDefinitionProvider>();
builder.Services.AddScoped<PermissionSeederService>();

// Policy Enforcement
builder.Services.AddScoped<IPolicyEnforcer, PolicyEnforcer>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();
builder.Services.AddScoped<IDotPathResolver, DotPathResolver>();
builder.Services.AddScoped<IMutationApplier, MutationApplier>();
builder.Services.AddScoped<IPolicyAuditLogger, PolicyAuditLogger>();
builder.Services.AddScoped<PolicyEnforcementHelper>();

// Menu
builder.Services.AddScoped<GrcMenuContributor>();
```

### Usage in Services:
```csharp
public class EvidenceService
{
    private readonly PolicyEnforcementHelper _policyHelper;
    
    public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto dto)
    {
        var evidence = MapToEntity(dto);
        
        // Enforce policy
        await _policyHelper.EnforceCreateAsync(
            resourceType: "Evidence",
            resource: evidence,
            dataClassification: dto.DataClassification,
            owner: dto.Owner);
        
        // Save to database
        await _context.Evidences.AddAsync(evidence);
        await _context.SaveChangesAsync();
        
        return MapToDto(evidence);
    }
}
```

## 📊 Status

### ✅ Completed:
- Permission definitions (19 modules)
- Policy enforcement engine
- Menu contributor with Arabic menu
- Policy YAML file
- Integration helpers

### ⏳ Pending:
- Role seeding service (assign permissions to roles)
- Complete policy enforcement in all services
- Unit tests
- Integration tests

## 📝 Notes

- All permissions follow pattern: `Grc.{Module}.{Action}`
- Policy evaluation is deterministic (priority-based)
- Menu items are permission-aware
- Policy violations throw `PolicyViolationException` with remediation hints

---

**Created:** 2025-01-22  
**Version:** 1.0.0  
**Status:** Production Ready (Core Components)
