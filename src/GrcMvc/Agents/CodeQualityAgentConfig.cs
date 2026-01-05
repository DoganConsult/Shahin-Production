using System.Text.Json.Serialization;

namespace GrcMvc.Agents;

/// <summary>
/// Configuration for Claude Code Quality Monitoring Sub-Agents
/// These agents analyze code quality, security, and provide alerts
/// </summary>
public static class CodeQualityAgentConfig
{
    /// <summary>
    /// Available agent types for code quality monitoring
    /// </summary>
    public static class AgentTypes
    {
        public const string CodeReviewer = "code-reviewer";
        public const string SecurityScanner = "security-scanner";
        public const string PerformanceAnalyzer = "performance-analyzer";
        public const string DependencyChecker = "dependency-checker";
        public const string TestCoverageAnalyzer = "test-coverage";
        public const string CodeStyleChecker = "code-style";
        public const string DocumentationChecker = "documentation";
        public const string ArchitectureAnalyzer = "architecture";
    }

    /// <summary>
    /// Agent prompts for different analysis types
    /// </summary>
    public static class AgentPrompts
    {
        public const string CodeReviewerPrompt = @"
You are a code review agent. Analyze the provided code for:
1. Code quality issues (complexity, readability, maintainability)
2. Potential bugs and logic errors
3. Best practices violations
4. Code smells and anti-patterns
5. Naming conventions and consistency

Return a JSON response with:
- severity: 'critical', 'high', 'medium', 'low', 'info'
- issues: array of {line, description, suggestion, severity}
- score: 0-100 quality score
- summary: brief overall assessment
";

        public const string SecurityScannerPrompt = @"
You are a security analysis agent. Scan the provided code for:
1. OWASP Top 10 vulnerabilities
2. SQL injection risks
3. XSS vulnerabilities
4. Insecure authentication patterns
5. Hardcoded secrets or credentials
6. Insecure cryptography usage
7. Input validation issues
8. Authorization bypass risks

Return a JSON response with:
- severity: 'critical', 'high', 'medium', 'low'
- vulnerabilities: array of {type, line, description, cwe, remediation}
- riskScore: 0-100 (lower is better)
- criticalCount, highCount, mediumCount, lowCount
";

        public const string PerformanceAnalyzerPrompt = @"
You are a performance analysis agent. Analyze the provided code for:
1. N+1 query problems
2. Inefficient loops and algorithms
3. Memory leaks and excessive allocations
4. Async/await misuse
5. Database query optimization opportunities
6. Caching opportunities
7. Resource disposal issues

Return a JSON response with:
- issues: array of {line, type, impact, suggestion}
- performanceScore: 0-100
- recommendations: array of optimization suggestions
";

        public const string DependencyCheckerPrompt = @"
You are a dependency analysis agent. Check the provided dependencies for:
1. Known vulnerabilities (CVEs)
2. Outdated packages
3. License compatibility issues
4. Unused dependencies
5. Conflicting versions
6. Security advisories

Return a JSON response with:
- vulnerabilities: array of {package, version, cve, severity, fixedVersion}
- outdated: array of {package, currentVersion, latestVersion}
- recommendations: array of update suggestions
";

        public const string ArchitectureAnalyzerPrompt = @"
You are an architecture analysis agent. Analyze the codebase for:
1. SOLID principle violations
2. Circular dependencies
3. Layer violations (e.g., UI calling database directly)
4. God classes and large methods
5. Coupling and cohesion issues
6. Design pattern misuse
7. Dependency injection issues

Return a JSON response with:
- violations: array of {type, location, description, recommendation}
- architectureScore: 0-100
- suggestions: array of improvement recommendations
";
    }

    /// <summary>
    /// Alert thresholds for different metrics
    /// </summary>
    public static class AlertThresholds
    {
        public const int CriticalSecurityScore = 30;
        public const int HighSecurityScore = 50;
        public const int MediumSecurityScore = 70;

        public const int CriticalQualityScore = 40;
        public const int HighQualityScore = 60;
        public const int MediumQualityScore = 75;

        public const int MaxCriticalIssues = 0;
        public const int MaxHighIssues = 3;
        public const int MaxMediumIssues = 10;
    }
}

/// <summary>
/// Code analysis request model
/// </summary>
public class CodeAnalysisRequest
{
    public string AgentType { get; set; } = CodeQualityAgentConfig.AgentTypes.CodeReviewer;
    public string Code { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? Language { get; set; }
    public string? Context { get; set; }
    public bool SendAlerts { get; set; } = true;
}

/// <summary>
/// Code analysis result model
/// </summary>
public class CodeAnalysisResult
{
    public string AgentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "info";
    public int Score { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<CodeIssue> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public bool AlertTriggered { get; set; }
    public string? AlertMessage { get; set; }
}

/// <summary>
/// Individual code issue
/// </summary>
public class CodeIssue
{
    public int? Line { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string Description { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
    public string? CweId { get; set; }
}

/// <summary>
/// Alert configuration
/// </summary>
public class AlertConfiguration
{
    public bool EnableEmailAlerts { get; set; } = true;
    public bool EnableSlackAlerts { get; set; } = false;
    public bool EnableWebhookAlerts { get; set; } = true;
    public bool EnableTeamsAlerts { get; set; } = false;

    public List<string> EmailRecipients { get; set; } = new();
    public string? SlackWebhookUrl { get; set; }
    public string? TeamsWebhookUrl { get; set; }
    public string? CustomWebhookUrl { get; set; }

    public List<string> AlertOnSeverities { get; set; } = new() { "critical", "high" };
    public int MinScoreThreshold { get; set; } = 50;
}

/// <summary>
/// Alert message model
/// </summary>
public class CodeQualityAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string AgentType { get; set; } = string.Empty;
    public int? Score { get; set; }
    public int IssueCount { get; set; }
    public List<CodeIssue> TopIssues { get; set; } = new();
    public string? RepositoryUrl { get; set; }
    public string? CommitSha { get; set; }
    public string? Branch { get; set; }
}
