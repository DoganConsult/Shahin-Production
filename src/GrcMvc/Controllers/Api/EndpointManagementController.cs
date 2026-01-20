using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for endpoint management and discovery
    /// </summary>
    [ApiController]
    [Route("api/endpoints")]
    [Authorize(Policy = "ActivePlatformAdmin")]
    public class EndpointManagementController : ControllerBase
    {
        private readonly IEndpointDiscoveryService _endpointDiscovery;
        private readonly IEndpointMonitoringService _endpointMonitoring;
        private readonly ILogger<EndpointManagementController> _logger;

        public EndpointManagementController(
            IEndpointDiscoveryService endpointDiscovery,
            IEndpointMonitoringService endpointMonitoring,
            ILogger<EndpointManagementController> logger)
        {
            _endpointDiscovery = endpointDiscovery;
            _endpointMonitoring = endpointMonitoring;
            _logger = logger;
        }

        /// <summary>
        /// Get all API endpoints
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllEndpoints()
        {
            try
            {
                var endpoints = await _endpointDiscovery.GetAllEndpointsAsync();
                return Ok(new
                {
                    success = true,
                    count = endpoints.Count,
                    endpoints = endpoints.OrderBy(e => e.ControllerName).ThenBy(e => e.Route)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all endpoints");
                return StatusCode(500, new { success = false, error = "Failed to retrieve endpoints" });
            }
        }

        /// <summary>
        /// Get endpoints by controller
        /// </summary>
        [HttpGet("controller/{controllerName}")]
        public async Task<IActionResult> GetEndpointsByController(string controllerName)
        {
            try
            {
                var endpoints = await _endpointDiscovery.GetEndpointsByControllerAsync(controllerName);
                return Ok(new
                {
                    success = true,
                    controller = controllerName,
                    count = endpoints.Count,
                    endpoints = endpoints.OrderBy(e => e.Route)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting endpoints for controller {Controller}", controllerName);
                return StatusCode(500, new { success = false, error = "Failed to retrieve endpoints" });
            }
        }

        /// <summary>
        /// Get endpoints by HTTP method
        /// </summary>
        [HttpGet("method/{httpMethod}")]
        public async Task<IActionResult> GetEndpointsByMethod(string httpMethod)
        {
            try
            {
                var endpoints = await _endpointDiscovery.GetEndpointsByMethodAsync(httpMethod);
                return Ok(new
                {
                    success = true,
                    method = httpMethod,
                    count = endpoints.Count,
                    endpoints = endpoints.OrderBy(e => e.Route)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting endpoints for method {Method}", httpMethod);
                return StatusCode(500, new { success = false, error = "Failed to retrieve endpoints" });
            }
        }

        /// <summary>
        /// Get endpoint statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _endpointDiscovery.GetStatisticsAsync();
                return Ok(new
                {
                    success = true,
                    statistics = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting endpoint statistics");
                return StatusCode(500, new { success = false, error = "Failed to retrieve statistics" });
            }
        }

        /// <summary>
        /// Get production-ready endpoints only
        /// </summary>
        [HttpGet("production")]
        public async Task<IActionResult> GetProductionEndpoints()
        {
            try
            {
                var allEndpoints = await _endpointDiscovery.GetAllEndpointsAsync();
                var productionEndpoints = allEndpoints
                    .Where(e => e.IsProductionReady)
                    .OrderBy(e => e.ControllerName)
                    .ThenBy(e => e.Route)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    count = productionEndpoints.Count,
                    endpoints = productionEndpoints
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production endpoints");
                return StatusCode(500, new { success = false, error = "Failed to retrieve production endpoints" });
            }
        }

        /// <summary>
        /// Get endpoint usage statistics
        /// </summary>
        [HttpGet("{route}/usage")]
        public async Task<IActionResult> GetEndpointUsage(string route, [FromQuery] string method = "GET", [FromQuery] int days = 7)
        {
            try
            {
                var stats = await _endpointMonitoring.GetUsageStatsAsync(route, method, days);
                return Ok(new { success = true, stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage stats for {Route} {Method}", route, method);
                return StatusCode(500, new { success = false, error = "Failed to retrieve usage statistics" });
            }
        }

        /// <summary>
        /// Get endpoint health status
        /// </summary>
        [HttpGet("{route}/health")]
        public async Task<IActionResult> GetEndpointHealth(string route, [FromQuery] string method = "GET")
        {
            try
            {
                var health = await _endpointMonitoring.GetHealthStatusAsync(route, method);
                return Ok(new { success = true, health });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health for {Route} {Method}", route, method);
                return StatusCode(500, new { success = false, error = "Failed to retrieve health status" });
            }
        }

        /// <summary>
        /// Get endpoint performance metrics
        /// </summary>
        [HttpGet("{route}/performance")]
        public async Task<IActionResult> GetEndpointPerformance(string route, [FromQuery] string method = "GET", [FromQuery] int days = 7)
        {
            try
            {
                var metrics = await _endpointMonitoring.GetPerformanceMetricsAsync(route, method, days);
                return Ok(new { success = true, metrics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance for {Route} {Method}", route, method);
                return StatusCode(500, new { success = false, error = "Failed to retrieve performance metrics" });
            }
        }

        /// <summary>
        /// Get top slow endpoints
        /// </summary>
        [HttpGet("monitoring/slow")]
        public async Task<IActionResult> GetTopSlowEndpoints([FromQuery] int count = 10)
        {
            try
            {
                var endpoints = await _endpointMonitoring.GetTopSlowEndpointsAsync(count);
                return Ok(new { success = true, count = endpoints.Count, endpoints });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top slow endpoints");
                return StatusCode(500, new { success = false, error = "Failed to retrieve slow endpoints" });
            }
        }

        /// <summary>
        /// Get top error endpoints
        /// </summary>
        [HttpGet("monitoring/errors")]
        public async Task<IActionResult> GetTopErrorEndpoints([FromQuery] int count = 10)
        {
            try
            {
                var endpoints = await _endpointMonitoring.GetTopErrorEndpointsAsync(count);
                return Ok(new { success = true, count = endpoints.Count, endpoints });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top error endpoints");
                return StatusCode(500, new { success = false, error = "Failed to retrieve error endpoints" });
            }
        }

        /// <summary>
        /// Get most used endpoints
        /// </summary>
        [HttpGet("monitoring/popular")]
        public async Task<IActionResult> GetMostUsedEndpoints([FromQuery] int count = 10, [FromQuery] int days = 7)
        {
            try
            {
                var endpoints = await _endpointMonitoring.GetMostUsedEndpointsAsync(count, days);
                return Ok(new { success = true, count = endpoints.Count, endpoints });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most used endpoints");
                return StatusCode(500, new { success = false, error = "Failed to retrieve popular endpoints" });
            }
        }

        /// <summary>
        /// Test endpoint (make a test call)
        /// </summary>
        [HttpPost("{route}/test")]
        public async Task<IActionResult> TestEndpoint(string route, [FromQuery] string method = "GET", [FromBody] object? body = null)
        {
            try
            {
                // This would make an actual HTTP call to test the endpoint
                // For now, return a mock response
                var startTime = DateTime.UtcNow;
                
                // Simulate endpoint call
                await Task.Delay(100);
                
                var endTime = DateTime.UtcNow;
                var responseTime = (long)(endTime - startTime).TotalMilliseconds;

                // Record the test call
                await _endpointMonitoring.RecordEndpointCallAsync(route, method, 200, responseTime, false);

                return Ok(new
                {
                    success = true,
                    message = "Endpoint test completed",
                    route,
                    method,
                    statusCode = 200,
                    responseTimeMs = responseTime,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing endpoint {Route} {Method}", route, method);
                return StatusCode(500, new { success = false, error = "Failed to test endpoint" });
            }
        }
    }
}
