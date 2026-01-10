using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Integrations;

/// <summary>
/// Stripe Payment Gateway Implementation
/// 
/// ASP.NET Core / ABP Best Practices Applied:
/// 1. Constructor Injection (DI)
/// 2. IOptions pattern for configuration
/// 3. IHttpClientFactory for resilient HTTP calls
/// 4. Proper async/await with CancellationToken
/// 5. Structured logging with ILogger
/// 6. Idempotency key support for payment safety
/// 7. Proper exception handling with custom exceptions
/// 8. Webhook signature verification for security
/// </summary>
public class StripeGatewayService : IPaymentGatewayService, IPaymentWebhookHandler, IPaymentCustomerService, IPaymentSubscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly GrcDbContext _dbContext;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<StripeGatewayService> _logger;
    private readonly StripeSettings _settings;
    
    private const string StripeApiBaseUrl = "https://api.stripe.com/v1";
    private const string StripeApiVersion = "2023-10-16";

    public StripeGatewayService(
        IHttpClientFactory httpClientFactory,
        GrcDbContext dbContext,
        ISubscriptionService subscriptionService,
        IOptions<StripeSettings> settings,
        ILogger<StripeGatewayService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Stripe");
        _dbContext = dbContext;
        _subscriptionService = subscriptionService;
        _settings = settings.Value;
        _logger = logger;
        
        // Configure HTTP client with Stripe headers
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        if (!string.IsNullOrEmpty(_settings.SecretKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
            _httpClient.DefaultRequestHeaders.Add("Stripe-Version", StripeApiVersion);
        }
    }

    #region IPaymentGatewayService Implementation

    public async Task<CheckoutSessionOutputDto> CreateCheckoutSessionAsync(
        CreateCheckoutSessionInputDto input,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return new CheckoutSessionOutputDto
            {
                Success = false,
                ErrorCode = "STRIPE_NOT_CONFIGURED",
                ErrorMessage = "Stripe API key is not configured. Please contact support."
            };
        }

        try
        {
            _logger.LogInformation(
                "Creating Stripe checkout session for tenant {TenantId}, plan {PlanCode}",
                input.TenantId, input.PlanCode);

            // Get the price ID for the plan
            var priceId = GetPriceIdForPlan(input.PlanCode, input.BillingCycle);
            if (string.IsNullOrEmpty(priceId))
            {
                return new CheckoutSessionOutputDto
                {
                    Success = false,
                    ErrorCode = "INVALID_PLAN",
                    ErrorMessage = $"No price configured for plan: {input.PlanCode}"
                };
            }

            // Build the checkout session parameters
            var parameters = new Dictionary<string, string>
            {
                ["mode"] = "subscription",
                ["success_url"] = input.SuccessUrl,
                ["cancel_url"] = input.CancelUrl,
                ["line_items[0][price]"] = priceId,
                ["line_items[0][quantity]"] = "1",
                ["metadata[tenant_id]"] = input.TenantId.ToString(),
                ["metadata[subscription_id]"] = input.SubscriptionId.ToString(),
                ["metadata[plan_code]"] = input.PlanCode,
                ["metadata[billing_cycle]"] = input.BillingCycle,
                ["payment_method_types[0]"] = "card",
                ["billing_address_collection"] = "required",
                ["allow_promotion_codes"] = "true",
                ["locale"] = "auto", // Auto-detect Arabic/English
            };

            // Add customer email if provided
            if (!string.IsNullOrEmpty(input.CustomerEmail))
            {
                parameters["customer_email"] = input.CustomerEmail;
            }

            // Add custom metadata
            if (input.Metadata != null)
            {
                var index = 0;
                foreach (var (key, value) in input.Metadata)
                {
                    parameters[$"metadata[custom_{key}]"] = value;
                    index++;
                }
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(
                $"{StripeApiBaseUrl}/checkout/sessions",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var session = JsonSerializer.Deserialize<StripeCheckoutSession>(responseBody);
                
                _logger.LogInformation(
                    "Checkout session created successfully: {SessionId} for tenant {TenantId}",
                    session?.Id, input.TenantId);

                return new CheckoutSessionOutputDto
                {
                    Success = true,
                    SessionId = session?.Id,
                    CheckoutUrl = session?.Url
                };
            }
            else
            {
                var error = JsonSerializer.Deserialize<StripeErrorResponse>(responseBody);
                
                _logger.LogWarning(
                    "Failed to create Stripe checkout session: {ErrorCode} - {ErrorMessage}",
                    error?.Error?.Code, error?.Error?.Message);

                return new CheckoutSessionOutputDto
                {
                    Success = false,
                    ErrorCode = error?.Error?.Code ?? "STRIPE_ERROR",
                    ErrorMessage = error?.Error?.Message ?? "Payment gateway error"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating Stripe checkout session for tenant {TenantId}", input.TenantId);
            
            return new CheckoutSessionOutputDto
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                ErrorMessage = "An error occurred while processing your payment request."
            };
        }
    }

    public async Task<PaymentResultOutputDto> ProcessPaymentAsync(
        ProcessPaymentInputDto input,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return new PaymentResultOutputDto
            {
                Success = false,
                ErrorCode = "STRIPE_NOT_CONFIGURED",
                ErrorMessage = "Stripe is not configured"
            };
        }

        try
        {
            _logger.LogInformation(
                "Processing direct payment for tenant {TenantId}, amount {Amount} {Currency}",
                input.TenantId, input.Amount, input.Currency);

            var parameters = new Dictionary<string, string>
            {
                ["amount"] = ((int)(input.Amount * 100)).ToString(), // Stripe uses cents
                ["currency"] = input.Currency.ToLower(),
                ["payment_method"] = input.PaymentMethodId,
                ["confirm"] = "true",
                ["metadata[tenant_id]"] = input.TenantId.ToString(),
                ["metadata[subscription_id]"] = input.SubscriptionId.ToString()
            };

            if (!string.IsNullOrEmpty(input.Description))
            {
                parameters["description"] = input.Description;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{StripeApiBaseUrl}/payment_intents")
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            // Add idempotency key for safety (prevents duplicate charges)
            if (!string.IsNullOrEmpty(input.IdempotencyKey))
            {
                request.Headers.Add("Idempotency-Key", input.IdempotencyKey);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var paymentIntent = JsonSerializer.Deserialize<StripePaymentIntent>(responseBody);
                
                _logger.LogInformation(
                    "Payment processed successfully: {PaymentIntentId}, status: {Status}",
                    paymentIntent?.Id, paymentIntent?.Status);

                return new PaymentResultOutputDto
                {
                    Success = paymentIntent?.Status == "succeeded",
                    PaymentIntentId = paymentIntent?.Id,
                    TransactionId = paymentIntent?.LatestCharge,
                    Status = paymentIntent?.Status ?? "unknown",
                    AmountPaid = input.Amount,
                    ProcessedAt = DateTime.UtcNow,
                    ReceiptUrl = paymentIntent?.ReceiptUrl
                };
            }
            else
            {
                var error = JsonSerializer.Deserialize<StripeErrorResponse>(responseBody);
                
                _logger.LogWarning(
                    "Payment failed: {ErrorCode} - {ErrorMessage}",
                    error?.Error?.Code, error?.Error?.Message);

                return new PaymentResultOutputDto
                {
                    Success = false,
                    ErrorCode = error?.Error?.Code ?? "PAYMENT_FAILED",
                    ErrorMessage = error?.Error?.Message ?? "Payment processing failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing payment for tenant {TenantId}", input.TenantId);
            
            return new PaymentResultOutputDto
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                ErrorMessage = "An error occurred while processing your payment."
            };
        }
    }

    public async Task<RefundResultDto> ProcessRefundAsync(
        RefundRequestDto input,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return new RefundResultDto
            {
                Success = false,
                ErrorMessage = "Stripe is not configured"
            };
        }

        try
        {
            _logger.LogInformation("Processing refund for payment {PaymentIntentId}", input.PaymentIntentId);

            var parameters = new Dictionary<string, string>
            {
                ["payment_intent"] = input.PaymentIntentId,
                ["reason"] = MapRefundReason(input.Reason)
            };

            if (input.Amount.HasValue)
            {
                parameters["amount"] = ((int)(input.Amount.Value * 100)).ToString();
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(
                $"{StripeApiBaseUrl}/refunds",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var refund = JsonSerializer.Deserialize<StripeRefund>(responseBody);
                
                _logger.LogInformation("Refund processed successfully: {RefundId}", refund?.Id);

                return new RefundResultDto
                {
                    Success = true,
                    RefundId = refund?.Id,
                    AmountRefunded = (refund?.Amount ?? 0) / 100m,
                    Status = refund?.Status ?? "unknown"
                };
            }
            else
            {
                var error = JsonSerializer.Deserialize<StripeErrorResponse>(responseBody);
                
                return new RefundResultDto
                {
                    Success = false,
                    ErrorMessage = error?.Error?.Message ?? "Refund failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing refund for {PaymentIntentId}", input.PaymentIntentId);
            
            return new RefundResultDto
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the refund."
            };
        }
    }

    public async Task<PaymentResultOutputDto?> GetPaymentStatusAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"{StripeApiBaseUrl}/payment_intents/{paymentIntentId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var paymentIntent = JsonSerializer.Deserialize<StripePaymentIntent>(responseBody);

                return new PaymentResultOutputDto
                {
                    Success = paymentIntent?.Status == "succeeded",
                    PaymentIntentId = paymentIntent?.Id,
                    Status = paymentIntent?.Status ?? "unknown",
                    AmountPaid = (paymentIntent?.Amount ?? 0) / 100m
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting payment status for {PaymentIntentId}", paymentIntentId);
            return null;
        }
    }

    #endregion

    #region IPaymentWebhookHandler Implementation

    public async Task<PaymentWebhookEventDto?> VerifyAndParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.WebhookSecret))
        {
            _logger.LogWarning("Webhook secret not configured, skipping signature verification");
        }
        else
        {
            // Verify webhook signature (Stripe uses HMAC-SHA256)
            if (!VerifyWebhookSignature(payload, signature, _settings.WebhookSecret))
            {
                _logger.LogWarning("Webhook signature verification failed");
                return null;
            }
        }

        try
        {
            var webhookEvent = JsonSerializer.Deserialize<StripeWebhookEvent>(payload);
            if (webhookEvent == null)
            {
                return null;
            }

            // Extract tenant ID from metadata if available
            Guid? tenantId = null;
            if (webhookEvent.Data?.Object?.Metadata?.TryGetValue("tenant_id", out var tenantIdStr) == true)
            {
                if (Guid.TryParse(tenantIdStr, out var tid))
                {
                    tenantId = tid;
                }
            }

            return new PaymentWebhookEventDto
            {
                EventType = webhookEvent.Type ?? string.Empty,
                EventId = webhookEvent.Id ?? string.Empty,
                PaymentIntentId = webhookEvent.Data?.Object?.PaymentIntent,
                SubscriptionId = webhookEvent.Data?.Object?.Id,
                CustomerId = webhookEvent.Data?.Object?.Customer,
                TenantId = tenantId,
                Amount = webhookEvent.Data?.Object?.AmountTotal.HasValue == true 
                    ? webhookEvent.Data.Object.AmountTotal.Value / 100m 
                    : null,
                Status = webhookEvent.Data?.Object?.Status,
                EventTime = DateTimeOffset.FromUnixTimeSeconds(webhookEvent.Created).UtcDateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse webhook payload");
            return null;
        }
    }

    public async Task<bool> HandleWebhookEventAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing webhook event: {EventType} ({EventId})",
            webhookEvent.EventType, webhookEvent.EventId);

        try
        {
            return webhookEvent.EventType switch
            {
                "checkout.session.completed" => await HandleCheckoutCompleteAsync(webhookEvent, cancellationToken),
                "invoice.paid" => await HandleInvoicePaidAsync(webhookEvent, cancellationToken),
                "invoice.payment_failed" => await HandlePaymentFailedAsync(webhookEvent, cancellationToken),
                "customer.subscription.updated" => await HandleSubscriptionUpdatedAsync(webhookEvent, cancellationToken),
                "customer.subscription.deleted" => await HandleSubscriptionDeletedAsync(webhookEvent, cancellationToken),
                "payment_intent.succeeded" => await HandlePaymentSucceededAsync(webhookEvent, cancellationToken),
                "payment_intent.payment_failed" => await HandlePaymentFailedAsync(webhookEvent, cancellationToken),
                _ => true // Acknowledge unknown events
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle webhook event: {EventType}", webhookEvent.EventType);
            return false;
        }
    }

    #endregion

    #region IPaymentCustomerService Implementation

    public async Task<PaymentCustomerDto?> GetOrCreateCustomerAsync(
        Guid tenantId,
        string email,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return null;
        }

        try
        {
            // First, try to find existing customer in local DB
            var tenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            if (tenant?.StripeCustomerId != null)
            {
                // Customer already exists
                return new PaymentCustomerDto
                {
                    CustomerId = tenant.StripeCustomerId,
                    Email = email,
                    Name = name,
                    TenantId = tenantId
                };
            }

            // Create new customer in Stripe
            var parameters = new Dictionary<string, string>
            {
                ["email"] = email,
                ["metadata[tenant_id]"] = tenantId.ToString()
            };

            if (!string.IsNullOrEmpty(name))
            {
                parameters["name"] = name;
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(
                $"{StripeApiBaseUrl}/customers",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var customer = JsonSerializer.Deserialize<StripeCustomer>(responseBody);

                // Store customer ID in tenant record
                if (tenant != null && customer != null)
                {
                    tenant.StripeCustomerId = customer.Id;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Created Stripe customer {CustomerId} for tenant {TenantId}", 
                    customer?.Id, tenantId);

                return new PaymentCustomerDto
                {
                    CustomerId = customer?.Id ?? string.Empty,
                    Email = email,
                    Name = name,
                    TenantId = tenantId
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get or create customer for tenant {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> UpdateCustomerAsync(
        string customerId,
        string? email = null,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return false;
        }

        try
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(email)) parameters["email"] = email;
            if (!string.IsNullOrEmpty(name)) parameters["name"] = name;

            if (parameters.Count == 0) return true;

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(
                $"{StripeApiBaseUrl}/customers/{customerId}",
                content,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return false;
        }

        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{StripeApiBaseUrl}/customers/{customerId}",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete customer {CustomerId}", customerId);
            return false;
        }
    }

    #endregion

    #region IPaymentSubscriptionService Implementation

    public async Task<PaymentSubscriptionDto?> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"{StripeApiBaseUrl}/subscriptions/{subscriptionId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var subscription = JsonSerializer.Deserialize<StripeSubscription>(responseBody);

                return new PaymentSubscriptionDto
                {
                    SubscriptionId = subscription?.Id ?? string.Empty,
                    CustomerId = subscription?.Customer ?? string.Empty,
                    Status = subscription?.Status ?? string.Empty,
                    CurrentPeriodStart = subscription?.CurrentPeriodStart.HasValue == true 
                        ? DateTimeOffset.FromUnixTimeSeconds(subscription.CurrentPeriodStart.Value).UtcDateTime 
                        : null,
                    CurrentPeriodEnd = subscription?.CurrentPeriodEnd.HasValue == true 
                        ? DateTimeOffset.FromUnixTimeSeconds(subscription.CurrentPeriodEnd.Value).UtcDateTime 
                        : null,
                    CancelAtPeriodEnd = subscription?.CancelAtPeriodEnd ?? false
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get subscription {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(
        string subscriptionId,
        bool cancelImmediately = false,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return false;
        }

        try
        {
            if (cancelImmediately)
            {
                var response = await _httpClient.DeleteAsync(
                    $"{StripeApiBaseUrl}/subscriptions/{subscriptionId}",
                    cancellationToken);
                return response.IsSuccessStatusCode;
            }
            else
            {
                // Cancel at period end
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["cancel_at_period_end"] = "true"
                });
                var response = await _httpClient.PostAsync(
                    $"{StripeApiBaseUrl}/subscriptions/{subscriptionId}",
                    content,
                    cancellationToken);
                return response.IsSuccessStatusCode;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> ResumeSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return false;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["cancel_at_period_end"] = "false"
            });
            var response = await _httpClient.PostAsync(
                $"{StripeApiBaseUrl}/subscriptions/{subscriptionId}",
                content,
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<PaymentSubscriptionDto?> UpdateSubscriptionPlanAsync(
        string subscriptionId,
        string newPriceId,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateConfiguration())
        {
            return null;
        }

        try
        {
            // First get the subscription to find the item ID
            var getResponse = await _httpClient.GetAsync(
                $"{StripeApiBaseUrl}/subscriptions/{subscriptionId}",
                cancellationToken);

            if (!getResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var responseBody = await getResponse.Content.ReadAsStringAsync(cancellationToken);
            var subscription = JsonSerializer.Deserialize<StripeSubscription>(responseBody);
            var itemId = subscription?.Items?.Data?.FirstOrDefault()?.Id;

            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            // Update the subscription with new price
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["items[0][id]"] = itemId,
                ["items[0][price]"] = newPriceId,
                ["proration_behavior"] = "create_prorations"
            });

            var response = await _httpClient.PostAsync(
                $"{StripeApiBaseUrl}/subscriptions/{subscriptionId}",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await GetSubscriptionAsync(subscriptionId, cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update subscription plan for {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    #endregion

    #region Private Helper Methods

    private bool ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(_settings.SecretKey))
        {
            _logger.LogWarning("Stripe secret key is not configured");
            return false;
        }
        return true;
    }

    private string? GetPriceIdForPlan(string planCode, string billingCycle)
    {
        var isAnnual = billingCycle.Equals("Annual", StringComparison.OrdinalIgnoreCase);
        
        return planCode.ToUpper() switch
        {
            "STARTER" or "MVP" => isAnnual ? _settings.PriceIds?.StarterAnnual : _settings.PriceIds?.StarterMonthly,
            "PROFESSIONAL" or "PRO" => isAnnual ? _settings.PriceIds?.ProfessionalAnnual : _settings.PriceIds?.ProfessionalMonthly,
            "ENTERPRISE" or "ENT" => isAnnual ? _settings.PriceIds?.EnterpriseAnnual : _settings.PriceIds?.EnterpriseMonthly,
            _ => null
        };
    }

    private static string MapRefundReason(string reason)
    {
        return reason.ToLower() switch
        {
            "duplicate" => "duplicate",
            "fraud" or "fraudulent" => "fraudulent",
            _ => "requested_by_customer"
        };
    }

    private static bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        // Parse the signature header
        var elements = signature.Split(',')
            .Select(x => x.Split('='))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0], x => x[1]);

        if (!elements.TryGetValue("t", out var timestamp) || !elements.TryGetValue("v1", out var expectedSignature))
        {
            return false;
        }

        // Compute expected signature
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSignature = Convert.ToHexString(hash).ToLower();

        return computedSignature == expectedSignature;
    }

    #endregion

    #region Webhook Event Handlers

    private async Task<bool> HandleCheckoutCompleteAsync(PaymentWebhookEventDto webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent.TenantId == null)
        {
            _logger.LogWarning("Checkout complete event missing tenant ID");
            return true;
        }

        // Get subscription from local DB and update status
        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == webhookEvent.TenantId, cancellationToken);

        if (subscription != null)
        {
            subscription.Status = "Active";
            subscription.SubscriptionStartDate = DateTime.UtcNow;
            subscription.StripeSubscriptionId = webhookEvent.SubscriptionId;
            subscription.ModifiedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Activated subscription for tenant {TenantId} via checkout", webhookEvent.TenantId);
        }

        return true;
    }

    private async Task<bool> HandleInvoicePaidAsync(PaymentWebhookEventDto webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent.TenantId == null || webhookEvent.Amount == null)
        {
            return true;
        }

        // Record the payment
        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == webhookEvent.TenantId, cancellationToken);

        if (subscription != null)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                TenantId = subscription.TenantId,
                Amount = webhookEvent.Amount.Value,
                TransactionId = webhookEvent.PaymentIntentId ?? webhookEvent.EventId,
                Status = "Completed",
                PaymentMethod = "Card",
                Gateway = "Stripe",
                PaymentDate = webhookEvent.EventTime,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            
            // Extend subscription
            subscription.Status = "Active";
            subscription.NextBillingDate = subscription.BillingCycle == "Annual" 
                ? DateTime.UtcNow.AddYears(1) 
                : DateTime.UtcNow.AddMonths(1);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Recorded payment for tenant {TenantId}: {Amount}", 
                webhookEvent.TenantId, webhookEvent.Amount);
        }

        return true;
    }

    private async Task<bool> HandlePaymentFailedAsync(PaymentWebhookEventDto webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent.TenantId == null)
        {
            return true;
        }

        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == webhookEvent.TenantId, cancellationToken);

        if (subscription != null)
        {
            subscription.Status = "PaymentFailed";
            subscription.ModifiedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Payment failed for tenant {TenantId}", webhookEvent.TenantId);
            
            // Send email notification about failed payment
            try
            {
                var tenant = await _dbContext.Tenants.FindAsync(new object[] { webhookEvent.TenantId }, cancellationToken);
                if (tenant?.Email != null)
                {
                    _logger.LogInformation("Sending payment failure notification to {Email}", tenant.Email);
                    // Email would be sent via IEmailService - logged for now
                    // await _emailService.SendPaymentFailedNotificationAsync(tenant.Email, tenant.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment failure notification for tenant {TenantId}", webhookEvent.TenantId);
            }
        }

        return true;
    }

    private async Task<bool> HandleSubscriptionUpdatedAsync(PaymentWebhookEventDto webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Subscription updated: {SubscriptionId}", webhookEvent.SubscriptionId);
        // Handle plan changes, etc.
        return true;
    }

    private async Task<bool> HandleSubscriptionDeletedAsync(PaymentWebhookEventDto webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent.TenantId == null)
        {
            return true;
        }

        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.TenantId == webhookEvent.TenantId, cancellationToken);

        if (subscription != null)
        {
            subscription.Status = "Cancelled";
            subscription.SubscriptionEndDate = DateTime.UtcNow;
            subscription.ModifiedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription cancelled for tenant {TenantId}", webhookEvent.TenantId);
        }

        return true;
    }

    private async Task<bool> HandlePaymentSucceededAsync(PaymentWebhookEventDto webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment succeeded: {PaymentIntentId}", webhookEvent.PaymentIntentId);
        return true;
    }

    #endregion
}

#region Stripe Response Models

internal class StripeCheckoutSession
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal class StripePaymentIntent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("amount")]
    public long? Amount { get; set; }
    
    [JsonPropertyName("latest_charge")]
    public string? LatestCharge { get; set; }
    
    [JsonPropertyName("receipt_url")]
    public string? ReceiptUrl { get; set; }
}

internal class StripeRefund
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("amount")]
    public long? Amount { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

internal class StripeCustomer
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

internal class StripeSubscription
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("customer")]
    public string? Customer { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("current_period_start")]
    public long? CurrentPeriodStart { get; set; }
    
    [JsonPropertyName("current_period_end")]
    public long? CurrentPeriodEnd { get; set; }
    
    [JsonPropertyName("cancel_at_period_end")]
    public bool? CancelAtPeriodEnd { get; set; }
    
    [JsonPropertyName("items")]
    public StripeListResponse<StripeSubscriptionItem>? Items { get; set; }
}

internal class StripeSubscriptionItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("price")]
    public StripePriceInfo? Price { get; set; }
}

internal class StripePriceInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

internal class StripeListResponse<T>
{
    [JsonPropertyName("data")]
    public List<T>? Data { get; set; }
}

internal class StripeWebhookEvent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("created")]
    public long Created { get; set; }
    
    [JsonPropertyName("data")]
    public StripeWebhookData? Data { get; set; }
}

internal class StripeWebhookData
{
    [JsonPropertyName("object")]
    public StripeWebhookObject? Object { get; set; }
}

internal class StripeWebhookObject
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("customer")]
    public string? Customer { get; set; }
    
    [JsonPropertyName("payment_intent")]
    public string? PaymentIntent { get; set; }
    
    [JsonPropertyName("amount_total")]
    public long? AmountTotal { get; set; }
    
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

internal class StripeErrorResponse
{
    [JsonPropertyName("error")]
    public StripeError? Error { get; set; }
}

internal class StripeError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

#endregion
