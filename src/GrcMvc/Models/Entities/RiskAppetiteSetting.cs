using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Risk Appetite Settings entity for defining organizational risk thresholds.
    /// 
    /// ABP Best Practice: Entity with multi-tenant support via TenantId.
    /// This defines the acceptable levels of risk for different categories.
    /// </summary>
    [Table("RiskAppetiteSettings")]
    public class RiskAppetiteSetting : BaseEntity
    {
        /// <summary>
        /// Tenant identifier for multi-tenant isolation.
        /// </summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Risk category this appetite setting applies to.
        /// Examples: Strategic, Operational, Financial, Compliance, Reputational
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name for this appetite setting.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the risk appetite for this category.
        /// </summary>
        [StringLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Minimum acceptable risk score (0-100).
        /// Risks below this are considered over-controlled.
        /// </summary>
        [Range(0, 100)]
        public int MinimumRiskScore { get; set; } = 0;

        /// <summary>
        /// Maximum acceptable risk score (0-100).
        /// Risks above this require immediate attention.
        /// </summary>
        [Range(0, 100)]
        public int MaximumRiskScore { get; set; } = 100;

        /// <summary>
        /// Target risk score the organization aims for (0-100).
        /// </summary>
        [Range(0, 100)]
        public int TargetRiskScore { get; set; } = 50;

        /// <summary>
        /// Tolerance percentage above target before escalation.
        /// </summary>
        [Range(0, 100)]
        public int TolerancePercentage { get; set; } = 10;

        /// <summary>
        /// Impact threshold for this category (1-5 scale).
        /// 1=Negligible, 2=Minor, 3=Moderate, 4=Major, 5=Severe
        /// </summary>
        [Range(1, 5)]
        public int ImpactThreshold { get; set; } = 3;

        /// <summary>
        /// Likelihood threshold for this category (1-5 scale).
        /// 1=Rare, 2=Unlikely, 3=Possible, 4=Likely, 5=Almost Certain
        /// </summary>
        [Range(1, 5)]
        public int LikelihoodThreshold { get; set; } = 3;

        /// <summary>
        /// Whether this appetite setting is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when this appetite setting was approved.
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// User ID of the approver.
        /// </summary>
        [StringLength(450)]
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Date when this setting expires and needs review.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Number of days before expiry to send review reminder.
        /// </summary>
        public int ReviewReminderDays { get; set; } = 30;

        /// <summary>
        /// Audit: Created date.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Audit: Created by user.
        /// </summary>
        [StringLength(450)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Audit: Last updated date.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Audit: Updated by user.
        /// </summary>
        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Navigation property to Tenant.
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Calculate if a given risk score is within appetite.
        /// </summary>
        public bool IsWithinAppetite(int riskScore)
        {
            return riskScore >= MinimumRiskScore && riskScore <= MaximumRiskScore;
        }

        /// <summary>
        /// Calculate if a given risk score exceeds tolerance.
        /// </summary>
        public bool ExceedsTolerance(int riskScore)
        {
            var toleranceScore = TargetRiskScore * (1 + TolerancePercentage / 100.0);
            return riskScore > toleranceScore;
        }

        /// <summary>
        /// Get the appetite status for a given risk score.
        /// </summary>
        public RiskAppetiteStatus GetAppetiteStatus(int riskScore)
        {
            if (riskScore < MinimumRiskScore)
                return RiskAppetiteStatus.UnderControlled;
            if (riskScore > MaximumRiskScore)
                return RiskAppetiteStatus.Exceeded;
            if (ExceedsTolerance(riskScore))
                return RiskAppetiteStatus.AtTolerance;
            if (Math.Abs(riskScore - TargetRiskScore) <= 5)
                return RiskAppetiteStatus.OnTarget;
            return RiskAppetiteStatus.WithinAppetite;
        }
    }

    /// <summary>
    /// Status of risk relative to appetite thresholds.
    /// </summary>
    public enum RiskAppetiteStatus
    {
        /// <summary>Risk is below minimum threshold - may indicate over-control.</summary>
        UnderControlled = 0,

        /// <summary>Risk is within target range.</summary>
        OnTarget = 1,

        /// <summary>Risk is within acceptable range.</summary>
        WithinAppetite = 2,

        /// <summary>Risk is approaching tolerance limits.</summary>
        AtTolerance = 3,

        /// <summary>Risk exceeds maximum acceptable level.</summary>
        Exceeded = 4
    }
}
