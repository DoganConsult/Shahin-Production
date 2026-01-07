using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Services.Interfaces;
using System;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization attribute that ensures workspace context is properly set before action execution.
/// Validates that user has access to the workspace and workspace context is available.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireWorkspaceAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// If true, workspace context is required. If false, workspace context is optional.
    /// </summary>
    public bool Required { get; set; } = true;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var workspaceContextService = context.HttpContext.RequestServices.GetService<IWorkspaceContextService>();
        
        if (workspaceContextService == null)
        {
            if (Required)
            {
                context.Result = new UnauthorizedObjectResult("Workspace context service not available");
            }
            return;
        }

        if (Required && !workspaceContextService.HasWorkspaceContext())
        {
            context.Result = new BadRequestObjectResult("Workspace context is required but not set");
            return;
        }

        if (Required)
        {
            var workspaceId = workspaceContextService.GetCurrentWorkspaceId();
            if (workspaceId == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult("Workspace context is required but invalid");
                return;
            }
        }

        // Workspace context is valid (or optional), allow action to proceed
    }
}
