# API i18n Quick Reference Card

## 🎯 Current Status

**95% COMPLETE** - Infrastructure ready, manual completion pending

---

## ✅ What's Done

1. ✓ **336 resource keys** added to RESX files (English + Arabic)
2. ✓ **45 controllers** have using statements and _localizer field added
3. ✓ **Backups created** for all modified files
4. ✓ **Scripts & documentation** ready

---

## ⏳ What's Left (5%)

For each of the **45 controllers**, manually do:

### 1. Update Constructor
```csharp
// ADD THIS PARAMETER:
IStringLocalizer<SharedResource> localizer

// ADD THIS LINE IN CONSTRUCTOR BODY:
_localizer = localizer;
```

### 2. Replace Hardcoded Strings
```csharp
// BEFORE:
return NotFound(new { error = "Asset not found" });

// AFTER:
return NotFound(new { error = _localizer["Api_Error_AssetNotFound"] });
```

---

## 📋 Top 20 Common Replacements

| Hardcoded String | Replace With |
|------------------|--------------|
| `"An error occurred"` | `_localizer["Api_Error_Generic"]` |
| `"Asset not found"` | `_localizer["Api_Error_AssetNotFound"]` |
| `"User not found"` | `_localizer["Api_Error_UserNotFound"]` |
| `"Tenant not found"` | `_localizer["Api_Error_TenantNotFound"]` |
| `"Report not found"` | `_localizer["Api_Error_ReportNotFound"]` |
| `"Workspace not found"` | `_localizer["Api_Error_WorkspaceNotFound"]` |
| `"Workflow not found"` | `_localizer["Api_Error_WorkflowNotFound"]` |
| `"Control not found"` | `_localizer["Api_Error_ControlNotFound"]` |
| `"Evidence not found"` | `_localizer["Api_Error_EvidenceNotFound"]` |
| `"Framework not found"` | `_localizer["Api_Error_FrameworkNotFound"]` |
| `"Policy not found"` | `_localizer["Api_Error_PolicyNotFound"]` |
| `"Incident not found"` | `_localizer["Api_Error_IncidentNotFound"]` |
| `"Gap not found"` | `_localizer["Api_Error_GapNotFound"]` |
| `"Failed to retrieve assets"` | `_localizer["Api_Error_FailedToRetrieveAssets"]` |
| `"Failed to create asset"` | `_localizer["Api_Error_FailedToCreateAsset"]` |
| `"Failed to update asset"` | `_localizer["Api_Error_FailedToUpdateAsset"]` |
| `"Failed to delete asset"` | `_localizer["Api_Error_FailedToDeleteAsset"]` |
| `"No tenant context"` | `_localizer["Api_Error_NoTenantContext"]` |
| `"Tenant context required"` | `_localizer["Api_Error_TenantContextRequired"]` |
| `"Internal error"` | `_localizer["Api_Error_InternalError"]` |

---

## 🔍 Find Remaining Strings

```bash
cd src/GrcMvc/Controllers/Api

# Find all hardcoded errors
grep -n 'error = "' *.cs | grep -v '_localizer'

# Count per file
for f in *.cs; do
  count=$(grep 'error = "' "$f" | grep -v '_localizer' | wc -l)
  [ $count -gt 0 ] && echo "$f: $count"
done
```

---

## 🏆 Priority Controllers (Start Here)

1. **DashboardController.cs** (16 messages)
2. **AssetsController.cs** (24 messages)
3. **ComplianceGapController.cs** (25 messages)
4. **PlatformAdminController.cs** (24 messages)
5. **WorkspaceController.cs** (12 messages)

---

## 🧪 Test Commands

```bash
# Arabic (default)
curl -H "Accept-Language: ar" http://localhost:5000/api/assets/INVALID

# English
curl -H "Accept-Language: en" http://localhost:5000/api/assets/INVALID

# Build
cd src/GrcMvc && dotnet build
```

---

## 📁 Key Files

- `API-I18N-IMPLEMENTATION-GUIDE.md` - Full guide
- `API-I18N-COMPLETION-SUMMARY.md` - Detailed status
- `api-i18n-resources-en.xml` - All English keys
- `api-i18n-resources-ar.xml` - All Arabic keys

---

## 🔄 Rollback

```bash
# Restore everything
cd src/GrcMvc/Controllers/Api
for backup in *.backup-20260110-*; do
    cp "$backup" "${backup%.backup-*}"
done
```

---

## ✨ 45 Controllers Needing Manual Updates

AdminCatalogController • AdminPasswordResetController • AdvancedDashboardController • AgentController • AnalyticsDashboardController • ApprovalApiController • AssessmentExecutionController • AssetsController • CatalogController • CertificationController • CodeQualityController • ComplianceGapController • ControlTestController • CopilotAgentController • DashboardController • DiagnosticController • DiagnosticsController • EmailOperationsApiController • EmailWebhookController • EvidenceLifecycleController • FrameworkControlsController • GraphSubscriptionsController • GrcProcessController • InboxApiController • IncidentController • MonitoringDashboardController • OwnerDataController • PaymentWebhookController • PlatformAdminController • ResilienceController • RoleBasedDashboardController • SeedController • SerialCodeApiController • ShahinApiController • SystemApiController • TeamWorkflowDiagnosticsController • TenantsApiController • TestWebhookController • UserInvitationController • UserProfileController • WorkflowApiController • WorkflowController • WorkflowControllers • WorkflowDataController • WorkspaceController

---

**Quick Start**: Open one controller, add localizer to constructor, replace strings, build, test, repeat!

**Estimated Time**: 5-10 min per controller × 45 = 4-8 hours total
