# GRC Permissions & Policy System - Integration Guide

## Quick Start

### 1. Register Services (Program.cs)

```csharp
// Permissions System
builder.Services.AddSingleton<IPermissionDefinitionProvider, GrcPermissionDefinitionProvider>();
builder.Services.AddScoped<PermissionSeederService>();

// Policy Enforcement System
builder.Services.AddScoped<IPolicyEnforcer, PolicyEnforcer>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();
builder.Services.AddScoped<IDotPathResolver, DotPathResolver>();
builder.Services.AddScoped<IMutationApplier, MutationApplier>();
builder.Services.AddScoped<IPolicyAuditLogger, PolicyAuditLogger>();
builder.Services.AddScoped<PolicyEnforcementHelper>();

// Menu System
builder.Services.AddScoped<GrcMenuContributor>();
```

### 2. Use Policy Enforcement in Services

```csharp
public class YourService
{
    private readonly PolicyEnforcementHelper _policyHelper;
    
    public YourService(PolicyEnforcementHelper policyHelper)
    {
        _policyHelper = policyHelper;
    }
    
    public async Task<YourDto> CreateAsync(CreateYourDto dto)
    {
        var entity = MapToEntity(dto);
        
        // Enforce policy before saving
        await _policyHelper.EnforceCreateAsync(
            resourceType: "YourResource",
            resource: entity,
            dataClassification: dto.DataClassification ?? "internal",
            owner: dto.Owner ?? GetCurrentUser());
        
        // Save to database
        await _context.YourEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        
        return MapToDto(entity);
    }
}
```

### 3. Check Permissions in Controllers

```csharp
[Authorize(GrcPermissions.Evidence.Upload)]
public async Task<IActionResult> UploadEvidence(CreateEvidenceDto dto)
{
    // Your code here
}
```

### 4. Use Menu Contributor

The menu is automatically built by `GrcMenuContributor` based on user permissions.

## Policy Rules

### Current Rules:
1. **REQUIRE_DATA_CLASSIFICATION** - Requires data classification label
2. **REQUIRE_OWNER** - Requires owner label
3. **PROD_RESTRICTED_MUST_HAVE_APPROVAL** - Restricted data in prod needs approval
4. **NORMALIZE_EMPTY_LABELS** - Normalizes invalid values

### Adding New Rules:

Edit `etc/policies/grc-baseline.yml`:

```yaml
rules:
  - id: YOUR_NEW_RULE
    priority: 40
    description: "Your rule description"
    enabled: true
    match:
      resource:
        type: "YourResource"
    when:
      - op: equals
        path: "metadata.labels.status"
        value: "draft"
    effect: deny
    severity: medium
    message: "Your error message"
    remediation:
      hint: "How to fix it"
```

## Permissions

### Adding New Permission:

1. Add to `GrcPermissions.cs`:
```csharp
public static class YourModule
{
    public const string Default = GroupName + ".YourModule";
    public const string View = Default + ".View";
    public const string Create = Default + ".Create";
}
```

2. Add to `PermissionDefinitionProvider.cs`:
```csharp
var yourModule = grc.AddPermission(GrcPermissions.YourModule.Default, "Your Module");
yourModule.AddChild(GrcPermissions.YourModule.View, "View");
yourModule.AddChild(GrcPermissions.YourModule.Create, "Create");
```

3. Add to menu in `GrcMenuContributor.cs`:
```csharp
m.AddItem(new ApplicationMenuItem("Grc.YourModule", "اسمك", "/your-module", icon: "fas fa-icon")
    .RequirePermissions(GrcPermissions.YourModule.View));
```

## Troubleshooting

### Policy Not Enforcing?
- Check policy file path in `appsettings.json`
- Verify `PolicyStore` is registered as singleton
- Check logs for policy loading errors

### Menu Items Not Showing?
- Verify user has required permissions
- Check `RoleFeatures` table for feature access
- Verify `GrcMenuContributor` is registered

### Permission Check Failing?
- Verify permission is defined in `GrcPermissions`
- Check `RolePermissions` table
- Verify user has role with permission
