using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Dtos;

/// <summary>
/// DTO for control list display
/// </summary>
public class ControlListItemDto
{
    public Guid Id { get; set; }
    public string ControlNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Detective, Preventive, Corrective
    public string Category { get; set; } = string.Empty; // Administrative, Technical, Physical
    public string Status { get; set; } = "Active"; // Active, Inactive, Pending, Decommissioned
    public string Effectiveness { get; set; } = "Effective"; // Effective, Partially Effective, Ineffective
    public string Owner { get; set; } = string.Empty;
    public DateTime? LastTestedDate { get; set; }
    public int LinkedRiskCount { get; set; }
    public int LinkedAssessmentCount { get; set; }
}

/// <summary>
/// DTO for control detail view
/// </summary>
public class ControlDetailDto
{
    public Guid Id { get; set; }
    public string ControlNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Effectiveness { get; set; } = "Effective";
    public string Owner { get; set; } = string.Empty;
    public string KeyPersonnel { get; set; } = string.Empty;
    
    // Testing & Assessment
    public DateTime? LastTestedDate { get; set; }
    public string TestingFrequency { get; set; } = "Quarterly"; // Monthly, Quarterly, Semi-Annually, Annually
    public int EffectivenessScore { get; set; } // 1-100
    
    // Linked Items
    public List<Guid> LinkedRiskIds { get; set; } = new();
    public List<Guid> LinkedAssessmentIds { get; set; } = new();
    public List<Guid> LinkedPolicyIds { get; set; } = new();
    
    // Evidence & Documentation
    public List<Guid> LinkedEvidenceIds { get; set; } = new();
    
    // Audit Trail
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
}

/// <summary>
/// DTO for creating a new control
/// </summary>
public class ControlCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string KeyPersonnel { get; set; } = string.Empty;
    public string TestingFrequency { get; set; } = "Quarterly";
    public int EffectivenessScore { get; set; }
}

/// <summary>
/// DTO for editing a control
/// </summary>
public class ControlEditDto
{
    public Guid Id { get; set; }
    public string ControlNumber { get; set; } = string.Empty; // Read-only
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Effectiveness { get; set; } = "Effective";
    public string Owner { get; set; } = string.Empty;
    public string KeyPersonnel { get; set; } = string.Empty;
    public DateTime? LastTestedDate { get; set; }
    public string TestingFrequency { get; set; } = "Quarterly";
    public int EffectivenessScore { get; set; }
}

/// <summary>
/// DTO for control test result
/// </summary>
public class ControlTestResultDto
{
    public Guid Id { get; set; }
    public Guid ControlId { get; set; }
    public DateTime TestDate { get; set; }
    public string TestType { get; set; } = string.Empty; // Observation, Document Review, Testing, etc.
    public string Result { get; set; } = "Passed"; // Passed, Failed, Partial
    public string Findings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string Tester { get; set; } = string.Empty;
    public int EffectivenessScore { get; set; }
}

/// <summary>
/// DTO for creating a control test result
/// </summary>
public class CreateControlTestResultDto
{
    public Guid ControlId { get; set; }
    public DateTime TestDate { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string Result { get; set; } = "Passed";
    public string Findings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string Tester { get; set; } = string.Empty;
    public int EffectivenessScore { get; set; }
}
