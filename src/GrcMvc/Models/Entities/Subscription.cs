using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents an active subscription for a tenant
    /// Tracks subscription status, billing period, and payment details
    /// </summary>
    public class Subscription : BaseEntity
    {
        public Guid? TenantId { get; set; }
        public Guid PlanId { get; set; }
        
        /// <summary>
        /// Subscription Status: Trial, Active, Suspended, Cancelled, Expired
        /// </summary>
        public string Status { get; set; } = "Trial"; // Trial, Active, Suspended, Cancelled, Expired
        
        /// <summary>
        /// Trial period end date (null if not in trial)
        /// </summary>
        public DateTime? TrialEndDate { get; set; }
        
        /// <summary>
        /// Subscription start date
        /// </summary>
        public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Subscription end date (null for active subscriptions)
        /// </summary>
        public DateTime? SubscriptionEndDate { get; set; }
        
        /// <summary>
        /// Next billing date
        /// </summary>
        public DateTime? NextBillingDate { get; set; }
        
        /// <summary>
        /// Billing cycle: Monthly or Annual
        /// </summary>
        public string BillingCycle { get; set; } = "Monthly"; // Monthly, Annual
        
        /// <summary>
        /// Auto-renewal enabled
        /// </summary>
        public bool AutoRenew { get; set; } = true;
        
        /// <summary>
        /// Current number of active users
        /// </summary>
        public int CurrentUserCount { get; set; } = 1;

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual SubscriptionPlan Plan { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
