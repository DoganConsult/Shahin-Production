using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Integrations;

namespace GrcMvc.Services.Integrations;

/// <summary>
/// Stripe Payment Gateway Service Implementation
/// Implements IPaymentGatewayService for Stripe payment processing
/// Note: This is the comprehensive implementation. The simpler IPaymentIntegrationService
/// implementation exists in IntegrationServices.cs for backward compatibility.
/// </summary>
public class StripePaymentGatewayService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentGatewayService> _logger;
    private readonly string _apiKey;

    public StripePaymentGatewayService(
        IConfiguration configuration,
        ILogger<StripePaymentGatewayService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Get Stripe API key from configuration
        _apiKey = _configuration["Stripe:SecretKey"] ?? 
                  _configuration["Stripe__SecretKey"] ?? 
                  Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? 
                  string.Empty;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Stripe API key not configured. Payment processing will fail.");
        }
        else
        {
            // Initialize Stripe with API key
            StripeConfiguration.ApiKey = _apiKey;
            _logger.LogInformation("Stripe payment service initialized");
        }
    }

    public async Task<CheckoutSessionOutputDto> CreateCheckoutSessionAsync(
        CreateCheckoutSessionInputDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new CheckoutSessionOutputDto
                {
                    Success = false,
                    ErrorCode = "CONFIGURATION_ERROR",
                    ErrorMessage = "Stripe API key not configured"
                };
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "sar", // Saudi Riyal
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Subscription Plan: {input.PlanCode}",
                                Description = $"Billing Cycle: {input.BillingCycle}"
                            },
                            UnitAmount = (long)(input.BillingCycle == "Annual" 
                                ? GetAnnualAmount(input.PlanCode) * 100 // Convert to cents
                                : GetMonthlyAmount(input.PlanCode) * 100),
                            Recurring = new SessionLineItemPriceDataRecurringOptions
                            {
                                Interval = input.BillingCycle == "Annual" ? "year" : "month"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = input.SuccessUrl,
                CancelUrl = input.CancelUrl,
                CustomerEmail = input.CustomerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "TenantId", input.TenantId.ToString() },
                    { "SubscriptionId", input.SubscriptionId.ToString() },
                    { "PlanCode", input.PlanCode },
                    { "BillingCycle", input.BillingCycle }
                }
            };

            // Add custom metadata if provided
            if (input.Metadata != null)
            {
                foreach (var kvp in input.Metadata)
                {
                    options.Metadata[kvp.Key] = kvp.Value;
                }
            }

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created Stripe checkout session: {SessionId} for tenant {TenantId}, subscription {SubscriptionId}",
                session.Id, input.TenantId, input.SubscriptionId);

            return new CheckoutSessionOutputDto
            {
                Success = true,
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session for tenant {TenantId}", input.TenantId);
            return new CheckoutSessionOutputDto
            {
                Success = false,
                ErrorCode = ex.StripeError?.Code ?? "STRIPE_ERROR",
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session for tenant {TenantId}", input.TenantId);
            return new CheckoutSessionOutputDto
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentResultOutputDto> ProcessPaymentAsync(
        ProcessPaymentInputDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new PaymentResultOutputDto
                {
                    Success = false,
                    ErrorCode = "CONFIGURATION_ERROR",
                    ErrorMessage = "Stripe API key not configured"
                };
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(input.Amount * 100), // Convert to cents
                Currency = input.Currency.ToLower(),
                PaymentMethod = input.PaymentMethodId,
                ConfirmationMethod = "manual",
                Confirm = true,
                Description = input.Description ?? $"Subscription payment for tenant {input.TenantId}",
                Metadata = new Dictionary<string, string>
                {
                    { "TenantId", input.TenantId.ToString() },
                    { "SubscriptionId", input.SubscriptionId.ToString() }
                }
            };

            // Add custom metadata if provided
            if (input.Metadata != null)
            {
                foreach (var kvp in input.Metadata)
                {
                    options.Metadata[kvp.Key] = kvp.Value;
                }
            }

            // Add idempotency key if provided
            if (!string.IsNullOrEmpty(input.IdempotencyKey))
            {
                options.IdempotencyKey = input.IdempotencyKey;
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created Stripe payment intent: {PaymentIntentId} for tenant {TenantId}, amount {Amount} {Currency}",
                paymentIntent.Id, input.TenantId, input.Amount, input.Currency);

            return new PaymentResultOutputDto
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                TransactionId = paymentIntent.Id,
                Status = paymentIntent.Status,
                AmountPaid = paymentIntent.Amount / 100m, // Convert from cents
                ProcessedAt = paymentIntent.Created,
                ReceiptUrl = paymentIntent.Charges?.Data?.FirstOrDefault()?.ReceiptUrl
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing payment for tenant {TenantId}", input.TenantId);
            return new PaymentResultOutputDto
            {
                Success = false,
                ErrorCode = ex.StripeError?.Code ?? "STRIPE_ERROR",
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for tenant {TenantId}", input.TenantId);
            return new PaymentResultOutputDto
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RefundResultDto> ProcessRefundAsync(
        RefundRequestDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new RefundResultDto
                {
                    Success = false,
                    ErrorMessage = "Stripe API key not configured"
                };
            }

            var options = new RefundCreateOptions
            {
                PaymentIntent = input.PaymentIntentId,
                Amount = input.Amount.HasValue ? (long)(input.Amount.Value * 100) : null, // null = full refund
                Reason = input.Reason switch
                {
                    "duplicate" => "duplicate",
                    "fraudulent" => "fraudulent",
                    "requested_by_customer" => "requested_by_customer",
                    _ => "requested_by_customer"
                }
            };

            // Add metadata if provided
            if (input.Metadata != null)
            {
                options.Metadata = new Dictionary<string, string>();
                foreach (var kvp in input.Metadata)
                {
                    options.Metadata[kvp.Key] = kvp.Value;
                }
            }

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Processed Stripe refund: {RefundId} for payment intent {PaymentIntentId}, amount {Amount}",
                refund.Id, input.PaymentIntentId, refund.Amount / 100m);

            return new RefundResultDto
            {
                Success = refund.Status == "succeeded" || refund.Status == "pending",
                RefundId = refund.Id,
                AmountRefunded = refund.Amount / 100m, // Convert from cents
                Status = refund.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing refund for payment intent {PaymentIntentId}", input.PaymentIntentId);
            return new RefundResultDto
            {
                Success = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment intent {PaymentIntentId}", input.PaymentIntentId);
            return new RefundResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentResultOutputDto?> GetPaymentStatusAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return null;
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            return new PaymentResultOutputDto
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                TransactionId = paymentIntent.Id,
                Status = paymentIntent.Status,
                AmountPaid = paymentIntent.Amount / 100m, // Convert from cents
                ProcessedAt = paymentIntent.Created,
                ReceiptUrl = paymentIntent.Charges?.Data?.FirstOrDefault()?.ReceiptUrl
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting payment status for {PaymentIntentId}", paymentIntentId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for {PaymentIntentId}", paymentIntentId);
            return null;
        }
    }

    // Helper methods to get plan amounts (should be configured or retrieved from database)
    private decimal GetMonthlyAmount(string planCode)
    {
        // TODO: Retrieve from database or configuration
        return planCode.ToUpper() switch
        {
            "BASIC" => 99m,
            "PROFESSIONAL" => 299m,
            "ENTERPRISE" => 999m,
            _ => 99m
        };
    }

    private decimal GetAnnualAmount(string planCode)
    {
        // Annual pricing is typically 10 months (2 months free)
        return GetMonthlyAmount(planCode) * 10m;
    }
}
