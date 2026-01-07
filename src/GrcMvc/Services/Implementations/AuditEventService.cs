using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly GrcDbContext _context;
        private readonly ILogger<AuditEventService> _logger;

        // Platform tenant ID for cross-tenant audit events
        private static readonly Guid PlatformTenantId = Guid.Empty;

        public AuditEventService(
            IUnitOfWork unitOfWork,
            GrcDbContext context,
            ILogger<AuditEventService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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
            string? correlationId = null)
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

        /// <summary>
        /// Log a platform admin action (cross-tenant audit).
        /// </summary>
        public async Task LogPlatformAdminActionAsync(
            string adminUserId,
            string eventType,
            string action,
            string description,
            Guid? targetTenantId = null,
            string? targetUserId = null,
            string? ipAddress = null,
            string? payloadJson = null)
        {
            try
            {
                var payload = new
                {
                    adminUserId,
                    targetTenantId,
                    targetUserId,
                    ipAddress,
                    description,
                    timestamp = DateTime.UtcNow,
                    additionalData = payloadJson
                };

                var auditEvent = new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = $"platform-{Guid.NewGuid()}",
                    TenantId = targetTenantId ?? PlatformTenantId,
                    EventType = eventType,
                    AffectedEntityType = "Platform",
                    AffectedEntityId = targetTenantId?.ToString() ?? targetUserId ?? "PLATFORM",
                    Action = action,
                    Actor = adminUserId,
                    PayloadJson = JsonSerializer.Serialize(payload),
                    CorrelationId = $"admin-{adminUserId}",
                    Status = "Success",
                    Severity = GetSeverityForEventType(eventType),
                    EventTimestamp = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = adminUserId,
                    IpAddress = ipAddress,
                    Description = description
                };

                await _unitOfWork.AuditEvents.AddAsync(auditEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Platform admin audit: {EventType} by {Admin} - {Description}",
                    eventType, adminUserId, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging platform admin audit event");
            }
        }

        /// <summary>
        /// Get audit events with filtering.
        /// </summary>
        public async Task<List<AuditEvent>> GetFilteredEventsAsync(AuditEventFilter filter)
        {
            try
            {
                var query = _context.AuditEvents.IgnoreQueryFilters().AsQueryable();

                if (filter.TenantId.HasValue)
                    query = query.Where(e => e.TenantId == filter.TenantId.Value);

                if (!string.IsNullOrEmpty(filter.UserId))
                    query = query.Where(e => e.Actor == filter.UserId);

                if (!string.IsNullOrEmpty(filter.EventType))
                    query = query.Where(e => e.EventType == filter.EventType);

                if (!string.IsNullOrEmpty(filter.Action))
                    query = query.Where(e => e.Action == filter.Action);

                if (!string.IsNullOrEmpty(filter.AffectedEntityType))
                    query = query.Where(e => e.AffectedEntityType == filter.AffectedEntityType);

                if (!string.IsNullOrEmpty(filter.Severity))
                    query = query.Where(e => e.Severity == filter.Severity);

                if (filter.From.HasValue)
                    query = query.Where(e => e.EventTimestamp >= filter.From.Value);

                if (filter.To.HasValue)
                    query = query.Where(e => e.EventTimestamp <= filter.To.Value);

                var skip = (filter.PageNumber - 1) * filter.PageSize;

                return await query
                    .Where(e => !e.IsDeleted)
                    .OrderByDescending(e => e.EventTimestamp)
                    .Skip(skip)
                    .Take(filter.PageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered audit events");
                return new List<AuditEvent>();
            }
        }

        /// <summary>
        /// Get platform admin audit events.
        /// </summary>
        public async Task<List<AuditEvent>> GetPlatformAdminEventsAsync(
            string? adminUserId = null,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 500)
        {
            try
            {
                var query = _context.AuditEvents
                    .IgnoreQueryFilters()
                    .Where(e => e.EventType.StartsWith("PLATFORM_") && !e.IsDeleted);

                if (!string.IsNullOrEmpty(adminUserId))
                    query = query.Where(e => e.Actor == adminUserId);

                if (from.HasValue)
                    query = query.Where(e => e.EventTimestamp >= from.Value);

                if (to.HasValue)
                    query = query.Where(e => e.EventTimestamp <= to.Value);

                return await query
                    .OrderByDescending(e => e.EventTimestamp)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform admin audit events");
                return new List<AuditEvent>();
            }
        }

        /// <summary>
        /// Get audit event statistics.
        /// </summary>
        public async Task<AuditStatistics> GetStatisticsAsync(Guid? tenantId = null, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var query = _context.AuditEvents
                    .IgnoreQueryFilters()
                    .Where(e => !e.IsDeleted);

                if (tenantId.HasValue)
                    query = query.Where(e => e.TenantId == tenantId.Value);

                if (from.HasValue)
                    query = query.Where(e => e.EventTimestamp >= from.Value);

                if (to.HasValue)
                    query = query.Where(e => e.EventTimestamp <= to.Value);

                var today = DateTime.UtcNow.Date;

                var stats = new AuditStatistics
                {
                    TotalEvents = await query.CountAsync(),
                    TodayEvents = await query.CountAsync(e => e.EventTimestamp >= today),
                    ErrorEvents = await query.CountAsync(e => e.Severity == "Error" || e.Status == "Failed"),
                    WarningEvents = await query.CountAsync(e => e.Severity == "Warning")
                };

                // Get events by type (top 10)
                stats.EventsByType = await query
                    .GroupBy(e => e.EventType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToDictionaryAsync(x => x.Type, x => x.Count);

                // Get events by action (top 10)
                stats.EventsByAction = await query
                    .GroupBy(e => e.Action)
                    .Select(g => new { Action = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToDictionaryAsync(x => x.Action, x => x.Count);

                // Get top users (top 10)
                stats.TopUsers = await query
                    .Where(e => !string.IsNullOrEmpty(e.Actor) && e.Actor != "SYSTEM")
                    .GroupBy(e => e.Actor)
                    .Select(g => new { User = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToDictionaryAsync(x => x.User, x => x.Count);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating audit statistics");
                return new AuditStatistics();
            }
        }

        /// <summary>
        /// Export audit events for compliance reporting.
        /// </summary>
        public async Task<List<AuditEvent>> ExportEventsAsync(Guid tenantId, DateTime from, DateTime to)
        {
            try
            {
                return await _context.AuditEvents
                    .IgnoreQueryFilters()
                    .Where(e => e.TenantId == tenantId &&
                                e.EventTimestamp >= from &&
                                e.EventTimestamp <= to &&
                                !e.IsDeleted)
                    .OrderBy(e => e.EventTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit events");
                return new List<AuditEvent>();
            }
        }

        /// <summary>
        /// Determine severity based on event type.
        /// </summary>
        private static string GetSeverityForEventType(string eventType)
        {
            return eventType switch
            {
                PlatformAuditEventTypes.TenantDeleted => "Warning",
                PlatformAuditEventTypes.TenantSuspended => "Warning",
                PlatformAuditEventTypes.AdminSuspended => "Warning",
                PlatformAuditEventTypes.AdminDeleted => "Warning",
                PlatformAuditEventTypes.UserImpersonated => "Warning",
                PlatformAuditEventTypes.PasswordReset => "Info",
                PlatformAuditEventTypes.LoginFailed => "Warning",
                _ => "Info"
            };
        }
    }
}
