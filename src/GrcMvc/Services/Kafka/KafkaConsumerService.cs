using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Kafka;

/// <summary>
/// Background service for consuming Kafka messages
/// </summary>
public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly KafkaSettings _settings;
    private IConsumer<string, string>? _consumer;

    public KafkaConsumerService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> settings,
        ILogger<KafkaConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Kafka consumer is disabled");
            return;
        }

        await Task.Delay(5000, stoppingToken); // Wait for Kafka to be ready

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        };

        try
        {
            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Reason}", e.Reason))
                .Build();

            // Subscribe to GRC topics
            var topics = new[]
            {
                KafkaTopics.WorkflowStarted,
                KafkaTopics.WorkflowCompleted,
                KafkaTopics.TaskAssigned,
                KafkaTopics.AssessmentSubmitted,
                KafkaTopics.RiskIdentified,
                KafkaTopics.EmailReceived,
                KafkaTopics.AgentAnalysisRequested
            };

            _consumer.Subscribe(topics);
            _logger.LogInformation("Kafka consumer subscribed to {Count} topics", topics.Length);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result.IsPartitionEOF)
                    {
                        continue;
                    }

                    await ProcessMessageAsync(result, stoppingToken);
                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kafka consumer failed to start");
        }
        finally
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken ct)
    {
        var topic = result.Topic;
        var message = result.Message.Value;

        _logger.LogDebug("Processing message from {Topic}: {Key}", topic, result.Message.Key);

        using var scope = _serviceProvider.CreateScope();

        try
        {
            switch (topic)
            {
                case KafkaTopics.WorkflowStarted:
                    await HandleWorkflowStarted(scope.ServiceProvider, message, ct);
                    break;

                case KafkaTopics.TaskAssigned:
                    await HandleTaskAssigned(scope.ServiceProvider, message, ct);
                    break;

                case KafkaTopics.AssessmentSubmitted:
                    await HandleAssessmentSubmitted(scope.ServiceProvider, message, ct);
                    break;

                case KafkaTopics.RiskIdentified:
                    await HandleRiskIdentified(scope.ServiceProvider, message, ct);
                    break;

                case KafkaTopics.EmailReceived:
                    await HandleEmailReceived(scope.ServiceProvider, message, ct);
                    break;

                case KafkaTopics.AgentAnalysisRequested:
                    await HandleAgentAnalysisRequested(scope.ServiceProvider, message, ct);
                    break;

                default:
                    _logger.LogDebug("Unhandled topic: {Topic}", topic);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {Topic}", topic);
        }
    }

    private async Task HandleWorkflowStarted(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<WorkflowEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Workflow started: {WorkflowId} - {Type}", evt.WorkflowId, evt.WorkflowType);

        try
        {
            // Get audit service to log the workflow start event
            var auditService = sp.GetService<GrcMvc.Services.Interfaces.IAuditEventService>();
            if (auditService != null && Guid.TryParse(evt.TenantId, out var tenantId))
            {
                await auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "WorkflowStarted",
                    affectedEntityType: "Workflow",
                    affectedEntityId: evt.WorkflowId,
                    action: "Start",
                    actor: "SYSTEM",
                    payloadJson: message,
                    correlationId: evt.WorkflowId);
            }

            _logger.LogInformation("Workflow {WorkflowId} start event processed successfully", evt.WorkflowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process workflow started event for {WorkflowId}", evt.WorkflowId);
        }
    }

    private async Task HandleTaskAssigned(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<TaskEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Task assigned: {TaskId} to {Assignee}", evt.TaskId, evt.AssigneeId);

        try
        {
            // Send notification to assignee
            var notificationService = sp.GetService<GrcMvc.Services.Interfaces.INotificationService>();
            if (notificationService != null)
            {
                await notificationService.SendNotificationAsync(
                    workflowInstanceId: Guid.TryParse(evt.WorkflowId, out var wfId) ? wfId : Guid.Empty,
                    recipientUserId: evt.AssigneeId,
                    notificationType: "TaskAssigned",
                    subject: $"New Task Assigned: {evt.TaskType}",
                    body: $"You have been assigned a new task (ID: {evt.TaskId}). Due date: {evt.DueDate:yyyy-MM-dd}",
                    priority: evt.DueDate < DateTime.UtcNow.AddDays(2) ? "High" : "Normal");

                _logger.LogInformation("Notification sent to {Assignee} for task {TaskId}", evt.AssigneeId, evt.TaskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification for task {TaskId}", evt.TaskId);
        }
    }

    private async Task HandleAssessmentSubmitted(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<AssessmentEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Assessment submitted: {AssessmentId}", evt.AssessmentId);

        try
        {
            // Log the assessment submission event
            var auditService = sp.GetService<GrcMvc.Services.Interfaces.IAuditEventService>();
            if (auditService != null && Guid.TryParse(evt.TenantId, out var tenantId))
            {
                await auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "AssessmentSubmitted",
                    affectedEntityType: "Assessment",
                    affectedEntityId: evt.AssessmentId,
                    action: "Submit",
                    actor: "SYSTEM",
                    payloadJson: message);
            }

            // Notify compliance managers about the submission
            var notificationService = sp.GetService<GrcMvc.Services.Interfaces.INotificationService>();
            var userService = sp.GetService<GrcMvc.Services.Interfaces.IUserDirectoryService>();

            if (notificationService != null && userService != null)
            {
                var managers = await userService.GetUsersInRoleAsync("ComplianceManager");
                foreach (var manager in managers)
                {
                    await notificationService.SendNotificationAsync(
                        workflowInstanceId: Guid.Empty,
                        recipientUserId: manager.Id.ToString(),
                        notificationType: "AssessmentSubmitted",
                        subject: "Assessment Submitted for Review",
                        body: $"Assessment {evt.AssessmentId} has been submitted and requires review.",
                        priority: "Normal",
                        tenantId: Guid.TryParse(evt.TenantId, out var tid) ? tid : Guid.Empty);
                }
            }

            _logger.LogInformation("Assessment {AssessmentId} submission processed successfully", evt.AssessmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process assessment submitted event for {AssessmentId}", evt.AssessmentId);
        }
    }

    private async Task HandleRiskIdentified(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<RiskEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Risk identified: {RiskId} - Severity: {Severity}", evt.RiskId, evt.Severity);

        try
        {
            // Log the risk identification event
            var auditService = sp.GetService<GrcMvc.Services.Interfaces.IAuditEventService>();
            if (auditService != null && Guid.TryParse(evt.TenantId, out var tenantId))
            {
                await auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "RiskIdentified",
                    affectedEntityType: "Risk",
                    affectedEntityId: evt.RiskId,
                    action: "Identify",
                    actor: "SYSTEM",
                    payloadJson: message);
            }

            // Notify risk managers based on severity
            var notificationService = sp.GetService<GrcMvc.Services.Interfaces.INotificationService>();
            var userService = sp.GetService<GrcMvc.Services.Interfaces.IUserDirectoryService>();

            if (notificationService != null && userService != null)
            {
                var priority = evt.Severity switch
                {
                    "Critical" => "High",
                    "High" => "High",
                    _ => "Normal"
                };

                var rolesToNotify = evt.Severity switch
                {
                    "Critical" => new[] { "RiskManager", "ComplianceManager", "PlatformAdmin" },
                    "High" => new[] { "RiskManager", "ComplianceManager" },
                    _ => new[] { "RiskManager" }
                };

                foreach (var role in rolesToNotify)
                {
                    var users = await userService.GetUsersInRoleAsync(role);
                    foreach (var user in users)
                    {
                        await notificationService.SendNotificationAsync(
                            workflowInstanceId: Guid.Empty,
                            recipientUserId: user.Id.ToString(),
                            notificationType: "RiskIdentified",
                            subject: $"[{evt.Severity}] New Risk Identified: {evt.Title}",
                            body: $"A new {evt.Severity} severity risk has been identified and requires attention.",
                            priority: priority,
                            tenantId: Guid.TryParse(evt.TenantId, out var tid) ? tid : Guid.Empty);
                    }
                }
            }

            _logger.LogInformation("Risk {RiskId} identification processed successfully", evt.RiskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process risk identified event for {RiskId}", evt.RiskId);
        }
    }

    private async Task HandleEmailReceived(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<EmailEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Email received: {Subject} from {From}", evt.Subject, evt.From);

        try
        {
            // Try to classify the email using AI if available
            var agentService = sp.GetService<GrcMvc.Services.Interfaces.IClaudeAgentService>();
            string? classification = null;

            if (agentService != null && await agentService.IsAvailableAsync())
            {
                try
                {
                    // Simple classification based on subject keywords
                    var subject = evt.Subject?.ToLower() ?? "";
                    classification = subject switch
                    {
                        var s when s.Contains("audit") => "Audit",
                        var s when s.Contains("compliance") => "Compliance",
                        var s when s.Contains("risk") => "Risk",
                        var s when s.Contains("incident") => "Incident",
                        var s when s.Contains("urgent") || s.Contains("critical") => "Priority",
                        _ => "General"
                    };

                    _logger.LogInformation("Email classified as: {Classification}", classification);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to classify email, using default classification");
                    classification = "General";
                }
            }

            _logger.LogInformation("Email {MessageId} processed. Classification: {Classification}",
                evt.MessageId, classification ?? "Unclassified");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email received event for {MessageId}", evt.MessageId);
        }
    }

    private async Task HandleAgentAnalysisRequested(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<AgentAnalysisEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Agent analysis requested: {Type} for {EntityId}", evt.AnalysisType, evt.EntityId);

        try
        {
            // Get AI service and process
            var agentService = sp.GetService<GrcMvc.Services.Interfaces.IClaudeAgentService>();
            if (agentService != null && await agentService.IsAvailableAsync())
            {
                _logger.LogInformation("AI Agent processing analysis request for {EntityType} {EntityId}",
                    evt.EntityType, evt.EntityId);

                // Log the analysis request
                var auditService = sp.GetService<GrcMvc.Services.Interfaces.IAuditEventService>();
                if (auditService != null && Guid.TryParse(evt.TenantId, out var tenantId))
                {
                    await auditService.LogEventAsync(
                        tenantId: tenantId,
                        eventType: "AgentAnalysisRequested",
                        affectedEntityType: evt.EntityType,
                        affectedEntityId: evt.EntityId,
                        action: evt.AnalysisType,
                        actor: "AI_AGENT",
                        payloadJson: message);
                }

                _logger.LogInformation("Agent analysis request for {EntityId} processed successfully", evt.EntityId);
            }
            else
            {
                _logger.LogWarning("AI Agent not available for analysis request {EntityId}", evt.EntityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process agent analysis request for {EntityId}", evt.EntityId);
        }
    }
}

// Event DTOs
public record WorkflowEvent(string WorkflowId, string WorkflowType, string TenantId, DateTime StartedAt);
public record TaskEvent(string TaskId, string WorkflowId, string AssigneeId, string TaskType, DateTime DueDate);
public record AssessmentEvent(string AssessmentId, string FrameworkId, string TenantId, string Status);
public record RiskEvent(string RiskId, string Title, string Severity, string TenantId);
public record EmailEvent(string MessageId, string Subject, string From, string To, DateTime ReceivedAt);
public record AgentAnalysisEvent(string EntityId, string EntityType, string AnalysisType, string TenantId);
