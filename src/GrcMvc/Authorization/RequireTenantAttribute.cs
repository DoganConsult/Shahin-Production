using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;
using GrcMvc.Exceptions;
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
            context.Result = new UnauthorizedObjectResult(new { error = "TenantContextServiceNotAvailable", message = "Tenant context service not available" });
            return;
        }

        if (!tenantContextService.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Unauthorized", message = "User is not authenticated" });
            return;
        }

        try
        {
            // Use ValidateAsync() which handles tenant existence and user authorization
            await tenantContextService.ValidateAsync(context.HttpContext.RequestAborted);
        }
        catch (TenantRequiredException ex)
        {
            context.Result = new BadRequestObjectResult(new { error = "TenantRequired", message = ex.Message });
            return;
        }
        catch (TenantForbiddenException ex)
        {
            context.Result = new ObjectResult(new { error = "TenantForbidden", message = ex.Message }) { StatusCode = 403 };
            return;
        }
        catch (InvalidOperationException ex)
        {
            // Map InvalidOperationException to appropriate HTTP status
            if (ex.Message.Contains("not found") || ex.Message.Contains("inactive"))
            {
                context.Result = new ObjectResult(new { error = "TenantForbidden", message = ex.Message }) { StatusCode = 403 };
            }
            else
            {
                context.Result = new BadRequestObjectResult(new { error = "TenantRequired", message = ex.Message });
            }
            return;
        }

        // Additional verification: Ensure user belongs to tenant (defense in depth)
        var tenantId = tenantContextService.GetCurrentTenantId();
        var userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Fast path: Check claim first
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
            if (dbContext != null)
            {
                var userBelongsToTenant = await dbContext.TenantUsers
                    .AsNoTracking()
                    .AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && tu.Status == "Active" && !tu.IsDeleted);

                if (!userBelongsToTenant)
                {
                    // User does not belong to this tenant - deny access
                    context.Result = new ObjectResult(new { error = "TenantForbidden", message = "User does not belong to this tenant" }) { StatusCode = 403 };
                    return;
                }
            }
        }

        // User verified to belong to tenant, allow action to proceed
    }
}
