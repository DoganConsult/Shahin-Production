namespace GrcMvc.Models.Entities;

/// <summary>
/// AI Provider Configuration - Multi-tenant, Multi-provider support
/// Supports: Claude, OpenAI, Azure OpenAI, Google Gemini, Local LLMs, Custom providers
/// </summary>
public class AiProviderConfiguration : BaseEntity
{
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// Configuration name for identification
    /// </summary>
    public string Name { get; set; } = "Default";
    
    /// <summary>
    /// Provider type: Claude, OpenAI, AzureOpenAI, Gemini, Ollama, Custom
    /// </summary>
    public string Provider { get; set; } = "Claude";
    
    /// <summary>
    /// API endpoint URL (optional - uses default for known providers)
    /// </summary>
    public string? ApiEndpoint { get; set; }
    
    /// <summary>
    /// API key (encrypted at rest)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Model identifier (e.g., claude-sonnet-4-20250514, gpt-4o, gemini-pro)
    /// </summary>
    public string ModelId { get; set; } = "claude-sonnet-4-20250514";
    
    /// <summary>
    /// API version for providers that require it (e.g., Anthropic)
    /// </summary>
    public string? ApiVersion { get; set; }
    
    /// <summary>
    /// Max tokens per response
    /// </summary>
    public int MaxTokens { get; set; } = 4096;
    
    /// <summary>
    /// Temperature (0-2, higher = more creative)
    /// </summary>
    public decimal Temperature { get; set; } = 0.7m;
    
    /// <summary>
    /// Top P for nucleus sampling
    /// </summary>
    public decimal TopP { get; set; } = 0.95m;
    
    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Is this the default configuration for the tenant?
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Is this configuration active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Priority (for fallback - lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// Usage limit per month (0 = unlimited)
    /// </summary>
    public int MonthlyUsageLimit { get; set; } = 0;
    
    /// <summary>
    /// Current month usage count
    /// </summary>
    public int CurrentMonthUsage { get; set; } = 0;
    
    /// <summary>
    /// Last usage reset date
    /// </summary>
    public DateTime? LastUsageResetDate { get; set; }
    
    /// <summary>
    /// Custom headers (JSON) for custom providers
    /// </summary>
    public string? CustomHeaders { get; set; }
    
    /// <summary>
    /// Custom request template (JSON) for custom providers
    /// </summary>
    public string? RequestTemplate { get; set; }
    
    /// <summary>
    /// Custom response path (JSON path to extract response text)
    /// </summary>
    public string? ResponsePath { get; set; }
    
    /// <summary>
    /// System prompt override (optional)
    /// </summary>
    public string? SystemPrompt { get; set; }
    
    /// <summary>
    /// Allowed use cases (comma-separated: compliance,risk,audit,chat,all)
    /// </summary>
    public string AllowedUseCases { get; set; } = "all";
    
    public DateTime ConfiguredAt { get; set; } = DateTime.UtcNow;
    public string? ConfiguredBy { get; set; }
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation
    public virtual Tenant? Tenant { get; set; }
}

/// <summary>
/// Supported AI Providers
/// </summary>
public static class AiProviders
{
    public const string Claude = "Claude";
    public const string OpenAI = "OpenAI";
    public const string AzureOpenAI = "AzureOpenAI";
    public const string Gemini = "Gemini";
    public const string Ollama = "Ollama";
    public const string LMStudio = "LMStudio";
    public const string Custom = "Custom";
    
    public static readonly Dictionary<string, ProviderDefaults> Defaults = new()
    {
        [Claude] = new ProviderDefaults
        {
            Endpoint = "https://api.anthropic.com/v1/messages",
            ApiVersion = "2023-06-01",
            Models = new[] { "claude-sonnet-4-20250514", "claude-3-5-sonnet-20241022", "claude-3-opus-20240229", "claude-3-haiku-20240307" },
            AuthHeader = "x-api-key",
            VersionHeader = "anthropic-version"
        },
        [OpenAI] = new ProviderDefaults
        {
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Models = new[] { "gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo" },
            AuthHeader = "Authorization",
            AuthPrefix = "Bearer "
        },
        [AzureOpenAI] = new ProviderDefaults
        {
            // Endpoint is tenant-specific: https://{resource}.openai.azure.com/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview
            AuthHeader = "api-key"
        },
        [Gemini] = new ProviderDefaults
        {
            Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent",
            Models = new[] { "gemini-pro", "gemini-pro-vision", "gemini-1.5-pro", "gemini-1.5-flash" },
            AuthQueryParam = "key"
        },
        [Ollama] = new ProviderDefaults
        {
            Endpoint = "http://localhost:11434/api/chat",
            Models = new[] { "llama3.2", "mistral", "codellama", "qwen2.5" }
        },
        [LMStudio] = new ProviderDefaults
        {
            Endpoint = "http://localhost:1234/v1/chat/completions",
            Models = new[] { "local-model" }
        }
    };
    
    public class ProviderDefaults
    {
        public string? Endpoint { get; set; }
        public string? ApiVersion { get; set; }
        public string[]? Models { get; set; }
        public string? AuthHeader { get; set; }
        public string? AuthPrefix { get; set; }
        public string? VersionHeader { get; set; }
        public string? AuthQueryParam { get; set; }
    }
}
