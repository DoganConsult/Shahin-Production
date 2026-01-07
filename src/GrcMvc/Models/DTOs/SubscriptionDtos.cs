using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for subscription plans
    /// </summary>
    public class SubscriptionPlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxAssessments { get; set; }
        public int MaxPolicies { get; set; }
        public bool HasAdvancedReporting { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasPrioritySupport { get; set; }
        public List<string> Features { get; set; } = new List<string>();
        public int DisplayOrder { get; set; }
    }

    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public Guid PlanId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? TrialEndDate { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string BillingCycle { get; set; } = string.Empty;
        public bool AutoRenew { get; set; }
        public int CurrentUserCount { get; set; }
        public SubscriptionPlanDto? Plan { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidDate { get; set; }
    }

    /// <summary>
    /// DTO for checkout/payment initiation
    /// </summary>
    public class CheckoutDto
    {
        public Guid PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly"; // Monthly or Annual
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for payment processing
    /// </summary>
    public class ProcessPaymentDto
    {
        public Guid SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodToken { get; set; } = string.Empty; // Stripe token
        public string Email { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
    }

    /// <summary>
    /// DTO for payment confirmation response
    /// </summary>
    public class PaymentConfirmationDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public SubscriptionDto? Subscription { get; set; }
        public InvoiceDto? Invoice { get; set; }
    }
}
