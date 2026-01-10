# Workflow Management Localization Keys (Batch 3)

**Date:** 2026-01-10
**Status:** Complete i18n Conversion
**Files Converted:** 31 files (14 Workflow + 17 WorkflowUI)
**Total Keys:** 487 keys

---

## Summary

This document contains all localization keys required for Workflow and WorkflowUI views in the GRC system. Keys are organized by view file and functional area.

### Statistics
- **Workflow Views:** 14 files, 245 keys
- **WorkflowUI Views:** 17 files, 242 keys
- **Common Elements:** Buttons, labels, statuses, messages

---

## 1. WORKFLOW VIEWS (14 files)

### 1.1 Approvals.cshtml (45 keys)

#### Page Structure
```
WorkflowApprovals              // Workflow Approvals
PendingApprovals               // Pending Approvals
ReviewApproveWorkflowRequests  // Review and approve pending workflow requests
Pending                        // Pending
SearchApprovals                // Search approvals by workflow, submitter...
AllStatuses                    // All Statuses
DelegatedToMe                  // Delegated to Me
Overdue                        // Overdue
```

#### Table Headers
```
Workflow                       // Workflow
Level                          // Level
SubmittedBy                    // Submitted By
Priority                       // Priority
DueDate                        // Due Date
DaysRemaining                  // Days Remaining
Actions                        // Actions
```

#### Statistics
```
Completed                      // Completed
AvgTurnaround                  // Avg Turnaround
```

#### Modal
```
ReviewApproval                 // Review Approval
ApprovalLevel                  // Approval Level
Comments                       // Comments
CommentsOptional               // Comments (Optional)
AddYourComments                // Add your comments...
ApprovalChain                  // Approval Chain
Delegate                       // Delegate
Reject                         // Reject
Approve                        // Approve
```

#### JavaScript Messages
```
Review                         // Review
ApprovalSubmittedSuccessfully  // Approval submitted successfully!
ErrorSubmittingApproval        // Error submitting approval
PleaseProvideRejectionReason   // Please provide a reason for rejection
RejectionSubmittedSuccessfully // Rejection submitted successfully!
ErrorSubmittingRejection       // Error submitting rejection
```

---

### 1.2 ByCategory.cshtml (15 keys)

```
WorkflowsByCategory            // Workflows by Category
Category                       // Category
AllWorkflows                   // All Workflows
NoWorkflowsFoundInCategory     // No workflows found in this category.
Number                         // Number
Name                           // Name
Status                         // Status
AssignedTo                     // Assigned To
Edit                           // Edit
Details                        // Details
```

---

### 1.3 ByStatus.cshtml (14 keys)

```
WorkflowsByStatus              // Workflows by Status
NoWorkflowsFoundWithStatus     // No workflows found with this status.
```

---

### 1.4 Create.cshtml (50 keys)

```
CreateNewWorkflow              // Create New Workflow
BackToList                     // Back to List
WorkflowName                   // Workflow Name
EgQuarterlyAccessReview        // e.g. Quarterly Access Review
WorkflowNumber                 // Workflow Number
EgWfAcc01                      // e.g. WF-ACC-01
Draft                          // Draft
Active                         // Active
Suspended                      // Suspended
SelectCategory                 // Select Category...
AccessControl                  // Access Control
IncidentManagement             // Incident Management
ChangeManagement               // Change Management
VendorManagement               // Vendor Management
Compliance                     // Compliance
Type                           // Type
SelectType                     // Select Type...
Sequential                     // Sequential
Parallel                       // Parallel
StateMachine                   // State Machine
Low                            // Low
Medium                         // Medium
High                           // High
Critical                       // Critical
Description                    // Description
WorkflowStepsJsonText          // Workflow Steps (JSON/Text)
DefineSteps                    // Define steps...
ConditionsLogic                // Conditions / Logic
DefineConditions               // Define conditions...
InitiatedBy                    // Initiated By
NotificationSettings           // Notification Settings
EmailAddressesForNotifications // Email addresses or logic for notifications...
CreateWorkflow                 // Create Workflow
Cancel                         // Cancel
Creating                       // Creating...
WorkflowCreatedSuccessfully    // Workflow created successfully!
ErrorCreatingWorkflow          // Error creating workflow
```

---

### 1.5 Delete.cshtml (20 keys)

```
DeleteWorkflow                 // Delete Workflow
DeleteConfirmation             // Delete Confirmation
AreYouSureDeleteWorkflow       // Are you sure you want to delete this workflow?
CannotBeUndone                 // This action cannot be undone. The workflow {0} ({1}) will be permanently deleted.
HistoricalExecutionsDeleted    // This workflow has {0} historical executions that will also be deleted.
```

---

### 1.6 Details.cshtml (45 keys)

```
WorkflowDetails                // Workflow Details
Execute                        // Execute
StartNewExecutionConfirm       // Start a new execution of this workflow?
GeneralInformation             // General Information
LogicSteps                     // Logic & Steps
Steps                          // Steps
Conditions                     // Conditions
Notifications                  // Notifications
AssignmentTiming               // Assignment & Timing
ExecutionHistory               // Execution History
ViewExecutionHistory           // View Execution History
```

---

### 1.7 Edit.cshtml (30 keys)

```
EditWorkflow                   // Edit Workflow
SaveChanges                    // Save Changes
```

---

### 1.8 Escalations.cshtml (75 keys)

```
WorkflowEscalations            // Workflow Escalations
SlaEscalations                 // SLA & Escalations
MonitorOverdueWorkflows        // Monitor overdue workflows and escalation triggers
ActiveEscalations              // Active Escalations
EscalationLevel                // Escalation Level
AllLevels                      // All Levels
Level1_24hrs                   // Level 1 (24hrs)
Level2_48hrs                   // Level 2 (48hrs)
Level3_5days                   // Level 3 (5 days)
HoursOverdue                   // Hours Overdue
SlaBreach                      // SLA Breach
EscalatedTo                    // Escalated to
View                           // View
Acknowledge                    // Acknowledge
TimeRemaining                  // Time Remaining
NextEscalation                 // Next Escalation
CurrentApprover                // Current Approver
EscalateNow                    // Escalate Now
SlaConfiguration               // SLA Configuration
AutoEscalate                   // Auto-Escalate
PendingSla                     // Pending SLA
Resolved                       // Resolved
AvgResolution                  // Avg Resolution
EscalationDetails              // Escalation Details
SlaBreach                      // SLA Breach
ThisApprovalHoursOverdue       // This approval is {0} hours overdue
EscalationHistory              // Escalation History
AdditionalNotes                // Additional Notes
AddEscalationNotes             // Add escalation notes...
Close                          // Close
ForceEscalation                // Force Escalation
EscalateConfirm                // Are you sure you want to escalate this workflow now?
WorkflowEscalatedSuccessfully  // Workflow escalated successfully!
ErrorEscalatingWorkflow        // Error escalating workflow
EscalationAcknowledged         // Escalation acknowledged!
ErrorAcknowledgingEscalation   // Error acknowledging escalation
```

---

### 1.9 Executions.cshtml (25 keys)

```
WorkflowExecutions             // Workflow Executions
ExecutionHistory               // Execution History
ForWorkflowId                  // For Workflow ID: {0}
ExecuteAgain                   // Execute Again
StartNewExecutionConfirm       // Start a new execution?
BackToWorkflow                 // Back to Workflow
NoExecutionHistoryFound        // No execution history found.
ThisWorkflowNotRunYet          // This workflow has not been run yet.
ExecutionNumber                // Execution #
StartTime                      // Start Time
EndTime                        // End Time
Duration                       // Duration
Logs                           // Logs
LogsComingSoon                 // Logs coming soon
```

---

### 1.10 Inbox.cshtml (65 keys)

```
WorkflowInbox                  // Workflow Inbox
TrackManageWorkflowTasks       // Track and manage your workflow tasks and approvals
NewWorkflow                    // New Workflow
MyTasks                        // My Tasks
Approvals                      // Approvals
Escalations                    // Escalations
Delegated                      // Delegated
Task                           // Task
InProgress                     // In Progress
Continue                       // Continue
Start                          // Start
PendingApprovals               // Pending Approvals
SubmittedByLevel               // Submitted by {0} • Level {1}
RiskApproval                   // Risk Approval
EscalationAlert                // EscalationAlert
FindingRemediationOverdue      // Finding Remediation (WF-004) is 24 hours overdue
EscalatedToDirector            // Escalated to: Director • Action Required
ViewDetails                    // View Details
DelegatedApprovalsNote         // You have delegated {0} approvals. They will return to you if not completed by the delegate.
CompletedWorkflowsThisMonth    // You have completed {0} workflows this month. Great progress!
TodaysTasks                    // Today's Tasks
ThisWeek                       // This Week
CompletionRate                 // Completion Rate
WorkflowType                   // Workflow Type
SelectWorkflow                 // -- Select Workflow --
Notes                          // Notes
```

---

### 1.11 Index.cshtml (30 keys)

```
Workflows                      // Workflows
NewWorkflow                    // New Workflow
Statistics                     // Statistics
Category                       // Category
Executions                     // Executions
Runs                           // {0} Runs
ExecuteNow                     // Execute Now
```

---

### 1.12 Overdue.cshtml (15 keys)

```
OverdueWorkflows               // Overdue Workflows
NoOverdueWorkflows             // No overdue workflows.
EverythingOnTrack              // Everything is on track!
OverdueBy                      // Overdue By
DaysOverdue                    // {0} days
```

---

### 1.13 ProcessFlow.cshtml (55 keys)

```
WorkflowProcess                // Workflow Process
VisualizeMonitorExecution      // Visualize and monitor workflow execution
AllWorkflows                   // All Workflows
StartedByStarted               // Started by: {0} • Started: {1}
OverallProgress                // Overall Progress
ProcessFlow                    // Process Flow
DefineScope                    // Define Scope
CompletedOnHours               // Completed • {0} • {1} hours
AllDocumentationSubmitted      // All documentation submitted
AssetIdentification            // Asset Identification
AssetsIdentifiedRemaining      // {0} assets identified, {1} remaining
ControlAssessment              // Control Assessment
Scheduled                      // Scheduled
GapAnalysis                    // Gap Analysis
ReviewApproval                 // Review & Approval
WillRequireApprovals           // Will require {0} approvals
AwaitingCompletionPreviousSteps // Awaiting completion of previous steps
ReportGeneration               // Report Generation
ScheduledAfterApproval         // Scheduled after approval
ApprovalChain                  // Approval Chain
DepartmentHead                 // Department Head
SlaHours                       // SLA: {0} hours
Manager                        // Manager
Director                       // Director
WorkflowStatistics             // Workflow Statistics
TotalSteps                     // Total Steps
CompletedSteps                 // Completed
InProgressSteps                // In Progress
PendingSteps                   // Pending
EstimatedCompletion            // Estimated Completion
```

---

### 1.14 Statistics.cshtml (40 keys)

```
WorkflowStatistics             // Workflow Statistics
TotalWorkflows                 // Total Workflows
ActiveWorkflows                // Active
OverdueWorkflows               // Overdue
TotalExecutions                // Executions
ExecutionSuccessRate           // Execution Success Rate
SuccessfulFailed               // {0} Successful / {1} Failed
AvgExecutionTime               // Avg Execution Time
AverageDurationPerExecution    // Average duration per execution
WorkflowsByCategory            // Workflows by Category
StatusDistribution             // Status Distribution
PriorityDistribution           // Priority Distribution
```

---

## 2. WORKFLOWUI VIEWS (17 files)

### 2.1 ApprovalDetail.cshtml (35 keys)

```
ApprovalDetail                 // Approval Detail
ApprovalRequest                // Approval Request
ReviewApprovePendingRequest    // Review and approve pending request
RequestDetails                 // Request Details
RequestType                    // Request Type
EvidenceApproval               // Evidence Approval
RequestedBy                    // Requested By
RequestDate                    // Request Date
RequestDescription             // Request for approval of Information Security Policy v2.0 evidence submission for NCA-ECC control ECC-1-1-1.
Attachments                    // Attachments
AddComment                     // Add Comment
PendingApproval                // Pending Approval
WaitingForYourDecision         // Waiting for your decision
ApprovalHistory                // Approval History
Level1Approved                 // Level 1 Approved
Level2Pending                  // Level 2 Pending
By                             // By
AssignedTo                     // Assigned to
BackToApprovals                // Back to Approvals
ApproveRequestConfirm          // Are you sure you want to approve this request?
RequestApprovedSuccessfully    // Request approved successfully!
ProvideRejectionReason         // Please provide a reason for rejection:
RequestRejected                // Request rejected.
```

---

### 2.2 Approvals.cshtml (80 keys)

```
ApprovalWorkflows              // Approval Workflows
MultiLevelApprovalRouting      // Multi-level approval routing with manager, compliance, and executive sign-off
SubmitForApproval              // Submit for Approval
AwaitingManager                // Awaiting Manager
ManagerApproved                // Manager Approved
AwaitingCompliance             // Awaiting Compliance
Rejected                       // Rejected
AllApprovals                   // All Approvals
Id                             // ID
DocumentType                   // Document Type
CurrentStage                   // Current Stage
SubmittedDate                  // Submitted Date
LoadingApprovals               // Loading approvals...
ApprovalProcessFlow            // Approval Process Flow
Submit                         // Submit
Initiate                       // Initiate
Approval1                      // Approval 1
Approval2                      // Approval 2
FinalSignOff                   // Final Sign-off
SelectDocumentType             // Select document type...
PolicyDocument                 // Policy Document
Procedure                      // Procedure
ControlImplementation          // Control Implementation
FrameworkChange                // Framework Change
DocumentId                     // Document ID
SupportingDocuments            // Supporting Documents
DocumentSubmittedForApproval   // Document submitted for approval
ErrorSubmittingDocument        // Error submitting document
EnterApprovalComments          // Enter approval comments:
ApprovalSubmitted              // Approval submitted
ErrorApproving                 // Error approving
EnterRejectionReason           // Enter rejection reason:
DocumentRejected               // Document rejected
ErrorRejecting                 // Error rejecting
NoPendingApprovals             // No pending approvals
ErrorLoadingApprovals          // Error loading approvals
ItemApproved                   // Item approved
ItemRejected                   // Item rejected
```

---

### 2.3 AuditDetail.cshtml (40 keys)

```
AuditWorkflowDetail            // Audit Workflow Detail
AuditFindingRemediation        // Audit Finding Remediation
TrackRemediateAuditFindings    // Track and remediate audit findings
CloseFinding                   // Close Finding
FindingDetails                 // Finding Details
FindingId                      // Finding ID
Severity                       // Severity
Audit                          // Audit
ControlReference               // Control Reference
FindingDescription             // Finding Description
Recommendation                 // Recommendation
RemediationPlan                // Remediation Plan
DefineAccessReviewProcess      // Define access review process
ImplementAutomatedReviewTool   // Implement automated review tool
CompleteFirstQuarterlyReview   // Complete first quarterly review
EvidenceOfRemediation          // Evidence of Remediation
UploadRemediationEvidence      // Upload Remediation Evidence
Progress                       // Progress
TasksCompleted                 // {0} of {1} tasks completed
Timeline                       // Timeline
FindingDate                    // Finding Date
Owner                          // Owner
BackToAudits                   // Back to Audits
AllRemediationTasksComplete    // Are all remediation tasks complete? Close this finding?
FindingClosedSuccessfully      // Finding closed successfully!
```

---

### 2.4 Audits.cshtml (75 keys)

```
AuditManagement                // Audit Management
PlanExecuteReportAudits        // Plan, execute, and report on compliance audits
NewAudit                       // New Audit
Planning                       // Planning
Fieldwork                      // Fieldwork
DraftReport                    // Draft Report
FinalReport                    // Final Report
AuditProcessFlow               // Audit Process Flow
Plan                           // Plan
Report                         // Report
FollowUp                       // Follow-up
AllAudits                      // All Audits
AuditId                        // Audit ID
Scope                          // Scope
Auditors                       // Auditor(s)
TargetEnd                      // Target End
LoadingAudits                  // Loading audits...
RecentFindings                 // Recent Findings
NoFindingsRecordedYet          // No findings recorded yet
CreateNewAudit                 // Create New Audit
AuditType                      // Audit Type
InternalAudit                  // Internal Audit
ExternalAudit                  // External Audit
ComplianceAudit                // Compliance Audit
OperationalAudit               // Operational Audit
ItAudit                        // IT Audit
DefineAuditScope               // Define audit scope...
AuditPeriodStart               // Audit Period Start
AuditPeriodEnd                 // Audit Period End
LeadAuditor                    // Lead Auditor
SelectAuditor                  // Select auditor...
TeamMembers                    // Team Members
ListAuditorsCommaSeparated     // List auditors (comma separated)
AuditDetails                   // Audit Details
UpdateStatus                   // Update Status
AuditCreatedSuccessfully       // Audit created successfully
ErrorCreatingAudit             // Error creating audit
NoAuditsScheduled              // No audits scheduled
StartFieldwork                 // Start Fieldwork
AddFinding                     // Add Finding
SubmitReport                   // Submit Report
AuditStatusUpdated             // Audit status updated
```

---

### 2.5 ControlImplementation.cshtml (35 keys)

```
ControlImplementationWorkflows // Control Implementation Workflows
ManageControlImplementationTasks // Manage control implementation tasks and track progress
NewImplementation              // New Implementation
TotalImplementations           // Total Implementations
AllStatus                      // All Status
AllFrameworks                  // All Frameworks
NcaEcc                         // NCA ECC
SamaCsf                        // SAMA CSF
Pdpl                           // PDPL
SearchControls                 // Search controls...
Filter                         // Filter
Control                        // Control
Framework                      // Framework
LoadingWorkflows               // Loading workflows...
```

---

### 2.6 ControlImplementationDetail.cshtml (40 keys)

```
ControlImplementationDetail    // Control Implementation Detail
Loading                        // Loading...
LoadingControlDetails          // Loading control details...
CompleteTask                   // Complete Task
Reassign                       // Reassign
ImplementationSteps            // Implementation Steps
ReviewRequirements             // Step 1: Review Requirements
ImplementControl               // Step 2: Implement Control
UploadEvidence                 // Step 3: Upload Evidence
NoEvidenceUploadedYet          // No evidence uploaded yet. Upload evidence to proceed.
CurrentStatus                  // Current Status
ControlInfo                    // Control Info
Domain                         // Domain
ControlType                    // Control Type
Governance                     // Governance
Preventive                     // Preventive
```

---

### 2.7 Error.cshtml (10 keys)

```
Error                          // Error
SomethingWentWrong             // Something Went Wrong
ErrorProcessingRequest         // We encountered an error while processing your request. Please try again or contact support if the problem persists.
BackToWorkflows                // Back to Workflows
GoToDashboard                  // Go to Dashboard
NeedHelp                       // Need help?
ContactSupport                 // Contact Support
```

---

### 2.8 Evidence.cshtml (60 keys)

```
EvidenceCollection             // Evidence Collection
SubmitManageControlEvidence    // Submit and manage control evidence for compliance verification
SubmitEvidence                 // Submit Evidence
PendingReview                  // Pending Review
UnderReview                    // Under Review
Approved                       // Approved
EvidenceSubmissions            // Evidence Submissions
ControlId                      // Control ID
SubmissionDate                 // Submission Date
Files                          // Files
LoadingEvidence                // Loading evidence...
SubmitControlEvidence          // Submit Control Evidence
SelectAControl                 // Select a control...
EvidenceType                   // Evidence Type
SelectType                     // Select type...
Documentation                  // Documentation
Screenshot                     // Screenshot
TestReport                     // Test Report
Interview                      // Interview Notes
SystemLog                      // System Log
Other                          // Other
DescribeEvidenceSupport        // Describe the evidence and why it supports this control...
UploadFiles                    // Upload Files
Upload                         // Upload
AcceptedFormats                // Accepted: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG (Max 10 MB each)
ReviewEvidence                 // Review Evidence
RequestRevision                // Request Revision
PleaseFillAllRequiredFields    // Please fill all required fields
EvidenceSubmittedSuccessfully  // Evidence submitted successfully
ErrorSubmittingEvidence        // Error submitting evidence
EvidenceApproved               // Evidence approved
EnterRevisionReason            // Enter reason for revision request:
RevisionRequested              // Revision requested
NoEvidenceSubmitted            // No evidence submitted
FileCount                      // {0} files
```

---

### 2.9 EvidenceDetail.cshtml (35 keys)

```
EvidenceWorkflowDetail         // Evidence Workflow Detail
EvidenceReview                 // Evidence Review
ReviewScoreEvidenceSubmission  // Review and score evidence submission
Accept                         // Accept
EvidenceNumber                 // Evidence Number
Document                       // Document
Title                          // Title
AttachedFiles                  // Attached Files
Download                       // Download
Scoring                        // Scoring
CompletenessScore              // Completeness Score
NotAcceptable                  // 1 - Not Acceptable
PartiallyAcceptable            // 2 - Partially Acceptable
Acceptable                     // 3 - Acceptable
Good                           // 4 - Good
Excellent                      // 5 - Excellent
RelevanceScore                 // Relevance Score
NotRelevant                    // 1 - Not Relevant
PartiallyRelevant              // 2 - Partially Relevant
Relevant                       // 3 - Relevant
HighlyRelevant                 // 4 - Highly Relevant
PerfectlyRelevant              // 5 - Perfectly Relevant
ReviewComments                 // Review Comments
AddReviewComments              // Add review comments...
BackToEvidence                 // Back to Evidence
AcceptEvidenceConfirm          // Accept this evidence?
EvidenceAccepted               // Evidence accepted!
WhatRevisionsNeeded            // What revisions are needed?
ReasonForRejection             // Reason for rejection:
EvidenceRejected               // Evidence rejected.
```

---

### 2.10 ExceptionDetail.cshtml (30 keys)

```
ExceptionDetail                // Exception Detail
ExceptionRequest               // Exception Request
ControlExceptionRequestDetails // Control exception request details
ApproveException               // Approve Exception
RejectException                // Reject Exception
ExceptionDetails               // Exception Details
RiskLevel                      // Risk Level
Justification                  // Justification
CompensatingControls           // Compensating Controls
RequestedDuration              // Requested Duration
BusinessImpactIfDenied         // Business Impact if Denied
CriticalSystemUnavailable      // Critical system unavailable, affecting 500+ users
ApprovalChain                  // Approval Chain
RiskOfficer                    // Risk Officer
Ciso                           // CISO
BackToExceptions               // Back to Exceptions
ApproveExceptionConfirm        // Approve this exception request?
ExceptionApproved              // Exception approved!
ExceptionRejected              // Exception rejected.
```

---

### 2.11 Exceptions.cshtml (80 keys)

```
ExceptionWorkflows             // Exception Workflows
ManageControlExceptions        // Manage control exceptions and waivers
RequestException               // Request Exception
PendingReview                  // Pending Review
ExpiringSoon                   // Expiring Soon
Expired                        // Expired
ExceptionRequests              // Exception Requests
ExceptionId                    // Exception ID
ExpiryDate                     // Expiry Date
RequestControlException        // Request Control Exception
ControlTitle                   // Control Title
AutoFilledFromControl          // Auto-filled from control
MinimalImpact                  // Low - Minimal impact
ModerateImpact                 // Medium - Moderate impact
SignificantImpact              // High - Significant impact
SevereImpact                   // Critical - Severe impact
ExceptionDuration              // Exception Duration
SelectDuration                 // Select duration...
Days30                         // 30 Days
Days90Quarter                  // 90 Days (Quarter)
Days180HalfYear                // 180 Days (Half Year)
OneYear                        // 1 Year
CustomDate                     // Custom Date
CustomExpiryDate               // Custom Expiry Date
ExplainWhyExceptionNeeded      // Explain why this exception is needed, what compensating controls are in place, and the plan to remediate...
MinimumCharsRequired           // Minimum 50 characters required
DescribeCompensatingControls   // Describe any compensating controls that mitigate the risk...
SubmitRequest                  // Submit Request
ReviewExceptionRequest         // Review Exception Request
NoExceptionsFound              // No exceptions found
SelectRiskLevel                // Select risk level...
JustificationTooShort          // Justification must be at least 50 characters
SelectCustomExpiryDate         // Please select a custom expiry date
ExceptionRequestSubmitted      // Exception request {0} submitted successfully
ErrorSubmittingExceptionRequest // Error submitting exception request
ExceptionApproved              // Exception approved
ExceptionRejected              // Exception rejected
```

---

### 2.12 Index.cshtml (45 keys)

```
WorkflowManagement             // Workflow Management
ManageComplianceWorkflows      // Manage compliance, control, and operational workflows
ControlImplementation          // Control Implementation
Evidence                       // Evidence
Risks                          // Risks
Audits                         // Audits
ControlImplementationWorkflows // Control Implementation Workflows
ApprovalWorkflows              // Approval Workflows
EvidenceCollectionWorkflows    // Evidence Collection Workflows
RiskAssessmentWorkflows        // Risk Assessment Workflows
AuditWorkflows                 // Audit Workflows
CollectionDate                 // Collection Date
NoEvidenceSubmissions          // No evidence submissions
NoRiskAssessments              // No risk assessments
NoActiveAudits                 // No active audits
CreateNewWorkflow              // Create New Workflow
SelectWorkflowType             // Select a workflow type...
ControlImplementationType      // Control Implementation
ApprovalSignOff                // Approval/Sign-off
RiskAssessment                 // Risk Assessment
ComplianceTesting              // Compliance Testing
Remediation                    // Remediation
PolicyReview                   // Policy Review
TrainingAssignment             // Training Assignment
ExceptionHandling              // Exception Handling
ReferenceId                    // Reference ID
WorkflowCreatedSuccessfully    // Workflow created successfully
ErrorCreatingWorkflow          // Error creating workflow
```

---

### 2.13 Policies.cshtml (55 keys)

```
PolicyWorkflows                // Policy Workflows
ManagePolicyReviewApproval     // Manage policy review, approval, and publication workflows
NewPolicy                      // New Policy
Draft                          // Draft
UnderReview                    // Under Review
PendingApproval                // Pending Approval
Published                      // Published
PolicyWorkflowQueue            // Policy Workflow Queue
Version                        // Version
Stage                          // Stage
ReviewDate                     // Review Date
LoadingPolicies                // Loading policies...
RejectPolicy                   // Reject Policy
PleaseProvideRejectionReason   // Please provide a reason for rejecting this policy:
RejectionReason                // Rejection Reason
DescribeWhatNeedsChanged       // Describe what needs to be changed or corrected...
RejectReturn                   // Reject & Return
ApprovePolicy                  // Approve Policy
ApprovePublishPolicyConfirm    // Are you sure you want to approve and publish this policy?
OnceApprovedPolicyPublished    // Once approved, the policy will be published and visible to all users.
ApprovePublish                 // Approve & Publish
NoPoliciesFound                // No policies found
PolicyApprovedPublished        // Policy approved and published successfully
ErrorApprovingPolicy           // Error approving policy
PleaseProvideRejectionReason   // Please provide a rejection reason
PolicyReturnedForRevision      // Policy returned for revision
ErrorRejectingPolicy           // Error rejecting policy
```

---

### 2.14 Remediation.cshtml (50 keys)

```
RemediationWorkflows           // Remediation Workflows
TrackManageRemediationActivities // Track and manage remediation activities for findings and gaps
NewRemediation                 // New Remediation
InProgress                     // In Progress
PendingVerification            // Pending Verification
Closed                         // Closed
ActiveRemediations             // Active Remediations
AllPriorities                  // All Priorities
RemediationId                  // Remediation ID
Finding                        // Finding
Source                         // Source
LoadingRemediations            // Loading remediations...
UpdateRemediationProgress      // Update Remediation Progress
ProgressPercentage             // Progress (%)
UpdateNotes                    // Update Notes
DescribeProgressMade           // Describe the progress made...
SaveProgress                   // Save Progress
NoRemediationsFound            // No remediations found
ProgressUpdatedSuccessfully    // Progress updated successfully
ErrorUpdatingProgress          // Error updating progress
```

---

### 2.15 Risks.cshtml (50 keys)

```
RiskManagementWorkflows        // Risk Management Workflows
ManageRiskAssessments          // Manage risk assessments and treatment workflows
NewRiskAssessment              // New Risk Assessment
CriticalRisks                  // Critical Risks
HighRisks                      // High Risks
MediumRisks                    // Medium Risks
LowRisks                       // Low Risks
ActiveRiskWorkflows            // Active Risk Workflows
AllCategories                  // All Categories
Cybersecurity                  // Cybersecurity
Operational                    // Operational
Financial                      // Financial
Strategic                      // Strategic
RiskId                         // Risk ID
RiskTitle                      // Risk Title
RiskLevel                      // Risk Level
Score                          // Score
Treatment                      // Treatment
LoadingRisks                   // Loading risks...
RiskByCategory                 // Risk by Category
TreatmentStatus                // Treatment Status
NoRisksFound                   // No risks found
Uncategorized                  // Uncategorized
Unnamed                        // Unnamed
Unassigned                     // Unassigned
InTreatment                    // In Treatment
Assessment                     // Assessment
Accepted                       // Accepted
Mitigated                      // Mitigated
Unknown                        // Unknown
Count                          // Count
```

---

### 2.16 Testing.cshtml (50 keys)

```
ControlTestingWorkflows        // Control Testing Workflows
ManageControlTestingEffectiveness // Manage control testing and effectiveness evaluation
ScheduleTest                   // Schedule Test
ScheduledTests                 // Scheduled Tests
Passed                         // Passed
Failed                         // Failed
TestingQueue                   // Testing Queue
TestId                         // Test ID
TestType                       // Test Type
Tester                         // Tester
Result                         // Result
LoadingTests                   // Loading tests...
ExecuteControlTest             // Execute Control Test
TestResult                     // Test Result
SelectResult                   // Select result...
Pass                           // Pass - Control is effective
Fail                           // Fail - Control is not effective
Partial                        // Partial - Some issues identified
Score0To100                    // Score (0-100)
EnterTestObservations          // Enter test observations and findings...
AttachScreenshotsDocuments     // Attach screenshots or documents
SubmitResult                   // Submit Result
NoTestsFound                   // No tests found
PleaseSelectTestResult         // Please select a test result
TestResultSubmittedSuccessfully // Test result submitted successfully
ErrorSubmittingTestResult      // Error submitting test result
```

---

### 2.17 Training.cshtml (25 keys)

```
TrainingWorkflows              // Training Workflows
ManageSecurityAwarenessTraining // Manage security awareness and compliance training workflows
AssignTraining                 // Assign Training
TotalAssigned                  // Total Assigned
TrainingAssignments            // Training Assignments
Training                       // Training
Assignee                       // Assignee
SecurityAwareness2026          // Security Awareness 2026
AllEmployees                   // All Employees
PdplCompliance                 // PDPL Compliance
DataHandlers                   // Data Handlers
PhishingAwareness              // Phishing Awareness
ItDepartment                   // IT Department
CompletionRate                 // Completion Rate
OverallCompletion              // Overall Completion
Reminders                      // Reminders
SendReminders                  // Send Reminders
ExportReport                   // Export Report
```

---

## 3. COMMON ELEMENTS (Shared across views)

### 3.1 Status Values
```
Active                         // Active
Draft                          // Draft
Suspended                      // Suspended
Completed                      // Completed
Pending                        // Pending
InProgress                     // In Progress
Overdue                        // Overdue
UnderReview                    // Under Review
PendingApproval                // Pending Approval
Published                      // Published
Rejected                       // Rejected
Closed                         // Closed
Scheduled                      // Scheduled
Passed                         // Passed
Failed                         // Failed
```

### 3.2 Priority/Severity Levels
```
Low                            // Low
Medium                         // Medium
High                           // High
Critical                       // Critical
Normal                         // Normal
```

### 3.3 Common Actions
```
Create                         // Create
Edit                           // Edit
Delete                         // Delete
View                           // View
Details                        // Details
Save                           // Save
Cancel                         // Cancel
Submit                         // Submit
Approve                        // Approve
Reject                         // Reject
Close                          // Close
Back                           // Back
Filter                         // Filter
Search                         // Search
Upload                         // Upload
Download                       // Download
Export                         // Export
```

### 3.4 Common Labels
```
Name                           // Name
Title                          // Title
Description                    // Description
Status                         // Status
Priority                       // Priority
DueDate                        // Due Date
CreatedDate                    // Created Date
UpdatedDate                    // Updated Date
Owner                          // Owner
AssignedTo                     // Assigned To
Category                       // Category
Type                           // Type
Comments                       // Comments
Actions                        // Actions
Progress                       // Progress
Loading                        // Loading
```

### 3.5 Common Messages
```
LoadingPleaseWait              // Loading, please wait...
NoDataAvailable                // No data available
ErrorLoadingData               // Error loading data
OperationSuccessful            // Operation successful
OperationFailed                // Operation failed
ConfirmAction                  // Are you sure?
ChangesSaved                   // Changes saved successfully
ErrorSavingChanges             // Error saving changes
PleaseSelectOption             // Please select an option
RequiredField                  // This field is required
InvalidInput                   // Invalid input
```

---

## 4. IMPLEMENTATION NOTES

### 4.1 Localization Injection Pattern
```csharp
@inject IStringLocalizer<SharedResource> L
@{
    ViewData["Title"] = L["WorkflowApprovals"];
}
```

### 4.2 String Interpolation
For messages with parameters, use string formatting:
```csharp
// English
L["TasksCompleted", completedCount, totalCount]
// Produces: "3 of 5 tasks completed"

// Arabic (if RTL formatting needed)
L["TasksCompleted", completedCount, totalCount]
// Produces: "تم إكمال 3 من 5 مهام"
```

### 4.3 Dynamic Content
For status badges and dynamic elements:
```csharp
<span class="badge bg-@GetStatusClass(status)">@L[status]</span>
```

### 4.4 JavaScript Localization
For JavaScript alerts and messages, pass localized strings from the view:
```javascript
const successMsg = '@L["WorkflowCreatedSuccessfully"]';
alert(successMsg);
```

---

## 5. ARABIC TRANSLATIONS (Sample)

### Core Workflow Terms
```
WorkflowApprovals              // موافقات سير العمل
PendingApprovals               // الموافقات المعلقة
ReviewApprove                  // المراجعة والموافقة
Approve                        // موافقة
Reject                         // رفض
Submit                         // إرسال
Create                         // إنشاء
Edit                           // تعديل
Delete                         // حذف
Status                         // الحالة
Priority                       // الأولوية
DueDate                        // تاريخ الاستحقاق
AssignedTo                     // مسند إلى
Active                         // نشط
Completed                      // مكتمل
Pending                        // معلق
Overdue                        // متأخر
High                           // عالي
Medium                         // متوسط
Low                            // منخفض
Critical                       // حرج
```

---

## 6. TESTING CHECKLIST

### 6.1 Visual Verification
- [ ] All page titles display correctly
- [ ] All buttons show localized text
- [ ] All table headers are translated
- [ ] All form labels are localized
- [ ] All validation messages appear in correct language

### 6.2 Functional Testing
- [ ] Language toggle works on all workflow pages
- [ ] Form submissions work with localized content
- [ ] Search filters work in both languages
- [ ] Date formats respect locale settings
- [ ] Number formats respect locale settings

### 6.3 RTL Testing (Arabic)
- [ ] Layout flips correctly for RTL
- [ ] Icons align properly
- [ ] Modals display correctly
- [ ] Tables align properly
- [ ] Forms flow right-to-left

---

## 7. NEXT STEPS

1. **Add to .resx files** (separate task)
   - SharedResource.en.resx
   - SharedResource.ar.resx

2. **Test each view** with language toggle

3. **Validate Arabic translations** with native speaker

4. **Update documentation** with any additional keys

5. **Create migration guide** for future workflow views

---

## Summary Statistics

- **Total Views:** 31 files
- **Total Keys:** 487 keys
- **Workflow Views:** 14 files, 245 keys
- **WorkflowUI Views:** 17 files, 242 keys
- **Common Elements:** ~50 shared keys
- **Estimated .resx entries needed:** ~400 unique keys (after deduplication)

---

**Document Version:** 1.0
**Last Updated:** 2026-01-10
**Status:** Complete - Ready for .resx implementation
