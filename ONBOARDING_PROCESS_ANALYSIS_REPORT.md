# GRC System Onboarding Process - Complete Analysis Report

**Generated:** January 7, 2026  
**Status:** READ-ONLY Analysis (No data changes)

---

## Executive Summary

The GRC System has **two parallel onboarding paths**:

| Path | Steps | Questions | Target Use Case |
|------|-------|-----------|-----------------|
| **Simple Flow** | 4 steps | 13+ questions | Quick setup, trial users |
| **12-Step Wizard** | 12 sections (A-L) | 96+ questions | Comprehensive enterprise configuration |

---

## 1. Controllers Architecture

### 1.1 MVC Controllers

| Controller | Location | Purpose | Auth Required |
|------------|----------|---------|---------------|
| `OnboardingController` | `Controllers/OnboardingController.cs` | Simple 4-step flow (Signup → OrgProfile → ReviewScope → CreatePlan) | Partial (AllowAnonymous for Signup) |
| `OnboardingWizardController` | `Controllers/OnboardingWizardController.cs` | 12-step comprehensive wizard (Sections A-L) | Yes (Tenant Admin) |
| `OrgSetupController` | `Controllers/OrgSetupController.cs` | Post-onboarding team/user management | Yes |
| `OwnerSetupController` | `Controllers/OwnerSetupController.cs` | Platform owner initial registration | Yes (Platform Admin) |

### 1.2 API Controllers

| Controller | Route | Purpose |
|------------|-------|---------|
| `OnboardingApiController` | `api/onboarding/*` | REST API for onboarding operations |

---

## 2. Simple Flow (4 Steps)

### Step 1: Signup (`/Onboarding/Signup`)

**View:** `Views/Onboarding/Signup.cshtml`  
**Progress:** 25%

#### Questions Asked:

| # | Field | Type | Required | Purpose |
|---|-------|------|----------|---------|
| 1 | Organization Name | Text Input | ✅ | Primary identifier |
| 2 | Administrator Email | Email Input | ✅ | Activation link destination |
| 3 | Subscription Tier | Select (Starter/Professional/Enterprise) | ✅ | Feature limits |
| 4 | Country of Operation | Select (GCC countries + Other) | ✅ | Regulatory framework derivation |
| 5 | Terms of Service | Checkbox | ✅ | Legal consent |
| 6 | Privacy Policy | Checkbox | ✅ | PDPL compliance |
| 7 | Data Processing Consent | Checkbox | ✅ | PDPL requirement |

#### Database Consequences:

```sql
INSERT INTO Tenants (
    Id,
    TenantSlug,           -- Generated from OrganizationName
    OrganizationName,
    AdminEmail,
    Status,               -- 'Pending'
    ActivationToken,      -- Generated UUID
    SubscriptionTier,
    CorrelationId,
    OnboardingStatus,     -- 'NOT_STARTED'
    CreatedAt
) VALUES (...)
```

#### Actions Triggered:
- ✅ Activation email sent to AdminEmail
- ✅ Consent record created via `/api/support/consent`
- ✅ TempData populated for next step

---

### Step 2: Organization Profile (`/Onboarding/OrgProfile`)

**View:** `Views/Onboarding/OrgProfile.cshtml`  
**Progress:** 50%

#### Questions Asked:

| # | Field | Type | Required | Purpose |
|---|-------|------|----------|---------|
| 1 | Organization Type | Select (11 options) | ✅ | Sector classification |
| 2 | Sector | Text Input | ✅ | Industry identification |
| 3 | Primary Country | Select (GCC + Other) | ✅ | Jurisdiction |
| 4 | Data Hosting Model | Select (OnPremise/Cloud/Hybrid) | ✅ | Infrastructure classification |
| 5 | Data Types Processed | Multi-Select (6 options) | ✅ | PDPL/regulatory triggers |
| 6 | Organization Size | Select (Startup/Small/Medium/Large) | ✅ | Resource scaling |
| 7 | Compliance Maturity | Select (Initial→Optimized) | ✅ | Implementation planning |
| 8 | Is Critical Infrastructure | Checkbox | ❌ | NCA-CSCC triggers |
| 9 | Third-Party Vendors | Textarea | ❌ | Vendor risk context |

#### Database Consequences:

```sql
INSERT INTO OrganizationProfiles (
    Id,
    TenantId,
    OrganizationType,
    Sector,
    Country,
    HostingModel,
    DataTypes,
    OrganizationSize,
    ComplianceMaturity,
    IsCriticalInfrastructure,
    Vendors,
    OnboardingQuestionsJson,  -- Full form data as JSON
    OnboardingStatus,         -- 'InProgress'
    OnboardingStartedAt,
    OnboardingProgressPercent,
    LastScopeDerivedAt
) VALUES (...)
```

#### Actions Triggered:
- ✅ OrganizationProfile persisted to database
- ✅ TempData.TenantId set for next step
- ✅ Rules Engine scope derivation queued

---

### Step 3: Review Scope (`/Onboarding/ReviewScope`)

**View:** `Views/Onboarding/ReviewScope.cshtml`  
**Progress:** 75%

#### Display (Read-Only):

| Section | Content |
|---------|---------|
| Applicable Baselines | Cards showing BaselineCode, Reason, Est. Controls |
| Recommended Packages | Cards showing PackageCode, Reason, Est. Controls |
| Recommended Templates | Cards showing TemplateCode, Reason, Sections |
| Scope Summary | Count of baselines, packages, templates, total controls |
| Agreement Checkbox | Consent to proceed with derived scope |

#### Database Processing (Rules Engine):

```
1. LOAD OrganizationProfile WHERE TenantId = @tenantId
2. LOAD Active Ruleset WHERE Status = 'Active'
3. AGGREGATE Asset characteristics (if any)
4. BUILD evaluation context (30+ fields)
5. FOR EACH Rule IN Rules ORDER BY Priority:
   - EVALUATE ConditionJson against context
   - IF matched: COLLECT ActionsJson
6. CLEAR existing TenantBaselines/Packages/Templates
7. INSERT derived scope:
   - TenantBaselines (BaselineCode, ReasonJson)
   - TenantPackages (PackageCode, ReasonJson)
   - TenantTemplates (TemplateCode, ReasonJson)
8. INSERT RuleExecutionLog (audit trail)
```

#### Example Rule Evaluation:

**Input (Context):**
```json
{
    "country": "SA",
    "sector": "Banking",
    "dataTypes": "PII,Financial",
    "hostingModel": "Cloud",
    "isCriticalInfrastructure": "true"
}
```

**Rules Matched:**
| Rule ID | Condition | Actions |
|---------|-----------|---------|
| `RULE_SECTOR_BANKING` | sector == "Banking" | SAMA_CSF, NCA_ECC |
| `RULE_KSA_JURISDICTION` | country == "SA" | NCA_ECC |
| `RULE_DATA_PERSONAL` | dataTypes contains "PII" | PDPL |

**Output (TenantBaselines):**
- SAMA_CSF (Banking sector regulated by SAMA)
- NCA_ECC (Saudi Arabia jurisdiction)
- PDPL (Processes personal data)

---

### Step 4: Create Plan (`/Onboarding/CreatePlan`)

**View:** `Views/Onboarding/CreatePlan.cshtml`  
**Progress:** 100%

#### Questions Asked:

| # | Field | Type | Required | Purpose |
|---|-------|------|----------|---------|
| 1 | Plan Name | Text Input | ✅ | Identifier |
| 2 | Description | Textarea | ❌ | Documentation |
| 3 | Start Date | Date Picker | ✅ | Timeline |
| 4 | Target End Date | Date Picker | ✅ | Deadline |

#### Database Consequences:

```sql
INSERT INTO Plans (
    Id,
    TenantId,
    PlanCode,
    Name,
    Description,
    PlanType,         -- 'Initial'
    Status,           -- 'Active'
    StartDate,
    TargetEndDate,
    CreatedAt
) VALUES (...)

UPDATE Tenants SET
    GrcPlanId = @planId,
    OnboardingStatus = 'COMPLETED',
    OnboardingCompletedAt = GETUTCDATE()
WHERE Id = @tenantId
```

---

## 3. 12-Step Wizard (Comprehensive)

### Wizard Entry Point

**Route:** `/OnboardingWizard/Index?tenantId={guid}`  
**Auth:** Requires Tenant Admin authentication

### Section Overview

| Step | Section | Name | Questions | Focus Area |
|------|---------|------|-----------|------------|
| 1 | A | Organization Identity | 13 | Legal names, country, timezone, email domains, sector |
| 2 | B | Assurance Objective | 5 | Primary driver, timeline, pain points, maturity target |
| 3 | C | Regulatory Applicability | 7 | Regulators, frameworks, certifications |
| 4 | D | Scope Definition | 9 | Legal entities, systems, processes, environments |
| 5 | E | Data & Risk Profile | 7 | Data types, PCI, cross-border, internet-facing |
| 6 | F | Technology Landscape | 13 | IDP, ITSM, SIEM, cloud providers, ERP, CMDB |
| 7 | G | Control Ownership | 8 | Ownership approach, approvers, risk committee |
| 8 | H | Teams & Roles | 10 | Org admins, teams, RACI, delegation |
| 9 | I | Workflow Cadence | 11 | Evidence frequency, SLAs, escalation |
| 10 | J | Evidence Standards | 9 | Naming, retention, access rules, sampling |
| 11 | K | Baseline & Overlays | 4 | Default baseline, overlay selection |
| 12 | L | Go-Live & Metrics | 6 | Success metrics, baselines, pilot scope |

**Total:** 96+ questions

---

### Section A: Organization Identity (Questions 1-13)

**View:** `Views/OnboardingWizard/StepA.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 1 | Legal Name (EN) | Text | ✅ | Official organization name |
| 2 | Legal Name (AR) | Text | ❌ | Arabic name |
| 3 | Trade Name | Text | ❌ | Brand name if different |
| 4 | Country of Incorporation | Select | ✅ | Primary jurisdiction |
| 5 | Operating Countries | Multi-Select | ❌ | All jurisdictions |
| 6 | Primary HQ Location | Text | ✅ | Headquarters |
| 7 | Timezone | Select | ✅ | Default timezone |
| 8 | Primary Language | Select | ✅ | English/Arabic/Bilingual |
| 9 | Corporate Email Domains | Array | ✅ | Allowed email domains |
| 10 | Domain Verification | Select | ❌ | DNS TXT or Admin Email |
| 11 | Organization Type | Select | ✅ | Enterprise/SME/Government/etc. |
| 12 | Industry/Sector | Multi-Select | ✅ | Banking/Healthcare/etc. |
| 13 | Data Residency Required | Boolean + Countries | ❌ | Compliance requirement |

#### Database Consequences:
```sql
UPDATE OnboardingWizards SET
    SectionA_Json = @jsonData,
    CurrentStep = 1,
    ProgressPercent = @progress,
    LastUpdatedAt = GETUTCDATE()
WHERE TenantId = @tenantId

UPDATE OrganizationProfiles SET
    LegalEntityName = @legalNameEn,
    LegalEntityNameAr = @legalNameAr,
    Country = @country,
    -- ... other mapped fields
WHERE TenantId = @tenantId
```

---

### Section B: Assurance Objective (Questions 14-18)

**View:** `Views/OnboardingWizard/StepB.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 14 | Primary Driver | Select | ✅ | Regulator Exam/Internal Audit/Certification/etc. |
| 15 | Target Date | Date | ❌ | Go-live or audit date |
| 16 | Target Milestone | Select | ❌ | GoLive/AuditDate/CertificationDate |
| 17 | Current Pain Points | Ranked List (Top 3) | ❌ | Evidence Collection/Mapping/Remediation/etc. |
| 18 | Desired Maturity | Select | ❌ | Foundation/AssuranceOps/ContinuousAssurance |
| 19 | Reporting Audience | Multi-Select | ❌ | Board/CRO/CISO/Regulators |

---

### Section C: Regulatory Applicability (Questions 19-25)

**View:** `Views/OnboardingWizard/StepC.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 19 | Primary Regulators | Array (Jurisdiction + Regulator) | Conditional | NCA/SAMA/CITC/CMA |
| 20 | Secondary Regulators | Multi-Select | ❌ | Additional oversight |
| 21 | Mandatory Frameworks | Multi-Select | Conditional | NCA-ECC/SAMA-CSF/PDPL/ISO27001/PCI-DSS |
| 22 | Benchmarking Frameworks | Multi-Select | ❌ | Optional standards |
| 23 | Internal Policies | Array | ❌ | ISMS/IT Policies |
| 24 | Certifications Held | Array (Type + Dates + Body) | ❌ | ISO27001/SOC2/etc. |
| 25 | Audit Scope Type | Select | ❌ | Enterprise/BusinessUnit/System/Process |

---

### Section D: Scope Definition (Questions 26-34)

**View:** `Views/OnboardingWizard/StepD.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 26 | In-Scope Legal Entities | Array | ✅ | Entity name, registration, country |
| 27 | In-Scope Business Units | Array | ✅ | Unit code, name, owner |
| 28 | In-Scope Systems | Array | ✅ | System code, type, criticality, owner |
| 29 | In-Scope Processes | Multi-Select | ✅ | Onboarding/Payments/IncidentResponse/etc. |
| 30 | In-Scope Environments | Multi-Select | ❌ | Production/NonProduction/Both |
| 31 | In-Scope Locations | Array | ✅ | DataCenter/Office/CloudRegion |
| 32 | Criticality Tiers | Array | ❌ | Tier definitions with RTO/RPO |
| 33 | Important Business Services | Array | ❌ | Critical services list |
| 34 | Exclusions | Array | ✅ | Explicitly out of scope with rationale |

---

### Section E: Data & Risk Profile (Questions 35-40)

**View:** `Views/OnboardingWizard/StepE.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 35 | Data Types Processed | Multi-Select | ✅ | PII/PCI/PHI/Confidential/Classified |
| 36 | Payment Card Data | Boolean + Systems + Processes | ✅ | PCI-DSS trigger |
| 37 | Cross-Border Transfers | Boolean + Transfer Details | ✅ | PDPL Article 29 |
| 38 | Customer Volume Tier | Select | ❌ | <10K/10K-100K/100K-1M/1M+ |
| 39 | Internet-Facing Systems | Boolean + System List | ✅ | External attack surface |
| 40 | Third-Party Processing | Boolean + Processor Details | ❌ | Vendor risk context |

---

### Section F: Technology Landscape (Questions 41-53)

**View:** `Views/OnboardingWizard/StepF.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 41 | Identity Provider | Select | ✅ | AzureAD/Okta/PingIdentity/ADFS |
| 42 | SSO Enabled | Boolean + Protocol | ✅ | SAML/OIDC integration |
| 43 | SCIM Provisioning | Boolean | ❌ | Automated user sync |
| 44 | ITSM Platform | Select | ✅ | ServiceNow/Jira/Freshservice |
| 45 | Evidence Repository | Select | ❌ | SharePoint/GrcVault/GoogleDrive |
| 46 | SIEM Platform | Select | ✅ | Sentinel/Splunk/QRadar/Elastic |
| 47 | Vulnerability Management | Select | ✅ | Tenable/Qualys/Rapid7 |
| 48 | EDR Platform | Select | ✅ | Defender/CrowdStrike/SentinelOne |
| 49 | Cloud Providers | Multi-Select | ✅ | AWS/Azure/GCP/AliCloud/OnPremise |
| 50 | ERP Platform | Select | ✅ | SAP/Oracle/Dynamics/None |
| 51 | CMDB Source | Select | ❌ | ServiceNowCMDB/CloudInventory/Custom |
| 52 | CI/CD Tools | Multi-Select | ❌ | GitHub/GitLab/AzureDevOps/Jenkins |
| 53 | Backup/DR Tooling | Text | ❌ | DR infrastructure |

---

### Section G: Control Ownership (Questions 54-60)

**View:** `Views/OnboardingWizard/StepG.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 54 | Ownership Approach | Select | ✅ | Centralized/Federated/Hybrid |
| 55 | Default Owner Team | Text + Email | ❌ | Fallback owner |
| 56 | Exception Approver | Role + Title | ❌ | Exception workflow |
| 57 | Regulatory Approver | Role + Title | ❌ | Mapping approval |
| 58 | Effectiveness Signoff | Role + Title | ❌ | Control sign-off |
| 59 | Internal Audit Contact | Name + Email + Role | ❌ | Audit stakeholder |
| 60 | Risk Committee | Cadence + Attendees | ❌ | Governance structure |

---

### Section H: Teams & Roles (Questions 61-70)

**View:** `Views/OnboardingWizard/StepH.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 61 | Org Admins | Array (Name + Email) | ✅ | System administrators |
| 62 | Create Teams Now | Boolean | ❌ | Immediate team creation |
| 63 | Team Definitions | Array (Code + Name + Owner) | Conditional | Team structure |
| 64 | Team Members | Array (Team + Name + Role) | ❌ | User assignments |
| 65 | Role Catalog | Multi-Select | ✅ | ControlOwner/Approver/Assessor/etc. |
| 66 | RACI Mapping Needed | Boolean + Mappings | ✅ | Responsibility matrix |
| 67 | Approval Gates | Boolean + Gate Config | ❌ | Workflow gates |
| 68 | Delegation Rules | Array | ❌ | OOO handling |
| 69 | Notification Channels | Multi-Select | ✅ | Teams/Email/Slack |
| 70 | Escalation Path | Days + Target | ✅ | Escalation rules |

---

### Section I: Workflow Cadence (Questions 71-80)

**View:** `Views/OnboardingWizard/StepI.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 71 | Evidence Frequency by Domain | Dictionary | ✅ | AccessControl: Quarterly, etc. |
| 72 | Access Reviews Frequency | Select | ❌ | Quarterly/SemiAnnual/Annual |
| 73 | Vulnerability Review Frequency | Select | ❌ | Monthly/Quarterly |
| 74 | Backup Review Frequency | Select + Restore Test | ❌ | Monthly/Quarterly |
| 75 | DR Exercise Cadence | Select | ❌ | Annual/SemiAnnual |
| 76 | Incident Tabletop Cadence | Select | ❌ | SemiAnnual/Annual |
| 77 | Evidence Submit SLA | Number (days) | ✅ | Due date compliance |
| 78 | Remediation SLA by Severity | Dictionary | ✅ | Critical: 7, High: 14, etc. |
| 79 | Exception Expiry Default | Number (days) | ✅ | Auto-expiration period |
| 80 | Audit Request Handling | Select | ❌ | SingleQueue/PerDomainQueue |

---

### Section J: Evidence Standards (Questions 81-87)

**View:** `Views/OnboardingWizard/StepJ.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 81 | Naming Convention Required | Boolean + Pattern | ❌ | File naming rules |
| 82 | Storage Location by Domain | Dictionary | ❌ | Where to store evidence |
| 83 | Retention Period | Number (years) | ✅ | Compliance requirement |
| 84 | Access Rules | Object (Viewer/Approver/Upload roles) | ❌ | Permission matrix |
| 85 | Acceptable Evidence Types | Multi-Select | ❌ | Reports/Logs/Screenshots/etc. |
| 86 | Sampling Guidance | Object (Method + Size) | ❌ | Audit sampling |
| 87 | Confidential Handling | Object (Encryption/Access/Watermark) | ❌ | Sensitive evidence |

---

### Section K: Baseline & Overlays (Questions 88-90)

**View:** `Views/OnboardingWizard/StepK.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 88 | Adopt Default Baseline | Boolean + Code | ✅ | Use standard control set |
| 89 | Overlay Selections | Object (Jurisdiction/Sector/Data/Tech) | ❌ | Additional controls |
| 90 | Custom Requirements | Boolean + File + Array | ❌ | Client-specific controls |

---

### Section L: Go-Live & Metrics (Questions 91-96)

**View:** `Views/OnboardingWizard/StepL.cshtml`

| Q# | Field | Type | Required | Purpose |
|----|-------|------|----------|---------|
| 91 | Success Metrics | Multi-Select (Top 3) | ✅ | FewerAuditHours/FasterTurnaround/etc. |
| 92 | Current Audit Prep Hours | Number | ❌ | Baseline measurement |
| 93 | Current Remediation Closure | Number (days) | ❌ | Baseline measurement |
| 94 | Current Overdue Controls | Number | ❌ | Baseline measurement |
| 95 | Target Improvement % | Dictionary | ❌ | Success targets |
| 96 | Pilot Scope | Domains + Count | ❌ | Initial rollout |

---

## 4. Database Tables Affected

### Primary Tables

| Table | Created During | Key Fields |
|-------|----------------|------------|
| `Tenants` | Signup | Id, TenantSlug, Status, OnboardingStatus |
| `OrganizationProfiles` | OrgProfile/StepA | TenantId, Sector, DataTypes, ComplianceMaturity |
| `OnboardingWizards` | StepA | TenantId, CurrentStep, ProgressPercent, SectionA-L_Json |
| `TenantBaselines` | ReviewScope | TenantId, BaselineCode, ReasonJson |
| `TenantPackages` | ReviewScope | TenantId, PackageCode, ReasonJson |
| `TenantTemplates` | ReviewScope | TenantId, TemplateCode, ReasonJson |
| `Plans` | CreatePlan/Finalize | TenantId, PlanCode, Status, StartDate |
| `RuleExecutionLogs` | ReviewScope | TenantId, ExecutedAt, Status, DerivedScopeJson |

### Secondary Tables (Finalization)

| Table | Created During | Purpose |
|-------|----------------|---------|
| `Workspaces` | Finalization | Default workspace for tenant |
| `Teams` | Finalization (if CreateTeamsNow) | Team structure |
| `TeamMembers` | Finalization | User-team assignments |
| `RACIAssignments` | Finalization (if RaciMappingNeeded) | Responsibility matrix |
| `Assessments` | Background provisioning | Initial assessments |
| `TenantEvidenceRequirements` | Background provisioning | Evidence collection schedule |

---

## 5. Services Involved

### Core Services

| Service | Interface | Purpose |
|---------|-----------|---------|
| `OnboardingService` | `IOnboardingService` | Orchestrates onboarding flow |
| `Phase1RulesEngineService` | `IRulesEngineService` | Evaluates rules, derives scope |
| `TenantOnboardingProvisioner` | `ITenantOnboardingProvisioner` | Creates workspace, templates, plans |
| `OwnerSetupService` | `IOwnerSetupService` | Platform owner registration |
| `OwnerTenantService` | `IOwnerTenantService` | Tenant lifecycle management |

### Supporting Services

| Service | Purpose |
|---------|---------|
| `NotificationService` | Sends activation/invitation emails |
| `WorkspaceManagementService` | Creates/manages workspaces |
| `PlanService` | Creates GRC plans |
| `SerialNumberService` | Generates unique identifiers |
| `GrcCachingService` | Caches derived scope |

---

## 6. Finalization Consequences

### Phase 1: Synchronous (User Waits)

```
1. SET OnboardingWizard.WizardStatus = "Processing"
2. SYNC OrganizationProfile with wizard data
3. CREATE Workspace (idempotent - exactly ONE per tenant)
   - INSERT INTO Workspaces (TenantId, WorkspaceCode, IsDefault=true)
   - UPDATE Tenants SET DefaultWorkspaceId = @workspaceId
4. CREATE Teams (if CreateTeamsNow = true)
   - FOR EACH team IN wizard.Teams:
     - INSERT INTO Teams (TenantId, TeamCode, TeamName, OwnerEmail)
5. CREATE RACIAssignments (if RaciMappingNeeded = true)
   - FOR EACH mapping IN wizard.RaciMappings:
     - INSERT INTO RACIAssignments (TenantId, ScopeType, Responsible, Accountable, ...)
6. SET OnboardingWizard.WizardStatus = "Completed"
7. REDIRECT to Dashboard
```

### Phase 2: Asynchronous (Background)

```
1. CREATE TenantTemplate (100Q baseline assessment)
   - INSERT INTO TenantTemplates (TenantId, TemplateCode='100Q-BASELINE', ...)
   - UPDATE Tenants SET AssessmentTemplateId = @templateId
2. CREATE Plan (initial GRC plan)
   - INSERT INTO Plans (TenantId, PlanCode, Status='Active', ...)
   - UPDATE Tenants SET GrcPlanId = @planId
3. CREATE Assessment(s) (1-3 linked to template)
   - INSERT INTO Assessments (TenantId, PlanId, TemplateCode, Status='Draft')
4. ACTIVATE Workflows (3-5 default workflows)
   - INSERT INTO WorkflowInstances (TenantId, WorkflowType, Status='Active')
5. CONFIGURE RoleLandingPages (6 role-specific dashboards)
   - INSERT INTO RoleLandingConfigs (TenantId, RoleCode, LandingPage)
6. CREATE TenantEvidenceRequirements (50-200 per framework)
   - INSERT INTO TenantEvidenceRequirements (TenantId, ControlCode, Frequency, ...)
7. UPDATE Tenants SET OnboardingStatus = 'COMPLETED', OnboardingCompletedAt = GETUTCDATE()
8. SEND User invitation emails (via NotificationService)
```

---

## 7. UI Components

### Views Structure

```
Views/
├── Onboarding/
│   ├── Index.cshtml          # Landing with 3 buttons
│   ├── Signup.cshtml         # Step 1: Registration form
│   ├── Activate.cshtml       # Email activation
│   ├── OrgProfile.cshtml     # Step 2: Organization profile
│   ├── ReviewScope.cshtml    # Step 3: Derived scope display
│   ├── CreatePlan.cshtml     # Step 4: Plan creation
│   └── GuidedWelcome.cshtml  # Post-onboarding welcome
├── OnboardingWizard/
│   ├── StepA.cshtml          # Organization Identity
│   ├── StepB.cshtml          # Assurance Objective
│   ├── StepC.cshtml          # Regulatory Applicability
│   ├── StepD.cshtml          # Scope Definition
│   ├── StepE.cshtml          # Data & Risk Profile
│   ├── StepF.cshtml          # Technology Landscape
│   ├── StepG.cshtml          # Control Ownership
│   ├── StepH.cshtml          # Teams & Roles
│   ├── StepI.cshtml          # Workflow Cadence
│   ├── StepJ.cshtml          # Evidence Standards
│   ├── StepK.cshtml          # Baseline & Overlays
│   ├── StepL.cshtml          # Go-Live & Metrics
│   ├── Summary.cshtml        # Progress summary
│   ├── Complete.cshtml       # Finalization confirmation
│   ├── _WizardSidebar.cshtml # Navigation sidebar
│   └── _WizardNavButtons.cshtml # Prev/Next buttons
└── OrgSetup/
    ├── Index.cshtml          # Post-onboarding setup
    ├── Teams.cshtml          # Team management
    ├── CreateTeam.cshtml     # Add new team
    ├── EditTeam.cshtml       # Modify team
    ├── TeamMembers.cshtml    # Member management
    ├── Users.cshtml          # User management
    └── RACI.cshtml           # RACI matrix
```

---

## 8. Expert-Driven Framework Mapping

The OrgProfile view includes client-side sector blueprints that pre-populate applicable frameworks:

| Sector | Frameworks Applied | Controls |
|--------|-------------------|----------|
| Banking | SAMA-CSF, NCA-ECC, PDPL, SAMA-AML, PCI-DSS | ~400 |
| Healthcare | NCA-ECC, PDPL, SFDA, MOH-HIS | ~300 |
| Government | NCA-ECC, NCA-CSCC, DGA-CLOUD, PDPL, NDMO | ~400 |
| Telecom | CST-CRF, NCA-ECC, NCA-CSCC, PDPL | ~300 |
| Energy | NCA-ECC, NCA-CSCC, HCIS, PDPL | ~280 |
| Retail | PDPL, NCA-ECC, PCI-DSS, MOCI-ECOM | ~260 |
| Technology | NCA-ECC, PDPL, CST-CLOUD, ISO27001 | ~320 |

---

## 9. Validation Rules

### Simple Flow Validation

| Step | Required Fields | Validation |
|------|-----------------|------------|
| Signup | OrganizationName, AdminEmail, SubscriptionTier, Country, All Checkboxes | Email format, unique slug |
| OrgProfile | OrganizationType, Sector, Country, HostingModel, DataTypes, Size, Maturity | At least 1 data type |
| ReviewScope | Agree checkbox | Scope must have ≥1 baseline |
| CreatePlan | PlanName, StartDate, TargetEndDate | EndDate > StartDate |

### Wizard Validation (Minimal Required Set)

| Section | Required Questions |
|---------|-------------------|
| A | A1 (LegalNameEn), A3 (Country), A5 (HQ), A6 (Timezone), A7 (Language), A8 (Domains), A10 (OrgType), A11 (Sectors) |
| D | D26 (Entities), D27 (BUs), D28 (Systems), D29 (Processes), D31 (Locations) |
| E | E35 (DataTypes) |
| F | F41 (IDP), F44 (ITSM), F49 (Cloud) |
| H | H61 (OrgAdmins), H65 (Roles), H69 (Notifications) |
| I | I77 (EvidenceSLA), I78 (RemediationSLA) |

---

## 10. Error Handling

| Scenario | Handling |
|----------|----------|
| No TenantId in TempData | Redirect to Signup |
| No active Ruleset | Fallback to default baselines (NCA-ECC, PDPL) |
| OrganizationProfile not found | Redirect to OrgProfile with error |
| Wizard already completed | Redirect to Dashboard with info message |
| Workspace already exists | Idempotent - skip creation |
| Team creation fails | Log error, continue with other teams |
| Background provisioning fails | Retry queue, admin notification |

---

## 11. Summary Statistics

| Metric | Simple Flow | 12-Step Wizard |
|--------|-------------|----------------|
| Total Questions | 13+ | 96+ |
| Required Questions | 10 | 40+ |
| Database Tables Affected | 8 | 15+ |
| Estimated Completion Time | 10-15 min | 45-60 min |
| Background Jobs Triggered | 3-5 | 10-15 |
| Email Notifications | 2-3 | 5-10 |

---

## 12. Status Tracking

### Tenant Status Values

| Status | Meaning |
|--------|---------|
| `Pending` | Awaiting admin activation |
| `Active` | Activated and usable |
| `Suspended` | Temporarily disabled |
| `Deleted` | Soft deleted |

### OnboardingStatus Values

| Status | Meaning |
|--------|---------|
| `NOT_STARTED` | Signup complete, no profile |
| `IN_PROGRESS` | Profile partially complete |
| `FAILED` | Error during finalization |
| `COMPLETED` | All steps done |

### WizardStatus Values

| Status | Meaning |
|--------|---------|
| `NotStarted` | Wizard never accessed |
| `InProgress` | Actively filling sections |
| `Processing` | Finalization in progress |
| `Completed` | All sections done, provisioned |
| `Failed` | Error during finalization |

---

*This report documents the complete onboarding process without modifying any data.*
