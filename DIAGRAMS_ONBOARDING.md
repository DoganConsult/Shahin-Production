# Onboarding System - Feature Connection Diagrams
## 12-Step Wizard Flow and Provisioning Connections

**Generated:** 2025-01-07  
**Focus:** Onboarding wizard and provisioning flows  

---

## 1. 12-Step Onboarding Wizard Flow

```mermaid
flowchart TD
    Start[Start Onboarding] --> StepA[Step A: Organization Identity]
    StepA --> StepB[Step B: Regulatory Landscape]
    StepB --> StepC[Step C: Framework Selection]
    StepC --> StepD[Step D: Scope Definition]
    StepD --> StepE[Step E: Asset Recognition]
    StepE --> StepF[Step F: Control Baseline]
    StepF --> StepG[Step G: Governance Structure]
    StepG --> StepH[Step H: Team Setup]
    StepH --> StepI[Step I: Workflow Configuration]
    StepI --> StepJ[Step J: Evidence Standards]
    StepJ --> StepK[Step K: Integration Setup]
    StepK --> StepL[Step L: Review & Provisioning]
    StepL --> Provision[Provision Tenant]
    Provision --> Complete[Onboarding Complete]
```

---

## 2. Step Dependencies

```mermaid
graph LR
    StepA[Step A<br/>Organization Identity] --> StepB[Step B<br/>Regulatory Landscape]
    StepB --> StepC[Step C<br/>Framework Selection]
    StepC --> StepD[Step D<br/>Scope Definition]
    StepD --> StepE[Step E<br/>Asset Recognition]
    StepE --> StepF[Step F<br/>Control Baseline]
    StepF --> StepG[Step G<br/>Governance Structure]
    StepG --> StepH[Step H<br/>Team Setup]
    StepH --> StepI[Step I<br/>Workflow Configuration]
    StepI --> StepJ[Step J<br/>Evidence Standards]
    StepJ --> StepK[Step K<br/>Integration Setup]
    StepK --> StepL[Step L<br/>Review & Provisioning]
    
    StepA -.->|"Provides Tenant Info"| StepH
    StepB -.->|"Provides Regulators"| StepC
    StepC -.->|"Provides Frameworks"| StepF
    StepD -.->|"Provides Scope"| StepE
    StepE -.->|"Provides Assets"| StepF
    StepF -.->|"Provides Controls"| StepJ
    StepG -.->|"Provides Governance"| StepI
    StepH -.->|"Provides Teams"| StepI
```

---

## 3. Provisioning Flow

```mermaid
sequenceDiagram
    participant User
    participant OnboardingWizard
    participant ProvisioningService
    participant TenantService
    participant WorkspaceService
    participant TeamService
    participant RBACService
    participant Database
    participant NotificationService
    
    User->>OnboardingWizard: Complete Step L
    OnboardingWizard->>ProvisioningService: ProvisionTenant()
    ProvisioningService->>TenantService: CreateTenant()
    TenantService->>Database: Create Tenant Entity
    Database-->>TenantService: Tenant Created
    
    ProvisioningService->>Database: Create Database
    Database-->>ProvisioningService: Database Created
    ProvisioningService->>Database: Run Migrations
    Database-->>ProvisioningService: Migrations Complete
    
    ProvisioningService->>WorkspaceService: CreateDefaultWorkspace()
    WorkspaceService->>Database: Create Workspace
    Database-->>WorkspaceService: Workspace Created
    
    ProvisioningService->>TeamService: CreateDefaultTeam()
    TeamService->>Database: Create Team
    Database-->>TeamService: Team Created
    
    ProvisioningService->>RBACService: ConfigureRBAC()
    RBACService->>Database: Create Roles
    RBACService->>Database: Grant Permissions
    Database-->>RBACService: RBAC Configured
    
    ProvisioningService->>NotificationService: SendCredentials()
    NotificationService-->>User: Credentials Email
    
    ProvisioningService-->>OnboardingWizard: Provisioning Complete
    OnboardingWizard-->>User: Onboarding Complete
```

---

## 4. Team Setup Connections

```mermaid
flowchart TD
    Start[Step H: Team Setup] --> CreateTeam[Create Team]
    CreateTeam --> AddTeamMembers[Add Team Members]
    AddTeamMembers --> AssignRACI[Assign RACI Roles]
    AssignRACI --> SetTeamOwner[Set Team Owner]
    SetTeamOwner --> SetBackupOwner[Set Backup Owner]
    
    SetBackupOwner --> LinkToWorkspace[Link Team to Workspace]
    LinkToWorkspace --> AssignControls[Assign Controls to Team]
    AssignControls --> ConfigureWorkflows[Configure Team Workflows]
    ConfigureWorkflows --> CompleteTeam[Team Setup Complete]
    
    CreateTeam --> TeamEntity[Team Entity]
    AddTeamMembers --> TeamMemberEntity[TeamMember Entity]
    AssignRACI --> RACIAssignmentEntity[RACIAssignment Entity]
    LinkToWorkspace --> WorkspaceMembershipEntity[WorkspaceMembership Entity]
```

---

## 5. Workspace Creation Flow

```mermaid
graph TB
    subgraph "Workspace Creation"
        CreateWorkspace[Create Workspace]
        SetWorkspaceName[Set Workspace Name]
        SetWorkspaceType[Set Workspace Type]
        AssignWorkspaceMembers[Assign Workspace Members]
    end
    
    subgraph "Workspace Configuration"
        AssignControls[Assign Controls]
        ConfigureApprovalGates[Configure Approval Gates]
        SetApprovers[Set Approvers]
        ConfigureWorkflows[Configure Workflows]
    end
    
    subgraph "Workspace Entities"
        Workspace[Workspace Entity]
        WorkspaceMembership[WorkspaceMembership Entity]
        WorkspaceControl[WorkspaceControl Entity]
        WorkspaceApprovalGate[WorkspaceApprovalGate Entity]
        WorkspaceApprovalGateApprover[WorkspaceApprovalGateApprover Entity]
    end
    
    CreateWorkspace --> SetWorkspaceName
    SetWorkspaceName --> SetWorkspaceType
    SetWorkspaceType --> AssignWorkspaceMembers
    AssignWorkspaceMembers --> AssignControls
    AssignControls --> ConfigureApprovalGates
    ConfigureApprovalGates --> SetApprovers
    SetApprovers --> ConfigureWorkflows
    
    CreateWorkspace --> Workspace
    AssignWorkspaceMembers --> WorkspaceMembership
    AssignControls --> WorkspaceControl
    ConfigureApprovalGates --> WorkspaceApprovalGate
    SetApprovers --> WorkspaceApprovalGateApprover
```

---

## 6. Complete Onboarding Sequence

```mermaid
sequenceDiagram
    participant Owner
    participant OnboardingWizard
    participant RulesEngine
    participant TenantService
    participant WorkspaceService
    participant TeamService
    participant WorkflowService
    participant RBACService
    participant Database
    participant NotificationService
    
    Owner->>OnboardingWizard: Start Onboarding
    OnboardingWizard->>OnboardingWizard: Step A: Organization Identity
    OnboardingWizard->>Database: Save Step A Data
    
    OnboardingWizard->>OnboardingWizard: Step B: Regulatory Landscape
    OnboardingWizard->>Database: Save Step B Data
    
    OnboardingWizard->>OnboardingWizard: Step C: Framework Selection
    OnboardingWizard->>Database: Save Step C Data
    
    OnboardingWizard->>OnboardingWizard: Step D: Scope Definition
    OnboardingWizard->>Database: Save Step D Data
    
    OnboardingWizard->>OnboardingWizard: Step E: Asset Recognition
    OnboardingWizard->>Database: Save Step E Data
    
    OnboardingWizard->>RulesEngine: Derive Applicable Controls
    RulesEngine->>Database: Query Controls
    Database-->>RulesEngine: Controls
    RulesEngine-->>OnboardingWizard: Applicable Controls
    
    OnboardingWizard->>OnboardingWizard: Step F: Control Baseline
    OnboardingWizard->>Database: Save Step F Data
    
    OnboardingWizard->>OnboardingWizard: Step G: Governance Structure
    OnboardingWizard->>Database: Save Step G Data
    
    OnboardingWizard->>OnboardingWizard: Step H: Team Setup
    OnboardingWizard->>TeamService: Create Teams
    TeamService->>Database: Create Team Entities
    Database-->>TeamService: Teams Created
    TeamService-->>OnboardingWizard: Teams Ready
    
    OnboardingWizard->>OnboardingWizard: Step I: Workflow Configuration
    OnboardingWizard->>WorkflowService: Configure Workflows
    WorkflowService->>Database: Save Workflow Configs
    Database-->>WorkflowService: Configs Saved
    WorkflowService-->>OnboardingWizard: Workflows Configured
    
    OnboardingWizard->>OnboardingWizard: Step J: Evidence Standards
    OnboardingWizard->>Database: Save Step J Data
    
    OnboardingWizard->>OnboardingWizard: Step K: Integration Setup
    OnboardingWizard->>Database: Save Step K Data
    
    OnboardingWizard->>OnboardingWizard: Step L: Review & Provisioning
    OnboardingWizard->>TenantService: Provision Tenant
    TenantService->>Database: Create Tenant
    TenantService->>WorkspaceService: Create Workspace
    WorkspaceService->>Database: Create Workspace
    TenantService->>RBACService: Configure RBAC
    RBACService->>Database: Setup RBAC
    TenantService-->>OnboardingWizard: Tenant Provisioned
    
    OnboardingWizard->>NotificationService: Send Credentials
    NotificationService-->>Owner: Credentials Email
    
    OnboardingWizard-->>Owner: Onboarding Complete
```

---

## 7. Onboarding Data Flow

```mermaid
flowchart TB
    Start[Owner Creates Account] --> OwnerSetup[Owner Setup]
    OwnerSetup --> CreateTenant[Create Tenant Record]
    CreateTenant --> StartWizard[Start Onboarding Wizard]
    
    StartWizard --> CollectData[Collect 96+ Questions]
    CollectData --> StepA[Step A Data]
    CollectData --> StepB[Step B Data]
    CollectData --> StepC[Step C Data]
    CollectData --> StepD[Step D Data]
    CollectData --> StepE[Step E Data]
    CollectData --> StepF[Step F Data]
    CollectData --> StepG[Step G Data]
    CollectData --> StepH[Step H Data]
    CollectData --> StepI[Step I Data]
    CollectData --> StepJ[Step J Data]
    CollectData --> StepK[Step K Data]
    CollectData --> StepL[Step L Data]
    
    StepA --> OnboardingWizard[OnboardingWizard Entity]
    StepB --> OnboardingWizard
    StepC --> OnboardingWizard
    StepD --> OnboardingWizard
    StepE --> OnboardingWizard
    StepF --> OnboardingWizard
    StepG --> OnboardingWizard
    StepH --> OnboardingWizard
    StepI --> OnboardingWizard
    StepJ --> OnboardingWizard
    StepK --> OnboardingWizard
    StepL --> OnboardingWizard
    
    OnboardingWizard --> Provisioning[Provisioning Service]
    Provisioning --> Tenant[Tenant Entity]
    Provisioning --> Workspace[Workspace Entity]
    Provisioning --> Team[Team Entity]
    Provisioning --> RBAC[RBAC Configuration]
    Provisioning --> Workflow[Workflow Configuration]
```

---

## 8. Provisioning Architecture

```mermaid
graph TB
    subgraph "Onboarding Wizard"
        WizardController[OnboardingWizardController]
        WizardService[OnboardingWizardService]
        WizardEntity[OnboardingWizard Entity]
    end
    
    subgraph "Provisioning Services"
        ProvisioningService[OnboardingProvisioningService]
        TenantProvisioner[TenantOnboardingProvisioner]
        TenantService[TenantService]
        WorkspaceService[WorkspaceService]
        TeamService[TeamService]
    end
    
    subgraph "Configuration Services"
        WorkflowService[WorkflowService]
        RBACService[RBACService]
        RulesEngine[RulesEngineService]
    end
    
    subgraph "Database Provisioning"
        DatabaseResolver[TenantDatabaseResolver]
        DbContextFactory[TenantAwareDbContextFactory]
        Migrations[EF Migrations]
    end
    
    WizardController --> WizardService
    WizardService --> WizardEntity
    WizardService --> ProvisioningService
    
    ProvisioningService --> TenantProvisioner
    TenantProvisioner --> TenantService
    TenantProvisioner --> WorkspaceService
    TenantProvisioner --> TeamService
    TenantProvisioner --> DatabaseResolver
    
    DatabaseResolver --> DbContextFactory
    DbContextFactory --> Migrations
    
    ProvisioningService --> WorkflowService
    ProvisioningService --> RBACService
    ProvisioningService --> RulesEngine
```

---

## 9. Team and RACI Setup Flow

```mermaid
flowchart LR
    CreateTeam[Create Team] --> AddMembers[Add Team Members]
    AddMembers --> AssignRACI[Assign RACI Roles]
    AssignRACI --> Responsible[Responsible]
    AssignRACI --> Accountable[Accountable]
    AssignRACI --> Consulted[Consulted]
    AssignRACI --> Informed[Informed]
    
    Responsible --> TeamMember[TeamMember Entity]
    Accountable --> TeamMember
    Consulted --> TeamMember
    Informed --> TeamMember
    
    TeamMember --> RACIAssignment[RACIAssignment Entity]
    RACIAssignment --> WorkflowRouting[Workflow Routing]
    WorkflowRouting --> TaskAssignment[Task Assignment]
```

---

## 10. Onboarding Entity Relationships

```mermaid
erDiagram
    OnboardingWizard ||--|| Tenant : "creates"
    OnboardingWizard ||--o{ Workspace : "provisions"
    OnboardingWizard ||--o{ Team : "creates"
    
    Tenant ||--o{ Workspace : "contains"
    Tenant ||--o{ TenantUser : "has_users"
    Tenant ||--o{ TenantBaseline : "has_baseline"
    Tenant ||--o{ TenantPackage : "has_package"
    Tenant ||--o{ TenantTemplate : "uses_template"
    Tenant ||--o{ TenantWorkflowConfig : "has_workflow_config"
    
    Workspace ||--o{ WorkspaceMembership : "has_members"
    Workspace ||--o{ WorkspaceControl : "has_controls"
    Workspace ||--o{ WorkspaceApprovalGate : "has_approval_gates"
    
    Team ||--o{ TeamMember : "has_members"
    Team ||--o{ RACIAssignment : "has_raci"
    
    Tenant ||--o{ OrganizationProfile : "has_profile"
    OrganizationProfile ||--o{ OnboardingWizard : "configured_by"
```

---

## 11. Smart Onboarding Flow

```mermaid
flowchart TD
    Start[Start Smart Onboarding] --> AnalyzeInput[Analyze Input Data]
    AnalyzeInput --> DetectIndustry[Detect Industry]
    DetectIndustry --> SuggestFrameworks[Suggest Frameworks]
    SuggestFrameworks --> SuggestControls[Suggest Controls]
    SuggestControls --> SuggestTeams[Suggest Teams]
    SuggestTeams --> SuggestWorkflows[Suggest Workflows]
    
    SuggestFrameworks --> UserReview[User Reviews Suggestions]
    SuggestControls --> UserReview
    SuggestTeams --> UserReview
    SuggestWorkflows --> UserReview
    
    UserReview --> AcceptSuggestions{Accept Suggestions?}
    AcceptSuggestions -->|Yes| AutoConfigure[Auto-Configure]
    AcceptSuggestions -->|No| ManualConfigure[Manual Configuration]
    
    AutoConfigure --> Provision[Provision Tenant]
    ManualConfigure --> Provision
    Provision --> Complete[Onboarding Complete]
```

---

## 12. Onboarding to Production Flow

```mermaid
sequenceDiagram
    participant Owner
    participant OnboardingWizard
    participant ProvisioningService
    participant TenantService
    participant Database
    participant RBACService
    participant NotificationService
    participant User
    
    Owner->>OnboardingWizard: Complete All 12 Steps
    OnboardingWizard->>ProvisioningService: Trigger Provisioning
    ProvisioningService->>TenantService: Create Tenant
    TenantService->>Database: Create Tenant Entity
    Database-->>TenantService: Tenant Created
    
    ProvisioningService->>Database: Create Database
    Database-->>ProvisioningService: Database Ready
    ProvisioningService->>Database: Run Migrations
    Database-->>ProvisioningService: Migrations Complete
    
    ProvisioningService->>TenantService: Create Workspace
    TenantService->>Database: Create Workspace
    Database-->>TenantService: Workspace Created
    
    ProvisioningService->>TenantService: Create Teams
    TenantService->>Database: Create Teams
    Database-->>TenantService: Teams Created
    
    ProvisioningService->>RBACService: Configure RBAC
    RBACService->>Database: Create Roles & Permissions
    Database-->>RBACService: RBAC Configured
    
    ProvisioningService->>TenantService: Create Tenant Admin
    TenantService->>Database: Create Admin User
    Database-->>TenantService: Admin Created
    
    ProvisioningService->>NotificationService: Send Credentials
    NotificationService-->>Owner: Credentials Email
    
    Owner->>User: Share Credentials
    User->>OnboardingWizard: Login
    OnboardingWizard-->>User: Access Granted
```

---

**Last Updated:** 2025-01-07  
**Next:** See [DIAGRAMS_DATA_FLOWS.md](./DIAGRAMS_DATA_FLOWS.md) for data flow patterns
