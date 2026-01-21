using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Services.Integrations;

/// <summary>
/// ASP.NET Core / ABP Best Practice: Interface Segregation Principle
/// Separate interfaces for different payment gateway responsibilities
/// </summary>

#region DTOs - Following ABP DTO naming conventions

/// <summary>
/// Input DTO for creating a checkout session
/// ABP Convention: Use *Dto suffix, Input suffix for creation DTOs
/// </summary>
public record CreateCheckoutSessionInputDto
{
    public Guid TenantId { get; init; }
    public Guid SubscriptionId { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string BillingCycle { get; init; } = "Monthly"; // Monthly or Annual
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public string? CustomerName { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Output DTO for checkout session
/// </summary>
public record CheckoutSessionOutputDto
{
    public bool Success { get; init; }
    public string? SessionId { get; init; }
    public string? CheckoutUrl { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Input DTO for processing payment intent
/// </summary>
public record ProcessPaymentInputDto
{
    public Guid TenantId { get; init; }
    public Guid SubscriptionId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "SAR"; // Saudi Riyal default
    public string PaymentMethodId { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? IdempotencyKey { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Output DTO for payment result
/// </summary>
public record PaymentResultOutputDto
{
    public bool Success { get; init; }
    public string? PaymentIntentId { get; init; }
    public string? TransactionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal AmountPaid { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? ReceiptUrl { get; init; }
}

/// <summary>
/// Webhook event DTO
/// </summary>
public record PaymentWebhookEventDto
{
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public string? SubscriptionId { get; set; }
    public string? CustomerId { get; set; }
    public Guid? TenantId { get; set; }
    public decimal? Amount { get; set; }
    public string? Status { get; set; }
    public DateTime EventTime { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Customer information DTO
/// </summary>
public record PaymentCustomerDto
{
    public string CustomerId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public Guid TenantId { get; init; }
}

/// <summary>
/// Subscription info from payment provider
/// </summary>
public record PaymentSubscriptionDto
{
    public string SubscriptionId { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string PriceId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? CurrentPeriodStart { get; init; }
    public DateTime? CurrentPeriodEnd { get; init; }
    public DateTime? CancelAt { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
}

/// <summary>
/// Refund request DTO
/// </summary>
public record RefundRequestDto
{
    public string PaymentIntentId { get; init; } = string.Empty;
    public decimal? Amount { get; init; } // null = full refund
    public string Reason { get; init; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Refund result DTO
/// </summary>
public record RefundResultDto
{
    public bool Success { get; init; }
    public string? RefundId { get; init; }
    public decimal AmountRefunded { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
}

#endregion

#region Interfaces - Following ABP/DDD patterns

/// <summary>
/// Core payment gateway interface
/// ABP Best Practice: Single Responsibility - only payment processing
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Create a checkout session for subscription payment
    /// </summary>
    Task<CheckoutSessionOutputDto> CreateCheckoutSessionAsync(
        CreateCheckoutSessionInputDto input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a direct payment (for saved payment methods)
    /// </summary>
    Task<PaymentResultOutputDto> ProcessPaymentAsync(
        ProcessPaymentInputDto input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a refund
    /// </summary>
    Task<RefundResultDto> ProcessRefundAsync(
        RefundRequestDto input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment intent status
    /// </summary>
    Task<PaymentResultOutputDto?> GetPaymentStatusAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Webhook processing interface
/// ABP Best Practice: Separate interface for webhook handling
/// </summary>
public interface IPaymentWebhookHandler
{
    /// <summary>
    /// Verify and parse webhook payload
    /// </summary>
    Task<PaymentWebhookEventDto?> VerifyAndParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a verified webhook event
    /// </summary>
    Task<bool> HandleWebhookEventAsync(
        PaymentWebhookEventDto webhookEvent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Customer management interface
/// ABP Best Practice: Separate interface for customer operations
/// </summary>
public interface IPaymentCustomerService
{
    /// <summary>
    /// Create or retrieve a customer in the payment provider
    /// </summary>
    Task<PaymentCustomerDto?> GetOrCreateCustomerAsync(
        Guid tenantId,
        string email,
        string? name = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update customer information
    /// </summary>
    Task<bool> UpdateCustomerAsync(
        string customerId,
        string? email = null,
        string? name = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete customer and all associated data
    /// </summary>
    Task<bool> DeleteCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Subscription management interface
/// ABP Best Practice: Separate interface for subscription operations
/// </summary>
public interface IPaymentSubscriptionService
{
    /// <summary>
    /// Get subscription details from payment provider
    /// </summary>
    Task<PaymentSubscriptionDto?> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    Task<bool> CancelSubscriptionAsync(
        string subscriptionId,
        bool cancelImmediately = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume a cancelled subscription
    /// </summary>
    Task<bool> ResumeSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update subscription plan
    /// </summary>
    Task<PaymentSubscriptionDto?> UpdateSubscriptionPlanAsync(
        string subscriptionId,
        string newPriceId,
        CancellationToken cancellationToken = default);
}

#endregion
