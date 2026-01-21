using System;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for tenant user operations
    /// Follows ABP Framework pattern: Application Services handle business logic, not controllers
    /// </summary>
    public interface ITenantUserService
    {
        /// <summary>
        /// Get tenant user by user ID (most recently activated tenant)
        /// </summary>
        Task<TenantUser?> GetTenantUserByUserIdAsync(string userId);

        /// <summary>
        /// Get tenant user by user ID and tenant ID
        /// </summary>
        Task<TenantUser?> GetTenantUserAsync(string userId, Guid tenantId);

        /// <summary>
        /// Check if user belongs to tenant
        /// </summary>
        Task<bool> UserBelongsToTenantAsync(string userId, Guid tenantId);
    }
}