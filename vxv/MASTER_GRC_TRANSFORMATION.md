# GRC MASTER TRANSFORMATION SPECIFICATION

## Executive Summary

This document defines the **unified GRC operating model** that integrates:
1. **6 Lifecycle Stages** with step-by-step workflows
2. **Gate definitions** with validation rules and thresholds
3. **Serial code standard** for complete traceability
4. **Kanban + Advanced Views** with direct actionable items
5. **Autonomous execution loop** with bounded automation

---

## PART 1: THE 6-STAGE GRC LIFECYCLE

```
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│                              MASTER GRC LIFECYCLE                                           │
├─────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                             │
│  STAGE 1          STAGE 2       STAGE 3        STAGE 4        STAGE 5        STAGE 6       │
│  ════════         ════════      ════════       ════════       ════════       ════════       │
│                                                                                             │
│  ┌──────────┐    ┌────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌────────────┐  │
│  │ASSESSMENT│───▶│  RISK  │───▶│COMPLIANCE│───▶│RESILIENCE│───▶│EXCELLENCE│───▶│SUSTAINABIL.│ │
│  │EXPLORATION│   │ANALYSIS│   │MONITORING│   │ BUILDING │   │BENCHMARKS│   │CONTINUOUS  │  │
│  └──────────┘    └────────┘   └──────────┘   └──────────┘   └──────────┘   └────────────┘  │
│       │              │             │              │              │              │          │
│   8 Steps        8 Steps       8 Steps        8 Steps        7 Steps        7 Steps       │
│                                                                                             │
│  "Where are     "What can     "Are we        "Can we        "How do we     "How do we     │
│   we now?"      go wrong?"    compliant?"    recover?"      compare?"      improve?"      │
│                                                                                             │
└─────────────────────────────────────────────────────────────────────────────────────────────┘

TOTAL: 46 Steps across 6 Stages with 5 Inter-Stage Gates
```

---

## PART 2: STAGE DEFINITIONS WITH STEPS AND GATES

### STAGE 1: ASSESSMENT & EXPLORATION (8 Steps)

**Purpose:** Discover and document the actual current state of the organization

| Step | Status Code | Name | Entry Requirements | Exit Gate (to next step) |
|------|-------------|------|-------------------|--------------------------|
| 1.1 | `DRAFT` | Draft | - | Scope defined, framework selected, owner assigned |
| 1.2 | `SCHEDULED` | Scheduled | Owner assigned | Start/end dates set, assessors assigned |
| 1.3 | `IN_PROGRESS` | In Progress | Dates approved | Requirements ≥40% scored |
| 1.4 | `EVIDENCE_COLLECTION` | Evidence Collection | Progress started | Evidence coverage ≥60% |
| 1.5 | `SCORING` | Scoring | Evidence attached | All requirements scored or N/A justified |
| 1.6 | `SUBMITTED` | Submitted | Scoring complete | Review package complete, signatory routed |
| 1.7 | `UNDER_REVIEW` | Under Review | Reviewer assigned | All review comments resolved |
| 1.8 | `APPROVED` | Approved/Completed | Review passed | Final report generated, archived |

**Gate Thresholds (Configurable):**
```yaml
assessment_gates:
  progress_minimum: 40%
  evidence_coverage_minimum: 60%
  scoring_completion: 100%
  review_resolution: 100%
```

---

### STAGE 2: RISK ANALYSIS (8 Steps)

**Purpose:** Identify what can go wrong based on assessment findings

| Step | Status Code | Name | Entry Requirements | Exit Gate |
|------|-------------|------|-------------------|-----------|
| 2.1 | `IDENTIFIED` | Identified | - | Title, description, category, owner set |
| 2.2 | `CONTEXTUALIZED` | Contextualized | Risk documented | Linked to asset/process/finding |
| 2.3 | `SCORED_INHERENT` | Inherent Scoring | Context linked | Likelihood × Impact calculated |
| 2.4 | `PENDING_DECISION` | Pending Decision | Score computed | Treatment decision required |
| 2.5 | `TREATMENT_PLANNED` | Treatment Planned | Decision made | Strategy, controls, timeline defined |
| 2.6 | `MITIGATING` | Mitigating | Plan approved | Actions in progress |
| 2.7 | `SCORED_RESIDUAL` | Residual Scoring | Actions complete | Residual risk ≤ appetite |
| 2.8 | `MONITORED_CLOSED` | Monitored/Closed | Residual accepted | KRIs defined OR closed |

**Risk Scoring Matrix (5×5):**
```
Likelihood (1-5) × Impact (1-5) = Risk Score (1-25)

SCORE     LEVEL      COLOR    ACTION REQUIRED
─────────────────────────────────────────────
20-25     Critical   Red      Immediate executive attention
12-19     High       Orange   Treatment plan within 30 days
6-11      Medium     Yellow   Treatment plan within 90 days
1-5       Low        Green    Accept or monitor
```

**Treatment Decision Options:**
- `ACCEPT` - Risk within appetite, approved by risk owner
- `MITIGATE` - Reduce via controls/actions
- `TRANSFER` - Insurance, contract, outsource
- `AVOID` - Eliminate the activity/asset

---

### STAGE 3: COMPLIANCE MONITORING (8 Steps)

**Purpose:** Track adherence to regulatory requirements

| Step | Status Code | Name | Entry Requirements | Exit Gate |
|------|-------------|------|-------------------|-----------|
| 3.1 | `OBLIGATIONS_MAPPED` | Obligations Mapped | - | Applicable frameworks identified |
| 3.2 | `CONTROLS_MAPPED` | Controls Mapped | Obligations set | Control coverage ≥80% |
| 3.3 | `EVIDENCE_REQUESTED` | Evidence Requested | Controls mapped | Evidence requests issued |
| 3.4 | `EVIDENCE_VERIFIED` | Evidence Verified | Evidence submitted | Verification rate ≥70% |
| 3.5 | `TESTED` | Tested | Evidence verified | Control tests executed |
| 3.6 | `SCORED` | Scored | Tests complete | Compliance score calculated |
| 3.7 | `REMEDIATION` | Remediation | Gaps identified | Action plans for gaps |
| 3.8 | `ATTESTED` | Attested/Reported | Gaps closed | Sign-off + report pack |

**Compliance Score Thresholds:**
```yaml
compliance_gates:
  control_coverage_minimum: 80%
  evidence_verification_minimum: 70%
  compliant_threshold: 80%
  partially_compliant_threshold: 50%
  # Below 50% = Non-Compliant
```

---

### STAGE 4: RESILIENCE BUILDING (8 Steps)

**Purpose:** Build ability to recover from incidents

| Step | Status Code | Name | Entry Requirements | Exit Gate |
|------|-------------|------|-------------------|-----------|
| 4.1 | `SCOPE_DEFINED` | Scope Defined | - | Critical services/assets identified |
| 4.2 | `BIA_COMPLETE` | BIA Complete | Scope approved | RTO/RPO defined for critical |
| 4.3 | `STRATEGIES_DESIGNED` | Strategies Designed | BIA approved | DR/BC/IR strategies documented |
| 4.4 | `PLANS_IMPLEMENTED` | Plans Implemented | Strategies approved | Playbooks + tooling ready |
| 4.5 | `DRILLS_EXECUTED` | Drills Executed | Plans ready | At least 1 drill completed |
| 4.6 | `VERIFIED` | Verified | Drill complete | RTO/RPO objectives met |
| 4.7 | `IMPROVEMENTS_APPLIED` | Improvements Applied | Findings documented | High-severity findings closed |
| 4.8 | `MONITORING` | Continuous Monitoring | Improvements done | KRIs/KPIs + alerting active |

**Resilience Score Components:**
```yaml
resilience_score:
  business_continuity_weight: 30%
  disaster_recovery_weight: 30%
  incident_response_weight: 20%
  crisis_management_weight: 20%

  strong_threshold: 70
  moderate_threshold: 50
  # Below 50 = Weak
```

---

### STAGE 5: EXCELLENCE & BENCHMARKING (7 Steps)

**Purpose:** Compare against sector peers and achieve certifications

| Step | Status Code | Name | Entry Requirements | Exit Gate |
|------|-------------|------|-------------------|-----------|
| 5.1 | `BASELINE_SET` | Maturity Baseline | - | 5 dimensions scored |
| 5.2 | `BENCHMARKED` | Benchmarked | Baseline complete | Peer comparison generated |
| 5.3 | `TARGETS_SET` | Targets Set | Benchmark available | Target maturity approved |
| 5.4 | `PROGRAMS_DEFINED` | Programs Defined | Targets approved | Initiatives + budget |
| 5.5 | `EXECUTING` | Executing | Funding approved | Initiatives in progress |
| 5.6 | `CERT_READY` | Certification Ready | Initiatives complete | Audit-ready status |
| 5.7 | `RECOGNIZED` | Recognized | Audit passed | Certification achieved |

**Maturity Model (CMM 1-5):**
```
LEVEL   NAME                    SCORE RANGE   CHARACTERISTICS
─────────────────────────────────────────────────────────────
5       Optimizing              ≥80           Continuous improvement, predictive
4       Quantitatively Managed  60-79         Metrics-driven, controlled
3       Defined                 40-59         Documented, standardized
2       Managed                 20-39         Repeatable, reactive
1       Initial                 <20           Ad-hoc, chaotic
```

---

### STAGE 6: CONTINUOUS SUSTAINABILITY (7 Steps)

**Purpose:** Long-term improvement and Vision 2030 alignment

| Step | Status Code | Name | Entry Requirements | Exit Gate |
|------|-------------|------|-------------------|-----------|
| 6.1 | `KPIS_ACTIVE` | KPIs Active | - | Dashboards + monitoring live |
| 6.2 | `HEALTH_REVIEWED` | Health Reviewed | KPIs active | Quarterly review complete |
| 6.3 | `TRENDS_ANALYZED` | Trends Analyzed | Review done | 12-month trend report |
| 6.4 | `INITIATIVES_IDENTIFIED` | Initiatives Identified | Trends documented | Improvement backlog |
| 6.5 | `ROADMAP_APPROVED` | Roadmap Approved | Initiatives prioritized | Multi-year plan approved |
| 6.6 | `STAKEHOLDERS_ENGAGED` | Stakeholders Engaged | Roadmap set | Board/management reporting |
| 6.7 | `REFRESH_COMPLETE` | Refresh Complete | Reports delivered | Annual policy/control update |

---

## PART 3: INTER-STAGE GATES

These are the "big gates" that prevent building on weak foundations:

| From Stage | To Stage | Gate Requirements |
|------------|----------|-------------------|
| 1. Assessment | 2. Risk | Assessment ≥80% complete, critical findings extracted, evidence coverage ≥60% |
| 2. Risk | 3. Compliance | All Critical/High risks have treatment decisions, risk-control mapping ≥80% |
| 3. Compliance | 4. Resilience | Compliance score ≥70%, no unaddressed critical gaps (without approved exceptions) |
| 4. Resilience | 5. Excellence | Resilience score ≥70%, at least 1 DR/BC drill completed, RTO/RPO defined |
| 5. Excellence | 6. Sustainability | Maturity Level ≥3 (Defined), no critical audit findings open |

---

## PART 4: SERIAL CODE STANDARD

### 4.1 What Gets a Serial Code

Every auditable artifact receives an immutable serial code:

| Artifact Type | Code Prefix | Example |
|---------------|-------------|---------|
| Tenant/Organization | ORG | ORG-ACME-2026-0001 |
| Assessment | ASM | ASM-ACME-S1-2026-0042-V1 |
| Assessment Requirement | REQ | REQ-ACME-S1-2026-0042-001 |
| Risk | RSK | RSK-ACME-S2-2026-0089-V2 |
| Control | CTL | CTL-ACME-S3-2026-0156 |
| Evidence | EVD | EVD-ACME-S3-2026-0234-V1 |
| Compliance Gap | GAP | GAP-ACME-S3-2026-0012 |
| Action Plan | ACT | ACT-ACME-S3-2026-0045 |
| Exception | EXC | EXC-ACME-S3-2026-0008 |
| Approval | APR | APR-ACME-S1-2026-0042-L2 |
| Attestation | ATT | ATT-ACME-S5-2026-Q1 |

### 4.2 Serial Code Format

```
{PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}

Example: EVD-ACME-S3-2026-0234-V1
         │    │     │   │    │     └── Version (for revisions)
         │    │     │   │    └────── Sequence (auto-increment)
         │    │     │   └───────── Year
         │    │     └──────────── Stage (S1-S6)
         │    └────────────────── Tenant code (4-6 chars)
         └─────────────────────── Artifact type prefix
```

### 4.3 Serial Code Rules

1. **Immutable**: Once issued, never changes (even if item deleted/archived)
2. **Sequential**: Per tenant + artifact type + year
3. **Versioned**: V1, V2, V3... for revisions (evidence, documents)
4. **Linked**: Every child references parent serial code
5. **Auditable**: Issuance logged with timestamp + actor

### 4.4 Cross-Reference Chain

```
Assessment ASM-ACME-S1-2026-0042
    │
    ├── Requirement REQ-ACME-S1-2026-0042-001
    │       │
    │       └── Evidence EVD-ACME-S3-2026-0234 (linked)
    │
    ├── Finding → Risk RSK-ACME-S2-2026-0089
    │       │
    │       ├── Control CTL-ACME-S3-2026-0156 (mitigating)
    │       │
    │       └── Action ACT-ACME-S3-2026-0045
    │
    └── Approval APR-ACME-S1-2026-0042-L1
                APR-ACME-S1-2026-0042-L2
                APR-ACME-S1-2026-0042-L3
```

---

## PART 5: GATE EVALUATION ENGINE

### 5.1 Gate Definition Model

```yaml
gate_definition:
  id: "GATE-ASM-S1-SUBMIT"
  name: "Assessment Submission Gate"
  applies_to: "Assessment"
  from_status: "SCORING"
  to_status: "SUBMITTED"
  conditions:
    - type: "DataCompleteness"
      field: "scoring_completion"
      operator: "gte"
      threshold: 100
      message: "All requirements must be scored"
    - type: "EvidenceCoverage"
      field: "evidence_coverage_percent"
      operator: "gte"
      threshold: 60
      message: "Evidence coverage must be ≥60%"
    - type: "RequiredField"
      field: "summary"
      message: "Executive summary is required"
  override_policy:
    allowed_roles: ["ComplianceOfficer", "Executive"]
    requires_justification: true
    creates_exception: true
    exception_expiry_days: 30
```

### 5.2 Gate Evaluation Pipeline

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      GATE EVALUATION PIPELINE                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  REQUEST: Transition(workItemId, fromStatus, toStatus, actor)           │
│                           │                                             │
│                           ▼                                             │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ STEP 1: STATE MACHINE CHECK                                     │    │
│  │ Is this transition allowed in the workflow definition?          │    │
│  │ If NO → REJECT with "Invalid transition"                        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                           │ YES                                         │
│                           ▼                                             │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ STEP 2: COLLECT FACTS                                           │    │
│  │ - Data completeness metrics                                     │    │
│  │ - Score values                                                  │    │
│  │ - Evidence states                                               │    │
│  │ - Approval records                                              │    │
│  │ - SLA/time calculations                                         │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                           │                                             │
│                           ▼                                             │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ STEP 3: EVALUATE GATE CONDITIONS                                │    │
│  │ For each condition in gate_definition:                          │    │
│  │   - Compute actual value                                        │    │
│  │   - Compare to threshold                                        │    │
│  │   - Record pass/fail + computed value                           │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                           │                                             │
│                           ▼                                             │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ STEP 4: DETERMINE RESULT                                        │    │
│  │                                                                 │    │
│  │ ALL CONDITIONS PASS?                                            │    │
│  │   YES → Execute transition, log audit event                     │    │
│  │   NO  → Return failure with:                                    │    │
│  │         - Failed conditions list                                │    │
│  │         - Computed values                                       │    │
│  │         - Suggested actions to fix                              │    │
│  │         - Override option (if policy allows)                    │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                           │                                             │
│                           ▼                                             │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ STEP 5: AUDIT + NOTIFY                                          │    │
│  │ - Create immutable audit event                                  │    │
│  │ - Notify stakeholders (success or failure)                      │    │
│  │ - Update dashboards                                             │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.3 Gate Evaluation Result

```json
{
  "transitionId": "TRX-2026-01-09-001234",
  "workItemId": "ASM-ACME-S1-2026-0042",
  "fromStatus": "SCORING",
  "toStatus": "SUBMITTED",
  "passed": false,
  "evaluatedAt": "2026-01-09T14:30:00Z",
  "evaluatedBy": "user:ahmed@acme.com",
  "conditions": [
    {
      "conditionId": "scoring_completion",
      "passed": true,
      "threshold": 100,
      "actualValue": 100,
      "message": null
    },
    {
      "conditionId": "evidence_coverage",
      "passed": false,
      "threshold": 60,
      "actualValue": 45,
      "message": "Evidence coverage 45% < 60% required"
    }
  ],
  "suggestedActions": [
    {
      "action": "UPLOAD_EVIDENCE",
      "target": "REQ-ACME-S1-2026-0042-007",
      "description": "Upload evidence for requirement 007"
    },
    {
      "action": "UPLOAD_EVIDENCE",
      "target": "REQ-ACME-S1-2026-0042-012",
      "description": "Upload evidence for requirement 012"
    }
  ],
  "overrideAvailable": true,
  "overrideRequires": ["ComplianceOfficer", "Executive"]
}
```

---

## PART 6: AUTONOMOUS EXECUTION LOOP

### 6.1 What Can Be Automated (Safe)

| Automation | Trigger | Action | Human Approval |
|------------|---------|--------|----------------|
| Auto-create assessment requirements | Assessment created | Generate from template | No |
| Auto-request evidence | Requirement activated | Send request to control owner | No |
| Auto-calculate scores | Evidence approved | Recompute compliance % | No |
| Auto-create risks from findings | Assessment finding flagged | Create draft risk | No |
| Auto-escalate overdue | SLA breach | Notify manager chain | No |
| Auto-route approvals | Item submitted | Assign to approver queue | No |
| Auto-close low risks | Risk accepted + monitored 90 days | Move to closed | Yes (configurable) |
| Auto-archive | Retention period passed | Archive with hash | No |

### 6.2 What Requires Human Approval

| Action | Approver Role | Justification |
|--------|---------------|---------------|
| Risk acceptance (Critical/High) | Risk Owner + Executive | Regulatory requirement |
| Compliance attestation | Compliance Officer + Executive | Legal accountability |
| Exception approval | Compliance Officer | Audit trail requirement |
| Certification submission | Executive | External commitment |
| Gate override | As per override policy | Deviation from standard |

### 6.3 Autonomous Loop Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     AUTONOMOUS EXECUTION LOOP                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─── EVENT TRIGGERS ───────────────────────────────────────────────┐   │
│  │                                                                   │   │
│  │  • Evidence uploaded/approved/rejected                            │   │
│  │  • Requirement status changed                                     │   │
│  │  • Risk score recalculated                                        │   │
│  │  • SLA threshold approaching/breached                             │   │
│  │  • Task completed                                                 │   │
│  │  • Scheduled job triggered                                        │   │
│  │                                                                   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│                              ▼                                          │
│  ┌─── RECOMPUTE METRICS ────────────────────────────────────────────┐   │
│  │                                                                   │   │
│  │  • Evidence coverage %                                            │   │
│  │  • Compliance score                                               │   │
│  │  • Risk score (inherent/residual)                                 │   │
│  │  • Maturity level                                                 │   │
│  │  • Readiness indicators                                           │   │
│  │                                                                   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│                              ▼                                          │
│  ┌─── EVALUATE GATES ───────────────────────────────────────────────┐   │
│  │                                                                   │   │
│  │  For each work item affected:                                     │   │
│  │    - Check if gate to next step now passes                        │   │
│  │    - Check if inter-stage gate now passes                         │   │
│  │                                                                   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│                              ▼                                          │
│  ┌─── TAKE ACTION ──────────────────────────────────────────────────┐   │
│  │                                                                   │   │
│  │  IF gate passes AND auto-transition allowed:                      │   │
│  │    → Execute transition automatically                             │   │
│  │    → Create audit event                                           │   │
│  │                                                                   │   │
│  │  IF gate passes AND approval required:                            │   │
│  │    → Create approval task (one-click approve)                     │   │
│  │    → Notify approver                                              │   │
│  │                                                                   │   │
│  │  IF gate fails:                                                   │   │
│  │    → Generate actionable tasks for failed conditions              │   │
│  │    → Notify responsible parties                                   │   │
│  │                                                                   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│                              ▼                                          │
│  ┌─── AUDIT + DASHBOARD ────────────────────────────────────────────┐   │
│  │                                                                   │   │
│  │  • Log all evaluations and actions                                │   │
│  │  • Update real-time dashboards                                    │   │
│  │  • Emit events for integrations                                   │   │
│  │                                                                   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.4 Scheduled Jobs

| Job | Frequency | Purpose |
|-----|-----------|---------|
| Metric Recompute | Every 15 min | Refresh all scores and coverage |
| SLA Monitor | Every 30 min | Check approaching/breached SLAs |
| Escalation Scan | Hourly | Escalate overdue items |
| Gate Sweep | Daily | Re-evaluate all blocked items |
| Trend Analysis | Weekly | Generate trend reports |
| Health Check | Quarterly | Full lifecycle health review |

---

## PART 7: KANBAN + ADVANCED VIEWS

### 7.1 View Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          VIEW TOGGLE                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  [📋 List]  [📊 Kanban]  [📈 Timeline]  [🔀 Swimlane]  [📉 Analytics]   │
│                                                                         │
│  All views read from the SAME data source (WorkflowInstance + WorkItem) │
│  All actions call the SAME transition API (with gate validation)        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Kanban Board Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ HEADER: [+ New] [Filter ▼] [Group By ▼] [My Items ☑] [Search...]           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  DRAFT (3)     IN_PROGRESS (5)   SUBMITTED (2)    APPROVED (8)             │
│  ┌─────────┐   ┌─────────┐       ┌─────────┐      ┌─────────┐              │
│  │ ▓▓▓▓▓▓▓ │   │ ▓▓▓▓▓▓▓ │       │ ▓▓▓▓▓▓▓ │      │ ▓▓▓▓▓▓▓ │              │
│  │ Card 1  │   │ Card 4  │       │ Card 9  │      │ Card 11 │              │
│  │ ─────── │   │ ─────── │       │ ─────── │      │ ─────── │              │
│  │ [Start] │   │ [Submit]│       │[Approve]│      │ [Close] │              │
│  └─────────┘   └─────────┘       └─────────┘      └─────────┘              │
│  ┌─────────┐   ┌─────────┐       ┌─────────┐      ┌─────────┐              │
│  │ Card 2  │   │ Card 5  │       │ Card 10 │      │ Card 12 │              │
│  └─────────┘   └─────────┘       └─────────┘      └─────────┘              │
│  ┌─────────┐   ┌─────────┐                        ...                      │
│  │ Card 3  │   │ Card 6  │                                                 │
│  └─────────┘   └─────────┘                                                 │
│                ┌─────────┐                                                 │
│                │ Card 7  │                                                 │
│                └─────────┘                                                 │
│                ┌─────────┐                                                 │
│                │ Card 8  │                                                 │
│                └─────────┘                                                 │
│                                                                             │
│  ← DRAG CARDS TO TRANSITION (with gate validation) →                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.3 Card Anatomy with Direct Actions

```
┌────────────────────────────────────────────────────────────────┐
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │ ← Priority color bar
├────────────────────────────────────────────────────────────────┤
│ ASM-ACME-S1-2026-0042                           [⋮ Menu]       │
│                                                                │
│ ┌────────────────────────────────────────────────────────────┐ │
│ │ Q1 NCA-ECC Compliance Assessment                           │ │
│ └────────────────────────────────────────────────────────────┘ │
│                                                                │
│ 📂 Framework: NCA-ECC                                          │
│ 📅 Due: Jan 31, 2026                                           │
│ ⏱️ SLA: 12 days remaining                                      │
│ ⚠️ Gate: 2 blockers                                            │
│                                                                │
│ ┌────────────────────────────────────────────────────────────┐ │
│ │ ████████████████░░░░░░░░░░ 65%                             │ │ ← Progress
│ └────────────────────────────────────────────────────────────┘ │
│                                                                │
│ 📎 12  💬 3  ⚠️ 2                                               │ ← Badges
├────────────────────────────────────────────────────────────────┤
│ QUICK ACTIONS:                                                 │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐                        │
│ │ ▶ Submit │ │ 👁 View  │ │ 📝 Edit  │                        │ ← Direct actions
│ └──────────┘ └──────────┘ └──────────┘                        │
├────────────────────────────────────────────────────────────────┤
│ 👤 Ahmed Al-Rashid                                   [Avatar]  │
└────────────────────────────────────────────────────────────────┘
```

### 7.4 Status-Based Action Matrix

| Current Status | Primary Action | Secondary Actions | Blocked Actions |
|----------------|----------------|-------------------|-----------------|
| DRAFT | ▶ Start | Schedule, Edit, Delete | Submit, Approve |
| SCHEDULED | ▶ Begin | Reschedule, Cancel | Submit, Approve |
| IN_PROGRESS | 📤 Submit | Add Evidence, Comment, Pause | Approve, Delete |
| SUBMITTED | ✓ Approve | Reject, Request Changes | Start, Delete |
| UNDER_REVIEW | ✓ Approve | Reject, Add Reviewer | Start, Cancel |
| APPROVED | ✓ Complete | Generate Report | Reject, Delete |
| REJECTED | 🔄 Resubmit | Edit, View Comments | Approve, Complete |
| COMPLETED | 📥 Archive | Export, Clone | Edit, Delete |

### 7.5 Drag-Drop with Gate Validation

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DRAG-DROP TRANSITION FLOW                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  USER DRAGS CARD                                                        │
│        │                                                                │
│        ▼                                                                │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ UI highlights valid drop zones (based on state machine)         │    │
│  │   Green = Valid transition                                      │    │
│  │   Red = Invalid (state machine blocks)                          │    │
│  │   Gray = Not applicable                                         │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│        │                                                                │
│        ▼ (card dropped)                                                 │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ UI calls: POST /api/workflow/{id}/transition                    │    │
│  │ Body: { fromStatus, toStatus, comment? }                        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│        │                                                                │
│        ▼                                                                │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ BACKEND: Gate Evaluation                                        │    │
│  │   → All gates pass? Execute transition                          │    │
│  │   → Gates fail? Return failure + blockers                       │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│        │                                                                │
│        ▼                                                                │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ IF SUCCESS:                                                     │    │
│  │   → Move card to new column                                     │    │
│  │   → Show success toast                                          │    │
│  │                                                                 │    │
│  │ IF FAILURE:                                                     │    │
│  │   → Show GATE MODAL with:                                       │    │
│  │     • Failed conditions                                         │    │
│  │     • Current values vs thresholds                              │    │
│  │     • Action buttons to fix each blocker                        │    │
│  │     • Override option (if role allows)                          │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.6 Gate Blocker Modal

```
┌────────────────────────────────────────────────────────────────────────┐
│                    ⚠️ TRANSITION BLOCKED                                │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  Moving "Q1 NCA-ECC Assessment" to SUBMITTED                           │
│                                                                        │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ GATE CHECK RESULTS                                               │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │ ✓ Scoring completion: 100% ≥ 100%                                │  │
│  │ ✗ Evidence coverage: 45% < 60%                     [Upload...]   │  │
│  │ ✗ Executive summary: Missing                       [Add...]      │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                        │
│  MISSING EVIDENCE FOR:                                                 │
│  • REQ-ACME-S1-2026-0042-007 - Access Control Policy    [Upload]      │
│  • REQ-ACME-S1-2026-0042-012 - Incident Response Plan   [Upload]      │
│  • REQ-ACME-S1-2026-0042-018 - Training Records         [Upload]      │
│                                                                        │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  [Cancel]                     [Request Override]      [Fix & Retry]    │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## PART 8: IMPLEMENTATION ROADMAP

### Phase 1: Foundation (Days 0-30)

| Deliverable | Priority | Description |
|-------------|----------|-------------|
| Serial Code Service | P0 | Generate immutable codes for all artifacts |
| Audit Event Ledger | P0 | Immutable audit trail for all actions |
| Gate Engine | P0 | Shared gate evaluation service |
| State Machine | P0 | Allowed transitions per artifact type |
| Transition API | P0 | Single API for all status changes |

### Phase 2: Stage Completion (Days 31-60)

| Deliverable | Priority | Description |
|-------------|----------|-------------|
| Risk Stage Hardening | P1 | Complete 8-step workflow with gates |
| Compliance Stage Hardening | P1 | Complete 8-step workflow with gates |
| Inter-Stage Gates | P1 | Implement stage-to-stage validation |
| Gap Remediation Workflow | P1 | Track gap closure with actions |

### Phase 3: Execution UX (Days 61-90)

| Deliverable | Priority | Description |
|-------------|----------|-------------|
| Kanban View | P2 | Drag-drop with gate validation |
| View Toggle | P2 | List/Kanban/Timeline/Swimlane |
| Direct Actions | P2 | Inline buttons on cards |
| Gate Modal | P2 | Blocker display with fix actions |

### Phase 4: Autonomy (Days 91-120)

| Deliverable | Priority | Description |
|-------------|----------|-------------|
| Event Triggers | P3 | Auto-evaluate gates on changes |
| Scheduled Jobs | P3 | SLA, escalation, trend analysis |
| Auto-Transitions | P3 | Move items when gates pass |
| AI Advisory | P3 | Evidence quality, recommendations |

---

## PART 9: CONFIGURATION CATALOG

### 9.1 Master Status Catalog

```yaml
statuses:
  assessment:
    - code: DRAFT
      sequence: 1
      entry_auto: false
      exit_gates: [scope_defined, owner_assigned]
    - code: SCHEDULED
      sequence: 2
      exit_gates: [dates_set, assessors_assigned]
    - code: IN_PROGRESS
      sequence: 3
      exit_gates: [progress_minimum_40]
    - code: EVIDENCE_COLLECTION
      sequence: 4
      exit_gates: [evidence_coverage_60]
    - code: SCORING
      sequence: 5
      exit_gates: [all_requirements_scored]
    - code: SUBMITTED
      sequence: 6
      exit_gates: [review_package_complete]
    - code: UNDER_REVIEW
      sequence: 7
      exit_gates: [review_comments_resolved]
    - code: APPROVED
      sequence: 8
      terminal: true

  risk:
    - code: IDENTIFIED
      sequence: 1
      exit_gates: [title_description_category_owner]
    - code: CONTEXTUALIZED
      sequence: 2
      exit_gates: [linked_to_asset_or_finding]
    - code: SCORED_INHERENT
      sequence: 3
      exit_gates: [likelihood_impact_calculated]
    - code: PENDING_DECISION
      sequence: 4
      exit_gates: [treatment_decision_made]
    - code: TREATMENT_PLANNED
      sequence: 5
      exit_gates: [strategy_controls_timeline]
    - code: MITIGATING
      sequence: 6
      exit_gates: [actions_in_progress]
    - code: SCORED_RESIDUAL
      sequence: 7
      exit_gates: [residual_within_appetite]
    - code: MONITORED_CLOSED
      sequence: 8
      terminal: true

  compliance:
    - code: OBLIGATIONS_MAPPED
      sequence: 1
      exit_gates: [frameworks_identified]
    - code: CONTROLS_MAPPED
      sequence: 2
      exit_gates: [control_coverage_80]
    - code: EVIDENCE_REQUESTED
      sequence: 3
      exit_gates: [requests_issued]
    - code: EVIDENCE_VERIFIED
      sequence: 4
      exit_gates: [verification_rate_70]
    - code: TESTED
      sequence: 5
      exit_gates: [tests_executed]
    - code: SCORED
      sequence: 6
      exit_gates: [score_calculated]
    - code: REMEDIATION
      sequence: 7
      exit_gates: [action_plans_for_gaps]
    - code: ATTESTED
      sequence: 8
      terminal: true

  evidence:
    - code: DRAFT
      sequence: 1
      exit_gates: [file_uploaded, metadata_complete]
    - code: SUBMITTED
      sequence: 2
      exit_gates: [quality_score_70]
    - code: UNDER_REVIEW
      sequence: 3
      exit_gates: [reviewer_comments_resolved]
    - code: APPROVED
      sequence: 4
      terminal: true
    - code: REJECTED
      sequence: 5
      terminal: false
      allows_resubmit: true
```

### 9.2 Gate Threshold Catalog

```yaml
thresholds:
  assessment:
    progress_minimum: 40
    evidence_coverage_minimum: 60
    scoring_completion: 100
    review_resolution: 100

  risk:
    critical_score: 20
    high_score: 12
    medium_score: 6
    appetite_default: 12
    treatment_plan_days_critical: 7
    treatment_plan_days_high: 30
    treatment_plan_days_medium: 90

  compliance:
    control_coverage_minimum: 80
    evidence_verification_minimum: 70
    compliant_threshold: 80
    partially_compliant_threshold: 50

  evidence:
    quality_score_minimum: 70
    max_age_days: 365

  resilience:
    strong_threshold: 70
    moderate_threshold: 50
    drill_frequency_months: 12

  maturity:
    level_5_optimizing: 80
    level_4_managed: 60
    level_3_defined: 40
    level_2_repeatable: 20
```

### 9.3 Override Policy Catalog

```yaml
overrides:
  assessment_submit_without_evidence:
    allowed_roles: [ComplianceOfficer, Executive]
    requires_justification: true
    creates_exception: true
    exception_expiry_days: 30
    notification_recipients: [RiskOwner, AuditCommittee]

  risk_accept_above_appetite:
    allowed_roles: [Executive, BoardDelegate]
    requires_justification: true
    requires_compensating_control: true
    creates_exception: true
    exception_expiry_days: 90
    notification_recipients: [RiskCommittee, Audit]

  compliance_attest_with_gaps:
    allowed_roles: [Executive]
    requires_justification: true
    requires_remediation_plan: true
    creates_exception: true
    blocks_certification: true
```

---

## APPENDIX A: RACI MATRIX FOR GRC LIFECYCLE

| Activity | GRC Owner | Compliance Officer | Risk Owner | Control Owner | Approver (L1-L3) |
|----------|-----------|-------------------|------------|---------------|------------------|
| Framework Selection | A | R | C | I | A |
| Assessment Planning | A | R | C | I | I |
| Evidence Collection | I | C | I | R | I |
| Risk Identification | C | C | R | C | I |
| Risk Treatment | C | C | A | R | A |
| Compliance Scoring | I | R | C | C | I |
| Gap Remediation | A | R | C | R | A |
| Attestation | A | R | C | I | A |

R = Responsible, A = Accountable, C = Consulted, I = Informed

---

## APPENDIX B: KSA REGULATOR FRAMEWORK MAPPING

| Regulator | Primary Framework | Sector Scope | Serial Prefix |
|-----------|-------------------|--------------|---------------|
| NCA | ECC, CSCC | All critical infrastructure | NCA- |
| SAMA | CSF, AML | Banking, Insurance, Finance | SAMA- |
| SDAIA | PDPL | All organizations processing personal data | PDPL- |
| CST | CRF | Telecom, ICT | CST- |
| MOH | HIS | Healthcare | MOH- |
| DGA | Cloud Policy | Government entities | DGA- |

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-09 | GRC Platform Team | Initial master transformation |

---

**END OF MASTER TRANSFORMATION SPECIFICATION**
