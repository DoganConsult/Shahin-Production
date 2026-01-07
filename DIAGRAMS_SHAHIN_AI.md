# Shahin AI Platform - Feature Connection Diagrams
## 6-Module Architecture and Interconnections

**Generated:** 2025-01-07  
**Focus:** Shahin AI platform modules and their connections  

---

## 1. Shahin AI 6-Module Architecture

```mermaid
graph TB
    subgraph "Shahin AI Platform"
        Orchestration[ShahinAIOrchestrationService]
    end
    
    subgraph "MAP Module - Control Library"
        MAPService[MAPService]
        CanonicalControl[CanonicalControl]
        FrameworkMapping[Framework Mapping]
        PlainLanguageControl[PlainLanguageControl]
    end
    
    subgraph "APPLY Module - Scope & Applicability"
        APPLYService[APPLYService]
        ApplicabilityMatrix[ApplicabilityMatrix]
        ApplicabilityRule[ApplicabilityRule]
        BaselineOverlay[BaselineOverlayModel]
    end
    
    subgraph "PROVE Module - Evidence & Testing"
        PROVEService[PROVEService]
        AutoTaggedEvidence[AutoTaggedEvidence]
        CapturedEvidence[CapturedEvidence]
        TestProcedure[TestProcedure]
        EvidenceScore[EvidenceScore]
    end
    
    subgraph "WATCH Module - Monitoring & Alerts"
        WATCHService[WATCHService]
        RiskIndicator[RiskIndicator]
        RiskIndicatorMeasurement[RiskIndicatorMeasurement]
        RiskIndicatorAlert[RiskIndicatorAlert]
        GovernanceCadence[GovernanceCadence]
    end
    
    subgraph "FIX Module - Remediation"
        FIXService[FIXService]
        ControlException[ControlException]
        ActionPlan[ActionPlan]
    end
    
    subgraph "VAULT Module - Secure Storage"
        VAULTService[VAULTService]
        CryptographicAsset[CryptographicAsset]
        ComplianceGuardrail[ComplianceGuardrail]
        StrategicRoadmap[StrategicRoadmapMilestone]
    end
    
    Orchestration --> MAPService
    Orchestration --> APPLYService
    Orchestration --> PROVEService
    Orchestration --> WATCHService
    Orchestration --> FIXService
    Orchestration --> VAULTService
    
    MAPService --> CanonicalControl
    MAPService --> FrameworkMapping
    MAPService --> PlainLanguageControl
    
    APPLYService --> ApplicabilityMatrix
    APPLYService --> ApplicabilityRule
    APPLYService --> BaselineOverlay
    
    PROVEService --> AutoTaggedEvidence
    PROVEService --> CapturedEvidence
    PROVEService --> TestProcedure
    PROVEService --> EvidenceScore
    
    WATCHService --> RiskIndicator
    WATCHService --> RiskIndicatorMeasurement
    WATCHService --> RiskIndicatorAlert
    WATCHService --> GovernanceCadence
    
    FIXService --> ControlException
    FIXService --> ActionPlan
    
    VAULTService --> CryptographicAsset
    VAULTService --> ComplianceGuardrail
    VAULTService --> StrategicRoadmap
```

---

## 2. Module Interconnections

```mermaid
graph LR
    MAP[MAP Module<br/>Control Library] --> APPLY[APPLY Module<br/>Scope & Applicability]
    APPLY --> PROVE[PROVE Module<br/>Evidence & Testing]
    PROVE --> WATCH[WATCH Module<br/>Monitoring & Alerts]
    WATCH --> FIX[FIX Module<br/>Remediation]
    FIX --> VAULT[VAULT Module<br/>Secure Storage]
    
    MAP -.->|"Provides Controls"| PROVE
    APPLY -.->|"Determines Scope"| PROVE
    PROVE -.->|"Provides Evidence"| WATCH
    WATCH -.->|"Triggers Alerts"| FIX
    FIX -.->|"Stores Results"| VAULT
    
    MAP --> CoreGRC[Core GRC Modules]
    APPLY --> CoreGRC
    PROVE --> CoreGRC
    WATCH --> CoreGRC
    FIX --> CoreGRC
    VAULT --> CoreGRC
```

---

## 3. MAP Module - Control Library Flow

```mermaid
flowchart TD
    Start[Load Control Library] --> LoadCanonical[Load Canonical Controls]
    LoadCanonical --> MapToFramework[Map to Frameworks]
    MapToFramework --> CreatePlainLanguage[Create Plain Language Controls]
    CreatePlainLanguage --> GenerateEvidencePacks[Generate Evidence Packs]
    GenerateEvidencePacks --> LinkToControls[Link Evidence Packs to Controls]
    LinkToControls --> StoreInLibrary[Store in Control Library]
    StoreInLibrary --> End[Control Library Ready]
    
    MapToFramework --> FrameworkControl[FrameworkControl Entity]
    CreatePlainLanguage --> PlainLanguageControl[PlainLanguageControl Entity]
    GenerateEvidencePacks --> UniversalEvidencePack[UniversalEvidencePack Entity]
```

---

## 4. APPLY Module - Scope & Applicability Flow

```mermaid
sequenceDiagram
    participant User
    participant APPLYService
    participant ApplicabilityMatrix
    participant ApplicabilityRule
    participant BaselineOverlay
    participant ControlService
    participant Database
    
    User->>APPLYService: Determine Applicability
    APPLYService->>ApplicabilityMatrix: Load Matrix
    ApplicabilityMatrix->>Database: Query Matrix
    Database-->>ApplicabilityMatrix: Matrix Data
    ApplicabilityMatrix-->>APPLYService: Matrix
    
    APPLYService->>ApplicabilityRule: Evaluate Rules
    ApplicabilityRule->>Database: Query Rules
    Database-->>ApplicabilityRule: Rules
    ApplicabilityRule-->>APPLYService: Applicable Controls
    
    APPLYService->>BaselineOverlay: Apply Overlay
    BaselineOverlay->>Database: Query Overlay
    Database-->>BaselineOverlay: Overlay Config
    BaselineOverlay-->>APPLYService: Overlay Applied
    
    APPLYService->>ControlService: Create Applicable Controls
    ControlService->>Database: Save Controls
    Database-->>ControlService: Controls Created
    ControlService-->>APPLYService: Success
    APPLYService-->>User: Applicability Determined
```

---

## 5. PROVE Module - Evidence & Testing Flow

```mermaid
flowchart LR
    EvidenceUpload[Evidence Uploaded] --> AutoTag[AI Auto-Tagging]
    AutoTag --> AutoTaggedEvidence[AutoTaggedEvidence]
    AutoTaggedEvidence --> EvidenceCapture[Evidence Capture]
    EvidenceCapture --> CapturedEvidence[CapturedEvidence]
    CapturedEvidence --> EvidenceScoring[Evidence Scoring]
    EvidenceScoring --> EvidenceScore[EvidenceScore]
    EvidenceScore --> TestProcedure[Test Procedure]
    TestProcedure --> TestExecution[Test Execution]
    TestExecution --> TestResult[Test Result]
    TestResult --> ControlTest[Control Test]
    ControlTest --> Assessment[Assessment]
```

---

## 6. WATCH Module - Monitoring Flow

```mermaid
graph TB
    subgraph "Data Collection"
        RiskData[Risk Data]
        ControlData[Control Data]
        EvidenceData[Evidence Data]
        AssessmentData[Assessment Data]
    end
    
    subgraph "WATCH Module"
        WATCHService[WATCHService]
        RiskIndicator[RiskIndicator]
        Measurement[RiskIndicatorMeasurement]
        Alert[RiskIndicatorAlert]
        Cadence[GovernanceCadence]
    end
    
    subgraph "Output"
        Dashboard[Dashboard]
        Notification[Notifications]
        Report[Reports]
    end
    
    RiskData --> WATCHService
    ControlData --> WATCHService
    EvidenceData --> WATCHService
    AssessmentData --> WATCHService
    
    WATCHService --> RiskIndicator
    RiskIndicator --> Measurement
    Measurement --> Alert
    WATCHService --> Cadence
    
    Alert --> Dashboard
    Alert --> Notification
    Cadence --> Dashboard
    Cadence --> Report
```

---

## 7. FIX Module - Remediation Flow

```mermaid
stateDiagram-v2
    [*] --> IssueIdentified: Issue Found
    IssueIdentified --> ExceptionCreated: Create Exception
    ExceptionCreated --> ExceptionApproved: Approve Exception
    ExceptionApproved --> RemediationPlan: Create Remediation Plan
    RemediationPlan --> ActionPlanCreated: Create Action Plan
    ActionPlanCreated --> ActionAssigned: Assign Actions
    ActionAssigned --> ActionInProgress: Execute Actions
    ActionInProgress --> ActionCompleted: Complete Actions
    ActionCompleted --> Verification: Verify Fix
    Verification --> Fixed: Issue Fixed
    Verification --> NotFixed: Not Fixed
    NotFixed --> RemediationPlan: Re-plan
    Fixed --> [*]
```

---

## 8. VAULT Module - Secure Storage Flow

```mermaid
flowchart TD
    DocumentUpload[Document Upload] --> ClassifyDocument[Classify Document]
    ClassifyDocument --> CheckClassification{Classification?}
    
    CheckClassification -->|Restricted| RequireApproval[Require Approval]
    CheckClassification -->|Confidential| RequireApproval
    CheckClassification -->|Internal| StandardStorage[Standard Storage]
    CheckClassification -->|Public| StandardStorage
    
    RequireApproval --> ApprovalWorkflow[Approval Workflow]
    ApprovalWorkflow --> Approved{Approved?}
    Approved -->|Yes| SecureStorage[Secure Storage]
    Approved -->|No| Reject[Reject Upload]
    
    StandardStorage --> SecureStorage
    SecureStorage --> Encrypt[Encrypt Document]
    Encrypt --> StoreVault[Store in Vault]
    StoreVault --> IndexDocument[Index Document]
    IndexDocument --> AccessControl[Apply Access Control]
    AccessControl --> Available[Document Available]
```

---

## 9. AI Agent System Connections

```mermaid
graph TB
    subgraph "AI Agent System"
        AgentDefinition[AgentDefinition]
        AgentCapability[AgentCapability]
        AgentAction[AgentAction]
        AgentApprovalGate[AgentApprovalGate]
        AgentSoDRule[AgentSoDRule]
    end
    
    subgraph "Shahin AI Modules"
        MAP[MAP Module]
        APPLY[APPLY Module]
        PROVE[PROVE Module]
        WATCH[WATCH Module]
        FIX[FIX Module]
        VAULT[VAULT Module]
    end
    
    subgraph "Human Oversight"
        PendingApproval[PendingApproval]
        HumanRetainedResponsibility[HumanRetainedResponsibility]
        RoleTransitionPlan[RoleTransitionPlan]
    end
    
    AgentDefinition --> AgentCapability
    AgentCapability --> AgentAction
    AgentAction --> AgentApprovalGate
    AgentAction --> AgentSoDRule
    
    AgentAction --> MAP
    AgentAction --> APPLY
    AgentAction --> PROVE
    AgentAction --> WATCH
    AgentAction --> FIX
    AgentAction --> VAULT
    
    AgentApprovalGate --> PendingApproval
    PendingApproval --> HumanRetainedResponsibility
    HumanRetainedResponsibility --> RoleTransitionPlan
```

---

## 10. Complete Shahin AI Data Flow

```mermaid
flowchart TB
    Start[User Accesses Shahin AI] --> Dashboard[Shahin AI Dashboard]
    Dashboard --> SelectModule{Select Module}
    
    SelectModule -->|MAP| MAPFlow[MAP: Control Library]
    SelectModule -->|APPLY| APPLYFlow[APPLY: Scope & Applicability]
    SelectModule -->|PROVE| PROVEFlow[PROVE: Evidence & Testing]
    SelectModule -->|WATCH| WATCHFlow[WATCH: Monitoring]
    SelectModule -->|FIX| FIXFlow[FIX: Remediation]
    SelectModule -->|VAULT| VAULTFlow[VAULT: Secure Storage]
    
    MAPFlow --> LoadControls[Load Canonical Controls]
    LoadControls --> MapFrameworks[Map to Frameworks]
    MapFrameworks --> GenerateEvidencePacks[Generate Evidence Packs]
    
    APPLYFlow --> LoadMatrix[Load Applicability Matrix]
    LoadMatrix --> EvaluateRules[Evaluate Applicability Rules]
    EvaluateRules --> ApplyOverlay[Apply Baseline Overlay]
    ApplyOverlay --> CreateControls[Create Applicable Controls]
    
    PROVEFlow --> UploadEvidence[Upload Evidence]
    UploadEvidence --> AutoTag[AI Auto-Tagging]
    AutoTag --> CaptureEvidence[Capture Evidence]
    CaptureEvidence --> ScoreEvidence[Score Evidence]
    ScoreEvidence --> RunTests[Run Test Procedures]
    
    WATCHFlow --> MonitorIndicators[Monitor Risk Indicators]
    MonitorIndicators --> MeasureMetrics[Measure Metrics]
    MeasureMetrics --> CheckAlerts[Check Alerts]
    CheckAlerts --> TriggerNotifications[Trigger Notifications]
    
    FIXFlow --> IdentifyIssue[Identify Issue]
    IdentifyIssue --> CreateException[Create Exception]
    CreateException --> CreateActionPlan[Create Action Plan]
    CreateActionPlan --> ExecuteRemediation[Execute Remediation]
    
    VAULTFlow --> UploadDocument[Upload Document]
    UploadDocument --> ClassifyDocument[Classify Document]
    ClassifyDocument --> RequireApproval{Approval Needed?}
    RequireApproval -->|Yes| ApprovalWorkflow[Approval Workflow]
    RequireApproval -->|No| StoreDocument[Store Document]
    ApprovalWorkflow --> StoreDocument
    StoreDocument --> EncryptDocument[Encrypt Document]
    EncryptDocument --> IndexDocument[Index Document]
    
    GenerateEvidencePacks --> CoreGRC[Core GRC Modules]
    CreateControls --> CoreGRC
    RunTests --> CoreGRC
    TriggerNotifications --> CoreGRC
    ExecuteRemediation --> CoreGRC
    IndexDocument --> CoreGRC
```

---

## 11. Shahin AI Entity Relationships

```mermaid
erDiagram
    MAPFrameworkConfig ||--o{ CanonicalControl : "contains"
    CanonicalControl ||--o{ PlainLanguageControl : "has_plain_language"
    CanonicalControl ||--o{ UniversalEvidencePack : "requires"
    UniversalEvidencePack ||--o{ UniversalEvidencePackItem : "contains"
    
    ApplicabilityMatrix ||--o{ ApplicabilityRule : "has_rules"
    ApplicabilityRule ||--o{ ApplicabilityEntry : "creates_entries"
    BaselineOverlayModel ||--o{ OverlayControlMapping : "maps_controls"
    
    AutoTaggedEvidence ||--o{ CapturedEvidence : "captures"
    CapturedEvidence ||--o{ EvidenceScore : "has_score"
    TestProcedure ||--o{ ControlTestProcedure : "tests_controls"
    
    RiskIndicator ||--o{ RiskIndicatorMeasurement : "has_measurements"
    RiskIndicatorMeasurement ||--o{ RiskIndicatorAlert : "triggers_alerts"
    GovernanceCadence ||--o{ CadenceExecution : "has_executions"
    
    ControlException ||--o{ ActionPlan : "creates_action_plan"
    
    CryptographicAsset ||--o{ ComplianceGuardrail : "protected_by"
    StrategicRoadmapMilestone ||--o{ ComplianceGuardrail : "enforced_by"
```

---

## 12. Shahin AI Integration with Core GRC

```mermaid
graph TB
    subgraph "Shahin AI Platform"
        MAP[MAP Module]
        APPLY[APPLY Module]
        PROVE[PROVE Module]
        WATCH[WATCH Module]
        FIX[FIX Module]
        VAULT[VAULT Module]
    end
    
    subgraph "Core GRC Modules"
        Risk[Risk Management]
        Control[Control Management]
        Evidence[Evidence Management]
        Assessment[Assessment Management]
        Audit[Audit Management]
        Policy[Policy Management]
        Workflow[Workflow Engine]
    end
    
    MAP --> Control
    MAP --> Framework[Framework Management]
    
    APPLY --> Control
    APPLY --> Assessment
    
    PROVE --> Evidence
    PROVE --> Control
    PROVE --> Assessment
    
    WATCH --> Risk
    WATCH --> Control
    WATCH --> Evidence
    
    FIX --> Control
    FIX --> ActionPlan[Action Plans]
    FIX --> Workflow
    
    VAULT --> Evidence
    VAULT --> Policy
    VAULT --> Audit
    
    Risk --> WATCH
    Control --> MAP
    Control --> APPLY
    Control --> PROVE
    Evidence --> PROVE
    Evidence --> VAULT
    Assessment --> PROVE
    Audit --> VAULT
    Policy --> VAULT
```

---

**Last Updated:** 2025-01-07  
**Next:** See [DIAGRAMS_INTEGRATIONS.md](./DIAGRAMS_INTEGRATIONS.md) for integration connections
