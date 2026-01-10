using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities.EmailOperations;
using GrcMvc.Services.EmailOperations;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API for managing Microsoft Graph subscriptions for email notifications
/// </summary>
[ApiController]
[Route("api/graph-subscriptions")]
[IgnoreAntiforgeryToken]
public class GraphSubscriptionsController : ControllerBase
{
    private readonly GrcDbContext _db;
    private readonly IMicrosoftGraphEmailService _graphService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GraphSubscriptionsController> _logger;
    private readonly IBackgroundJobClient _backgroundJobs;

    public GraphSubscriptionsController(
        GrcDbContext db,
        IMicrosoftGraphEmailService graphService,
        IConfiguration configuration,
        ILogger<GraphSubscriptionsController> logger,
        IBackgroundJobClient backgroundJobs)
    {
        _db = db;
        _graphService = graphService;
        _configuration = configuration;
        _logger = logger;
        _backgroundJobs = backgroundJobs;
    }

    /// <summary>
    /// Create subscriptions for all active mailboxes
    /// </summary>
    [HttpPost("create-all")]
    [AllowAnonymous] // For initial setup
    public async Task<IActionResult> CreateAllSubscriptions()
    {
        var results = new System.Collections.Generic.List<object>();
        
        var mailboxes = await _db.EmailMailboxes
            .Where(m => m.IsActive && m.GraphUserId != null)
            .ToListAsync();

        var webhookBaseUrl = _configuration["App:SelfUrl"] ?? "https://portal.shahin-ai.com";
        var webhookUrl = $"{webhookBaseUrl}/api/webhooks/email";

        // Get default credentials
        var tenantId = _configuration["EmailOperations:MicrosoftGraph:TenantId"]!;
        var clientId = _configuration["EmailOperations:MicrosoftGraph:ClientId"]!;
        var clientSecret = _configuration["EmailOperations:MicrosoftGraph:ClientSecret"]!;

        foreach (var mailbox in mailboxes)
        {
            try
            {
                // Use mailbox-specific credentials if available, otherwise use default
                var mTenantId = mailbox.TenantId ?? tenantId;
                var mClientId = mailbox.ClientId ?? clientId;
                var mClientSecret = mailbox.EncryptedClientSecret ?? clientSecret;

                var token = await _graphService.GetAccessTokenAsync(mTenantId, mClientId, mClientSecret);

                var subscription = await _graphService.CreateSubscriptionAsync(
                    token, 
                    mailbox.GraphUserId!, 
                    webhookUrl);

                mailbox.WebhookSubscriptionId = subscription.Id;
                mailbox.WebhookExpiresAt = subscription.ExpirationDateTime;
                mailbox.TenantId ??= tenantId;
                mailbox.ClientId ??= clientId;
                mailbox.EncryptedClientSecret ??= clientSecret;

                // Schedule renewal before expiry
                var renewAt = subscription.ExpirationDateTime.AddHours(-2);
                _backgroundJobs.Schedule<EmailProcessingJob>(
                    job => job.RenewSubscriptionAsync(subscription.Id),
                    renewAt);

                results.Add(new
                {
                    mailbox = mailbox.EmailAddress,
                    status = "success",
                    subscriptionId = subscription.Id,
                    expiresAt = subscription.ExpirationDateTime
                });

                _logger.LogInformation("Created subscription for {Mailbox}: {SubId}", 
                    mailbox.EmailAddress, subscription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create subscription for {Mailbox}", mailbox.EmailAddress);
                results.Add(new
                {
                    mailbox = mailbox.EmailAddress,
                    status = "failed",
                    error = ex.Message
                });
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { 
            message = $"Processed {mailboxes.Count} mailboxes",
            results 
        });
    }

    /// <summary>
    /// Create subscription for a specific mailbox
    /// </summary>
    [HttpPost("create/{mailboxId}")]
    public async Task<IActionResult> CreateSubscription(Guid mailboxId)
    {
        var mailbox = await _db.EmailMailboxes.FindAsync(mailboxId);
        if (mailbox == null)
            return NotFound(new { error = "Mailbox not found" });

        if (string.IsNullOrEmpty(mailbox.GraphUserId))
            return BadRequest(new { error = "Mailbox has no Graph User ID" });

        var webhookBaseUrl = _configuration["App:SelfUrl"] ?? "https://portal.shahin-ai.com";
        var webhookUrl = $"{webhookBaseUrl}/api/webhooks/email";

        var tenantId = mailbox.TenantId ?? _configuration["EmailOperations:MicrosoftGraph:TenantId"]!;
        var clientId = mailbox.ClientId ?? _configuration["EmailOperations:MicrosoftGraph:ClientId"]!;
        var clientSecret = mailbox.EncryptedClientSecret ?? _configuration["EmailOperations:MicrosoftGraph:ClientSecret"]!;

        try
        {
            var token = await _graphService.GetAccessTokenAsync(tenantId, clientId, clientSecret);

            var subscription = await _graphService.CreateSubscriptionAsync(
                token, 
                mailbox.GraphUserId, 
                webhookUrl);

            mailbox.WebhookSubscriptionId = subscription.Id;
            mailbox.WebhookExpiresAt = subscription.ExpirationDateTime;
            mailbox.TenantId = tenantId;
            mailbox.ClientId = clientId;
            mailbox.EncryptedClientSecret = clientSecret;

            await _db.SaveChangesAsync();

            // Schedule renewal
            var renewAt = subscription.ExpirationDateTime.AddHours(-2);
            _backgroundJobs.Schedule<EmailProcessingJob>(
                job => job.RenewSubscriptionAsync(subscription.Id),
                renewAt);

            return Ok(new
            {
                message = "Subscription created",
                subscriptionId = subscription.Id,
                expiresAt = subscription.ExpirationDateTime,
                mailbox = mailbox.EmailAddress
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create subscription for {Mailbox}", mailbox.EmailAddress);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete subscription for a mailbox
    /// </summary>
    [HttpDelete("{mailboxId}")]
    public async Task<IActionResult> DeleteSubscription(Guid mailboxId)
    {
        var mailbox = await _db.EmailMailboxes.FindAsync(mailboxId);
        if (mailbox == null)
            return NotFound(new { error = "Mailbox not found" });

        if (string.IsNullOrEmpty(mailbox.WebhookSubscriptionId))
            return BadRequest(new { error = "No subscription exists" });

        var tenantId = mailbox.TenantId ?? _configuration["EmailOperations:MicrosoftGraph:TenantId"]!;
        var clientId = mailbox.ClientId ?? _configuration["EmailOperations:MicrosoftGraph:ClientId"]!;
        var clientSecret = mailbox.EncryptedClientSecret ?? _configuration["EmailOperations:MicrosoftGraph:ClientSecret"]!;

        try
        {
            var token = await _graphService.GetAccessTokenAsync(tenantId, clientId, clientSecret);
            await _graphService.DeleteSubscriptionAsync(token, mailbox.WebhookSubscriptionId);

            mailbox.WebhookSubscriptionId = null;
            mailbox.WebhookExpiresAt = null;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Subscription deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete subscription for {Mailbox}", mailbox.EmailAddress);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// List all subscriptions
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListSubscriptions()
    {
        var mailboxes = await _db.EmailMailboxes
            .Where(m => m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.EmailAddress,
                m.Brand,
                m.GraphUserId,
                m.WebhookSubscriptionId,
                m.WebhookExpiresAt,
                m.LastSyncAt,
                hasSubscription = m.WebhookSubscriptionId != null,
                isExpired = m.WebhookExpiresAt.HasValue && m.WebhookExpiresAt < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(mailboxes);
    }

    /// <summary>
    /// Renew all expiring subscriptions
    /// </summary>
    [HttpPost("renew-all")]
    [AllowAnonymous]
    public async Task<IActionResult> RenewAllSubscriptions()
    {
        var expiringMailboxes = await _db.EmailMailboxes
            .Where(m => m.IsActive && 
                       m.WebhookSubscriptionId != null &&
                       m.WebhookExpiresAt.HasValue &&
                       m.WebhookExpiresAt < DateTime.UtcNow.AddHours(24))
            .ToListAsync();

        var results = new System.Collections.Generic.List<object>();

        foreach (var mailbox in expiringMailboxes)
        {
            try
            {
                var tenantId = mailbox.TenantId ?? _configuration["EmailOperations:MicrosoftGraph:TenantId"]!;
                var clientId = mailbox.ClientId ?? _configuration["EmailOperations:MicrosoftGraph:ClientId"]!;
                var clientSecret = mailbox.EncryptedClientSecret ?? _configuration["EmailOperations:MicrosoftGraph:ClientSecret"]!;

                var token = await _graphService.GetAccessTokenAsync(tenantId, clientId, clientSecret);
                var renewed = await _graphService.RenewSubscriptionAsync(token, mailbox.WebhookSubscriptionId!);

                mailbox.WebhookExpiresAt = renewed.ExpirationDateTime;

                results.Add(new
                {
                    mailbox = mailbox.EmailAddress,
                    status = "renewed",
                    newExpiry = renewed.ExpirationDateTime
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    mailbox = mailbox.EmailAddress,
                    status = "failed",
                    error = ex.Message
                });
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { 
            message = $"Processed {expiringMailboxes.Count} expiring subscriptions",
            results 
        });
    }

    /// <summary>
    /// Sync emails from all mailboxes (manual sync)
    /// </summary>
    [HttpPost("sync-all")]
    [AllowAnonymous]
    public async Task<IActionResult> SyncAllMailboxes()
    {
        var mailboxes = await _db.EmailMailboxes
            .Where(m => m.IsActive && m.GraphUserId != null)
            .ToListAsync();

        var results = new System.Collections.Generic.List<object>();
        var tenantId = _configuration["EmailOperations:MicrosoftGraph:TenantId"]!;
        var clientId = _configuration["EmailOperations:MicrosoftGraph:ClientId"]!;
        var clientSecret = _configuration["EmailOperations:MicrosoftGraph:ClientSecret"]!;

        foreach (var mailbox in mailboxes)
        {
            try
            {
                var mTenantId = mailbox.TenantId ?? tenantId;
                var mClientId = mailbox.ClientId ?? clientId;
                var mClientSecret = mailbox.EncryptedClientSecret ?? clientSecret;

                var token = await _graphService.GetAccessTokenAsync(mTenantId, mClientId, mClientSecret);

                // Get recent messages
                var messages = await _graphService.GetMessagesAsync(
                    token, 
                    mailbox.GraphUserId!, 
                    since: mailbox.LastSyncAt ?? DateTime.UtcNow.AddDays(-1),
                    top: 20);

                // Queue processing for each message
                foreach (var msg in messages)
                {
                    _backgroundJobs.Enqueue<EmailProcessingJob>(
                        job => job.ProcessNewEmailAsync(mailbox.GraphUserId!, msg.Id, null));
                }

                mailbox.LastSyncAt = DateTime.UtcNow;

                results.Add(new
                {
                    mailbox = mailbox.EmailAddress,
                    status = "synced",
                    messageCount = messages.Count
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    mailbox = mailbox.EmailAddress,
                    status = "failed",
                    error = ex.Message
                });
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { 
            message = $"Synced {mailboxes.Count} mailboxes",
            results 
        });
    }
}
