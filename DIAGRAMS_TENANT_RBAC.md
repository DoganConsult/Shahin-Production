# Multi-Tenancy & RBAC - Feature Connection Diagrams
## Tenant Hierarchy, Permission Flows, and Access Control

**Generated:** 2025-01-07  
**Focus:** Multi-tenancy architecture and RBAC system connections  

---

## 1. Tenant → Workspace → User Hierarchy

```mermaid
graph TB
    subgraph "Platform Level"
        PlatformAdmin[Platform Admin]
        PlatformAdminService[PlatformAdminService]
    end
    
    subgraph "Tenant Level"
        Tenant[Tenant Entity]
        TenantAdmin[Tenant Admin]
        TenantService[TenantService]
        TenantUser[TenantUser]
    end
    
    subgraph "Workspace Level"
        Workspace[Workspace Entity]
        WorkspaceService[WorkspaceService]
        WorkspaceMembership[WorkspaceMembership]
        WorkspaceControl[WorkspaceControl]
    end
    
    subgraph "Team Level"
        Team[Team Entity]
        TeamMember[TeamMember]
        RACIAssignment[RACIAssignment]
    end
    
    subgraph "User Level"
        ApplicationUser[ApplicationUser]
        UserRoleAssignment[UserRoleAssignment]
        UserWorkspace[UserWorkspace]
    end
    
    PlatformAdmin --> Tenant
    Tenant --> TenantAdmin
    Tenant --> TenantUser
    Tenant --> Workspace
    Workspace --> WorkspaceMembership
    Workspace --> WorkspaceControl
    Workspace --> Team
    Team --> TeamMember
    Team --> RACIAssignment
    WorkspaceMembership --> ApplicationUser
    TeamMember --> ApplicationUser
    ApplicationUser --> UserRoleAssignment
    ApplicationUser --> UserWorkspace
```

---

## 2. RBAC Permission Flow

```mermaid
sequenceDiagram
    participant User
    participant Controller
    participant AuthorizationService
    participant PermissionService
    participant FeatureService
    participant RoleService
    participant Database
    
    User->>Controller: Access Feature
    Controller->>AuthorizationService: CheckPermission()
    AuthorizationService->>PermissionService: GetUserPermissions()
    PermissionService->>Database: Query UserRoleAssignments
    Database-->>PermissionService: User Roles
    PermissionService->>Database: Query RolePermissions
    Database-->>PermissionService: Role Permissions
    PermissionService->>Database: Query FeaturePermissions
    Database-->>PermissionService: Feature Permissions
    PermissionService-->>AuthorizationService: User Permissions
    AuthorizationService->>FeatureService: GetFeaturePermissions()
    FeatureService->>Database: Query FeaturePermissions
    Database-->>FeatureService: Feature Permissions
    FeatureService-->>AuthorizationService: Required Permissions
    AuthorizationService->>AuthorizationService: CheckPermissionMatch()
    AuthorizationService-->>Controller: Allow/Deny
    Controller-->>User: Access Granted/Denied
```

---

## 3. Feature → Permission → Role Mapping

```mermaid
graph LR
    subgraph "Features"
        DashboardFeature[Dashboard Feature]
        RiskFeature[Risk Feature]
        ControlFeature[Control Feature]
        EvidenceFeature[Evidence Feature]
        AssessmentFeature[Assessment Feature]
        AuditFeature[Audit Feature]
        PolicyFeature[Policy Feature]
        WorkflowFeature[Workflow Feature]
    end
    
    subgraph "Permissions"
        DashboardView[Grc.Dashboard]
        RiskView[Grc.Risks.View]
        RiskManage[Grc.Risks.Manage]
        ControlView[Grc.Controls.View]
        ControlManage[Grc.Controls.Manage]
        EvidenceView[Grc.Evidence.View]
        EvidenceUpload[Grc.Evidence.Upload]
        EvidenceApprove[Grc.Evidence.Approve]
        AssessmentView[Grc.Assessments.View]
        AssessmentCreate[Grc.Assessments.Create]
        AuditView[Grc.Audits.View]
        AuditManage[Grc.Audits.Manage]
        PolicyView[Grc.Policies.View]
        PolicyManage[Grc.Policies.Manage]
        WorkflowView[Grc.Workflow.View]
        WorkflowManage[Grc.Workflow.Manage]
    end
    
    subgraph "Roles"
        SuperAdmin[SuperAdmin]
        ComplianceManager[ComplianceManager]
        RiskManager[RiskManager]
        Auditor[Auditor]
        EvidenceOfficer[EvidenceOfficer]
        Viewer[Viewer]
    end
    
    DashboardFeature --> DashboardView
    RiskFeature --> RiskView
    RiskFeature --> RiskManage
    ControlFeature --> ControlView
    ControlFeature --> ControlManage
    EvidenceFeature --> EvidenceView
    EvidenceFeature --> EvidenceUpload
    EvidenceFeature --> EvidenceApprove
    AssessmentFeature --> AssessmentView
    AssessmentFeature --> AssessmentCreate
    AuditFeature --> AuditView
    AuditFeature --> AuditManage
    PolicyFeature --> PolicyView
    PolicyFeature --> PolicyManage
    WorkflowFeature --> WorkflowView
    WorkflowFeature --> WorkflowManage
    
    DashboardView --> SuperAdmin
    DashboardView --> ComplianceManager
    DashboardView --> RiskManager
    DashboardView --> Auditor
    DashboardView --> Viewer
    
    RiskView --> SuperAdmin
    RiskView --> ComplianceManager
    RiskView --> RiskManager
    RiskView --> Viewer
    RiskManage --> SuperAdmin
    RiskManage --> ComplianceManager
    RiskManage --> RiskManager
    
    ControlView --> SuperAdmin
    ControlView --> ComplianceManager
    ControlView --> Viewer
    ControlManage --> SuperAdmin
    ControlManage --> ComplianceManager
    
    EvidenceView --> SuperAdmin
    EvidenceView --> ComplianceManager
    EvidenceView --> Auditor
    EvidenceView --> EvidenceOfficer
    EvidenceView --> Viewer
    EvidenceUpload --> SuperAdmin
    EvidenceUpload --> ComplianceManager
    EvidenceUpload --> EvidenceOfficer
    EvidenceApprove --> SuperAdmin
    EvidenceApprove --> ComplianceManager
    
    AssessmentView --> SuperAdmin
    AssessmentView --> ComplianceManager
    AssessmentView --> Auditor
    AssessmentView --> Viewer
    AssessmentCreate --> SuperAdmin
    AssessmentCreate --> ComplianceManager
    
    AuditView --> SuperAdmin
    AuditView --> ComplianceManager
    AuditView --> Auditor
    AuditView --> Viewer
    AuditManage --> SuperAdmin
    AuditManage --> ComplianceManager
    AuditManage --> Auditor
    
    PolicyView --> SuperAdmin
    PolicyView --> ComplianceManager
    PolicyView --> Viewer
    PolicyManage --> SuperAdmin
    PolicyManage --> ComplianceManager
    
    WorkflowView --> SuperAdmin
    WorkflowView --> ComplianceManager
    WorkflowView --> Viewer
    WorkflowManage --> SuperAdmin
    WorkflowManage --> ComplianceManager
```

---

## 4. Menu Visibility Based on Permissions

```mermaid
flowchart TD
    Start[User Accesses Application] --> GetUser[Get Current User]
    GetUser --> GetRoles[Get User Roles]
    GetRoles --> GetRoleFeatures[Get Role Features]
    GetRoleFeatures --> GetFeatures[Get Accessible Features]
    GetFeatures --> BuildMenu[Build Menu Items]
    
    BuildMenu --> CheckHome{Home Feature?}
    CheckHome -->|Yes| AddHome[Add Home Menu Item]
    CheckHome -->|No| SkipHome[Skip Home]
    
    AddHome --> CheckDashboard{Dashboard Feature?}
    SkipHome --> CheckDashboard
    
    CheckDashboard -->|Yes| AddDashboard[Add Dashboard Menu Item]
    CheckDashboard -->|No| SkipDashboard[Skip Dashboard]
    
    AddDashboard --> CheckRisk{Risk Feature?}
    SkipDashboard --> CheckRisk
    
    CheckRisk -->|Yes| CheckRiskPermission{Has Risk.View?}
    CheckRiskPermission -->|Yes| AddRisk[Add Risk Menu Item]
    CheckRiskPermission -->|No| SkipRisk[Skip Risk]
    CheckRisk -->|No| SkipRisk
    
    AddRisk --> CheckControl{Control Feature?}
    SkipRisk --> CheckControl
    
    CheckControl -->|Yes| CheckControlPermission{Has Control.View?}
    CheckControlPermission -->|Yes| AddControl[Add Control Menu Item]
    CheckControlPermission -->|No| SkipControl[Skip Control]
    CheckControl -->|No| SkipControl
    
    AddControl --> Continue[Continue for All Features]
    SkipControl --> Continue
    
    Continue --> RenderMenu[Render Menu]
    RenderMenu --> End[User Sees Menu]
```

---

## 5. Tenant Isolation Architecture

```mermaid
graph TB
    subgraph "Request Layer"
        HTTPRequest[HTTP Request]
        TenantMiddleware[TenantResolutionMiddleware]
    end
    
    subgraph "Context Services"
        TenantContext[TenantContextService]
        WorkspaceContext[WorkspaceContextService]
    end
    
    subgraph "Data Access"
        DbContext[GrcDbContext]
        GlobalFilter[Global Query Filter]
        TenantFilter[TenantId Filter]
        WorkspaceFilter[WorkspaceId Filter]
    end
    
    subgraph "Database"
        TenantData[(Tenant Data)]
        WorkspaceData[(Workspace Data)]
    end
    
    HTTPRequest --> TenantMiddleware
    TenantMiddleware --> TenantContext
    TenantContext --> WorkspaceContext
    
    TenantContext --> DbContext
    WorkspaceContext --> DbContext
    
    DbContext --> GlobalFilter
    GlobalFilter --> TenantFilter
    GlobalFilter --> WorkspaceFilter
    
    TenantFilter --> TenantData
    WorkspaceFilter --> WorkspaceData
```

---

## 6. Permission Evaluation Flow

```mermaid
flowchart LR
    Start[User Action] --> GetUser[Get User]
    GetUser --> GetTenant[Get Tenant Context]
    GetTenant --> GetWorkspace[Get Workspace Context]
    GetWorkspace --> GetRoles[Get User Roles]
    GetRoles --> GetRolePermissions[Get Role Permissions]
    GetRolePermissions --> GetFeaturePermissions[Get Feature Permissions]
    GetFeaturePermissions --> CheckPermission{Has Permission?}
    CheckPermission -->|Yes| CheckWorkspace{Workspace Context Valid?}
    CheckPermission -->|No| Deny[Access Denied]
    CheckWorkspace -->|Yes| CheckTenant{Tenant Context Valid?}
    CheckWorkspace -->|No| Deny
    CheckTenant -->|Yes| Allow[Access Allowed]
    CheckTenant -->|No| Deny
    Allow --> Execute[Execute Action]
    Deny --> Error[403 Forbidden]
```

---

## 7. Role Assignment Flow

```mermaid
sequenceDiagram
    participant Admin
    participant AdminController
    participant RoleService
    participant UserRoleService
    participant PermissionService
    participant Database
    participant NotificationService
    
    Admin->>AdminController: Assign Role to User
    AdminController->>RoleService: ValidateRole()
    RoleService->>Database: Check Role Exists
    Database-->>RoleService: Role Valid
    RoleService-->>AdminController: Role Valid
    
    AdminController->>UserRoleService: AssignRoleToUser()
    UserRoleService->>Database: Create UserRoleAssignment
    Database-->>UserRoleService: Assignment Created
    
    UserRoleService->>PermissionService: GetRolePermissions()
    PermissionService->>Database: Query RolePermissions
    Database-->>PermissionService: Permissions
    PermissionService-->>UserRoleService: Permissions List
    
    UserRoleService->>Database: Update User Permissions Cache
    Database-->>UserRoleService: Cache Updated
    
    UserRoleService->>NotificationService: NotifyRoleAssignment()
    NotificationService-->>Admin: Notification Sent
    
    UserRoleService-->>AdminController: Assignment Complete
    AdminController-->>Admin: Success Response
```

---

## 8. Tenant Provisioning Flow

```mermaid
flowchart TD
    Start[Create Tenant] --> ValidateInput[Validate Input]
    ValidateInput --> CreateTenant[Create Tenant Entity]
    CreateTenant --> ProvisionDB{Provision Database?}
    
    ProvisionDB -->|Yes| GenerateConnection[Generate Connection String]
    GenerateConnection --> CreateDatabase[Create Database]
    CreateDatabase --> RunMigrations[Run Migrations]
    RunMigrations --> SeedData[Seed Initial Data]
    SeedData --> CreateAdmin[Create Tenant Admin]
    
    ProvisionDB -->|No| CreateAdmin
    
    CreateAdmin --> CreateWorkspace[Create Default Workspace]
    CreateWorkspace --> CreateTeam[Create Default Team]
    CreateTeam --> AssignAdmin[Assign Admin to Team]
    AssignAdmin --> ConfigureRBAC[Configure RBAC]
    ConfigureRBAC --> GrantPermissions[Grant Permissions]
    GrantPermissions --> SendCredentials[Send Credentials]
    SendCredentials --> Complete[Tenant Ready]
```

---

## 9. Workspace Scoping Flow

```mermaid
sequenceDiagram
    participant User
    participant WorkspaceSwitcher
    participant WorkspaceService
    participant WorkspaceContext
    participant DbContext
    participant Database
    
    User->>WorkspaceSwitcher: Select Workspace
    WorkspaceSwitcher->>WorkspaceService: SwitchWorkspace()
    WorkspaceService->>WorkspaceContext: SetWorkspaceId()
    WorkspaceContext->>WorkspaceContext: Store in HttpContext
    
    User->>DbContext: Query Data
    DbContext->>WorkspaceContext: GetCurrentWorkspaceId()
    WorkspaceContext-->>DbContext: WorkspaceId
    DbContext->>DbContext: Apply Workspace Filter
    DbContext->>Database: SELECT ... WHERE WorkspaceId = @id
    Database-->>DbContext: Filtered Results
    DbContext-->>User: Workspace-Scoped Data
```

---

## 10. RBAC Entity Relationships

```mermaid
erDiagram
    Feature ||--o{ FeaturePermission : "has_permissions"
    Permission ||--o{ FeaturePermission : "granted_to"
    Permission ||--o{ RolePermission : "granted_to"
    Role ||--o{ RolePermission : "has_permissions"
    Role ||--o{ RoleFeature : "has_features"
    Feature ||--o{ RoleFeature : "accessible_by"
    Role ||--o{ UserRoleAssignment : "assigned_to"
    User ||--o{ UserRoleAssignment : "has_roles"
    Tenant ||--o{ TenantRoleConfiguration : "has_config"
    TenantRoleConfiguration ||--o{ Role : "configures"
```

---

## 11. Complete Access Control Flow

```mermaid
flowchart TB
    Start[User Request] --> ExtractTenant[Extract Tenant from Request]
    ExtractTenant --> ValidateTenant{Valid Tenant?}
    ValidateTenant -->|No| TenantError[Tenant Error]
    ValidateTenant -->|Yes| SetTenantContext[Set Tenant Context]
    
    SetTenantContext --> ExtractWorkspace[Extract Workspace from Request]
    ExtractWorkspace --> ValidateWorkspace{Valid Workspace?}
    ValidateWorkspace -->|No| UseDefaultWorkspace[Use Default Workspace]
    ValidateWorkspace -->|Yes| SetWorkspaceContext[Set Workspace Context]
    UseDefaultWorkspace --> SetWorkspaceContext
    
    SetWorkspaceContext --> GetUserRoles[Get User Roles]
    GetUserRoles --> GetRoleFeatures[Get Role Features]
    GetRoleFeatures --> GetRequiredPermission[Get Required Permission]
    GetRequiredPermission --> CheckPermission{Has Permission?}
    
    CheckPermission -->|No| AccessDenied[403 Access Denied]
    CheckPermission -->|Yes| CheckPolicy{Policy Check?}
    
    CheckPolicy -->|No| AllowAccess[Allow Access]
    CheckPolicy -->|Yes| EvaluatePolicy[Evaluate Policy]
    EvaluatePolicy --> PolicyDecision{Policy Decision}
    
    PolicyDecision -->|Allow| AllowAccess
    PolicyDecision -->|Deny| PolicyViolation[Policy Violation]
    
    AllowAccess --> ExecuteAction[Execute Action]
    ExecuteAction --> ApplyTenantFilter[Apply Tenant Filter]
    ApplyTenantFilter --> ApplyWorkspaceFilter[Apply Workspace Filter]
    ApplyWorkspaceFilter --> QueryDatabase[Query Database]
    QueryDatabase --> ReturnResults[Return Results]
    
    AccessDenied --> End[End]
    PolicyViolation --> End
    ReturnResults --> End
```

---

## 12. Multi-Tenant Data Isolation

```mermaid
graph TB
    subgraph "Tenant A"
        TenantA[Tenant A]
        WorkspaceA1[Workspace A1]
        WorkspaceA2[Workspace A2]
        DataA1[(Tenant A Data)]
        DataA2[(Workspace A1 Data)]
        DataA3[(Workspace A2 Data)]
    end
    
    subgraph "Tenant B"
        TenantB[Tenant B]
        WorkspaceB1[Workspace B1]
        DataB1[(Tenant B Data)]
        DataB2[(Workspace B1 Data)]
    end
    
    subgraph "Shared Auth"
        AuthDB[(GrcAuthDb<br/>Shared)]
        Users[Users]
        Roles[Roles]
    end
    
    TenantA --> WorkspaceA1
    TenantA --> WorkspaceA2
    WorkspaceA1 --> DataA2
    WorkspaceA2 --> DataA3
    TenantA --> DataA1
    
    TenantB --> WorkspaceB1
    WorkspaceB1 --> DataB2
    TenantB --> DataB1
    
    TenantA --> AuthDB
    TenantB --> AuthDB
    AuthDB --> Users
    AuthDB --> Roles
    
    DataA1 -.->|"Isolated"| DataB1
    DataA2 -.->|"Isolated"| DataB2
```

---

**Last Updated:** 2025-01-07  
**Next:** See [DIAGRAMS_SHAHIN_AI.md](./DIAGRAMS_SHAHIN_AI.md) for Shahin AI platform architecture
