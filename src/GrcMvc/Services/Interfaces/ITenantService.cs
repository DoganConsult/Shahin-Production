using System;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for multi-tenant provisioning and management.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Create a new tenant (organization).
        /// </summary>
        Task<Tenant> CreateTenantAsync(string organizationName, string adminEmail, string tenantSlug);

        /// <summary>
        /// Activate tenant after admin confirms email.
        /// </summary>
        Task<Tenant> ActivateTenantAsync(string tenantSlug, string activationToken, string activatedBy);

        /// <summary>
        /// Get tenant by slug (used in multi-tenant routing).
        /// </summary>
        Task<Tenant?> GetTenantBySlugAsync(string tenantSlug);

        /// <summary>
        /// Get tenant by ID.
        /// </summary>
        Task<Tenant?> GetTenantByIdAsync(Guid tenantId);

        /// <summary>
        /// Suspend a tenant (temporary deactivation).
        /// HIGH FIX: Missing lifecycle operation.
        /// </summary>
        Task<Tenant> SuspendTenantAsync(Guid tenantId, string suspendedBy, string? reason = null);

        /// <summary>
        /// Reactivate a suspended tenant.
        /// </summary>
        Task<Tenant> ReactivateTenantAsync(Guid tenantId, string reactivatedBy);

        /// <summary>
        /// Archive a tenant (soft delete with data retention).
        /// </summary>
        Task<Tenant> ArchiveTenantAsync(Guid tenantId, string archivedBy, string? reason = null);

        /// <summary>
        /// Permanently delete a tenant (requires admin confirmation).
        /// </summary>
        Task<bool> DeleteTenantAsync(Guid tenantId, string deletedBy, bool hardDelete = false);

        /// <summary>
        /// Resend activation email for a pending tenant.
        /// </summary>
        Task<bool> ResendActivationEmailAsync(string adminEmail);

        /// <summary>
        /// Get tenant by ID (alias for GetTenantByIdAsync).
        /// </summary>
        Task<Tenant?> GetByIdAsync(Guid tenantId);

        /// <summary>
        /// Check if a user belongs to a specific tenant.
        /// </summary>
        Task<bool> UserBelongsToTenantAsync(string userId, Guid tenantId);
    }
}
