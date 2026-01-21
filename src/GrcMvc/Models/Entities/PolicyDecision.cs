using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Audit trail for policy decisions made by the GRC system.
    /// Tracks what decisions were made, why, and with what confidence.
    /// Immutable - decisions are never modified, only new ones created.
    /// </summary>
    public class PolicyDecision : BaseEntity
    {
        public Guid TenantId { get; set; }

        /// <summary>
        /// Type of policy evaluated:
        /// - ScopeDerivation: Which frameworks/baselines apply
        /// - WorkflowActivation: Which workflows to enable
        /// - TaskAssignment: Who should handle a task
        /// - PriorityCalculation: What priority level to assign
        /// - SlaCalculation: What SLA duration to apply
        /// - AccessControl: Permission decisions
        /// - RiskClassification: Risk level determination
        /// </summary>
        public string PolicyType { get; set; } = string.Empty;

        /// <summary>
        /// Version of policy/ruleset used for this decision
        /// </summary>
        public string PolicyVersion { get; set; } = "1.0";

        /// <summary>
        /// Hash of the input context for cache lookup
        /// </summary>
        public string ContextHash { get; set; } = string.Empty;

        /// <summary>
        /// Full context JSON used for decision (for audit replay)
        /// </summary>
        public string ContextJson { get; set; } = "{}";

        /// <summary>
        /// The decision outcome: Allow, Deny, Require, Skip, High, Medium, Low, etc.
        /// </summary>
        public string Decision { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable explanation of why this decision was made
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Number of rules evaluated to reach decision
        /// </summary>
        public int RulesEvaluated { get; set; }

        /// <summary>
        /// Number of rules that matched/triggered
        /// </summary>
        public int RulesMatched { get; set; }

        /// <summary>
        /// Confidence score 0-100 (higher = more certain)
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// When the decision was made
        /// </summary>
        public DateTime EvaluatedAt { get; set; }

        /// <summary>
        /// When this cached decision expires
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Whether this decision came from cache
        /// </summary>
        public bool IsCached { get; set; }

        /// <summary>
        /// Related entity type (Assessment, Workflow, etc.)
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Related entity ID
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// User who triggered the evaluation (or SYSTEM)
        /// </summary>
        public string EvaluatedBy { get; set; } = "SYSTEM";

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
