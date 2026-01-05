# RBAC Integration Guide - Complete Implementation ✅

## Overview

The GRC system has a **complete Role-Based Access Control (RBAC) system** with predefined role profiles, permissions, and view authorities. This guide shows how everything works together.

## System Architecture

```
User (ApplicationUser)
    ↓
    ├──→ RoleProfile (Organizational Role - 15 predefined)
    │       └──→ Maps to Identity Role
    │
    └──→ Identity Roles (via UserManager)
            ├──→ Permissions (60+ - What they can DO)
            └──→ Features (19 - What they can SEE)
```

## Complete Component List

### 1. Role Profiles (15 Predefined)

**Executive Layer:**
- CRO (Chief Risk Officer)
- CCO (Chief Compliance Officer)  
- ED (Executive Director)

**Management Layer:**
- RM (Risk Manager)
- CM (Compliance Manager)
- AM (Audit Manager)
- SM (Security Manager)
- LM (Legal Manager)

**Operational Layer:**
- CO (Compliance Officer)
- RA (Risk Analyst)
- PO (Privacy Officer)
- QAM (Quality Assurance Manager)
- PRO (Process Owner)

**Support Layer:**
- DS (Documentation Specialist)
- RPA (Reporting Analyst)

### 2. Permissions (60+)

**Pattern:** `Grc.{Module}.{Action}`

**All Modules Covered:**
- Home, Dashboard
- Subscriptions (View, Manage)
- Admin (Access, Users, Roles, Tenants)
- Frameworks (View, Create, Update, Delete, Import)
- Regulators (View, Manage)
- Assessments (View, Create, Update, Submit, Approve)
- Control Assessments (View, Manage)
- Evidence (View, Upload, Update, Delete, Approve)
- Risks (View, Manage, Accept)
- Audits (View, Manage, Close)
- Action Plans (View, Manage, Assign, Close)
- Policies (View, Manage, Approve, Publish)
- Compliance Calendar (View, Manage)
- Workflow (View, Manage)
- Notifications (View, Manage)
- Vendors (View, Manage, Assess)
- Reports (View, Export)
- Integrations (View, Manage)

### 3. Features (19 - View Authority)

- Home, Dashboard, Subscriptions, Admin
- Frameworks, Regulators, Assessments, ControlAssessments, Evidence
- Risks, Audits, ActionPlans, Policies, ComplianceCalendar
- Workflow, Notifications, Vendors, Reports, Integrations

### 4. Identity Roles (12 Technical Roles)

- SuperAdmin, TenantAdmin, ComplianceManager, RiskManager
- Auditor, EvidenceOfficer, VendorManager, Viewer
- ComplianceOfficer, RiskAnalyst, PolicyManager, WorkflowManager

## Automatic Seeding Flow

On application startup, the system seeds in this order:

1. **Role Profiles** → 15 organizational roles
2. **Permissions** → 60+ GRC permissions
3. **Features** → 19 viewable features
4. **Features → Permissions** → Links features to required permissions
5. **Identity Roles** → 12 technical roles
6. **Identity Roles → Permissions** → Maps roles to permissions
7. **Identity Roles → Features** → Maps roles to features (view authority)
8. **Role Profiles → Identity Roles** → Maps organizational roles to technical roles
9. **Role Profiles → Permissions** → Assigns permissions based on layer/department
10. **Role Profiles → Features** → Assigns features based on layer/department

## How to Use in Your Application

### Step 1: Replace Navigation Bar

Update `_Layout.cshtml`:

```html
<!-- Replace this: -->
@await Html.PartialAsync("_NavBar")

<!-- With this: -->
<component type="typeof(GrcMvc.Components.Shared.NavBarRbac)" render-mode="ServerPrerendered" />
```

### Step 2: Add Permission Checks to Controllers

```csharp
[Authorize(Policy = "Grc.Assessments.Create")]
public async Task<IActionResult> CreateAssessment(CreateAssessmentDto dto)
{
    // Only users with Grc.Assessments.Create permission can access
}
```

### Step 3: Add Permission Checks in Services

```csharp
public class AssessmentService
{
    private readonly IMenuService _menuService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<AssessmentDto> CreateAsync(CreateAssessmentDto input)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new UnauthorizedAccessException();

        // Check permission
        var hasPermission = await _menuService.HasPermissionAsync(userId, "Grc.Assessments.Create");
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to create assessments.");
        }

        // Proceed with creation...
    }
}
```

### Step 4: Add Feature Checks in Blazor Pages

```razor
@page "/assessments"
@using GrcMvc.Services.Interfaces
@inject IMenuService MenuService
@inject IHttpContextAccessor HttpContextAccessor

@if (!hasAccess)
{
    <div class="alert alert-warning">
        You do not have access to this page.
    </div>
}
else
{
    <!-- Page content -->
}

@code {
    private bool hasAccess = false;

    protected override async Task OnInitializedAsync()
    {
        var userId = HttpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            hasAccess = await MenuService.HasFeatureAccessAsync(userId, "Assessments");
        }
    }
}
```

### Step 5: Get User Menu Items Dynamically

```razor
@inject IMenuService MenuService

<ul class="nav">
    @foreach (var item in menuItems)
    {
        <li class="nav-item">
            <a class="nav-link" href="@item.Url">
                <i class="@item.Icon"></i> @item.Name
            </a>
        </li>
    }
</ul>

@code {
    private List<MenuItemDto> menuItems = new();

    protected override async Task OnInitializedAsync()
    {
        var userId = HttpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            menuItems = await MenuService.GetUserMenuItemsAsync(userId);
        }
    }
}
```

## Permission Assignment Examples

### Example 1: Compliance Manager Role

**Role Profile:** Compliance Manager (CM)
**Identity Role:** ComplianceManager
**Permissions:**
- Grc.Frameworks.View, Grc.Frameworks.Create, Grc.Frameworks.Update
- Grc.Regulators.View, Grc.Regulators.Manage
- Grc.Assessments.View, Grc.Assessments.Create, Grc.Assessments.Update, Grc.Assessments.Submit, Grc.Assessments.Approve
- Grc.ControlAssessments.View, Grc.ControlAssessments.Manage
- Grc.Evidence.View, Grc.Evidence.Upload, Grc.Evidence.Update, Grc.Evidence.Approve
- Grc.Policies.View, Grc.Policies.Manage, Grc.Policies.Approve
- Grc.ComplianceCalendar.View, Grc.ComplianceCalendar.Manage
- Grc.Reports.View, Grc.Reports.Export

**Features:**
- Frameworks, Regulators, Assessments, ControlAssessments, Evidence, Policies, ComplianceCalendar, Reports

### Example 2: Risk Analyst Role

**Role Profile:** Risk Analyst (RA)
**Identity Role:** RiskAnalyst
**Permissions:**
- Grc.Risks.View, Grc.Risks.Manage
- Grc.Reports.View, Grc.Reports.Export

**Features:**
- Risks, Reports

### Example 3: Viewer Role

**Role Profile:** Documentation Specialist (DS) or Reporting Analyst (RPA)
**Identity Role:** Viewer
**Permissions:**
- Grc.Reports.View, Grc.Reports.Export
- Grc.Assessments.View
- Grc.Evidence.View
- Grc.Risks.View
- Grc.Policies.View

**Features:**
- Reports, Assessments, Evidence, Risks, Policies (read-only)

## Testing the System

### Test 1: Verify Seeding

After application startup, check logs for:
```
✅ Seeded 60+ permissions
✅ Seeded 19 features
✅ Seeded 12 identity roles
✅ Mapped 200+ role-permission relationships
✅ Mapped 100+ role-feature relationships
✅ Mapped 15 RoleProfiles to RBAC system
```

### Test 2: Verify Menu Visibility

1. Login as different users with different roles
2. Check that menu items match their assigned features
3. Verify that users cannot see features they don't have access to

### Test 3: Verify Permission Enforcement

1. Try to access a controller action without the required permission
2. Should receive 403 Forbidden
3. Try to access with the correct permission
4. Should succeed

## Files Reference

### Core Files:
- `src/GrcMvc/Data/Seeds/RbacSeeds.cs` - Main RBAC seeding
- `src/GrcMvc/Data/Seeds/RoleProfileRbacMapper.cs` - RoleProfile → RBAC mapping
- `src/GrcMvc/Services/Interfaces/IMenuService.cs` - Menu service interface
- `src/GrcMvc/Services/Implementations/MenuService.cs` - Menu builder implementation
- `src/GrcMvc/Components/Shared/NavBarRbac.razor` - RBAC-aware navigation

### Integration Points:
- `src/GrcMvc/Data/ApplicationInitializer.cs` - Calls RBAC seeding
- `src/GrcMvc/Program.cs` - Registers MenuService
- `src/GrcMvc/Models/Entities/ApplicationUser.cs` - Has RoleProfileId property

## Summary

✅ **15 Role Profiles** predefined with organizational context  
✅ **60+ Permissions** covering all GRC modules  
✅ **19 Features** for view authority  
✅ **12 Identity Roles** for technical access control  
✅ **Complete automatic mapping** between all components  
✅ **RBAC-aware navigation** that shows only accessible features  
✅ **Permission checking service** ready to use  
✅ **Production-ready** and automatically seeds on startup  

**The system is complete and ready for production use.**
