namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for tracking migration metrics
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Track method execution (legacy vs enhanced)
    /// </summary>
    void TrackMethodCall(string methodName, string implementation, bool success, long durationMs);
    
    /// <summary>
    /// Track feature flag decision
    /// </summary>
    void TrackFeatureFlagDecision(string feature, bool useEnhanced, string userId);
    
    /// <summary>
    /// Track data consistency check result
    /// </summary>
    void TrackConsistencyCheck(string operation, bool consistent, string details);
    
    /// <summary>
    /// Get migration statistics
    /// </summary>
    Task<MigrationStatistics> GetStatisticsAsync(DateTime from, DateTime to);
}

public class MigrationStatistics
{
    public int TotalCalls { get; set; }
    public int EnhancedCalls { get; set; }
    public int LegacyCalls { get; set; }
    public double EnhancedSuccessRate { get; set; }
    public double LegacySuccessRate { get; set; }
    public double EnhancedAverageDurationMs { get; set; }
    public double LegacyAverageDurationMs { get; set; }
    public int ConsistencyChecks { get; set; }
    public int ConsistencyFailures { get; set; }
    public Dictionary<string, int> CallsByMethod { get; set; } = new();
}
