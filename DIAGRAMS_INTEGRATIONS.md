# Integration & Services - Feature Connection Diagrams
## External Systems, Service Dependencies, and Message Flows

**Generated:** 2025-01-07  
**Focus:** Integration points and service connections  

---

## 1. External System Integrations

```mermaid
graph TB
    subgraph "GRC Application"
        CoreServices[Core Services]
        IntegrationService[IntegrationService]
        WebhookService[WebhookService]
        NotificationService[NotificationService]
    end
    
    subgraph "ERP Integration"
        ERPSystem[ERP Systems]
        ERPConfig[ERPSystemConfig]
        ERPExtract[ERPExtractConfig]
        ERPExtractExecution[ERPExtractExecution]
    end
    
    subgraph "HRIS Integration"
        HRISSystem[HRIS Systems]
        HRISService[HRISService]
    end
    
    subgraph "Communication"
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
    
    CoreServices --> IntegrationService
    CoreServices --> WebhookService
    CoreServices --> NotificationService
    
    IntegrationService --> ERPSystem
    IntegrationService --> HRISSystem
    IntegrationService --> RabbitMQ
    
    ERPSystem --> ERPConfig
    ERPConfig --> ERPExtract
    ERPExtract --> ERPExtractExecution
    
    HRISSystem --> HRISService
    
    NotificationService --> EmailProvider
    NotificationService --> Slack
    NotificationService --> Teams
    NotificationService --> Twilio
    
    WebhookService --> RabbitMQ
    RabbitMQ --> Kafka
    Debezium --> Kafka
    Kafka --> IntegrationService
```

---

## 2. Service Dependency Graph

```mermaid
graph TD
    subgraph "Presentation Layer"
        Controllers[Controllers]
    end
    
    subgraph "Domain Services"
        RiskService[RiskService]
        ControlService[ControlService]
        EvidenceService[EvidenceService]
        AssessmentService[AssessmentService]
        AuditService[AuditService]
        PolicyService[PolicyService]
        WorkflowService[WorkflowService]
    end
    
    subgraph "Infrastructure Services"
        TenantService[TenantService]
        WorkspaceService[WorkspaceService]
        NotificationService[NotificationService]
        IntegrationService[IntegrationService]
        AnalyticsService[AnalyticsService]
    end
    
    subgraph "Supporting Services"
        PolicyHelper[PolicyEnforcementHelper]
        UnitOfWork[UnitOfWork]
        CachingService[CachingService]
        SerialNumberService[SerialNumberService]
    end
    
    subgraph "External Services"
        EmailService[EmailService]
        SlackService[SlackService]
        TeamsService[TeamsService]
        SmsService[SmsService]
        WebhookService[WebhookService]
    end
    
    Controllers --> RiskService
    Controllers --> ControlService
    Controllers --> EvidenceService
    Controllers --> AssessmentService
    Controllers --> AuditService
    Controllers --> PolicyService
    Controllers --> WorkflowService
    
    RiskService --> PolicyHelper
    RiskService --> UnitOfWork
    RiskService --> NotificationService
    RiskService --> TenantService
    
    ControlService --> PolicyHelper
    ControlService --> UnitOfWork
    ControlService --> NotificationService
    ControlService --> TenantService
    
    EvidenceService --> PolicyHelper
    EvidenceService --> UnitOfWork
    EvidenceService --> NotificationService
    EvidenceService --> WorkflowService
    EvidenceService --> TenantService
    
    AssessmentService --> PolicyHelper
    AssessmentService --> UnitOfWork
    AssessmentService --> NotificationService
    AssessmentService --> TenantService
    
    AuditService --> PolicyHelper
    AuditService --> UnitOfWork
    AuditService --> NotificationService
    AuditService --> TenantService
    
    PolicyService --> PolicyHelper
    PolicyService --> UnitOfWork
    PolicyService --> NotificationService
    
    WorkflowService --> UnitOfWork
    WorkflowService --> NotificationService
    WorkflowService --> TenantService
    WorkflowService --> WorkspaceService
    
    NotificationService --> EmailService
    NotificationService --> SlackService
    NotificationService --> TeamsService
    NotificationService --> SmsService
    
    IntegrationService --> WebhookService
    IntegrationService --> NotificationService
    
    UnitOfWork --> CachingService
    WorkflowService --> SerialNumberService
```

---

## 3. Message Queue Flow

```mermaid
sequenceDiagram
    participant Service
    participant MassTransit
    participant RabbitMQ
    participant Kafka
    participant Consumer
    participant WebhookService
    participant NotificationService
    participant AnalyticsService
    
    Service->>MassTransit: Publish Domain Event
    MassTransit->>RabbitMQ: Send Message
    RabbitMQ->>Kafka: Forward to Kafka
    Kafka->>Consumer: Deliver Message
    
    Consumer->>WebhookService: Process Webhook Event
    WebhookService->>WebhookService: Find Subscriptions
    WebhookService->>WebhookService: Deliver Webhook
    WebhookService-->>External: HTTP POST
    
    Consumer->>NotificationService: Process Notification Event
    NotificationService->>NotificationService: Determine Recipients
    NotificationService->>NotificationService: Send Notifications
    
    Consumer->>AnalyticsService: Process Analytics Event
    AnalyticsService->>AnalyticsService: Project to ClickHouse
    AnalyticsService->>ClickHouse: Store Analytics Data
```

---

## 4. Webhook Delivery Flow

```mermaid
flowchart TD
    Start[Domain Event Generated] --> GetSubscriptions[Get Webhook Subscriptions]
    GetSubscriptions --> FilterSubscriptions[Filter by Event Type]
    FilterSubscriptions --> ForEachSubscription{For Each Subscription}
    
    ForEachSubscription --> ValidateSubscription{Subscription Valid?}
    ValidateSubscription -->|No| SkipSubscription[Skip Subscription]
    ValidateSubscription -->|Yes| PreparePayload[Prepare Webhook Payload]
    
    PreparePayload --> SignPayload[Sign Payload]
    SignPayload --> SendWebhook[Send HTTP POST]
    SendWebhook --> CheckResponse{Response Status?}
    
    CheckResponse -->|200-299| Success[Record Success]
    CheckResponse -->|Other| Failure[Record Failure]
    
    Success --> LogDelivery[Log Delivery]
    Failure --> RetryLogic{Retry Needed?}
    
    RetryLogic -->|Yes| ScheduleRetry[Schedule Retry]
    RetryLogic -->|No| DeadLetter[Move to Dead Letter]
    
    ScheduleRetry --> WebhookRetryJob[WebhookRetryJob]
    WebhookRetryJob --> SendWebhook
    
    LogDelivery --> End[End]
    DeadLetter --> End
    SkipSubscription --> ForEachSubscription
```

---

## 5. Background Job Connections

```mermaid
graph TB
    subgraph "Hangfire Dashboard"
        HangfireUI[Hangfire UI]
    end
    
    subgraph "Background Jobs"
        NotificationJob[NotificationDeliveryJob<br/>Every 5 min]
        EscalationJob[EscalationJob<br/>Every 1 hour]
        SlaMonitorJob[SlaMonitorJob<br/>Every 30 min]
        WebhookRetryJob[WebhookRetryJob<br/>On demand]
        AnalyticsJob[AnalyticsProjectionJob<br/>Scheduled]
        CodeQualityJob[CodeQualityMonitorJob<br/>Scheduled]
    end
    
    subgraph "Services Used"
        NotificationService[NotificationService]
        WorkflowService[WorkflowService]
        WebhookService[WebhookService]
        AnalyticsService[AnalyticsService]
        CodeQualityService[CodeQualityService]
    end
    
    subgraph "Storage"
        PostgreSQL[(PostgreSQL)]
        ClickHouse[(ClickHouse)]
    end
    
    HangfireUI --> NotificationJob
    HangfireUI --> EscalationJob
    HangfireUI --> SlaMonitorJob
    HangfireUI --> WebhookRetryJob
    HangfireUI --> AnalyticsJob
    HangfireUI --> CodeQualityJob
    
    NotificationJob --> NotificationService
    NotificationJob --> PostgreSQL
    
    EscalationJob --> WorkflowService
    EscalationJob --> NotificationService
    EscalationJob --> PostgreSQL
    
    SlaMonitorJob --> WorkflowService
    SlaMonitorJob --> NotificationService
    SlaMonitorJob --> PostgreSQL
    
    WebhookRetryJob --> WebhookService
    WebhookRetryJob --> PostgreSQL
    
    AnalyticsJob --> AnalyticsService
    AnalyticsJob --> PostgreSQL
    AnalyticsJob --> ClickHouse
    
    CodeQualityJob --> CodeQualityService
    CodeQualityJob --> PostgreSQL
```

---

## 6. Integration Health Monitoring

```mermaid
flowchart LR
    Start[Integration Check] --> CheckConnector{Connector Active?}
    CheckConnector -->|No| MarkInactive[Mark Inactive]
    CheckConnector -->|Yes| TestConnection[Test Connection]
    
    TestConnection --> ConnectionStatus{Connection OK?}
    ConnectionStatus -->|Yes| RecordSuccess[Record Success]
    ConnectionStatus -->|No| RecordFailure[Record Failure]
    
    RecordSuccess --> UpdateHealthMetric[Update Health Metric]
    RecordFailure --> UpdateHealthMetric
    UpdateHealthMetric --> CheckThreshold{Below Threshold?}
    
    CheckThreshold -->|Yes| TriggerAlert[Trigger Alert]
    CheckThreshold -->|No| Continue[Continue Monitoring]
    
    TriggerAlert --> SendNotification[Send Notification]
    SendNotification --> AdminNotified[Admin Notified]
    
    MarkInactive --> End[End]
    Continue --> End
    AdminNotified --> End
```

---

## 7. Event-Driven Architecture

```mermaid
graph TB
    subgraph "Event Sources"
        RiskCreated[Risk Created]
        EvidenceUploaded[Evidence Uploaded]
        AssessmentCompleted[Assessment Completed]
        AuditStarted[Audit Started]
        PolicyViolated[Policy Violated]
        WorkflowCompleted[Workflow Completed]
    end
    
    subgraph "Event Publishing"
        DomainEvent[DomainEvent]
        EventPublisher[Event Publisher]
    end
    
    subgraph "Message Infrastructure"
        RabbitMQ[RabbitMQ]
        Kafka[Kafka]
        EventSchemaRegistry[EventSchemaRegistry]
    end
    
    subgraph "Event Consumers"
        WebhookConsumer[WebhookConsumer]
        NotificationConsumer[NotificationConsumer]
        AnalyticsConsumer[AnalyticsConsumer]
        IntegrationConsumer[IntegrationConsumer]
    end
    
    subgraph "Actions"
        WebhookDelivery[Webhook Delivery]
        NotificationDelivery[Notification Delivery]
        AnalyticsProjection[Analytics Projection]
        IntegrationSync[Integration Sync]
    end
    
    RiskCreated --> DomainEvent
    EvidenceUploaded --> DomainEvent
    AssessmentCompleted --> DomainEvent
    AuditStarted --> DomainEvent
    PolicyViolated --> DomainEvent
    WorkflowCompleted --> DomainEvent
    
    DomainEvent --> EventPublisher
    EventPublisher --> EventSchemaRegistry
    EventSchemaRegistry --> RabbitMQ
    RabbitMQ --> Kafka
    
    Kafka --> WebhookConsumer
    Kafka --> NotificationConsumer
    Kafka --> AnalyticsConsumer
    Kafka --> IntegrationConsumer
    
    WebhookConsumer --> WebhookDelivery
    NotificationConsumer --> NotificationDelivery
    AnalyticsConsumer --> AnalyticsProjection
    IntegrationConsumer --> IntegrationSync
```

---

## 8. Service Communication Patterns

```mermaid
graph TB
    subgraph "Synchronous Communication"
        ControllerSync[Controller]
        ServiceSync[Service]
        RepositorySync[Repository]
        DatabaseSync[(Database)]
        
        ControllerSync -->|"Direct Call"| ServiceSync
        ServiceSync -->|"Direct Call"| RepositorySync
        RepositorySync -->|"Query"| DatabaseSync
        DatabaseSync -->|"Response"| RepositorySync
        RepositorySync -->|"Return"| ServiceSync
        ServiceSync -->|"Return"| ControllerSync
    end
    
    subgraph "Asynchronous Communication"
        ServiceAsync[Service]
        MassTransitAsync[MassTransit]
        RabbitMQAsync[RabbitMQ]
        ConsumerAsync[Consumer]
        ActionAsync[Action Service]
        
        ServiceAsync -->|"Publish Event"| MassTransitAsync
        MassTransitAsync -->|"Send Message"| RabbitMQAsync
        RabbitMQAsync -->|"Deliver Message"| ConsumerAsync
        ConsumerAsync -->|"Process"| ActionAsync
    end
    
    subgraph "Background Processing"
        HangfireScheduled[Hangfire Scheduled]
        BackgroundJob[Background Job]
        ServiceBackground[Service]
        
        HangfireScheduled -->|"Trigger"| BackgroundJob
        BackgroundJob -->|"Call"| ServiceBackground
    end
```

---

## 9. Integration Entity Relationships

```mermaid
erDiagram
    IntegrationConnector ||--o{ SyncJob : "has_jobs"
    SyncJob ||--o{ SyncExecutionLog : "has_executions"
    IntegrationConnector ||--o{ IntegrationHealthMetric : "has_metrics"
    
    WebhookSubscription ||--o{ WebhookDeliveryLog : "has_deliveries"
    WebhookSubscription ||--o{ EventSubscription : "subscribes_to"
    
    DomainEvent ||--o{ EventSubscription : "matches"
    DomainEvent ||--o{ EventDeliveryLog : "has_deliveries"
    
    ERPSystemConfig ||--o{ ERPExtractConfig : "has_extracts"
    ERPExtractConfig ||--o{ ERPExtractExecution : "has_executions"
    
    SystemOfRecordDefinition ||--o{ CrossReferenceMapping : "has_mappings"
    
    EventSchemaRegistry ||--o{ DomainEvent : "defines_schema"
    
    DeadLetterEntry ||--o{ DomainEvent : "failed_delivery"
```

---

## 10. Complete Integration Flow

```mermaid
flowchart TB
    Start[External System Event] --> ReceiveEvent[Receive Event]
    ReceiveEvent --> ValidateEvent[Validate Event Schema]
    ValidateEvent --> ParseEvent[Parse Event Data]
    ParseEvent --> TransformEvent[Transform to Domain Event]
    TransformEvent --> PublishEvent[Publish Domain Event]
    
    PublishEvent --> CheckSubscriptions[Check Event Subscriptions]
    CheckSubscriptions --> WebhookSubs{Webhook Subscriptions?}
    CheckSubscriptions --> IntegrationSubs{Integration Subscriptions?}
    CheckSubscriptions --> NotificationSubs{Notification Subscriptions?}
    
    WebhookSubs -->|Yes| DeliverWebhook[Deliver Webhook]
    DeliverWebhook --> LogWebhook[Log Webhook Delivery]
    LogWebhook --> CheckWebhookResponse{Success?}
    CheckWebhookResponse -->|No| RetryWebhook[Retry Webhook]
    CheckWebhookResponse -->|Yes| WebhookComplete[Webhook Complete]
    
    IntegrationSubs -->|Yes| SyncIntegration[Sync Integration]
    SyncIntegration --> CreateSyncJob[Create Sync Job]
    CreateSyncJob --> ExecuteSync[Execute Sync]
    ExecuteSync --> LogSync[Log Sync Execution]
    LogSync --> IntegrationComplete[Integration Complete]
    
    NotificationSubs -->|Yes| SendNotification[Send Notification]
    SendNotification --> Email[Email]
    SendNotification --> Slack[Slack]
    SendNotification --> Teams[Teams]
    SendNotification --> SMS[SMS]
    Email --> NotificationComplete[Notification Complete]
    Slack --> NotificationComplete
    Teams --> NotificationComplete
    SMS --> NotificationComplete
    
    RetryWebhook --> ScheduleRetry[Schedule Retry Job]
    ScheduleRetry --> WebhookRetryJob[WebhookRetryJob]
    WebhookRetryJob --> DeliverWebhook
    
    WebhookComplete --> End[End]
    IntegrationComplete --> End
    NotificationComplete --> End
```

---

## 11. Service Layer Architecture

```mermaid
graph TB
    subgraph "Interface Layer"
        IRiskService[IRiskService]
        IControlService[IControlService]
        IEvidenceService[IEvidenceService]
        IWorkflowService[IWorkflowService]
        ITenantService[ITenantService]
        INotificationService[INotificationService]
    end
    
    subgraph "Implementation Layer"
        RiskService[RiskService]
        ControlService[ControlService]
        EvidenceService[EvidenceService]
        WorkflowService[WorkflowService]
        TenantService[TenantService]
        NotificationService[NotificationService]
    end
    
    subgraph "Shared Services"
        UnitOfWork[UnitOfWork]
        PolicyHelper[PolicyEnforcementHelper]
        CachingService[CachingService]
    end
    
    IRiskService --> RiskService
    IControlService --> ControlService
    IEvidenceService --> EvidenceService
    IWorkflowService --> WorkflowService
    ITenantService --> TenantService
    INotificationService --> NotificationService
    
    RiskService --> UnitOfWork
    RiskService --> PolicyHelper
    ControlService --> UnitOfWork
    ControlService --> PolicyHelper
    EvidenceService --> UnitOfWork
    EvidenceService --> PolicyHelper
    WorkflowService --> UnitOfWork
    WorkflowService --> NotificationService
    
    RiskService --> CachingService
    ControlService --> CachingService
    EvidenceService --> CachingService
```

---

## 12. Debezium CDC Flow

```mermaid
sequenceDiagram
    participant PostgreSQL
    participant Debezium
    participant Kafka
    participant Consumer
    participant ClickHouse
    participant AnalyticsService
    
    PostgreSQL->>Debezium: Database Change Event
    Debezium->>Debezium: Capture Change
    Debezium->>Kafka: Publish to Kafka Topic
    Kafka->>Consumer: Deliver Message
    Consumer->>Consumer: Parse Change Event
    Consumer->>AnalyticsService: Process Analytics Event
    AnalyticsService->>ClickHouse: Project to ClickHouse
    ClickHouse-->>AnalyticsService: Stored
    AnalyticsService-->>Consumer: Processed
```

---

**Last Updated:** 2025-01-07  
**Next:** See [DIAGRAMS_ONBOARDING.md](./DIAGRAMS_ONBOARDING.md) for onboarding system flows
