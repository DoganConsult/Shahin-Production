# Core GRC Domain - Feature Connection Diagrams
## Risk, Control, Evidence, Assessment, Audit, Policy Connections

**Generated:** 2025-01-07  
**Focus:** Core GRC domain module interactions and data flows  

---

## 1. Core GRC Module Architecture

```mermaid
graph TB
    subgraph "Risk Management"
        Risk[Risk Entity]
        RiskService[RiskService]
        RiskController[RiskController]
        RiskWorkflow[RiskWorkflowService]
    end
    
    subgraph "Control Management"
        Control[Control Entity]
        ControlService[ControlService]
        ControlController[ControlController]
        FrameworkControl[FrameworkControl]
    end
    
    subgraph "Evidence Management"
        Evidence[Evidence Entity]
        EvidenceService[EvidenceService]
        EvidenceController[EvidenceController]
        EvidenceWorkflow[EvidenceWorkflowService]
        EvidencePack[EvidencePack]
    end
    
    subgraph "Assessment Management"
        Assessment[Assessment Entity]
        AssessmentService[AssessmentService]
        AssessmentController[AssessmentController]
        AssessmentRequirement[AssessmentRequirement]
    end
    
    subgraph "Audit Management"
        Audit[Audit Entity]
        AuditService[AuditService]
        AuditController[AuditController]
        AuditFinding[AuditFinding]
    end
    
    subgraph "Policy Management"
        Policy[Policy Entity]
        PolicyService[PolicyService]
        PolicyController[PolicyController]
        PolicyViolation[PolicyViolation]
        PolicyEngine[Policy Engine]
    end
    
    RiskController --> RiskService
    ControlController --> ControlService
    EvidenceController --> EvidenceService
    AssessmentController --> AssessmentService
    AuditController --> AuditService
    PolicyController --> PolicyService
    
    RiskService --> Risk
    ControlService --> Control
    EvidenceService --> Evidence
    AssessmentService --> Assessment
    AuditService --> Audit
    PolicyService --> Policy
    
    RiskService --> RiskWorkflow
    EvidenceService --> EvidenceWorkflow
    
    Risk -->|"Mitigated by"| Control
    Control -->|"Requires"| Evidence
    Control -->|"Tested by"| Assessment
    Assessment -->|"Uses"| Evidence
    Assessment -->|"Creates"| Audit
    Audit -->|"Finds"| Risk
    Audit -->|"Reviews"| Control
    Audit -->|"Validates"| Evidence
    Audit -->|"Creates"| AuditFinding
    
    PolicyEngine --> Evidence
    PolicyEngine --> Assessment
    PolicyEngine --> Control
    PolicyEngine --> Policy
    
    Policy -->|"Enforces"| PolicyViolation
    PolicyViolation -->|"Triggers"| RiskWorkflow
    
    Control --> FrameworkControl
    Evidence --> EvidencePack
    Assessment --> AssessmentRequirement
```

---

## 2. Risk → Control → Evidence Flow

```mermaid
sequenceDiagram
    participant User
    participant RiskController
    participant RiskService
    participant ControlService
    participant EvidenceService
    participant WorkflowService
    participant NotificationService
    participant Database
    
    User->>RiskController: Create Risk
    RiskController->>RiskService: CreateRiskAsync()
    RiskService->>Database: Save Risk
    RiskService->>ControlService: FindMitigatingControls()
    ControlService->>Database: Query Controls
    ControlService-->>RiskService: Return Controls
    RiskService->>WorkflowService: CreateRiskWorkflow()
    WorkflowService->>Database: Create WorkflowInstance
    WorkflowService->>NotificationService: NotifyRiskOwner()
    NotificationService-->>User: Email/Slack Notification
    
    User->>ControlController: Assign Evidence to Control
    ControlController->>EvidenceService: CreateEvidenceAsync()
    EvidenceService->>Database: Save Evidence
    EvidenceService->>WorkflowService: CreateEvidenceWorkflow()
    WorkflowService->>Database: Create WorkflowTask
    WorkflowService->>NotificationService: NotifyReviewer()
    NotificationService-->>User: Notification
    
    User->>EvidenceController: Submit Evidence
    EvidenceController->>EvidenceService: SubmitEvidenceAsync()
    EvidenceService->>WorkflowService: TransitionToReview()
    WorkflowService->>Database: Update Task Status
    WorkflowService->>NotificationService: NotifyApprover()
    
    User->>EvidenceController: Approve Evidence
    EvidenceController->>EvidenceService: ApproveEvidenceAsync()
    EvidenceService->>Database: Update Evidence Status
    EvidenceService->>ControlService: LinkEvidenceToControl()
    ControlService->>Database: Update Control Evidence Link
    ControlService->>RiskService: UpdateRiskMitigationStatus()
    RiskService->>Database: Update Risk Status
```

---

## 3. Assessment → Control → Evidence Flow

```mermaid
flowchart TD
    Start[Create Assessment] --> SelectControl[Select Control to Assess]
    SelectControl --> LoadControl[Load Control Details]
    LoadControl --> CheckEvidence{Evidence Available?}
    
    CheckEvidence -->|No| RequestEvidence[Request Evidence]
    CheckEvidence -->|Yes| LoadEvidence[Load Evidence]
    
    RequestEvidence --> CreateEvidenceTask[Create Evidence Task]
    CreateEvidenceTask --> Workflow[Workflow Engine]
    Workflow --> AssignTask[Assign to Evidence Officer]
    AssignTask --> Notification[Send Notification]
    
    LoadEvidence --> ReviewEvidence[Review Evidence]
    ReviewEvidence --> AssessControl[Assess Control Effectiveness]
    AssessControl --> RecordResults[Record Assessment Results]
    RecordResults --> CalculateScore[Calculate Score]
    CalculateScore --> UpdateControl[Update Control Status]
    UpdateControl --> CreateAudit{Create Audit?}
    
    CreateAudit -->|Yes| AuditCreation[Create Audit Record]
    CreateAudit -->|No| Complete[Assessment Complete]
    
    AuditCreation --> AuditFinding{Findings?}
    AuditFinding -->|Yes| CreateFinding[Create Audit Finding]
    AuditFinding -->|No| Complete
    
    CreateFinding --> ActionPlan{Action Plan Needed?}
    ActionPlan -->|Yes| CreateActionPlan[Create Action Plan]
    ActionPlan -->|No| Complete
    
    CreateActionPlan --> AssignOwner[Assign Action Plan Owner]
    AssignOwner --> Workflow
    Complete --> Report[Generate Report]
```

---

## 4. Audit → Finding → Action Plan Flow

```mermaid
stateDiagram-v2
    [*] --> AuditPlanned: Create Audit
    AuditPlanned --> AuditInProgress: Start Audit
    AuditInProgress --> EvidenceCollected: Collect Evidence
    EvidenceCollected --> FindingsIdentified: Review Evidence
    FindingsIdentified --> FindingsDocumented: Document Findings
    FindingsDocumented --> ActionPlanCreated: Create Action Plans
    ActionPlanCreated --> ActionPlanAssigned: Assign Owners
    ActionPlanAssigned --> ActionPlanInProgress: Start Remediation
    ActionPlanInProgress --> ActionPlanCompleted: Complete Actions
    ActionPlanCompleted --> AuditClosed: Close Audit
    AuditClosed --> [*]
    
    FindingsIdentified --> CriticalFinding: Critical Finding
    CriticalFinding --> ImmediateAction: Immediate Action Required
    ImmediateAction --> Escalation: Escalate to Management
    Escalation --> ActionPlanCreated
```

---

## 5. Policy → Violation → Workflow Flow

```mermaid
graph LR
    subgraph "Policy Enforcement"
        PolicyRule[Policy Rule<br/>YAML]
        PolicyEngine[Policy Engine]
        PolicyContext[Policy Context]
        PolicyDecision[Policy Decision]
    end
    
    subgraph "Violation Handling"
        PolicyViolation[Policy Violation]
        ViolationException[PolicyViolationException]
        ViolationLog[Violation Log]
    end
    
    subgraph "Workflow Trigger"
        WorkflowDefinition[Workflow Definition]
        WorkflowInstance[Workflow Instance]
        WorkflowTask[Workflow Task]
    end
    
    subgraph "Notification"
        NotificationService[Notification Service]
        Email[Email]
        Slack[Slack]
        Teams[Teams]
    end
    
    PolicyRule --> PolicyEngine
    PolicyContext --> PolicyEngine
    PolicyEngine --> PolicyDecision
    
    PolicyDecision -->|Deny| PolicyViolation
    PolicyViolation --> ViolationException
    ViolationException --> ViolationLog
    
    PolicyViolation --> WorkflowDefinition
    WorkflowDefinition --> WorkflowInstance
    WorkflowInstance --> WorkflowTask
    
    WorkflowTask --> NotificationService
    NotificationService --> Email
    NotificationService --> Slack
    NotificationService --> Teams
```

---

## 6. Complete GRC Data Flow

```mermaid
flowchart TB
    subgraph "Input Layer"
        UserInput[User Input]
        ExternalData[External System Data]
        FileUpload[File Uploads]
    end
    
    subgraph "Processing Layer"
        RiskProcessing[Risk Processing]
        ControlProcessing[Control Processing]
        EvidenceProcessing[Evidence Processing]
        AssessmentProcessing[Assessment Processing]
        AuditProcessing[Audit Processing]
        PolicyProcessing[Policy Processing]
    end
    
    subgraph "Workflow Layer"
        WorkflowOrchestration[Workflow Orchestration]
        TaskManagement[Task Management]
        ApprovalManagement[Approval Management]
    end
    
    subgraph "Storage Layer"
        RiskDB[(Risk Data)]
        ControlDB[(Control Data)]
        EvidenceDB[(Evidence Data)]
        AssessmentDB[(Assessment Data)]
        AuditDB[(Audit Data)]
        PolicyDB[(Policy Data)]
    end
    
    subgraph "Output Layer"
        Reports[Reports]
        Dashboards[Dashboards]
        Notifications[Notifications]
        Integrations[External Integrations]
    end
    
    UserInput --> RiskProcessing
    UserInput --> ControlProcessing
    UserInput --> EvidenceProcessing
    UserInput --> AssessmentProcessing
    UserInput --> AuditProcessing
    
    ExternalData --> RiskProcessing
    ExternalData --> ControlProcessing
    FileUpload --> EvidenceProcessing
    
    RiskProcessing --> RiskDB
    ControlProcessing --> ControlDB
    EvidenceProcessing --> EvidenceDB
    AssessmentProcessing --> AssessmentDB
    AuditProcessing --> AuditDB
    PolicyProcessing --> PolicyDB
    
    RiskProcessing --> WorkflowOrchestration
    ControlProcessing --> WorkflowOrchestration
    EvidenceProcessing --> WorkflowOrchestration
    AssessmentProcessing --> WorkflowOrchestration
    AuditProcessing --> WorkflowOrchestration
    
    PolicyProcessing --> RiskProcessing
    PolicyProcessing --> ControlProcessing
    PolicyProcessing --> EvidenceProcessing
    PolicyProcessing --> AssessmentProcessing
    
    WorkflowOrchestration --> TaskManagement
    TaskManagement --> ApprovalManagement
    ApprovalManagement --> Notifications
    
    RiskDB --> Reports
    ControlDB --> Reports
    EvidenceDB --> Reports
    AssessmentDB --> Reports
    AuditDB --> Reports
    PolicyDB --> Reports
    
    RiskDB --> Dashboards
    ControlDB --> Dashboards
    EvidenceDB --> Dashboards
    AssessmentDB --> Dashboards
    
    Reports --> Integrations
    Dashboards --> Integrations
    Notifications --> Integrations
```

---

## 7. Entity Relationships - Core GRC

```mermaid
erDiagram
    Risk ||--o{ Control : "mitigated_by"
    Risk ||--o{ RiskIndicator : "monitored_by"
    Risk ||--o{ RiskResilience : "has_resilience"
    
    Control ||--o{ Evidence : "requires"
    Control ||--o{ Assessment : "tested_by"
    Control ||--o{ FrameworkControl : "maps_to_framework"
    Control ||--o{ ControlException : "has_exceptions"
    
    Evidence ||--o{ EvidencePack : "grouped_in"
    Evidence ||--o{ Assessment : "used_by"
    Evidence ||--o{ Audit : "validated_by"
    Evidence ||--o{ AutoTaggedEvidence : "ai_tagged"
    
    Assessment ||--o{ AssessmentRequirement : "has_requirements"
    Assessment ||--o{ Evidence : "uses"
    Assessment ||--o{ Control : "tests"
    Assessment ||--o{ Audit : "creates"
    
    Audit ||--o{ AuditFinding : "has_findings"
    Audit ||--o{ Evidence : "validates"
    Audit ||--o{ Control : "reviews"
    Audit ||--o{ Risk : "identifies"
    Audit ||--o{ ActionPlan : "creates"
    
    Policy ||--o{ PolicyViolation : "has_violations"
    Policy ||--o{ PolicyDecision : "has_decisions"
    Policy ||--o{ Workflow : "triggers"
    
    ActionPlan ||--o{ Workflow : "managed_by"
    ActionPlan ||--o{ AuditFinding : "remediates"
    
    Workflow ||--o{ WorkflowTask : "contains"
    WorkflowTask ||--o{ ApprovalChain : "requires"
    ApprovalChain ||--o{ ApprovalInstance : "creates"
```

---

## 8. Service Interaction Sequence

```mermaid
sequenceDiagram
    participant Controller
    participant Service
    participant PolicyEngine
    participant WorkflowService
    participant UnitOfWork
    participant Repository
    participant Database
    participant NotificationService
    
    Controller->>Service: Action Request
    Service->>PolicyEngine: EnforcePolicy()
    PolicyEngine-->>Service: Allow/Deny
    
    alt Policy Allows
        Service->>UnitOfWork: BeginTransaction()
        UnitOfWork->>Repository: GetEntity()
        Repository->>Database: Query
        Database-->>Repository: Entity Data
        Repository-->>UnitOfWork: Entity
        UnitOfWork-->>Service: Entity
        
        Service->>Service: Business Logic
        Service->>UnitOfWork: UpdateEntity()
        UnitOfWork->>Repository: Save()
        Repository->>Database: Update
        Database-->>Repository: Success
        Repository-->>UnitOfWork: Success
        UnitOfWork-->>Service: Success
        
        Service->>WorkflowService: TriggerWorkflow()
        WorkflowService->>Database: Create Workflow
        WorkflowService->>NotificationService: SendNotification()
        NotificationService-->>User: Notification
        
        Service->>UnitOfWork: Commit()
        UnitOfWork->>Database: Commit Transaction
        Database-->>UnitOfWork: Committed
        UnitOfWork-->>Service: Committed
        Service-->>Controller: Success Response
        Controller-->>User: Success
    else Policy Denies
        PolicyEngine-->>Service: PolicyViolationException
        Service-->>Controller: Error Response
        Controller-->>User: Policy Violation Error
    end
```

---

## 9. Feature Dependency Matrix

```mermaid
graph TD
    subgraph "Foundation"
        MultiTenancy[Multi-Tenancy]
        RBAC[RBAC]
        PolicyEngine[Policy Engine]
    end
    
    subgraph "Core GRC"
        Risk[Risk Management]
        Control[Control Management]
        Evidence[Evidence Management]
        Assessment[Assessment Management]
        Audit[Audit Management]
        Policy[Policy Management]
    end
    
    subgraph "Supporting"
        Workflow[Workflow Engine]
        Notification[Notification System]
        Reporting[Reporting System]
    end
    
    MultiTenancy --> Risk
    MultiTenancy --> Control
    MultiTenancy --> Evidence
    MultiTenancy --> Assessment
    MultiTenancy --> Audit
    MultiTenancy --> Policy
    
    RBAC --> Risk
    RBAC --> Control
    RBAC --> Evidence
    RBAC --> Assessment
    RBAC --> Audit
    RBAC --> Policy
    
    PolicyEngine --> Evidence
    PolicyEngine --> Assessment
    PolicyEngine --> Control
    PolicyEngine --> Policy
    
    Risk --> Control
    Control --> Evidence
    Control --> Assessment
    Evidence --> Assessment
    Assessment --> Audit
    Audit --> Policy
    
    Risk --> Workflow
    Control --> Workflow
    Evidence --> Workflow
    Assessment --> Workflow
    Audit --> Workflow
    Policy --> Workflow
    
    Workflow --> Notification
    Risk --> Notification
    Control --> Notification
    Evidence --> Notification
    Assessment --> Notification
    Audit --> Notification
    
    Risk --> Reporting
    Control --> Reporting
    Evidence --> Reporting
    Assessment --> Reporting
    Audit --> Reporting
    Policy --> Reporting
```

---

## 10. Cross-Module Data Flow

```mermaid
flowchart LR
    subgraph "Risk Module"
        RiskCreate[Create Risk]
        RiskAssess[Assess Risk]
        RiskMitigate[Mitigate Risk]
    end
    
    subgraph "Control Module"
        ControlCreate[Create Control]
        ControlTest[Test Control]
        ControlMap[Map to Framework]
    end
    
    subgraph "Evidence Module"
        EvidenceUpload[Upload Evidence]
        EvidenceReview[Review Evidence]
        EvidenceApprove[Approve Evidence]
    end
    
    subgraph "Assessment Module"
        AssessmentCreate[Create Assessment]
        AssessmentExecute[Execute Assessment]
        AssessmentScore[Score Assessment]
    end
    
    subgraph "Audit Module"
        AuditPlan[Plan Audit]
        AuditExecute[Execute Audit]
        AuditReport[Report Findings]
    end
    
    RiskCreate --> ControlCreate
    RiskAssess --> ControlTest
    RiskMitigate --> EvidenceUpload
    
    ControlCreate --> EvidenceUpload
    ControlTest --> AssessmentCreate
    ControlMap --> AssessmentExecute
    
    EvidenceUpload --> EvidenceReview
    EvidenceReview --> EvidenceApprove
    EvidenceApprove --> ControlTest
    EvidenceApprove --> AssessmentExecute
    
    AssessmentCreate --> EvidenceUpload
    AssessmentExecute --> EvidenceReview
    AssessmentScore --> AuditPlan
    
    AuditPlan --> AssessmentExecute
    AuditExecute --> EvidenceReview
    AuditReport --> RiskAssess
    AuditReport --> ControlTest
```

---

**Last Updated:** 2025-01-07  
**Next:** See [DIAGRAMS_WORKFLOW_SYSTEM.md](./DIAGRAMS_WORKFLOW_SYSTEM.md) for workflow connections
