using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Feature Gate Middleware
    /// Blocks access to premium features for trial/lower-tier plans
    /// </summary>
    public class FeatureGateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FeatureGateMiddleware> _logger;

        // Map of paths to required features
        private static readonly Dictionary<string, string> ProtectedPaths = new()
        {
            // Advanced Reporting
            ["/reports/executive"] = "AdvancedReporting",
            ["/reports/advanced"] = "AdvancedReporting",
            ["/api/reports/executive"] = "AdvancedReporting",
            ["/api/reports/advanced"] = "AdvancedReporting",
            
            // API Access
            ["/api/v1/"] = "ApiAccess",
            ["/api/external/"] = "ApiAccess",
            
            // SSO
            ["/sso/configure"] = "SSO",
            ["/admin/sso"] = "SSO",
            
            // Custom Branding
            ["/admin/branding"] = "CustomBranding",
            ["/settings/branding"] = "CustomBranding"
        };

        public FeatureGateMiddleware(RequestDelegate next, ILogger<FeatureGateMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUsageTrackingService usageService, ITenantContextService tenantContext)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Check if path is protected
            string? requiredFeature = null;
            foreach (var (protectedPath, feature) in ProtectedPaths)
            {
                if (path.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase))
                {
                    requiredFeature = feature;
                    break;
                }
            }

            if (requiredFeature != null)
            {
                var tenantId = tenantContext.GetCurrentTenantId();
                if (tenantId != Guid.Empty)
                {
                    var isAvailable = await usageService.IsFeatureAvailableAsync(tenantId, requiredFeature);
                    if (!isAvailable)
                    {
                        _logger.LogWarning(
                            "Feature gate blocked access to {Path} for tenant {TenantId} - requires {Feature}",
                            path, tenantId, requiredFeature);

                        // Check if it's an API request
                        if (path.StartsWith("/api/"))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new
                            {
                                error = "feature_not_available",
                                feature = requiredFeature,
                                message = $"This feature requires an upgraded plan. Please upgrade to access {requiredFeature}.",
                                upgradeUrl = "/pricing"
                            });
                            return;
                        }
                        else
                        {
                            // Redirect to upgrade page
                            context.Response.Redirect($"/pricing?feature={requiredFeature}");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Feature gate attribute for controller actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireFeatureAttribute : Attribute
    {
        public string FeatureName { get; }

        public RequireFeatureAttribute(string featureName)
        {
            FeatureName = featureName;
        }
    }

    /// <summary>
    /// Action filter for feature gating
    /// </summary>
    public class FeatureGateFilter : Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter
    {
        private readonly IUsageTrackingService _usageService;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<FeatureGateFilter> _logger;

        public FeatureGateFilter(
            IUsageTrackingService usageService,
            ITenantContextService tenantContext,
            ILogger<FeatureGateFilter> logger)
        {
            _usageService = usageService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context,
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            var requireFeatureAttributes = context.ActionDescriptor.EndpointMetadata
                .OfType<RequireFeatureAttribute>();

            foreach (var attr in requireFeatureAttributes)
            {
                var tenantId = _tenantContext.GetCurrentTenantId();
                if (tenantId != Guid.Empty)
                {
                    var isAvailable = await _usageService.IsFeatureAvailableAsync(tenantId, attr.FeatureName);
                    if (!isAvailable)
                    {
                        _logger.LogWarning(
                            "Feature gate blocked action {Action} for tenant {TenantId} - requires {Feature}",
                            context.ActionDescriptor.DisplayName, tenantId, attr.FeatureName);

                        context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult($"/pricing?feature={attr.FeatureName}");
                        return;
                    }
                }
            }

            await next();
        }
    }
}
