using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for audit event logging (append-only immutable event trail).
    /// Different from the existing AuditService which manages audit entities.
    /// </summary>
    public class AuditEventService : IAuditEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditEventService> _logger;

        public AuditEventService(
            IUnitOfWork unitOfWork,
            ILogger<AuditEventService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Log an audit event (immutable append-only).
        /// </summary>
        public async Task LogEventAsync(
            Guid tenantId,
            string eventType,
            string affectedEntityType,
            string affectedEntityId,
            string action,
            string actor,
            string payloadJson,
            string correlationId)
        {
            try
            {
                var auditEvent = new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = $"evt-{Guid.NewGuid()}",
                    TenantId = tenantId,
                    EventType = eventType,
                    AffectedEntityType = affectedEntityType,
                    AffectedEntityId = affectedEntityId,
                    Action = action,
                    Actor = actor,
                    PayloadJson = payloadJson,
                    CorrelationId = correlationId,
                    Status = "Success",
                    EventTimestamp = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = actor
                };

                await _unitOfWork.AuditEvents.AddAsync(auditEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Audit event logged: {eventType} for {affectedEntityType} {affectedEntityId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit event");
                // Don't throw; audit logging should not break business operations
            }
        }

        /// <summary>
        /// Get audit events for a tenant.
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetEventsByTenantAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100)
        {
            try
            {
                var skip = (pageNumber - 1) * pageSize;
                return await _unitOfWork.AuditEvents
                    .Query()
                    .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                    .OrderByDescending(e => e.EventTimestamp)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        e.Id,
                        e.EventType,
                        e.AffectedEntityType,
                        e.AffectedEntityId,
                        e.Action,
                        e.Actor,
                        e.Status,
                        e.EventTimestamp
                    })
                    .Cast<dynamic>()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit events");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Get audit events by correlation ID.
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetEventsByCorrelationIdAsync(string correlationId)
        {
            try
            {
                return await _unitOfWork.AuditEvents
                    .Query()
                    .Where(e => e.CorrelationId == correlationId && !e.IsDeleted)
                    .OrderByDescending(e => e.EventTimestamp)
                    .Select(e => new
                    {
                        e.Id,
                        e.EventType,
                        e.AffectedEntityType,
                        e.AffectedEntityId,
                        e.Action,
                        e.Actor,
                        e.Status,
                        e.EventTimestamp
                    })
                    .Cast<dynamic>()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit events by correlation ID");
                return new List<dynamic>();
            }
        }
    }
}
