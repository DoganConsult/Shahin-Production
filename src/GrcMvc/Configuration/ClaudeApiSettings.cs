namespace GrcMvc.Configuration;

/// <summary>
/// Configuration settings for Claude AI API
/// </summary>
public class ClaudeApiSettings
{
    public const string SectionName = "ClaudeAgents";

    /// <summary>
    /// Whether Claude AI is enabled (default: true)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Claude API key (required)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Claude model to use (default: claude-sonnet-4-20250514)
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-4-20250514";

    /// <summary>
    /// Maximum tokens in response (default: 4096)
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature for response variability (default: 0.7)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

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

    /// <summary>
    /// Description of the service
    /// </summary>
    public string Description { get; set; } = "Claude AI for GRC agents, email classification, onboarding support, and customer assistance";
}
