# Onboarding Process Steps & Post-Onboarding Relationships

This document provides comprehensive tables documenting each onboarding step, the data collected, and how it relates to post-onboarding functionality.

---

## Table of Contents
1. [Simple Flow Overview (4 Steps)](#1-simple-flow-overview-4-steps)
2. [Comprehensive Wizard Overview (12 Sections)](#2-comprehensive-wizard-overview-12-sections)
3. [Simple Flow - Detailed Step Tables](#3-simple-flow---detailed-step-tables)
4. [Comprehensive Wizard - Detailed Section Tables](#4-comprehensive-wizard---detailed-section-tables)
5. [Post-Onboarding Feature Dependencies](#5-post-onboarding-feature-dependencies)
6. [Why Each Step Exists](#6-why-each-step-exists)

---

## 1. Simple Flow Overview (4 Steps)

| Step | Name | Progress | Data Collected | Post-Onboarding Impact |
|------|------|----------|----------------|------------------------|
| 1 | Signup | 25% | Organization name, admin email, subscription tier, country, legal consents | Creates tenant record, enables multi-tenancy, determines initial features |
| 2 | Organization Profile | 50% | Organization type, sector, data types, hosting model, compliance maturity | Triggers Rules Engine for framework selection, sets assessment depth |
| 3 | Review Scope | 75% | User reviews/confirms derived baselines, packages, templates | Finalizes applicable frameworks, controls, and templates |
| 4 | Create Plan | 100% | Plan name, type, start/end dates | Generates GRC plan, triggers smart onboarding, redirects to dashboard |

---

## 2. Comprehensive Wizard Overview (12 Sections)

| Section | Name | Questions | Purpose | Post-Onboarding Impact |
|---------|------|-----------|---------|------------------------|
| A | Organization Identity & Tenancy | 13 | Establish legal entity and operating context | Workspace configuration, jurisdiction rules, bilingual support |
| B | Assurance Objective | 5 | Define compliance goals and timeline | Assessment prioritization, dashboard KPIs, reporting setup |
| C | Regulatory & Framework Applicability | 7 | Identify regulators and mandatory frameworks | Control baseline selection, audit scope definition |
| D | Scope Definition | 9 | Define in-scope entities, systems, processes | Assessment boundary, control applicability filtering |
| E | Data & Risk Profile | 6 | Identify data types and risk exposure | Evidence requirements, PDPL/PCI-DSS rules, vendor risk |
| F | Technology Landscape | 13 | Map existing technology stack | Integration configuration, evidence storage, SSO setup |
| G | Control Ownership Model | 7 | Define ownership and approval structure | RACI mapping, approval workflows, escalation paths |
| H | Teams, Roles & Access | 10 | Configure teams and user access | User workspace assignment, role provisioning, notifications |
| I | Workflow & Cadence | 10 | Set operational frequencies and SLAs | Calendar reminders, deadline tracking, escalation rules |
| J | Evidence Standards | 7 | Define evidence collection requirements | Evidence naming, storage, retention, access rules |
| K | Baseline + Overlays Selection | 3 | Confirm/customize control baseline | Final control set, custom controls, overlay application |
| L | Go-Live & Success Metrics | 6 | Define success criteria and targets | Dashboard metrics, improvement tracking, pilot scope |

---

## 3. Simple Flow - Detailed Step Tables

### Step 1: Signup (25%)

| Field | Type | Required | Example Value | Post-Onboarding Use |
|-------|------|----------|---------------|---------------------|
| Organization Name | Text | Yes | "Shahin Financial Services" | Displayed in UI, tenant identification |
| Administrator Email | Email | Yes | "admin@shahin.sa" | Primary login, notification recipient |
| Subscription Tier | Select | Yes | Starter/Professional/Enterprise | Feature access limits, pricing |
| Country of Operation | Select | Yes | Saudi Arabia | Jurisdiction rules, NCA applicability |
| Terms of Service | Checkbox | Yes | Accepted | Legal compliance |
| Privacy Policy | Checkbox | Yes | Accepted | PDPL compliance |
| Data Processing Consent | Checkbox | Yes | Accepted | Data handling authorization |

**Why This Step Exists:**
- Creates the tenant record that enables multi-tenancy isolation
- Establishes legal agreement and consent framework
- Determines initial jurisdiction for framework selection
- Sets subscription tier for feature availability

---

### Step 2: Organization Profile (50%)

| Field | Type | Required | Options/Example | Post-Onboarding Use |
|-------|------|----------|-----------------|---------------------|
| Organization Type | Select | Yes | Enterprise, SME, Government, RegulatedFI, Fintech, Telecom, Healthcare, Education, Retail, Startup, Other | Workspace defaults, control priorities |
| Sector | Select | Yes | Banking, Healthcare, Energy, Telecom, Retail, Government | Industry-specific framework overlays |
| Primary Country | Select | Yes | Saudi Arabia, UAE, Bahrain, Kuwait, Oman, Qatar | Jurisdiction rules triggering |
| Data Hosting Model | Select | Yes | On-Premise, Cloud, Hybrid | Cloud-specific controls applicability |
| Data Types Processed | Multi-Select | Yes | PII, Financial, Health, Confidential, Classified, Customer | PDPL, PCI-DSS, HIPAA rules |
| Organization Size | Select | Yes | Startup (<50), Small (50-200), Medium (200-1000), Large (1000+) | Assessment complexity, team structure |
| Compliance Maturity | Select | Yes | Initial, Developing, Defined, Managed, Optimized | Assessment depth, SLA configuration |
| Is Critical Infrastructure | Boolean | No | Yes/No | NCA-CSCC framework applicability |
| Third-Party Vendors | Textarea | No | "AWS, Microsoft, Oracle" | Vendor risk considerations |

**Why This Step Exists:**
- Provides context for the Rules Engine to derive applicable frameworks
- Determines assessment depth based on maturity level
- Identifies critical infrastructure for enhanced requirements
- Maps data types to regulatory requirements (PDPL, PCI-DSS)

---

### Step 3: Review Scope (75%)

| Element | Description | Source | Post-Onboarding Use |
|---------|-------------|--------|---------------------|
| Applicable Baselines | Frameworks derived by Rules Engine | Rules Engine evaluation | Control baseline for assessments |
| Baseline Reason | JSON explanation of why framework applies | Rule conditions matched | Audit trail, regulatory justification |
| Estimated Controls | Number of controls per framework | Baseline definition | Workload estimation, resource planning |
| Recommended Packages | Grouped control sets | Rules Engine actions | Quick-start control bundles |
| Recommended Templates | Pre-built assessment templates | Template matching rules | Assessment auto-generation |

**Why This Step Exists:**
- Allows user to validate automated framework selection
- Provides transparency into Rules Engine decisions
- Enables manual override before final scope commitment
- Creates audit trail for regulatory justification

---

### Step 4: Create Plan (100%)

| Field | Type | Required | Example Value | Post-Onboarding Use |
|-------|------|----------|---------------|---------------------|
| Plan Name | Text | Yes | "2024 Compliance Roadmap" | Plan identification in dashboard |
| Description | Textarea | No | "Annual compliance assessment plan" | Plan context and documentation |
| Plan Type | Select | Yes | QuickScan, Comprehensive, Remediation | Assessment generation strategy |
| Start Date | Date | Yes | 2024-01-15 | Calendar scheduling, milestone tracking |
| Target End Date | Date | Yes | 2024-12-31 | Deadline management, progress calculation |

**Why This Step Exists:**
- Triggers the Smart Onboarding Service for auto-generation
- Creates the foundational GRC Plan entity
- Sets timeline for compliance activities
- Transitions user from onboarding to operational mode

---

## 4. Comprehensive Wizard - Detailed Section Tables

### Section A: Organization Identity & Tenancy (13 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| A.1 | Legal Name (English) | Text | Official documentation, reports |
| A.2 | Legal Name (Arabic) | Text | Arabic UI display, bilingual reports |
| A.3 | Trade Name | Text | Alternative display name |
| A.4 | Country of Incorporation | Select | Legal jurisdiction determination |
| A.5 | Operating Countries | Multi-Select | Multi-jurisdiction framework rules |
| A.6 | Primary HQ Location | Text | Time zone defaults, location context |
| A.7 | Timezone | Select | Deadline calculations, scheduling |
| A.8 | Primary Language | Select | UI language, notification language |
| A.9 | Corporate Email Domains | Text | User email validation, SSO domains |
| A.10 | Domain Verification Method | Select | Security verification process |
| A.11 | Organization Type | Select | Workspace configuration |
| A.12 | Industry/Sector | Select | Industry-specific overlays |
| A.13 | Data Residency Requirements | Select | Cloud region restrictions |

**Why This Section Exists:**
- Establishes complete organizational identity for legal compliance
- Enables multi-jurisdiction framework application
- Sets up localization (Arabic/English) preferences
- Defines security boundaries for user access

---

### Section B: Assurance Objective (5 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| B.1 | Primary Driver | Select | Assessment prioritization |
| B.2 | Target Timeline/Milestone | Date | Deadline tracking, urgency flags |
| B.3 | Current Pain Points (Rank 1-3) | Multi-Select | Dashboard focus areas |
| B.4 | Desired Maturity Level | Select | Goal tracking, improvement metrics |
| B.5 | Reporting Audience | Multi-Select | Report format, detail level |

**Options for Primary Driver:**
- RegulatorExam
- InternalAudit
- Certification (ISO, SOC2)
- CustomerRequirement
- BoardMandate
- RiskReduction
- OperationalImprovement

**Why This Section Exists:**
- Prioritizes compliance activities based on business drivers
- Aligns assessment focus with organizational goals
- Configures reporting for appropriate audience
- Establishes success criteria for maturity improvement

---

### Section C: Regulatory & Framework Applicability (7 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| C.1 | Primary Regulators | Multi-Select | Mandatory framework selection |
| C.2 | Secondary Regulators | Multi-Select | Additional framework considerations |
| C.3 | Mandatory Frameworks | Multi-Select | Control baseline core |
| C.4 | Benchmarking Frameworks | Multi-Select | Optional best-practice controls |
| C.5 | Internal Policies/Standards | Textarea | Custom control source |
| C.6 | Certifications Held | Multi-Select | Existing compliance leverage |
| C.7 | Audit Scope Type | Select | Assessment boundary definition |

**Regulator Options (by Jurisdiction):**
| Country | Regulators |
|---------|------------|
| Saudi Arabia | NCA, SAMA, CITC, CMA, MOH |
| UAE | CBUAE, TDRA, ADGM, DFSA |
| Bahrain | CBB, TRA |
| Kuwait | CBK |
| Qatar | QCB, CRA |

**Why This Section Exists:**
- Maps organization to specific regulatory requirements
- Identifies mandatory vs. optional frameworks
- Leverages existing certifications to reduce duplication
- Defines audit scope boundaries

---

### Section D: Scope Definition (9 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| D.1 | In-Scope Legal Entities | Table | Entity-level assessment filtering |
| D.2 | In-Scope Business Units | Table | BU-level control assignment |
| D.3 | In-Scope Systems/Applications | Table | System-level control mapping |
| D.4 | In-Scope Processes | Multi-Select | Process control applicability |
| D.5 | In-Scope Environments | Select | Production/Non-prod controls |
| D.6 | In-Scope Locations | Table | Physical location controls |
| D.7 | System Criticality Tiers | Table | RTO/RPO requirements |
| D.8 | Important Business Services | Table | Business impact analysis |
| D.9 | Exclusions (with rationale) | Table | Scope boundary documentation |

**Process Options:**
- Customer Onboarding
- Payment Processing
- Procurement (P2P)
- Change Management
- Incident Response
- Access Management
- Data Protection
- Backup & Recovery

**Why This Section Exists:**
- Precisely defines what is in/out of compliance scope
- Enables control applicability filtering
- Maps criticality for prioritization
- Documents exclusions for audit justification

---

### Section E: Data & Risk Profile (6 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| E.1 | Data Types Processed | Multi-Select | PDPL, PCI-DSS, HIPAA applicability |
| E.2 | Payment Card Data | Boolean + Detail | PCI-DSS scope determination |
| E.3 | Cross-Border Data Transfers | Table | Transfer mechanism controls |
| E.4 | Customer Volume Tier | Select | Scale-based controls |
| E.5 | Transaction Volume Tier | Select | Performance controls |
| E.6 | Third-Party Data Processors | Table | Vendor risk management |

**Data Type Impact on Frameworks:**
| Data Type | Triggered Framework/Controls |
|-----------|------------------------------|
| PII | PDPL, GDPR (if EU transfers) |
| Financial | SAMA-CSF financial controls |
| Payment Card | PCI-DSS |
| Health | MOH requirements, HIPAA (if US) |
| Classified | NCA-CSCC enhanced controls |

**Why This Section Exists:**
- Identifies data types for regulatory mapping
- Enables risk-based control prioritization
- Maps vendor relationships for third-party risk
- Determines cross-border compliance requirements

---

### Section F: Technology Landscape (13 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| F.1 | Identity Provider | Select | SSO integration setup |
| F.2 | SSO Enabled | Boolean + Protocol | Authentication configuration |
| F.3 | SCIM Provisioning | Boolean | Automated user provisioning |
| F.4 | ITSM/Ticketing Platform | Select | Remediation workflow integration |
| F.5 | Evidence Repository | Select | Evidence storage configuration |
| F.6 | SIEM/SOC Platform | Select | Security monitoring integration |
| F.7 | Vulnerability Management | Select | Vulnerability evidence collection |
| F.8 | EDR Platform | Select | Endpoint control evidence |
| F.9 | Cloud Providers | Multi-Select | Cloud-specific controls |
| F.10 | ERP Platform | Select | Business process evidence |
| F.11 | CMDB/Asset Inventory | Select | Asset-based control mapping |
| F.12 | CI/CD Tooling | Select | DevSecOps controls |
| F.13 | Backup/DR Tooling | Select | Recovery controls |

**Why This Section Exists:**
- Enables platform integrations for automated evidence collection
- Maps existing tools to control evidence sources
- Identifies gaps in security tooling
- Configures appropriate technical controls

---

### Section G: Control Ownership Model (7 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| G.1 | Ownership Approach | Select | Centralized/Federated/Hybrid model |
| G.2 | Default Control Owner Team | Select | Default assignment for new controls |
| G.3 | Exception Approver Role | Text | Exception workflow routing |
| G.4 | Regulatory Interpreter Role | Text | Interpretation approval routing |
| G.5 | Effectiveness Signoff Role | Text | Assessment completion approval |
| G.6 | Internal Audit Contact | Table | Audit liaison assignment |
| G.7 | Risk Committee | Table | Governance reporting |

**Why This Section Exists:**
- Establishes clear accountability for controls
- Configures approval workflows
- Sets up governance reporting structure
- Enables RACI matrix generation

---

### Section H: Teams, Roles & Access (10 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| H.1 | Organization Admins | Table | Initial admin user creation |
| H.2 | Create Teams Now | Boolean | Team workspace provisioning |
| H.3 | Team Definitions | Table | Team structure setup |
| H.4 | Team Members | Table | User assignment to teams |
| H.5 | Role Catalog | Multi-Select | Available roles in system |
| H.6 | RACI Mapping Needed | Boolean + Matrix | Responsibility assignment |
| H.7 | Approval Gates | Boolean + Config | Workflow gate configuration |
| H.8 | Delegation Rules | Table | Delegation permissions |
| H.9 | Notification Preferences | Select | Communication channel setup |
| H.10 | Escalation Path | Select + Config | Escalation workflow setup |

**Standard Roles:**
| Role | Description |
|------|-------------|
| ControlOwner | Responsible for control implementation |
| EvidenceCustodian | Responsible for evidence collection |
| Approver | Reviews and approves evidence/assessments |
| Assessor | Performs control assessments |
| RemediationOwner | Responsible for remediation tasks |
| Viewer | Read-only access |

**Why This Section Exists:**
- Creates team structure for collaborative work
- Assigns users to roles for access control
- Configures notification and escalation workflows
- Enables delegation for operational flexibility

---

### Section I: Workflow & Cadence (10 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| I.1 | Evidence Frequency Defaults | Table | Evidence collection schedule |
| I.2 | Access Review Frequency | Select | Access control calendar |
| I.3 | Vulnerability Review Frequency | Select | Vulnerability management calendar |
| I.4 | Backup Review Frequency | Select | Backup verification calendar |
| I.5 | Restore Test Cadence | Select | Recovery testing calendar |
| I.6 | DR Exercise Cadence | Select | DR drill calendar |
| I.7 | Incident Tabletop Cadence | Select | Tabletop exercise calendar |
| I.8 | Evidence SLA (Submit Days) | Number | Deadline calculation |
| I.9 | Remediation SLA by Severity | Table | Remediation deadline rules |
| I.10 | Exception Expiry Days | Number | Exception lifecycle management |

**Default Remediation SLAs:**
| Severity | Days to Remediate |
|----------|-------------------|
| Critical | 7 |
| High | 14 |
| Medium | 30 |
| Low | 90 |

**Why This Section Exists:**
- Configures compliance calendar with recurring activities
- Sets SLAs for deadline tracking
- Enables automated reminders and escalations
- Establishes operational rhythm for GRC activities

---

### Section J: Evidence Standards (7 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| J.1 | Naming Convention Required | Boolean + Pattern | Evidence file naming validation |
| J.2 | Storage Location by Domain | Table | Evidence storage routing |
| J.3 | Retention Period (Years) | Number | Evidence lifecycle management |
| J.4 | Access Rules | Table | Evidence access control |
| J.5 | Acceptable Evidence Types | Multi-Select | Evidence format validation |
| J.6 | Sampling Guidance | Table | Audit sampling configuration |
| J.7 | Confidential Evidence Handling | Config | Encryption/access requirements |

**Acceptable Evidence Types:**
- Export Reports
- System Logs
- Screenshots
- Signed PDFs
- System Attestations
- Automated Scans
- Configuration Files

**Why This Section Exists:**
- Standardizes evidence collection practices
- Ensures evidence meets audit requirements
- Configures retention for regulatory compliance
- Protects confidential evidence appropriately

---

### Section K: Baseline + Overlays Selection (3 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| K.1 | Adopt Default Baseline | Boolean | Confirms Rules Engine baseline |
| K.2 | Select Overlays | Multi-Select | Additional control layers |
| K.3 | Custom Control Requirements | Table | Organization-specific controls |

**Overlay Types:**
| Overlay Type | Description |
|--------------|-------------|
| Jurisdiction | Country-specific requirements |
| Sector | Industry-specific controls |
| Data | Data type-specific controls |
| Technology | Tech stack-specific controls |

**Why This Section Exists:**
- Allows user confirmation of automated baseline
- Enables overlay customization
- Supports custom control addition
- Finalizes control set before assessment generation

---

### Section L: Go-Live & Success Metrics (6 Questions)

| Question | Field | Type | Post-Onboarding Use |
|----------|-------|------|---------------------|
| L.1 | Success Metrics (Top 3) | Multi-Select | Dashboard KPI configuration |
| L.2 | Current Audit Prep Hours/Month | Number | Baseline measurement |
| L.3 | Current Remediation Closure Days | Number | Baseline measurement |
| L.4 | Current Overdue Controls/Month | Number | Baseline measurement |
| L.5 | Target Improvement % | Table | Goal setting |
| L.6 | Pilot Scope | Config | Phased rollout configuration |

**Success Metric Options:**
- Fewer Audit Hours
- Faster Evidence Turnaround
- Reduced Repeat Findings
- Higher Compliance Score
- Faster Remediation Closure
- Reduced Exception Volume

**Why This Section Exists:**
- Establishes baseline metrics for improvement tracking
- Sets measurable goals for compliance program
- Enables ROI demonstration
- Configures pilot scope for phased implementation

---

## 5. Post-Onboarding Feature Dependencies

### 5.1 Smart Onboarding Service Dependencies

| Feature Generated | Depends On (Onboarding Data) |
|-------------------|------------------------------|
| Assessment Templates | Sector (A.12), Frameworks (C.3), Scope (D.*) |
| GRC Plan | Plan Type (Step 4), Timeline (B.2), Maturity (B.4) |
| Control Baseline | Rules Engine output from Step 3, Overlays (K.2) |
| Team Workspaces | Teams (H.3), Roles (H.5), Members (H.4) |
| Evidence Requirements | Data Types (E.1), Standards (J.*), Frequency (I.1) |

### 5.2 Dashboard Configuration Dependencies

| Dashboard Element | Depends On (Onboarding Data) |
|-------------------|------------------------------|
| Compliance Score Widget | Frameworks (C.3), Scope (D.*) |
| Upcoming Deadlines | Timeline (B.2), Cadence (I.*) |
| Risk Heat Map | Data Types (E.1), Criticality (D.7) |
| Team Performance | Teams (H.3), SLAs (I.8-9) |
| Success Metrics | Metrics (L.1), Baselines (L.2-4), Targets (L.5) |

### 5.3 Integration Configuration Dependencies

| Integration | Depends On (Onboarding Data) |
|-------------|------------------------------|
| SSO | Identity Provider (F.1), SSO Config (F.2) |
| User Provisioning | SCIM (F.3), Domains (A.9) |
| Ticketing | ITSM Platform (F.4) |
| Evidence Storage | Repository (F.5), Standards (J.*) |
| Security Monitoring | SIEM (F.6), Vulnerability (F.7), EDR (F.8) |

### 5.4 Workflow Configuration Dependencies

| Workflow | Depends On (Onboarding Data) |
|----------|------------------------------|
| Evidence Collection | Frequency (I.1), SLA (I.8), Standards (J.*) |
| Remediation | SLAs (I.9), Ownership (G.*), Escalation (H.10) |
| Approval | Gates (H.7), Approvers (G.3-5), Delegation (H.8) |
| Exception Management | Expiry (I.10), Approvers (G.3) |
| Audit Response | Internal Audit (G.6), Scope (D.*) |

---

## 6. Why Each Step Exists

### Simple Flow Step Justifications

| Step | Business Justification | Technical Justification |
|------|------------------------|-------------------------|
| **1. Signup** | Establishes legal relationship and consent | Creates tenant for multi-tenant isolation |
| **2. Org Profile** | Captures context for compliance program design | Provides input data for Rules Engine |
| **3. Review Scope** | Ensures user agrees with framework selection | Validates Rules Engine output before commitment |
| **4. Create Plan** | Formalizes compliance roadmap | Triggers smart onboarding automation |

### Comprehensive Wizard Section Justifications

| Section | Business Justification | Technical Justification |
|---------|------------------------|-------------------------|
| **A. Organization Identity** | Complete legal and operational identity | Multi-jurisdiction routing, localization |
| **B. Assurance Objective** | Aligns system with business goals | Prioritization algorithms, reporting config |
| **C. Regulatory Applicability** | Maps to regulatory obligations | Framework and control selection |
| **D. Scope Definition** | Defines assessment boundaries | Control applicability filtering |
| **E. Data & Risk Profile** | Identifies risk exposure | Risk-based control selection |
| **F. Technology Landscape** | Leverages existing investments | Integration and evidence collection |
| **G. Control Ownership** | Clear accountability | Workflow routing, approvals |
| **H. Teams, Roles & Access** | Organizational structure | User provisioning, RBAC |
| **I. Workflow & Cadence** | Operational rhythm | Scheduling, SLAs, reminders |
| **J. Evidence Standards** | Audit-ready evidence | Validation, storage, retention |
| **K. Baseline + Overlays** | Customized control set | Final baseline commitment |
| **L. Success Metrics** | Measurable outcomes | KPI tracking, improvement |

---

## Appendix: Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           ONBOARDING FLOW                                │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  STEP 1: SIGNUP                                                          │
│  ─────────────────                                                       │
│  Input: Org Name, Admin Email, Tier, Country, Consents                  │
│  Output: Tenant Record (TenantId, Slug, Status=Pending)                 │
│  Triggers: Activation Email                                             │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  STEP 2: ORGANIZATION PROFILE                                            │
│  ──────────────────────────────                                          │
│  Input: Org Type, Sector, Data Types, Hosting, Maturity, Size           │
│  Output: OrganizationProfile Record                                      │
│  Triggers: Rules Engine Evaluation                                       │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  RULES ENGINE                                                            │
│  ────────────                                                            │
│  Input: OrganizationProfile fields (30+ evaluation context fields)      │
│  Processing: Evaluate rules against context                              │
│  Output: Applicable Baselines, Packages, Templates with Reasons         │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  STEP 3: REVIEW SCOPE                                                    │
│  ──────────────────                                                      │
│  Display: Baselines (NCA-ECC, SAMA-CSF, PDPL, etc.)                     │
│  Display: Estimated Controls, Packages, Templates                        │
│  User Action: Review and Confirm                                         │
│  Output: TenantBaselines, TenantPackages, TenantTemplates               │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  STEP 4: CREATE PLAN                                                     │
│  ──────────────────                                                      │
│  Input: Plan Name, Type, Start Date, End Date                           │
│  Output: Plan Record                                                     │
│  Triggers: Smart Onboarding Service                                      │
│  Status: OnboardingStatus = COMPLETED                                    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  SMART ONBOARDING SERVICE                                                │
│  ────────────────────────                                                │
│  Auto-generates:                                                         │
│    ├── Assessment Templates (based on frameworks)                        │
│    ├── GRC Plan with Phases                                              │
│    ├── Assessment Entities                                               │
│    ├── Team Workspaces                                                   │
│    ├── Evidence Requirements                                             │
│    └── Workflow Configurations                                           │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  POST-ONBOARDING SYSTEM                                                  │
│  ──────────────────────                                                  │
│  ├── Dashboard with Compliance Widgets                                   │
│  ├── Assessment Management                                               │
│  ├── Evidence Collection                                                 │
│  ├── Remediation Tracking                                                │
│  ├── Workflow & Approvals                                                │
│  ├── Reporting & Analytics                                               │
│  └── Calendar & Reminders                                                │
└─────────────────────────────────────────────────────────────────────────┘
```

---

*Document Generated: January 2026*
*Version: 1.0*
