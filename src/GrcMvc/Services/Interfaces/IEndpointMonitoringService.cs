using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for monitoring API endpoint health, usage, and performance
    /// </summary>
    public interface IEndpointMonitoringService
    {
        /// <summary>
        /// Get endpoint usage statistics
        /// </summary>
        Task<EndpointUsageStats> GetUsageStatsAsync(string route, string httpMethod, int days = 7);

        /// <summary>
        /// Get endpoint health status
        /// </summary>
        Task<EndpointHealthStatus> GetHealthStatusAsync(string route, string httpMethod);

        /// <summary>
        /// Get endpoint performance metrics
        /// </summary>
        Task<EndpointPerformanceMetrics> GetPerformanceMetricsAsync(string route, string httpMethod, int days = 7);

        /// <summary>
        /// Get all endpoints with monitoring data
        /// </summary>
        Task<List<EndpointMonitoringInfo>> GetAllEndpointsWithMonitoringAsync();

        /// <summary>
        /// Get top slow endpoints
        /// </summary>
        Task<List<EndpointMonitoringInfo>> GetTopSlowEndpointsAsync(int count = 10);

        /// <summary>
        /// Get top error endpoints
        /// </summary>
        Task<List<EndpointMonitoringInfo>> GetTopErrorEndpointsAsync(int count = 10);

        /// <summary>
        /// Get most used endpoints
        /// </summary>
        Task<List<EndpointMonitoringInfo>> GetMostUsedEndpointsAsync(int count = 10, int days = 7);

        /// <summary>
        /// Record endpoint call
        /// </summary>
        Task RecordEndpointCallAsync(string route, string httpMethod, int statusCode, long responseTimeMs, bool isError = false);
    }

    /// <summary>
    /// Endpoint usage statistics
    /// </summary>
    public class EndpointUsageStats
    {
        public string Route { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public int TotalCalls { get; set; }
        public int SuccessCalls { get; set; }
        public int ErrorCalls { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public long P95ResponseTime { get; set; }
        public long P99ResponseTime { get; set; }
        public DateTime LastCalled { get; set; }
        public Dictionary<DateTime, int> CallsByDay { get; set; } = new();
    }

    /// <summary>
    /// Endpoint health status
    /// </summary>
    public class EndpointHealthStatus
    {
        public string Route { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = "Unknown";
        public DateTime LastChecked { get; set; }
        public DateTime LastSuccessfulCall { get; set; }
        public DateTime LastErrorCall { get; set; }
        public int ErrorRate { get; set; }
        public string? LastError { get; set; }
    }

    /// <summary>
    /// Endpoint performance metrics
    /// </summary>
    public class EndpointPerformanceMetrics
    {
        public string Route { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public double AverageResponseTime { get; set; }
        public long MedianResponseTime { get; set; }
        public long P50ResponseTime { get; set; }
        public long P95ResponseTime { get; set; }
        public long P99ResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public int RequestsPerMinute { get; set; }
        public int RequestsPerHour { get; set; }
        public int RequestsPerDay { get; set; }
    }

    /// <summary>
    /// Endpoint monitoring information
    /// </summary>
    public class EndpointMonitoringInfo
    {
        public string Route { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public int TotalCalls { get; set; }
        public int ErrorCalls { get; set; }
        public double ErrorRate { get; set; }
        public double AverageResponseTime { get; set; }
        public long P95ResponseTime { get; set; }
        public DateTime LastCalled { get; set; }
        public string Status { get; set; } = "Unknown";
    }
}
