using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for discovering API endpoints using reflection
    /// </summary>
    public class EndpointDiscoveryService : IEndpointDiscoveryService
    {
        private readonly ILogger<EndpointDiscoveryService> _logger;
        private List<EndpointInfo>? _cachedEndpoints;

        public EndpointDiscoveryService(ILogger<EndpointDiscoveryService> logger)
        {
            _logger = logger;
        }

        public async Task<List<EndpointInfo>> GetAllEndpointsAsync()
        {
            if (_cachedEndpoints != null)
                return await Task.FromResult(_cachedEndpoints);

            _cachedEndpoints = new List<EndpointInfo>();

            try
            {
                // Get all controllers from the assembly
                var assembly = Assembly.GetExecutingAssembly();
                var controllerTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ControllerBase)) || 
                               (t.IsSubclassOf(typeof(Controller)) && t.Name.EndsWith("Controller")))
                    .Where(t => t.GetCustomAttribute<ApiControllerAttribute>() != null || 
                               t.GetCustomAttribute<RouteAttribute>() != null)
                    .ToList();

                foreach (var controllerType in controllerTypes)
                {
                    var controllerRoute = GetControllerRoute(controllerType);
                    var controllerName = controllerType.Name.Replace("Controller", "");

                    // Get all public methods that are actions
                    var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.IsPublic && 
                                   !m.IsSpecialName && 
                                   m.DeclaringType != typeof(Controller) &&
                                   m.DeclaringType != typeof(ControllerBase))
                        .ToList();

                    foreach (var method in methods)
                    {
                        var endpoint = BuildEndpointInfo(controllerType, method, controllerRoute, controllerName);
                        if (endpoint != null)
                        {
                            _cachedEndpoints.Add(endpoint);
                        }
                    }
                }

                _logger.LogInformation("Discovered {Count} API endpoints", _cachedEndpoints.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering endpoints");
            }

            return await Task.FromResult(_cachedEndpoints);
        }

        public async Task<List<EndpointInfo>> GetEndpointsByControllerAsync(string controllerName)
        {
            var allEndpoints = await GetAllEndpointsAsync();
            return allEndpoints
                .Where(e => e.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<List<EndpointInfo>> GetEndpointsByMethodAsync(string httpMethod)
        {
            var allEndpoints = await GetAllEndpointsAsync();
            return allEndpoints
                .Where(e => e.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<EndpointStatistics> GetStatisticsAsync()
        {
            var allEndpoints = await GetAllEndpointsAsync();

            return new EndpointStatistics
            {
                TotalEndpoints = allEndpoints.Count,
                GetEndpoints = allEndpoints.Count(e => e.HttpMethod == "GET"),
                PostEndpoints = allEndpoints.Count(e => e.HttpMethod == "POST"),
                PutEndpoints = allEndpoints.Count(e => e.HttpMethod == "PUT"),
                DeleteEndpoints = allEndpoints.Count(e => e.HttpMethod == "DELETE"),
                PatchEndpoints = allEndpoints.Count(e => e.HttpMethod == "PATCH"),
                AuthenticatedEndpoints = allEndpoints.Count(e => e.RequiresAuth),
                PublicEndpoints = allEndpoints.Count(e => !e.RequiresAuth),
                Controllers = allEndpoints.Select(e => e.ControllerName).Distinct().Count()
            };
        }

        private string GetControllerRoute(Type controllerType)
        {
            var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
            if (routeAttr != null && !string.IsNullOrEmpty(routeAttr.Template))
            {
                return routeAttr.Template.TrimStart('/');
            }

            var apiControllerAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
            if (apiControllerAttr != null)
            {
                // Default API route pattern
                var controllerName = controllerType.Name.Replace("Controller", "");
                return $"api/{controllerName.ToLowerInvariant()}";
            }

            return string.Empty;
        }

        private EndpointInfo? BuildEndpointInfo(Type controllerType, MethodInfo method, string controllerRoute, string controllerName)
        {
            // Get HTTP method attribute
            var httpMethod = GetHttpMethod(method);
            if (string.IsNullOrEmpty(httpMethod))
                return null; // Not an HTTP action

            // Get route
            var route = GetActionRoute(method, controllerRoute, controllerName);

            // Check authorization
            var requiresAuth = IsAuthorized(controllerType, method);
            var policy = GetPolicy(controllerType, method);
            var permissions = GetPermissions(method);

            // Get description
            var description = GetDescription(method);

            return new EndpointInfo
            {
                Route = route,
                HttpMethod = httpMethod,
                ControllerName = controllerName,
                ActionName = method.Name,
                Description = description,
                RequiresAuth = requiresAuth,
                Policy = policy,
                RequiredPermissions = permissions,
                IsProductionReady = true // Assume all endpoints are production ready
            };
        }

        private string GetHttpMethod(MethodInfo method)
        {
            if (method.GetCustomAttribute<HttpGetAttribute>() != null) return "GET";
            if (method.GetCustomAttribute<HttpPostAttribute>() != null) return "POST";
            if (method.GetCustomAttribute<HttpPutAttribute>() != null) return "PUT";
            if (method.GetCustomAttribute<HttpDeleteAttribute>() != null) return "DELETE";
            if (method.GetCustomAttribute<HttpPatchAttribute>() != null) return "PATCH";

            // Check for HttpMethod attribute (AcceptVerbs)
            var acceptVerbsAttr = method.GetCustomAttribute<Microsoft.AspNetCore.Mvc.AcceptVerbsAttribute>();
            if (acceptVerbsAttr != null && acceptVerbsAttr.HttpMethods.Any())
            {
                return acceptVerbsAttr.HttpMethods.First();
            }

            return string.Empty;
        }

        private string GetActionRoute(MethodInfo method, string controllerRoute, string controllerName)
        {
            var routeAttr = method.GetCustomAttribute<RouteAttribute>();
            if (routeAttr != null && !string.IsNullOrEmpty(routeAttr.Template))
            {
                if (routeAttr.Template.StartsWith("/"))
                    return routeAttr.Template.TrimStart('/');
                
                if (string.IsNullOrEmpty(controllerRoute))
                    return $"api/{controllerName.ToLowerInvariant()}/{routeAttr.Template}";
                
                return $"{controllerRoute}/{routeAttr.Template}";
            }

            // Default route
            if (string.IsNullOrEmpty(controllerRoute))
                return $"api/{controllerName.ToLowerInvariant()}/{method.Name.ToLowerInvariant()}";

            return $"{controllerRoute}/{method.Name.ToLowerInvariant()}";
        }

        private bool IsAuthorized(Type controllerType, MethodInfo method)
        {
            // Check method level
            if (method.GetCustomAttribute<AuthorizeAttribute>() != null)
                return true;
            
            if (method.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                return false;

            // Check controller level
            if (controllerType.GetCustomAttribute<AuthorizeAttribute>() != null)
                return true;

            return false;
        }

        private string? GetPolicy(Type controllerType, MethodInfo method)
        {
            var methodPolicy = method.GetCustomAttribute<AuthorizeAttribute>()?.Policy;
            if (!string.IsNullOrEmpty(methodPolicy))
                return methodPolicy;

            var controllerPolicy = controllerType.GetCustomAttribute<AuthorizeAttribute>()?.Policy;
            return controllerPolicy;
        }

        private List<string> GetPermissions(MethodInfo method)
        {
            var permissions = new List<string>();
            var authorizeAttrs = method.GetCustomAttributes<AuthorizeAttribute>();
            
            foreach (var attr in authorizeAttrs)
            {
                if (!string.IsNullOrEmpty(attr.Policy))
                    permissions.Add(attr.Policy);
            }

            return permissions;
        }

        private string? GetDescription(MethodInfo method)
        {
            var summaryAttr = method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            if (summaryAttr != null)
                return summaryAttr.Description;

            // Try XML documentation comment (if available)
            return null;
        }
    }
}
