namespace GrcMvc.Configuration;

/// <summary>
/// Configuration settings for Claude AI API
/// </summary>
public class ClaudeApiSettings
{
    public const string SectionName = "ClaudeAgents";

    /// <summary>
    /// Claude API key (required)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Claude model to use (default: claude-3-5-sonnet-20241022)
    /// </summary>
    public string Model { get; set; } = "claude-3-5-sonnet-20241022";

    /// <summary>
    /// Maximum tokens in response (default: 4096)
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// API endpoint URL (default: https://api.anthropic.com/v1/messages)
    /// </summary>
    public string ApiEndpoint { get; set; } = "https://api.anthropic.com/v1/messages";

    /// <summary>
    /// API version header (default: 2023-06-01)
    /// </summary>
    public string ApiVersion { get; set; } = "2023-06-01";

    /// <summary>
    /// Timeout in seconds (default: 60)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
