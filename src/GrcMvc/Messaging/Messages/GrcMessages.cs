namespace GrcMvc.Messaging.Messages
{
    /// <summary>
    /// Base interface for all GRC domain events
    /// </summary>
    public interface IGrcEvent
    {
        Guid EventId { get; }
        Guid TenantId { get; }
        DateTime Timestamp { get; }
        string EventType { get; }
    }

    /// <summary>
    /// Base interface for all GRC commands
    /// </summary>
    public interface IGrcCommand
    {
        Guid CommandId { get; }
        Guid TenantId { get; }
        string RequestedBy { get; }
    }

    // ============================================================
    // DOMAIN EVENTS
    // ============================================================

    /// <summary>
    /// Event fired when a risk is created or updated
    /// </summary>
    public record RiskAssessedEvent : IGrcEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string EventType => "Risk.Assessed";

        public Guid RiskId { get; init; }
        public string RiskTitle { get; init; } = string.Empty;
        public string RiskLevel { get; init; } = string.Empty;
        public decimal RiskScore { get; init; }
        public string AssessedBy { get; init; } = string.Empty;
    }

    /// <summary>
    /// Event fired when a control is implemented or updated
    /// </summary>
    public record ControlUpdatedEvent : IGrcEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string EventType => "Control.Updated";

        public Guid ControlId { get; init; }
        public string ControlCode { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    /// <summary>
    /// Event fired when compliance status changes
    /// </summary>
    public record ComplianceStatusChangedEvent : IGrcEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string EventType => "Compliance.StatusChanged";

        public string FrameworkCode { get; init; } = string.Empty;
        public string PreviousStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public decimal ComplianceScore { get; init; }
    }

    /// <summary>
    /// Event fired when a workflow task is assigned
    /// </summary>
    public record TaskAssignedEvent : IGrcEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string EventType => "Task.Assigned";

        public Guid TaskId { get; init; }
        public Guid WorkflowInstanceId { get; init; }
        public string TaskName { get; init; } = string.Empty;
        public string AssignedToUserId { get; init; } = string.Empty;
        public DateTime? DueDate { get; init; }
        public string Priority { get; init; } = "Medium";
    }

    /// <summary>
    /// Event fired when an SLA is breached
    /// </summary>
    public record SlaBreachedEvent : IGrcEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string EventType => "SLA.Breached";

        public Guid WorkflowId { get; init; }
        public string WorkflowType { get; init; } = string.Empty;
        public DateTime SlaDueDate { get; init; }
        public TimeSpan OverdueBy { get; init; }
    }

    /// <summary>
    /// Event fired when audit evidence is submitted
    /// </summary>
    public record EvidenceSubmittedEvent : IGrcEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string EventType => "Evidence.Submitted";

        public Guid EvidenceId { get; init; }
        public Guid ControlId { get; init; }
        public string SubmittedBy { get; init; } = string.Empty;
        public string EvidenceType { get; init; } = string.Empty;
    }

    // ============================================================
    // COMMANDS
    // ============================================================

    /// <summary>
    /// Command to send a notification
    /// </summary>
    public record SendNotificationCommand : IGrcCommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public string RequestedBy { get; init; } = "System";

        public string RecipientUserId { get; init; } = string.Empty;
        public string Channel { get; init; } = "Email"; // Email, Slack, Teams, SMS
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public string Priority { get; init; } = "Medium";
        public Dictionary<string, string>? Metadata { get; init; }
    }

    /// <summary>
    /// Command to process a webhook delivery
    /// </summary>
    public record ProcessWebhookCommand : IGrcCommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public string RequestedBy { get; init; } = "System";

        public Guid WebhookSubscriptionId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public string EventId { get; init; } = string.Empty;
        public string PayloadJson { get; init; } = string.Empty;
    }

    /// <summary>
    /// Command to generate a compliance report
    /// </summary>
    public record GenerateReportCommand : IGrcCommand
    {
        public Guid CommandId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public string RequestedBy { get; init; } = string.Empty;

        public string ReportType { get; init; } = string.Empty;
        public string FrameworkCode { get; init; } = string.Empty;
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public string OutputFormat { get; init; } = "PDF";
    }
}
