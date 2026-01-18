using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities.Onboarding;
using GrcMvc.Models.Entities.Compliance;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// 43-Layer Architecture: Onboarding Control Plane Service
    /// Manages answer snapshots, rules evaluation, derived outputs, and explainability.
    /// </summary>
    public interface IOnboardingControlPlaneService
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 14: Answer Snapshots (Immutable Audit Trail)
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Create an immutable snapshot of wizard answers after step completion.
        /// </summary>
        Task<OnboardingAnswerSnapshot> CreateSnapshotAsync(
            Guid wizardId,
            int completedStep,
            string sectionCode,
            string answersJson,
            string userId,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Get all snapshots for a wizard (for audit replay).
        /// </summary>
        Task<List<OnboardingAnswerSnapshot>> GetSnapshotsAsync(Guid wizardId);

        /// <summary>
        /// Get the latest snapshot for a wizard.
        /// </summary>
        Task<OnboardingAnswerSnapshot?> GetLatestSnapshotAsync(Guid wizardId);

        /// <summary>
        /// Mark a snapshot as final (wizard completed).
        /// </summary>
        Task<OnboardingAnswerSnapshot> MarkSnapshotAsFinalAsync(Guid snapshotId);

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 15-16: Rules Evaluation & Derived Outputs
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Evaluate rules for a step and create derived outputs.
        /// </summary>
        Task<RulesEvaluationResult> EvaluateRulesAsync(
            Guid wizardId,
            int triggerStep,
            Dictionary<string, object> inputContext);

        /// <summary>
        /// Get all derived outputs for a wizard.
        /// </summary>
        Task<List<OnboardingDerivedOutput>> GetDerivedOutputsAsync(Guid wizardId, string? outputType = null);

        /// <summary>
        /// Create a derived output (baseline, overlay, package, etc.).
        /// </summary>
        Task<OnboardingDerivedOutput> CreateDerivedOutputAsync(
            Guid wizardId,
            string outputType,
            string outputCode,
            string outputName,
            string outputPayloadJson,
            string applicability,
            int priority,
            int derivedAtStep,
            Guid? rulesEvaluationLogId = null);

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 17: Explainability (Why Decisions Were Made)
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Create an explainability payload for a decision.
        /// </summary>
        Task<ExplainabilityPayload> CreateExplainabilityAsync(
            Guid tenantId,
            string decisionType,
            string subjectCode,
            string subjectName,
            string decision,
            string primaryReason,
            string? primaryReasonAr = null,
            Dictionary<string, object>? inputFactors = null,
            List<string>? supportingReferences = null,
            Guid? wizardId = null,
            Guid? rulesEvaluationLogId = null);

        /// <summary>
        /// Get explainability payloads for a tenant.
        /// </summary>
        Task<List<ExplainabilityPayload>> GetExplainabilityAsync(
            Guid tenantId,
            string? decisionType = null,
            string? subjectCode = null);

        /// <summary>
        /// Override a decision with justification.
        /// </summary>
        Task<ExplainabilityPayload> OverrideDecisionAsync(
            Guid payloadId,
            string newDecision,
            string justification,
            string userId);

        // ═══════════════════════════════════════════════════════════════════════════════
        // LAYER 31-35: Tenant Compliance Resolution
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Derive tenant framework selections from wizard answers.
        /// </summary>
        Task<List<TenantFrameworkSelection>> DeriveFrameworkSelectionsAsync(Guid tenantId, Guid wizardId);

        /// <summary>
        /// Apply overlays based on wizard answers.
        /// </summary>
        Task<List<TenantOverlay>> ApplyOverlaysAsync(Guid tenantId, Guid wizardId);

        /// <summary>
        /// Resolve control set for tenant.
        /// </summary>
        Task<List<TenantControlSet>> ResolveControlSetAsync(Guid tenantId);

        /// <summary>
        /// Create scope boundaries from wizard answers.
        /// </summary>
        Task<List<TenantScopeBoundary>> CreateScopeBoundariesAsync(Guid tenantId, Guid wizardId);

        /// <summary>
        /// Calculate tenant risk profile from wizard answers.
        /// </summary>
        Task<TenantRiskProfile> CalculateRiskProfileAsync(Guid tenantId, Guid wizardId);

        // ═══════════════════════════════════════════════════════════════════════════════
        // COMPLETE ONBOARDING FLOW
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Complete onboarding and derive all outputs.
        /// Creates snapshots, evaluates rules, derives frameworks/overlays/controls.
        /// </summary>
        Task<OnboardingCompletionResult> CompleteOnboardingAsync(Guid tenantId, Guid wizardId, string userId);
    }

    /// <summary>
    /// Result of rules evaluation.
    /// </summary>
    public class RulesEvaluationResult
    {
        public List<Models.Entities.Onboarding.RulesEvaluationLog> Evaluations { get; set; } = new();
        public List<OnboardingDerivedOutput> DerivedOutputs { get; set; } = new();
        public List<ExplainabilityPayload> Explanations { get; set; } = new();
        public int MatchedRules { get; set; }
        public int TotalRulesEvaluated { get; set; }
    }

    /// <summary>
    /// Result of onboarding completion.
    /// </summary>
    public class OnboardingCompletionResult
    {
        public bool Success { get; set; }
        public Guid TenantId { get; set; }
        public Guid WizardId { get; set; }
        public DateTime CompletedAt { get; set; }

        // Snapshot
        public OnboardingAnswerSnapshot? FinalSnapshot { get; set; }

        // Derived outputs
        public List<TenantFrameworkSelection> FrameworkSelections { get; set; } = new();
        public List<TenantOverlay> Overlays { get; set; } = new();
        public List<TenantControlSet> ControlSet { get; set; } = new();
        public List<TenantScopeBoundary> ScopeBoundaries { get; set; } = new();
        public TenantRiskProfile? RiskProfile { get; set; }

        // Explanations
        public List<ExplainabilityPayload> Explanations { get; set; } = new();

        // Errors
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Count properties for logging
        public int FrameworkSelectionsCount => FrameworkSelections.Count;
        public int OverlaysCount => Overlays.Count;
        public int ControlsResolvedCount => ControlSet.Count;
        public int ScopeBoundariesCount => ScopeBoundaries.Count;
        public int ExplanationsCount => Explanations.Count;
    }
}
