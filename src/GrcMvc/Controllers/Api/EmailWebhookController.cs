using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Services.EmailOperations;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Webhook endpoint for Microsoft Graph change notifications
/// Receives notifications when new emails arrive
/// </summary>
[ApiController]
[Route("api/webhooks/email")]
[AllowAnonymous] // Graph sends notifications without auth
[IgnoreAntiforgeryToken]
public class EmailWebhookController : ControllerBase
{
    private readonly ILogger<EmailWebhookController> _logger;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IServiceProvider _serviceProvider;

    public EmailWebhookController(
        ILogger<EmailWebhookController> logger,
        IBackgroundJobClient backgroundJobs,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _backgroundJobs = backgroundJobs;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Main webhook endpoint for Graph notifications
    /// Handles both validation and notification delivery
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveNotification()
    {
        _logger.LogInformation("📬 Webhook received. Query: {Query}", Request.QueryString);
        
        // Handle validation request (Graph sends this when creating subscription)
        if (Request.Query.TryGetValue("validationToken", out var validationToken) && 
            !string.IsNullOrEmpty(validationToken))
        {
            _logger.LogInformation("✅ Validation request received, returning token: {Token}", validationToken.ToString());
            return Content(validationToken.ToString(), "text/plain");
        }

        // Read the request body for notifications
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(body))
        {
            _logger.LogInformation("Empty body received, accepting");
            return Accepted(); // Empty body is OK for some notifications
        }

        _logger.LogInformation("Received Graph webhook: {Body}", body.Length > 500 ? body[..500] + "..." : body);

        // Parse the notification
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("value", out var notifications))
            {
                foreach (var notification in notifications.EnumerateArray())
                {
                    await ProcessNotificationAsync(notification);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process webhook notification");
        }

        // Always return 202 Accepted to acknowledge receipt
        return Accepted();
    }

    private async Task ProcessNotificationAsync(JsonElement notification)
    {
        try
        {
            var changeType = notification.TryGetProperty("changeType", out var ct) 
                ? ct.GetString() : "unknown";
            
            var resource = notification.TryGetProperty("resource", out var res) 
                ? res.GetString() : null;
            
            var subscriptionId = notification.TryGetProperty("subscriptionId", out var sid) 
                ? sid.GetString() : null;

            _logger.LogInformation("Processing notification: ChangeType={ChangeType}, Resource={Resource}", 
                changeType, resource);

            // For new message notifications, queue background processing
            if (changeType == "created" && resource != null)
            {
                // Extract mailbox ID and message ID from resource
                // Resource format: /users/{mailbox}/messages/{messageId}
                var parts = resource.Split('/');
                if (parts.Length >= 4)
                {
                    var mailboxId = parts[2];
                    var messageId = parts.Length >= 5 ? parts[4] : null;

                    // Queue background job to process the new email
                    _backgroundJobs.Enqueue<EmailProcessingJob>(job => 
                        job.ProcessNewEmailAsync(mailboxId, messageId, subscriptionId));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process individual notification");
        }
    }

    /// <summary>
    /// Lifecycle notification for subscription expiration
    /// </summary>
    [HttpPost("lifecycle")]
    public async Task<IActionResult> LifecycleNotification()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        _logger.LogInformation("Received lifecycle notification: {Body}", body);

        // Handle subscription reauthorization/renewal
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("value", out var notifications))
            {
                foreach (var notification in notifications.EnumerateArray())
                {
                    var lifecycleEvent = notification.TryGetProperty("lifecycleEvent", out var le) 
                        ? le.GetString() : null;
                    
                    var subscriptionId = notification.TryGetProperty("subscriptionId", out var sid) 
                        ? sid.GetString() : null;

                    if (lifecycleEvent == "subscriptionRemoved" || lifecycleEvent == "reauthorizationRequired")
                    {
                        _logger.LogWarning("Subscription {SubscriptionId} requires action: {Event}", 
                            subscriptionId, lifecycleEvent);
                        
                        // Queue job to renew subscription
                        if (subscriptionId != null)
                        {
                            _backgroundJobs.Enqueue<EmailProcessingJob>(job => 
                                job.RenewSubscriptionAsync(subscriptionId));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process lifecycle notification");
        }

        return Ok();
    }
}
