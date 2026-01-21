using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Operational Resilience Assessment Entity
    /// Tracks resilience assessments for business continuity, disaster recovery, and operational continuity
    /// </summary>
    public class Resilience : BaseEntity
    {
        public Guid? TenantId { get; set; }

        // Assessment identification
        public string AssessmentNumber { get; set; } = string.Empty; // RES-2026-0001
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Assessment type and scope
        public string AssessmentType { get; set; } = string.Empty; // Operational, Business Continuity, Disaster Recovery, Cyber Resilience
        public string Framework { get; set; } = string.Empty; // SAMA, ISO 22301, NIST CSF
        public string Scope { get; set; } = string.Empty; // Organization-wide, Department, System-specific

        // Status and lifecycle
        public string Status { get; set; } = "Draft"; // Draft, InProgress, Completed, Approved, Rejected
        public DateTime? AssessmentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Ownership and assignment
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }

        // Resilience metrics and scores
        public decimal? ResilienceScore { get; set; } // 0-100
        public decimal? BusinessContinuityScore { get; set; } // 0-100
        public decimal? DisasterRecoveryScore { get; set; } // 0-100
        public decimal? CyberResilienceScore { get; set; } // 0-100
        public string? OverallRating { get; set; } // Excellent, Good, Satisfactory, Needs Improvement, Critical

        // Assessment details (JSON)
        public string? AssessmentDetails { get; set; } // JSON: {criticalSystems: [], recoveryTimeObjectives: {}, recoveryPointObjectives: {}}
        public string? Findings { get; set; } // JSON: {strengths: [], weaknesses: [], recommendations: []}
        public string? ActionItems { get; set; } // JSON: {items: [{id, description, priority, dueDate, owner}]}

        // Related entities
        public Guid? RelatedAssessmentId { get; set; } // Link to parent Assessment
        public Guid? RelatedRiskId { get; set; } // Link to Risk
        public Guid? RelatedWorkflowInstanceId { get; set; } // Link to WorkflowInstance

        // Evidence and documentation
        public string? EvidenceUrls { get; set; } // JSON array of evidence file URLs
        public string? ReportUrl { get; set; } // Link to generated resilience report

        // Metadata
        public string? Tags { get; set; } // JSON array of tags
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Risk Resilience Assessment Entity
    /// Tracks risk resilience assessments focusing on risk tolerance and recovery capabilities
    /// </summary>
    public class RiskResilience : BaseEntity
    {
        public Guid? TenantId { get; set; }

        // Assessment identification
        public string AssessmentNumber { get; set; } = string.Empty; // RISK-RES-2026-0001
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Risk focus
        public string RiskCategory { get; set; } = string.Empty; // Financial, Operational, Strategic, Compliance, Reputational
        public string RiskType { get; set; } = string.Empty; // Inherent, Residual, Emerging
        public Guid? RelatedRiskId { get; set; } // Link to Risk entity

        // Resilience metrics
        public decimal? RiskToleranceLevel { get; set; } // 0-100
        public decimal? RecoveryCapabilityScore { get; set; } // 0-100
        public decimal? ImpactMitigationScore { get; set; } // 0-100
        public string? ResilienceRating { get; set; } // High, Medium, Low

        // Assessment details
        public string? RiskScenario { get; set; } // Description of risk scenario
        public string? ResilienceMeasures { get; set; } // JSON: {preventive: [], detective: [], corrective: []}
        public string? RecoveryPlan { get; set; } // JSON: {steps: [], resources: [], timeline: {}}

        // Status and lifecycle
        public string Status { get; set; } = "Draft"; // Draft, InProgress, Completed, Approved
        public DateTime? AssessmentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Ownership
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }

        // Related entities
        public Guid? RelatedWorkflowInstanceId { get; set; } // Link to WorkflowInstance
        public Guid? RelatedAssessmentId { get; set; } // Link to parent Assessment

        // Metadata
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
