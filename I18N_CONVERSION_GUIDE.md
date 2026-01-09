# Internationalization (i18n) Conversion Guide

## Status: Partial Implementation Complete

### Completed Components
- ✅ Infrastructure setup (localization services, resource files)
- ✅ Language switcher (Arabic/English)
- ✅ Resource files with ~250+ keys (English, Arabic)
- ✅ JavaScript localization helper (`localization.js`)
- ✅ Sample views converted (Dashboard, Home, Risk/Index, Evidence/Index)
- ✅ Reusable ViewComponent for action buttons

### Remaining Work

**Estimated**: ~280 views still need conversion

## Conversion Pattern

### 1. Add View Imports (if not present)
```razor
@using Microsoft.AspNetCore.Mvc.Localization
@inject IHtmlLocalizer<GrcMvc.Resources.SharedResource> L
```

### 2. Convert ViewData Title
```razor
@{
    // Before:
    ViewData["Title"] = "Risk Management";
    
    // After:
    ViewData["Title"] = L["RiskManagement"].Value;
}
```

### 3. Convert Hardcoded Text
```razor
<!-- Before: -->
<h1>مركز القيادة</h1>
<p>نظام الحوكمة والمخاطر والامتثال المتكامل</p>

<!-- After: -->
<h1>@L["Dashboard_CommandCenter"]</h1>
<p>@L["Dashboard_IntegratedGrcSystem"]</p>
```

### 4. Convert Table Headers
```razor
<!-- Before: -->
<th>Risk Name</th>
<th>Status</th>
<th>Actions</th>

<!-- After: -->
<th>@L["RiskName"]</th>
<th>@L["Status"]</th>
<th>@L["Actions"]</th>
```

### 5. Convert Action Buttons
```razor
<!-- Before: -->
<a asp-action="Create" class="btn btn-primary">New Risk</a>
<a asp-action="Edit" asp-route-id="@item.Id" title="Edit">Edit</a>

<!-- After: -->
<a asp-action="Create" class="btn btn-primary">@L["NewRisk"]</a>
<a asp-action="Edit" asp-route-id="@item.Id" title="@L["Edit"]">@L["Edit"]</a>

<!-- Or use ViewComponent: -->
@await Component.InvokeAsync("ActionButtons", new { controller = "Risk", id = item.Id })
```

### 6. Add Missing Resource Keys

When you find hardcoded text that doesn't have a resource key:

1. Add to `src/GrcMvc/Resources/SharedResource.resx` (English):
```xml
<data name="YourKey" xml:space="preserve">
  <value>English Text</value>
</data>
```

2. Add to `src/GrcMvc/Resources/SharedResource.ar.resx` (Arabic):
```xml
<data name="YourKey" xml:space="preserve">
  <value>النص العربي</value>
</data>
```

3. Add to `src/GrcMvc/Resources/SharedResource.en.resx` (English - explicit):
```xml
<data name="YourKey" xml:space="preserve">
  <value>English Text</value>
</data>
```

### 7. JavaScript Localization

For client-side strings:

```javascript
// Use the L() function
const message = L('LoadingData');
const formatted = L('Validation_MinLength', 8); // With parameters

// Check culture
if (isRTL()) {
    // RTL-specific logic
}
```

## Priority Views to Convert

### High Priority (User-Facing)
1. Landing pages (`Views/Landing/*`)
2. Authentication pages (`Views/Account/*`)
3. Main CRUD views (`Views/Risk/*`, `Views/Control/*`, `Views/Policy/*`, `Views/Audit/*`)
4. Dashboard views (`Views/Dashboard/*`, `Views/MonitoringDashboard/*`)

### Medium Priority
1. Admin views (`Views/Admin/*`, `Views/PlatformAdmin/*`, `Views/TenantAdmin/*`)
2. Workflow views (`Views/Workflow/*`, `Views/WorkflowUI/*`)
3. Reports (`Views/Reports/*`)

### Low Priority
1. Help/Knowledge Base (`Views/Help/*`, `Views/KnowledgeBase/*`)
2. Email templates (`Views/EmailTemplates/*`)

## Testing Checklist

- [ ] Language switcher works on all pages
- [ ] All text switches between Arabic and English
- [ ] RTL layout works correctly for Arabic
- [ ] JavaScript strings are localized
- [ ] Date/number formatting respects culture
- [ ] No hardcoded strings remain in converted views

## Common Resource Keys Available

Most common keys are already in the resource files:
- Actions: `Create`, `Edit`, `Delete`, `View`, `Save`, `Cancel`, `Submit`, `Approve`
- Status: `Status_Active`, `Status_Pending`, `Status_Completed`, etc.
- Labels: `Name`, `Description`, `Status`, `Owner`, `Date`, `Type`, `Category`
- Navigation: `Nav_Home`, `Nav_Dashboard`, `Nav_RiskManagement`, etc.
- Messages: `Success_Saved`, `Error_NotFound`, `Validation_Required`, etc.

## Notes

- Always use `@L["Key"]` for HTML content (supports HTML in translations)
- Use `@L["Key"].Value` for attributes or non-HTML contexts
- Check existing resource files before adding new keys (avoid duplicates)
- Maintain consistent naming: `Module_Item` or `Category_Item` pattern
