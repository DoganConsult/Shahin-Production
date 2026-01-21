using Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Integrations;
using System.Text.Json;

namespace GrcMvc.Services.Integrations;

/// <summary>
/// Stripe Webhook Handler Implementation
/// Handles Stripe webhook events for payment processing
/// </summary>
public class StripeWebhookHandler : IPaymentWebhookHandler
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookHandler> _logger;
    private readonly string _webhookSecret;

    public StripeWebhookHandler(
        IConfiguration configuration,
        ILogger<StripeWebhookHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Get Stripe webhook secret from configuration
        _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? 
                        _configuration["Stripe__WebhookSecret"] ?? 
                        Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET") ?? 
                        string.Empty;

        if (string.IsNullOrEmpty(_webhookSecret))
        {
            _logger.LogWarning("Stripe webhook secret not configured. Webhook verification will fail.");
        }
    }

    public async Task<PaymentWebhookEventDto?> VerifyAndParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_webhookSecret))
            {
                _logger.LogWarning("Cannot verify webhook: webhook secret not configured");
                return null;
            }

            // Verify the webhook signature
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _webhookSecret,
                throwOnApiVersionMismatch: false);

            _logger.LogDebug(
                "Verified Stripe webhook: {EventType} ({EventId})",
                stripeEvent.Type,
                stripeEvent.Id);

            // Parse the event data
            var webhookEvent = new PaymentWebhookEventDto
            {
                EventType = stripeEvent.Type,
                EventId = stripeEvent.Id,
                EventTime = stripeEvent.Created
            };

            // Extract data based on event type
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                case "payment_intent.payment_failed":
                case "payment_intent.canceled":
                    if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                    {
                        webhookEvent.PaymentIntentId = paymentIntent.Id;
                        webhookEvent.Amount = paymentIntent.Amount / 100m; // Convert from cents
                        webhookEvent.Status = paymentIntent.Status;
                        
                        // Extract tenant/subscription from metadata
                        if (paymentIntent.Metadata.TryGetValue("TenantId", out var tenantIdStr) &&
                            Guid.TryParse(tenantIdStr, out var tenantId))
                        {
                            webhookEvent.TenantId = tenantId;
                        }
                        if (paymentIntent.Metadata.TryGetValue("SubscriptionId", out var subscriptionIdStr) &&
                            Guid.TryParse(subscriptionIdStr, out var subscriptionId))
                        {
                            webhookEvent.SubscriptionId = subscriptionId.ToString();
                        }
                    }
                    break;

                case "checkout.session.completed":
                    if (stripeEvent.Data.Object is Stripe.Checkout.Session session)
                    {
                        webhookEvent.SubscriptionId = session.SubscriptionId;
                        webhookEvent.CustomerId = session.CustomerId;
                        
                        // Extract from metadata
                        if (session.Metadata.TryGetValue("TenantId", out var sessionTenantIdStr) &&
                            Guid.TryParse(sessionTenantIdStr, out var sessionTenantId))
                        {
                            webhookEvent.TenantId = sessionTenantId;
                        }
                        if (session.Metadata.TryGetValue("SubscriptionId", out var sessionSubscriptionIdStr))
                        {
                            webhookEvent.SubscriptionId = sessionSubscriptionIdStr;
                        }
                    }
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.deleted":
                    if (stripeEvent.Data.Object is Stripe.Subscription subscription)
                    {
                        webhookEvent.SubscriptionId = subscription.Id;
                        webhookEvent.CustomerId = subscription.CustomerId;
                        webhookEvent.Status = subscription.Status;
                        
                        // Extract from metadata
                        if (subscription.Metadata.TryGetValue("TenantId", out var subTenantIdStr) &&
                            Guid.TryParse(subTenantIdStr, out var subTenantId))
                        {
                            webhookEvent.TenantId = subTenantId;
                        }
                    }
                    break;

                case "charge.refunded":
                    if (stripeEvent.Data.Object is Charge charge)
                    {
                        webhookEvent.PaymentIntentId = charge.PaymentIntentId;
                        webhookEvent.Amount = charge.AmountRefunded / 100m; // Convert from cents
                        webhookEvent.Status = "refunded";
                    }
                    break;
            }

            // Include full event data as JSON
            webhookEvent.Data = JsonSerializer.Deserialize<Dictionary<string, object>>(
                stripeEvent.Data.RawObject?.ToString() ?? "{}");

            return webhookEvent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error verifying webhook signature");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return null;
        }
    }

    public async Task<bool> HandleWebhookEventAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing webhook event: {EventType} ({EventId}) for tenant {TenantId}",
                webhookEvent.EventType,
                webhookEvent.EventId,
                webhookEvent.TenantId);

            // Handle different event types
            switch (webhookEvent.EventType)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentSucceededAsync(webhookEvent, cancellationToken);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentFailedAsync(webhookEvent, cancellationToken);
                    break;

                case "checkout.session.completed":
                    await HandleCheckoutCompletedAsync(webhookEvent, cancellationToken);
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                    await HandleSubscriptionUpdatedAsync(webhookEvent, cancellationToken);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionDeletedAsync(webhookEvent, cancellationToken);
                    break;

                case "charge.refunded":
                    await HandleRefundProcessedAsync(webhookEvent, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook event type: {EventType}", webhookEvent.EventType);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling webhook event: {EventId}", webhookEvent.EventId);
            return false;
        }
    }

    private async Task HandlePaymentSucceededAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment succeeded: {PaymentIntentId}, amount {Amount} for tenant {TenantId}",
            webhookEvent.PaymentIntentId,
            webhookEvent.Amount,
            webhookEvent.TenantId);

        // TODO: Update subscription status in database
        // This should be handled by a service that processes the payment
        // For now, we just log the event
    }

    private async Task HandlePaymentFailedAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Payment failed: {PaymentIntentId} for tenant {TenantId}",
            webhookEvent.PaymentIntentId,
            webhookEvent.TenantId);

        // TODO: Update subscription status, notify user, etc.
    }

    private async Task HandleCheckoutCompletedAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Checkout completed: subscription {SubscriptionId} for tenant {TenantId}",
            webhookEvent.SubscriptionId,
            webhookEvent.TenantId);

        // TODO: Activate subscription in database
    }

    private async Task HandleSubscriptionUpdatedAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Subscription updated: {SubscriptionId}, status {Status} for tenant {TenantId}",
            webhookEvent.SubscriptionId,
            webhookEvent.Status,
            webhookEvent.TenantId);

        // TODO: Update subscription in database
    }

    private async Task HandleSubscriptionDeletedAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Subscription deleted: {SubscriptionId} for tenant {TenantId}",
            webhookEvent.SubscriptionId,
            webhookEvent.TenantId);

        // TODO: Cancel subscription in database
    }

    private async Task HandleRefundProcessedAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Refund processed: {PaymentIntentId}, amount {Amount} for tenant {TenantId}",
            webhookEvent.PaymentIntentId,
            webhookEvent.Amount,
            webhookEvent.TenantId);

        // TODO: Update payment status in database
    }
}
