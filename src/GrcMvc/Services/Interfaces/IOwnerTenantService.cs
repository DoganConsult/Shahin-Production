using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for owner tenant creation and management
    /// Uses ABP Tenant with ExtraProperties
    /// </summary>
    public interface IOwnerTenantService
    {
        /// <summary>
        /// Create a tenant with full features (bypass payment)
        /// </summary>
        Task<Tenant> CreateTenantWithFullFeaturesAsync(
            string organizationName, 
            string adminEmail, 
            string tenantSlug, 
            string ownerId,
            int expirationDays = 14);
            
        /// <summary>
        /// Generate tenant admin account with credentials
        /// </summary>
        Task<TenantAdminCredentialsDto> GenerateTenantAdminAccountAsync(
            Guid tenantId, 
            string ownerId,
            int expirationDays = 14);
            
        /// <summary>
        /// Validate tenant admin credentials (Tenant ID + Username + Password)
        /// </summary>
        Task<bool> ValidateTenantAdminCredentialsAsync(
            Guid tenantId, 
            string username, 
            string password);
            
        /// <summary>
        /// Check if credentials are expired
        /// </summary>
        Task<bool> CheckCredentialExpirationAsync(Guid tenantId);
        
        /// <summary>
        /// Extend credential expiration
        /// </summary>
        Task<bool> ExtendCredentialExpirationAsync(Guid tenantId, int additionalDays);
        
        /// <summary>
        /// Deliver credentials via specified method
        /// </summary>
        Task<bool> DeliverCredentialsAsync(Guid tenantId, string deliveryMethod, string? recipientEmail = null);
    }
}
