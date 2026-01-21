using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrcMvc.Models.Entities;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for reading Risk Appetite Settings.
    /// </summary>
    public class RiskAppetiteSettingDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MinimumRiskScore { get; set; }
        public int MaximumRiskScore { get; set; }
        public int TargetRiskScore { get; set; }
        public int TolerancePercentage { get; set; }
        public int ImpactThreshold { get; set; }
        public int LikelihoodThreshold { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int ReviewReminderDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        /// <summary>
        /// Whether the appetite is expiring soon (within review reminder period).
        /// </summary>
        public bool IsExpiringSoon { get; set; }

        /// <summary>
        /// Days until expiry (null if no expiry set).
        /// </summary>
        public int? DaysUntilExpiry { get; set; }
    }

    /// <summary>
    /// DTO for creating a new Risk Appetite Setting.
    /// </summary>
    public class CreateRiskAppetiteSettingDto
    {
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public int MinimumRiskScore { get; set; } = 0;

        [Range(0, 100)]
        public int MaximumRiskScore { get; set; } = 100;

        [Range(0, 100)]
        public int TargetRiskScore { get; set; } = 50;

        [Range(0, 100)]
        public int TolerancePercentage { get; set; } = 10;

        [Range(1, 5)]
        public int ImpactThreshold { get; set; } = 3;

        [Range(1, 5)]
        public int LikelihoodThreshold { get; set; } = 3;

        public DateTime? ExpiryDate { get; set; }

        public int ReviewReminderDays { get; set; } = 30;
    }

    /// <summary>
    /// DTO for updating an existing Risk Appetite Setting.
    /// </summary>
    public class UpdateRiskAppetiteSettingDto
    {
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public int MinimumRiskScore { get; set; }

        [Range(0, 100)]
        public int MaximumRiskScore { get; set; }

        [Range(0, 100)]
        public int TargetRiskScore { get; set; }

        [Range(0, 100)]
        public int TolerancePercentage { get; set; }

        [Range(1, 5)]
        public int ImpactThreshold { get; set; }

        [Range(1, 5)]
        public int LikelihoodThreshold { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int ReviewReminderDays { get; set; }
    }

    /// <summary>
    /// DTO for risk appetite comparison results.
    /// Shows how a risk compares against the configured appetite.
    /// </summary>
    public class RiskAppetiteComparisonDto
    {
        public Guid RiskId { get; set; }
        public string RiskTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentRiskScore { get; set; }
        public int TargetRiskScore { get; set; }
        public int MinimumRiskScore { get; set; }
        public int MaximumRiskScore { get; set; }
        public RiskAppetiteStatus Status { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public int DeviationFromTarget { get; set; }
        public double DeviationPercentage { get; set; }
        public bool RequiresAction { get; set; }
        public string? RecommendedAction { get; set; }
    }

    /// <summary>
    /// Summary of risk appetite status across the organization.
    /// </summary>
    public class RiskAppetiteSummaryDto
    {
        public Guid TenantId { get; set; }
        public int TotalRisks { get; set; }
        public int RisksWithinAppetite { get; set; }
        public int RisksAtTolerance { get; set; }
        public int RisksExceedingAppetite { get; set; }
        public int RisksUnderControlled { get; set; }
        public double OverallCompliancePercentage { get; set; }
        public List<CategoryAppetiteSummaryDto> ByCategory { get; set; } = new();
    }

    /// <summary>
    /// Risk appetite summary for a specific category.
    /// </summary>
    public class CategoryAppetiteSummaryDto
    {
        public string Category { get; set; } = string.Empty;
        public int TotalRisks { get; set; }
        public int WithinAppetite { get; set; }
        public int ExceedingAppetite { get; set; }
        public double AverageRiskScore { get; set; }
        public int TargetRiskScore { get; set; }
        public double CompliancePercentage { get; set; }
    }
}
