# Assessment Views Localization Keys

This document lists all the resource keys used in the Assessment views that need to be added to the `.resx` files.

## General Keys (Common across views)

- `BackToList` - "Back to List"
- `Cancel` - "Cancel"
- `Edit` - "Edit"
- `Details` - "Details"
- `Actions` - "Actions"
- `SaveChanges` - "Save Changes"
- `Form_None` - "-- None --"
- `Form_SelectType` - "Select Type..."

## Assessment-Specific Keys

### Page Titles
- `Assessment_Create_Title` - "Create Assessment"
- `Assessment_Edit_Title` - "Edit Assessment"
- `Assessment_Details_Title` - "Assessment Details"
- `Assessment_Delete_Title` - "Delete Assessment"
- `Assessment_ByControl_Title` - "Assessments for Control"
- `Assessment_Statistics_Title` - "Assessment Statistics"
- `Assessment_Upcoming_Title` - "Upcoming Assessments (Next 30 Days)"

### Headings
- `Assessment_Create_New` - "Create New Assessment"
- `Assessment_GeneralInfo` - "General Information"
- `Assessment_ResultsAndFindings` - "Results & Findings"
- `Assessment_ScheduleAndAssignment` - "Schedule & Assignment"
- `Assessment_RelatedItems` - "Related Items"

### Field Labels
- `Assessment_Name` - "Assessment Name"
- `Assessment_Number` - "Assessment Number"
- `Assessment_Code` - "Code"
- `Assessment_Type` - "Type"
- `Assessment_Status` - "Status"
- `Assessment_StartDate` - "Start Date"
- `Assessment_EndDate` - "End Date"
- `Assessment_ScheduledDate` - "Scheduled Date"
- `Assessment_Description` - "Description"
- `Assessment_AssignedTo` - "Assigned To"
- `Assessment_ReviewedBy` - "Reviewed By"
- `Assessment_ComplianceScore` - "Compliance Score (%)"
- `Assessment_Results` - "Results & Analysis"
- `Assessment_Findings` - "Findings"
- `Assessment_Recommendations` - "Recommendations"
- `Assessment_RelatedRisk` - "Related Risk (Optional)"
- `Assessment_RelatedControl` - "Related Control (Optional)"

### Placeholders
- `Assessment_Name_Placeholder` - "e.g. Q1 Security Assessment"
- `Assessment_Number_Placeholder` - "e.g. ASM-2024-001"
- `Assessment_AssignedTo_Placeholder` - "User or Group"

### Assessment Types
- `Assessment_Type_Risk` - "Risk Assessment"
- `Assessment_Type_Control` - "Control Testing"
- `Assessment_Type_Compliance` - "Compliance Audit"
- `Assessment_Type_Security` - "Security Review"

### Assessment Status
- `Assessment_Status_Planned` - "Planned"
- `Assessment_Status_InProgress` - "In Progress"
- `Assessment_Status_Completed` - "Completed"
- `Assessment_Status_Cancelled` - "Cancelled"
- `Assessment_Status_Overdue` - "Overdue"

### Buttons
- `CreateAssessment` - "Create Assessment"
- `Assessment_Delete_Button` - "Delete Assessment"
- `Assessment_Update` - "Update"
- `Assessment_NewAssessment` - "New Assessment"

### Delete View
- `Assessment_Delete_Confirmation` - "Delete Confirmation"
- `Assessment_Delete_Confirm` - "Are you sure you want to delete this assessment?"
- `Assessment_Delete_Warning` - "This action cannot be undone. The assessment"
- `Assessment_Delete_Permanent` - "will be permanently deleted."

### Details View
- `Assessment_NotScored` - "Not scored"
- `Assessment_NoResults` - "No results recorded."
- `Assessment_NoFindings` - "No findings recorded."
- `Assessment_NoRecommendations` - "No recommendations recorded."
- `Assessment_ViewRisk` - "View Risk"
- `Assessment_ViewControl` - "View Control"

### ByControl View
- `Assessment_BackToControl` - "Back to Control"
- `Assessment_AllAssessments` - "All Assessments"
- `Assessment_NoAssessmentsForControl` - "No assessments found for this control."
- `Assessment_CreateForControl` - "Create Assessment for Control"
- `Assessment_Date` - "Date"
- `Assessment_Score` - "Score"
- `Assessment_Reviewer` - "Reviewer"

### Statistics View
- `Assessment_TotalAssessments` - "Total Assessments"
- `Assessment_Completed` - "Completed"
- `Assessment_InProgress` - "In Progress"
- `Assessment_Overdue` - "Overdue"
- `Assessment_AverageScoresAndCompletion` - "Average Scores & Completion"
- `Assessment_AverageScore` - "Average Score"
- `Assessment_CompletionRate` - "Completion Rate"
- `Assessment_PendingAssessments` - "Pending Assessments"
- `Assessment_AssessmentsByType` - "Assessments by Type"
- `Assessment_StatusDistribution` - "Status Distribution"

### Upcoming View
- `Assessment_NoUpcomingAssessments` - "No upcoming assessments scheduled for the next 30 days."
- `Assessment_AllCaughtUp` - "You're all caught up!"
- `Assessment_DueDate` - "Due Date"
- `Assessment_Assessment` - "Assessment"
- `Assessment_DaysLeft` - "Days Left"
- `Assessment_OverdueBy` - "Overdue by"
- `Assessment_Days` - "days"

## Files Converted

1. ✅ `src/GrcMvc/Views/Assessment/Create.cshtml`
2. ✅ `src/GrcMvc/Views/Assessment/Edit.cshtml`
3. ✅ `src/GrcMvc/Views/Assessment/Details.cshtml`
4. ✅ `src/GrcMvc/Views/Assessment/Delete.cshtml`
5. ✅ `src/GrcMvc/Views/Assessment/ByControl.cshtml`
6. ✅ `src/GrcMvc/Views/Assessment/Statistics.cshtml`
7. ✅ `src/GrcMvc/Views/Assessment/Upcoming.cshtml`

## Total Resource Keys

**Total: 78 unique resource keys** (excluding common keys that may already exist like BackToList, Cancel, Edit, etc.)

## Next Steps

1. Add all these keys to `SharedResource.en.resx` (English)
2. Add translated versions to `SharedResource.ar.resx` (Arabic)
3. Test all views to ensure proper localization display
4. Verify that language switching works correctly across all Assessment views
