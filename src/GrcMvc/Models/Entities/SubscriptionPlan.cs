using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Defines subscription plans available in the system
    /// MVP, Professional, Enterprise with features and pricing
    /// </summary>
    public class SubscriptionPlan : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // MVP, Professional, Enterprise
        public string Code { get; set; } = string.Empty; // MVP, PRO, ENT
        public string Description { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxAssessments { get; set; }
        public int MaxPolicies { get; set; }
        public bool HasAdvancedReporting { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasPrioritySupport { get; set; }
        public string Features { get; set; } = string.Empty; // JSON array of features
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
