namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for provisioning new tenant databases
    /// </summary>
    public interface ITenantProvisioningService
    {
        /// <summary>
        /// Provisions a complete tenant setup:
        /// 1. Creates tenant database
        /// 2. Runs migrations
        /// 3. Seeds initial data
        /// </summary>
        Task<bool> ProvisionTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if tenant is fully provisioned
        /// </summary>
        Task<bool> IsTenantProvisionedAsync(Guid tenantId, CancellationToken cancellationToken = default);
    }
}
