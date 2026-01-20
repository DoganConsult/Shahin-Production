using System;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Base
{
    /// <summary>
    /// Base class for tenant-aware application services.
    /// Automatically enforces tenant context validation.
    /// Eliminates repetitive tenant validation code across services.
    /// </summary>
    public abstract class TenantAwareAppService
    {
        protected readonly ITenantContextService TenantContext;
        protected readonly ILogger Logger;

        /// <summary>
        /// Gets the current tenant ID. Throws if not available.
        /// </summary>
        protected Guid TenantId => TenantContext.GetRequiredTenantId();

        protected TenantAwareAppService(
            ITenantContextService tenantContext,
            ILogger logger)
        {
            TenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Validate tenant context at construction time
            // This ensures the service cannot be instantiated without a valid tenant context
            if (!TenantContext.HasTenantContext())
            {
                throw new InvalidOperationException(
                    $"Tenant context is required for {GetType().Name} but is not available. " +
                    "Ensure [RequireTenant] attribute is applied to the controller.");
            }
        }
    }
}
