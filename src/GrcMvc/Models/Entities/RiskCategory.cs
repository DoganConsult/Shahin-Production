using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Risk taxonomy: Hierarchical categorization of risks
/// Supports multi-level classification (Strategic → Operational → Process)
/// </summary>
public class RiskCategory : BaseEntity
{
    /// <summary>
    /// Category name (e.g., "Operational", "Strategic", "Compliance")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Arabic name for the category
    /// </summary>
    [MaxLength(200)]
    public string NameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Arabic description
    /// </summary>
    [MaxLength(2000)]
    public string DescriptionAr { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent category for hierarchical structure
    /// Null = top-level category
    /// </summary>
    public Guid? ParentCategoryId { get; set; }
    
    [ForeignKey("ParentCategoryId")]
    public virtual RiskCategory? ParentCategory { get; set; }
    
    /// <summary>
    /// Child categories
    /// </summary>
    public virtual ICollection<RiskCategory> ChildCategories { get; set; } = new List<RiskCategory>();
    
    /// <summary>
    /// Category code for reference (e.g., "OP", "STR", "CMP")
    /// </summary>
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Default risk appetite for this category (1-5)
    /// </summary>
    [Range(1, 5)]
    public int DefaultRiskAppetite { get; set; } = 3;
    
    /// <summary>
    /// Escalation threshold: score at which auto-escalation triggers
    /// </summary>
    [Range(1, 25)]
    public int EscalationThreshold { get; set; } = 15;
    
    /// <summary>
    /// Roles that should be notified when risks in this category exceed threshold
    /// </summary>
    [MaxLength(1000)]
    public string EscalationRoles { get; set; } = string.Empty; // JSON array of role names
    
    /// <summary>
    /// Whether this category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Icon class for UI display
    /// </summary>
    [MaxLength(100)]
    public string IconClass { get; set; } = "fas fa-exclamation-triangle";
    
    /// <summary>
    /// Color code for UI display
    /// </summary>
    [MaxLength(20)]
    public string ColorCode { get; set; } = "#ffc107";
}

/// <summary>
/// Risk type classification within a category
/// e.g., Under "Operational" category: "IT", "HR", "Supply Chain"
/// </summary>
public class RiskType : BaseEntity
{
    /// <summary>
    /// Type name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Arabic name
    /// </summary>
    [MaxLength(200)]
    public string NameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// Type description
    /// </summary>
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent category
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public virtual RiskCategory? Category { get; set; }
    
    /// <summary>
    /// Type code for reference
    /// </summary>
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this type is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Risk treatment options
/// Formal treatment options: Mitigate, Accept, Avoid, Transfer
/// </summary>
public class RiskTreatment : BaseEntity
{
    /// <summary>
    /// The risk being treated
    /// </summary>
    [Required]
    public Guid RiskId { get; set; }
    
    [ForeignKey("RiskId")]
    public virtual Risk? Risk { get; set; }
    
    /// <summary>
    /// Treatment type: Mitigate, Accept, Avoid, Transfer
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TreatmentType { get; set; } = "Mitigate";
    
    /// <summary>
    /// Treatment description/plan
    /// </summary>
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Treatment status: Planned, InProgress, Completed, Cancelled
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Planned";
    
    /// <summary>
    /// Person responsible for executing treatment
    /// </summary>
    [MaxLength(256)]
    public string Owner { get; set; } = string.Empty;
    
    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime? TargetDate { get; set; }
    
    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? CompletionDate { get; set; }
    
    /// <summary>
    /// Expected residual risk after treatment (1-25)
    /// </summary>
    [Range(1, 25)]
    public int ExpectedResidualRisk { get; set; } = 5;
    
    /// <summary>
    /// Actual residual risk after treatment (1-25)
    /// </summary>
    [Range(1, 25)]
    public int? ActualResidualRisk { get; set; }
    
    /// <summary>
    /// Estimated cost of treatment
    /// </summary>
    public decimal EstimatedCost { get; set; } = 0;
    
    /// <summary>
    /// Actual cost of treatment
    /// </summary>
    public decimal? ActualCost { get; set; }
    
    /// <summary>
    /// For Transfer: Name of third party receiving risk
    /// </summary>
    [MaxLength(500)]
    public string TransferParty { get; set; } = string.Empty;
    
    /// <summary>
    /// For Accept: Justification for acceptance
    /// </summary>
    [MaxLength(2000)]
    public string AcceptanceJustification { get; set; } = string.Empty;
    
    /// <summary>
    /// Approver for this treatment decision
    /// </summary>
    [MaxLength(256)]
    public string ApprovedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Approval date
    /// </summary>
    public DateTime? ApprovalDate { get; set; }
    
    /// <summary>
    /// Linked controls implementing this treatment
    /// </summary>
    public virtual ICollection<RiskTreatmentControl> TreatmentControls { get; set; } = new List<RiskTreatmentControl>();
}

/// <summary>
/// Link between risk treatment and controls
/// </summary>
public class RiskTreatmentControl : BaseEntity
{
    [Required]
    public Guid TreatmentId { get; set; }
    
    [ForeignKey("TreatmentId")]
    public virtual RiskTreatment? Treatment { get; set; }
    
    [Required]
    public Guid ControlId { get; set; }
    
    [ForeignKey("ControlId")]
    public virtual Control? Control { get; set; }
    
    /// <summary>
    /// Expected effectiveness of this control for the treatment (0-100%)
    /// </summary>
    [Range(0, 100)]
    public int ExpectedEffectiveness { get; set; } = 50;
    
    /// <summary>
    /// Implementation status
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Planned";
}
