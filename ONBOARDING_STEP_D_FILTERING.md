# Step D Dropdown Filtering Implementation

## Overview

Step D (Scope Definition) dropdown menus are now **database-driven and filtered** based on Step C (Regulatory and Framework Applicability) selections. All options come from the `ReferenceData` table and are dynamically filtered based on:

1. **Audit Scope Type** (from Step C)
2. **Selected Frameworks** (mandatory + optional from Step C)
3. **Selected Regulators** (primary + secondary from Step C)
4. **Industry Sector** (from Step A)
5. **Organization Type** (from Step A)

## How It Works

### Step C Selections → Step D Filters

When Step C is completed, the following data is saved:
- `PrimaryRegulatorsJson` - List of primary regulators
- `SecondaryRegulatorsJson` - List of secondary regulators
- `MandatoryFrameworksJson` - List of mandatory frameworks
- `OptionalFrameworksJson` - List of optional frameworks
- `AuditScopeType` - enterprise / business unit / system / process / vendor

### Step D Filtering Logic

When Step D loads, the controller:

1. **Parses Step C selections** from JSON fields
2. **Builds filter context** with:
   - Audit scope type
   - Framework codes (mandatory + optional)
   - Regulator codes (primary + secondary)
   - Industry sector
   - Organization type

3. **Calls ReferenceDataService** with filter context:
   ```csharp
   var filterContext = new Dictionary<string, object>
   {
       { "AuditScopeType", wizard.AuditScopeType ?? "enterprise" },
       { "FrameworkCodes", allFrameworks },
       { "RegulatorCodes", primaryRegulators },
       { "IndustrySector", wizard.IndustrySector ?? "" },
       { "OrganizationType", wizard.OrganizationType ?? "" }
   };
   
   var filteredOptions = await _referenceDataService.GetFilteredOptionsAsync(
       category, filterContext, language);
   ```

4. **Returns filtered options** to the view

### Database Filtering

The `ReferenceDataService.GetFilteredOptionsAsync` method filters options by:

1. **Audit Scope Type**: Matches `MetadataJson` containing `"auditScopeType":"{value}"`
2. **Framework Codes**: Matches `MetadataJson` containing `"frameworkCode":"{code}"`
3. **Regulator Codes**: Matches `MetadataJson` containing `"regulatorCode":"{code}"`
4. **Industry Context**: Matches `IndustryContext` field
5. **Organization Type Context**: Matches `OrganizationTypeContext` field

## ReferenceData Table Structure

Options in the `ReferenceData` table should include metadata in `MetadataJson`:

```json
{
  "auditScopeType": "enterprise",
  "frameworkCode": "ISO27001",
  "regulatorCode": "SAMA",
  "industrySector": "financial_services"
}
```

### Example ReferenceData Records

**InScopeProcesses Category:**
```sql
INSERT INTO "ReferenceData" ("Category", "Value", "LabelEn", "LabelAr", "MetadataJson", "IsActive", "SortOrder")
VALUES 
('InScopeProcesses', 'payment_processing', 'Payment Processing', 'معالجة المدفوعات', 
 '{"auditScopeType":"enterprise","frameworkCode":"PCI-DSS","regulatorCode":"SAMA"}', 
 true, 1),
('InScopeProcesses', 'access_management', 'Access Management', 'إدارة الوصول', 
 '{"auditScopeType":"enterprise","frameworkCode":"ISO27001","regulatorCode":"NCA"}', 
 true, 2);
```

**InScopeBusinessUnits Category:**
```sql
INSERT INTO "ReferenceData" ("Category", "Value", "LabelEn", "LabelAr", "IndustryContext", "IsActive", "SortOrder")
VALUES 
('InScopeBusinessUnits', 'Treasury Operations', 'Treasury Operations', 'عمليات الخزينة', 
 'financial_services', true, 1),
('InScopeBusinessUnits', 'IT Security', 'IT Security', 'أمن تكنولوجيا المعلومات', 
 'technology', true, 2);
```

## Filtered Categories in Step D

The following categories are filtered based on Step C:

1. **InScopeProcesses** - Filtered by frameworks and regulators
2. **InScopeBusinessUnits** - Filtered by industry and regulators
3. **InScopeSystems** - Filtered by frameworks and industry
4. **InScopeLocations** - Filtered by operating countries (from Step A)
5. **InScopeLegalEntities** - Filtered by regulators
6. **InScopeEnvironments** - Filtered by audit scope type

## Implementation Details

### Controller: OnboardingWizardController.StepD

```csharp
[HttpGet("StepD/{tenantId:guid}")]
public async Task<IActionResult> StepD(Guid tenantId)
{
    // Parse Step C selections
    var primaryRegulators = ParseRegulators(wizard.PrimaryRegulatorsJson);
    var allFrameworks = ParseFrameworks(wizard);
    
    // Build filter context
    var filterContext = new Dictionary<string, object> { ... };
    
    // Get filtered options from database
    var filteredOptions = await _referenceDataService.GetFilteredOptionsAsync(
        category, filterContext, language);
    
    // Pass to view
    ViewData["DropdownOptions"] = filteredOptions;
}
```

### Service: OnboardingReferenceDataService.GetFilteredOptionsAsync

```csharp
public async Task<List<ReferenceDataOptionDto>> GetFilteredOptionsAsync(
    string category,
    Dictionary<string, object>? filterContext = null,
    string? language = "en")
{
    var query = _context.ReferenceData
        .Where(r => r.Category == category && r.IsActive);
    
    // Apply filters from context
    if (filterContext.ContainsKey("AuditScopeType"))
        query = query.Where(r => r.MetadataJson.Contains(...));
    
    if (filterContext.ContainsKey("FrameworkCodes"))
        query = query.Where(r => frameworkCodes.Any(...));
    
    // ... more filters
    
    return await query.ToListAsync();
}
```

## View Integration

The view receives filtered options in `ViewData["DropdownOptions"]`:

```csharp
var dropdownOptions = ViewData["DropdownOptions"] as Dictionary<string, List<ReferenceDataOptionDto>>;
var processes = dropdownOptions?["InScopeProcesses"] ?? new List<ReferenceDataOptionDto>();
```

Each option includes:
- `Value` - The actual value to save
- `Label` - Display label (English or Arabic)
- `Description` - Help text
- `IsCommon` - Whether it's a common/recommended option
- `Metadata` - Additional metadata for filtering

## Legacy Support

The controller maintains backward compatibility by also populating:
- `ViewData["FilteredProcesses"]` - Simple string list
- `ViewData["FilteredBusinessUnits"]` - Simple string list
- `ViewData["FilteredSystems"]` - Simple string list
- `ViewData["FilteredLocations"]` - Simple string list

These are derived from the filtered options for existing views that haven't been updated yet.

## Logging

Step D logs detailed filtering information:

```
[ONBOARDING_PROGRESS] Step D loaded with filtered dropdowns. 
TenantId={TenantId}, Progress={Progress}%, 
AuditScopeType={AuditScopeType}, Frameworks={Frameworks}, Regulators={Regulators}, 
FilteredProcesses={ProcessCount}, FilteredBusinessUnits={BUCount}, 
FilteredSystems={SystemCount}, FilteredLocations={LocationCount}
```

## Testing

1. **Complete Step C** with:
   - Primary regulators: SAMA, NCA
   - Mandatory frameworks: ISO27001, PCI-DSS
   - Audit scope type: enterprise

2. **Navigate to Step D** and verify:
   - Processes dropdown shows only processes relevant to SAMA, NCA, ISO27001, PCI-DSS
   - Business units show only units relevant to selected regulators
   - Systems show only systems relevant to selected frameworks
   - Locations show only locations from operating countries

3. **Change Step C selections** and verify Step D updates accordingly

## Next Steps

1. ✅ Database-driven filtering implemented
2. ✅ Filter context built from Step C selections
3. ✅ ReferenceDataService filtering methods added
4. ⏳ Seed ReferenceData table with filtered options
5. ⏳ Update Step D views to use filtered dropdowns
6. ⏳ Test filtering with various Step C combinations

---

**Status**: Backend filtering complete. Database seeding and view updates pending.
