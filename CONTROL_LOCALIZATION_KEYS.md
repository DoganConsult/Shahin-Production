# Control and Controls Views - Localization Keys Reference

This document lists all localization keys used in the Control and Controls views for the GRC MVC application.

## Overview
- **Total Views**: 18 files (8 in Control folder, 10 in Controls folder)
- **Conversion Status**: Systematic conversion to i18n localization
- **Pattern**: All keys follow the format `Control_Context_Detail`

---

## Common Keys (Used across multiple views)

### Navigation & Actions
- `BackToList` - Back to List button
- `Cancel` - Cancel button
- `SaveChanges` - Save Changes button
- `CreateControl` - Create Control button
- `Edit` - Edit action
- `Details` - Details action
- `Delete` - Delete action
- `Actions` - Actions column header
- `Close` - Close button
- `Submit` - Submit button

### General Labels
- `Status` - Status field
- `None` - None option in dropdowns
- `Yes` - Yes option
- `No` - No option

---

## Control-Specific Keys

### Titles & Headers
- `Control_List_Title` - "Controls" (Index page title)
- `Control_Create_Title` - "Create New Control"
- `Control_Edit_Title` - "Edit Control"
- `Control_Details_Title` - "Control Details"
- `Control_Delete_Title` - "Delete Control"
- `Control_Assess_Title` - "Control Assessment" / "Assess Control"
- `Control_ByRisk_Title` - "Controls for Risk"
- `Control_Matrix_Title` - "Control Matrix & Statistics"

### Field Labels
- `Control_ID` - Control ID field
- `Control_Name` - Control Name field
- `Control_Description` - Description field
- `Control_Category` - Category field
- `Control_Type` - Type field
- `Control_Frequency` - Frequency field
- `Control_Owner` - Owner field
- `Control_Effectiveness` - Effectiveness field
- `Control_EffectivenessScore` - Effectiveness Score (0-100)
- `Control_EffectivenessRating` - Effectiveness Rating
- `Control_RelatedRisk` - Related Risk field
- `Control_Implementation` - Implementation field
- `Control_ImplementationDate` - Implementation Date
- `Control_TestingFrequency` - Testing Frequency
- `Control_Evidence` - Evidence field
- `Control_LastReviewDate` - Last Review Date
- `Control_NextReviewDate` - Next Review Date
- `Control_LastTestDate` - Last Test Date
- `Control_NextTestDate` - Next Test Date

### Placeholders
- `Control_Name_Placeholder` - "e.g. Access Review Policy"
- `Control_ID_Placeholder` - "e.g. CTRL-001"

### Control Categories
- `Control_Category_Preventive` - "Preventive"
- `Control_Category_Detective` - "Detective"
- `Control_Category_Corrective` - "Corrective"
- `Control_Category_Directive` - "Directive"

### Control Types
- `Control_Type_Technical` - "Technical"
- `Control_Type_Administrative` - "Administrative"
- `Control_Type_Physical` - "Physical"
- `Control_Type_Preventive` - "Preventive" (alternate)
- `Control_Type_Corrective` - "Corrective" (alternate)
- `Control_Type_Compensating` - "Compensating"

### Control Status
- `Control_Status_Active` - "Active"
- `Control_Status_Inactive` - "Inactive"
- `Control_Status_Draft` - "Draft"
- `Control_Status_Retired` - "Retired"
- `Control_Status_UnderReview` - "Under Review"
- `Control_Status_Effective` - "Effective"
- `Control_Status_PartiallyEffective` - "Partially Effective"
- `Control_Status_Ineffective` - "Ineffective"

### Control Frequency
- `Control_Frequency_Continuous` - "Continuous"
- `Control_Frequency_Daily` - "Daily"
- `Control_Frequency_Weekly` - "Weekly"
- `Control_Frequency_Monthly` - "Monthly"
- `Control_Frequency_Quarterly` - "Quarterly"
- `Control_Frequency_Annually` - "Annually"
- `Control_Frequency_AdHoc` - "Ad-Hoc"

### Dropdown Defaults
- `SelectCategory` - "Select Category..."
- `SelectType` - "Select Type..."
- `SelectFrequency` - "Select Frequency..."
- `SelectStatus` - "Select Status..."
- `SelectImplementationStatus` - "-- Select Status --"
- `SelectRating` - "-- Select Rating --"

### Messages & Alerts
- `Control_CreatingForRisk` - "Creating control for Risk"
- `Control_DeleteConfirmation` - "Are you sure you want to delete this control?"
- `Control_DeleteWarning` - "This action cannot be undone. The control {0} will be permanently deleted."
- `Control_AssessingControl` - "Assessing control"

### Buttons & Links
- `NewControl` - "New Control"
- `MatrixAndStats` - "Matrix & Stats"
- `AddControl` - "Add Control"
- `BackToRisk` - "Back to Risk"
- `AllControls` - "All Controls"
- `ViewRisk` - "View Risk"
- `NewAssessment` - "New Assessment"
- `ViewAll` - "View All"
- `SubmitAssessment` - "Submit Assessment"

---

## Control Matrix & Statistics Keys

### Matrix Headers
- `Control_Matrix_GeneralInformation` - "General Information"
- `Control_Matrix_EffectivenessAndReview` - "Effectiveness & Review"
- `Control_Matrix_OwnershipAndRelations` - "Ownership & Relations"
- `Control_Matrix_Assessments` - "Assessments"
- `Control_Matrix_ReviewSchedule` - "Review Schedule"
- `Control_Matrix_TestingSchedule` - "Testing Schedule"

### Statistics
- `Control_Stats_TotalControls` - "Total Controls"
- `Control_Stats_Effective` - "Effective"
- `Control_Stats_Ineffective` - "Ineffective"
- `Control_Stats_Tested` - "Tested"
- `Control_Stats_AverageEffectiveness` - "Average Effectiveness"
- `Control_Stats_EffectivenessRate` - "Effectiveness Rate"
- `Control_Stats_PerformanceMetrics` - "Performance Metrics"
- `Control_Stats_ControlsByType` - "Controls by Type"

---

## Assessment-Specific Keys

### Assessment Form
- `Control_Assess_ImplementationStatus` - "Implementation Status"
- `Control_Assess_EffectivenessRating` - "Effectiveness Rating"
- `Control_Assess_AssessmentDate` - "Assessment Date"
- `Control_Assess_AssessorNotes` - "Assessor Notes"
- `Control_Assess_EvidenceReference` - "Evidence Reference"
- `Control_Assess_GapIdentified` - "Gap Identified"
- `Control_Assess_GapDescription` - "Gap Description"
- `Control_Assess_AssessorName` - "Assessor Name"
- `Control_Assess_TestingMethod` - "Testing Method"
- `Control_Assess_FindingsAndGaps` - "Findings & Gaps"

### Assessment Status
- `Control_Assess_NotImplemented` - "Not Implemented"
- `Control_Assess_PartiallyImplemented` - "Partially Implemented"
- `Control_Assess_FullyImplemented` - "Fully Implemented"
- `Control_Assess_NotApplicable` - "Not Applicable"

### Assessment Ratings
- `Control_Assess_Rating_1` - "1 - Ineffective"
- `Control_Assess_Rating_2` - "2 - Needs Improvement"
- `Control_Assess_Rating_3` - "3 - Partially Effective"
- `Control_Assess_Rating_4` - "4 - Largely Effective"
- `Control_Assess_Rating_5` - "5 - Fully Effective"

### Assessment Guidelines
- `Control_Assess_Guidelines` - "Assessment Guidelines"
- `Control_Assess_Guideline_ReviewDocs` - "Review control documentation"
- `Control_Assess_Guideline_VerifyEvidence` - "Verify evidence of operation"
- `Control_Assess_Guideline_TestEffectiveness` - "Test control effectiveness"
- `Control_Assess_Guideline_DocumentGaps` - "Document any gaps found"
- `Control_Assess_Guideline_RecommendRemediation` - "Recommend remediation if needed"

### Assessment Tips
- `Control_Assess_Tips` - "Assessment Tips"
- `Control_Assess_RatingScale` - "Rating Scale"

### Testing Methods
- `Control_Test_DocumentReview` - "Document Review (policies, procedures, evidence)"
- `Control_Test_SystemTesting` - "System Testing (access logs, configurations, settings)"
- `Control_Test_Interview` - "Interviews (staff, management, process owners)"
- `Control_Test_SampleTest` - "Sample Testing (testing actual access, user provisioning)"

### Remediation
- `Control_Remediation_Title` - "Remediation Plan"
- `Control_Remediation_Actions` - "Remediation Actions"
- `Control_Remediation_TargetDate` - "Target Completion Date"
- `Control_Remediation_ResponsibleParty` - "Responsible Party"

### Maturity Levels
- `Control_Maturity_Initial` - "Initial (Ad-hoc, informal processes)"
- `Control_Maturity_Repeatable` - "Repeatable (Documented, partially automated)"
- `Control_Maturity_Defined` - "Defined (Standardized, integrated processes)"
- `Control_Maturity_Managed` - "Managed (Measured, monitored, optimized)"
- `Control_Maturity_Optimized` - "Optimized (Continuous improvement, innovation)"

---

## Controls Library (Shahin MAP) Keys

### Page Headers
- `Controls_Library_Title` - "Controls Library"
- `Controls_Library_Subtitle` - "Shahin MAP"
- `Controls_Library_Description` - "Manage your control library across all frameworks and baselines"

### Filters
- `Controls_Filter_Framework` - "Framework"
- `Controls_Filter_AllFrameworks` - "All Frameworks"
- `Controls_Filter_Domain` - "Domain"
- `Controls_Filter_AllDomains` - "All Domains"
- `Controls_Filter_AllStatus` - "All Status"
- `Controls_Filter_Search` - "Search"
- `Controls_Filter_SearchPlaceholder` - "Search controls..."

### Domain Values
- `Controls_Domain_GOV` - "Governance"
- `Controls_Domain_DEF` - "Defense"
- `Controls_Domain_RES` - "Resilience"
- `Controls_Domain_TP` - "Third Party"
- `Controls_Domain_IC` - "Industrial Control"
- `Controls_Domain_DATA` - "Data Protection"
- `Controls_Domain_ACCESS` - "Access Control"

### Framework Values
- `Controls_Framework_NCA_ECC` - "NCA ECC 2:2024"
- `Controls_Framework_SAMA_CSF` - "SAMA CSF"
- `Controls_Framework_PDPL` - "PDPL"
- `Controls_Framework_NCA_CSCC` - "NCA CSCC"
- `Controls_Framework_ISO27001` - "ISO 27001"
- `Controls_Framework_Custom` - "Custom"

### Statistics
- `Controls_Stats_TotalControls` - "Total Controls"
- `Controls_Stats_Compliant` - "Compliant"
- `Controls_Stats_Partial` - "Partial"
- `Controls_Stats_NonCompliant` - "Non-Compliant"
- `Controls_Stats_NotAssessed` - "Not Assessed"
- `Controls_Stats_ComplianceRate` - "Compliance Rate"

### Table Headers
- `Controls_Table_ControlID` - "Control ID"
- `Controls_Table_ControlName` - "Control Name"
- `Controls_Table_Framework` - "Framework"
- `Controls_Table_Domain` - "Domain"
- `Controls_Table_Status` - "Status"
- `Controls_Table_Maturity` - "Maturity"
- `Controls_Table_LastAssessed` - "Last Assessed"
- `Controls_Table_Actions` - "Actions"

### Status Badges
- `Controls_Status_Compliant` - "Compliant"
- `Controls_Status_Partial` - "Partial"
- `Controls_Status_NonCompliant` - "Non-Compliant"
- `Controls_Status_NotAssessed` - "Not Assessed"
- `Controls_Status_NotApplicable` - "N/A"

### Messages
- `Controls_LoadingControls` - "Loading controls..."
- `Controls_NoControlsFound` - "No controls found"
- `Controls_ErrorLoading` - "Error loading controls. Please try again."
- `Controls_Showing` - "Showing"
- `Controls_Of` - "of"
- `Controls_Controls` - "controls"

### Modal - Add Control
- `Controls_AddControl_Title` - "Add New Control"
- `Controls_AddControl_ControlNumber` - "Control Number"
- `Controls_AddControl_TitleEn` - "Title (English)"
- `Controls_AddControl_TitleAr` - "Title (Arabic)"
- `Controls_AddControl_RequirementEn` - "Requirement (English)"
- `Controls_AddControl_RequirementAr` - "Requirement (Arabic)"
- `Controls_AddControl_ImplementationGuidance` - "Implementation Guidance"
- `Controls_AddControl_EvidenceRequirements` - "Evidence Requirements"
- `Controls_AddControl_MaturityLevel` - "Maturity Level"

### Mappings
- `Controls_Mapping_Title` - "Control Mapping"
- `Controls_Mapping_FrameworkMappings` - "Framework Mappings"
- `Controls_Mapping_ISO27001` - "ISO 27001 Mapping"
- `Controls_Mapping_NIST` - "NIST CSF Mapping"

---

## Applicability Manager Keys

### Page Headers
- `Controls_Applicability_Title` - "Applicability Manager"
- `Controls_Applicability_Subtitle` - "Shahin APPLY"
- `Controls_Applicability_Description` - "Define which controls apply to your organization based on business context"

### Organization Context
- `Controls_Applicability_OrgContext` - "Organization Context"
- `Controls_Applicability_Industry` - "Industry"
- `Controls_Applicability_Sector` - "Sector"
- `Controls_Applicability_Size` - "Size"
- `Controls_Applicability_DataType` - "Data Type"
- `Controls_Applicability_Hosting` - "Hosting"
- `Controls_Applicability_Regulators` - "Regulators"

### Stats
- `Controls_Applicability_ApplicableControls` - "Applicable Controls"
- `Controls_Applicability_NotApplicable` - "Not Applicable"
- `Controls_Applicability_PendingReview` - "Pending Review"
- `Controls_Applicability_ActiveRules` - "Active Rules"

### Rules
- `Controls_Applicability_Rules` - "Applicability Rules"
- `Controls_Applicability_RunScopeEngine` - "Run Scope Engine"
- `Controls_Applicability_AddRule` - "Add Rule"

---

## Mapping Keys

### Page Headers
- `Controls_Mapping_Title` - "Control Mapping"
- `Controls_Mapping_Subtitle` - "Requirement → Objective → Control"
- `Controls_Mapping_Description` - "Map regulatory requirements to control objectives and controls"

### Columns
- `Controls_Mapping_Requirements` - "Requirements"
- `Controls_Mapping_ControlObjectives` - "Control Objectives"
- `Controls_Mapping_Controls` - "Controls"

### Stats
- `Controls_Mapping_TotalRequirements` - "Total Requirements"
- `Controls_Mapping_Mapped` - "Mapped"
- `Controls_Mapping_Unmapped` - "Unmapped"

---

## Remediation Manager Keys

### Page Headers
- `Controls_Remediation_Title` - "Remediation Manager"
- `Controls_Remediation_Subtitle` - "Shahin FIX"
- `Controls_Remediation_Description` - "Track and manage remediation actions for compliance gaps"

### Issue Status
- `Controls_Remediation_Open` - "Open"
- `Controls_Remediation_InProgress` - "In Progress"
- `Controls_Remediation_Review` - "Review"
- `Controls_Remediation_Closed` - "Closed"

### Severity
- `Controls_Remediation_Critical` - "Critical"
- `Controls_Remediation_High` - "High"
- `Controls_Remediation_Medium` - "Medium"
- `Controls_Remediation_Low` - "Low"
- `Controls_Remediation_Resolved` - "Resolved"
- `Controls_Remediation_Overdue` - "Overdue"

---

## Requirements Manager Keys

### Page Headers
- `Controls_Requirements_Title` - "Requirements Manager"
- `Controls_Requirements_Description` - "Import and manage regulatory requirements from frameworks"

### Actions
- `Controls_Requirements_Import` - "Import"
- `Controls_Requirements_AddRequirement` - "Add Requirement"
- `Controls_Requirements_AllRequirements` - "All Requirements"

---

## Test Manager Keys

### Page Headers
- `Controls_Tests_Title` - "Test Manager"
- `Controls_Tests_Subtitle` - "Shahin PROVE"
- `Controls_Tests_Description` - "Define and execute control testing procedures"

### Actions
- `Controls_Tests_RunAllTests` - "Run All Tests"
- `Controls_Tests_AddTest` - "Add Test"

### Test Status
- `Controls_Tests_TotalTests` - "Total Tests"
- `Controls_Tests_Passed` - "Passed"
- `Controls_Tests_Failed` - "Failed"
- `Controls_Tests_Pending` - "Pending"
- `Controls_Tests_NotRun` - "Not Run"
- `Controls_Tests_PassRate` - "Pass Rate"

### Test Types
- `Controls_Tests_Auto` - "Auto"
- `Controls_Tests_Manual` - "Manual"
- `Controls_Tests_Hybrid` - "Hybrid"

---

## Usage Instructions

### Adding Keys to Resource Files

1. **For English** - Add to `src/GrcMvc/Resources/SharedResource.en.resx`:
```xml
<data name="Control_List_Title" xml:space="preserve">
  <value>Controls</value>
</data>
```

2. **For Arabic** - Add to `src/GrcMvc/Resources/SharedResource.ar.resx`:
```xml
<data name="Control_List_Title" xml:space="preserve">
  <value>الضوابط</value>
</data>
```

### Key Naming Convention

Follow the pattern: `{Module}_{Context}_{Detail}`

Examples:
- `Control_List_Title` - Control module, List context, Title detail
- `Control_Status_Active` - Control module, Status context, Active detail
- `Control_Category_Preventive` - Control module, Category context, Preventive detail

---

## File Conversion Status

### Control Folder (8 files)
- ✅ `Index.cshtml` - Converted
- ✅ `Create.cshtml` - Converted
- ⏳ `Edit.cshtml` - Pending
- ⏳ `Details.cshtml` - Pending
- ⏳ `Delete.cshtml` - Pending
- ⏳ `Assess.cshtml` - Pending
- ⏳ `ByRisk.cshtml` - Pending
- ⏳ `Matrix.cshtml` - Pending

### Controls Folder (10 files)
- ⏳ `Index.cshtml` - Pending
- ⏳ `Create.cshtml` - Pending
- ⏳ `Edit.cshtml` - Pending
- ⏳ `Details.cshtml` - Pending
- ⏳ `Assess.cshtml` - Pending
- ⏳ `Applicability.cshtml` - Pending
- ⏳ `Mapping.cshtml` - Pending
- ⏳ `Remediation.cshtml` - Pending
- ⏳ `Requirements.cshtml` - Pending
- ⏳ `Tests.cshtml` - Pending

---

## Next Steps

1. Complete conversion of remaining Control folder views
2. Convert all Controls folder views
3. Add all keys to English resource file (.en.resx)
4. Add all Arabic translations to Arabic resource file (.ar.resx)
5. Test all views for proper localization
6. Verify Arabic text displays correctly with RTL support

---

**Document Version**: 1.0
**Last Updated**: January 2026
**Author**: GRC System Development Team
