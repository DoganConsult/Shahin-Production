using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs;

/// <summary>
/// DTO for risk list display
/// </summary>
public class RiskListItemDto
{
    public Guid Id { get; set; }
    public string RiskNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, Mitigated, Accepted, Closed
    public int InherentScore { get; set; } // 1-25
    public int ResidualScore { get; set; } // 1-25
    public string ResidualRating { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string ResponsibleParty { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetClosureDate { get; set; }
    public int MitigationCount { get; set; }
}

/// <summary>
/// DTO for risk detail view
/// </summary>
public class RiskDetailDto
{
    public Guid Id { get; set; }
    public string RiskNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    
    // Scoring
    public int InherentScore { get; set; }
    public string InherentRating { get; set; } = "Medium";
    public int ResidualScore { get; set; }
    public string ResidualRating { get; set; } = "Medium";
    
    // Ownership & Tracking
    public string ResponsibleParty { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetClosureDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    
    // Impact & Likelihood
    public string Impact { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Likelihood { get; set; } = "Medium"; // Low, Medium, High
    public string ConsequenceArea { get; set; } = string.Empty;
    
    // Mitigations
    public List<RiskMitigationDto> Mitigations { get; set; } = new();
    
    // Audit Trail
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
}

/// <summary>
/// DTO for creating a new risk
/// </summary>
public class RiskCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Operational, Compliance, Strategic, Financial, etc.
    public string Description { get; set; } = string.Empty;
    public int InherentScore { get; set; }
    public int ResidualScore { get; set; }
    public string ResponsibleParty { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetClosureDate { get; set; }
    public string Impact { get; set; } = "Medium";
    public string Likelihood { get; set; } = "Medium";
    public string ConsequenceArea { get; set; } = string.Empty;
}

/// <summary>
/// DTO for editing a risk
/// </summary>
public class RiskEditDto
{
    public Guid Id { get; set; }
    public string RiskNumber { get; set; } = string.Empty; // Read-only
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public int InherentScore { get; set; }
    public int ResidualScore { get; set; }
    public string ResponsibleParty { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetClosureDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string Impact { get; set; } = "Medium";
    public string Likelihood { get; set; } = "Medium";
    public string ConsequenceArea { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public int MitigationCount { get; set; }
}

/// <summary>
/// DTO for risk mitigation tracking
/// </summary>
public class RiskMitigationDto
{
    public Guid Id { get; set; }
    public Guid RiskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Planned"; // Planned, In Progress, Completed
    public int PlannedEffectiveness { get; set; } // 1-10
    public int ActualEffectiveness { get; set; } // 1-10
    public string Owner { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal CostEstimate { get; set; }
    public decimal ActualCost { get; set; }
}

/// <summary>
/// DTO for creating risk mitigation
/// </summary>
public class CreateRiskMitigationDto
{
    public Guid RiskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PlannedEffectiveness { get; set; }
    public string Owner { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public decimal CostEstimate { get; set; }
}
