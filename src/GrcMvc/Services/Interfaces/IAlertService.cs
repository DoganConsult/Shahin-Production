using GrcMvc.Agents;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service interface for code quality alerting
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Send alert based on analysis result
    /// </summary>
    Task<bool> SendAlertAsync(CodeQualityAlert alert, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if alert should be triggered based on result
    /// </summary>
    bool ShouldTriggerAlert(CodeAnalysisResult result, AlertConfiguration? config = null);

    /// <summary>
    /// Create alert from analysis result
    /// </summary>
    CodeQualityAlert CreateAlert(CodeAnalysisResult result, string? repositoryUrl = null, string? commitSha = null, string? branch = null);

    /// <summary>
    /// Send email alert
    /// </summary>
    Task<bool> SendEmailAlertAsync(CodeQualityAlert alert, List<string> recipients, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send Slack webhook alert
    /// </summary>
    Task<bool> SendSlackAlertAsync(CodeQualityAlert alert, string webhookUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send Microsoft Teams webhook alert
    /// </summary>
    Task<bool> SendTeamsAlertAsync(CodeQualityAlert alert, string webhookUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send custom webhook alert
    /// </summary>
    Task<bool> SendWebhookAlertAsync(CodeQualityAlert alert, string webhookUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get alert history
    /// </summary>
    Task<List<CodeQualityAlert>> GetAlertHistoryAsync(DateTime? from = null, DateTime? to = null, string? severity = null);

    /// <summary>
    /// Get alert configuration
    /// </summary>
    Task<AlertConfiguration> GetConfigurationAsync();

    /// <summary>
    /// Update alert configuration
    /// </summary>
    Task UpdateConfigurationAsync(AlertConfiguration config);
}
