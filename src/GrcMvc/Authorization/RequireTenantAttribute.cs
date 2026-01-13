using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization attribute that ensures tenant context is properly set before action execution.
/// Validates that user has access to the tenant and tenant context is available.
/// CRITICAL: Also verifies user actually belongs to the requested tenant (prevents tenant hopping).
/// Uses async database operations to prevent thread pool exhaustion under load.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireTenantAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenantContextService = context.HttpContext.RequestServices.GetService<ITenantContextService>();

        if (tenantContextService == null)
        {
            context.Result = new UnauthorizedObjectResult("Tenant context service not available");
            return;
        }

        if (!tenantContextService.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult("User is not authenticated");
            return;
        }

        var tenantId = tenantContextService.GetCurrentTenantId();
        if (tenantId == Guid.Empty)
        {
            context.Result = new BadRequestObjectResult("Tenant context is required but not set");
            return;
        }

        // CRITICAL FIX: Verify user actually belongs to this tenant
        // This prevents authorization bypass where any authenticated user could access any tenant
        var userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedObjectResult("User identity not found");
            return;
        }

        // Check if user has TenantId claim matching the resolved tenant
        var tenantIdClaim = context.HttpContext.User?.FindFirstValue("TenantId");
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var claimTenantId))
        {
            if (claimTenantId == tenantId)
            {
                // Claim matches - fast path, allow access
                return;
            }
        }

        // Fallback: Verify via database that user belongs to this tenant
        var dbContext = context.HttpContext.RequestServices.GetService<GrcDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var userBelongsToTenant = await dbContext.TenantUsers
            .AsNoTracking()
            .AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && tu.Status == "Active" && !tu.IsDeleted);

        if (!userBelongsToTenant)
        {
            // User does not belong to this tenant - deny access
            context.Result = new ForbidResult();
            return;
        }

        // User verified to belong to tenant, allow action to proceed
    }
}
