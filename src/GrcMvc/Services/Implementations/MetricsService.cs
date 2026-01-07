using System.Collections.Concurrent;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// In-memory metrics service for migration tracking
/// For production, replace with proper metrics backend (Prometheus, AppInsights, etc.)
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;
    private readonly ConcurrentBag<MetricEntry> _metrics = new();
    
    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }
    
    public void TrackMethodCall(string methodName, string implementation, bool success, long durationMs)
    {
        var entry = new MetricEntry
        {
            Timestamp = DateTime.UtcNow,
            Type = "MethodCall",
            MethodName = methodName,
            Implementation = implementation,
            Success = success,
            DurationMs = durationMs
        };
        
        _metrics.Add(entry);
        
        _logger.LogInformation(
            "Migration Metric: {Method} via {Implementation} - {Status} ({Duration}ms)",
            methodName, implementation, success ? "Success" : "Failed", durationMs);
    }
    
    public void TrackFeatureFlagDecision(string feature, bool useEnhanced, string userId)
    {
        var entry = new MetricEntry
        {
            Timestamp = DateTime.UtcNow,
            Type = "FeatureFlag",
            Feature = feature,
            UseEnhanced = useEnhanced,
            UserId = userId
        };
        
        _metrics.Add(entry);
        
        _logger.LogDebug(
            "Feature Flag Decision: {Feature} = {Implementation} for user {UserId}",
            feature, useEnhanced ? "Enhanced" : "Legacy", userId);
    }
    
    public void TrackConsistencyCheck(string operation, bool consistent, string details)
    {
        var entry = new MetricEntry
        {
            Timestamp = DateTime.UtcNow,
            Type = "ConsistencyCheck",
            Operation = operation,
            Consistent = consistent,
            Details = details
        };
        
        _metrics.Add(entry);
        
        if (!consistent)
        {
            _logger.LogWarning(
                "Data Inconsistency Detected: {Operation} - {Details}",
                operation, details);
        }
    }
    
    public async Task<MigrationStatistics> GetStatisticsAsync(DateTime from, DateTime to)
    {
        await Task.CompletedTask; // Async for future database queries
        
        var entries = _metrics
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .ToList();
        
        var methodCalls = entries.Where(e => e.Type == "MethodCall").ToList();
        var enhancedCalls = methodCalls.Where(e => e.Implementation == "Enhanced").ToList();
        var legacyCalls = methodCalls.Where(e => e.Implementation == "Legacy").ToList();
        var consistencyChecks = entries.Where(e => e.Type == "ConsistencyCheck").ToList();
        
        return new MigrationStatistics
        {
            TotalCalls = methodCalls.Count,
            EnhancedCalls = enhancedCalls.Count,
            LegacyCalls = legacyCalls.Count,
            EnhancedSuccessRate = enhancedCalls.Any() ? enhancedCalls.Count(e => e.Success) / (double)enhancedCalls.Count * 100 : 0,
            LegacySuccessRate = legacyCalls.Any() ? legacyCalls.Count(e => e.Success) / (double)legacyCalls.Count * 100 : 0,
            EnhancedAverageDurationMs = enhancedCalls.Any() ? enhancedCalls.Average(e => e.DurationMs) : 0,
            LegacyAverageDurationMs = legacyCalls.Any() ? legacyCalls.Average(e => e.DurationMs) : 0,
            ConsistencyChecks = consistencyChecks.Count,
            ConsistencyFailures = consistencyChecks.Count(e => !e.Consistent),
            CallsByMethod = methodCalls
                .GroupBy(e => e.MethodName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
    
    private class MetricEntry
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? MethodName { get; set; }
        public string? Implementation { get; set; }
        public bool Success { get; set; }
        public long DurationMs { get; set; }
        public string? Feature { get; set; }
        public bool UseEnhanced { get; set; }
        public string? UserId { get; set; }
        public string? Operation { get; set; }
        public bool Consistent { get; set; }
        public string? Details { get; set; }
    }
}
