using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

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
            string? correlationId = null);

        /// <summary>
        /// Log a platform admin action (cross-tenant audit).
        /// </summary>
        Task LogPlatformAdminActionAsync(
            string adminUserId,
            string eventType,
            string action,
            string description,
            Guid? targetTenantId = null,
            string? targetUserId = null,
            string? ipAddress = null,
            string? payloadJson = null);

        /// <summary>
        /// Get audit events for a tenant.
        /// </summary>
        Task<IEnumerable<dynamic>> GetEventsByTenantAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100);

        /// <summary>
        /// Get audit events by correlation ID.
        /// </summary>
        Task<IEnumerable<dynamic>> GetEventsByCorrelationIdAsync(string correlationId);

        /// <summary>
        /// Get audit events with filtering.
        /// </summary>
        Task<List<AuditEvent>> GetFilteredEventsAsync(AuditEventFilter filter);

        /// <summary>
        /// Get platform admin audit events.
        /// </summary>
        Task<List<AuditEvent>> GetPlatformAdminEventsAsync(
            string? adminUserId = null,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 500);

        /// <summary>
        /// Get audit event statistics.
        /// </summary>
        Task<AuditStatistics> GetStatisticsAsync(Guid? tenantId = null, DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Export audit events for compliance reporting.
        /// </summary>
        Task<List<AuditEvent>> ExportEventsAsync(Guid tenantId, DateTime from, DateTime to);
    }

    /// <summary>
    /// Filter for querying audit events
    /// </summary>
    public class AuditEventFilter
    {
        public Guid? TenantId { get; set; }
        public string? UserId { get; set; }
        public string? EventType { get; set; }
        public string? Action { get; set; }
        public string? AffectedEntityType { get; set; }
        public string? Severity { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    /// <summary>
    /// Audit statistics summary
    /// </summary>
    public class AuditStatistics
    {
        public int TotalEvents { get; set; }
        public int TodayEvents { get; set; }
        public int ErrorEvents { get; set; }
        public int WarningEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsByAction { get; set; } = new();
        public Dictionary<string, int> TopUsers { get; set; } = new();
    }

    /// <summary>
    /// Platform admin audit event types
    /// </summary>
    public static class PlatformAuditEventTypes
    {
        // Tenant Management
        public const string TenantCreated = "PLATFORM_TENANT_CREATED";
        public const string TenantActivated = "PLATFORM_TENANT_ACTIVATED";
        public const string TenantSuspended = "PLATFORM_TENANT_SUSPENDED";
        public const string TenantDeleted = "PLATFORM_TENANT_DELETED";
        public const string TenantUpdated = "PLATFORM_TENANT_UPDATED";

        // User Management
        public const string UserCreated = "PLATFORM_USER_CREATED";
        public const string UserDeactivated = "PLATFORM_USER_DEACTIVATED";
        public const string UserActivated = "PLATFORM_USER_ACTIVATED";
        public const string PasswordReset = "PLATFORM_PASSWORD_RESET";
        public const string UserImpersonated = "PLATFORM_USER_IMPERSONATED";

        // Admin Management
        public const string AdminCreated = "PLATFORM_ADMIN_CREATED";
        public const string AdminUpdated = "PLATFORM_ADMIN_UPDATED";
        public const string AdminSuspended = "PLATFORM_ADMIN_SUSPENDED";
        public const string AdminReactivated = "PLATFORM_ADMIN_REACTIVATED";
        public const string AdminDeleted = "PLATFORM_ADMIN_DELETED";

        // Authentication
        public const string AdminLogin = "PLATFORM_ADMIN_LOGIN";
        public const string AdminLogout = "PLATFORM_ADMIN_LOGOUT";
        public const string LoginFailed = "PLATFORM_LOGIN_FAILED";

        // Configuration
        public const string ConfigChanged = "PLATFORM_CONFIG_CHANGED";
        public const string CatalogUpdated = "PLATFORM_CATALOG_UPDATED";
    }
}
