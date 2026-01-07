# GRC System - Complete Feature Inventory
## Comprehensive List of All Features, Modules, and Capabilities

**Generated:** 2025-01-07  
**Based on:** Direct code analysis  

---

## Executive Summary

This document provides a **complete inventory** of all features, modules, integrations, and capabilities found in the GRC System codebase.

**Total Features Identified:** 100+  
**Major Modules:** 18  
**Specialized Platforms:** 2 (Shahin AI, CCM)  
**Integration Points:** 15+  

---

## 1. Core GRC Domain Modules (18 Modules)

### 1.1 Risk Management
**Location:** `Controllers/RiskController.cs`, `Services/Implementations/RiskService.cs`  
**Views:** `Views/Risk/` (7 files)  
**Features:**
- Risk registration and assessment
- Risk scoring and categorization
- Risk mitigation tracking
- Risk acceptance workflow
- Risk indicators (KRI) monitoring
- Risk heatmap visualization
- Risk-resilience mapping

**Entities:**
- `Risk` - Main risk entity
- `RiskIndicator` - Key Risk Indicators
- `RiskIndicatorMeasurement` - KRI measurements
- `RiskIndicatorAlert` - KRI alerts
- `Resilience` - Risk resilience records
- `RiskResilience` - Risk-resilience relationships

---

### 1.2 Control Management
**Location:** `Controllers/ControlController.cs`, `Controllers/ControlsController.cs`  
**Views:** `Views/Control/`, `Views/Controls/` (18 files total)  
**Features:**
- Control library management
- Control testing and assessment
- Control effectiveness evaluation
- Control implementation tracking
- Control exception management
- Framework-control mapping
- Canonical control library

**Entities:**
- `Control` - Main control entity
- `ControlDomain` - Control domains
- `ControlObjective` - Control objectives
- `CanonicalControl` - Canonical controls
- `FrameworkControl` - Framework-control mappings
- `ControlException` - Control exceptions
- `ControlChangeHistory` - Change tracking

---

### 1.3 Evidence Management
**Location:** `Controllers/EvidenceController.cs`  
**Views:** `Views/Evidence/` (11 files)  
**Features:**
- Evidence upload and storage
- Evidence classification (public/internal/confidential/restricted)
- Evidence lifecycle management
- Evidence expiration tracking
- Evidence-by-audit view
- Evidence-by-type view
- Evidence-by-classification view
- Evidence submission workflow
- Evidence approval workflow
- Auto-tagged evidence (AI)

**Entities:**
- `Evidence` - Main evidence entity
- `EvidencePack` - Evidence pack grouping
- `ControlEvidencePack` - Control-evidence relationships
- `EvidencePackFamily` - Evidence pack families
- `StandardEvidenceItem` - Standard evidence templates
- `AutoTaggedEvidence` - AI-tagged evidence
- `CapturedEvidence` - Captured evidence records
- `EvidenceSourceIntegration` - Evidence source integrations

---

### 1.4 Assessment Management
**Location:** `Controllers/AssessmentController.cs`  
**Views:** `Views/Assessment/` (8 files)  
**Features:**
- Compliance assessments
- Control assessments
- Assessment by control
- Assessment statistics
- Upcoming assessments
- Assessment requirements tracking
- Assessment scope management

**Entities:**
- `Assessment` - Main assessment entity
- `AssessmentRequirement` - Assessment requirements
- `AssessmentScope` - Assessment scope definitions

---

### 1.5 Audit Management
**Location:** `Controllers/AuditController.cs`  
**Views:** `Views/Audit/` (9 files)  
**Features:**
- Audit planning
- Audit execution
- Audit findings management
- Audit event tracking
- Audit package management
- Audit trail logging

**Entities:**
- `Audit` - Main audit entity
- `AuditFinding` - Audit findings
- `AuditEvent` - System audit events

---

### 1.6 Policy Management
**Location:** `Controllers/PolicyController.cs`  
**Views:** `Views/Policy/` (10 files)  
**Features:**
- Policy document management
- Policy approval workflow
- Policy publishing
- Policy violation tracking
- Policy decision audit trail
- Policy enforcement (YAML-based)

**Entities:**
- `Policy` - Main policy entity
- `PolicyViolation` - Policy violations
- `PolicyDecision` - Policy decision audit trail

---

### 1.7 Workflow Engine
**Location:** `Controllers/WorkflowController.cs`, `Controllers/WorkflowsController.cs`  
**Views:** `Views/Workflow/` (13 files), `Views/WorkflowUI/` (8 files)  
**Features:**
- Workflow definition management
- Workflow instance execution
- Workflow task assignment
- Workflow approvals
- Workflow escalations
- Workflow inbox
- Workflow statistics
- Workflow process flow visualization
- Workflow by status/category
- Overdue workflow tracking
- Workflow audit entries
- Task delegation
- Task comments

**Entities:**
- `Workflow` - Legacy workflow entity
- `WorkflowDefinition` - Workflow templates
- `WorkflowInstance` - Active workflow instances
- `WorkflowTask` - Individual tasks
- `WorkflowExecution` - Execution history
- `WorkflowAuditEntry` - Audit trail
- `WorkflowEscalation` - Escalation records
- `WorkflowNotification` - Notifications
- `WorkflowApproval` - Approval records
- `WorkflowTransition` - State transitions
- `ApprovalChain` - Approval chain definitions
- `ApprovalInstance` - Approval instances
- `ApprovalRecord` - Approval decisions
- `TaskComment` - Task comments
- `TaskDelegation` - Task delegations

---

### 1.8 Action Plans
**Location:** `Controllers/ActionPlansController.cs`  
**Views:** `Views/ActionPlans/` (1 file)  
**Features:**
- Action plan creation
- Action plan tracking
- Action plan assignment
- Action plan closure

**Entities:**
- `ActionPlan` - Action plan items

---

### 1.9 Compliance Calendar
**Location:** `Controllers/ComplianceCalendarController.cs`  
**Views:** `Views/ComplianceCalendar/` (1 file)  
**Features:**
- Compliance event scheduling
- Compliance deadline tracking
- Compliance event reminders

**Entities:**
- `ComplianceEvent` - Compliance calendar events

---

### 1.10 Regulatory Frameworks
**Location:** `Controllers/FrameworksController.cs`  
**Views:** `Views/Frameworks/` (1 file)  
**Features:**
- Framework library management
- Framework versioning
- Framework-control mapping
- Framework comparison
- Framework catalog

**Entities:**
- `Framework` - Regulatory framework
- `FrameworkControl` - Framework-control mappings
- `FrameworkCatalog` - Framework catalog entries

---

### 1.11 Regulators
**Location:** `Controllers/RegulatorsController.cs`  
**Views:** `Views/Regulators/` (1 file)  
**Features:**
- Regulator management
- Regulator catalog
- Regulatory requirement tracking

**Entities:**
- `Regulator` - Regulatory body
- `RegulatorCatalog` - Regulator catalog entries
- `RegulatoryRequirement` - Regulatory requirements
- `RequirementMapping` - Requirement mappings

---

### 1.12 Vendors
**Location:** `Controllers/VendorsController.cs`  
**Views:** `Views/Vendors/` (1 file)  
**Features:**
- Vendor management
- Vendor assessment
- Vendor risk tracking
- Third-party concentration monitoring

**Entities:**
- `Vendor` - Vendor entity
- `ThirdPartyConcentration` - Third-party concentration tracking

---

### 1.13 Reports & Analytics
**Location:** `Controllers/Api/ReportController.cs`, `Controllers/Api/EnhancedReportController.cs`  
**Views:** `Views/Reports/` (1 file), `Views/Analytics/` (1 file)  
**Features:**
- Report generation
- Compliance trend analysis
- Risk heatmap
- Framework comparison
- Task metrics by role
- Evidence metrics
- User activity tracking
- Dashboard snapshots
- Enhanced reporting with QuestPDF

**Entities:**
- `Report` - Report definitions

**Analytics Services:**
- ClickHouse OLAP integration (with stub fallback)
- Dashboard projection service
- Analytics event tracking

---

### 1.14 Plans & Phases
**Location:** `Controllers/PlansController.cs`  
**Views:** `Views/Plans/` (7 files)  
**Features:**
- Implementation plan management
- Plan phase tracking
- Plan creation and editing
- Phase details management

**Entities:**
- `Plan` - Implementation plans
- `PlanPhase` - Plan phases

---

### 1.15 Subscriptions
**Location:** `Controllers/SubscriptionController.cs`, `Controllers/SubscribeController.cs`  
**Views:** `Views/Subscription/` (4 files), `Views/Subscribe/` (5 files)  
**Features:**
- Subscription plan management
- Subscription checkout
- Payment processing
- Invoice generation
- Subscription activation/suspension/cancellation
- Trial management
- Subscription renewal

**Entities:**
- `Subscription` - Tenant subscriptions
- `SubscriptionPlan` - Subscription plan definitions
- `Payment` - Payment records
- `Invoice` - Invoice records

---

### 1.16 Notifications
**Location:** `Controllers/NotificationsController.cs`  
**Views:** `Views/Notifications/` (1 file)  
**Features:**
- User notifications
- Notification preferences
- Email notifications
- Slack notifications
- Microsoft Teams notifications
- SMS notifications (Twilio)
- Notification delivery job (Hangfire)

**Entities:**
- `UserNotificationPreference` - User preferences

**Services:**
- `NotificationService` - Core notification service
- `EmailServiceAdapter` - Email delivery
- `SlackNotificationService` - Slack integration
- `TeamsNotificationService` - Teams integration
- `TwilioSmsService` - SMS delivery

---

### 1.17 Integrations
**Location:** `Controllers/IntegrationsController.cs`  
**Views:** `Views/Integrations/` (1 file)  
**Features:**
- Integration connector management
- Webhook subscriptions
- Event delivery logging
- ERP system integration
- Integration health monitoring
- Sync job management
- Dead letter queue management
- Event schema registry

**Entities:**
- `WebhookSubscription` - Webhook subscriptions
- `WebhookDeliveryLog` - Delivery audit
- `IntegrationConnector` - Integration connectors
- `SyncJob` - Sync job definitions
- `SyncExecutionLog` - Sync execution logs
- `IntegrationHealthMetric` - Health metrics
- `DeadLetterEntry` - Failed message tracking
- `EventSchemaRegistry` - Event schemas
- `ERPSystemConfig` - ERP configurations
- `ERPExtractConfig` - ERP extract configs
- `ERPExtractExecution` - Extract executions
- `SystemOfRecordDefinition` - System of record
- `CrossReferenceMapping` - Cross-reference mappings

---

### 1.18 Help & Support
**Location:** `Controllers/HelpController.cs`, `Controllers/SupportController.cs`  
**Views:** `Views/Help/` (5 files)  
**Features:**
- Help documentation
- FAQ system
- Glossary
- Getting started guide
- Contact form
- Support conversations
- Support messages

**Entities:**
- `SupportConversation` - Support conversations
- `SupportMessage` - Support messages

---

## 2. Shahin AI Platform (6 Modules)

**Location:** `Controllers/ShahinAIController.cs`, `Services/Implementations/ShahinModuleServices.cs`  
**Views:** `Views/ShahinAI/` (7 files)  

### 2.1 MAP Module (Multi-Agent Platform - Control Library)
**Route:** `/shahin-ai/map`  
**Features:**
- Canonical control library
- Control mapping
- Framework mapping
- Plain language controls
- Universal evidence packs

**Entities:**
- `MAPFrameworkConfig` - MAP framework configuration
- `CanonicalControl` - Canonical controls
- `PlainLanguageControl` - Plain language controls
- `UniversalEvidencePack` - Universal evidence packs
- `UniversalEvidencePackItem` - Pack items

---

### 2.2 APPLY Module (Scope & Applicability)
**Route:** `/shahin-ai/apply`  
**Features:**
- Applicability matrix
- Applicability rules
- Applicability entries
- Baseline overlay model
- Overlay control mappings

**Entities:**
- `ApplicabilityMatrix` - Applicability matrix
- `ApplicabilityRule` - Applicability rules
- `ApplicabilityEntry` - Applicability entries
- `BaselineOverlayModel` - Baseline overlays
- `OverlayCatalog` - Overlay catalogs
- `OverlayControlMapping` - Overlay mappings
- `OverlayParameterOverride` - Parameter overrides
- `ApplicabilityRuleCatalog` - Rule catalog

---

### 2.3 PROVE Module (Evidence & Testing)
**Route:** `/shahin-ai/prove`  
**Features:**
- Auto-tagged evidence (AI)
- Evidence capture
- Evidence source integration
- Test procedures
- Control test procedures
- Evidence scoring

**Entities:**
- `AutoTaggedEvidence` - AI-tagged evidence
- `CapturedEvidence` - Captured evidence
- `EvidenceSourceIntegration` - Source integrations
- `TestProcedure` - Test procedures
- `ControlTestProcedure` - Control test mappings
- `EvidenceScore` - Evidence scoring

---

### 2.4 WATCH Module (Monitoring & Alerts)
**Route:** `/shahin-ai/watch`  
**Features:**
- Risk indicator monitoring
- KRI dashboard
- Risk indicator measurements
- Risk indicator alerts
- Important business services tracking
- Governance cadence
- Cadence execution tracking

**Entities:**
- `RiskIndicator` - Risk indicators
- `RiskIndicatorMeasurement` - Measurements
- `RiskIndicatorAlert` - Alerts
- `ImportantBusinessService` - Business services
- `GovernanceCadence` - Governance cadences
- `CadenceExecution` - Cadence executions

---

### 2.5 FIX Module (Remediation)
**Route:** `/shahin-ai/fix`  
**Features:**
- Exception management
- Remediation tracking
- Control exception handling

**Entities:**
- `ControlException` - Control exceptions

---

### 2.6 VAULT Module (Secure Storage)
**Route:** `/shahin-ai/vault`  
**Features:**
- Secure document vault
- Cryptographic asset management
- Compliance guardrails
- Strategic roadmap milestones

**Entities:**
- `CryptographicAsset` - Cryptographic assets
- `ComplianceGuardrail` - Compliance guardrails
- `StrategicRoadmapMilestone` - Roadmap milestones

**Additional Shahin AI Entities:**
- `ShahinAIBrandConfig` - Branding configuration
- `ShahinAIModule` - Module definitions
- `UITextEntry` - UI text entries
- `SiteMapEntry` - Site map entries

---

## 3. CCM (Continuous Control Monitoring)

**Location:** `Controllers/CCMController.cs`  
**Views:** `Views/CCM/` (2 files)  
**Features:**
- Automated control testing
- Control test execution
- Test result tracking
- Exception management
- Segregation of Duties (SoD) rules
- SoD conflict detection

**Entities:**
- `CCMControlTest` - Control tests
- `CCMTestExecution` - Test executions
- `CCMException` - Test exceptions
- `SoDRuleDefinition` - SoD rules
- `SoDConflict` - SoD conflicts

---

## 4. Onboarding System

### 4.1 Comprehensive 12-Step Wizard
**Location:** `Controllers/OnboardingWizardController.cs` (111,347 bytes - largest controller)  
**Views:** `Views/OnboardingWizard/` (15 files)  

**Steps:**
- **Step A:** Organization Identity and Tenancy
- **Step B:** Regulatory Landscape
- **Step C:** Framework Selection
- **Step D:** Scope Definition
- **Step E:** Asset Recognition
- **Step F:** Control Baseline
- **Step G:** Governance Structure
- **Step H:** Team Setup
- **Step I:** Workflow Configuration
- **Step J:** Evidence Standards
- **Step K:** Integration Setup
- **Step L:** Review & Provisioning

**Features:**
- 96+ questions across 12 steps
- Tenant provisioning
- Workspace creation
- Team setup
- RACI assignment
- Workflow configuration
- Evidence naming patterns
- Integration configuration

**Entities:**
- `OnboardingWizard` - Wizard state (25,511 bytes - largest entity)

---

### 4.2 Smart Onboarding
**Location:** `Services/Implementations/SmartOnboardingService.cs`  
**Features:**
- Intelligent onboarding flow
- Automated provisioning
- Smart defaults

---

### 4.3 Organization Setup
**Location:** `Controllers/OrgSetupController.cs`  
**Views:** `Views/OrgSetup/` (7 files)  
**Features:**
- Team creation and management
- Team member assignment
- RACI matrix setup
- User management

---

### 4.4 Owner Setup
**Location:** `Controllers/OwnerSetupController.cs`  
**Views:** `Views/Owner/` (7 files)  
**Features:**
- Owner account creation
- Tenant creation by owner
- Owner status tracking
- Owner details management

---

## 5. Multi-Tenancy & Workspace System

### 5.1 Tenant Management
**Location:** `Controllers/TenantAdminController.cs`, `Controllers/PlatformAdminController.cs`  
**Views:** `Views/TenantAdmin/`, `Views/PlatformAdmin/`  
**Features:**
- Tenant creation and management
- Tenant activation/suspension
- Tenant database provisioning
- Tenant-specific configurations
- Tenant baselines
- Tenant packages
- Tenant templates
- Tenant workflow configuration

**Entities:**
- `Tenant` - Tenant entity
- `TenantUser` - Tenant-user relationships
- `TenantBaseline` - Tenant baselines
- `TenantPackage` - Tenant packages
- `TenantTemplate` - Tenant templates
- `TenantWorkflowConfig` - Workflow configs
- `OwnerTenantCreation` - Owner-tenant creation records

---

### 5.2 Workspace System
**Location:** `Services/Implementations/WorkspaceService.cs`, `Services/Implementations/WorkspaceManagementService.cs`  
**Views:** `Views/Shared/Components/WorkspaceSwitcher/`  
**Features:**
- Workspace creation (sub-scope within tenant)
- Workspace membership
- Workspace control assignment
- Workspace approval gates
- Workspace approvers
- User workspace preferences
- User workspace tasks
- Workspace templates

**Entities:**
- `Workspace` - Workspace entity
- `WorkspaceMembership` - Workspace memberships
- `WorkspaceControl` - Workspace controls
- `WorkspaceApprovalGate` - Approval gates
- `WorkspaceApprovalGateApprover` - Gate approvers
- `UserWorkspace` - User preferences
- `UserWorkspaceTask` - User tasks
- `WorkspaceTemplate` - Workspace templates

---

## 6. RBAC (Role-Based Access Control)

**Location:** `Services/Implementations/RBAC/`  
**Features:**
- Permission management
- Feature management
- Role-permission mapping
- Role-feature mapping
- Feature-permission mapping
- Tenant role configuration
- User role assignment
- Access control service

**Entities:**
- `Permission` - Permission definitions
- `Feature` - Feature definitions
- `RolePermission` - Role-permission mappings
- `RoleFeature` - Role-feature mappings
- `FeaturePermission` - Feature-permission mappings
- `TenantRoleConfiguration` - Tenant role configs
- `UserRoleAssignment` - User role assignments
- `RoleProfile` - Role profile templates
- `RoleLandingConfig` - Role landing page configs

**Services:**
- `PermissionService` - Permission management
- `FeatureService` - Feature management
- `TenantRoleConfigurationService` - Tenant role configs
- `UserRoleAssignmentService` - User assignments
- `AccessControlService` - Access control

---

## 7. Team & RACI System

**Location:** `Models/Entities/TeamEntities.cs`  
**Features:**
- Team creation and management
- Team member assignment
- RACI matrix (Responsible, Accountable, Consulted, Informed)
- RACI assignments

**Entities:**
- `Team` - Team definitions
- `TeamMember` - Team memberships
- `RACIAssignment` - RACI assignments

---

## 8. Asset Management

**Location:** `Controllers/Api/AssetsController.cs`, `Services/Implementations/AssetService.cs`  
**Features:**
- Asset inventory
- Asset recognition
- Asset-based control derivation

**Entities:**
- `Asset` - Asset inventory

---

## 9. Rules Engine

**Location:** `Services/Implementations/Phase1RulesEngineService.cs`  
**Features:**
- Ruleset management
- Rule evaluation
- Rule execution logging
- Validation rules
- Validation results
- Trigger rules
- Trigger execution logging

**Entities:**
- `Ruleset` - Ruleset definitions
- `Rule` - Rule definitions
- `RuleExecutionLog` - Execution logs
- `ValidationRule` - Validation rules
- `ValidationResult` - Validation results
- `TriggerRule` - Trigger rules
- `TriggerExecutionLog` - Trigger logs

---

## 10. Catalog System

**Location:** `Controllers/Api/AdminCatalogController.cs`, `Services/Implementations/AdminCatalogService.cs`  
**Features:**
- Regulator catalog
- Framework catalog
- Control catalog
- Role catalog
- Title catalog
- Baseline catalog
- Package catalog
- Template catalog
- Evidence type catalog

**Entities:**
- `RegulatorCatalog` - Regulator catalog
- `FrameworkCatalog` - Framework catalog
- `ControlCatalog` - Control catalog
- `RoleCatalog` - Role catalog
- `TitleCatalog` - Title catalog
- `BaselineCatalog` - Baseline catalog
- `PackageCatalog` - Package catalog
- `TemplateCatalog` - Template catalog
- `EvidenceTypeCatalog` - Evidence type catalog

---

## 11. Control Suite Generation

**Location:** `Services/Implementations/SuiteGenerationService.cs`  
**Features:**
- Automated control suite generation
- Suite control entries
- Suite evidence requests
- Generated control suites

**Entities:**
- `GeneratedControlSuite` - Generated suites
- `SuiteControlEntry` - Suite control entries
- `SuiteEvidenceRequest` - Evidence requests

---

## 12. Expert Framework Mapping

**Location:** `Services/Implementations/ExpertFrameworkMappingService.cs`  
**Features:**
- Expert framework mapping
- Mapping quality gates
- Mapping workflow steps
- Mapping workflow templates

**Entities:**
- `MappingQualityGate` - Quality gates
- `MappingWorkflowStep` - Workflow steps
- `MappingWorkflowTemplate` - Workflow templates

---

## 13. AI Agent System

**Location:** `Models/Entities/AgentOperatingModel.cs` (22,294 bytes)  
**Features:**
- Agent definitions
- Agent capabilities
- Agent actions
- Agent approval gates
- Pending approvals
- Agent confidence scores
- Agent SoD rules
- Agent SoD violations
- Human retained responsibilities
- Role transition plans

**Entities:**
- `AgentDefinition` - Agent definitions
- `AgentCapability` - Agent capabilities
- `AgentAction` - Agent actions
- `AgentApprovalGate` - Approval gates
- `PendingApproval` - Pending approvals
- `AgentConfidenceScore` - Confidence scores
- `AgentSoDRule` - SoD rules
- `AgentSoDViolation` - SoD violations
- `HumanRetainedResponsibility` - Human responsibilities
- `RoleTransitionPlan` - Role transitions
- `AgentOperatingModel` - Operating model

---

## 14. Governance & Compliance

**Features:**
- Governance cadence templates
- Governance rhythm items
- One-page guides
- Compliance guardrails
- Strategic roadmap milestones

**Entities:**
- `GovernanceRhythmTemplate` - Cadence templates
- `GovernanceRhythmItem` - Rhythm items
- `OnePageGuide` - One-page guides
- `ComplianceGuardrail` - Guardrails
- `StrategicRoadmapMilestone` - Milestones

---

## 15. User Management

**Location:** `Controllers/PlatformAdminController.cs`, `Services/Implementations/UserManagementFacade.cs`  
**Features:**
- User profile management
- User profile assignments
- User directory service
- User invitation service
- Credential delivery
- User consent management
- Legal document tracking
- User workspace management

**Entities:**
- `UserProfile` - User profiles
- `UserProfileAssignment` - Profile assignments
- `UserConsent` - User consents
- `LegalDocument` - Legal documents
- `UserWorkspace` - User workspace preferences
- `UserWorkspaceTask` - User tasks

---

## 16. Platform Administration

**Location:** `Controllers/PlatformAdminController.cs` (31,286 bytes)  
**Views:** `Views/PlatformAdmin/` (13 files)  
**Features:**
- Platform admin dashboard
- Tenant management
- User management
- Admin management
- Feature flags
- Migration metrics
- Audit logs
- Settings management
- Tenant details
- User details

---

## 17. Background Jobs (6 Jobs)

**Location:** `BackgroundJobs/`  
**Technology:** Hangfire with PostgreSQL storage

1. **NotificationDeliveryJob** - Scheduled notification delivery (every 5 minutes)
2. **EscalationJob** - Workflow escalation processing (every 1 hour)
3. **SlaMonitorJob** - SLA monitoring and alerts (every 30 minutes)
4. **WebhookRetryJob** - Failed webhook retry logic (on demand)
5. **AnalyticsProjectionJob** - Analytics data projection (scheduled)
6. **CodeQualityMonitorJob** - Code quality monitoring (scheduled)

---

## 18. Messaging & Events

**Location:** `Messaging/`  
**Technology:** MassTransit + RabbitMQ

**Features:**
- Domain event publishing
- Event subscriptions
- Event delivery logging
- Message consumers:
  - `GrcEventConsumer` - GRC domain events
  - `NotificationConsumer` - Notification events
  - `WebhookConsumer` - Webhook events

**Entities:**
- `DomainEvent` - Domain events
- `EventSubscription` - Event subscriptions
- `EventDeliveryLog` - Delivery logs
- `EventSchemaRegistry` - Event schemas

---

## 19. Health Checks

**Location:** `HealthChecks/`  
**Features:**
- Database health checks (PostgreSQL)
- Hangfire health checks
- Entity Framework health checks
- Tenant database health checks

**Endpoints:**
- `/health` - Basic health
- `/health/ready` - Readiness check

---

## 20. Localization

**Location:** `Resources/`  
**Languages:**
- English (en) - Default
- Arabic (ar) - RTL support

**Features:**
- Resource files (`.resx`)
- Arabic menu navigation
- Bilingual UI support
- Culture switching

---

## 21. Search Functionality

**Location:** `Controllers/HomeController.cs`  
**Views:** `Views/Home/Search.cshtml`  
**Features:**
- Global search across:
  - Risks
  - Controls
  - Policies
  - Evidence
  - Audits
  - Assessments

---

## 22. Dashboard & Analytics

**Location:** `Controllers/DashboardController.cs`, `Controllers/Api/DashboardController.cs`  
**Views:** `Views/Dashboard/`  
**Features:**
- Executive dashboard
- Compliance trends
- Risk heatmap
- Framework comparison
- Task metrics
- Evidence metrics
- User activity
- Top actions
- Dashboard snapshots (ClickHouse)

---

## 23. Document Flow

**Location:** `Views/DocumentFlow/` (3 files)  
**Features:**
- Document upload
- Document review
- Document flow tracking

---

## 24. Email Templates

**Location:** `Views/EmailTemplates/` (6 files)  
**Features:**
- Email template management
- Template customization

---

## 25. Role Delegation

**Location:** `Controllers/RoleProfileController.cs`  
**Views:** `Views/RoleDelegation/`  
**Features:**
- Role delegation rules
- Delegation logging

**Entities:**
- `DelegationRule` - Delegation rules
- `DelegationLog` - Delegation logs

---

## 26. Vault System

**Location:** `Controllers/VaultController.cs`  
**Views:** `Views/Vault/`  
**Features:**
- Secure document vault
- Cryptographic asset storage

---

## 27. Invitation System

**Location:** `Controllers/Api/UserInvitationController.cs`  
**Views:** `Views/Invitation/`  
**Features:**
- User invitation management
- Invitation tracking

---

## 28. Code Quality Monitoring

**Location:** `Controllers/Api/CodeQualityController.cs`, `Services/Implementations/CodeQualityService.cs`  
**Features:**
- Code quality metrics
- Quality monitoring
- Quality alerts

---

## 29. Diagnostic Services

**Location:** `Controllers/Api/DiagnosticController.cs`, `Services/Implementations/DiagnosticAgentService.cs`  
**Features:**
- System diagnostics
- Diagnostic agent service
- Performance metrics

---

## 30. Resilience Service

**Location:** `Controllers/Api/ResilienceController.cs`, `Services/Implementations/ResilienceService.cs`  
**Features:**
- Autonomous risk resilience
- Resilience tracking

**Entities:**
- `AutonomousRiskResilience` - Risk resilience (23,656 bytes)

---

## 31. Seed & Data Management

**Location:** `Controllers/Api/SeedController.cs`, `Data/Seeds/`  
**Features:**
- Catalog seeding
- Workflow seeding
- Regulator seeding
- KSA framework seeding
- Control import
- POC organization seeding

---

## 32. Serial Number Service

**Location:** `Services/Implementations/SerialNumberService.cs`  
**Features:**
- Serial number generation
- Counter management

**Entities:**
- `SerialNumberCounter` - Serial number counters

---

## 33. Data Quality

**Features:**
- Data quality scoring
- Quality metrics

**Entities:**
- `DataQualityScore` - Data quality scores

---

## 34. Validation System

**Features:**
- Validation rules
- Validation results

**Entities:**
- `ValidationRule` - Validation rules
- `ValidationResult` - Validation results

---

## 35. Support Agent Service

**Location:** `Services/Implementations/SupportAgentService.cs`  
**Features:**
- AI-powered support agent
- Support conversation management

---

## 36. Menu Service

**Location:** `Services/Implementations/MenuService.cs`  
**Features:**
- Dynamic menu generation
- Permission-based menu visibility
- Feature-based menu items

---

## 37. Metrics Service

**Location:** `Services/Implementations/MetricsService.cs`  
**Features:**
- System metrics collection
- Performance metrics
- Business metrics

---

## 38. Alert Service

**Location:** `Services/Implementations/AlertService.cs`  
**Features:**
- Alert generation
- Alert delivery
- Alert management

---

## Integration Capabilities

### External System Integrations

1. **ERP Integration**
   - ERP system configuration
   - ERP extract configuration
   - ERP extract execution

2. **HRIS Integration**
   - HRIS employee data
   - HRIS service

3. **Email Integration**
   - SMTP email service
   - MailKit integration
   - Email templates

4. **Slack Integration**
   - Slack notifications
   - Slack webhook support

5. **Microsoft Teams Integration**
   - Teams notifications
   - Teams webhook support
   - Teams notification config

6. **SMS Integration (Twilio)**
   - SMS notifications
   - Twilio service

7. **Webhook Integration**
   - Webhook subscriptions
   - Webhook delivery
   - Webhook retry logic

8. **ClickHouse Analytics**
   - OLAP queries
   - Analytics projection
   - Dashboard data

9. **Kafka + Debezium**
   - Change data capture (CDC)
   - Event streaming
   - Database change events

10. **Redis Cache**
    - Distributed caching
    - Session storage
    - Rate limiting

---

## API Endpoints Summary

### REST API Controllers (24+)

**Core Domain APIs:**
- `/api/risk` - Risk management
- `/api/control` - Control management
- `/api/evidence` - Evidence management
- `/api/assessment` - Assessment management
- `/api/audit` - Audit management
- `/api/policy` - Policy management
- `/api/workflow` - Workflow management

**Management APIs:**
- `/api/dashboard` - Dashboard data
- `/api/reports` - Report generation
- `/api/plans` - Plan management
- `/api/subscription` - Subscription management
- `/api/tenants` - Tenant management
- `/api/users` - User management

**Specialized APIs:**
- `/api/seed` - Data seeding
- `/api/catalog` - Catalog management
- `/api/assets` - Asset management
- `/api/diagnostics` - System diagnostics
- `/api/code-quality` - Code quality
- `/api/resilience` - Resilience metrics
- `/api/shahin` - Shahin AI APIs
- `/api/workspace` - Workspace management

---

## View Components Summary

### Shared Components
- `_Layout.cshtml` - Main layout
- `_WelcomeTour.cshtml` - Welcome tour
- `_SupportChatWidget.cshtml` - Support chat
- `_GlossaryModal.cshtml` - Glossary modal
- `_SecurePasswordModal.cshtml` - Password modal
- `WorkspaceSwitcher` - Workspace switcher component

### Specialized Views
- **Onboarding Wizard:** 15 step views
- **Workflow UI:** 8 specialized workflow views
- **Platform Admin:** 13 admin views
- **Shahin AI:** 7 module views
- **Help System:** 5 help views

---

## Feature Flags

**Location:** `appsettings.json` - `GrcFeatureFlags`  
**Flags:**
- `UseSecurePasswordGeneration` - Secure password generation
- `UseSessionBasedClaims` - Session-based claims
- `UseEnhancedAuditLogging` - Enhanced audit logging
- `UseDeterministicTenantResolution` - Deterministic tenant resolution
- `DisableDemoLogin` - Disable demo login
- `CanaryPercentage` - Canary deployment percentage
- `VerifyConsistency` - Consistency verification
- `LogFeatureFlagDecisions` - Feature flag logging

---

## Summary Statistics

| Category | Count |
|----------|-------|
| **Core GRC Modules** | 18 |
| **Shahin AI Modules** | 6 |
| **Specialized Modules** | 3 (CCM, KRI, Analytics) |
| **Total Controllers** | 81 |
| **Total Services** | 182 |
| **Total Entities** | 189 |
| **Total Views** | 254 |
| **Total Components** | 56 |
| **Background Jobs** | 6 |
| **Integration Points** | 15+ |
| **API Endpoints** | 100+ |
| **Permissions** | 40+ |
| **Features** | 100+ |

---

**Report Generated:** 2025-01-07  
**Status:** Complete feature inventory based on actual code analysis
