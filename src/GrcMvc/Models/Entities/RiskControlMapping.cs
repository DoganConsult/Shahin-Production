using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Many-to-many relationship between Risk and Control with metadata
/// Tracks which controls mitigate which risks and how effectively
/// </summary>
public class RiskControlMapping : BaseEntity
{
    [Required]
    public Guid RiskId { get; set; }
    
    [ForeignKey("RiskId")]
    public virtual Risk? Risk { get; set; }
    
    [Required]
    public Guid ControlId { get; set; }
    
    [ForeignKey("ControlId")]
    public virtual Control? Control { get; set; }
    
    /// <summary>
    /// Mapping strength: Primary (main control), Secondary (supporting), Tertiary (indirect)
    /// </summary>
    [MaxLength(50)]
    public string MappingStrength { get; set; } = "Primary";
    
    /// <summary>
    /// Expected effectiveness percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ExpectedEffectiveness { get; set; } = 0;
    
    /// <summary>
    /// Actual measured effectiveness percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ActualEffectiveness { get; set; } = 0;
    
    /// <summary>
    /// Rationale for why this control mitigates this risk
    /// </summary>
    [MaxLength(2000)]
    public string Rationale { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this mapping is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Last assessment date
    /// </summary>
    public DateTime? LastAssessedDate { get; set; }
    
    /// <summary>
    /// Who performed the last assessment
    /// </summary>
    [MaxLength(256)]
    public string? LastAssessedBy { get; set; }
    
    /// <summary>
    /// Notes from the last assessment
    /// </summary>
    [MaxLength(2000)]
    public string? AssessmentNotes { get; set; }
}
