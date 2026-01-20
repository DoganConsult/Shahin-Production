# Progressive Filtering Implementation - All Steps

## Overview

**Each step's dropdown options are now filtered based on ALL previous step selections.** This ensures that:
- Step B filters based on Step A
- Step C filters based on Step A + Step B
- Step D filters based on Step A + Step B + Step C
- Step E filters based on Step A + B + C + D
- And so on...

## Implementation Pattern

### Helper Methods (Already Added)

1. **`BuildCumulativeFilterContext(OnboardingWizard wizard, int currentStep)`**
   - Builds filter context from all previous steps
   - Returns `Dictionary<string, object>` with all relevant filter criteria
   - Automatically includes data from steps 1 through (currentStep - 1)

2. **`LoadFilteredOptionsForStepAsync(OnboardingWizard wizard, int stepNumber, List<string> categories, string? language)`**
   - Loads filtered dropdown options for a step
   - Uses cumulative filter context
   - Returns dictionary of category → filtered options

### Filter Context Accumulation

The filter context accumulates data as follows:

| Step | Available Context |
|------|-------------------|
| Step A | None (first step) |
| Step B | Step A: OrganizationType, IndustrySector, CountryOfIncorporation, OperatingCountries |
| Step C | Step A + Step B: + PrimaryDriver, DesiredMaturity |
| Step D | Step A + B + C: + RegulatorCodes, FrameworkCodes, AuditScopeType |
| Step E | Step A + B + C + D: + InScopeSystems, InScopeProcesses, InScopeBusinessUnits |
| Step F | Step A + B + C + D + E: + DataTypesProcessed, HasPaymentCardData, HasInternetFacingSystems |
| Step G+ | All previous steps |

## How to Update Each Step

### Pattern for Each Step GET Action

```csharp
[HttpGet("StepX/{tenantId:guid}")]
public async Task<IActionResult> StepX(Guid tenantId)
{
    var wizard = await GetOrCreateWizardAsync(tenantId);
    var dto = MapToStepXDto(wizard);
    ViewData["WizardSummary"] = BuildWizardSummary(wizard);
    
    // Load filtered dropdown options based on previous steps
    if (_referenceDataService != null)
    {
        var categories = new List<string> 
        { 
            "Category1",
            "Category2",
            // ... all categories for this step
        };
        var filteredOptions = await LoadFilteredOptionsForStepAsync(wizard, stepNumber, categories);
        ViewData["DropdownOptions"] = filteredOptions;
    }
    
    return View("StepX", dto);
}
```

## Step-by-Step Implementation

### Step B (Assurance Objective)
**Filters based on:** Step A
**Categories:**
- `PrimaryDriver`
- `DesiredMaturity`
- `CurrentPainPoints`
- `ReportingAudience`

### Step C (Regulatory Applicability)
**Filters based on:** Step A + Step B
**Categories:**
- `PrimaryRegulators`
- `SecondaryRegulators`
- `MandatoryFrameworks`
- `OptionalFrameworks`
- `InternalPolicies`
- `CertificationsHeld`
- `AuditScopeType`

### Step D (Scope Definition) - ✅ Already Implemented
**Filters based on:** Step A + B + C
**Categories:**
- `InScopeProcesses`
- `InScopeBusinessUnits`
- `InScopeSystems`
- `InScopeLocations`
- `InScopeLegalEntities`
- `InScopeEnvironments`

### Step E (Data and Risk Profile)
**Filters based on:** Step A + B + C + D
**Categories:**
- `DataTypesProcessed`
- `PaymentCardDataLocations`
- `CrossBorderTransferCountries`
- `CustomerVolumeTier`
- `TransactionVolumeTier`
- `InternetFacingSystems`
- `ThirdPartyDataProcessors`

### Step F (Technology Landscape)
**Filters based on:** Step A + B + C + D + E
**Categories:**
- `IdentityProvider`
- `ItsmPlatform`
- `EvidenceRepository`
- `SiemPlatform`
- `VulnerabilityManagementTool`
- `EdrPlatform`
- `CloudProviders`
- `ErpSystem`
- `CmdbSource`
- `CiCdTooling`
- `BackupDrTooling`

### Step G (Control Ownership Model)
**Filters based on:** Step A + B + C + D + E + F
**Categories:**
- `ControlOwnershipApproach`
- `DefaultControlOwnerTeam`
- `ExceptionApproverRole`
- `RegulatoryInterpretationApproverRole`
- `ControlEffectivenessSignoffRole`
- `InternalAuditStakeholder`
- `RiskCommitteeCadence`
- `RiskCommitteeAttendees`

### Step H (Teams, Roles, and Access)
**Filters based on:** Step A + B + C + D + E + F + G
**Categories:**
- `OrgAdmins`
- `TeamRoles`
- `AccessControlModel`
- `SsoConfiguration`

### Step I (Workflow Configuration)
**Filters based on:** Step A + B + C + D + E + F + G + H
**Categories:**
- `WorkflowTypes`
- `ApprovalWorkflows`
- `NotificationRules`

### Step J (Integration Preferences)
**Filters based on:** Step A + B + C + D + E + F + G + H + I
**Categories:**
- `IntegrationTypes`
- `ApiConnections`
- `DataSyncPreferences`

### Step K (Assessment Preferences)
**Filters based on:** Step A + B + C + D + E + F + G + H + I + J
**Categories:**
- `AssessmentTypes`
- `EvidenceRequirements`
- `ReviewCycles`

### Step L (Final Review and Provisioning)
**Filters based on:** All previous steps
**Categories:**
- `ProvisioningOptions`
- `InitialSetupTasks`

## Database Filtering Logic

The `ReferenceDataService.GetFilteredOptionsAsync` method filters options by:

1. **Audit Scope Type**: Matches `MetadataJson` containing `"auditScopeType":"{value}"`
2. **Framework Codes**: Matches `MetadataJson` containing `"frameworkCode":"{code}"`
3. **Regulator Codes**: Matches `MetadataJson` containing `"regulatorCode":"{code}"`
4. **Industry Context**: Matches `IndustryContext` field
5. **Organization Type Context**: Matches `OrganizationTypeContext` field
6. **In-Scope Systems**: Matches `MetadataJson` containing `"inScopeSystem":"{system}"`
7. **In-Scope Processes**: Matches `MetadataJson` containing `"inScopeProcess":"{process}"`
8. **Data Types**: Matches `MetadataJson` containing `"dataType":"{type}"`
9. **Cloud Providers**: Matches `MetadataJson` containing `"cloudProvider":"{provider}"`

## Example: Step E Filtering

Step E (Data and Risk Profile) filters based on:
- **Step A**: Industry sector → shows relevant data types for that industry
- **Step C**: Selected frameworks → shows data types required by those frameworks
- **Step D**: In-scope systems → shows data types processed by those systems

If Step A = "financial_services", Step C = "PCI-DSS", Step D = "Payment Gateway":
- Step E `DataTypesProcessed` dropdown shows: PCI, PII, Financial Data (filtered)
- Step E `PaymentCardDataLocations` dropdown shows: Payment Gateway system (from Step D)

## Benefits

1. **Context-Aware**: Each step only shows relevant options
2. **Progressive Refinement**: Options become more specific as user progresses
3. **Reduced Errors**: Users can't select incompatible options
4. **Better UX**: Fewer irrelevant options to scroll through
5. **Database-Driven**: All filtering logic uses database metadata

## Testing

1. Complete Step A with specific industry/organization type
2. Navigate to Step B → verify options filtered by Step A
3. Complete Step B
4. Navigate to Step C → verify options filtered by Step A + B
5. Continue through all steps, verifying cumulative filtering

## Status

- ✅ Helper methods implemented
- ✅ Step D filtering implemented
- ⏳ Steps B, C, E, F, G, H, I, J, K, L need filtering added
- ⏳ Database seeding with filtered options needed

---

**Next Steps**: Add `LoadFilteredOptionsForStepAsync` calls to each step's GET action.
