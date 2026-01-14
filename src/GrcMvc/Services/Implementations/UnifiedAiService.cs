using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Unified AI Service - Multi-provider, Multi-tenant, Dynamic configuration
/// Supports: Claude, OpenAI, Azure OpenAI, Gemini, Ollama, LMStudio, Custom
/// Enhanced with: Security, Retry Logic, Caching, Token Tracking, Rate Limiting
/// </summary>
public class UnifiedAiService : IUnifiedAiService
{
    private readonly GrcDbContext _db;
    private readonly ILogger<UnifiedAiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    // Rate limiting: Track requests per tenant
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimits = new();

    // Token estimation constants (approximate)
    private const double CHARS_PER_TOKEN = 4.0;
    private const int DEFAULT_RATE_LIMIT_PER_MINUTE = 60;
    private const int DEFAULT_MAX_INPUT_LENGTH = 10000;
    private const int CACHE_DURATION_MINUTES = 15;

    #region Security - Prompt Injection Prevention

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
        @"developer\s+mode",
        @"ignore\s+safety",
        @"bypass\s+(filter|safety|restrictions?)"
    };

    private static readonly Regex PromptInjectionRegex = new(
        string.Join("|", PromptInjectionPatterns),
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Patterns for detecting sensitive data
    /// </summary>
    private static readonly (string Name, Regex Pattern)[] SensitiveDataPatterns = new[]
    {
        ("SSN", new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled)),
        ("CreditCard", new Regex(@"\b\d{16}\b|\b\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}\b", RegexOptions.Compiled)),
        ("Password", new Regex(@"password\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("ApiKey", new Regex(@"api[_-]?key\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("Secret", new Regex(@"secret\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        ("SaudiId", new Regex(@"\b[12]\d{9}\b", RegexOptions.Compiled)) // Saudi National ID pattern
    };

    #endregion

    private const string DefaultSystemPrompt = @"You are an expert GRC (Governance, Risk, and Compliance) AI assistant for a Saudi Arabian enterprise platform.
You have deep knowledge of:
- NCA ECC (Essential Cybersecurity Controls)
- SAMA CSF (Cybersecurity Framework)
- PDPL (Personal Data Protection Law)
- ISO 27001, ISO 27701, ISO 22301
- SOC 2, PCI DSS, HIPAA

You provide accurate, actionable insights in both English and Arabic.
Always return responses in valid JSON format when requested.";

    public UnifiedAiService(
        GrcDbContext db,
        ILogger<UnifiedAiService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMemoryCache cache)
    {
        _db = db;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _cache = cache;
    }

    #region Input Sanitization & Security

    /// <summary>
    /// Sanitizes user input to prevent prompt injection attacks.
    /// Returns sanitized string or throws if injection is detected.
    /// </summary>
    private string SanitizeInput(string? input, string fieldName, int maxLength = DEFAULT_MAX_INPUT_LENGTH)
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

        // Log warning if sensitive data detected (but don't block)
        var sensitiveTypes = DetectSensitiveData(sanitized);
        if (sensitiveTypes.Any())
        {
            _logger.LogWarning(
                "Sensitive data patterns detected in {FieldName}: {Types}. Consider removing before submission.",
                fieldName, string.Join(", ", sensitiveTypes));
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
    /// Detects sensitive data patterns and returns list of detected types
    /// </summary>
    private List<string> DetectSensitiveData(string input)
    {
        var detected = new List<string>();
        foreach (var (name, pattern) in SensitiveDataPatterns)
        {
            if (pattern.IsMatch(input))
            {
                detected.Add(name);
            }
        }
        return detected;
    }

    /// <summary>
    /// Escapes characters that could affect prompt parsing
    /// </summary>
    private static string EscapePromptCharacters(string input)
    {
        return input
            .Replace("```", "'''")  // Prevent code block injection
            .Replace("<<", "< <")   // Prevent XML-style tags
            .Replace(">>", "> >")
            .Replace("{{", "{ {")   // Prevent template injection
            .Replace("}}", "} }");
    }

    #endregion

    #region Rate Limiting

    /// <summary>
    /// Checks if the request should be rate limited
    /// </summary>
    private bool IsRateLimited(Guid? tenantId, out string message)
    {
        var key = tenantId?.ToString() ?? "global";
        var now = DateTime.UtcNow;
        var limitPerMinute = _configuration.GetValue("AI:RateLimitPerMinute", DEFAULT_RATE_LIMIT_PER_MINUTE);

        var entry = _rateLimits.GetOrAdd(key, _ => new RateLimitEntry());

        lock (entry)
        {
            // Reset if window expired
            if ((now - entry.WindowStart).TotalMinutes >= 1)
            {
                entry.WindowStart = now;
                entry.RequestCount = 0;
            }

            if (entry.RequestCount >= limitPerMinute)
            {
                var waitSeconds = 60 - (int)(now - entry.WindowStart).TotalSeconds;
                message = $"Rate limit exceeded. Please wait {waitSeconds} seconds.";
                _logger.LogWarning("Rate limit exceeded for tenant {TenantId}. Count: {Count}", key, entry.RequestCount);
                return true;
            }

            entry.RequestCount++;
            message = string.Empty;
            return false;
        }
    }

    private class RateLimitEntry
    {
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
        public int RequestCount { get; set; }
    }

    #endregion

    #region Token Estimation

    /// <summary>
    /// Estimates the number of tokens in a string (approximate)
    /// </summary>
    private int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return (int)Math.Ceiling(text.Length / CHARS_PER_TOKEN);
    }

    /// <summary>
    /// Estimates total tokens for a request (input + estimated output)
    /// </summary>
    private TokenEstimate EstimateRequestTokens(string message, string? systemPrompt, int maxOutputTokens)
    {
        var inputTokens = EstimateTokens(message) + EstimateTokens(systemPrompt ?? DefaultSystemPrompt);
        return new TokenEstimate
        {
            InputTokens = inputTokens,
            MaxOutputTokens = maxOutputTokens,
            TotalEstimate = inputTokens + maxOutputTokens
        };
    }

    private class TokenEstimate
    {
        public int InputTokens { get; set; }
        public int MaxOutputTokens { get; set; }
        public int TotalEstimate { get; set; }
    }

    #endregion

    #region Response Caching

    /// <summary>
    /// Generates a cache key for a prompt
    /// </summary>
    private string GenerateCacheKey(string message, string? systemPrompt, string provider, string model)
    {
        var combined = $"{provider}:{model}:{systemPrompt}:{message}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return $"ai_response_{Convert.ToHexString(hash)[..32]}";
    }

    /// <summary>
    /// Tries to get a cached response
    /// </summary>
    private bool TryGetCachedResponse(string cacheKey, out string? cachedResponse)
    {
        return _cache.TryGetValue(cacheKey, out cachedResponse);
    }

    /// <summary>
    /// Caches a response
    /// </summary>
    private void CacheResponse(string cacheKey, string response)
    {
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));
        _cache.Set(cacheKey, response, options);
    }

    #endregion

    #region Retry Logic

    /// <summary>
    /// Executes an async operation with exponential backoff retry
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        CancellationToken ct = default)
    {
        var delays = new[] { 1000, 2000, 4000 }; // Exponential backoff in ms

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (attempt < maxRetries && IsTransientError(ex))
            {
                var delay = delays[Math.Min(attempt, delays.Length - 1)];
                _logger.LogWarning(
                    "AI request failed (attempt {Attempt}/{Max}). Retrying in {Delay}ms. Error: {Error}",
                    attempt + 1, maxRetries + 1, delay, ex.Message);
                await Task.Delay(delay, ct);
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw; // Don't retry on cancellation
            }
            catch (TaskCanceledException ex) when (attempt < maxRetries)
            {
                // Timeout - retry with backoff
                var delay = delays[Math.Min(attempt, delays.Length - 1)];
                _logger.LogWarning(
                    "AI request timed out (attempt {Attempt}/{Max}). Retrying in {Delay}ms.",
                    attempt + 1, maxRetries + 1, delay);
                await Task.Delay(delay, ct);
            }
        }

        // Final attempt
        return await operation();
    }

    /// <summary>
    /// Determines if an HTTP error is transient and should be retried
    /// </summary>
    private static bool IsTransientError(HttpRequestException ex)
    {
        // Retry on 429 (rate limit), 500, 502, 503, 504
        if (ex.StatusCode.HasValue)
        {
            var code = (int)ex.StatusCode.Value;
            return code == 429 || code >= 500;
        }
        // Retry on connection errors
        return ex.InnerException is System.Net.Sockets.SocketException;
    }

    #endregion

    #region Provider Management

    public async Task<bool> IsAvailableAsync(Guid? tenantId = null, CancellationToken ct = default)
    {
        // Check database configuration first
        var hasDbConfig = await _db.Set<AiProviderConfiguration>()
            .AnyAsync(c => c.IsActive && (c.TenantId == null || c.TenantId == tenantId), ct);
        
        if (hasDbConfig) return true;
        
        // Fallback to appsettings
        var apiKey = _configuration["ClaudeAgents:ApiKey"];
        return !string.IsNullOrEmpty(apiKey);
    }

    public async Task<List<AiProviderInfo>> GetAvailableProvidersAsync(Guid? tenantId = null, CancellationToken ct = default)
    {
        var configs = await _db.Set<AiProviderConfiguration>()
            .Where(c => c.IsActive && (c.TenantId == null || c.TenantId == tenantId))
            .OrderBy(c => c.Priority)
            .ToListAsync(ct);

        var result = configs.Select(c => new AiProviderInfo
        {
            ConfigurationId = c.Id,
            Name = c.Name,
            Provider = c.Provider,
            ModelId = c.ModelId,
            IsDefault = c.IsDefault,
            IsActive = c.IsActive,
            Priority = c.Priority,
            AllowedUseCases = c.AllowedUseCases.Split(',', StringSplitOptions.RemoveEmptyEntries),
            UsageRemaining = c.MonthlyUsageLimit > 0 ? c.MonthlyUsageLimit - c.CurrentMonthUsage : null
        }).ToList();

        // Add fallback from appsettings if no DB config
        if (!result.Any())
        {
            var apiKey = _configuration["ClaudeAgents:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                result.Add(new AiProviderInfo
                {
                    ConfigurationId = Guid.Empty,
                    Name = "Default (appsettings)",
                    Provider = "Claude",
                    ModelId = _configuration["ClaudeAgents:Model"] ?? "claude-sonnet-4-20250514",
                    IsDefault = true,
                    IsActive = true,
                    Priority = 0,
                    AllowedUseCases = new[] { "all" }
                });
            }
        }

        return result;
    }

    public async Task<AiTestResult> TestProviderAsync(Guid configurationId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var config = await GetConfigurationAsync(configurationId, null, ct);
            if (config == null)
            {
                return new AiTestResult { Success = false, Error = "Configuration not found" };
            }

            var response = await CallProviderAsync(config, "Say 'Hello, I am working!' in one sentence.", null, ct);
            sw.Stop();

            return new AiTestResult
            {
                Success = true,
                Provider = config.Provider,
                Model = config.ModelId,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                Response = response
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new AiTestResult
            {
                Success = false,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    #endregion

    #region Generic AI Calls

    public async Task<AiResponse> ChatAsync(
        string message,
        string? systemPrompt = null,
        Guid? tenantId = null,
        string? preferredProvider = null,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // Rate limiting check
            if (IsRateLimited(tenantId, out var rateLimitMessage))
            {
                return new AiResponse { Success = false, Error = rateLimitMessage };
            }

            // Input sanitization
            string sanitizedMessage;
            try
            {
                sanitizedMessage = SanitizeInput(message, "message");
            }
            catch (ArgumentException ex)
            {
                return new AiResponse { Success = false, Error = ex.Message };
            }

            var config = await GetBestConfigurationAsync(tenantId, preferredProvider, "chat", ct);
            if (config == null)
            {
                return new AiResponse { Success = false, Error = "No AI provider configured" };
            }

            // Token estimation for logging/monitoring
            var tokenEstimate = EstimateRequestTokens(sanitizedMessage, systemPrompt, config.MaxTokens);
            _logger.LogDebug("AI request token estimate: Input={Input}, MaxOutput={MaxOutput}, Total={Total}",
                tokenEstimate.InputTokens, tokenEstimate.MaxOutputTokens, tokenEstimate.TotalEstimate);

            // Check cache first
            var cacheKey = GenerateCacheKey(sanitizedMessage, systemPrompt, config.Provider, config.ModelId);
            if (TryGetCachedResponse(cacheKey, out var cachedResponse) && !string.IsNullOrEmpty(cachedResponse))
            {
                sw.Stop();
                _logger.LogDebug("AI response served from cache");
                return new AiResponse
                {
                    Success = true,
                    Content = cachedResponse,
                    Provider = config.Provider,
                    Model = config.ModelId,
                    LatencyMs = (int)sw.ElapsedMilliseconds,
                    FromCache = true,
                    TokensUsed = tokenEstimate.InputTokens // Approximate
                };
            }

            // Execute with retry logic
            var response = await ExecuteWithRetryAsync(
                () => CallProviderAsync(config, sanitizedMessage, systemPrompt ?? DefaultSystemPrompt, ct),
                maxRetries: 3,
                ct);

            sw.Stop();

            // Cache the response
            CacheResponse(cacheKey, response);

            // Track usage
            await IncrementUsageAsync(config, ct);

            // Estimate output tokens
            var outputTokens = EstimateTokens(response);

            return new AiResponse
            {
                Success = true,
                Content = response,
                Provider = config.Provider,
                Model = config.ModelId,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                TokensUsed = tokenEstimate.InputTokens + outputTokens,
                FromCache = false
            };
        }
        catch (ArgumentException ex)
        {
            // Prompt injection or other input validation error
            _logger.LogWarning(ex, "Input validation failed in ChatAsync");
            return new AiResponse { Success = false, Error = ex.Message, LatencyMs = (int)sw.ElapsedMilliseconds };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ChatAsync");
            return new AiResponse { Success = false, Error = ex.Message, LatencyMs = (int)sw.ElapsedMilliseconds };
        }
    }

    public async Task<T?> PromptAsync<T>(
        string prompt,
        string? systemPrompt = null,
        Guid? tenantId = null,
        string? preferredProvider = null,
        CancellationToken ct = default) where T : class
    {
        var response = await ChatAsync(prompt, systemPrompt, tenantId, preferredProvider, ct);
        if (!response.Success || string.IsNullOrEmpty(response.Content))
            return null;

        try
        {
            // Try to extract JSON from response
            var content = response.Content;
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                content = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response as {Type}", typeof(T).Name);
            return null;
        }
    }

    public async Task<AiResponse> ConversationAsync(
        List<AiMessage> messages,
        string? systemPrompt = null,
        Guid? tenantId = null,
        string? preferredProvider = null,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var securityWarnings = new List<string>();

        try
        {
            // Rate limiting check
            if (IsRateLimited(tenantId, out var rateLimitMessage))
            {
                return new AiResponse { Success = false, Error = rateLimitMessage };
            }

            // Sanitize all user messages
            var sanitizedMessages = new List<AiMessage>();
            foreach (var msg in messages)
            {
                if (msg.Role == "user")
                {
                    try
                    {
                        var sanitizedContent = SanitizeInput(msg.Content, "message");
                        var sensitiveData = DetectSensitiveData(msg.Content);
                        if (sensitiveData.Any())
                        {
                            securityWarnings.Add($"Sensitive data detected: {string.Join(", ", sensitiveData)}");
                        }
                        sanitizedMessages.Add(new AiMessage { Role = msg.Role, Content = sanitizedContent });
                    }
                    catch (ArgumentException ex)
                    {
                        return new AiResponse { Success = false, Error = ex.Message, SecurityWarnings = securityWarnings };
                    }
                }
                else
                {
                    sanitizedMessages.Add(msg);
                }
            }

            var config = await GetBestConfigurationAsync(tenantId, preferredProvider, "chat", ct);
            if (config == null)
            {
                return new AiResponse { Success = false, Error = "No AI provider configured" };
            }

            // Execute with retry logic
            var response = await ExecuteWithRetryAsync(
                () => CallProviderWithMessagesAsync(config, sanitizedMessages, systemPrompt ?? DefaultSystemPrompt, ct),
                maxRetries: 3,
                ct);

            sw.Stop();

            await IncrementUsageAsync(config, ct);

            // Token estimation
            var inputTokens = sanitizedMessages.Sum(m => EstimateTokens(m.Content));
            var outputTokens = EstimateTokens(response);

            return new AiResponse
            {
                Success = true,
                Content = response,
                Provider = config.Provider,
                Model = config.ModelId,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TokensUsed = inputTokens + outputTokens,
                SecurityWarnings = securityWarnings.Any() ? securityWarnings : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ConversationAsync");
            return new AiResponse { Success = false, Error = ex.Message, LatencyMs = (int)sw.ElapsedMilliseconds };
        }
    }

    #endregion

    #region GRC-Specific Functions

    public async Task<ComplianceAiResult> AnalyzeComplianceAsync(
        string frameworkCode,
        Guid? assessmentId = null,
        Guid? tenantId = null,
        CancellationToken ct = default)
    {
        var assessmentContext = "";
        if (assessmentId.HasValue)
        {
            var assessment = await _db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId.Value, ct);
            if (assessment != null)
            {
                assessmentContext = $"Assessment Status: {assessment.Status}, Score: {assessment.Score}%";
            }
        }

        var prompt = $@"Analyze compliance for framework: {frameworkCode}
{assessmentContext}

Return JSON:
{{
  ""complianceScore"": 75.5,
  ""gaps"": [{{""controlId"": ""ECC-1-1"", ""controlName"": ""Security Policy"", ""gapDescription"": ""Not reviewed"", ""severity"": ""High"", ""remediation"": ""Update policy"", ""estimatedEffortDays"": 5}}],
  ""recommendations"": [""Establish review cycle""],
  ""summary"": ""Summary in English"",
  ""summaryAr"": ""ملخص بالعربية""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        var result = new ComplianceAiResult
        {
            Success = response.Success,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error,
            FrameworkCode = frameworkCode
        };

        if (response.Success)
        {
            try
            {
                var parsed = await PromptAsync<ComplianceAiResult>(prompt, null, tenantId, null, ct);
                if (parsed != null)
                {
                    result.ComplianceScore = parsed.ComplianceScore;
                    result.Gaps = parsed.Gaps;
                    result.Recommendations = parsed.Recommendations;
                    result.Summary = parsed.Summary;
                    result.SummaryAr = parsed.SummaryAr;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI compliance analysis JSON response. Using raw response. Response length: {Length}", result.Content?.Length ?? 0);
                // Use raw response as fallback
            }
        }

        return result;
    }

    public async Task<RiskAiResult> AnalyzeRiskAsync(
        string riskDescription,
        Dictionary<string, object>? context = null,
        Guid? tenantId = null,
        CancellationToken ct = default)
    {
        // Sanitize user input
        string sanitizedDescription;
        try
        {
            sanitizedDescription = SanitizeInput(riskDescription, "riskDescription", 5000);
        }
        catch (ArgumentException ex)
        {
            return new RiskAiResult { Success = false, Error = ex.Message };
        }

        var contextJson = context != null ? JsonSerializer.Serialize(context) : "{}";
        var prompt = $@"Analyze risk: {sanitizedDescription}
Context: {contextJson}

Return JSON:
{{
  ""riskScore"": 65.0,
  ""riskLevel"": ""High"",
  ""likelihoodScore"": 3.5,
  ""impactScore"": 4.0,
  ""riskFactors"": [""Factor 1""],
  ""mitigationStrategies"": [""Strategy 1""],
  ""analysis"": ""Analysis in English"",
  ""analysisAr"": ""تحليل بالعربية""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        var result = new RiskAiResult
        {
            Success = response.Success,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };

        if (response.Success)
        {
            try
            {
                var parsed = await PromptAsync<RiskAiResult>(prompt, null, tenantId, null, ct);
                if (parsed != null)
                {
                    result.RiskScore = parsed.RiskScore;
                    result.RiskLevel = parsed.RiskLevel;
                    result.LikelihoodScore = parsed.LikelihoodScore;
                    result.ImpactScore = parsed.ImpactScore;
                    result.RiskFactors = parsed.RiskFactors;
                    result.MitigationStrategies = parsed.MitigationStrategies;
                    result.Analysis = parsed.Analysis;
                    result.AnalysisAr = parsed.AnalysisAr;
                }
            }
            catch { }
        }

        return result;
    }

    public async Task<AuditAiResult> AnalyzeAuditAsync(Guid auditId, Guid? tenantId = null, CancellationToken ct = default)
    {
        var audit = await _db.Audits.FirstOrDefaultAsync(a => a.Id == auditId, ct);
        if (audit == null)
        {
            return new AuditAiResult { Success = false, AuditId = auditId, Error = "Audit not found" };
        }

        var prompt = $@"Analyze audit: {audit.Title}, Type: {audit.Type}, Status: {audit.Status}

Return JSON:
{{
  ""findings"": [{{""title"": ""Finding"", ""description"": ""Desc"", ""severity"": ""High"", ""impact"": ""Impact"", ""recommendation"": ""Rec""}}],
  ""patterns"": [""Pattern 1""],
  ""recommendations"": [""Rec 1""],
  ""summary"": ""Summary""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        return new AuditAiResult
        {
            Success = response.Success,
            AuditId = auditId,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };
    }

    public async Task<PolicyAiResult> AnalyzePolicyAsync(string policyContent, string? frameworkCode = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Sanitize user input (allow larger content for policies)
        string sanitizedContent;
        try
        {
            sanitizedContent = SanitizeInput(policyContent, "policyContent", 15000);
        }
        catch (ArgumentException ex)
        {
            return new PolicyAiResult { Success = false, Error = ex.Message };
        }

        var prompt = $@"Analyze policy quality{(frameworkCode != null ? $" for {frameworkCode}" : "")}:
{sanitizedContent.Substring(0, Math.Min(sanitizedContent.Length, 2000))}

Return JSON:
{{
  ""qualityScore"": 72.0,
  ""issues"": [{{""category"": ""Clarity"", ""description"": ""Issue"", ""severity"": ""Medium"", ""suggestion"": ""Fix""}}],
  ""improvements"": [""Improvement 1""],
  ""frameworkAlignments"": [""Aligns with X""],
  ""summary"": ""Summary""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        return new PolicyAiResult
        {
            Success = response.Success,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };
    }

    public async Task<InsightsAiResult> GenerateInsightsAsync(string dataType, Dictionary<string, object>? data = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        var dataJson = data != null ? JsonSerializer.Serialize(data) : "{}";
        var prompt = $@"Generate insights for {dataType}:
Data: {dataJson}

Return JSON:
{{
  ""insights"": [{{""title"": ""Insight"", ""description"": ""Desc"", ""category"": ""Cat"", ""importance"": ""High"", ""actionSuggestion"": ""Action""}}],
  ""trends"": [{{""metric"": ""Metric"", ""direction"": ""Up"", ""changePercentage"": 10.5, ""interpretation"": ""Interp""}}],
  ""metrics"": {{}},
  ""summary"": ""Summary""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        return new InsightsAiResult
        {
            Success = response.Success,
            DataType = dataType,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };
    }

    public async Task<ReportAiResult> GenerateReportAsync(string reportType, Dictionary<string, object>? parameters = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        var paramsJson = parameters != null ? JsonSerializer.Serialize(parameters) : "{}";
        var prompt = $@"Generate {reportType} report:
Parameters: {paramsJson}

Return JSON:
{{
  ""title"": ""Report Title"",
  ""executiveSummary"": ""Executive summary..."",
  ""keyFindings"": [""Finding 1""],
  ""recommendations"": [""Recommendation 1""]
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        return new ReportAiResult
        {
            Success = response.Success,
            ReportType = reportType,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };
    }

    public async Task<ControlAiResult> AssessControlAsync(Guid controlId, string? evidenceDescription = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Sanitize optional user input
        string? sanitizedEvidence = null;
        if (!string.IsNullOrEmpty(evidenceDescription))
        {
            try
            {
                sanitizedEvidence = SanitizeInput(evidenceDescription, "evidenceDescription", 5000);
            }
            catch (ArgumentException ex)
            {
                return new ControlAiResult { Success = false, ControlId = controlId, Error = ex.Message };
            }
        }

        var prompt = $@"Assess control {controlId}:
Evidence: {sanitizedEvidence ?? "None provided"}

Return JSON:
{{
  ""effectivenessRating"": ""Partially Effective"",
  ""effectivenessScore"": 65.0,
  ""strengths"": [""Strength 1""],
  ""weaknesses"": [""Weakness 1""],
  ""improvements"": [""Improvement 1""],
  ""analysis"": ""Analysis""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        return new ControlAiResult
        {
            Success = response.Success,
            ControlId = controlId,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };
    }

    public async Task<EvidenceAiResult> AnalyzeEvidenceAsync(Guid evidenceId, string? content = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Sanitize optional user-provided content
        string? sanitizedContent = null;
        if (!string.IsNullOrEmpty(content))
        {
            try
            {
                sanitizedContent = SanitizeInput(content, "content", 8000);
            }
            catch (ArgumentException ex)
            {
                return new EvidenceAiResult { Success = false, EvidenceId = evidenceId, Error = ex.Message };
            }
        }

        var evidence = await _db.Evidences.FirstOrDefaultAsync(e => e.Id == evidenceId, ct);
        var evidenceInfo = evidence != null ? $"Title: {evidence.Title}, Type: {evidence.Type}" : "Not found";

        var prompt = $@"Analyze evidence quality:
{evidenceInfo}
Content: {sanitizedContent ?? "Not provided"}

Return JSON:
{{
  ""qualityScore"": 75.0,
  ""isRelevant"": true,
  ""isSufficient"": false,
  ""issues"": [""Issue 1""],
  ""suggestions"": [""Suggestion 1""],
  ""analysis"": ""Analysis""
}}";

        var response = await ChatAsync(prompt, null, tenantId, null, ct);
        return new EvidenceAiResult
        {
            Success = response.Success,
            EvidenceId = evidenceId,
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            LatencyMs = response.LatencyMs,
            Error = response.Error
        };
    }

    public async Task<AiResponse> ArabicAssistantAsync(string query, string? context = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Sanitize user input
        string sanitizedQuery;
        string? sanitizedContext = null;
        try
        {
            sanitizedQuery = SanitizeInput(query, "query");
            if (context != null)
            {
                sanitizedContext = SanitizeInput(context, "context", 5000);
            }
        }
        catch (ArgumentException ex)
        {
            return new AiResponse { Success = false, Error = ex.Message };
        }

        var arabicSystemPrompt = @"أنت مساعد ذكي متخصص في الحوكمة والمخاطر والامتثال للمؤسسات السعودية.
لديك معرفة عميقة بـ:
- ضوابط الأمن السيبراني الأساسية (NCA ECC)
- إطار الأمن السيبراني للبنك المركزي (SAMA CSF)
- نظام حماية البيانات الشخصية (PDPL)
- المعايير الدولية ISO 27001, ISO 27701

قدم إجابات دقيقة وعملية باللغة العربية.";

        var fullQuery = sanitizedContext != null ? $"السياق: {sanitizedContext}\n\nالسؤال: {sanitizedQuery}" : sanitizedQuery;
        return await ChatAsync(fullQuery, arabicSystemPrompt, tenantId, null, ct);
    }

    #endregion

    #region Provider-Specific Implementations

    private async Task<string> CallProviderAsync(AiProviderConfiguration config, string message, string? systemPrompt, CancellationToken ct)
    {
        var messages = new List<AiMessage> { new() { Role = "user", Content = message } };
        return await CallProviderWithMessagesAsync(config, messages, systemPrompt, ct);
    }

    private async Task<string> CallProviderWithMessagesAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        return config.Provider switch
        {
            AiProviders.Claude => await CallClaudeAsync(config, messages, systemPrompt, ct),
            AiProviders.OpenAI => await CallOpenAiAsync(config, messages, systemPrompt, ct),
            AiProviders.AzureOpenAI => await CallAzureOpenAiAsync(config, messages, systemPrompt, ct),
            AiProviders.Gemini => await CallGeminiAsync(config, messages, systemPrompt, ct),
            AiProviders.Ollama or AiProviders.LMStudio => await CallLocalLlmAsync(config, messages, systemPrompt, ct),
            AiProviders.Custom => await CallCustomAsync(config, messages, systemPrompt, ct),
            _ => throw new NotSupportedException($"Provider {config.Provider} not supported")
        };
    }

    private async Task<string> CallClaudeAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", config.ApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", config.ApiVersion ?? "2023-06-01");
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        var endpoint = config.ApiEndpoint ?? AiProviders.Defaults[AiProviders.Claude].Endpoint!;
        var requestBody = new
        {
            model = config.ModelId,
            max_tokens = config.MaxTokens,
            system = systemPrompt ?? config.SystemPrompt ?? DefaultSystemPrompt,
            messages = messages.Select(m => new { role = m.Role, content = m.Content })
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return responseObj.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
    }

    private async Task<string> CallOpenAiAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        var endpoint = config.ApiEndpoint ?? AiProviders.Defaults[AiProviders.OpenAI].Endpoint!;
        var allMessages = new List<object>();
        
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            allMessages.Add(new { role = "system", content = systemPrompt });
        }
        allMessages.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

        var requestBody = new
        {
            model = config.ModelId,
            messages = allMessages,
            max_tokens = config.MaxTokens,
            temperature = (double)config.Temperature
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    private async Task<string> CallAzureOpenAiAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("api-key", config.ApiKey);
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        if (string.IsNullOrEmpty(config.ApiEndpoint))
            throw new InvalidOperationException("Azure OpenAI requires ApiEndpoint configuration");

        var allMessages = new List<object>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            allMessages.Add(new { role = "system", content = systemPrompt });
        }
        allMessages.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

        var requestBody = new
        {
            messages = allMessages,
            max_tokens = config.MaxTokens,
            temperature = (double)config.Temperature
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(config.ApiEndpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    private async Task<string> CallGeminiAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        var endpoint = (config.ApiEndpoint ?? AiProviders.Defaults[AiProviders.Gemini].Endpoint!)
            .Replace("{model}", config.ModelId) + $"?key={config.ApiKey}";

        var parts = new List<object>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            parts.Add(new { text = $"System: {systemPrompt}" });
        }
        foreach (var m in messages)
        {
            parts.Add(new { text = $"{m.Role}: {m.Content}" });
        }

        var requestBody = new
        {
            contents = new[] { new { parts } },
            generationConfig = new
            {
                maxOutputTokens = config.MaxTokens,
                temperature = (double)config.Temperature
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        return responseObj.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";
    }

    private async Task<string> CallLocalLlmAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        var endpoint = config.ApiEndpoint ?? AiProviders.Defaults[config.Provider].Endpoint!;
        
        var allMessages = new List<object>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            allMessages.Add(new { role = "system", content = systemPrompt });
        }
        allMessages.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

        var requestBody = new
        {
            model = config.ModelId,
            messages = allMessages,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        // Handle both Ollama and OpenAI-compatible response formats
        if (responseObj.TryGetProperty("message", out var message))
        {
            return message.GetProperty("content").GetString() ?? "";
        }
        if (responseObj.TryGetProperty("choices", out var choices))
        {
            return choices[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        return responseObj.GetProperty("response").GetString() ?? "";
    }

    private async Task<string> CallCustomAsync(AiProviderConfiguration config, List<AiMessage> messages, string? systemPrompt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(config.ApiEndpoint))
            throw new InvalidOperationException("Custom provider requires ApiEndpoint");

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        // Apply custom headers
        if (!string.IsNullOrEmpty(config.CustomHeaders))
        {
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(config.CustomHeaders);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.Replace("{API_KEY}", config.ApiKey));
                }
            }
        }

        // Build request from template or default
        object requestBody;
        if (!string.IsNullOrEmpty(config.RequestTemplate))
        {
            var template = config.RequestTemplate
                .Replace("{MESSAGES}", JsonSerializer.Serialize(messages))
                .Replace("{SYSTEM_PROMPT}", systemPrompt ?? "")
                .Replace("{MAX_TOKENS}", config.MaxTokens.ToString())
                .Replace("{MODEL}", config.ModelId);
            requestBody = JsonSerializer.Deserialize<object>(template)!;
        }
        else
        {
            requestBody = new { messages, system = systemPrompt, max_tokens = config.MaxTokens };
        }

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(config.ApiEndpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        // Extract response using custom path or try common paths
        if (!string.IsNullOrEmpty(config.ResponsePath))
        {
            var parts = config.ResponsePath.Split('.');
            var current = responseObj;
            foreach (var part in parts)
            {
                if (int.TryParse(part, out var index))
                {
                    current = current[index];
                }
                else
                {
                    current = current.GetProperty(part);
                }
            }
            return current.GetString() ?? "";
        }

        // Try common response paths
        if (responseObj.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.Array)
            return c[0].GetProperty("text").GetString() ?? "";
        if (responseObj.TryGetProperty("choices", out var ch))
            return ch[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        if (responseObj.TryGetProperty("response", out var r))
            return r.GetString() ?? "";

        return responseJson;
    }

    #endregion

    #region Helper Methods

    private async Task<AiProviderConfiguration?> GetConfigurationAsync(Guid configId, Guid? tenantId, CancellationToken ct)
    {
        if (configId == Guid.Empty)
        {
            // Return appsettings-based config
            var apiKey = _configuration["ClaudeAgents:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return null;

            return new AiProviderConfiguration
            {
                Id = Guid.Empty,
                Provider = AiProviders.Claude,
                ApiKey = apiKey,
                ModelId = _configuration["ClaudeAgents:Model"] ?? "claude-sonnet-4-20250514",
                MaxTokens = int.TryParse(_configuration["ClaudeAgents:MaxTokens"], out var mt) ? mt : 4096,
                TimeoutSeconds = int.TryParse(_configuration["ClaudeAgents:TimeoutSeconds"], out var ts) ? ts : 60,
                ApiVersion = _configuration["ClaudeAgents:ApiVersion"] ?? "2023-06-01",
                ApiEndpoint = _configuration["ClaudeAgents:ApiEndpoint"]
            };
        }

        return await _db.Set<AiProviderConfiguration>()
            .FirstOrDefaultAsync(c => c.Id == configId && c.IsActive, ct);
    }

    private async Task<AiProviderConfiguration?> GetBestConfigurationAsync(Guid? tenantId, string? preferredProvider, string useCase, CancellationToken ct)
    {
        // First try database configuration
        var query = _db.Set<AiProviderConfiguration>()
            .Where(c => c.IsActive && (c.TenantId == null || c.TenantId == tenantId));

        if (!string.IsNullOrEmpty(preferredProvider))
        {
            query = query.Where(c => c.Provider == preferredProvider);
        }

        var configs = await query.OrderBy(c => c.Priority).ToListAsync(ct);

        // Filter by use case
        var matching = configs.Where(c => 
            c.AllowedUseCases == "all" || 
            c.AllowedUseCases.Split(',').Contains(useCase) ||
            c.AllowedUseCases.Split(',').Contains("all")).ToList();

        // Check usage limits
        foreach (var config in matching)
        {
            if (config.MonthlyUsageLimit <= 0 || config.CurrentMonthUsage < config.MonthlyUsageLimit)
            {
                return config;
            }
        }

        // Fallback to appsettings
        var apiKey = _configuration["ClaudeAgents:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            return new AiProviderConfiguration
            {
                Id = Guid.Empty,
                Provider = AiProviders.Claude,
                ApiKey = apiKey,
                ModelId = _configuration["ClaudeAgents:Model"] ?? "claude-sonnet-4-20250514",
                MaxTokens = int.TryParse(_configuration["ClaudeAgents:MaxTokens"], out var mt) ? mt : 4096,
                TimeoutSeconds = int.TryParse(_configuration["ClaudeAgents:TimeoutSeconds"], out var ts) ? ts : 60,
                ApiVersion = _configuration["ClaudeAgents:ApiVersion"] ?? "2023-06-01",
                ApiEndpoint = _configuration["ClaudeAgents:ApiEndpoint"]
            };
        }

        return null;
    }

    private async Task IncrementUsageAsync(AiProviderConfiguration config, CancellationToken ct)
    {
        if (config.Id == Guid.Empty) return; // appsettings config, no tracking

        config.CurrentMonthUsage++;
        config.LastUsedAt = DateTime.UtcNow;

        // Reset if month changed
        if (config.LastUsageResetDate.HasValue && config.LastUsageResetDate.Value.Month != DateTime.UtcNow.Month)
        {
            config.CurrentMonthUsage = 1;
            config.LastUsageResetDate = DateTime.UtcNow;
        }

        _db.Set<AiProviderConfiguration>().Update(config);
        await _db.SaveChangesAsync(ct);
    }

    #endregion
}
