# Workflow System - Feature Connection Diagrams
## Workflow Engine, Task Routing, Approvals, and Escalations

**Generated:** 2025-01-07  
**Focus:** Workflow system architecture and connections  

---

## 1. Workflow Engine Architecture

```mermaid
graph TB
    subgraph "Workflow Types"
        RiskWorkflow[Risk Workflow]
        ControlWorkflow[Control Workflow]
        EvidenceWorkflow[Evidence Workflow]
        AssessmentWorkflow[Assessment Workflow]
        AuditWorkflow[Audit Workflow]
        PolicyWorkflow[Policy Workflow]
        ActionPlanWorkflow[Action Plan Workflow]
    end
    
    subgraph "Workflow Engine Core"
        WorkflowService[WorkflowService]
        WorkflowEngine[WorkflowEngineService]
        WorkflowRouting[WorkflowRoutingService]
        WorkflowAudit[WorkflowAuditService]
    end
    
    subgraph "Workflow Components"
        WorkflowDefinition[WorkflowDefinition]
        WorkflowInstance[WorkflowInstance]
        WorkflowTask[WorkflowTask]
        ApprovalChain[ApprovalChain]
        ApprovalInstance[ApprovalInstance]
    end
    
    subgraph "Supporting Services"
        EscalationService[EscalationService]
        NotificationService[NotificationService]
        TenantService[TenantService]
        WorkspaceService[WorkspaceService]
    end
    
    RiskWorkflow --> WorkflowService
    ControlWorkflow --> WorkflowService
    EvidenceWorkflow --> WorkflowService
    AssessmentWorkflow --> WorkflowService
    AuditWorkflow --> WorkflowService
    PolicyWorkflow --> WorkflowService
    ActionPlanWorkflow --> WorkflowService
    
    WorkflowService --> WorkflowEngine
    WorkflowService --> WorkflowRouting
    WorkflowService --> WorkflowAudit
    
    WorkflowEngine --> WorkflowDefinition
    WorkflowEngine --> WorkflowInstance
    WorkflowEngine --> WorkflowTask
    
    WorkflowTask --> ApprovalChain
    ApprovalChain --> ApprovalInstance
    
    WorkflowEngine --> EscalationService
    WorkflowEngine --> NotificationService
    WorkflowRouting --> TenantService
    WorkflowRouting --> WorkspaceService
```

---

## 2. Workflow Execution Flow

```mermaid
sequenceDiagram
    participant User
    participant Controller
    participant WorkflowService
    participant WorkflowEngine
    participant WorkflowRouting
    participant TaskAssignment
    participant ApprovalChain
    participant NotificationService
    participant Database
    
    User->>Controller: Trigger Workflow
    Controller->>WorkflowService: CreateWorkflowInstance()
    WorkflowService->>WorkflowEngine: InitializeWorkflow()
    WorkflowEngine->>Database: Load WorkflowDefinition
    WorkflowEngine->>Database: Create WorkflowInstance
    WorkflowEngine->>WorkflowRouting: DetermineAssignee()
    WorkflowRouting->>Database: Query Team/RACI
    Database-->>WorkflowRouting: Assignee Info
    WorkflowRouting-->>WorkflowEngine: Assignee
    WorkflowEngine->>Database: Create WorkflowTask
    WorkflowEngine->>ApprovalChain: CheckApprovalRequired()
    ApprovalChain->>Database: Load ApprovalChain
    ApprovalChain-->>WorkflowEngine: Approval Required
    WorkflowEngine->>Database: Create ApprovalInstance
    WorkflowEngine->>NotificationService: NotifyAssignee()
    NotificationService-->>User: Task Notification
    
    User->>Controller: Complete Task
    Controller->>WorkflowService: CompleteTask()
    WorkflowService->>WorkflowEngine: ProcessTaskCompletion()
    WorkflowEngine->>ApprovalChain: CheckApproval()
    ApprovalChain->>Database: Query Approval Status
    Database-->>ApprovalChain: Approval Status
    ApprovalChain-->>WorkflowEngine: Approved/Rejected
    
    alt Approved
        WorkflowEngine->>WorkflowRouting: GetNextStep()
        WorkflowRouting-->>WorkflowEngine: Next State
        WorkflowEngine->>Database: Transition State
        WorkflowEngine->>Database: Create Next Task
        WorkflowEngine->>NotificationService: NotifyNextAssignee()
    else Rejected
        WorkflowEngine->>Database: Update Status to Rejected
        WorkflowEngine->>WorkflowRouting: GetPreviousStep()
        WorkflowRouting-->>WorkflowEngine: Previous State
        WorkflowEngine->>Database: Transition to Previous
        WorkflowEngine->>NotificationService: NotifyRejection()
    end
```

---

## 3. Task Assignment and Routing

```mermaid
flowchart TD
    Start[Workflow Task Created] --> GetAssignee[Get Assignee Rules]
    GetAssignee --> CheckRACI{Has RACI Assignment?}
    
    CheckRACI -->|Yes| GetRACIAssignee[Get RACI Assignee]
    CheckRACI -->|No| CheckTeam{Has Team Assignment?}
    
    GetRACIAssignee --> CheckAvailability{Assignee Available?}
    CheckTeam -->|Yes| GetTeamMember[Get Team Member]
    CheckTeam -->|No| CheckRole{Has Role Assignment?}
    
    CheckRole -->|Yes| GetRoleMember[Get User by Role]
    CheckRole -->|No| GetDefaultAssignee[Get Default Assignee]
    
    GetTeamMember --> CheckAvailability
    GetRoleMember --> CheckAvailability
    GetDefaultAssignee --> CheckAvailability
    
    CheckAvailability -->|Yes| AssignTask[Assign Task]
    CheckAvailability -->|No| CheckDelegation{Has Delegation?}
    
    CheckDelegation -->|Yes| GetDelegatedAssignee[Get Delegated Assignee]
    CheckDelegation -->|No| Escalate[Escalate Task]
    
    GetDelegatedAssignee --> AssignTask
    Escalate --> AssignTask
    
    AssignTask --> CreateNotification[Create Notification]
    CreateNotification --> SendNotification[Send Notification]
    SendNotification --> End[Task Assigned]
```

---

## 4. Approval Chain Flow

```mermaid
stateDiagram-v2
    [*] --> TaskCreated: Create Task
    TaskCreated --> ApprovalRequired: Approval Needed?
    
    ApprovalRequired --> ApprovalChainCreated: Create Approval Chain
    ApprovalChainCreated --> ApprovalInstanceCreated: Create Approval Instance
    
    ApprovalInstanceCreated --> PendingApproval: Wait for Approval
    PendingApproval --> ApprovalSubmitted: User Submits
    
    ApprovalSubmitted --> CheckAllApprovers{All Approvers<br/>Approved?}
    CheckAllApprovers -->|No| PendingApproval: Wait for More
    CheckAllApprovers -->|Yes| AllApproved: All Approved
    
    AllApproved --> ApprovalRecorded: Record Approval
    ApprovalRecorded --> TaskApproved: Task Approved
    TaskApproved --> WorkflowContinues: Continue Workflow
    
    ApprovalSubmitted --> Rejected: User Rejects
    Rejected --> RejectionRecorded: Record Rejection
    RejectionRecorded --> TaskRejected: Task Rejected
    TaskRejected --> WorkflowRollback: Rollback Workflow
    
    WorkflowContinues --> [*]
    WorkflowRollback --> [*]
```

---

## 5. Escalation Path

```mermaid
flowchart LR
    TaskCreated[Task Created] --> StartTimer[Start SLA Timer]
    StartTimer --> CheckSLA{SLA Expired?}
    
    CheckSLA -->|No| Monitor[Monitor Task]
    Monitor --> CheckSLA
    
    CheckSLA -->|Yes| CheckEscalationRule{Escalation Rule?}
    CheckEscalationRule -->|Yes| GetEscalationTarget[Get Escalation Target]
    CheckEscalationRule -->|No| NotifyOwner[Notify Task Owner]
    
    GetEscalationTarget --> EscalateTask[Escalate Task]
    EscalateTask --> CreateEscalationRecord[Create Escalation Record]
    CreateEscalationRecord --> NotifyEscalatedUser[Notify Escalated User]
    NotifyEscalatedUser --> UpdateSLA[Update SLA Timer]
    UpdateSLA --> Monitor
    
    NotifyOwner --> Monitor
```

---

## 6. Workflow → Notification Connections

```mermaid
graph TB
    subgraph "Workflow Events"
        TaskCreated[Task Created]
        TaskAssigned[Task Assigned]
        TaskCompleted[Task Completed]
        TaskRejected[Task Rejected]
        ApprovalRequired[Approval Required]
        ApprovalSubmitted[Approval Submitted]
        EscalationTriggered[Escalation Triggered]
        WorkflowCompleted[Workflow Completed]
    end
    
    subgraph "Notification Service"
        NotificationService[NotificationService]
        EmailService[EmailService]
        SlackService[SlackService]
        TeamsService[TeamsService]
        SmsService[SmsService]
    end
    
    subgraph "Notification Delivery"
        HangfireJob[NotificationDeliveryJob]
        ImmediateDelivery[Immediate Delivery]
        ScheduledDelivery[Scheduled Delivery]
    end
    
    TaskCreated --> NotificationService
    TaskAssigned --> NotificationService
    TaskCompleted --> NotificationService
    TaskRejected --> NotificationService
    ApprovalRequired --> NotificationService
    ApprovalSubmitted --> NotificationService
    EscalationTriggered --> NotificationService
    WorkflowCompleted --> NotificationService
    
    NotificationService --> EmailService
    NotificationService --> SlackService
    NotificationService --> TeamsService
    NotificationService --> SmsService
    
    EmailService --> ImmediateDelivery
    EmailService --> HangfireJob
    SlackService --> ImmediateDelivery
    TeamsService --> ImmediateDelivery
    SmsService --> ImmediateDelivery
    
    HangfireJob --> ScheduledDelivery
```

---

## 7. Workflow Types and Their Connections

```mermaid
graph LR
    subgraph "Risk Workflow"
        RiskCreate[Risk Created]
        RiskAssess[Risk Assessed]
        RiskAccept[Risk Accepted]
        RiskMitigate[Risk Mitigated]
    end
    
    subgraph "Evidence Workflow"
        EvidenceUpload[Evidence Uploaded]
        EvidenceSubmit[Evidence Submitted]
        EvidenceReview[Evidence Under Review]
        EvidenceApprove[Evidence Approved]
        EvidenceReject[Evidence Rejected]
    end
    
    subgraph "Assessment Workflow"
        AssessmentCreate[Assessment Created]
        AssessmentExecute[Assessment Executed]
        AssessmentSubmit[Assessment Submitted]
        AssessmentApprove[Assessment Approved]
    end
    
    subgraph "Audit Workflow"
        AuditPlan[Audit Planned]
        AuditExecute[Audit Executed]
        FindingDocument[Finding Documented]
        ActionPlanCreate[Action Plan Created]
        AuditClose[Audit Closed]
    end
    
    subgraph "Policy Workflow"
        PolicyCreate[Policy Created]
        PolicyReview[Policy Reviewed]
        PolicyApprove[Policy Approved]
        PolicyPublish[Policy Published]
    end
    
    RiskCreate --> RiskAssess
    RiskAssess --> RiskAccept
    RiskAssess --> RiskMitigate
    RiskMitigate --> EvidenceUpload
    
    EvidenceUpload --> EvidenceSubmit
    EvidenceSubmit --> EvidenceReview
    EvidenceReview --> EvidenceApprove
    EvidenceReview --> EvidenceReject
    EvidenceReject --> EvidenceUpload
    
    AssessmentCreate --> AssessmentExecute
    AssessmentExecute --> EvidenceReview
    AssessmentExecute --> AssessmentSubmit
    AssessmentSubmit --> AssessmentApprove
    
    AuditPlan --> AuditExecute
    AuditExecute --> FindingDocument
    FindingDocument --> ActionPlanCreate
    ActionPlanCreate --> AuditClose
    
    PolicyCreate --> PolicyReview
    PolicyReview --> PolicyApprove
    PolicyApprove --> PolicyPublish
```

---

## 8. Workflow State Transitions

```mermaid
stateDiagram-v2
    [*] --> Draft: Create
    Draft --> InProgress: Start
    InProgress --> TaskAssigned: Assign Task
    TaskAssigned --> InReview: Submit for Review
    InReview --> Approved: Approve
    InReview --> Rejected: Reject
    Rejected --> TaskAssigned: Reassign
    Approved --> MoreTasks{More Tasks?}
    MoreTasks -->|Yes| TaskAssigned: Next Task
    MoreTasks -->|No| Completed: All Complete
    Completed --> [*]
    
    InProgress --> Escalated: SLA Expired
    Escalated --> TaskAssigned: Reassign
    TaskAssigned --> Overdue: Timeout
    Overdue --> Escalated: Escalate
```

---

## 9. Workflow Service Dependencies

```mermaid
graph TD
    subgraph "Workflow Services"
        WorkflowService[WorkflowService]
        WorkflowEngine[WorkflowEngineService]
        WorkflowRouting[WorkflowRoutingService]
        WorkflowAudit[WorkflowAuditService]
        RiskWorkflow[RiskWorkflowService]
        EvidenceWorkflow[EvidenceWorkflowService]
    end
    
    subgraph "Supporting Services"
        TenantService[TenantService]
        WorkspaceService[WorkspaceService]
        NotificationService[NotificationService]
        EscalationService[EscalationService]
        UnitOfWork[UnitOfWork]
    end
    
    subgraph "Infrastructure"
        Repository[Repository]
        Database[(Database)]
    end
    
    WorkflowService --> WorkflowEngine
    WorkflowService --> WorkflowRouting
    WorkflowService --> WorkflowAudit
    WorkflowService --> RiskWorkflow
    WorkflowService --> EvidenceWorkflow
    
    WorkflowEngine --> UnitOfWork
    WorkflowRouting --> TenantService
    WorkflowRouting --> WorkspaceService
    WorkflowAudit --> UnitOfWork
    RiskWorkflow --> NotificationService
    EvidenceWorkflow --> NotificationService
    
    WorkflowEngine --> EscalationService
    EscalationService --> NotificationService
    
    UnitOfWork --> Repository
    Repository --> Database
```

---

## 10. Complete Workflow Lifecycle

```mermaid
flowchart TB
    Start[Workflow Triggered] --> LoadDefinition[Load Workflow Definition]
    LoadDefinition --> CreateInstance[Create Workflow Instance]
    CreateInstance --> InitializeState[Initialize State]
    InitializeState --> CreateFirstTask[Create First Task]
    CreateFirstTask --> AssignTask[Assign Task]
    AssignTask --> NotifyAssignee[Notify Assignee]
    NotifyAssignee --> WaitForAction[Wait for User Action]
    
    WaitForAction --> UserAction{User Action}
    UserAction -->|Complete| ProcessCompletion[Process Completion]
    UserAction -->|Reject| ProcessRejection[Process Rejection]
    UserAction -->|Delegate| ProcessDelegation[Process Delegation]
    UserAction -->|Escalate| ProcessEscalation[Process Escalation]
    
    ProcessCompletion --> CheckApproval{Approval Required?}
    CheckApproval -->|Yes| CreateApproval[Create Approval Instance]
    CheckApproval -->|No| CheckNextStep{More Steps?}
    
    CreateApproval --> WaitForApproval[Wait for Approval]
    WaitForApproval --> ApprovalDecision{Approval Decision}
    ApprovalDecision -->|Approved| CheckNextStep
    ApprovalDecision -->|Rejected| ProcessRejection
    
    ProcessRejection --> UpdateState[Update State to Rejected]
    UpdateState --> NotifyRejection[Notify Rejection]
    NotifyRejection --> Rollback[Rollback if Needed]
    Rollback --> End[End Workflow]
    
    ProcessDelegation --> UpdateAssignee[Update Assignee]
    UpdateAssignee --> NotifyNewAssignee[Notify New Assignee]
    NotifyNewAssignee --> WaitForAction
    
    ProcessEscalation --> CreateEscalation[Create Escalation Record]
    CreateEscalation --> UpdateAssignee
    UpdateAssignee --> NotifyEscalated[Notify Escalated User]
    NotifyEscalated --> WaitForAction
    
    CheckNextStep -->|Yes| GetNextStep[Get Next Step]
    GetNextStep --> CreateNextTask[Create Next Task]
    CreateNextTask --> AssignTask
    
    CheckNextStep -->|No| CompleteWorkflow[Complete Workflow]
    CompleteWorkflow --> UpdateFinalState[Update Final State]
    UpdateFinalState --> NotifyCompletion[Notify Completion]
    NotifyCompletion --> End
```

---

## 11. Workflow Entity Relationships

```mermaid
erDiagram
    WorkflowDefinition ||--o{ WorkflowInstance : "instantiates"
    WorkflowInstance ||--o{ WorkflowTask : "contains"
    WorkflowInstance ||--o{ WorkflowAuditEntry : "has_audit"
    WorkflowInstance ||--o{ WorkflowEscalation : "has_escalations"
    
    WorkflowTask ||--o{ TaskComment : "has_comments"
    WorkflowTask ||--o{ TaskDelegation : "has_delegations"
    WorkflowTask ||--o{ ApprovalChain : "requires"
    
    ApprovalChain ||--o{ ApprovalInstance : "creates"
    ApprovalInstance ||--o{ ApprovalRecord : "has_records"
    
    WorkflowInstance ||--o{ WorkflowNotification : "triggers"
    WorkflowInstance ||--o{ WorkflowApproval : "has_approvals"
    WorkflowInstance ||--o{ WorkflowTransition : "has_transitions"
    
    WorkflowTask ||--o{ SlaRule : "has_sla"
    WorkflowTask ||--o{ EscalationRule : "has_escalation_rules"
```

---

## 12. Workflow Integration Points

```mermaid
graph TB
    subgraph "Workflow Triggers"
        RiskCreated[Risk Created]
        EvidenceUploaded[Evidence Uploaded]
        AssessmentCreated[Assessment Created]
        AuditStarted[Audit Started]
        PolicyViolation[Policy Violation]
        ActionPlanCreated[Action Plan Created]
    end
    
    subgraph "Workflow Engine"
        WorkflowService[WorkflowService]
        WorkflowEngine[WorkflowEngineService]
    end
    
    subgraph "Workflow Actions"
        TaskCreated[Task Created]
        ApprovalRequired[Approval Required]
        NotificationSent[Notification Sent]
        EscalationTriggered[Escalation Triggered]
    end
    
    subgraph "External Systems"
        Email[Email Service]
        Slack[Slack Service]
        Teams[Teams Service]
        Webhook[Webhook Service]
    end
    
    RiskCreated --> WorkflowService
    EvidenceUploaded --> WorkflowService
    AssessmentCreated --> WorkflowService
    AuditStarted --> WorkflowService
    PolicyViolation --> WorkflowService
    ActionPlanCreated --> WorkflowService
    
    WorkflowService --> WorkflowEngine
    WorkflowEngine --> TaskCreated
    WorkflowEngine --> ApprovalRequired
    WorkflowEngine --> NotificationSent
    WorkflowEngine --> EscalationTriggered
    
    NotificationSent --> Email
    NotificationSent --> Slack
    NotificationSent --> Teams
    NotificationSent --> Webhook
```

---

**Last Updated:** 2025-01-07  
**Next:** See [DIAGRAMS_TENANT_RBAC.md](./DIAGRAMS_TENANT_RBAC.md) for multi-tenancy and RBAC flows
