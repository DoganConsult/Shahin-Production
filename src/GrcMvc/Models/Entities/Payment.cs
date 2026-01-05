using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Records payment transactions for subscriptions
    /// Tracks payment status, amount, and method
    /// </summary>
    public class Payment : BaseEntity
    {
        public Guid? SubscriptionId { get; set; }
        public Guid? TenantId { get; set; }
        
        /// <summary>
        /// Unique transaction ID from payment provider
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Currency (USD, EUR, etc.)
        /// </summary>
        public string Currency { get; set; } = "USD";
        
        /// <summary>
        /// Payment Status: Pending, Completed, Failed, Refunded
        /// </summary>
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        
        /// <summary>
        /// Payment Method: CreditCard, BankTransfer, Check, etc.
        /// </summary>
        public string PaymentMethod { get; set; } = "CreditCard";
        
        /// <summary>
        /// Payment gateway: Stripe, PayPal, etc.
        /// </summary>
        public string Gateway { get; set; } = "Stripe";
        
        /// <summary>
        /// Payment date
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Invoice associated with this payment
        /// </summary>
        public Guid? InvoiceId { get; set; }
        
        /// <summary>
        /// Error message if payment failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Additional payment details (JSON)
        /// </summary>
        public string? PaymentDetails { get; set; }

        // Navigation properties
        public virtual Subscription Subscription { get; set; } = null!;
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual Invoice? Invoice { get; set; }
    }
}
