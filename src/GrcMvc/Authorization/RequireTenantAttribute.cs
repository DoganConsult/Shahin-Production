using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Services.Interfaces;
using System;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization attribute that ensures tenant context is properly set before action execution.
/// Validates that user has access to the tenant and tenant context is available.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireTenantAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
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

        // Tenant context is valid, allow action to proceed
    }
}
