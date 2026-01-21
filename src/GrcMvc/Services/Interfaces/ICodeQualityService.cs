using GrcMvc.Agents;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service interface for code quality analysis using Claude sub-agents
/// </summary>
public interface ICodeQualityService
{
    /// <summary>
    /// Analyze code using specified agent type
    /// </summary>
    Task<CodeAnalysisResult> AnalyzeCodeAsync(CodeAnalysisRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Run full code quality scan with all agents
    /// </summary>
    Task<List<CodeAnalysisResult>> RunFullScanAsync(string code, string? filePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze a file from the filesystem
    /// </summary>
    Task<CodeAnalysisResult> AnalyzeFileAsync(string filePath, string agentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze entire directory/project
    /// </summary>
    Task<List<CodeAnalysisResult>> AnalyzeDirectoryAsync(string directoryPath, string[] filePatterns, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get analysis history
    /// </summary>
    Task<List<CodeAnalysisResult>> GetAnalysisHistoryAsync(string? filePath = null, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// Get aggregated quality metrics
    /// </summary>
    Task<CodeQualityMetrics> GetQualityMetricsAsync(DateTime? from = null, DateTime? to = null);
}

/// <summary>
/// Aggregated code quality metrics
/// </summary>
public class CodeQualityMetrics
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int TotalFilesAnalyzed { get; set; }
    public int TotalIssuesFound { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }
    public double AverageQualityScore { get; set; }
    public double AverageSecurityScore { get; set; }
    public double AveragePerformanceScore { get; set; }
    public List<TrendDataPoint> QualityTrend { get; set; } = new();
    public List<string> TopIssueTypes { get; set; } = new();
    public List<FileQualitySummary> WorstFiles { get; set; } = new();
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public int IssueCount { get; set; }
}

public class FileQualitySummary
{
    public string FilePath { get; set; } = string.Empty;
    public int Score { get; set; }
    public int IssueCount { get; set; }
    public string TopIssue { get; set; } = string.Empty;
}
