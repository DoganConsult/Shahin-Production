using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for audit event logging (append-only event trail).
    /// Different from IAuditService which manages audit entities.
    /// </summary>
    public interface IAuditEventService
    {
        /// <summary>
        /// Log an audit event (immutable append-only).
        /// </summary>
        Task LogEventAsync(
            Guid tenantId,
            string eventType,
            string affectedEntityType,
            string affectedEntityId,
            string action,
            string actor,
            string payloadJson,
            string correlationId);

        /// <summary>
        /// Get audit events for a tenant.
        /// </summary>
        Task<IEnumerable<dynamic>> GetEventsByTenantAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100);

        /// <summary>
        /// Get audit events by correlation ID.
        /// </summary>
        Task<IEnumerable<dynamic>> GetEventsByCorrelationIdAsync(string correlationId);
    }
}
