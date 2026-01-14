using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Unified Claude AI Agent Service implementation
/// Provides AI-powered analysis for compliance, risk, audit, policy, and more
/// </summary>
public class ClaudeAgentService : IClaudeAgentService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<ClaudeAgentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeApiSettings _settings;
    private readonly bool _isConfigured;

    /// <summary>
    /// Patterns that may indicate prompt injection attempts
    /// </summary>
    private static readonly string[] PromptInjectionPatterns = new[]
    {
        @"ignore\s+(all\s+)?(previous|prior|above)\s+(instructions?|prompts?|context)",
        @"system:\s*you\s+are",
        @"assistant:\s*",
        @"human:\s*",
        @"<\s*system\s*>",
        @"<\s*/?\s*prompt\s*>",
        @"forget\s+(everything|all)",
        @"new\s+instructions?:",
        @"override\s+(system|instructions?)",
        @"act\s+as\s+if",
        @"pretend\s+(you\s+are|to\s+be)",
        @"jailbreak",
        @"dan\s+mode",
        @"developer\s+mode"
    };

    private static readonly Regex PromptInjectionRegex = new(
        string.Join("|", PromptInjectionPatterns),
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private const string SystemPrompt = @"You are an expert GRC (Governance, Risk, and Compliance) AI assistant for a Saudi Arabian enterprise platform.
You have deep knowledge of:
- NCA ECC (Essential Cybersecurity Controls)
- SAMA CSF (Cybersecurity Framework)
- PDPL (Personal Data Protection Law)
- ISO 27001, ISO 27701, ISO 22301
- SOC 2, PCI DSS, HIPAA (for applicable organizations)

You provide accurate, actionable insights in both English and Arabic when appropriate.
Always return responses in valid JSON format as specified in the prompts.";

    public ClaudeAgentService(
        GrcDbContext dbContext,
        ILogger<ClaudeAgentService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeApiSettings> settings)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _isConfigured = !string.IsNullOrEmpty(_settings.ApiKey);

        if (!_isConfigured)
        {
            _logger.LogWarning("Claude API key is not configured. AI agent features will be limited.");
        }
    }

    #region Input Sanitization

    /// <summary>
    /// Sanitizes user input to prevent prompt injection attacks.
    /// Returns sanitized string or throws if injection is detected.
    /// </summary>
    /// <param name="input">User-provided input</param>
    /// <param name="fieldName">Name of the field for logging</param>
    /// <param name="maxLength">Maximum allowed length (default 10000)</param>
    /// <returns>Sanitized input string</returns>
    /// <exception cref="ArgumentException">Thrown when prompt injection is detected</exception>
    private string SanitizeInput(string? input, string fieldName, int maxLength = 10000)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Truncate to max length to prevent token exhaustion
        var sanitized = input.Length > maxLength ? input[..maxLength] : input;

        // Check for prompt injection patterns
        if (DetectPromptInjection(sanitized))
        {
            _logger.LogWarning(
                "Potential prompt injection detected in field {FieldName}. Input blocked.",
                fieldName);
            throw new ArgumentException($"Invalid input detected in {fieldName}. Please remove special instructions.");
        }

        // Escape special characters that could affect prompt parsing
        sanitized = EscapePromptCharacters(sanitized);

        return sanitized;
    }

    /// <summary>
    /// Detects potential prompt injection patterns in user input
    /// </summary>
    private bool DetectPromptInjection(string input)
    {
        return PromptInjectionRegex.IsMatch(input);
    }

    /// <summary>
    /// Escapes characters that could affect prompt parsing
    /// </summary>
    private static string EscapePromptCharacters(string input)
    {
        // Replace characters that could be used to manipulate prompts
        return input
            .Replace("```", "'''")  // Prevent code block injection
            .Replace("<<", "< <")   // Prevent XML-style tags
            .Replace(">>", "> >")
            .Replace("{{", "{ {")   // Prevent template injection
            .Replace("}}", "} }");
    }

    /// <summary>
    /// Validates that input doesn't contain sensitive data patterns
    /// </summary>
    private bool ContainsSensitiveData(string input)
    {
        var sensitivePatterns = new[]
        {
            @"\b\d{3}-\d{2}-\d{4}\b",           // SSN pattern
            @"\b\d{16}\b",                       // Credit card pattern
            @"password\s*[:=]\s*\S+",           // Password pattern
            @"api[_-]?key\s*[:=]\s*\S+",        // API key pattern
            @"secret\s*[:=]\s*\S+"              // Secret pattern
        };

        foreach (var pattern in sensitivePatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isConfigured);
    }

    public async Task<ComplianceAnalysisResult> AnalyzeComplianceAsync(
        string frameworkCode,
        Guid? assessmentId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new ComplianceAnalysisResult { Success = false, Summary = "AI service not configured" };
        }

        try
        {
            // Get assessment data if provided
            var assessmentContext = "";
            if (assessmentId.HasValue)
            {
                var assessment = await _dbContext.Assessments
                    .FirstOrDefaultAsync(a => a.Id == assessmentId.Value, cancellationToken);

                if (assessment != null)
                {
                    assessmentContext = $"Assessment Status: {assessment.Status}, Score: {assessment.Score}%";
                }
            }

            var prompt = $@"
Analyze compliance status for framework: {frameworkCode}
{assessmentContext}

Provide a comprehensive compliance analysis including:
1. Overall compliance score (0-100)
2. Identified gaps with severity levels
3. Specific remediation recommendations
4. Priority actions

Return JSON:
{{
  ""complianceScore"": 75.5,
  ""gaps"": [
    {{
      ""controlId"": ""ECC-1-1"",
      ""controlName"": ""Information Security Policy"",
      ""gapDescription"": ""Policy not reviewed in last 12 months"",
      ""severity"": ""High"",
      ""remediationSuggestion"": ""Update and approve security policy"",
      ""estimatedEffortDays"": 5
    }}
  ],
  ""recommendations"": [""Establish quarterly policy review cycle"", ""Implement automated compliance monitoring""],
  ""summary"": ""Organization shows moderate compliance with {frameworkCode}...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseComplianceResult(response, frameworkCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing compliance for {FrameworkCode}", frameworkCode);
            return new ComplianceAnalysisResult { Success = false, FrameworkCode = frameworkCode, Summary = ex.Message };
        }
    }

    public async Task<RiskAnalysisResult> AnalyzeRiskAsync(
        string riskDescription,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new RiskAnalysisResult { Success = false, Analysis = "AI service not configured" };
        }

        try
        {
            // Sanitize user input to prevent prompt injection
            var sanitizedDescription = SanitizeInput(riskDescription, "riskDescription", 5000);
            var contextJson = context != null ? JsonSerializer.Serialize(context) : "No additional context";

            // Check for sensitive data in user input
            if (ContainsSensitiveData(sanitizedDescription))
            {
                _logger.LogWarning("Risk description contains potentially sensitive data patterns");
            }

            var prompt = $@"
Analyze this risk and provide assessment:

Risk Description: {sanitizedDescription}

Context: {contextJson}

Provide comprehensive risk analysis:
1. Risk score (0-100)
2. Risk level (Critical, High, Medium, Low)
3. Likelihood score (1-5)
4. Impact score (1-5)
5. Contributing risk factors
6. Mitigation strategies

Return JSON:
{{
  ""riskScore"": 65.0,
  ""riskLevel"": ""High"",
  ""likelihoodScore"": 3.5,
  ""impactScore"": 4.0,
  ""riskFactors"": [""Lack of security controls"", ""Third-party dependencies""],
  ""mitigationStrategies"": [
    ""Implement additional access controls"",
    ""Conduct vendor security assessment"",
    ""Enable real-time monitoring""
  ],
  ""analysis"": ""This risk represents a significant concern due to...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseRiskResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing risk");
            return new RiskAnalysisResult { Success = false, Analysis = ex.Message };
        }
    }

    public async Task<AuditAnalysisResult> AnalyzeAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new AuditAnalysisResult { Success = false, AuditId = auditId, Summary = "AI service not configured" };
        }

        try
        {
            var audit = await _dbContext.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId, cancellationToken);

            if (audit == null)
            {
                return new AuditAnalysisResult { Success = false, AuditId = auditId, Summary = "Audit not found" };
            }

            var prompt = $@"
Analyze this internal audit and provide insights:

Audit Title: {audit.Title}
Audit Type: {audit.Type}
Status: {audit.Status}
Scope: {audit.Scope}
Objectives: {audit.Objectives}

Provide:
1. Key findings analysis
2. Pattern identification
3. Recommendations for improvement
4. Risk areas identified

Return JSON:
{{
  ""findings"": [
    {{
      ""title"": ""Access Control Gap"",
      ""description"": ""Privileged access not properly monitored"",
      ""severity"": ""High"",
      ""impact"": ""Potential unauthorized access to sensitive data"",
      ""recommendation"": ""Implement privileged access management solution""
    }}
  ],
  ""patterns"": [""Recurring access control issues"", ""Documentation gaps""],
  ""recommendations"": [""Strengthen access governance"", ""Automate audit logging""],
  ""summary"": ""The audit revealed several areas requiring attention...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseAuditResult(response, auditId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing audit {AuditId}", auditId);
            return new AuditAnalysisResult { Success = false, AuditId = auditId, Summary = ex.Message };
        }
    }

    public async Task<PolicyAnalysisResult> AnalyzePolicyAsync(
        string policyContent,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new PolicyAnalysisResult { Success = false, Summary = "AI service not configured" };
        }

        try
        {
            var frameworkContext = frameworkCode != null ? $"Framework alignment required: {frameworkCode}" : "";

            var prompt = $@"
Analyze this policy document for quality and compliance alignment:

{frameworkContext}

Policy Content (excerpt):
{policyContent.Substring(0, Math.Min(policyContent.Length, 3000))}

Provide:
1. Quality score (0-100)
2. Issues identified with severity
3. Improvement suggestions
4. Framework alignment points

Return JSON:
{{
  ""qualityScore"": 72.0,
  ""issues"": [
    {{
      ""category"": ""Clarity"",
      ""description"": ""Policy scope needs clearer definition"",
      ""severity"": ""Medium"",
      ""suggestion"": ""Add explicit scope statement in section 1""
    }}
  ],
  ""suggestedImprovements"": [
    ""Add version control section"",
    ""Include exception handling procedure"",
    ""Define review frequency""
  ],
  ""complianceAlignments"": [
    ""Aligns with NCA ECC 1-1 requirement for security policy"",
    ""Meets PDPL Article 5 for data protection policy""
  ],
  ""summary"": ""The policy provides good foundation but requires improvements in...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParsePolicyResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing policy");
            return new PolicyAnalysisResult { Success = false, Summary = ex.Message };
        }
    }

    public async Task<AnalyticsResult> GenerateInsightsAsync(
        string dataType,
        Dictionary<string, object>? filters = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new AnalyticsResult { Success = false, DataType = dataType, Summary = "AI service not configured" };
        }

        try
        {
            // Gather relevant metrics based on data type
            var metrics = await GatherMetricsAsync(dataType, tenantId, cancellationToken);
            var metricsJson = JsonSerializer.Serialize(metrics);

            var prompt = $@"
Generate insights from this GRC data:

Data Type: {dataType}
Metrics: {metricsJson}
Filters: {(filters != null ? JsonSerializer.Serialize(filters) : "None")}

Provide:
1. Key insights with importance levels
2. Trend analysis
3. Aggregated metrics
4. Executive summary

Return JSON:
{{
  ""insights"": [
    {{
      ""title"": ""Compliance Score Improving"",
      ""description"": ""Overall compliance has improved 15% over 3 months"",
      ""category"": ""Compliance"",
      ""importance"": ""High"",
      ""actionSuggestion"": ""Continue current remediation efforts""
    }}
  ],
  ""trends"": [
    {{
      ""metric"": ""Risk Score"",
      ""direction"": ""Decreasing"",
      ""changePercentage"": -12.5,
      ""interpretation"": ""Risk exposure reducing due to control implementations""
    }}
  ],
  ""metrics"": {{
    ""totalAssessments"": 25,
    ""avgComplianceScore"": 78.5,
    ""openRisks"": 12
  }},
  ""summary"": ""Analysis shows positive trends in compliance posture...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseAnalyticsResult(response, dataType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights for {DataType}", dataType);
            return new AnalyticsResult { Success = false, DataType = dataType, Summary = ex.Message };
        }
    }

    public async Task<ReportGenerationResult> GenerateReportAsync(
        string reportType,
        Dictionary<string, object>? parameters = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new ReportGenerationResult { Success = false, ReportType = reportType, Content = "AI service not configured" };
        }

        try
        {
            var data = await GatherReportDataAsync(reportType, tenantId, cancellationToken);
            var dataJson = JsonSerializer.Serialize(data);

            var prompt = $@"
Generate a professional {reportType} report for GRC stakeholders:

Data: {dataJson}
Parameters: {(parameters != null ? JsonSerializer.Serialize(parameters) : "Standard report")}

Create:
1. Executive summary (2-3 paragraphs)
2. Key findings (bullet points)
3. Detailed content sections
4. Recommendations

Return JSON:
{{
  ""title"": ""Monthly Compliance Status Report"",
  ""executiveSummary"": ""This report presents the current compliance posture..."",
  ""content"": ""## Current Status\n\n...\n\n## Key Metrics\n\n..."",
  ""keyFindings"": [
    ""Compliance score improved to 82%"",
    ""3 high-risk items require immediate attention"",
    ""Policy review cycle on track""
  ],
  ""recommendations"": [
    ""Prioritize remediation of critical findings"",
    ""Increase automation of evidence collection"",
    ""Schedule quarterly board presentations""
  ]
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseReportResult(response, reportType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report {ReportType}", reportType);
            return new ReportGenerationResult { Success = false, ReportType = reportType, Content = ex.Message };
        }
    }

    public async Task<ChatResponse> ChatAsync(
        string message,
        List<ChatMessage>? conversationHistory = null,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new ChatResponse { Success = false, Response = "AI service not configured" };
        }

        try
        {
            var messages = new List<object>();

            // Add conversation history if provided
            if (conversationHistory != null)
            {
                foreach (var msg in conversationHistory.TakeLast(10))
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }
            }

            // Add current message with context
            var fullMessage = context != null
                ? $"Context: {context}\n\nUser Question: {message}"
                : message;

            messages.Add(new { role = "user", content = fullMessage });

            var response = await CallClaudeWithMessagesAsync(messages, cancellationToken);

            return new ChatResponse
            {
                Success = true,
                Response = response,
                ResponseAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat");
            return new ChatResponse { Success = false, Response = ex.Message };
        }
    }

    public async Task<ControlAssessmentResult> AssessControlAsync(
        Guid controlId,
        string? evidenceDescription = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new ControlAssessmentResult { Success = false, ControlId = controlId, Analysis = "AI service not configured" };
        }

        try
        {
            var prompt = $@"
Assess the effectiveness of this control:

Control ID: {controlId}
Evidence Description: {evidenceDescription ?? "No evidence provided"}

Evaluate:
1. Effectiveness rating (Effective, Partially Effective, Ineffective)
2. Effectiveness score (0-100)
3. Strengths identified
4. Weaknesses found
5. Improvement suggestions

Return JSON:
{{
  ""effectivenessRating"": ""Partially Effective"",
  ""effectivenessScore"": 65.0,
  ""strengths"": [""Clear documentation"", ""Regular review process""],
  ""weaknesses"": [""Incomplete evidence"", ""Missing metrics""],
  ""improvementSuggestions"": [
    ""Implement automated evidence collection"",
    ""Define KPIs for control effectiveness""
  ],
  ""analysis"": ""The control shows partial effectiveness due to...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseControlAssessmentResult(response, controlId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing control {ControlId}", controlId);
            return new ControlAssessmentResult { Success = false, ControlId = controlId, Analysis = ex.Message };
        }
    }

    public async Task<EvidenceAnalysisResult> AnalyzeEvidenceAsync(
        Guid evidenceId,
        string? content = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new EvidenceAnalysisResult { Success = false, EvidenceId = evidenceId, Analysis = "AI service not configured" };
        }

        try
        {
            var evidence = await _dbContext.Evidences
                .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

            var evidenceInfo = evidence != null
                ? $"Title: {evidence.Title}, Type: {evidence.Type}"
                : "Evidence details not found";

            var prompt = $@"
Analyze this evidence for quality and relevance:

{evidenceInfo}
Content/Description: {content ?? "Not provided"}

Evaluate:
1. Quality score (0-100)
2. Is evidence relevant to the control?
3. Is evidence sufficient for compliance?
4. Issues or gaps identified
5. Suggestions for improvement

Return JSON:
{{
  ""qualityScore"": 75.0,
  ""isRelevant"": true,
  ""isSufficient"": false,
  ""issues"": [""Missing date stamp"", ""No approval signature""],
  ""suggestions"": [
    ""Add timestamp to evidence"",
    ""Include approver signature or acknowledgment"",
    ""Provide supporting context""
  ],
  ""analysis"": ""The evidence demonstrates control implementation but lacks...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseEvidenceResult(response, evidenceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing evidence {EvidenceId}", evidenceId);
            return new EvidenceAnalysisResult { Success = false, EvidenceId = evidenceId, Analysis = ex.Message };
        }
    }

    public async Task<WorkflowOptimizationResult> OptimizeWorkflowAsync(
        string workflowType,
        Dictionary<string, object>? currentMetrics = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isConfigured)
        {
            return new WorkflowOptimizationResult { Success = false, WorkflowType = workflowType, Summary = "AI service not configured" };
        }

        try
        {
            var metricsJson = currentMetrics != null ? JsonSerializer.Serialize(currentMetrics) : "No metrics provided";

            var prompt = $@"
Optimize this GRC workflow:

Workflow Type: {workflowType}
Current Metrics: {metricsJson}

Provide:
1. Optimization suggestions with priority
2. Identified bottlenecks
3. Projected improvements
4. Implementation recommendations

Return JSON:
{{
  ""suggestions"": [
    {{
      ""title"": ""Automate Evidence Collection"",
      ""description"": ""Implement API integrations for automated evidence gathering"",
      ""priority"": ""High"",
      ""expectedBenefit"": ""Reduce manual effort by 60%"",
      ""estimatedEffortHours"": 40
    }}
  ],
  ""bottlenecks"": [
    {{
      ""stage"": ""Evidence Review"",
      ""description"": ""Manual review causing delays"",
      ""impact"": ""Average 5-day delay per assessment"",
      ""resolution"": ""Implement AI-assisted pre-review""
    }}
  ],
  ""projectedImprovements"": {{
    ""timeReduction"": ""45%"",
    ""costSavings"": ""30%"",
    ""qualityIncrease"": ""20%""
  }},
  ""summary"": ""The workflow can be significantly optimized by...""
}}";

            var response = await CallClaudeAsync(prompt, cancellationToken);
            return ParseWorkflowResult(response, workflowType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing workflow {WorkflowType}", workflowType);
            return new WorkflowOptimizationResult { Success = false, WorkflowType = workflowType, Summary = ex.Message };
        }
    }

    #region Private Helper Methods

    private async Task<string> CallClaudeAsync(string prompt, CancellationToken cancellationToken)
    {
        var messages = new List<object>
        {
            new { role = "user", content = prompt }
        };
        return await CallClaudeWithMessagesAsync(messages, cancellationToken);
    }

    private async Task<string> CallClaudeWithMessagesAsync(List<object> messages, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", _settings.ApiVersion);
        httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = _settings.MaxTokens,
            system = SystemPrompt,
            messages = messages
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(_settings.ApiEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return responseObj.GetProperty("content")[0].GetProperty("text").GetString() ?? "{}";
    }

    private async Task<Dictionary<string, object>> GatherMetricsAsync(
        string dataType, Guid? tenantId, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();

        switch (dataType.ToLower())
        {
            case "compliance":
                metrics["totalAssessments"] = await _dbContext.Assessments.CountAsync(cancellationToken);
                var assessments = await _dbContext.Assessments.ToListAsync(cancellationToken);
                metrics["avgScore"] = assessments.Any() ? assessments.Average(a => a.Score) : 0;
                break;
            case "risk":
                metrics["totalRisks"] = await _dbContext.Risks.CountAsync(cancellationToken);
                metrics["highRisks"] = await _dbContext.Risks
                    .CountAsync(r => r.RiskLevel == "High" || r.RiskLevel == "Critical", cancellationToken);
                break;
            case "audit":
                metrics["totalAudits"] = await _dbContext.Audits.CountAsync(cancellationToken);
                metrics["openFindings"] = await _dbContext.AuditFindings
                    .CountAsync(f => f.Status != "Closed", cancellationToken);
                break;
            default:
                metrics["dataType"] = dataType;
                break;
        }

        return metrics;
    }

    private async Task<Dictionary<string, object>> GatherReportDataAsync(
        string reportType, Guid? tenantId, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, object>
        {
            ["reportType"] = reportType,
            ["generatedAt"] = DateTime.UtcNow
        };

        // Add relevant data based on report type
        data["complianceCount"] = await _dbContext.Assessments.CountAsync(cancellationToken);
        data["riskCount"] = await _dbContext.Risks.CountAsync(cancellationToken);
        data["auditCount"] = await _dbContext.Audits.CountAsync(cancellationToken);

        return data;
    }

    private ComplianceAnalysisResult ParseComplianceResult(string response, string frameworkCode)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new ComplianceAnalysisResult
            {
                Success = true,
                FrameworkCode = frameworkCode,
                ComplianceScore = json.TryGetProperty("complianceScore", out var cs) ? cs.GetDouble() : 0,
                Gaps = json.TryGetProperty("gaps", out var gaps)
                    ? JsonSerializer.Deserialize<List<ComplianceGap>>(gaps.GetRawText()) ?? new()
                    : new(),
                Recommendations = json.TryGetProperty("recommendations", out var recs)
                    ? JsonSerializer.Deserialize<List<string>>(recs.GetRawText()) ?? new()
                    : new(),
                Summary = json.TryGetProperty("summary", out var sum) ? sum.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing compliance result");
            return new ComplianceAnalysisResult { Success = false, FrameworkCode = frameworkCode };
        }
    }

    private RiskAnalysisResult ParseRiskResult(string response)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new RiskAnalysisResult
            {
                Success = true,
                RiskScore = json.TryGetProperty("riskScore", out var rs) ? rs.GetDouble() : 0,
                RiskLevel = json.TryGetProperty("riskLevel", out var rl) ? rl.GetString() ?? "Medium" : "Medium",
                LikelihoodScore = json.TryGetProperty("likelihoodScore", out var ls) ? ls.GetDouble() : 0,
                ImpactScore = json.TryGetProperty("impactScore", out var imps) ? imps.GetDouble() : 0,
                RiskFactors = json.TryGetProperty("riskFactors", out var rf)
                    ? JsonSerializer.Deserialize<List<string>>(rf.GetRawText()) ?? new()
                    : new(),
                MitigationStrategies = json.TryGetProperty("mitigationStrategies", out var ms)
                    ? JsonSerializer.Deserialize<List<string>>(ms.GetRawText()) ?? new()
                    : new(),
                Analysis = json.TryGetProperty("analysis", out var an) ? an.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing risk result");
            return new RiskAnalysisResult { Success = false };
        }
    }

    private AuditAnalysisResult ParseAuditResult(string response, Guid auditId)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new AuditAnalysisResult
            {
                Success = true,
                AuditId = auditId,
                Findings = json.TryGetProperty("findings", out var f)
                    ? JsonSerializer.Deserialize<List<AuditFindingItem>>(f.GetRawText()) ?? new()
                    : new(),
                Patterns = json.TryGetProperty("patterns", out var p)
                    ? JsonSerializer.Deserialize<List<string>>(p.GetRawText()) ?? new()
                    : new(),
                Recommendations = json.TryGetProperty("recommendations", out var r)
                    ? JsonSerializer.Deserialize<List<string>>(r.GetRawText()) ?? new()
                    : new(),
                Summary = json.TryGetProperty("summary", out var s) ? s.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing audit result");
            return new AuditAnalysisResult { Success = false, AuditId = auditId };
        }
    }

    private PolicyAnalysisResult ParsePolicyResult(string response)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new PolicyAnalysisResult
            {
                Success = true,
                QualityScore = json.TryGetProperty("qualityScore", out var qs) ? qs.GetDouble() : 0,
                Issues = json.TryGetProperty("issues", out var i)
                    ? JsonSerializer.Deserialize<List<PolicyIssue>>(i.GetRawText()) ?? new()
                    : new(),
                SuggestedImprovements = json.TryGetProperty("suggestedImprovements", out var si)
                    ? JsonSerializer.Deserialize<List<string>>(si.GetRawText()) ?? new()
                    : new(),
                ComplianceAlignments = json.TryGetProperty("complianceAlignments", out var ca)
                    ? JsonSerializer.Deserialize<List<string>>(ca.GetRawText()) ?? new()
                    : new(),
                Summary = json.TryGetProperty("summary", out var s) ? s.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing policy result");
            return new PolicyAnalysisResult { Success = false };
        }
    }

    private AnalyticsResult ParseAnalyticsResult(string response, string dataType)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new AnalyticsResult
            {
                Success = true,
                DataType = dataType,
                Insights = json.TryGetProperty("insights", out var i)
                    ? JsonSerializer.Deserialize<List<InsightItem>>(i.GetRawText()) ?? new()
                    : new(),
                Trends = json.TryGetProperty("trends", out var t)
                    ? JsonSerializer.Deserialize<List<TrendItem>>(t.GetRawText()) ?? new()
                    : new(),
                Metrics = json.TryGetProperty("metrics", out var m)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(m.GetRawText()) ?? new()
                    : new(),
                Summary = json.TryGetProperty("summary", out var s) ? s.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing analytics result");
            return new AnalyticsResult { Success = false, DataType = dataType };
        }
    }

    private ReportGenerationResult ParseReportResult(string response, string reportType)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new ReportGenerationResult
            {
                Success = true,
                ReportType = reportType,
                Title = json.TryGetProperty("title", out var ti) ? ti.GetString() ?? "" : "",
                Content = json.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "",
                ExecutiveSummary = json.TryGetProperty("executiveSummary", out var es) ? es.GetString() : null,
                KeyFindings = json.TryGetProperty("keyFindings", out var kf)
                    ? JsonSerializer.Deserialize<List<string>>(kf.GetRawText()) ?? new()
                    : new(),
                Recommendations = json.TryGetProperty("recommendations", out var r)
                    ? JsonSerializer.Deserialize<List<string>>(r.GetRawText()) ?? new()
                    : new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing report result");
            return new ReportGenerationResult { Success = false, ReportType = reportType };
        }
    }

    private ControlAssessmentResult ParseControlAssessmentResult(string response, Guid controlId)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new ControlAssessmentResult
            {
                Success = true,
                ControlId = controlId,
                EffectivenessRating = json.TryGetProperty("effectivenessRating", out var er)
                    ? er.GetString() ?? "Partially Effective" : "Partially Effective",
                EffectivenessScore = json.TryGetProperty("effectivenessScore", out var es) ? es.GetDouble() : 0,
                Strengths = json.TryGetProperty("strengths", out var s)
                    ? JsonSerializer.Deserialize<List<string>>(s.GetRawText()) ?? new()
                    : new(),
                Weaknesses = json.TryGetProperty("weaknesses", out var w)
                    ? JsonSerializer.Deserialize<List<string>>(w.GetRawText()) ?? new()
                    : new(),
                ImprovementSuggestions = json.TryGetProperty("improvementSuggestions", out var is_)
                    ? JsonSerializer.Deserialize<List<string>>(is_.GetRawText()) ?? new()
                    : new(),
                Analysis = json.TryGetProperty("analysis", out var a) ? a.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing control assessment result");
            return new ControlAssessmentResult { Success = false, ControlId = controlId };
        }
    }

    private EvidenceAnalysisResult ParseEvidenceResult(string response, Guid evidenceId)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new EvidenceAnalysisResult
            {
                Success = true,
                EvidenceId = evidenceId,
                QualityScore = json.TryGetProperty("qualityScore", out var qs) ? qs.GetDouble() : 0,
                IsRelevant = json.TryGetProperty("isRelevant", out var ir) && ir.GetBoolean(),
                IsSufficient = json.TryGetProperty("isSufficient", out var is_) && is_.GetBoolean(),
                Issues = json.TryGetProperty("issues", out var i)
                    ? JsonSerializer.Deserialize<List<string>>(i.GetRawText()) ?? new()
                    : new(),
                Suggestions = json.TryGetProperty("suggestions", out var s)
                    ? JsonSerializer.Deserialize<List<string>>(s.GetRawText()) ?? new()
                    : new(),
                Analysis = json.TryGetProperty("analysis", out var a) ? a.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing evidence result");
            return new EvidenceAnalysisResult { Success = false, EvidenceId = evidenceId };
        }
    }

    private WorkflowOptimizationResult ParseWorkflowResult(string response, string workflowType)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new WorkflowOptimizationResult
            {
                Success = true,
                WorkflowType = workflowType,
                Suggestions = json.TryGetProperty("suggestions", out var s)
                    ? JsonSerializer.Deserialize<List<OptimizationSuggestion>>(s.GetRawText()) ?? new()
                    : new(),
                Bottlenecks = json.TryGetProperty("bottlenecks", out var b)
                    ? JsonSerializer.Deserialize<List<BottleneckItem>>(b.GetRawText()) ?? new()
                    : new(),
                ProjectedImprovements = json.TryGetProperty("projectedImprovements", out var pi)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(pi.GetRawText()) ?? new()
                    : new(),
                Summary = json.TryGetProperty("summary", out var sum) ? sum.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing workflow result");
            return new WorkflowOptimizationResult { Success = false, WorkflowType = workflowType };
        }
    }

    #endregion
}
