using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// AI-powered diagnostic agent service for analyzing application errors, conditions, and issues
/// </summary>
public interface IDiagnosticAgentService
{
    /// <summary>
    /// Analyze recent errors and provide diagnostic insights
    /// </summary>
    Task<DiagnosticReport> AnalyzeErrorsAsync(
        int? hoursBack = 24,
        string? severity = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Diagnose a specific error by ID or exception details
    /// </summary>
    Task<ErrorDiagnosis> DiagnoseErrorAsync(
        string errorId,
        string? exceptionType = null,
        string? stackTrace = null,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze application health conditions
    /// </summary>
    Task<HealthDiagnosis> AnalyzeHealthAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect patterns in recurring issues
    /// </summary>
    Task<PatternAnalysis> DetectPatternsAsync(
        int daysBack = 7,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get root cause analysis for a specific problem
    /// </summary>
    Task<RootCauseAnalysis> AnalyzeRootCauseAsync(
        string problemDescription,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get proactive recommendations based on current system state
    /// </summary>
    Task<List<DiagnosticRecommendation>> GetRecommendationsAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitor and alert on critical conditions
    /// </summary>
    Task<List<DiagnosticAlert>> MonitorConditionsAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Diagnostic report containing analyzed errors and insights
/// </summary>
public class DiagnosticReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisPeriod { get; set; }
    public int TotalErrors { get; set; }
    public int CriticalErrors { get; set; }
    public int HighErrors { get; set; }
    public int MediumErrors { get; set; }
    public int LowErrors { get; set; }
    public List<ErrorSummary> ErrorSummaries { get; set; } = new();
    public List<ErrorPattern> Patterns { get; set; } = new();
    public List<DiagnosticInsight> Insights { get; set; } = new();
    public List<DiagnosticRecommendation> Recommendations { get; set; } = new();
    public string OverallStatus { get; set; } = "Unknown";
    public int HealthScore { get; set; } // 0-100
}

/// <summary>
/// Summary of a specific error type
/// </summary>
public class ErrorSummary
{
    public string ErrorType { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    public string Severity { get; set; } = "Medium";
    public List<string> AffectedTenants { get; set; } = new();
    public string? MostCommonMessage { get; set; }
    public string? MostCommonStackTrace { get; set; }
}

/// <summary>
/// Detected pattern in errors
/// </summary>
public class ErrorPattern
{
    public string PatternType { get; set; } = string.Empty; // "recurring", "escalating", "time-based", "tenant-specific"
    public string Description { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public string Trend { get; set; } = "stable"; // "increasing", "decreasing", "stable"
    public List<string> RelatedErrors { get; set; } = new();
    public string? RootCause { get; set; }
    public string? SuggestedFix { get; set; }
}

/// <summary>
/// AI-generated diagnostic insight
/// </summary>
public class DiagnosticInsight
{
    public string Category { get; set; } = string.Empty; // "performance", "security", "reliability", "data"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public string? Evidence { get; set; }
    public string? Impact { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Error diagnosis for a specific error
/// </summary>
public class ErrorDiagnosis
{
    public string ErrorId { get; set; } = string.Empty;
    public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "Medium";
    public string ErrorType { get; set; } = string.Empty;
    public string? RootCause { get; set; }
    public string? Explanation { get; set; }
    public List<string> ContributingFactors { get; set; } = new();
    public List<FixSuggestion> FixSuggestions { get; set; } = new();
    public List<string> PreventionSteps { get; set; } = new();
    public string? RelatedCodeLocation { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

/// <summary>
/// Fix suggestion for an error
/// </summary>
public class FixSuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // "Critical", "High", "Medium", "Low"
    public string? CodeExample { get; set; }
    public string? DocumentationLink { get; set; }
    public int EstimatedEffort { get; set; } // minutes
}

/// <summary>
/// Health diagnosis
/// </summary>
public class HealthDiagnosis
{
    public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;
    public string OverallStatus { get; set; } = "Unknown";
    public int HealthScore { get; set; } // 0-100
    public List<HealthCheckResult> CheckResults { get; set; } = new();
    public List<HealthIssue> Issues { get; set; } = new();
    public List<DiagnosticRecommendation> Recommendations { get; set; } = new();
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    public string CheckName { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public string? Description { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Health issue
/// </summary>
public class HealthIssue
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Impact { get; set; }
    public string? AffectedComponent { get; set; }
}

/// <summary>
/// Pattern analysis result
/// </summary>
public class PatternAnalysis
{
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisPeriod { get; set; }
    public List<ErrorPattern> Patterns { get; set; } = new();
    public List<TrendAnalysis> Trends { get; set; } = new();
    public List<Correlation> Correlations { get; set; } = new();
    public string? Summary { get; set; }
}

/// <summary>
/// Trend analysis
/// </summary>
public class TrendAnalysis
{
    public string Metric { get; set; } = string.Empty;
    public string Trend { get; set; } = "stable"; // "increasing", "decreasing", "stable", "volatile"
    public double ChangePercentage { get; set; }
    public string? Interpretation { get; set; }
}

/// <summary>
/// Correlation between events
/// </summary>
public class Correlation
{
    public string Event1 { get; set; } = string.Empty;
    public string Event2 { get; set; } = string.Empty;
    public double CorrelationStrength { get; set; } // 0-1
    public string? Relationship { get; set; }
}

/// <summary>
/// Root cause analysis
/// </summary>
public class RootCauseAnalysis
{
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public string ProblemDescription { get; set; } = string.Empty;
    public string? RootCause { get; set; }
    public List<string> ContributingFactors { get; set; } = new();
    public List<string> Symptoms { get; set; } = new();
    public List<FixSuggestion> Fixes { get; set; } = new();
    public string Confidence { get; set; } = "Medium"; // "High", "Medium", "Low"
    public Dictionary<string, object>? Evidence { get; set; }
}

/// <summary>
/// Diagnostic recommendation
/// </summary>
public class DiagnosticRecommendation
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? Impact { get; set; }
    public int EstimatedEffort { get; set; } // minutes
    public List<string> Steps { get; set; } = new();
    public string? RelatedIssue { get; set; }
}

/// <summary>
/// Diagnostic alert
/// </summary>
public class DiagnosticAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "Medium";
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AffectedComponent { get; set; }
    public Guid? TenantId { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}
