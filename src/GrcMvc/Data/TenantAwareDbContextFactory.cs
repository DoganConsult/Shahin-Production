using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrcMvc.Data
{
    /// <summary>
    /// Factory for creating tenant-specific DbContext instances
    /// Each tenant gets its own isolated database
    /// </summary>
    public class TenantAwareDbContextFactory : IDbContextFactory<GrcDbContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantDatabaseResolver _databaseResolver;
        private readonly ITenantContextService _tenantContext;

        public TenantAwareDbContextFactory(
            IServiceProvider serviceProvider,
            ITenantDatabaseResolver databaseResolver,
            ITenantContextService tenantContext)
        {
            _serviceProvider = serviceProvider;
            _databaseResolver = databaseResolver;
            _tenantContext = tenantContext;
        }

        public GrcDbContext CreateDbContext()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            
            if (tenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Cannot create DbContext: No tenant context available. " +
                    "Ensure user is authenticated and associated with a tenant.");
            }

            // Get connection string for tenant's database
            var connectionString = _databaseResolver.GetConnectionString(tenantId);

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<GrcDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Create context with tenant-specific connection
            return new GrcDbContext(optionsBuilder.Options);
        }
    }
}
