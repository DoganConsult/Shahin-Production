# GRC System - Feature Connection Overview
## High-Level Architecture and Feature Dependencies

**Generated:** 2025-01-07  
**Purpose:** Visual overview of how all GRC system features connect and interact  

---

## 1. Complete System Architecture

```mermaid
graph TB
    subgraph "User Interface Layer"
        Browser[Web Browser]
        Mobile[Mobile App]
        API_Client[API Clients]
    end
    
    subgraph "Presentation Layer"
        MVC[81 MVC Controllers]
        API[24 API Controllers]
        Views[254 Razor Views]
        Blazor[56 Blazor Components]
    end
    
    subgraph "Core GRC Domain - 18 Modules"
        Risk[Risk Management]
        Control[Control Management]
        Evidence[Evidence Management]
        Assessment[Assessment Management]
        Audit[Audit Management]
        Policy[Policy Management]
        Workflow[Workflow Engine]
        ActionPlan[Action Plans]
        Compliance[Compliance Calendar]
        Framework[Regulatory Frameworks]
        Regulator[Regulators]
        Vendor[Vendor Management]
        Report[Reports & Analytics]
        Plan[Plans & Phases]
        Subscription[Subscriptions]
        Notification[Notifications]
        Integration[Integrations]
        Help[Help & Support]
    end
    
    subgraph "Shahin AI Platform - 6 Modules"
        MAP[MAP Module<br/>Control Library]
        APPLY[APPLY Module<br/>Scope & Applicability]
        PROVE[PROVE Module<br/>Evidence & Testing]
        WATCH[WATCH Module<br/>Monitoring & Alerts]
        FIX[FIX Module<br/>Remediation]
        VAULT[VAULT Module<br/>Secure Storage]
    end
    
    subgraph "Specialized Modules"
        CCM[CCM<br/>Continuous Control Monitoring]
        KRI[KRI Dashboard<br/>Risk Indicators]
        Onboarding[12-Step Onboarding Wizard]
        PlatformAdmin[Platform Administration]
    end
    
    subgraph "Application Layer - 182 Services"
        DomainServices[Domain Services]
        WorkflowServices[Workflow Services]
        TenantServices[Tenant Services]
        RBACServices[RBAC Services]
        NotificationServices[Notification Services]
        IntegrationServices[Integration Services]
        AnalyticsServices[Analytics Services]
    end
    
    subgraph "Policy & Security"
        PolicyEngine[Policy Engine<br/>YAML-based]
        Permissions[Permission System<br/>40+ Permissions]
        RBAC[RBAC System]
    end
    
    subgraph "Data Access Layer"
        UnitOfWork[Unit of Work]
        Repository[Generic Repository]
        DbContext[GrcDbContext<br/>189 DbSets]
        AuthDbContext[GrcAuthDbContext<br/>Identity]
    end
    
    subgraph "Background Processing"
        Hangfire[6 Background Jobs]
        MassTransit[MassTransit<br/>Message Queue]
    end
    
    subgraph "Infrastructure"
        PostgreSQL[(PostgreSQL<br/>GrcMvcDb)]
        AuthDB[(PostgreSQL<br/>GrcAuthDb)]
        Redis[(Redis Cache)]
        ClickHouse[(ClickHouse OLAP)]
        Kafka[(Kafka + Debezium)]
    end
    
    Browser --> MVC
    Browser --> Views
    Browser --> Blazor
    Mobile --> API
    API_Client --> API
    
    MVC --> Risk
    MVC --> Control
    MVC --> Evidence
    MVC --> Assessment
    MVC --> Audit
    MVC --> Policy
    MVC --> Workflow
    MVC --> MAP
    MVC --> APPLY
    MVC --> PROVE
    MVC --> WATCH
    MVC --> FIX
    MVC --> VAULT
    MVC --> CCM
    MVC --> Onboarding
    MVC --> PlatformAdmin
    
    API --> DomainServices
    API --> WorkflowServices
    API --> AnalyticsServices
    
    Risk --> DomainServices
    Control --> DomainServices
    Evidence --> DomainServices
    Assessment --> DomainServices
    Audit --> DomainServices
    Policy --> DomainServices
    Workflow --> WorkflowServices
    
    MAP --> DomainServices
    APPLY --> DomainServices
    PROVE --> DomainServices
    WATCH --> DomainServices
    FIX --> DomainServices
    VAULT --> DomainServices
    
    DomainServices --> PolicyEngine
    DomainServices --> Permissions
    DomainServices --> RBAC
    DomainServices --> UnitOfWork
    
    WorkflowServices --> NotificationServices
    WorkflowServices --> TenantServices
    
    UnitOfWork --> Repository
    Repository --> DbContext
    Repository --> AuthDbContext
    
    DbContext --> PostgreSQL
    AuthDbContext --> AuthDB
    
    DomainServices --> Hangfire
    DomainServices --> MassTransit
    
    Hangfire --> PostgreSQL
    MassTransit --> Kafka
    AnalyticsServices --> ClickHouse
    DomainServices --> Redis
```

---

## 2. Feature Dependency Graph

```mermaid
graph LR
    subgraph "Foundation Layer"
        MultiTenancy[Multi-Tenancy]
        RBAC[RBAC System]
        PolicyEngine[Policy Engine]
        WorkflowEngine[Workflow Engine]
    end
    
    subgraph "Core GRC Features"
        RiskMgmt[Risk Management]
        ControlMgmt[Control Management]
        EvidenceMgmt[Evidence Management]
        AssessmentMgmt[Assessment Management]
        AuditMgmt[Audit Management]
        PolicyMgmt[Policy Management]
    end
    
    subgraph "Supporting Features"
        Reporting[Reporting & Analytics]
        Notifications[Notification System]
        Integrations[Integration Hub]
        Onboarding[Onboarding System]
    end
    
    subgraph "Advanced Features"
        ShahinAI[Shahin AI Platform]
        CCM[CCM Monitoring]
        Analytics[Analytics Dashboard]
    end
    
    MultiTenancy --> RiskMgmt
    MultiTenancy --> ControlMgmt
    MultiTenancy --> EvidenceMgmt
    MultiTenancy --> AssessmentMgmt
    MultiTenancy --> AuditMgmt
    MultiTenancy --> PolicyMgmt
    
    RBAC --> RiskMgmt
    RBAC --> ControlMgmt
    RBAC --> EvidenceMgmt
    RBAC --> AssessmentMgmt
    RBAC --> AuditMgmt
    RBAC --> PolicyMgmt
    RBAC --> Reporting
    RBAC --> Notifications
    
    PolicyEngine --> EvidenceMgmt
    PolicyEngine --> AssessmentMgmt
    PolicyEngine --> PolicyMgmt
    PolicyEngine --> AuditMgmt
    
    WorkflowEngine --> RiskMgmt
    WorkflowEngine --> ControlMgmt
    WorkflowEngine --> EvidenceMgmt
    WorkflowEngine --> AssessmentMgmt
    WorkflowEngine --> AuditMgmt
    WorkflowEngine --> PolicyMgmt
    WorkflowEngine --> Notifications
    
    RiskMgmt --> ControlMgmt
    ControlMgmt --> EvidenceMgmt
    EvidenceMgmt --> AssessmentMgmt
    AssessmentMgmt --> AuditMgmt
    AuditMgmt --> Reporting
    
    Onboarding --> MultiTenancy
    Onboarding --> RBAC
    Onboarding --> WorkflowEngine
    
    Integrations --> EvidenceMgmt
    Integrations --> Notifications
    Integrations --> Reporting
    
    ShahinAI --> ControlMgmt
    ShahinAI --> EvidenceMgmt
    ShahinAI --> RiskMgmt
    ShahinAI --> Analytics
    
    CCM --> ControlMgmt
    CCM --> EvidenceMgmt
    CCM --> Notifications
    
    Analytics --> Reporting
    Analytics --> RiskMgmt
    Analytics --> ControlMgmt
```

---

## 3. Data Flow Overview

```mermaid
flowchart TD
    Start[User Action] --> Auth{Authenticated?}
    Auth -->|No| Login[Login Page]
    Auth -->|Yes| Authz{Authorized?}
    
    Authz -->|No| Forbidden[403 Forbidden]
    Authz -->|Yes| Policy{Policy Check}
    
    Policy -->|Violation| PolicyError[Policy Violation]
    Policy -->|Pass| Controller[Controller]
    
    Controller --> Service[Service Layer]
    Service --> PolicyEnforce[Policy Enforcement]
    Service --> BusinessLogic[Business Logic]
    
    BusinessLogic --> Workflow{Workflow?}
    Workflow -->|Yes| WorkflowEngine[Workflow Engine]
    Workflow -->|No| DirectSave[Direct Save]
    
    WorkflowEngine --> TaskAssignment[Task Assignment]
    TaskAssignment --> Approval{Approval Needed?}
    Approval -->|Yes| ApprovalChain[Approval Chain]
    Approval -->|No| Complete[Complete]
    
    ApprovalChain --> Notification[Send Notification]
    Notification --> UserNotify[User Notified]
    
    DirectSave --> UnitOfWork[Unit of Work]
    Complete --> UnitOfWork
    
    UnitOfWork --> Repository[Repository]
    Repository --> DbContext[DbContext]
    DbContext --> Database[(PostgreSQL)]
    
    Service --> Event{Event Generated?}
    Event -->|Yes| DomainEvent[Domain Event]
    DomainEvent --> MassTransit[MassTransit]
    MassTransit --> Kafka[(Kafka)]
    
    Kafka --> Consumer[Event Consumers]
    Consumer --> Webhook[Webhook Delivery]
    Consumer --> Analytics[Analytics Projection]
    Consumer --> NotificationService[Notification Service]
    
    NotificationService --> Email[Email]
    NotificationService --> Slack[Slack]
    NotificationService --> Teams[Teams]
    NotificationService --> SMS[SMS]
    
    Analytics --> ClickHouse[(ClickHouse)]
    Database --> Response[Response to User]
    Response --> End[User Sees Result]
```

---

## 4. Integration Points Map

```mermaid
graph TB
    subgraph "GRC Application Core"
        Core[Core Services]
        Workflow[Workflow Engine]
        Evidence[Evidence Service]
        Notification[Notification Service]
        Analytics[Analytics Service]
    end
    
    subgraph "External Systems"
        ERP[ERP Systems]
        HRIS[HRIS Systems]
        EmailProvider[Email Service<br/>SMTP]
        Slack[Slack]
        Teams[Microsoft Teams]
        Twilio[Twilio SMS]
    end
    
    subgraph "Message Infrastructure"
        RabbitMQ[RabbitMQ]
        Kafka[Kafka]
        Debezium[Debezium CDC]
    end
    
    subgraph "Data Infrastructure"
        PostgreSQL[(PostgreSQL)]
        ClickHouse[(ClickHouse)]
        Redis[(Redis)]
    end
    
    subgraph "Background Processing"
        Hangfire[Hangfire]
        BackgroundJobs[6 Background Jobs]
    end
    
    Core --> ERP
    Core --> HRIS
    Core --> RabbitMQ
    Core --> PostgreSQL
    Core --> Redis
    
    Workflow --> Notification
    Workflow --> Hangfire
    
    Evidence --> Analytics
    Evidence --> PostgreSQL
    
    Notification --> EmailProvider
    Notification --> Slack
    Notification --> Teams
    Notification --> Twilio
    
    Analytics --> ClickHouse
    Analytics --> PostgreSQL
    
    RabbitMQ --> Kafka
    Debezium --> Kafka
    Kafka --> Core
    
    Hangfire --> BackgroundJobs
    BackgroundJobs --> PostgreSQL
    BackgroundJobs --> Notification
    BackgroundJobs --> Analytics
```

---

## 5. Module Interaction Matrix

```mermaid
graph TB
    subgraph "Risk Management"
        Risk[Risk Module]
    end
    
    subgraph "Control Management"
        Control[Control Module]
    end
    
    subgraph "Evidence Management"
        Evidence[Evidence Module]
    end
    
    subgraph "Assessment Management"
        Assessment[Assessment Module]
    end
    
    subgraph "Audit Management"
        Audit[Audit Module]
    end
    
    subgraph "Policy Management"
        Policy[Policy Module]
    end
    
    subgraph "Workflow Engine"
        Workflow[Workflow Module]
    end
    
    Risk -->|"Mitigated by"| Control
    Control -->|"Requires"| Evidence
    Control -->|"Tested by"| Assessment
    Assessment -->|"Uses"| Evidence
    Assessment -->|"Creates"| Audit
    Audit -->|"Finds"| Risk
    Audit -->|"Reviews"| Control
    Audit -->|"Validates"| Evidence
    Policy -->|"Enforces"| Evidence
    Policy -->|"Enforces"| Assessment
    Policy -->|"Enforces"| Control
    Workflow -->|"Orchestrates"| Risk
    Workflow -->|"Orchestrates"| Control
    Workflow -->|"Orchestrates"| Evidence
    Workflow -->|"Orchestrates"| Assessment
    Workflow -->|"Orchestrates"| Audit
    Workflow -->|"Orchestrates"| Policy
```

---

## 6. Feature Categories Overview

```mermaid
mindmap
  root((GRC System<br/>100+ Features))
    Core GRC Domain
      Risk Management
        Risk Registration
        Risk Assessment
        Risk Mitigation
        Risk Indicators
        Risk Heatmap
      Control Management
        Control Library
        Control Testing
        Control Effectiveness
        Framework Mapping
      Evidence Management
        Evidence Upload
        Evidence Classification
        Evidence Lifecycle
        Auto-Tagging
      Assessment Management
        Compliance Assessments
        Control Assessments
        Assessment Statistics
      Audit Management
        Audit Planning
        Audit Execution
        Audit Findings
        Audit Trail
      Policy Management
        Policy Documents
        Policy Approval
        Policy Enforcement
        Policy Violations
    Workflow System
      Workflow Engine
      Task Management
      Approval Chains
      Escalations
      Notifications
    Shahin AI Platform
      MAP Module
      APPLY Module
      PROVE Module
      WATCH Module
      FIX Module
      VAULT Module
    Multi-Tenancy
      Tenant Management
      Workspace System
      Team Management
      RACI Matrix
    RBAC System
      Permissions
      Roles
      Features
      Access Control
    Integrations
      ERP Integration
      HRIS Integration
      Webhooks
      Email/Slack/Teams
    Analytics
      Dashboards
      Reports
      Compliance Trends
      Risk Heatmap
    Onboarding
      12-Step Wizard
      Provisioning
      Team Setup
      Configuration
```

---

## 7. Service Dependency Hierarchy

```mermaid
graph TD
    subgraph "Top Level - Controllers"
        RiskController[RiskController]
        ControlController[ControlController]
        EvidenceController[EvidenceController]
        WorkflowController[WorkflowController]
    end
    
    subgraph "Domain Services"
        RiskService[RiskService]
        ControlService[ControlService]
        EvidenceService[EvidenceService]
        WorkflowService[WorkflowService]
    end
    
    subgraph "Supporting Services"
        PolicyHelper[PolicyEnforcementHelper]
        NotificationService[NotificationService]
        TenantService[TenantService]
        WorkspaceService[WorkspaceService]
    end
    
    subgraph "Infrastructure Services"
        UnitOfWork[UnitOfWork]
        Repository[GenericRepository]
        CachingService[CachingService]
    end
    
    subgraph "External Services"
        EmailService[EmailService]
        SlackService[SlackService]
        TeamsService[TeamsService]
    end
    
    RiskController --> RiskService
    ControlController --> ControlService
    EvidenceController --> EvidenceService
    WorkflowController --> WorkflowService
    
    RiskService --> PolicyHelper
    RiskService --> UnitOfWork
    RiskService --> NotificationService
    
    ControlService --> PolicyHelper
    ControlService --> UnitOfWork
    ControlService --> NotificationService
    
    EvidenceService --> PolicyHelper
    EvidenceService --> UnitOfWork
    EvidenceService --> NotificationService
    EvidenceService --> WorkflowService
    
    WorkflowService --> UnitOfWork
    WorkflowService --> NotificationService
    WorkflowService --> TenantService
    WorkflowService --> WorkspaceService
    
    UnitOfWork --> Repository
    UnitOfWork --> CachingService
    
    NotificationService --> EmailService
    NotificationService --> SlackService
    NotificationService --> TeamsService
```

---

## 8. Complete Feature Map

```mermaid
graph TB
    subgraph "User Access"
        Login[Login/Authentication]
        RBAC_Check[RBAC Authorization]
        Policy_Check[Policy Enforcement]
    end
    
    subgraph "Core GRC Workflows"
        Risk_Flow[Risk → Control → Evidence]
        Assessment_Flow[Assessment → Control → Evidence]
        Audit_Flow[Audit → Finding → Action Plan]
        Policy_Flow[Policy → Violation → Workflow]
    end
    
    subgraph "Shahin AI Workflows"
        MAP_Flow[MAP: Control Library]
        APPLY_Flow[APPLY: Scope & Applicability]
        PROVE_Flow[PROVE: Evidence & Testing]
        WATCH_Flow[WATCH: Monitoring]
        FIX_Flow[FIX: Remediation]
        VAULT_Flow[VAULT: Secure Storage]
    end
    
    subgraph "Supporting Systems"
        Onboarding_System[Onboarding Wizard]
        Subscription_System[Subscription Management]
        Notification_System[Notification Delivery]
        Integration_System[Integration Hub]
        Analytics_System[Analytics & Reporting]
    end
    
    subgraph "Background Processing"
        Hangfire_Jobs[Background Jobs]
        Message_Queue[Message Queue]
        Event_Streaming[Event Streaming]
    end
    
    Login --> RBAC_Check
    RBAC_Check --> Policy_Check
    Policy_Check --> Risk_Flow
    Policy_Check --> Assessment_Flow
    Policy_Check --> Audit_Flow
    Policy_Check --> Policy_Flow
    
    Risk_Flow --> MAP_Flow
    Risk_Flow --> WATCH_Flow
    Assessment_Flow --> PROVE_Flow
    Audit_Flow --> FIX_Flow
    Policy_Flow --> VAULT_Flow
    
    MAP_Flow --> APPLY_Flow
    APPLY_Flow --> PROVE_Flow
    PROVE_Flow --> WATCH_Flow
    WATCH_Flow --> FIX_Flow
    
    Onboarding_System --> Risk_Flow
    Onboarding_System --> Assessment_Flow
    
    Subscription_System --> Risk_Flow
    Subscription_System --> Assessment_Flow
    
    Risk_Flow --> Notification_System
    Assessment_Flow --> Notification_System
    Audit_Flow --> Notification_System
    Policy_Flow --> Notification_System
    
    Risk_Flow --> Integration_System
    Assessment_Flow --> Integration_System
    
    Risk_Flow --> Analytics_System
    Assessment_Flow --> Analytics_System
    Audit_Flow --> Analytics_System
    
    Notification_System --> Hangfire_Jobs
    Integration_System --> Message_Queue
    Analytics_System --> Event_Streaming
```

---

## 9. Feature Connection Summary

### Primary Connections

1. **Risk → Control → Evidence → Assessment → Audit**
   - Risks are mitigated by Controls
   - Controls require Evidence
   - Controls are tested by Assessments
   - Assessments use Evidence
   - Audits validate all of the above

2. **Policy → All GRC Modules**
   - Policy engine enforces rules on Evidence, Assessment, Control, Risk
   - Policy violations trigger Workflows

3. **Workflow → All GRC Modules**
   - Workflow engine orchestrates Risk, Control, Evidence, Assessment, Audit, Policy workflows
   - Workflows trigger Notifications

4. **RBAC → All Features**
   - Permissions control access to all 18 modules
   - Features determine menu visibility
   - Roles grant permissions

5. **Multi-Tenancy → All Features**
   - All data is tenant-scoped
   - Workspaces provide sub-scoping
   - Teams organize users within workspaces

6. **Shahin AI → Core GRC**
   - MAP provides control library
   - APPLY determines scope
   - PROVE manages evidence
   - WATCH monitors risks
   - FIX handles remediation
   - VAULT secures documents

7. **Onboarding → Provisioning → Configuration**
   - 12-step wizard creates tenant
   - Provisions database and workspace
   - Configures teams, workflows, and integrations

8. **Events → Consumers → Actions**
   - Domain events published to Kafka
   - Consumers trigger webhooks, notifications, analytics
   - Background jobs process async tasks

---

## 10. Navigation to Detailed Diagrams

For detailed feature connection diagrams, see:

1. **[DIAGRAMS_CORE_GRC.md](./DIAGRAMS_CORE_GRC.md)** - Core GRC domain feature connections
2. **[DIAGRAMS_WORKFLOW_SYSTEM.md](./DIAGRAMS_WORKFLOW_SYSTEM.md)** - Workflow system connections
3. **[DIAGRAMS_TENANT_RBAC.md](./DIAGRAMS_TENANT_RBAC.md)** - Multi-tenancy and RBAC flows
4. **[DIAGRAMS_SHAHIN_AI.md](./DIAGRAMS_SHAHIN_AI.md)** - Shahin AI platform architecture
5. **[DIAGRAMS_INTEGRATIONS.md](./DIAGRAMS_INTEGRATIONS.md)** - Integration and service connections
6. **[DIAGRAMS_ONBOARDING.md](./DIAGRAMS_ONBOARDING.md)** - Onboarding system flows
7. **[DIAGRAMS_DATA_FLOWS.md](./DIAGRAMS_DATA_FLOWS.md)** - Data flow patterns

---

**Last Updated:** 2025-01-07
