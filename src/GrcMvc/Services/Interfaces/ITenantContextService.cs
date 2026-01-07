using System;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service to get current tenant context from authenticated user
    /// Supports multi-layer resolution: Domain/Subdomain → Claims → Database
    /// </summary>
    public interface ITenantContextService
    {
        Guid GetCurrentTenantId();
        string GetCurrentUserId();
        string GetCurrentUserName();
        bool IsAuthenticated();
        /// <summary>
        /// Returns connection string for current tenant's database
        /// </summary>
        string? GetTenantConnectionString();
        /// <summary>
        /// Checks if tenant context is available
        /// </summary>
        bool HasTenantContext();
    }
}
