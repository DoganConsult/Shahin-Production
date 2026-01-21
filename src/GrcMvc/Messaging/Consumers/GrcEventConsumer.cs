using GrcMvc.Data;
using GrcMvc.Messaging.Messages;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Messaging.Consumers
{
    /// <summary>
    /// Consumer for GRC domain events - logs to audit trail and triggers webhooks
    /// </summary>
    public class GrcEventConsumer :
        IConsumer<RiskAssessedEvent>,
        IConsumer<ControlUpdatedEvent>,
        IConsumer<ComplianceStatusChangedEvent>,
        IConsumer<TaskAssignedEvent>,
        IConsumer<SlaBreachedEvent>,
        IConsumer<EvidenceSubmittedEvent>
    {
        private readonly GrcDbContext _context;
        private readonly IWebhookService _webhookService;
        private readonly ILogger<GrcEventConsumer> _logger;

        public GrcEventConsumer(
            GrcDbContext context,
            IWebhookService webhookService,
            ILogger<GrcEventConsumer> logger)
        {
            _context = context;
            _webhookService = webhookService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RiskAssessedEvent> context)
        {
            await ProcessEventAsync(context.Message, "Risk", context.Message.RiskId.ToString());
        }

        public async Task Consume(ConsumeContext<ControlUpdatedEvent> context)
        {
            await ProcessEventAsync(context.Message, "Control", context.Message.ControlId.ToString());
        }

        public async Task Consume(ConsumeContext<ComplianceStatusChangedEvent> context)
        {
            await ProcessEventAsync(context.Message, "Compliance", context.Message.FrameworkCode);
        }

        public async Task Consume(ConsumeContext<TaskAssignedEvent> context)
        {
            await ProcessEventAsync(context.Message, "Task", context.Message.TaskId.ToString());
        }

        public async Task Consume(ConsumeContext<SlaBreachedEvent> context)
        {
            await ProcessEventAsync(context.Message, "Workflow", context.Message.WorkflowId.ToString());
        }

        public async Task Consume(ConsumeContext<EvidenceSubmittedEvent> context)
        {
            await ProcessEventAsync(context.Message, "Evidence", context.Message.EvidenceId.ToString());
        }

        private async Task ProcessEventAsync(IGrcEvent @event, string entityType, string entityId)
        {
            _logger.LogInformation(
                "Processing event {EventType} ({EventId}) for {EntityType} {EntityId}",
                @event.EventType, @event.EventId, entityType, entityId);

            try
            {
                // Log to audit trail
                var auditEvent = new AuditEvent
                {
                    TenantId = @event.TenantId,
                    EventId = $"evt-{@event.EventId}",
                    EventType = @event.EventType,
                    CorrelationId = @event.EventId.ToString(),
                    AffectedEntityType = entityType,
                    AffectedEntityId = entityId,
                    Actor = "MassTransit",
                    Action = @event.EventType.Split('.').LastOrDefault() ?? "Unknown",
                    PayloadJson = JsonSerializer.Serialize(@event),
                    Status = "Success",
                    EventTimestamp = @event.Timestamp
                };

                _context.AuditEvents.Add(auditEvent);
                await _context.SaveChangesAsync();

                // Trigger webhooks for this event
                await _webhookService.TriggerEventAsync(
                    @event.TenantId,
                    @event.EventType,
                    @event.EventId.ToString(),
                    @event);

                _logger.LogDebug(
                    "Event {EventId} processed and logged to audit trail",
                    @event.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing event {EventId}: {Message}",
                    @event.EventId, ex.Message);
                throw;
            }
        }
    }
}
