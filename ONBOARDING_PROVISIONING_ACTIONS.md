# What Onboarding Wizard Does When Completed

## Overview
When the tenant admin completes the 12-step onboarding wizard, the system performs **comprehensive provisioning** to set up the entire GRC environment. This document details all the actions that occur.

## 🎯 Phase 1: Critical Path (Synchronous - Must Complete)

These operations happen **immediately** and must complete before the user is redirected:

### 1. **Mark Wizard as Processing**
```csharp
wizard.WizardStatus = "Processing";
wizard.LastStepSavedAt = DateTime.UtcNow;
```
- Prevents duplicate submissions
- Locks wizard state

### 2. **Sync Organization Profile**
**Location**: `SyncOrganizationProfileAsync()`

**Creates/Updates**:
- ✅ **OrganizationProfile** entity with all 96 answers
- ✅ **Tenant.OnboardingStatus** = "COMPLETED"
- ✅ **Tenant.OnboardingCompletedAt** = DateTime.UtcNow
- ✅ Stores all wizard answers in `OnboardingQuestionsJson` for audit

**Data Synced**:
- **Section A**: Organization identity (name, type, sector, countries)
- **Section B**: Assurance objectives (primary driver, maturity, timeline)
- **Section C**: Regulatory applicability (regulators, frameworks)
- **Section D**: Scope (legal entities, business units, systems)
- **Section E**: Data landscape (data types, payment cards, cross-border)
- **Section F**: Technology stack (identity provider, ITSM, cloud)
- **Section G**: Governance (control ownership, exception approvers)
- **Section H**: Teams & roles (org admins, team members, RACI)
- **Section I**: Evidence standards (frequency, SLAs)
- **Section J**: Evidence retention (years, acceptable types)
- **Section K**: Baseline & overlays (default baseline, selected overlays)
- **Section L**: Success metrics & pilot scope

### 3. **Create Default Workspace**
**Location**: `ITenantOnboardingProvisioner.EnsureDefaultWorkspaceAsync()`

**Creates**:
- ✅ **Workspace** entity
  - Name: Organization legal name (or "Default Workspace")
  - Code: "DEFAULT"
  - Type: "Market"
  - Jurisdiction: Country of incorporation
  - Default language: Arabic
  - IsDefault: true

**Why**: Users need a workspace to access the app. Without workspace, users have nowhere to go.

---

## 🚀 Phase 2: Background Tasks (Asynchronous)

These operations run **in the background** after redirect to avoid timeout:

### 4. **Comprehensive Tenant Provisioning**
**Location**: `ITenantOnboardingProvisioner.ProvisionTenantAsync()`

**Creates**:
- ✅ **Assessment Template** (100Q template)
- ✅ **GRC Plan** (initial compliance plan)
- ✅ **Initial Assessments** (based on scope)
- ✅ **Workflows** (default workflow templates)

**Result**:
```
WorkspaceId: {Guid}
AssessmentTemplateId: {Guid}
GrcPlanId: {Guid}
```

### 5. **Scope Derivation**
**Location**: `IRulesEngineService.DeriveAndPersistScopeAsync()`

**Process**:
- ✅ Analyzes onboarding answers
- ✅ Applies rules engine
- ✅ Determines applicable:
  - Compliance frameworks
  - Regulatory requirements
  - Control libraries
  - Risk categories
- ✅ Creates **RuleExecutionLog** with scope results

**Why**: Determines what compliance requirements apply based on organization profile.

### 6. **GRC Plan Creation**
**Location**: `IPlanService.CreatePlanAsync()`

**Creates**:
- ✅ **Plan** entity
  - PlanCode: `PLAN-{yyyyMMdd}-001`
  - Name: "{Organization} - Initial Compliance Plan"
  - Description: "Auto-generated plan from onboarding wizard"
  - PlanType: "QuickScan" or "Full" (based on desired maturity)
  - StartDate: DateTime.UtcNow
  - TargetEndDate: Target timeline from wizard (or 90 days)
  - RulesetVersionId: From scope derivation

**Why**: Creates the initial compliance plan that drives all assessments.

### 7. **Create Initial Assessments**
**Location**: `CreateInitialAssessmentsAsync()`

**Creates**:
- ✅ **Assessment** entities (based on scope)
- ✅ Links to GRC Plan
- ✅ Links to Assessment Template
- ✅ Sets initial status: "Pending"

**Why**: Creates the actual assessments that users will work on.

### 8. **Auto-Assign Tasks by RACI**
**Location**: `AutoAssignTasksByRACIAsync()`

**Process**:
- ✅ Reads RACI matrix from Section H
- ✅ Assigns assessment tasks to team members
- ✅ Creates **WorkflowTask** entities
- ✅ Links tasks to assessments
- ✅ Sets assignees based on RACI roles

**Why**: Automatically assigns work to the right people based on RACI matrix.

### 9. **Setup Workspace Features**
**Location**: `SetupWorkspaceFeaturesAsync()`

**Enables**:
- ✅ Feature flags based on subscription tier
- ✅ Workspace-specific configurations
- ✅ Default settings from wizard answers

**Why**: Configures what features are available in the workspace.

### 10. **Activate Default Workflows**
**Location**: `ActivateDefaultWorkflowsAsync()`

**Creates**:
- ✅ **Workflow** templates
- ✅ **WorkflowInstance** entities
- ✅ Links workflows to assessments
- ✅ Sets workflow status: "Active"

**Why**: Activates the workflow engine for automated processes.

### 11. **Send Team Member Invitations**
**Location**: `SendOrgAdminInvitationsAsync()`

**Process**:
- ✅ Reads team members from Section H
- ✅ Creates **TenantUser** entities (if not exists)
- ✅ Sends invitation emails via `IGrcEmailService.SendTeamInvitationEmailAsync()`
- ✅ Sets invitation status: "Pending"

**Why**: Invites team members so they can access the system.

### 12. **Send Welcome Email**
**Location**: `SendOnboardingWelcomeEmailAsync()`

**Sends**:
- ✅ Welcome email to tenant admin
- ✅ Confirmation that onboarding is complete
- ✅ Next steps and getting started guide
- ✅ Links to dashboard and resources

**Why**: Confirms completion and guides user on next steps.

### 13. **Audit Logging**
**Location**: `LogOnboardingCompletedEventAsync()`

**Creates**:
- ✅ **AuditEvent** entity
  - EventType: "OnboardingCompleted"
  - Action: "Onboarding wizard completed with 12/12 steps"
  - Status: "Completed"
  - CompletedSteps: 12
  - Payload: Full wizard data JSON

**Why**: Creates audit trail of who completed onboarding and when.

---

## 📊 Complete Provisioning Summary

### Entities Created:
1. ✅ **OrganizationProfile** (with all 96 answers)
2. ✅ **Workspace** (default workspace)
3. ✅ **AssessmentTemplate** (100Q template)
4. ✅ **Plan** (initial GRC plan)
5. ✅ **Assessment** entities (based on scope)
6. ✅ **WorkflowTask** entities (auto-assigned)
7. ✅ **Workflow** templates
8. ✅ **WorkflowInstance** entities
9. ✅ **TenantUser** entities (for team members)
10. ✅ **RuleExecutionLog** (scope derivation results)
11. ✅ **AuditEvent** (onboarding completion)

### Entities Updated:
1. ✅ **Tenant** (OnboardingStatus = "COMPLETED")
2. ✅ **OnboardingWizard** (Status = "Completed", ProgressPercent = 100)

### Emails Sent:
1. ✅ Welcome email to tenant admin
2. ✅ Invitation emails to team members

### Features Enabled:
1. ✅ Workspace features (based on subscription)
2. ✅ Default workflows
3. ✅ Assessment templates
4. ✅ GRC plan access

---

## 🔄 Execution Flow

```
User Completes Step 12 (Final Step)
    ↓
Mark Wizard as "Processing"
    ↓
Sync Organization Profile (CRITICAL)
    ↓
Create Default Workspace (CRITICAL)
    ↓
Redirect to Completion Page
    ↓
[BACKGROUND TASKS START]
    ↓
├─→ Comprehensive Tenant Provisioning
│   ├─→ Assessment Template
│   ├─→ GRC Plan
│   ├─→ Initial Assessments
│   └─→ Workflows
    ↓
├─→ Scope Derivation
│   └─→ Rule Execution Log
    ↓
├─→ Create GRC Plan
│   └─→ Link to Scope
    ↓
├─→ Create Initial Assessments
│   └─→ Link to Plan
    ↓
├─→ Auto-Assign Tasks (RACI)
│   └─→ Create WorkflowTasks
    ↓
├─→ Setup Workspace Features
│   └─→ Enable Features
    ↓
├─→ Activate Default Workflows
│   └─→ Create WorkflowInstances
    ↓
├─→ Send Team Invitations
│   └─→ Create TenantUsers + Emails
    ↓
├─→ Send Welcome Email
│   └─→ Email to Admin
    ↓
└─→ Audit Logging
    └─→ Create AuditEvent
    ↓
[ALL BACKGROUND TASKS COMPLETE]
    ↓
System Ready for All Users ✅
```

---

## ⚠️ Why This Must Be Admin-Only

### 1. **Irreversible Operations**
- Workspace creation
- Plan creation
- Assessment creation
- Feature enablement
- **Cannot be undone** - requires admin authority

### 2. **System Resource Provisioning**
- Creates database records
- Allocates system resources
- Configures tenant settings
- **Requires admin permissions**

### 3. **Data Integrity**
- Single source of truth
- No conflicts
- Complete data sets
- **Admin ensures accuracy**

### 4. **Security Configuration**
- Sets security policies
- Configures access controls
- Defines compliance requirements
- **Admin is security owner**

---

## 📝 Code Locations

### Main Completion Logic:
- **File**: `OnboardingWizardController.cs`
- **Method**: `FinalizeOnboarding()` (Line 629)
- **Background**: `CompleteOnboardingBackgroundTasksAsync()` (Line 760)

### Provisioning Services:
- **ITenantOnboardingProvisioner**: Comprehensive provisioning
- **IWorkspaceManagementService**: Workspace creation
- **IPlanService**: GRC plan creation
- **IRulesEngineService**: Scope derivation
- **IGrcEmailService**: Email notifications

---

## ✅ Summary

**When onboarding is completed, the system:**

1. ✅ **Saves all 96 answers** to OrganizationProfile
2. ✅ **Creates default workspace** (users need this to access app)
3. ✅ **Provisions assessment templates** (100Q template)
4. ✅ **Creates initial GRC plan** (drives compliance work)
5. ✅ **Derives scope** (determines applicable requirements)
6. ✅ **Creates assessments** (actual work items)
7. ✅ **Auto-assigns tasks** (based on RACI matrix)
8. ✅ **Enables features** (based on subscription)
9. ✅ **Activates workflows** (automated processes)
10. ✅ **Invites team members** (sends emails)
11. ✅ **Sends welcome email** (confirmation)
12. ✅ **Logs audit event** (compliance trail)

**Result**: Complete GRC environment ready for all users! 🎯
