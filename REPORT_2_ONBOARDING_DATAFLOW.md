# REPORT 2: ONBOARDING & DATA FLOW REQUIREMENTS

## Executive Summary
This report details the complete onboarding journey, data collection requirements, system interactions, and role-profile assignments needed to configure a tenant for multi-framework compliance management.

---

## SECTION 1: COMPLETE ONBOARDING FLOW (8 STEPS)

### Current Implementation (Steps 1-4)

#### STEP 1: User Signup
**Current**:
- Email, Password, Full Name
- Create ApplicationUser
- Send verification email

**Missing**:
- Company name field
- Phone number
- Country selection
- Industry selection
- Estimated user count

**Data Created**:
- ApplicationUser
- Initial TenantId (PENDING Step 2)

**Workflow Trigger**: Step 1 Complete → Proceed to Step 2

---

#### STEP 2: Organization Profile
**Current**:
- Org name, type, sector
- Country, hosting model, size
- Maturity level, data types, vendors

**Missing**:
- Business registration number
- Tax ID
- Compliance officer contact
- Technical contact
- Legal entity structure
- Fiscal year (for reporting)
- Critical infrastructure flag
- Data residency requirements

**Data Created**:
- OrganizationProfile
- Tenant record updated

**Workflow Trigger**: Step 2 Complete → Auto-derive frameworks (Step 3)

---

#### STEP 3: Compliance Scope Derivation
**Current**:
- Auto-select frameworks based on:
  - Country (mandatory frameworks)
  - Sector (industry-specific)
  - Maturity goal
- Display applicable baselines

**Missing**:
- Document applicable regulations per country
- Calculate total controls affected (500+)
- Estimate implementation timeline
- Cost estimation per framework
- Risk assessment pre-baseline

**Data Created**:
- TenantBaseline (per framework)
- ComplianceScope
- FrameworkVersion (effective date)

**Workflow Trigger**: Step 3 Complete → Create implementation plan (Step 4)

---

#### STEP 4: Initial Plan Creation
**Current**:
- Create Assessment Plan
- Set phase schedule
- Assign initial control owners

**Missing**:
- Timeline estimation (3, 6, 12 months)
- Resource allocation
- Budget estimation
- Milestone definitions
- Communication plan
- Training schedule

**Data Created**:
- AssessmentPlan
- WorkflowInstance (control assessment)
- Initial RiskAssessment

**Workflow Trigger**: Step 4 Complete → Proceed to HRIS setup (Step 5)

---

### NEW REQUIRED STEPS (Steps 5-8)

#### STEP 5: HRIS Integration Setup
**Purpose**: Sync employees, create user accounts, assign roles

**Data Collection**:
```
HRIS Connection Details:
  - HR System Type (SAP, Workday, ADP, Bamboo, Custom)
  - API Endpoint URL
  - Authentication Type (OAuth2, API Key, Basic Auth)
  - Client ID / API Key / Username
  - Password (encrypted)
  
Data Mapping:
  - Employee ID field name
  - First Name field name
  - Last Name field name
  - Email field name
  - Department field name
  - Job Title field name
  - Manager ID field name
  - Activation/Termination date fields
  
Employee Import Rules:
  - Active employees only? (Yes/No)
  - Include terminated employees? (Yes/No)
  - Date range for import
  - Department filter (if needed)
```

**System Actions**:
```
1. Establish HRIS connection
2. Test API connectivity
3. Fetch employee list (100+ employees)
4. Map job titles to roles (auto-suggest):
   - CTO → Technology Manager
   - Security Officer → Risk Manager
   - Compliance Officer → Compliance Manager
   - etc.
5. Create ApplicationUser for each employee
6. Assign initial RoleProfile based on job title
7. Set department from HRIS
8. Create UserWorkspace filtered by role scope
9. Schedule daily sync (every 6 hours)
10. Log all created accounts
```

**Data Created**:
- HRISIntegration record
- HRISEmployee (100+ records)
- ApplicationUser (100+ accounts)
- UserRoleProfile assignments
- BackgroundJob (daily sync)
- HRISLog (sync history)

**Workflow Triggers**: 
- HRIS connection successful → Proceed to Step 6
- HRIS sync complete → Auto-assign control owners
- Daily HRIS sync → Update employees/roles

**Impact on Other Modules**:
- Workflows: Now have actual users to assign tasks
- Assessments: Control owners assigned from HRIS
- Audits: Auditors identified from HRIS
- Reports: User access per department

---

#### STEP 6: Regulatory Framework & Control Selection
**Purpose**: Select applicable compliance frameworks, define control baselines

**Data Collection**:
```
Regulatory Requirements:
  - Primary country of operation
  - Secondary countries (if multi-regional)
  - Industries (Healthcare, Finance, Tech, etc.)
  - Business size (Startup, SMB, Enterprise)
  - Data types processed (PII, PHI, Payment data, etc.)
  
Compliance Requirements (Multi-select):
  - ISO 27001 (Information Security Management)?
  - NIST Cybersecurity Framework (CSF)?
  - SOC 2 Type II (Service Organization Control)?
  - GDPR (General Data Protection Regulation)?
  - HIPAA (Healthcare data)?
  - PCI-DSS (Payment card data)?
  - SAMA CSF (Saudi Arabia)?
  - PDPL (Saudi Privacy)?
  - MOI/NRA (Saudi Cybersecurity)?
  
Compliance Maturity Goals:
  - Current maturity (1-5)
  - Target maturity (1-5)
  - Timeline to achieve (months)
  
Risk Appetite:
  - Risk tolerance (Low/Medium/High)
  - Critical infrastructure (Yes/No)
  - Regulatory oversight (Yes/No)
  - Third-party requirements (Yes/No)
```

**System Actions**:
```
1. Map country → Mandatory frameworks
   - Saudi Arabia → SAMA, PDPL, MOI, VAT (ZATCA)
   - USA → NIST, SOC2, HIPAA (if healthcare)
   - EU → GDPR, NIS2, sector-specific (TISAX, etc.)
   
2. Map sector → Industry frameworks
   - Healthcare → HIPAA, HITRUST
   - Finance → SOC2, PCI-DSS, GLBA
   - Tech/SaaS → SOC2, ISO27001
   - Telecom → NCA, SAMA
   
3. Calculate total controls (500+):
   - ISO 27001: 114 controls
   - NIST CSF: 176 subcategories
   - GDPR: 99+ requirements
   - SOC2: 64 controls
   - HIPAA: 164 requirements
   - SAMA: 42 control areas
   - PDPL: 50+ requirements
   
4. Create baseline per framework:
   - Select control subset (critical controls only)
   - Map controls to maturity levels
   - Define testing frequency per control
   
5. Generate implementation timeline:
   - Phase 1 (Critical controls): 3 months
   - Phase 2 (High controls): 6 months
   - Phase 3 (Medium controls): 12 months
   - Phase 4 (Low controls): 18+ months
   
6. Create assessment templates per framework
   
7. Schedule initial assessment (90 days from now)
```

**Data Created**:
- TenantFramework (1-9 records per tenant)
- Control (500+ records)
- ControlVersion (per framework version)
- ControlBaseline (curated subset)
- Baseline (compliance baseline)
- AssessmentTemplate (per framework)
- RiskRegister (pre-baseline risks)
- ComplianceProject (implementation plan)

**Workflow Triggers**:
- Framework selected → Create 500+ control records
- Control created → Auto-assign default owner
- Baseline created → Create assessment plan
- Assessment scheduled → Create workflow instance

**Impact on Other Modules**:
- Assessments: Now have control requirements per framework
- Audits: Audit procedures derived from controls
- Risks: Risk library populated per controls
- Reports: Compliance dashboard shows frameworks

---

#### STEP 7: Control Ownership & Assignment
**Purpose**: Define who is responsible for each control, create assessment workflows

**Data Collection**:
```
Per Control (500+ selections):
  - Owner (from HRIS employee list)
    - Department
    - Role
  - Alternative owner (backup)
  - Testing responsibility (same owner or different?)
  - Evidence collection responsibility
  - Approval authority (who signs off)
  
Control Categorization:
  - Control type (Preventive, Detective, Corrective)
  - Risk if not implemented (Critical/High/Medium/Low)
  - Testing frequency (Continuous/Daily/Monthly/Quarterly/Annually)
  - Evidence types (2-5 required types)
  - Acceptable evidence age (days)
  
Department-level Decisions:
  - Per department: How often to test controls?
  - Per department: Evidence retention period?
  - Per department: Who approves evidence?
  - Per department: Escalation process?
```

**System Actions**:
```
1. Auto-assign controls by category:
   - Policy controls → Compliance Manager
   - Technical controls → CTO / Security Manager
   - Administrative controls → HR Manager
   - Physical controls → Facility Manager
   - Operational controls → Process Owner
   
2. Create ControlOwnershipMatrix:
   - 500+ controls × Departments × Roles
   - Defines RACI (Responsible, Accountable, Consulted, Informed)
   
3. Create assessment tasks:
   - For each control: Create AssessmentTask
   - Assign to owner with due date
   - Set evidence requirements
   - Set testing procedures
   
4. Create testing schedule:
   - Monthly controls: 12 tests/year
   - Quarterly controls: 4 tests/year
   - Annual controls: 1 test/year
   - Continuous controls: Monitored 24/7
   
5. Create workflow instances:
   - Control Assessment workflow per control
   - Evidence Collection workflow per framework
   - Approval workflow for control changes
   
6. Define escalation rules:
   - Non-compliant → Escalate to manager
   - Overdue test → Escalate to owner
   - Failed test → Escalate to CRO
   
7. Create notification rules:
   - Control owner: Test reminder (14 days before)
   - Manager: Overdue notification (7 days)
   - CRO: Critical issues (immediate)
```

**Data Created**:
- ControlOwnership (500+ records)
- WorkflowInstance (500+ assessment workflows)
- TaskAssignment (500+ initial tasks)
- EscalationRule (5-10 rules)
- NotificationRule (10-15 rules)
- TestingSchedule (calendar entries)

**Workflow Triggers**:
- Control owner assigned → Create assessment task
- Task created → Send notification to owner
- Testing date → Auto-generate test procedure
- Test due → Escalate if overdue

**Impact on Other Modules**:
- Workflows: Now populated with 500+ assessment tasks
- Assessments: Control owners can perform assessments
- Audits: Audit scope includes 500+ controls
- Reports: RACI matrix visible in compliance reports

---

#### STEP 8: Evidence Source Configuration
**Purpose**: Set up evidence collection from internal and external systems

**Data Collection**:
```
Document Repositories:
  - SharePoint instance (URL, credentials)
  - Google Drive folder (OAuth token)
  - OneDrive folder (Azure AD)
  - Local file server (path, credentials)
  - Custom repository (API details)
  
Policy Documents (User Upload):
  - Information Security Policy
  - Data Protection Policy
  - Incident Response Plan
  - Business Continuity Plan
  - Access Control Policy
  - Password Policy
  - etc. (10-20 policies)
  
Third-party Evidence Sources:
  - Cloud provider (AWS, Azure, Google Cloud)
  - Backup system (Veeam, Commvault, etc.)
  - Monitoring tool (Splunk, DataDog, etc.)
  - Vulnerability scanner (Tenable, Qualys, etc.)
  - Ticketing system (Jira, ServiceNow, etc.)
  - Identity provider (Okta, Azure AD, etc.)
  
Automation Connectors Setup:
  - API endpoints for auto-collection
  - Credentials/tokens (encrypted)
  - Collection frequency (hourly/daily/weekly)
  - Data mapping rules
  - Filtering rules (date range, status, etc.)
  
Evidence Expiration Rules:
  - Policy documents: 1 year expiration
  - Audit reports: 3 years retention
  - System logs: 90 days retention
  - Test results: 1 year retention
  - Risk assessments: 1 year retention
  
Evidence Linking Rules:
  - Map uploaded policies to controls
  - Link audit reports to frameworks
  - Connect logs to controls
  - Associate test results to controls
```

**System Actions**:
```
1. Index document repositories:
   - Connect to SharePoint → Index 100+ documents
   - Connect to Google Drive → Index 50+ documents
   - Index local file server
   - Create full-text search index
   
2. Process uploaded policies:
   - Store in Document table
   - Extract text for search
   - Auto-link to controls (keyword matching)
   - Create DocumentVersion (v1.0)
   - Schedule expiration alerts (1 year)
   
3. Set up automated evidence collection:
   - AWS CloudTrail logs → Evidence collection job
   - Azure audit logs → Evidence collection job
   - Okta logs → Evidence collection job
   - Splunk searches → Evidence collection job
   - Schedule: Every 6 hours
   
4. Create evidence collection workflows:
   - For each control type:
     - Policy controls → Evidence from repositories
     - Technical controls → Evidence from systems
     - Operational controls → Evidence from tickets
     - Test results → Evidence from assessments
   
5. Define evidence validation rules:
   - File format validation (PDF, DOC, XLSX, JSON)
   - File size limits (5MB max)
   - Expiration date validation
   - Completeness validation (all required types)
   
6. Create evidence expiration monitoring:
   - Policy: Alert 30 days before expiration
   - Logs: Auto-delete after retention period
   - Assessments: Flag stale evidence
   
7. Configure evidence linking:
   - Link policies to controls (by keyword)
   - Link audit reports to frameworks
   - Link logs to controls
   - Link test results to controls
   
8. Schedule background jobs:
   - Daily: Sync document repos
   - 6-hourly: Collect evidence from systems
   - Weekly: Validate evidence completeness
   - Monthly: Generate evidence status report
```

**Data Created**:
- EvidenceSource (5-10 source configs)
- Document (100+ policies)
- DocumentVersion (v1.0 for each)
- EvidenceCollection (2000+ collected items)
- EvidenceMapping (control-to-evidence)
- BackgroundJob (4 collection jobs)
- JobSchedule (daily/6-hourly/weekly runs)
- EvidenceExpirationRule (10+ rules)

**Workflow Triggers**:
- Evidence source connected → Start collection job
- Collection job complete → Validate evidence
- Validation complete → Map to controls
- Evidence mapped → Update control status
- Evidence expires in 30 days → Send alert
- Evidence expired → Flag for replacement

**Impact on Other Modules**:
- Assessments: Evidence available during control assessments
- Controls: Control status shows evidence completeness
- Audits: Audit scope includes evidence review
- Reports: Evidence collection status in dashboards

---

## SECTION 2: DATA FLOW DIAGRAM

```
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 1: SIGNUP                                                      │
│ Input: Email, Password, Name                                        │
│ Creates: ApplicationUser, Tenant (pending)                          │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 2: ORGANIZATION PROFILE                                        │
│ Input: Org name, country, sector, size, maturity, data types       │
│ Creates: OrganizationProfile, Updates Tenant                        │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 3: COMPLIANCE SCOPE (AUTO)                                     │
│ Rules: Country + Sector → Select Frameworks                         │
│ Creates: TenantFramework, Baseline, 500+ Controls                   │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 4: INITIAL PLAN                                                │
│ Input: Implementation timeline, phases, milestones                  │
│ Creates: AssessmentPlan, WorkflowInstances (500+ controls)          │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 5: HRIS INTEGRATION (NEW)                                      │
│ Input: HRIS details, API creds, data mapping                        │
│ Actions: Fetch employees, create accounts, assign roles             │
│ Creates: 100+ Users, RoleProfiles, sync jobs                        │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 6: FRAMEWORK SELECTION (NEW)                                   │
│ Input: Frameworks, maturity goals, risk appetite                    │
│ Actions: Derive 500+ controls per frameworks                        │
│ Creates: Control baselines, assessment templates                    │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 7: CONTROL OWNERSHIP (NEW)                                     │
│ Input: Per-control owner, testing frequency, approval authority     │
│ Actions: Assign 500+ controls to owners from HRIS                   │
│ Creates: ControlOwnership matrix, task assignments, workflows       │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STEP 8: EVIDENCE CONFIGURATION (NEW)                                │
│ Input: Document repos, policies, evidence sources, expiration rules  │
│ Actions: Set up auto-collection, link to controls                   │
│ Creates: Evidence sources, collection jobs, linking rules           │
└──────────────────────┬──────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│ ONBOARDING COMPLETE                                                 │
│ System Ready: 500+ controls assigned to 100+ users                  │
│ Evidence collection running, assessments can begin                  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## SECTION 3: ROLE PROFILE ASSIGNMENTS DURING ONBOARDING

### Roles Auto-Assigned from HRIS Job Titles

#### Executive Level (C-Suite)
```
CRO (Chief Risk Officer) → Risk Manager Profile
  - Full visibility across all risks
  - Approval authority for high-risk items
  - Access to executive dashboards
  - Receives escalations
  
CCO (Chief Compliance Officer) → Compliance Manager Profile
  - Oversight of all compliance items
  - Approval authority for control changes
  - Access to compliance reports
  - Risk acceptance authority
```

#### Management Level
```
IT Manager / CTO → Technology Manager Profile
  - Owns technical controls (50+ controls)
  - Tests technical assessments
  - Reviews security evidence
  - Approves technical changes
  
CISO / Security Officer → Security Manager Profile
  - Owns security controls (70+ controls)
  - Leads penetration testing
  - Reviews vulnerability evidence
  - Incident response authority
  
Compliance Officer → Compliance Officer Profile
  - Owns policy controls (30+ controls)
  - Manages compliance assessments
  - Reviews policy evidence
  - Audit liaison
  
Audit Manager → Audit Manager Profile
  - Plans and executes audits
  - Reviews all control assessments
  - Issues findings
  - Tracks remediation
```

#### Operational Level
```
Department Manager → Department Manager Profile
  - Owns operational controls (10-20 per department)
  - Supervises control testing
  - Approves evidence collection
  - Submits test results
  
Individual Contributor (Staff) → Staff Profile
  - Tests assigned controls
  - Submits evidence
  - Participates in assessments
  - Limited approval authority
```

### Role Profile Defines:
- **Permissions**: Which modules can access?
- **Scope**: Which controls/departments visible?
- **Approvals**: What can they approve?
- **Escalations**: Who escalates to them?
- **Reports**: Which reports can they view?
- **SLA**: What is their response time?

### Role → Scope Mapping During STEP 5
```
HRIS Job Title          → Role Profile              → Scope
─────────────────────────────────────────────────────────
CEO/CRO                 → Risk Manager              → Organization-wide
CTO/IT Manager          → Technology Manager        → IT Department
CISO/Security Officer   → Security Manager          → Security Department
Compliance Officer      → Compliance Officer        → All controls
HR Manager              → HR Manager                → HR Department
Finance Manager         → Finance Manager           → Finance Department
Operations Manager      → Operations Manager        → Operations
Quality Manager         → Quality Manager           → QA Department
(Default Staff)         → Staff Member              → Assigned controls only
```

---

## SECTION 4: MISSING DATA STRUCTURES FOR ONBOARDING

### Database Tables Needed for Complete Onboarding

```sql
-- Step 5: HRIS Integration
HRISIntegration
  - IntegrationId (PK)
  - TenantId (FK)
  - SourceSystem (SAP, Workday, ADP, etc.)
  - APIEndpoint
  - AuthType (OAuth, API Key, Basic)
  - LastSyncDate
  - NextSyncDate
  - SyncStatus (Active, Failed, Paused)

HRISEmployee
  - EmployeeId (PK)
  - TenantId (FK)
  - FirstName, LastName, Email
  - Department
  - JobTitle
  - ReportsTo
  - StartDate, TerminationDate
  - SyncedFromHRIS (Yes/No)

-- Step 6: Framework Selection
TenantFramework
  - FrameworkId (PK)
  - TenantId (FK)
  - Framework (ISO27001, NIST, GDPR, etc.)
  - AdoptionDate
  - TargetMaturityLevel
  - ComplianceStatus

Control (per framework: 500+ records)
  - ControlId (PK)
  - FrameworkId (FK)
  - ControlCode
  - ControlName
  - Category
  - TestingFrequency
  - EvidenceTypes (JSON)
  - RiskIfNotImplemented

-- Step 7: Control Ownership
ControlOwnership
  - OwnershipId (PK)
  - ControlId (FK)
  - OwnerId (FK ApplicationUser)
  - AlternateOwnerId (FK)
  - TestingResponsibility
  - ApprovalAuthority
  - StartDate

-- Step 8: Evidence Configuration
EvidenceSource
  - SourceId (PK)
  - TenantId (FK)
  - SourceType (Repository, System, Manual)
  - SourceName
  - ConnectionDetails (encrypted JSON)
  - SyncFrequency
  - LastSyncDate

EvidenceMapping
  - MappingId (PK)
  - ControlId (FK)
  - EvidenceTypeId (FK)
  - SourceId (FK)
  - ExpirationRule
  - CollectionFrequency
```

---

## SECTION 5: WORKFLOW TRIGGERS & AUTOMATION

### Onboarding Automation Triggers

```
Step 1 → Step 2:
  Trigger: User clicks "Continue to Org Profile"
  Automation: None (manual progression)

Step 2 → Step 3:
  Trigger: Org profile form submitted
  Automation:
    - Save OrganizationProfile
    - Query RulesEngine
    - Get applicable frameworks
    - Auto-select frameworks
    - Create 500+ control records
    - Create baseline
    - Display "Frameworks selected"

Step 3 → Step 4:
  Trigger: User clicks "Create Plan"
  Automation:
    - Create AssessmentPlan
    - Estimate timeline (3-18 months)
    - Create 500+ WorkflowInstances (one per control)
    - Schedule initial assessment (90 days)
    - Send notification to CRO

Step 4 → Step 5:
  Trigger: User clicks "Next: HRIS Setup"
  Automation: None (wait for HRIS config)

Step 5 Completion:
  Trigger: HRIS sync completes successfully
  Automation:
    - Create 100+ ApplicationUser records
    - Assign RoleProfile based on job title
    - Create 100+ UserWorkspace records
    - Auto-assign 500 controls to owners from HRIS
    - Auto-create 500 task assignments
    - Notify all users of their assignments
    - Enable HRIS sync job (daily)

Step 6 → Step 7:
  Trigger: Framework selection confirmed
  Automation:
    - Create ControlBaseline
    - Create AssessmentTemplate (per framework)
    - Schedule initial audit (6 months)
    - Generate control assessment plan

Step 7 → Step 8:
  Trigger: Control ownership complete
  Automation:
    - Create EscalationRules (5-10)
    - Create NotificationRules (10-15)
    - Create TestingSchedule
    - Notify control owners of assignments

Step 8 Complete:
  Trigger: Evidence sources connected
  Automation:
    - Enable evidence collection jobs
    - Start initial evidence scan
    - Map evidence to controls
    - Notify stakeholders onboarding complete
    - Show "Go to Dashboard"

Post-Onboarding:
  Daily: HRIS sync (fetch updated employees)
  6-hourly: Evidence collection
  Weekly: Evidence validation
  Monthly: Evidence expiration alerts
  Quarterly: Compliance status updates
```

---

## NEXT STEPS (AWAITING APPROVAL)

- [ ] Review onboarding flow (Steps 1-8)
- [ ] Confirm data collection requirements
- [ ] Approve role profile assignments
- [ ] Confirm automation triggers
- [ ] Identify any missing data structures
- [ ] Proceed to REPORT 3 (Module Interconnections)

**STATUS**: 🔴 AWAITING REVIEW - No implementation until approved
