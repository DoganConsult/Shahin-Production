using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents an invoice for a subscription period
    /// Tracks invoice details, amounts, and payment status
    /// </summary>
    public class Invoice : BaseEntity
    {
        public Guid? SubscriptionId { get; set; }
        public Guid? TenantId { get; set; }
        
        /// <summary>
        /// Unique invoice number
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Due date for payment
        /// </summary>
        public DateTime DueDate { get; set; }
        
        /// <summary>
        /// Billing period start
        /// </summary>
        public DateTime PeriodStart { get; set; }
        
        /// <summary>
        /// Billing period end
        /// </summary>
        public DateTime PeriodEnd { get; set; }
        
        /// <summary>
        /// Subtotal before taxes
        /// </summary>
        public decimal SubTotal { get; set; }
        
        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Total amount due
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal AmountPaid { get; set; }
        
        /// <summary>
        /// Invoice Status: Draft, Sent, Viewed, Partial, Paid, Overdue, Cancelled
        /// </summary>
        public string Status { get; set; } = "Draft"; // Draft, Sent, Viewed, Partial, Paid, Overdue, Cancelled
        
        /// <summary>
        /// Notes or additional information
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Date when invoice was sent to customer
        /// </summary>
        public DateTime? SentDate { get; set; }
        
        /// <summary>
        /// Date when invoice was paid
        /// </summary>
        public DateTime? PaidDate { get; set; }

        // Navigation properties
        public virtual Subscription Subscription { get; set; } = null!;
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
