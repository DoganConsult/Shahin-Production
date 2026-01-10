using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for creating a new resilience assessment
    /// </summary>
    public class CreateResilienceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty; // Operational, Business Continuity, Disaster Recovery, Cyber Resilience
        public string Framework { get; set; } = string.Empty; // SAMA, ISO 22301, NIST CSF
        public string Scope { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public Guid? RelatedRiskId { get; set; }
    }

    /// <summary>
    /// DTO for updating resilience assessment
    /// </summary>
    public class UpdateResilienceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? ResilienceScore { get; set; }
        public decimal? BusinessContinuityScore { get; set; }
        public decimal? DisasterRecoveryScore { get; set; }
        public decimal? CyberResilienceScore { get; set; }
        public string? OverallRating { get; set; }
        public string? AssessmentDetails { get; set; }
        public string? Findings { get; set; }
        public string? ActionItems { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for resilience assessment response
    /// </summary>
    public class ResilienceDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? AssessmentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }
        public decimal? ResilienceScore { get; set; }
        public decimal? BusinessContinuityScore { get; set; }
        public decimal? DisasterRecoveryScore { get; set; }
        public decimal? CyberResilienceScore { get; set; }
        public string? OverallRating { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public Guid? RelatedRiskId { get; set; }
        public Guid? RelatedWorkflowInstanceId { get; set; }
        
        /// <summary>
        /// Recovery Time Objective in hours
        /// </summary>
        public int? RecoveryTimeObjective { get; set; }
        
        /// <summary>
        /// Recovery Point Objective in hours
        /// </summary>
        public int? RecoveryPointObjective { get; set; }
        
        /// <summary>
        /// Priority level (Critical, High, Medium, Low)
        /// </summary>
        public string? Priority { get; set; }
        
        /// <summary>
        /// Impact score (0-100)
        /// </summary>
        public decimal? ImpactScore { get; set; }
        
        /// <summary>
        /// Category of the resilience assessment
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// List of dependencies for BIA
        /// </summary>
        public List<ResilienceDependencyDto>? Dependencies { get; set; }
    }

    /// <summary>
    /// DTO for resilience dependency
    /// </summary>
    public class ResilienceDependencyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Criticality { get; set; } = string.Empty;
        public int? RecoveryPriority { get; set; }
    }

    /// <summary>
    /// DTO for creating a risk resilience assessment
    /// </summary>
    public class CreateRiskResilienceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RiskCategory { get; set; } = string.Empty;
        public string RiskType { get; set; } = string.Empty;
        public Guid? RelatedRiskId { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AssessedByUserId { get; set; }
    }

    /// <summary>
    /// DTO for risk resilience assessment response
    /// </summary>
    public class RiskResilienceDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RiskCategory { get; set; } = string.Empty;
        public string RiskType { get; set; } = string.Empty;
        public Guid? RelatedRiskId { get; set; }
        public decimal? RiskToleranceLevel { get; set; }
        public decimal? RecoveryCapabilityScore { get; set; }
        public decimal? ImpactMitigationScore { get; set; }
        public string? ResilienceRating { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? AssessmentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }
    }

    /// <summary>
    /// DTO for resilience assessment request (from workflow)
    /// </summary>
    public class ResilienceAssessmentRequestDto
    {
        public Guid? RelatedAssessmentId { get; set; }
        public Guid? RelatedRiskId { get; set; }
        public Guid? RelatedWorkflowInstanceId { get; set; }
        public string AssessmentType { get; set; } = "Operational";
        public string Framework { get; set; } = "SAMA";
        public Dictionary<string, object>? InputVariables { get; set; }
    }
}
