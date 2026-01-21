using System.Threading.Tasks;

namespace GrcMvc.Services.Kafka;

/// <summary>
/// Kafka message producer interface
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Publish event to Kafka topic
    /// </summary>
    Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken ct = default);
    
    /// <summary>
    /// Publish event with headers
    /// </summary>
    Task PublishAsync<T>(string topic, T message, Dictionary<string, string>? headers, string? key = null, CancellationToken ct = default);
}

/// <summary>
/// GRC Event Topics
/// </summary>
public static class KafkaTopics
{
    // Workflow Events
    public const string WorkflowStarted = "grc.workflow.started";
    public const string WorkflowCompleted = "grc.workflow.completed";
    public const string TaskAssigned = "grc.task.assigned";
    public const string TaskCompleted = "grc.task.completed";
    
    // Compliance Events
    public const string AssessmentCreated = "grc.assessment.created";
    public const string AssessmentSubmitted = "grc.assessment.submitted";
    public const string AssessmentApproved = "grc.assessment.approved";
    public const string ControlStatusChanged = "grc.control.status-changed";
    
    // Risk Events
    public const string RiskIdentified = "grc.risk.identified";
    public const string RiskMitigated = "grc.risk.mitigated";
    public const string RiskEscalated = "grc.risk.escalated";
    
    // Audit Events  
    public const string AuditStarted = "grc.audit.started";
    public const string AuditFindingCreated = "grc.audit.finding-created";
    public const string AuditCompleted = "grc.audit.completed";
    
    // Evidence Events
    public const string EvidenceUploaded = "grc.evidence.uploaded";
    public const string EvidenceApproved = "grc.evidence.approved";
    public const string EvidenceExpiring = "grc.evidence.expiring";
    
    // Policy Events
    public const string PolicyCreated = "grc.policy.created";
    public const string PolicyApproved = "grc.policy.approved";
    public const string PolicyPublished = "grc.policy.published";
    
    // Email Events
    public const string EmailReceived = "grc.email.received";
    public const string EmailClassified = "grc.email.classified";
    public const string EmailReplied = "grc.email.replied";
    
    // AI Agent Events
    public const string AgentAnalysisRequested = "grc.agent.analysis-requested";
    public const string AgentAnalysisCompleted = "grc.agent.analysis-completed";
    
    // Notification Events
    public const string NotificationCreated = "grc.notification.created";
    public const string AlertTriggered = "grc.alert.triggered";
}
