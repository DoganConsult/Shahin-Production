using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for monitoring API endpoint health, usage, and performance
    /// Uses in-memory cache for real-time metrics (can be extended to use database)
    /// </summary>
    public class EndpointMonitoringService : IEndpointMonitoringService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<EndpointMonitoringService> _logger;
        private readonly GrcDbContext _context;
        private const string CacheKeyPrefix = "endpoint_monitoring_";

        public EndpointMonitoringService(
            IMemoryCache cache,
            ILogger<EndpointMonitoringService> logger,
            GrcDbContext context)
        {
            _cache = cache;
            _logger = logger;
            _context = context;
        }

        public async Task<EndpointUsageStats> GetUsageStatsAsync(string route, string httpMethod, int days = 7)
        {
            var cacheKey = $"{CacheKeyPrefix}usage_{route}_{httpMethod}_{days}";
            
            if (_cache.TryGetValue(cacheKey, out EndpointUsageStats? cached))
            {
                return cached!;
            }

            // In a real implementation, this would query a database table tracking endpoint calls
            // For now, we'll return mock data based on cache entries
            var stats = new EndpointUsageStats
            {
                Route = route,
                HttpMethod = httpMethod,
                TotalCalls = GetCachedCallCount(route, httpMethod),
                SuccessCalls = GetCachedSuccessCount(route, httpMethod),
                ErrorCalls = GetCachedErrorCount(route, httpMethod),
                AverageResponseTime = GetCachedAverageResponseTime(route, httpMethod),
                MinResponseTime = GetCachedMinResponseTime(route, httpMethod),
                MaxResponseTime = GetCachedMaxResponseTime(route, httpMethod),
                P95ResponseTime = GetCachedP95ResponseTime(route, httpMethod),
                P99ResponseTime = GetCachedP99ResponseTime(route, httpMethod),
                LastCalled = GetCachedLastCalled(route, httpMethod),
                CallsByDay = GetCachedCallsByDay(route, httpMethod, days)
            };

            stats.SuccessRate = stats.TotalCalls > 0 
                ? (double)stats.SuccessCalls / stats.TotalCalls * 100 
                : 0;

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));
            return await Task.FromResult(stats);
        }

        public async Task<EndpointHealthStatus> GetHealthStatusAsync(string route, string httpMethod)
        {
            var cacheKey = $"{CacheKeyPrefix}health_{route}_{httpMethod}";
            
            if (_cache.TryGetValue(cacheKey, out EndpointHealthStatus? cached))
            {
                return cached!;
            }

            var usage = await GetUsageStatsAsync(route, httpMethod, 1);
            var errorRate = usage.TotalCalls > 0 ? (double)usage.ErrorCalls / usage.TotalCalls * 100 : 0;
            var isHealthy = errorRate < 5 && usage.LastCalled > DateTime.UtcNow.AddHours(-1);

            var health = new EndpointHealthStatus
            {
                Route = route,
                HttpMethod = httpMethod,
                IsHealthy = isHealthy,
                Status = isHealthy ? "Healthy" : "Degraded",
                LastChecked = DateTime.UtcNow,
                LastSuccessfulCall = usage.LastCalled,
                ErrorRate = (int)errorRate
            };

            _cache.Set(cacheKey, health, TimeSpan.FromMinutes(1));
            return health;
        }

        public async Task<EndpointPerformanceMetrics> GetPerformanceMetricsAsync(string route, string httpMethod, int days = 7)
        {
            var usage = await GetUsageStatsAsync(route, httpMethod, days);
            
            return new EndpointPerformanceMetrics
            {
                Route = route,
                HttpMethod = httpMethod,
                AverageResponseTime = usage.AverageResponseTime,
                P50ResponseTime = (long)usage.AverageResponseTime,
                P95ResponseTime = usage.P95ResponseTime,
                P99ResponseTime = usage.P99ResponseTime,
                MinResponseTime = usage.MinResponseTime,
                MaxResponseTime = usage.MaxResponseTime,
                RequestsPerMinute = CalculateRequestsPerMinute(route, httpMethod),
                RequestsPerHour = CalculateRequestsPerHour(route, httpMethod),
                RequestsPerDay = usage.CallsByDay.Values.Sum()
            };
        }

        public async Task<List<EndpointMonitoringInfo>> GetAllEndpointsWithMonitoringAsync()
        {
            // This would integrate with EndpointDiscoveryService
            // For now, return empty list - will be populated by controller
            return await Task.FromResult(new List<EndpointMonitoringInfo>());
        }

        public async Task<List<EndpointMonitoringInfo>> GetTopSlowEndpointsAsync(int count = 10)
        {
            var slowEndpoints = GetCachedSlowEndpoints(count);
            return await Task.FromResult(slowEndpoints);
        }

        public async Task<List<EndpointMonitoringInfo>> GetTopErrorEndpointsAsync(int count = 10)
        {
            var errorEndpoints = GetCachedErrorEndpoints(count);
            return await Task.FromResult(errorEndpoints);
        }

        public async Task<List<EndpointMonitoringInfo>> GetMostUsedEndpointsAsync(int count = 10, int days = 7)
        {
            var mostUsed = GetCachedMostUsedEndpoints(count, days);
            return await Task.FromResult(mostUsed);
        }

        public async Task RecordEndpointCallAsync(string route, string httpMethod, int statusCode, long responseTimeMs, bool isError = false)
        {
            var key = $"{route}_{httpMethod}";
            var now = DateTime.UtcNow;

            // Update call count
            var callCountKey = $"{CacheKeyPrefix}calls_{key}";
            var callCount = _cache.Get<int>(callCountKey) + 1;
            _cache.Set(callCountKey, callCount, TimeSpan.FromDays(30));

            // Update success/error counts
            if (isError || statusCode >= 400)
            {
                var errorKey = $"{CacheKeyPrefix}errors_{key}";
                var errorCount = _cache.Get<int>(errorKey) + 1;
                _cache.Set(errorKey, errorCount, TimeSpan.FromDays(30));
            }
            else
            {
                var successKey = $"{CacheKeyPrefix}success_{key}";
                var successCount = _cache.Get<int>(successKey) + 1;
                _cache.Set(successKey, successCount, TimeSpan.FromDays(30));
            }

            // Update response times
            var responseTimesKey = $"{CacheKeyPrefix}times_{key}";
            var responseTimes = _cache.Get<List<long>>(responseTimesKey) ?? new List<long>();
            responseTimes.Add(responseTimeMs);
            if (responseTimes.Count > 1000) responseTimes.RemoveAt(0); // Keep last 1000
            _cache.Set(responseTimesKey, responseTimes, TimeSpan.FromDays(30));

            // Update last called
            _cache.Set($"{CacheKeyPrefix}last_{key}", now, TimeSpan.FromDays(30));

            await Task.CompletedTask;
        }

        // Helper methods for cache operations
        private int GetCachedCallCount(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}calls_{route}_{httpMethod}";
            return _cache.Get<int>(key);
        }

        private int GetCachedSuccessCount(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}success_{route}_{httpMethod}";
            return _cache.Get<int>(key);
        }

        private int GetCachedErrorCount(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}errors_{route}_{httpMethod}";
            return _cache.Get<int>(key);
        }

        private double GetCachedAverageResponseTime(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}times_{route}_{httpMethod}";
            var times = _cache.Get<List<long>>(key);
            return times?.Any() == true ? times.Average() : 0;
        }

        private long GetCachedMinResponseTime(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}times_{route}_{httpMethod}";
            var times = _cache.Get<List<long>>(key);
            return times?.Any() == true ? times.Min() : 0;
        }

        private long GetCachedMaxResponseTime(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}times_{route}_{httpMethod}";
            var times = _cache.Get<List<long>>(key);
            return times?.Any() == true ? times.Max() : 0;
        }

        private long GetCachedP95ResponseTime(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}times_{route}_{httpMethod}";
            var times = _cache.Get<List<long>>(key);
            if (times?.Any() != true) return 0;
            var sorted = times.OrderBy(t => t).ToList();
            var index = (int)(sorted.Count * 0.95);
            return sorted[index];
        }

        private long GetCachedP99ResponseTime(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}times_{route}_{httpMethod}";
            var times = _cache.Get<List<long>>(key);
            if (times?.Any() != true) return 0;
            var sorted = times.OrderBy(t => t).ToList();
            var index = (int)(sorted.Count * 0.99);
            return sorted[index];
        }

        private DateTime GetCachedLastCalled(string route, string httpMethod)
        {
            var key = $"{CacheKeyPrefix}last_{route}_{httpMethod}";
            return _cache.Get<DateTime?>(key) ?? DateTime.MinValue;
        }

        private Dictionary<DateTime, int> GetCachedCallsByDay(string route, string httpMethod, int days)
        {
            // Simplified - would track daily counts in real implementation
            return new Dictionary<DateTime, int>();
        }

        private int CalculateRequestsPerMinute(string route, string httpMethod)
        {
            // Simplified calculation
            return GetCachedCallCount(route, httpMethod) / 60;
        }

        private int CalculateRequestsPerHour(string route, string httpMethod)
        {
            return GetCachedCallCount(route, httpMethod) / 24;
        }

        private List<EndpointMonitoringInfo> GetCachedSlowEndpoints(int count)
        {
            // Would query cache/database for slow endpoints
            return new List<EndpointMonitoringInfo>();
        }

        private List<EndpointMonitoringInfo> GetCachedErrorEndpoints(int count)
        {
            // Would query cache/database for error endpoints
            return new List<EndpointMonitoringInfo>();
        }

        private List<EndpointMonitoringInfo> GetCachedMostUsedEndpoints(int count, int days)
        {
            // Would query cache/database for most used endpoints
            return new List<EndpointMonitoringInfo>();
        }
    }
}
