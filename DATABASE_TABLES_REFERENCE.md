# GRC System - Complete Database Tables Reference

**Project**: GRC MVC (ASP.NET Core 8.0 + PostgreSQL 15)
**Generated**: 2026-01-10
**Total Tables**: 100+ tables across 12 functional areas

---

## Table of Contents

1. [Core Multi-Tenant & Authentication](#1-core-multi-tenant--authentication)
2. [Compliance & Risk Management](#2-compliance--risk-management)
3. [Assessment & Evidence](#3-assessment--evidence)
4. [Workflow & Tasks](#4-workflow--tasks)
5. [Integration Layer](#5-integration-layer)
6. [Email Operations](#6-email-operations)
7. [AI Agents & Automation](#7-ai-agents--automation)
8. [Subscription & Billing](#8-subscription--billing)
9. [Audit & Monitoring](#9-audit--monitoring)
10. [Policy Engine](#10-policy-engine)
11. [Incident & Security](#11-incident--security)
12. [Support System Tables](#12-support-system-tables)

---

## 1. Core Multi-Tenant & Authentication

### Tenants & Organizations (7 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Tenants** | Multi-tenant isolation | `TenantId`, `Name`, `SubscriptionPlanId`, `AdminEmail` | ✅ PK, FK to SubscriptionPlans |
| **TenantUsers** | User-to-tenant mapping | `TenantId`, `UserId`, `Roles` | ✅ Composite (TenantId, UserId) |
| **TenantPackages** | Tenant feature packages | `TenantId`, `PackageType`, `Features` | ✅ TenantId |
| **TenantBaselines** | Tenant compliance baselines | `TenantId`, `BaselineId` | ✅ TenantId |
| **TenantTemplates** | Tenant provisioning templates | `TemplateId`, `Name`, `ConfigJson` | ✅ PK |
| **TenantWorkflowConfig** | Tenant-specific workflow settings | `TenantId`, `WorkflowType`, `ConfigJson` | ✅ TenantId |
| **OrganizationProfile** | Organization details | `TenantId`, `LegalName`, `Industry`, `Size` | ✅ TenantId |

### Users & Authentication (6 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **ApplicationUser** | User accounts (ASP.NET Identity) | `Id`, `Email`, `TenantId` | ✅ PK, Email |
| **UserProfiles** | Extended user data | `UserId`, `FirstName`, `LastName`, `Department` | ✅ UserId |
| **UserWorkspaces** | User workspace assignments | `UserId`, `WorkspaceId`, `Role` | ✅ Composite |
| **UserConsents** | GDPR/privacy consents | `UserId`, `ConsentType`, `ConsentedAt` | ✅ UserId |
| **UserNotificationPreferences** | Notification settings | `UserId`, `Channel`, `IsEnabled` | ✅ UserId |
| **RoleProfiles** | Role definitions | `RoleId`, `Name`, `Permissions` | ✅ PK |

---

## 2. Compliance & Risk Management

### Frameworks & Controls (10 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Frameworks** | Regulatory frameworks (ISO, NIST, GDPR) | `FrameworkId`, `Code`, `Name`, `Country` | ✅ PK, Code |
| **FrameworkVersions** | Framework version tracking | `VersionId`, `FrameworkId`, `Version` | ✅ FrameworkId |
| **Controls** | Compliance controls | `ControlId`, `TenantId`, `FrameworkId`, `Category`, `Status` | ✅ **(TenantId, Category)** |
| **FrameworkControls** | Framework-to-control mapping | `FrameworkId`, `ControlId` | ✅ Composite |
| **ControlOwnership** | Control ownership assignments | `ControlId`, `OwnerId`, `AlternateOwnerId` | ✅ ControlId |
| **ControlEvidence** | Evidence requirements per control | `ControlId`, `EvidenceType`, `FrequencyDays` | ✅ ControlId |
| **ControlTests** | Control testing results | `TestId`, `ControlId`, `TenantId`, `TestedDate`, `Score` | ✅ **(TenantId, TestedDate)** |
| **ControlTestResults** | Historical test results | `TestResultId`, `ControlId`, `TestDate`, `Result` | ✅ ControlId |
| **Baselines** | Compliance baselines | `BaselineId`, `FrameworkId`, `Sector` | ✅ FrameworkId |
| **BaselineControls** | Baseline-to-control mapping | `BaselineId`, `ControlId`, `Priority` | ✅ Composite |

### Risks & Risk Management (5 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Risks** | Risk register | `RiskId`, `TenantId`, `WorkspaceId`, `Likelihood`, `Impact`, `RiskScore`, `Status` | ✅ **(TenantId, Status)**, **(TenantId, WorkspaceId, RiskScore)** |
| **RiskCategories** | Risk categorization | `CategoryId`, `Name`, `Description` | ✅ PK |
| **RiskControlMappings** | Risk-to-control relationships | `RiskId`, `ControlId` | ✅ Composite |
| **ActionPlans** | Risk mitigation action plans | `ActionPlanId`, `RiskId`, `Status` | ✅ RiskId |
| **AutonomousRiskResilience** | AI-powered risk resilience | `TenantId`, `RiskId`, `ResilienceScore` | ✅ TenantId |

### Certifications & Compliance (3 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Certifications** | ISO/SOC2/etc certifications | `CertificationId`, `TenantId`, `Type`, `IssuedDate`, `ExpiryDate`, `Status` | ✅ **(ExpiryDate, Status)** |
| **Regulators** | Regulatory authority data | `RegulatorId`, `Name`, `Country`, `Industry` | ✅ PK |
| **ComplianceSnapshots** | Point-in-time compliance state | `SnapshotId`, `TenantId`, `FrameworkId`, `SnapshotDate`, `CompliancePercentage` | ✅ (TenantId, FrameworkId) |

---

## 3. Assessment & Evidence

### Assessments (5 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Assessments** | Compliance assessments | `AssessmentId`, `TenantId`, `Status`, `DueDate`, `CompletionPercentage` | ✅ **(TenantId, Status, DueDate)** |
| **AssessmentRequirements** | Assessment requirements | `RequirementId`, `AssessmentId`, `Status` | ✅ **(AssessmentId, Status)** |
| **RequirementNotes** | Assessment requirement notes | `NoteId`, `RequirementId`, `Note` | ✅ RequirementId |
| **ComplianceEvents** | Compliance event tracking | `EventId`, `TenantId`, `EventType`, `EventDate` | ✅ TenantId |
| **OnboardingStepScores** | Onboarding step completion tracking | `TenantId`, `StepName`, `Score` | ✅ TenantId |

### Evidence Management (3 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Evidences** | Evidence artifacts | `EvidenceId`, `TenantId`, `AssessmentRequirementId`, `UploadedDate`, `Status`, `FileSize` | ✅ **(AssessmentRequirementId, UploadedDate)**, **(TenantId, Status)** |
| **EvidenceScoringCriteria** | Evidence scoring rules | `CriteriaId`, `Name`, `WeightPercentage` | ✅ PK |
| **DocumentTemplates** | Document templates | `TemplateId`, `Name`, `TemplateContent` | ✅ PK |

---

## 4. Workflow & Tasks

### Workflows (11 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Workflows** | Workflow definitions | `WorkflowId`, `Name`, `Type` | ✅ PK |
| **WorkflowDefinitions** | Workflow definition metadata | `DefinitionId`, `Name`, `BpmnXml` | ✅ PK |
| **WorkflowInstances** | Running workflow instances | `InstanceId`, `TenantId`, `WorkflowId`, `Status`, `StartedDate` | ✅ **(TenantId, Status, StartedDate)** |
| **WorkflowTasks** | Workflow task items | `TaskId`, `WorkflowInstanceId`, `AssignedToUserId`, `Status`, `DueDate` | ✅ **(AssignedToUserId, Status, DueDate)**, **(WorkflowInstanceId, Status)** |
| **WorkflowExecution** | Workflow execution tracking | `ExecutionId`, `WorkflowInstanceId`, `Status` | ✅ WorkflowInstanceId |
| **WorkflowAuditEntry** | Workflow audit trail | `EntryId`, `WorkflowInstanceId`, `Action`, `Timestamp` | ✅ WorkflowInstanceId |
| **WorkflowEscalations** | Workflow escalation rules | `EscalationId`, `WorkflowId`, `TriggerCondition` | ✅ WorkflowId |
| **ApprovalChains** | Approval chain definitions | `ChainId`, `Name`, `Approvers` | ✅ PK |
| **ApprovalInstances** | Approval instance execution | `InstanceId`, `ChainId`, `Status` | ✅ ChainId |
| **ApprovalRecords** | Individual approval records | `RecordId`, `InstanceId`, `ApproverId`, `Decision` | ✅ InstanceId |
| **EscalationRules** | Escalation automation rules | `RuleId`, `EntityType`, `Condition`, `Action` | ✅ EntityType |

### Task Management (3 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **TaskComments** | Task comments/notes | `CommentId`, `TaskId`, `UserId`, `Comment` | ✅ TaskId |
| **TaskDelegations** | Task delegation tracking | `DelegationId`, `TaskId`, `FromUserId`, `ToUserId` | ✅ TaskId |
| **DelegationRules** | Delegation automation rules | `RuleId`, `FromRoleId`, `ToRoleId`, `Condition` | ✅ PK |

---

## 5. Integration Layer

### Integration Connectors (11 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **IntegrationConnector** | External system connections | `ConnectorId`, `TenantId`, `ConnectorType`, `TargetSystem`, `ConnectionStatus` | ✅ TenantId |
| **SyncJob** | Scheduled sync jobs | `JobId`, `ConnectorId`, `ObjectType`, `Frequency`, `LastRunAt`, `NextRunAt` | ✅ ConnectorId |
| **SyncExecutionLog** | Sync execution history | `ExecutionId`, `SyncJobId`, `Status`, `RecordsProcessed`, `StartedAt` | ✅ **(SyncJobId, StartedAt)** |
| **IntegrationHealthMetric** | Integration health monitoring | `MetricId`, `ConnectorId`, `MetricType`, `Value`, `RecordedAt` | ✅ ConnectorId |
| **HRISIntegration** | HR system integration | `IntegrationId`, `TenantId`, `SourceSystem`, `LastSyncDate` | ✅ TenantId |
| **HRISEmployee** | Synced employee data | `EmployeeId`, `TenantId`, `IntegrationId`, `HRISEmployeeId` | ✅ (TenantId, IntegrationId) |
| **ERPIntegration** | ERP system integration | `IntegrationId`, `TenantId`, `ERPSystem`, `SyncStatus` | ✅ TenantId |
| **SystemOfRecordDefinition** | System of record configuration | `Id`, `ObjectType`, `SystemCode`, `IsAuthoritative` | ✅ ObjectType |
| **CrossReferenceMapping** | Cross-system ID mapping | `MappingId`, `TenantId`, `ObjectType`, `InternalId`, `ExternalSystemCode`, `ExternalId` | ✅ (TenantId, ObjectType, InternalId) |
| **IntegrationLayer** | Integration layer metadata | `LayerId`, `Name`, `Type` | ✅ PK |
| **DeadLetterEntry** | Failed integration entries | `EntryId`, `EntryType`, `ErrorMessage`, `Status`, `FailedAt` | ✅ Status |

### Event-Driven Architecture (4 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **DomainEvent** | Domain events for pub/sub | `EventId`, `TenantId`, `EventType`, `ObjectType`, `ObjectId`, `Status`, `OccurredAt` | ✅ (TenantId, EventType, Status) |
| **EventSubscription** | Event subscriptions | `SubscriptionId`, `EventTypePattern`, `SubscriberSystem`, `DeliveryMethod` | ✅ EventTypePattern |
| **EventDeliveryLog** | Event delivery tracking | `DeliveryId`, `EventId`, `SubscriptionId`, `Status`, `AttemptedAt` | ✅ EventId |
| **EventSchemaRegistry** | Event schema definitions | `SchemaId`, `EventType`, `SchemaVersion`, `JsonSchema` | ✅ (EventType, SchemaVersion) |

### Webhooks (2 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **WebhookSubscriptions** | Webhook configurations | `SubscriptionId`, `TenantId`, `EventType`, `WebhookUrl` | ✅ TenantId |
| **WebhookDeliveryLogs** | Webhook delivery history | `DeliveryId`, `SubscriptionId`, `Status`, `HttpStatusCode` | ✅ SubscriptionId |

---

## 6. Email Operations

### Email System (7 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **EmailMailbox** | Email mailboxes | `MailboxId`, `TenantId`, `EmailAddress`, `Provider` | ✅ TenantId |
| **EmailMessages** | Email messages | `MessageId`, `MailboxId`, `ThreadId`, `ReceivedAt`, `IsRead` | ✅ **(ThreadId, ReceivedAt)**, **(MailboxId, IsRead, ReceivedAt)** |
| **EmailThreads** | Email conversation threads | `ThreadId`, `Subject`, `ParticipantCount`, `LastMessageAt` | ✅ LastMessageAt |
| **EmailAttachments** | Email attachments | `AttachmentId`, `MessageId`, `FileName`, `FileSize` | ✅ MessageId |
| **EmailTemplates** | Email templates | `TemplateId`, `Name`, `Subject`, `BodyHtml` | ✅ PK |
| **EmailTasks** | Tasks from emails | `TaskId`, `MessageId`, `AssignedToUserId`, `DueDate` | ✅ MessageId |
| **EmailAutoReplyRules** | Auto-reply automation | `RuleId`, `MailboxId`, `Condition`, `ResponseTemplate` | ✅ MailboxId |

---

## 7. AI Agents & Automation

### AI Agent System (7 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **AgentOperatingModel** | Agent configuration | `AgentId`, `Name`, `Capabilities`, `Status` | ✅ PK |
| **AiProviderConfiguration** | AI provider settings | `ProviderId`, `Provider`, `ApiKey`, `Model` | ✅ PK |
| **LlmConfiguration** | LLM model configuration | `ConfigId`, `ModelName`, `Provider`, `Parameters` | ✅ PK |
| **RuleExecutionLogs** | Rule execution tracking | `LogId`, `TenantId`, `RulesetId`, `ExecutedAt`, `Result` | ✅ **(RulesetId, ExecutedAt)**, **(TenantId, ExecutedAt)** |
| **Rulesets** | Rule collections | `RulesetId`, `Name`, `Rules` | ✅ PK |
| **Rules** | Individual rules | `RuleId`, `RulesetId`, `Condition`, `Action` | ✅ RulesetId |
| **ValidationRules** | Validation rule definitions | `RuleId`, `EntityType`, `FieldName`, `ValidationLogic` | ✅ EntityType |

---

## 8. Subscription & Billing

### Subscription Management (6 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **SubscriptionPlans** | Subscription tier definitions | `PlanId`, `Name`, `MonthlyPrice`, `MaxUsers`, `MaxAssessments` | ✅ PK |
| **Subscriptions** | Active subscriptions | `SubscriptionId`, `TenantId`, `PlanId`, `Status`, `StartDate`, `EndDate` | ✅ (TenantId, Status) |
| **Invoices** | Billing invoices | `InvoiceId`, `SubscriptionId`, `Amount`, `DueDate`, `Status` | ✅ SubscriptionId |
| **Payments** | Payment records | `PaymentId`, `InvoiceId`, `Amount`, `PaymentDate` | ✅ InvoiceId |
| **TrialRequests** | Trial account requests | `RequestId`, `Email`, `CompanyName`, `Status` | ✅ Status |
| **OnboardingWizard** | Onboarding progress tracking | `WizardId`, `TenantId`, `CurrentStep`, `IsCompleted` | ✅ TenantId |

---

## 9. Audit & Monitoring

### Audit Trail (4 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **AuditEvents** | Comprehensive audit log | `EventId`, `TenantId`, `AffectedEntityType`, `Actor`, `Action`, `CreatedDate` | ✅ **(TenantId, CreatedDate)**, **(EntityType, EntityId, CreatedDate)** |
| **AuditLog** | Legacy audit log | `LogId`, `TenantId`, `EntityType`, `EntityId`, `Action`, `ChangedDate` | ✅ (TenantId, ChangedDate) |
| **ComplianceSnapshots** | Point-in-time compliance | `SnapshotId`, `TenantId`, `SnapshotDate`, `CompliancePercentage` | ✅ TenantId |
| **SerialCodeRegistry** | Serial number tracking | `RegistryId`, `EntityType`, `Code`, `GeneratedAt` | ✅ EntityType |

### Reports (1 table)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Reports** | Generated reports | `ReportId`, `TenantId`, `ReportType`, `GeneratedDate` | ✅ TenantId |

---

## 10. Policy Engine

### Policy Management (5 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Policies** | Policy documents | `PolicyId`, `TenantId`, `Title`, `Status`, `EffectiveDate`, `ExpiryDate` | ✅ **(TenantId, Status, EffectiveDate)** |
| **PolicyDecisions** | Policy decision records | `DecisionId`, `PolicyId`, `RequestContext`, `Decision` | ✅ PolicyId |
| **PolicyViolations** | Policy violation tracking | `ViolationId`, `PolicyId`, `TenantId`, `ViolationDate` | ✅ (PolicyId, TenantId) |
| **SlaRules** | SLA automation rules | `RuleId`, `Name`, `TargetMetric`, `Threshold` | ✅ PK |
| **TriggerRules** | Event trigger rules | `RuleId`, `EventType`, `Condition`, `Action` | ✅ EventType |

---

## 11. Incident & Security

### Incident Management (2 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Incidents** | Security incidents | `IncidentId`, `TenantId`, `Status`, `Severity`, `DetectedAt`, `Phase` | ✅ **(TenantId, Status, Severity)**, **(DetectedAt DESC)** |
| **AuditFindings** | Audit findings | `FindingId`, `AuditId`, `Severity`, `Status` | ✅ AuditId |

### Security & Assets (3 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Assets** | IT assets inventory | `AssetId`, `TenantId`, `AssetType`, `Owner` | ✅ TenantId |
| **Vendors** | Vendor/supplier management | `VendorId`, `TenantId`, `Name`, `RiskRating` | ✅ TenantId |
| **Resilience** | Resilience planning | `ResilienceId`, `TenantId`, `PlanType`, `Status` | ✅ TenantId |

---

## 12. Support System Tables

### Catalog & Configuration (4 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **CatalogItems** | Catalog item definitions | `ItemId`, `Category`, `Name`, `IsActive` | ✅ Category |
| **ApplicabilityMatrix** | Framework applicability rules | `MatrixId`, `FrameworkId`, `Sector`, `Applicability` | ✅ FrameworkId |
| **MAPFramework** | MAP framework metadata | `FrameworkId`, `Name`, `Version` | ✅ PK |
| **CanonicalControlLibrary** | Master control library | `ControlId`, `ControlCode`, `Name` | ✅ ControlCode |

### Workspaces & Teams (3 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Workspaces** | Workspace definitions | `WorkspaceId`, `TenantId`, `Name` | ✅ TenantId |
| **Teams** | Team definitions | `TeamId`, `TenantId`, `Name` | ✅ TenantId |
| **TeamMembers** | Team membership | `TeamId`, `UserId`, `Role` | ✅ Composite |

### Plans & Phases (2 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **Plans** | Project plans | `PlanId`, `TenantId`, `Name`, `Status` | ✅ TenantId |
| **PlanPhases** | Plan phase breakdown | `PhaseId`, `PlanId`, `Name`, `StartDate`, `EndDate` | ✅ PlanId |

### Miscellaneous (5 tables)

| Table | Purpose | Key Fields | Indexes |
|-------|---------|------------|---------|
| **PlatformAdmins** | Platform administrator accounts | `AdminId`, `UserId`, `Permissions` | ✅ UserId |
| **RoleLandingConfig** | Role-based landing page config | `ConfigId`, `RoleId`, `LandingPagePath` | ✅ RoleId |
| **ShahinAIBranding** | AI branding configuration | `BrandingId`, `LogoUrl`, `ColorScheme` | ✅ PK |
| **SerialCounters** | Auto-increment counters | `CounterId`, `EntityType`, `CurrentValue` | ✅ EntityType |
| **BaselineOverlayModel** | Baseline overlay configuration | `OverlayId`, `BaselineId`, `OverlayConfig` | ✅ BaselineId |

---

## Integration Tables Summary

For your question about **integration tables**, here are the key tables needed for **each integration stage**:

### Stage 1: Connection Setup
- **IntegrationConnector** - Define external system connection
- **AiProviderConfiguration** - Configure AI provider (Claude API)
- **SystemOfRecordDefinition** - Define authoritative systems

### Stage 2: Data Mapping
- **CrossReferenceMapping** - Map IDs across systems
- **SyncJob** - Configure sync jobs with field mappings

### Stage 3: Synchronization
- **SyncExecutionLog** - Track sync execution (steps: Start → Process → Complete)
- **DomainEvent** - Publish events for event-driven sync

### Stage 4: Monitoring & Health
- **IntegrationHealthMetric** - Monitor integration performance
- **DeadLetterEntry** - Track failed sync attempts

### Stage 5: Event Delivery
- **EventSubscription** - Subscribe to events
- **EventDeliveryLog** - Track event delivery (steps: Pending → Delivered/Failed → Retry)
- **WebhookDeliveryLogs** - Track webhook delivery

**Integration Flow Steps** (5 stages per integration):
1. **Setup** → Create connector in `IntegrationConnector`
2. **Configure** → Define mappings in `CrossReferenceMapping`, create `SyncJob`
3. **Execute** → Run sync, log in `SyncExecutionLog` (Running → Completed/Failed)
4. **Publish** → Emit `DomainEvent` (Pending → Published → Processed)
5. **Monitor** → Track health in `IntegrationHealthMetric`, failed items in `DeadLetterEntry`

---

## Missing Tables (Not Yet Implemented)

Based on the audit, these tables are **referenced but not found**:

❌ **Audits** - Main audit entity (only `AuditEvent` and `AuditFinding` exist)
❌ **FrameworkMappings** - Framework-to-framework control mapping
❌ **KRIDefinitions** - Key Risk Indicator definitions
❌ **ExceptionRequests** - Compliance exception tracking
❌ **ThirdPartyRiskAssessments** - Vendor risk assessments

---

## Database Performance

### Indexes Added (Recent Migration 20260110000001)
- 20 composite indexes for multi-tenant queries
- Covers: Controls, Risks, Evidences, WorkflowTasks, Incidents, EmailMessages, Assessments, etc.

### Data Integrity Constraints (Recent Migration 20260110000002)
- 15 check constraints for business rule enforcement
- Validates: Risk scores (0-25), Likelihood/Impact (1-5), Percentages (0-100), Dates, Email format

---

## Technology Notes

- **ORM**: Entity Framework Core 8.0.8
- **Database**: PostgreSQL 15 (Npgsql provider)
- **Multi-Tenancy**: Row-level isolation via `TenantId` column + query filters
- **Soft Delete**: `IsDeleted` column on all entities (via `BaseEntity`)
- **Audit Fields**: `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy` on all entities
- **Total Migrations**: 60+ EF Core migrations

---

## Quick Reference

**Total Table Count**: ~100 tables
**Core Entities**: 15 (Tenant, User, Control, Risk, Assessment, Evidence, Workflow, etc.)
**Integration Tables**: 17 (Connectors, SyncJobs, Events, Webhooks, etc.)
**Support Tables**: 30+ (Catalogs, Templates, Configurations, etc.)

**Production Status**: ✅ Ready (with 20 indexes + 15 constraints from recent fixes)

---

**Document Owner**: GRC MVC Development Team
**Last Updated**: 2026-01-10
**Related Documents**:
- [CLAUDE.md](d:\Shahin-Jan-2026\CLAUDE.md) - Project overview
- [FIXES_COMPLETED_2026-01-10.md](d:\Shahin-Jan-2026\FIXES_COMPLETED_2026-01-10.md) - Recent database improvements
