namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Resolves database connection string for a specific tenant
    /// Each tenant has its own isolated database
    /// </summary>
    public interface ITenantDatabaseResolver
    {
        /// <summary>
        /// Gets the connection string for a specific tenant
        /// </summary>
        string GetConnectionString(Guid tenantId);

        /// <summary>
        /// Gets the database name for a specific tenant
        /// </summary>
        string GetDatabaseName(Guid tenantId);

        /// <summary>
        /// Creates a new database for a tenant
        /// </summary>
        Task<bool> CreateTenantDatabaseAsync(Guid tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if tenant database exists
        /// </summary>
        Task<bool> DatabaseExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs migrations on tenant database
        /// </summary>
        Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tenant IDs that have databases
        /// </summary>
        Task<List<Guid>> GetTenantIdsWithDatabasesAsync(CancellationToken cancellationToken = default);
    }
}
