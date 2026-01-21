using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Base
{
    /// <summary>
    /// Base class for services that need tenant-aware database access
    /// Provides helper methods for common patterns
    /// </summary>
    public abstract class TenantAwareService
    {
        protected readonly IDbContextFactory<GrcDbContext> _contextFactory;

        protected TenantAwareService(IDbContextFactory<GrcDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        /// Creates a tenant-aware DbContext
        /// Automatically uses the current tenant's database
        /// </summary>
        protected GrcDbContext CreateContext()
        {
            return _contextFactory.CreateDbContext();
        }

        /// <summary>
        /// Executes an action with a tenant-aware context
        /// Automatically disposes the context
        /// </summary>
        protected async Task<T> ExecuteWithContextAsync<T>(Func<GrcDbContext, Task<T>> action)
        {
            await using var context = CreateContext();
            return await action(context);
        }

        /// <summary>
        /// Executes an action with a tenant-aware context
        /// Automatically disposes the context
        /// </summary>
        protected async Task ExecuteWithContextAsync(Func<GrcDbContext, Task> action)
        {
            await using var context = CreateContext();
            await action(context);
        }

        /// <summary>
        /// Executes an action with a tenant-aware context (synchronous)
        /// Automatically disposes the context
        /// </summary>
        protected T ExecuteWithContext<T>(Func<GrcDbContext, T> action)
        {
            using var context = CreateContext();
            return action(context);
        }
    }
}
