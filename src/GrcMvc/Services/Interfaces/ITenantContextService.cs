using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service to get current tenant context from authenticated user
    /// Supports multi-layer resolution: Domain/Subdomain → Claims → Database
    /// </summary>
    public interface ITenantContextService
    {
        Guid GetCurrentTenantId();
        
        /// <summary>
        /// Gets the current tenant ID, throwing an exception if not available.
        /// Use this in services to enforce tenant requirement.
        /// </summary>
        /// <returns>Current tenant ID</returns>
        /// <exception cref="InvalidOperationException">Thrown when tenant context is not available</exception>
        Guid GetRequiredTenantId();
        
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

        /// <summary>
        /// Validates that tenant context is available and valid.
        /// Throws exception if validation fails.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="InvalidOperationException">Thrown when tenant context is invalid or missing</exception>
        Task ValidateAsync(CancellationToken ct = default);

        /// <summary>
        /// Clear cached tenant ID (useful when user switches tenant).
        /// HIGH FIX: Added to support tenant switching without stale cache.
        /// </summary>
        void ClearTenantCache();
    }
}
