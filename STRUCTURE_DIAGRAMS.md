# GRC System - Structure Diagrams
## Visual Architecture and Code Structure Documentation

**Generated:** 2025-01-07  
**Based on:** Actual code analysis  

---

## 1. High-Level Architecture Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser]
        API_Client[API Clients]
    end
    
    subgraph "Presentation Layer"
        MVC_Controllers[81 MVC Controllers]
        API_Controllers[API Controllers]
        Blazor_Components[56 Blazor Components]
        Razor_Views[254 Razor Views]
    end
    
    subgraph "Application Layer"
        Services[182 Services]
        Permissions[Permission System]
        Policy_Engine[Policy Engine]
        Middleware[5 Middleware]
    end
    
    subgraph "Data Layer"
        UnitOfWork[Unit of Work]
        Repositories[Generic Repositories]
        DbContext[GrcDbContext<br/>189 DbSets]
        AuthDbContext[GrcAuthDbContext<br/>Identity Tables]
    end
    
    subgraph "Background Processing"
        Hangfire[6 Background Jobs]
        MassTransit[Message Queue]
    end
    
    subgraph "Infrastructure"
        PostgreSQL[(PostgreSQL<br/>GrcMvcDb)]
        AuthDB[(PostgreSQL<br/>GrcAuthDb)]
        Redis[(Redis Cache)]
        ClickHouse[(ClickHouse OLAP)]
        Kafka[(Kafka + Debezium)]
    end
    
    Browser --> MVC_Controllers
    Browser --> Blazor_Components
    Browser --> Razor_Views
    API_Client --> API_Controllers
    
    MVC_Controllers --> Services
    API_Controllers --> Services
    Blazor_Components --> Services
    
    Services --> Permissions
    Services --> Policy_Engine
    Services --> UnitOfWork
    
    UnitOfWork --> Repositories
    Repositories --> DbContext
    Repositories --> AuthDbContext
    
    DbContext --> PostgreSQL
    AuthDbContext --> AuthDB
    
    Services --> Hangfire
    Services --> MassTransit
    
    MassTransit --> Kafka
    Hangfire --> PostgreSQL
    Services --> Redis
    Services --> ClickHouse
```

---

## 2. Directory Structure Tree

```
src/GrcMvc/
│
├── 📁 Application/              (23 files)
│   ├── Permissions/            (7 files)
│   │   ├── GrcPermissions.cs
│   │   ├── PermissionDefinitionProvider.cs
│   │   └── ...
│   └── Policy/                  (16 files)
│       ├── PolicyEnforcer.cs
│       ├── PolicyStore.cs
│       └── ...
│
├── 📁 Controllers/              (81 files)
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── DashboardController.cs
│   ├── RiskController.cs
│   ├── ControlController.cs
│   ├── EvidenceController.cs
│   ├── WorkflowController.cs
│   └── Api/                    (24 API controllers)
│       ├── RiskApiController.cs
│       ├── EvidenceApiController.cs
│       └── ...
│
├── 📁 Services/                 (182 files)
│   ├── Interfaces/              (75 interfaces)
│   │   ├── IRiskService.cs
│   │   ├── IControlService.cs
│   │   ├── IWorkflowService.cs
│   │   └── ...
│   └── Implementations/         (107 implementations)
│       ├── RiskService.cs
│       ├── ControlService.cs
│       ├── WorkflowService.cs
│       ├── RBAC/               (RBAC services)
│       └── Workflows/          (Workflow services)
│
├── 📁 Models/                   (124 files)
│   ├── Entities/                (80 entity files)
│   │   ├── Risk.cs
│   │   ├── Control.cs
│   │   ├── Evidence.cs
│   │   ├── Workflow*.cs
│   │   └── ...
│   ├── DTOs/                    (24 DTO files)
│   │   ├── RiskDto.cs
│   │   ├── ControlDto.cs
│   │   └── ...
│   └── ViewModels/              (3 files)
│
├── 📁 Data/                     
│   ├── GrcDbContext.cs          (189 DbSets)
│   ├── GrcAuthDbContext.cs     (Identity tables)
│   ├── IUnitOfWork.cs
│   ├── UnitOfWork.cs
│   ├── Repositories/
│   ├── Migrations/              (36+ migrations)
│   └── Seeds/                   (Seed data)
│
├── 📁 Views/                    (254 .cshtml files)
│   ├── Home/
│   ├── Dashboard/
│   ├── Risk/
│   ├── Control/
│   ├── Evidence/
│   └── ...
│
├── 📁 Components/               (56 .razor files)
│   └── Shared/
│
├── 📁 Middleware/                (5 files)
│   ├── TenantResolutionMiddleware.cs
│   ├── SecurityHeadersMiddleware.cs
│   └── ...
│
├── 📁 BackgroundJobs/            (6 files)
│   ├── NotificationDeliveryJob.cs
│   ├── EscalationJob.cs
│   └── ...
│
└── 📁 wwwroot/                  (Static files)
    ├── css/
    ├── js/
    └── lib/
```

---

## 3. Database Entity Relationship Overview

```mermaid
erDiagram
    Tenant ||--o{ TenantUser : "has"
    Tenant ||--o{ Workspace : "contains"
    Tenant ||--o{ Risk : "owns"
    Tenant ||--o{ Control : "owns"
    Tenant ||--o{ Evidence : "owns"
    Tenant ||--o{ Assessment : "owns"
    Tenant ||--o{ Audit : "owns"
    
    Workspace ||--o{ WorkspaceMembership : "has"
    Workspace ||--o{ WorkspaceControl : "contains"
    
    Risk ||--o{ Control : "mitigated_by"
    Control ||--o{ Evidence : "requires"
    Control ||--o{ Assessment : "tested_by"
    Assessment ||--o{ Evidence : "uses"
    
    WorkflowDefinition ||--o{ WorkflowInstance : "instantiates"
    WorkflowInstance ||--o{ WorkflowTask : "contains"
    WorkflowTask ||--o{ ApprovalChain : "requires"
    ApprovalChain ||--o{ ApprovalInstance : "creates"
    
    Role ||--o{ RolePermission : "has"
    Role ||--o{ UserRoleAssignment : "assigned_to"
    Permission ||--o{ RolePermission : "granted_to"
    
    Framework ||--o{ FrameworkControl : "contains"
    FrameworkControl ||--o{ Control : "maps_to"
    
    Subscription ||--o{ SubscriptionPlan : "uses"
    Tenant ||--o{ Subscription : "has"
    
    Evidence ||--o{ EvidencePack : "grouped_in"
    Control ||--o{ EvidencePack : "requires"
```

---

## 4. Service Layer Architecture

```mermaid
graph LR
    subgraph "Controller Layer"
        C1[MVC Controllers]
        C2[API Controllers]
    end
    
    subgraph "Service Interfaces (75)"
        I1[IRiskService]
        I2[IControlService]
        I3[IEvidenceService]
        I4[IWorkflowService]
        I5[ITenantService]
        I6[IAuthorizationService]
        I7[INotificationService]
    end
    
    subgraph "Service Implementations (107)"
        S1[RiskService]
        S2[ControlService]
        S3[EvidenceService]
        S4[WorkflowService]
        S5[TenantService]
        S6[AuthorizationService]
        S7[NotificationService]
    end
    
    subgraph "Data Access"
        UOW[UnitOfWork]
        REPO[GenericRepository]
    end
    
    C1 --> I1
    C1 --> I2
    C2 --> I3
    C2 --> I4
    
    I1 --> S1
    I2 --> S2
    I3 --> S3
    I4 --> S4
    
    S1 --> UOW
    S2 --> UOW
    S3 --> UOW
    S4 --> UOW
    
    UOW --> REPO
```

---

## 5. Multi-Tenancy Architecture

```mermaid
sequenceDiagram
    participant Client
    participant Middleware as TenantResolutionMiddleware
    participant Service as TenantContextService
    participant DbContext as GrcDbContext
    participant DB as PostgreSQL
    
    Client->>Middleware: HTTP Request
    Middleware->>Middleware: Extract Tenant ID
    Middleware->>Service: Set Tenant Context
    Service->>Service: Store in HttpContext
    Client->>Service: Service Call
    Service->>DbContext: Query Request
    DbContext->>DbContext: Apply Global Filter (TenantId)
    DbContext->>DB: SELECT ... WHERE TenantId = @tenantId
    DB-->>DbContext: Filtered Results
    DbContext-->>Service: Tenant-Scoped Data
    Service-->>Client: Response
```

---

## 6. Request Flow Diagram

```mermaid
flowchart TD
    Start[HTTP Request] --> Middleware1[TenantResolutionMiddleware]
    Middleware1 --> Middleware2[SecurityHeadersMiddleware]
    Middleware2 --> Middleware3[RequestLoggingMiddleware]
    Middleware3 --> Auth{Authenticated?}
    
    Auth -->|No| Login[Login Page]
    Auth -->|Yes| Authz{Authorized?}
    
    Authz -->|No| Forbidden[403 Forbidden]
    Authz -->|Yes| Policy{Policy Check}
    
    Policy -->|Violation| PolicyError[PolicyViolationException]
    Policy -->|Pass| Controller[Controller Action]
    
    Controller --> Service[Service Layer]
    Service --> PolicyEngine[Policy Enforcement]
    Service --> UnitOfWork[Unit of Work]
    
    UnitOfWork --> Repository[Repository]
    Repository --> DbContext[GrcDbContext]
    DbContext --> Database[(PostgreSQL)]
    
    Database --> DbContext
    DbContext --> Repository
    Repository --> UnitOfWork
    UnitOfWork --> Service
    Service --> Controller
    Controller --> View[Razor View / API Response]
    View --> End[HTTP Response]
```

---

## 7. Background Job Architecture

```mermaid
graph TB
    subgraph "Hangfire Dashboard"
        HD[Hangfire UI]
    end
    
    subgraph "Background Jobs (6)"
        J1[NotificationDeliveryJob<br/>Every 5 minutes]
        J2[EscalationJob<br/>Every 1 hour]
        J3[SlaMonitorJob<br/>Every 30 minutes]
        J4[WebhookRetryJob<br/>On demand]
        J5[AnalyticsProjectionJob<br/>Scheduled]
        J6[CodeQualityMonitorJob<br/>Scheduled]
    end
    
    subgraph "Services"
        NS[NotificationService]
        WS[WorkflowService]
        AS[AnalyticsService]
    end
    
    subgraph "Storage"
        PG[(PostgreSQL<br/>Hangfire Tables)]
    end
    
    HD --> J1
    HD --> J2
    HD --> J3
    
    J1 --> NS
    J2 --> WS
    J3 --> WS
    J4 --> WS
    J5 --> AS
    J6 --> AS
    
    J1 --> PG
    J2 --> PG
    J3 --> PG
```

---

## 8. Database Schema Overview

```mermaid
graph TB
    subgraph "Multi-Tenancy Core"
        T[Tenants]
        TU[TenantUsers]
        W[Workspaces]
        WM[WorkspaceMemberships]
    end
    
    subgraph "GRC Domain"
        R[Risks]
        C[Controls]
        E[Evidence]
        A[Assessments]
        AU[Audits]
        P[Policies]
    end
    
    subgraph "Workflow System"
        WD[WorkflowDefinitions]
        WI[WorkflowInstances]
        WT[WorkflowTasks]
        AC[ApprovalChains]
        AI[ApprovalInstances]
    end
    
    subgraph "RBAC"
        RO[Roles]
        PERM[Permissions]
        RP[RolePermissions]
        URA[UserRoleAssignments]
    end
    
    subgraph "Catalogs"
        RC[RegulatorCatalogs]
        FC[FrameworkCatalogs]
        CC[ControlCatalogs]
    end
    
    T --> TU
    T --> W
    W --> WM
    T --> R
    T --> C
    T --> E
    WD --> WI
    WI --> WT
    WT --> AC
    AC --> AI
    RO --> RP
    PERM --> RP
```

---

## 9. Controller Organization

```mermaid
mindmap
  root((Controllers<br/>81 Files))
    MVC Controllers
      Account
      Admin
      Dashboard
      Risk
      Control
      Evidence
      Audit
      Policy
      Workflow
      Framework
      Vendor
      Subscription
    API Controllers
      RiskApi
      ControlApi
      EvidenceApi
      AuditApi
      AssessmentApi
      PolicyApi
      WorkflowApi
      DashboardApi
      PlansApi
      SubscriptionApi
    Specialized
      OnboardingWizard
      PlatformAdmin
      TenantAdmin
      RoleProfile
      ShahinAI
      Integrations
```

---

## 10. Service Dependency Graph

```mermaid
graph TD
    subgraph "Core Domain Services"
        RS[RiskService]
        CS[ControlService]
        ES[EvidenceService]
        AS[AssessmentService]
        PS[PolicyService]
    end
    
    subgraph "Workflow Services"
        WS[WorkflowService]
        WES[WorkflowEngineService]
        WRS[WorkflowRoutingService]
    end
    
    subgraph "Infrastructure Services"
        TS[TenantService]
        US[UserService]
        NS[NotificationService]
        AUTH[AuthorizationService]
    end
    
    subgraph "Data Access"
        UOW[UnitOfWork]
    end
    
    RS --> UOW
    CS --> UOW
    ES --> UOW
    AS --> UOW
    PS --> UOW
    
    WS --> WES
    WS --> WRS
    WES --> UOW
    WRS --> TS
    WRS --> NS
    
    RS --> AUTH
    CS --> AUTH
    ES --> AUTH
```

---

## 11. Policy Engine Flow

```mermaid
flowchart TD
    Start[Service Method Call] --> CreateContext[Create PolicyContext]
    CreateContext --> LoadPolicy[PolicyStore.LoadPolicy]
    LoadPolicy --> ParseYAML[Parse YAML File]
    ParseYAML --> Validate[Validate Policy Schema]
    
    Validate --> CheckExceptions{Check Exceptions}
    CheckExceptions -->|Match| SkipRule[Skip Rule]
    CheckExceptions -->|No Match| EvaluateRules[Evaluate Rules by Priority]
    
    EvaluateRules --> MatchRule{Rule Matches?}
    MatchRule -->|No| NextRule[Next Rule]
    MatchRule -->|Yes| CheckConditions{All Conditions Met?}
    
    CheckConditions -->|No| NextRule
    CheckConditions -->|Yes| CheckEffect{Effect Type?}
    
    CheckEffect -->|allow| Allow[Allow Action]
    CheckEffect -->|deny| Deny[Deny - Throw Exception]
    CheckEffect -->|mutate| Mutate[Apply Mutations]
    CheckEffect -->|audit| Audit[Log Decision]
    
    Mutate --> Continue[Continue Evaluation]
    Continue --> NextRule
    NextRule --> MoreRules{More Rules?}
    MoreRules -->|Yes| MatchRule
    MoreRules -->|No| Default[Apply Default Effect]
    
    Allow --> End[Proceed with Action]
    Deny --> End
    Default --> End
    Audit --> End
```

---

## 12. Entity Count by Category

```mermaid
pie title Database Entities by Category (189 Total)
    "Multi-Tenancy" : 15
    "GRC Domain" : 14
    "Workflow System" : 18
    "RBAC" : 8
    "Catalogs" : 9
    "Control Library" : 23
    "Assessment" : 3
    "Risk Management" : 5
    "Integration" : 17
    "AI Agents" : 12
    "Subscription" : 6
    "Other" : 59
```

---

## 13. File Distribution

```mermaid
pie title Code Files by Type (565 Total C# Files)
    "Controllers" : 81
    "Services" : 182
    "Entities" : 80
    "DTOs/ViewModels" : 27
    "Data Layer" : 15
    "Application Layer" : 23
    "Middleware" : 5
    "Background Jobs" : 6
    "Other" : 146
```

---

## 14. Technology Stack Layers

```mermaid
graph TB
    subgraph "Frontend"
        Razor[254 Razor Views]
        Blazor[56 Blazor Components]
        JS[JavaScript/CSS]
    end
    
    subgraph "Application Framework"
        ASPNET[ASP.NET Core 8.0]
        MVC[MVC Pattern]
        SignalR[SignalR Hubs]
    end
    
    subgraph "Business Logic"
        Services[182 Services]
        Policy[Policy Engine]
        Permissions[Permission System]
    end
    
    subgraph "Data Access"
        EF[Entity Framework Core 8.0.8]
        UOW[Unit of Work]
        Repo[Repository Pattern]
    end
    
    subgraph "Background Processing"
        Hangfire[Hangfire 1.8.14]
        MassTransit[MassTransit 8.1.3]
    end
    
    subgraph "Databases"
        PG1[(PostgreSQL<br/>GrcMvcDb)]
        PG2[(PostgreSQL<br/>GrcAuthDb)]
        Redis[(Redis)]
        CH[(ClickHouse)]
    end
    
    Razor --> ASPNET
    Blazor --> ASPNET
    ASPNET --> Services
    Services --> EF
    EF --> PG1
    Services --> Hangfire
    Hangfire --> PG1
    Services --> MassTransit
    MassTransit --> Kafka
```

---

## 15. Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant User
    participant Browser
    participant AccountController
    participant AuthService
    participant Identity
    participant JWT
    participant PolicyEngine
    participant Service
    
    User->>Browser: Login Request
    Browser->>AccountController: POST /Account/Login
    AccountController->>AuthService: Authenticate()
    AuthService->>Identity: Validate Credentials
    Identity-->>AuthService: User Identity
    AuthService->>JWT: Generate Token
    JWT-->>AuthService: JWT Token
    AuthService-->>AccountController: Auth Result
    AccountController-->>Browser: Set Cookie/Token
    
    Browser->>Service: API Request (with Token)
    Service->>JWT: Validate Token
    JWT-->>Service: Claims Principal
    Service->>PolicyEngine: Check Permission
    PolicyEngine->>PolicyEngine: Evaluate Rules
    PolicyEngine-->>Service: Allow/Deny
    Service-->>Browser: Response
```

---

## 16. Workflow Execution Flow

```mermaid
stateDiagram-v2
    [*] --> Created: Create Workflow
    Created --> InProgress: Start
    InProgress --> TaskAssigned: Assign Task
    TaskAssigned --> InReview: Submit
    InReview --> Approved: Approve
    InReview --> Rejected: Reject
    Rejected --> TaskAssigned: Reassign
    Approved --> Completed: All Tasks Done
    Completed --> [*]
    
    InProgress --> Escalated: Timeout
    Escalated --> TaskAssigned: Reassign
```

---

## 17. Docker Services Architecture

```mermaid
graph TB
    subgraph "Docker Network: grc-network"
        subgraph "Application"
            APP[grcmvc:latest<br/>Ports: 80, 443]
        end
        
        subgraph "Databases"
            PG[(postgres:15-alpine<br/>Port: 5432<br/>GrcMvcDb + GrcAuthDb)]
            CH[(clickhouse:latest<br/>Ports: 8123, 9000)]
            RD[(redis:7-alpine<br/>Port: 6379)]
        end
        
        subgraph "Message Queue"
            ZK[zookeeper:latest<br/>Port: 2181]
            KF[kafka:latest<br/>Port: 9092]
            KC[kafka-connect:latest<br/>Port: 8083<br/>Debezium CDC]
        end
    end
    
    APP --> PG
    APP --> RD
    APP --> CH
    APP --> KF
    KC --> KF
    KC --> PG
    KF --> ZK
```

---

## 18. Data Flow: Evidence Upload

```mermaid
flowchart LR
    User[User] --> Upload[Upload Evidence]
    Upload --> Controller[EvidenceController]
    Controller --> Validate[Validate File]
    Validate --> Policy{Policy Check}
    Policy -->|Deny| Error[Policy Violation]
    Policy -->|Allow| Service[EvidenceService]
    Service --> Storage[FileStorageService]
    Storage --> FileSystem[(File System)]
    Service --> DB[Save Metadata]
    DB --> GrcDbContext
    GrcDbContext --> PostgreSQL[(PostgreSQL)]
    Service --> Workflow[Trigger Workflow]
    Workflow --> Notification[Send Notification]
    Notification --> User
```

---

## 19. Component Size Distribution

```mermaid
graph LR
    subgraph "Controllers"
        Small[< 5KB: 45 files]
        Medium[5-20KB: 30 files]
        Large[> 20KB: 6 files]
    end
    
    subgraph "Entities"
        E_Small[< 5KB: 60 files]
        E_Medium[5-20KB: 15 files]
        E_Large[> 20KB: 5 files]
    end
    
    subgraph "Services"
        S_Small[< 10KB: 120 files]
        S_Medium[10-30KB: 50 files]
        S_Large[> 30KB: 12 files]
    end
```

---

## 20. Integration Points

```mermaid
graph TB
    subgraph "GRC Application"
        Core[Core Services]
    end
    
    subgraph "External Systems"
        ERP[ERP Systems]
        HRIS[HRIS Systems]
        Email[Email Service]
        Slack[Slack]
        Teams[Microsoft Teams]
    end
    
    subgraph "Message Queue"
        Kafka[Kafka Topics]
    end
    
    subgraph "CDC"
        Debezium[Debezium Connectors]
    end
    
    Core --> ERP
    Core --> HRIS
    Core --> Email
    Core --> Slack
    Core --> Teams
    Core --> Kafka
    Debezium --> Kafka
    Kafka --> Core
```

---

## Diagram Legend

### Symbols Used:
- 📁 = Directory/Folder
- [ ] = Component/Class
- ( ) = Database/Storage
- { } = Decision Point
- --> = Flow/Dependency
- ||--o{ = One-to-Many Relationship

### Colors (in rendered Mermaid):
- Blue = Controllers/API Layer
- Green = Services/Business Logic
- Orange = Data Access Layer
- Purple = Infrastructure
- Yellow = Background Processing

---

## How to View These Diagrams

1. **Mermaid Diagrams**: 
   - View in GitHub (renders automatically)
   - Use [Mermaid Live Editor](https://mermaid.live)
   - VS Code with Mermaid extension
   - Markdown viewers with Mermaid support

2. **ASCII Diagrams**: 
   - View directly in text editor
   - Use monospace font for alignment

3. **Export Options**:
   - Mermaid diagrams can be exported as PNG/SVG
   - Use online tools or VS Code extensions

---

**Note:** All diagrams are based on actual code structure analysis, not documentation assumptions.
