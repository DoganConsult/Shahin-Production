using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.MultiTenancy;
using GrcMvc.Data;

namespace GrcMvc.Abp;

/// <summary>
/// ABP Tenant Resolver that resolves tenant by subdomain/slug.
/// Implements ABP's ITenantResolveContributor for multi-tenancy.
/// 
/// Resolution order:
/// 1. Extract subdomain from host (e.g., "acme" from "acme.grcsystem.com")
/// 2. Skip common subdomains (www, api, admin, localhost)
/// 3. Lookup tenant by TenantSlug in database
/// </summary>
public class TenantSlugResolveContributor : ITenantResolveContributor
{
    public string Name => "TenantSlug";

    public async Task ResolveAsync(ITenantResolveContext context)
    {
        var httpContext = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var host = httpContext.Request.Host.Host;
        if (string.IsNullOrEmpty(host))
        {
            return;
        }

        // Extract subdomain (e.g., "acme" from "acme.grcsystem.com")
        var parts = host.Split('.');
        if (parts.Length < 2)
        {
            return; // No subdomain
        }

        var subdomain = parts[0].ToLower();

        // Skip common subdomains (www, api, admin, etc.)
        var skipSubdomains = new[] { "www", "api", "admin", "app", "portal", "www2", "localhost" };
        if (skipSubdomains.Contains(subdomain))
        {
            return;
        }

        try
        {
            // Get DbContext to lookup tenant
            var dbContext = context.ServiceProvider.GetRequiredService<GrcDbContext>();
            var logger = context.ServiceProvider.GetService<ILogger<TenantSlugResolveContributor>>();

            // Lookup tenant by slug (subdomain matches TenantSlug)
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantSlug.ToLower() == subdomain && t.IsActive && !t.IsDeleted);

            if (tenant != null)
            {
                context.TenantIdOrName = tenant.Id.ToString();
                logger?.LogDebug("ABP resolved tenant {TenantId} from subdomain {Subdomain}", tenant.Id, subdomain);
            }
            else
            {
                logger?.LogDebug("ABP: No tenant found for subdomain {Subdomain}", subdomain);
            }
        }
        catch (Exception ex)
        {
            var logger = context.ServiceProvider.GetService<ILogger<TenantSlugResolveContributor>>();
            logger?.LogWarning(ex, "ABP tenant resolution from domain failed for host {Host}", host);
        }
    }
}
