using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Integrations;
using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Stripe Webhook Controller
/// 
/// ASP.NET Core / ABP Best Practices:
/// 1. Dedicated controller for webhooks (Single Responsibility)
/// 2. [ApiController] for automatic model validation
/// 3. Raw body reading for signature verification
/// 4. Proper HTTP status codes
/// 5. Idempotency handling (events may be sent multiple times)
/// 6. Async/await throughout
/// 7. Structured logging
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentWebhookHandler _webhookHandler;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        IPaymentWebhookHandler webhookHandler,
        ILogger<PaymentWebhookController> logger)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
    }

    /// <summary>
    /// Stripe Webhook Endpoint
    /// POST /api/webhooks/stripe
    /// 
    /// Security Notes:
    /// - Verifies Stripe signature before processing
    /// - Always returns 200 OK to Stripe after verification
    /// - Logs all events for audit purposes
    /// </summary>
    [HttpPost("stripe")]
    [IgnoreAntiforgeryToken] // Stripe webhooks don't include CSRF tokens
    public async Task<IActionResult> HandleStripeWebhook()
    {
        string payload;
        
        // Read raw body for signature verification
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrEmpty(payload))
        {
            _logger.LogWarning("Empty webhook payload received");
            return BadRequest(new { error = "Empty payload" });
        }

        // Get Stripe signature from header
        var signature = Request.Headers["Stripe-Signature"].ToString();
        
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Missing Stripe-Signature header");
            return BadRequest(new { error = "Missing signature" });
        }

        try
        {
            // Verify and parse the webhook
            var webhookEvent = await _webhookHandler.VerifyAndParseWebhookAsync(payload, signature);
            
            if (webhookEvent == null)
            {
                _logger.LogWarning("Webhook signature verification failed");
                return BadRequest(new { error = "Invalid signature" });
            }

            _logger.LogInformation(
                "Processing Stripe webhook: {EventType} ({EventId}) for tenant {TenantId}",
                webhookEvent.EventType,
                webhookEvent.EventId,
                webhookEvent.TenantId);

            // Process the event
            var success = await _webhookHandler.HandleWebhookEventAsync(webhookEvent);

            if (!success)
            {
                _logger.LogWarning("Failed to process webhook event: {EventId}", webhookEvent.EventId);
                // Still return 200 to prevent Stripe from retrying
                // We'll handle failed events through our own retry mechanism
            }

            // Always return 200 to acknowledge receipt
            // Stripe will retry if we return non-2xx
            return Ok(new { received = true, eventId = webhookEvent.EventId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing Stripe webhook");
            
            // Return 200 anyway to prevent infinite retries
            // Log the error for manual investigation
            return Ok(new { received = true, error = "Processing error logged" });
        }
    }

    /// <summary>
    /// PayPal Webhook Endpoint (Future)
    /// POST /api/webhooks/paypal
    /// </summary>
    [HttpPost("paypal")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> HandlePayPalWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            
            _logger.LogInformation("PayPal webhook received");

            // Verify PayPal webhook signature (placeholder - would use PayPal SDK)
            var webhookId = Request.Headers["PAYPAL-TRANSMISSION-ID"].FirstOrDefault();
            if (string.IsNullOrEmpty(webhookId))
            {
                _logger.LogWarning("PayPal webhook missing transmission ID");
                return BadRequest(new { error = "Missing PayPal transmission ID" });
            }

            // Parse and process the webhook event
            var eventType = Request.Headers["PAYPAL-EVENT-TYPE"].FirstOrDefault() ?? "unknown";
            _logger.LogInformation("PayPal event type: {EventType}, ID: {WebhookId}", eventType, webhookId);

            // Handle different event types
            switch (eventType)
            {
                case "PAYMENT.CAPTURE.COMPLETED":
                    _logger.LogInformation("PayPal payment captured successfully");
                    break;
                case "PAYMENT.CAPTURE.DENIED":
                    _logger.LogWarning("PayPal payment denied");
                    break;
                case "BILLING.SUBSCRIPTION.ACTIVATED":
                    _logger.LogInformation("PayPal subscription activated");
                    break;
                case "BILLING.SUBSCRIPTION.CANCELLED":
                    _logger.LogInformation("PayPal subscription cancelled");
                    break;
                default:
                    _logger.LogInformation("Unhandled PayPal event type: {EventType}", eventType);
                    break;
            }

            return Ok(new { received = true, eventType });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Webhook health check - can be used to verify endpoint is reachable
    /// GET /api/webhooks/health
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            status = "healthy",
            timestamp = DateTime.UtcNow,
            endpoints = new[] { "stripe", "paypal" }
        });
    }
}
