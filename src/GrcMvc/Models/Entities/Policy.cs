using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    public class Policy : BaseEntity
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public string PolicyCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, UnderReview, Approved, Active, Expired
        public bool IsActive { get; set; } = true;
        public string Owner { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public DateTime NextReviewDate { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<PolicyViolation> Violations { get; set; } = new List<PolicyViolation>();
    }
}