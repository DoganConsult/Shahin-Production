# Shahin AI GRC Platform - Full Plan Checklist

> Complete implementation checklist extracted from `fullplan` specification document.

---

## Table of Contents
1. [Onboarding Process](#1-onboarding-process)
2. [AI Agents](#2-ai-agents)
3. [Workflow State Machine](#3-workflow-state-machine)
4. [Field Registry](#4-field-registry)
5. [Conditional Logic Rules](#5-conditional-logic-rules)
6. [Agent Communication Contracts](#6-agent-communication-contracts)
7. [Data Model Alignment](#7-data-model-alignment)
8. [Advanced Engagement Features](#8-advanced-engagement-features)
9. [Roles & Permissions](#9-roles--permissions)
10. [Evidence Scoring Model](#10-evidence-scoring-model)
11. [Workflow Bindings](#11-workflow-bindings)
12. [Feature Flags](#12-feature-flags)
13. [Agent Role Overlays](#13-agent-role-overlays)
14. [Audit Replay Model](#14-audit-replay-model)
15. [Motivation Scoring](#15-motivation-scoring)
16. [Prompt Contracts](#16-prompt-contracts)

---

## 1. Onboarding Process

### 1.1 Simple Flow (4 Steps)
| # | Step | Description | Status |
|---|------|-------------|--------|
| [ ] | 1 | **Organization Profile** | Company name, industry, size, region |
| [ ] | 2 | **Framework Selection** | Select compliance frameworks (PDPL, PCI-DSS, ISO 27001, etc.) |
| [ ] | 3 | **Integration Setup** | SSO, cloud provider, existing policies |
| [ ] | 4 | **Plan Generation** | Auto-generate compliance plan |

### 1.2 Comprehensive Wizard (12 Sections)
| # | Section | Description | Status |
|---|---------|-------------|--------|
| [ ] | 1 | **Welcome & Context** | Introduction, value proposition |
| [ ] | 2 | **Organization Profile** | Legal name, industry, size, regions |
| [ ] | 3 | **Regulatory Landscape** | Applicable regulations by region |
| [ ] | 4 | **Framework Selection** | Choose target frameworks |
| [ ] | 5 | **Current State Assessment** | Existing controls, policies, gaps |
| [ ] | 6 | **Risk Appetite** | Risk tolerance levels |
| [ ] | 7 | **Integration Preferences** | SSO, cloud, existing tools |
| [ ] | 8 | **Team & Roles** | Assign compliance team members |
| [ ] | 9 | **Timeline & Priorities** | Set deadlines, prioritize frameworks |
| [ ] | 10 | **Evidence Strategy** | Manual vs automated evidence |
| [ ] | 11 | **Review & Confirm** | Summary review before submission |
| [ ] | 12 | **Plan Generation** | Generate Fast Start + Missions plan |

---

## 2. AI Agents

### 2.1 Core Agents
| # | Agent | Responsibility | Input Schema | Output Schema | Status |
|---|-------|----------------|--------------|---------------|--------|
| [ ] | **OnboardingAgent** | Guide user through onboarding wizard, collect org data | user_inputs, progress_state | onboarding_data (company_profile, frameworks, integrations) | |
| [ ] | **RulesEngineAgent** | Evaluate onboarding data against rules, determine frameworks | onboarding_data | effective_frameworks, special_flags | |
| [ ] | **PlanAgent** | Generate compliance plan with tasks and timeline | frameworks, special_flags | plan_id, tasks[], timeline | |
| [ ] | **WorkflowAgent** | Execute plan tasks, schedule, notify, track status | plan | task_statuses[], notifications_sent[] | |
| [ ] | **EvidenceAgent** | Collect & verify evidence for completed tasks | completed_tasks[], monitoring_triggers | evidence_records[], compliance_status | |
| [ ] | **DashboardAgent** | Aggregate data for dashboard display | task_statuses, evidence_records, plan | dashboard_data (progress, alerts, frameworks_status) | |
| [ ] | **NextBestActionAgent** | Recommend next actions based on context | inputs, decisionState, contextWindow | recommendations[] | |

### 2.2 Agent Triggers
| # | Agent | Trigger Events | Status |
|---|-------|----------------|--------|
| [ ] | OnboardingAgent | User initiates onboarding, Returns to incomplete wizard |
| [ ] | RulesEngineAgent | Onboarding data submitted, Manual rule refresh |
| [ ] | PlanAgent | Rules evaluated, Change in frameworks/flags |
| [ ] | WorkflowAgent | Plan ready, Task status changed |
| [ ] | EvidenceAgent | Task completed, Scheduled compliance check |
| [ ] | DashboardAgent | New data from agents, User refresh request |

### 2.3 Agent Fallback Behaviors
| # | Agent | Fallback Behavior | Status |
|---|-------|-------------------|--------|
| [ ] | OnboardingAgent | Save partial progress, allow resume later |
| [ ] | RulesEngineAgent | Default to base frameworks if rule lookup fails |
| [ ] | PlanAgent | Generate partial plan or placeholders if template missing |
| [ ] | WorkflowAgent | Flag issue, escalate to coordinator if task blocked |
| [ ] | EvidenceAgent | Request manual evidence if auto-collection fails |
| [ ] | DashboardAgent | Show last known status with warning if data delayed |

---

## 3. Workflow State Machine

### 3.1 States
| # | State | Description | Agent Action | Status |
|---|-------|-------------|--------------|--------|
| [ ] | **Onboarding** | Gathering initial info | OnboardingAgent prompts user, collects data |
| [ ] | **RulesEvaluation** | Processing rules | RulesEngineAgent evaluates frameworks/flags |
| [ ] | **PlanGeneration** | Creating compliance plan | PlanAgent creates task list + timeline |
| [ ] | **ExecuteFastStart** | Executing baseline tasks | WorkflowAgent schedules Fast Start tasks |
| [ ] | **FastStartComplete** | Baseline controls done | Notify user, enable mission selection |
| [ ] | **ExecuteMission** | Executing mission tasks | WorkflowAgent executes Mission tasks |
| [ ] | **MissionComplete** | Mission finished | Record completion, update status |
| [ ] | **Monitoring** | Continuous monitoring | EvidenceAgent collects; DashboardAgent updates |

### 3.2 State Transitions
| # | From State | Trigger | To State | Status |
|---|------------|---------|----------|--------|
| [ ] | Onboarding | onboarding_complete | RulesEvaluation |
| [ ] | RulesEvaluation | rules_evaluated | PlanGeneration |
| [ ] | PlanGeneration | plan_ready | ExecuteFastStart |
| [ ] | ExecuteFastStart | fast_start_tasks_completed | FastStartComplete |
| [ ] | FastStartComplete | mission_selected | ExecuteMission |
| [ ] | FastStartComplete | no_mission_selected | Monitoring |
| [ ] | ExecuteMission | mission_completed | MissionComplete |
| [ ] | MissionComplete | another_mission_selected | ExecuteMission |
| [ ] | MissionComplete | all_missions_completed | Monitoring |
| [ ] | Monitoring | compliance_drift_detected | PlanGeneration |
| [ ] | Monitoring | new_requirement_added | PlanGeneration |

---

## 4. Field Registry

### 4.1 Organization Profile Fields
| # | Field ID | Type | Required | Description | Status |
|---|----------|------|----------|-------------|--------|
| [ ] | company_name | string | Yes | Legal company name |
| [ ] | industry | string | Yes | Industry sector (FinTech, Healthcare, etc.) |
| [ ] | company_size | number | Yes | Employee count |
| [ ] | region | string | Yes | Primary operating region |
| [ ] | headquarters_country | string | Yes | Country of HQ |
| [ ] | subsidiaries | array | No | List of subsidiary locations |

### 4.2 Framework Selection Fields
| # | Field ID | Type | Required | Description | Status |
|---|----------|------|----------|-------------|--------|
| [ ] | frameworks_selected | array | Yes | Selected compliance frameworks |
| [ ] | primary_framework | string | No | Main framework to prioritize |
| [ ] | certification_targets | array | No | Target certifications |

### 4.3 Integration Fields
| # | Field ID | Type | Required | Description | Status |
|---|----------|------|----------|-------------|--------|
| [ ] | use_sso | boolean | Yes | SSO enabled |
| [ ] | sso_provider | string | No | SSO provider name |
| [ ] | cloud_provider | string | No | AWS, Azure, GCP, etc. |
| [ ] | has_policies | boolean | Yes | Existing security policies |
| [ ] | existing_tools | array | No | Current GRC/security tools |

### 4.4 Output Fields
| # | Field ID | Type | Description | Status |
|---|----------|------|-------------|--------|
| [ ] | effective_frameworks | array | Final list after rules evaluation |
| [ ] | special_flags.requireSSOConfig | boolean | SSO setup needed |
| [ ] | special_flags.dualOverlay | boolean | Dual framework overlay applied |
| [ ] | plan_id | string | Generated plan identifier |
| [ ] | tasks | array | List of compliance tasks |
| [ ] | timeline.FastStart | string | Fast Start completion date |
| [ ] | timeline.Missions | string | Missions completion date |

---

## 5. Conditional Logic Rules

### 5.1 SSO Configuration
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | use_sso: true | Launch SSO configuration workflow |
| [ ] | use_sso: true | Set special_flags.requireSSOConfig = true |

### 5.2 Dual Framework Overlay
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | frameworks include PDPL + PCI-DSS | Apply dual compliance overlay |
| [ ] | frameworks include PDPL + PCI-DSS | Set special_flags.dualOverlay = true |

### 5.3 Multiple Frameworks
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | multiple_frameworks: true | Unify common controls (avoid duplicates) |

### 5.4 Cloud Provider Rules
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | cloud_provider: AWS | Include AWS-specific security controls |
| [ ] | cloud_provider: AWS | Exclude other cloud provider tasks |
| [ ] | cloud_provider: Azure | Include Azure-specific controls |
| [ ] | cloud_provider: GCP | Include GCP-specific controls |

### 5.5 Policy Rules
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | has_policies: false | Add task to develop baseline security policies |

### 5.6 Company Size Rules
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | company_size: small | Simplify tasks (reduced scope) |
| [ ] | company_size: small | Adjust timeline for limited resources |

### 5.7 Region-Based Rules
| # | Condition | Action | Status |
|---|-----------|--------|--------|
| [ ] | region: Saudi Arabia | Auto-add PDPL to effective frameworks |
| [ ] | region: EU | Auto-add GDPR to effective frameworks |

---

## 6. Agent Communication Contracts

### 6.1 OnboardingAgent → RulesEngineAgent
| # | Item | Description | Status |
|---|------|-------------|--------|
| [ ] | Request Schema | company_profile, frameworks_selected, integrations |
| [ ] | Response Schema | effective_frameworks, special_flags |
| [ ] | Error: MissingData | Prompt OnboardingAgent to supply missing fields |
| [ ] | Error: InvalidFramework | Halt workflow, notify user |
| [ ] | Validation | All required fields present, frameworks recognized |

### 6.2 RulesEngineAgent → PlanAgent
| # | Item | Description | Status |
|---|------|-------------|--------|
| [ ] | Request Schema | frameworks, special_flags |
| [ ] | Response Schema | plan_id, tasks[], timeline |
| [ ] | Error: TemplateMissing | Respond with partial plan or error |
| [ ] | Error: InvalidFlags | Ignore unrecognized flags, proceed |
| [ ] | Validation | Frameworks list not empty, at least one task per framework |

### 6.3 PlanAgent → WorkflowAgent
| # | Item | Description | Status |
|---|------|-------------|--------|
| [ ] | Request Schema | plan (plan_id, tasks, timeline) |
| [ ] | Response Schema | execution_id, status |
| [ ] | Error: ScheduleConflict | Adjust task times, return warning |
| [ ] | Error: InvalidTaskData | Reject execution, respond with error |
| [ ] | Validation | Non-empty tasks list, no circular dependencies |

### 6.4 WorkflowAgent → EvidenceAgent
| # | Item | Description | Status |
|---|------|-------------|--------|
| [ ] | Request Schema | completed_task (task_id, completed_on, requires_evidence) |
| [ ] | Response Schema | evidence_request_id, status |
| [ ] | Error: EvidenceSourceNotFound | Return error, prompt user for manual evidence |
| [ ] | Error: NoEvidenceRequired | Return status 'not_required' |
| [ ] | Validation | requires_evidence is boolean, task_id maps to known control |

### 6.5 EvidenceAgent → DashboardAgent
| # | Item | Description | Status |
|---|------|-------------|--------|
| [ ] | Request Schema | update (task_id, evidence_status, compliance_status) |
| [ ] | Response Schema | dashboard_refresh (boolean) |
| [ ] | Error: DashboardOffline | Return false, queue update |
| [ ] | Validation | Valid task_id and status fields |

---

## 7. Data Model Alignment

### 7.1 Field Propagation Mappings
| # | Source Field | Propagates To | Impact | Status |
|---|--------------|---------------|--------|--------|
| [ ] | onboarding.frameworks_selected | RulesEngine, PlanAgent, Dashboard | Determines tasks and tracking |
| [ ] | onboarding.integrations.use_sso | RulesEngine, special_flags, PlanAgent | Triggers SSO tasks |
| [ ] | onboarding.company_profile.region | RulesEngine, effective_frameworks | Auto-adds regional regulations |
| [ ] | special_flags.dualOverlay | PlanAgent, Dashboard | Merges overlapping controls |
| [ ] | PlanAgent.output.tasks | WorkflowAgent, Dashboard | Defines work breakdown |
| [ ] | WorkflowAgent.output.task_statuses | EvidenceAgent, Dashboard | Drives evidence collection |
| [ ] | EvidenceAgent.output.evidence_records | Dashboard | Provides compliance proof |

---

## 8. Advanced Engagement Features

### 8.1 Progress Intelligence
| # | Feature | Description | Status |
|---|---------|-------------|--------|
| [ ] | **Progress Certainty Index (PCI)** | Score 0-100 predicting compliance completion |
| [ ] | PCI Inputs | % tasks completed, velocity trend, rejection rate, SLA breaches |
| [ ] | PCI Output | risk_band, primary_risk_factors, recommended_intervention |

### 8.2 Next Best Action Engine
| # | Action Type | Description | Status |
|---|-------------|-------------|--------|
| [ ] | Remind | Send reminder to owner |
| [ ] | Reassign | Transfer task to another owner |
| [ ] | Split task | Break large task into smaller ones |
| [ ] | Auto-collect evidence | Trigger automated evidence gathering |
| [ ] | Reduce scope | Defer non-mandatory controls |
| [ ] | Escalate | Notify manager/admin |
| [ ] | Pause & explain | Stop and explain situation |

### 8.3 Explainability Features
| # | Feature | Description | Status |
|---|---------|-------------|--------|
| [ ] | Human-Readable Rationale | Every decision includes "because" field |
| [ ] | Alternatives Considered | Show rejected options |
| [ ] | Confidence Level | 0-1 score for decision confidence |

### 8.4 Motivation Mechanics
| # | Feature | Description | Status |
|---|---------|-------------|--------|
| [ ] | Mission-Based Framing | Show mission progress, not raw task counts |
| [ ] | Delta vs Baseline | Show days ahead/behind schedule |
| [ ] | Micro-Wins Engine | Confirm actions with benefit statements |
| [ ] | Smart Scope Reduction | Propose scope optimization when burnout detected |

### 8.5 Advanced Automation
| # | Feature | Description | Status |
|---|---------|-------------|--------|
| [ ] | Evidence Autopilot Mode | Auto-accept evidence with high confidence |
| [ ] | Predictive Delay Detection | Forecast delays before they happen |
| [ ] | Control Reuse Intelligence | Cross-framework control mapping |

### 8.6 UI Panels
| # | Panel | Description | Status |
|---|-------|-------------|--------|
| [ ] | Live Preview Panel | Real-time control/task changes |
| [ ] | "Why This Exists" Panel | Origin, risk, evidence, beneficiary |
| [ ] | Dashboard Data | progress, alerts, frameworks_status |

### 8.7 Governance & Safety
| # | Feature | Description | Status |
|---|---------|-------------|--------|
| [ ] | Kill-Switch | Human override for all agent actions |
| [ ] | Override Logging | Record who overrode and why |
| [ ] | Audit Replay Mode | Time-travel through compliance history |

---

## 9. Roles & Permissions

### 9.1 Predefined Roles
| # | Role | Description | Status |
|---|------|-------------|--------|
| [ ] | **OrgAdmin** | Full access, manage users/roles/controls/policies |
| [ ] | **ComplianceLead** | Manage compliance program, override decisions |
| [ ] | **Assessor** | Review controls, approve/reject evidence |
| [ ] | **ControlOwner** | Operate assigned controls, submit evidence |
| [ ] | **EvidenceCustodian** | Manage evidence collection and storage |
| [ ] | **Approver** | Approve exceptions and changes |
| [ ] | **RemediationOwner** | Address findings and control failures |
| [ ] | **InternalAuditLiaison** | Coordinate with internal audit |
| [ ] | **Auditor** | Conduct audits, record findings |
| [ ] | **ExecutiveViewer** | View dashboards and reports |
| [ ] | **Viewer** | Read-only access |

### 9.2 Permission Matrix
| # | Role | Permissions | Status |
|---|------|-------------|--------|
| [ ] | OrgAdmin | manage_users, manage_roles, manage_controls, manage_policies, manage_sla, override_decisions, view_reports, view_all_data, assign_controls |
| [ ] | ComplianceLead | manage_controls, manage_policies, override_decisions, view_reports, assign_controls |
| [ ] | Assessor | view_controls, review_evidence, approve_evidence, assign_remediation, escalate_issue, view_reports |
| [ ] | Auditor | view_controls, request_evidence, record_finding, initiate_audit, create_issue, view_reports, escalate_issue |
| [ ] | ControlOwner | view_controls, submit_evidence, request_exception, view_tasks |
| [ ] | EvidenceCustodian | view_controls, submit_evidence, manage_evidence_sources |
| [ ] | RemediationOwner | view_tasks, update_remediation, resolve_issue |
| [ ] | Viewer | view_controls, view_reports |
| [ ] | BusinessUser | view_controls, request_exception, view_reports |

### 9.3 Policy Bindings
| # | Role | SLA Authority | Override Ability | Exception Request | Status |
|---|------|---------------|------------------|-------------------|--------|
| [ ] | OrgAdmin | Yes | Yes | No |
| [ ] | ComplianceLead | Yes | Yes | No |
| [ ] | Assessor | No | No | No |
| [ ] | Auditor | No | No | No |
| [ ] | ControlOwner | No | No | Yes |
| [ ] | EvidenceCustodian | No | No | Yes |
| [ ] | RemediationOwner | No | No | No |
| [ ] | Viewer | No | No | No |
| [ ] | BusinessUser | No | No | Yes |

### 9.4 Access Scopes
| # | Role | Scope | Status |
|---|------|-------|--------|
| [ ] | OrgAdmin | Global (all data and functions) |
| [ ] | ComplianceLead | Global (compliance data) |
| [ ] | Assessor | All controls (program-wide) |
| [ ] | Auditor | All controls (organization-wide) |
| [ ] | ControlOwner | Assigned controls only |
| [ ] | EvidenceCustodian | Evidence sources and records |
| [ ] | RemediationOwner | Assigned remediation tasks |
| [ ] | Viewer | Limited (read-only subset) |
| [ ] | BusinessUser | Own department/business unit |

---

## 10. Evidence Scoring Model

### 10.1 Scoring Metrics
| # | Metric | Description | Scale | Status |
|---|--------|-------------|-------|--------|
| [ ] | **Confidence Score** | Trust in evidence reliability | 0-100 |
| [ ] | **Automation Coverage** | % automated vs manual collection | 0-100% |
| [ ] | **SLA Adherence** | Timeliness of submission | 0-100% |
| [ ] | **Quality Score** | Completeness and relevance | 0-100 or 1-5 |

### 10.2 Evidence Status Levels
| # | Status | Description | Status |
|---|--------|-------------|--------|
| [ ] | verified | Evidence validated and approved |
| [ ] | rejected | Evidence does not meet requirements |
| [ ] | pending_review | Awaiting assessor review |
| [ ] | not_required | Task does not require evidence |
| [ ] | evidence_requested | Request sent to owner |

### 10.3 Evidence Collection Modes
| # | Mode | Description | Status |
|---|------|-------------|--------|
| [ ] | Manual | Human uploads evidence |
| [ ] | Automated | System collects via integrations |
| [ ] | AutoAccept | Auto-approve for high-confidence sources |
| [ ] | Hybrid | Combination of manual and automated |

---

## 11. Workflow Bindings

### 11.1 Workflow Triggers
| # | Workflow | Initiator | Action | Target | Outcome | Status |
|---|----------|-----------|--------|--------|---------|--------|
| [ ] | ControlReassignment | Admin | reassign_control | ControlOwner | Ownership transferred |
| [ ] | EvidenceSubmission | ControlOwner | submit_evidence | Assessor | Evidence queued for review |
| [ ] | EvidenceApproval | Assessor | approve_evidence | ControlOwner | Control marked compliant/non-compliant |
| [ ] | ExceptionRequest | ControlOwner | request_exception | Admin | Exception workflow initiated |
| [ ] | AuditInitiation | Auditor | initiate_audit | ControlOwner(s) | Audit launched |
| [ ] | IssueCreation | Auditor | record_finding | RemediationOwner | New issue logged |
| [ ] | RemediationTaskCreation | Assessor | create_remediation_task | RemediationOwner | Remediation task created |
| [ ] | RemediationCompletion | RemediationOwner | resolve_issue | Assessor | Remediation marked complete |
| [ ] | IssueEscalation | WorkflowAgent | escalate_issue | Admin | Overdue task escalated |

---

## 12. Feature Flags

### 12.1 Role-Based Feature Access
| # | Feature | Enabled For | Status |
|---|---------|-------------|--------|
| [ ] | LivePreviewPanel | OrgAdmin, ComplianceLead, Assessor |
| [ ] | NextBestActionPanel | OrgAdmin, ComplianceLead, ControlOwner, EvidenceCustodian, RemediationOwner |
| [ ] | RulesExplainability | OrgAdmin, ComplianceLead, Assessor, Auditor, InternalAuditLiaison |
| [ ] | AutoEvidenceCollection | OrgAdmin, ComplianceLead, EvidenceCustodian |
| [ ] | AuditReplayMode | OrgAdmin, ComplianceLead, Auditor, InternalAuditLiaison |
| [ ] | ScopeOptimizer | ComplianceLead, OrgAdmin |
| [ ] | OverrideBaseline | ComplianceLead, OrgAdmin |

---

## 13. Agent Role Overlays

### 13.1 Agent Behavior by Role
| # | Agent | OrgAdmin | ComplianceLead | Assessor | ControlOwner | Auditor | Viewer | Status |
|---|-------|----------|----------------|----------|--------------|---------|--------|--------|
| [ ] | OnboardingAgent | Full config | Full config | View | Limited | View | None |
| [ ] | RulesEngineAgent | Override | Override | View | None | View | None |
| [ ] | PlanAgent | Edit | Edit | View | View assigned | View | None |
| [ ] | WorkflowAgent | Full | Full | Assign | View tasks | View | None |
| [ ] | EvidenceAgent | Override | Override | Review | Submit | Request | None |
| [ ] | DashboardAgent | Full | Full | Filtered | Personal | Audit view | Limited |
| [ ] | NextBestActionAgent | All actions | All actions | Review | Personal | Audit | None |

---

## 14. Audit Replay Model

### 14.1 Audit Event Types
| # | Event Type | Description | Status |
|---|------------|-------------|--------|
| [ ] | agentDecision | Record of agent decision output |
| [ ] | uiAction | User interaction in UI |
| [ ] | stateTransition | Workflow state change |
| [ ] | rationale | Explanation/reasoning text |

### 14.2 Audit Event Schema
| # | Field | Type | Required | Status |
|---|-------|------|----------|--------|
| [ ] | timestamp | date-time | Yes |
| [ ] | actor | string | Yes |
| [ ] | eventType | string | Yes |
| [ ] | details | object | No |

### 14.3 Audit Replay Session
| # | Field | Type | Description | Status |
|---|-------|------|-------------|--------|
| [ ] | sessionId | string | Unique session identifier |
| [ ] | events | array | Ordered list of AuditEvent |

---

## 15. Motivation Scoring

### 15.1 Motivation Score Components
| # | Factor | Description | Scale | Status |
|---|--------|-------------|-------|--------|
| [ ] | **Interaction Quality** | Clarity, responsiveness | 0-1 |
| [ ] | **Control Alignment** | User autonomy/control | 0-1 |
| [ ] | **Task Impact** | Meaningfulness of progress | 0-1 |

### 15.2 Motivation Audit Trail
| # | Field | Type | Description | Status |
|---|-------|------|-------------|--------|
| [ ] | timestamp | date-time | When score was recorded |
| [ ] | score | number | Motivation score at that time |
| [ ] | details | string | Why score changed |

---

## 16. Prompt Contracts

### 16.1 Base Prompt Contract Schema
| # | Field | Type | Required | Description | Status |
|---|-------|------|----------|-------------|--------|
| [ ] | agentName | string | Yes | Name of the agent |
| [ ] | promptTemplate | string | Yes | Prompt template with placeholders |
| [ ] | contextFields | array | Yes | Required context fields |
| [ ] | outputFormat | string | Yes | Expected output format |
| [ ] | retryLogic | object | Yes | Retry rules |

### 16.2 Agent-Specific Prompt Contracts
| # | Agent | Context Fields | Output Format | Status |
|---|-------|----------------|---------------|--------|
| [ ] | OnboardingAgent | userProfile, initialGoals, systemSettings | Conversational text/Markdown |
| [ ] | RulesEngineAgent | policySet, caseDetails | Decision/validation result JSON |
| [ ] | PlanAgent | goal, constraints, context | Structured plan (steps/milestones) |
| [ ] | WorkflowAgent | workflowState, pendingTasks, previousResults | Next step command/directive |
| [ ] | EvidenceAgent | claim/question, sources | Evidence list/citations |
| [ ] | DashboardAgent | progressMetrics, completedTasks, outstanding | Summary metrics JSON |
| [ ] | NextBestActionAgent | currentState, recentInteractions, userProfile | Recommended actions with rationale |

### 16.3 Retry Logic
| # | Field | Description | Status |
|---|-------|-------------|--------|
| [ ] | maxRetries | Maximum retry attempts |
| [ ] | retryConditions | When to trigger retry (format error, low confidence) |
| [ ] | retryStrategy | immediate, exponential backoff, modified prompt |

---

## Resources

### 17.1 Resource Types
| # | Resource | Actions | Status |
|---|----------|---------|--------|
| [ ] | Tenant | Create, Read, Update, Delete, Export |
| [ ] | OnboardingProfile | Create, Read, Update, Delete, Approve, Export |
| [ ] | Baseline | Create, Read, Update, Approve, Override, Export |
| [ ] | Scope | Create, Read, Update, Approve, Override, Export |
| [ ] | Plan | Create, Read, Update, Approve, Recompute, Export |
| [ ] | Task | Create, Read, Update, Assign, Reassign, Complete, Approve, Escalate, Export |
| [ ] | Evidence | Create, Read, Update, Submit, Validate, Approve, Reject, Export |
| [ ] | Exception | Create, Read, Update, Approve, Reject, Expire, Export |
| [ ] | Integration | Create, Read, Update, Test, Disable, Export |
| [ ] | Dashboard | Read, Configure, Export |
| [ ] | AuditReplay | Read, Export |

---

## Summary Statistics

| Category | Total Items |
|----------|-------------|
| Onboarding Steps | 16 |
| AI Agents | 7 |
| Workflow States | 8 |
| State Transitions | 11 |
| Field Registry | 20+ |
| Conditional Rules | 15 |
| Agent Contracts | 5 |
| Engagement Features | 20+ |
| Roles | 11 |
| Permission Types | 30+ |
| Evidence Metrics | 4 |
| Workflow Triggers | 9 |
| Feature Flags | 7 |
| Prompt Contracts | 7 |
| Resource Types | 11 |
| **TOTAL CHECKLIST ITEMS** | **250+** |

---

*Generated from `fullplan` specification document*
*Last Updated: 2026-01-13*
