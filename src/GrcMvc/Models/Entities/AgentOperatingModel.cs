using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region Agent Role Definitions

/// <summary>
/// AI Agent Definition - Defines an autonomous agent with specific capabilities
/// Agents handle repeatable execution while humans govern and approve
/// </summary>
public class AgentDefinition : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string AgentCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Agent type: Evidence, Monitoring, Testing, Mapping, AuditResponse, Workflow
    /// </summary>
    [Required]
    [StringLength(30)]
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// Capabilities this agent has (JSON array)
    /// </summary>
    public string? CapabilitiesJson { get; set; }

    /// <summary>
    /// Data sources this agent can access (JSON array)
    /// </summary>
    public string? DataSourcesJson { get; set; }

    /// <summary>
    /// Actions this agent can perform (JSON array)
    /// </summary>
    public string? AllowedActionsJson { get; set; }

    /// <summary>
    /// Actions that require human approval (JSON array)
    /// </summary>
    public string? ApprovalRequiredActionsJson { get; set; }

    /// <summary>
    /// Maximum confidence threshold for auto-approval (0-100)
    /// Actions above this threshold can proceed without human review
    /// </summary>
    public int AutoApprovalConfidenceThreshold { get; set; } = 95;

    /// <summary>
    /// Human oversight role code (who supervises this agent)
    /// </summary>
    [StringLength(50)]
    public string? OversightRoleCode { get; set; }

    /// <summary>
    /// Escalation role code (who handles agent failures)
    /// </summary>
    [StringLength(50)]
    public string? EscalationRoleCode { get; set; }

    /// <summary>
    /// Is this agent active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Version for change control
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ICollection<AgentAction> Actions { get; set; } = new List<AgentAction>();
    public virtual ICollection<AgentApprovalGate> ApprovalGates { get; set; } = new List<AgentApprovalGate>();
}

/// <summary>
/// Agent Capability - Specific capability an agent has
/// </summary>
public class AgentCapability : BaseEntity
{
    public Guid AgentId { get; set; }

    [ForeignKey("AgentId")]
    public virtual AgentDefinition Agent { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string CapabilityCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Capability category: Read, Write, Execute, Suggest, Notify
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Category { get; set; } = "Read";

    /// <summary>
    /// Risk level of this capability: Low, Medium, High, Critical
    /// </summary>
    [StringLength(20)]
    public string RiskLevel { get; set; } = "Low";

    /// <summary>
    /// Requires human approval for each use?
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// Maximum uses per hour (rate limiting)
    /// </summary>
    public int? MaxUsesPerHour { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Agent Actions & Audit Trail

/// <summary>
/// Agent Action - Immutable audit trail of every agent action
/// "who/what/when" for full traceability
/// </summary>
public class AgentAction : BaseEntity
{
    public Guid TenantId { get; set; }

    public Guid AgentId { get; set; }

    [ForeignKey("AgentId")]
    public virtual AgentDefinition Agent { get; set; } = null!;

    /// <summary>
    /// Unique action correlation ID
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ActionCorrelationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Action type: CollectEvidence, RunTest, CreateMapping, GenerateReport,
    /// CreateTicket, SendNotification, UpdateStatus, SuggestMapping
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Action description
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ActionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Target object type: Control, Evidence, Mapping, Ticket, Assessment
    /// </summary>
    [StringLength(50)]
    public string? TargetObjectType { get; set; }

    /// <summary>
    /// Target object ID
    /// </summary>
    public Guid? TargetObjectId { get; set; }

    /// <summary>
    /// Input data (JSON)
    /// </summary>
    public string? InputDataJson { get; set; }

    /// <summary>
    /// Output data (JSON)
    /// </summary>
    public string? OutputDataJson { get; set; }

    /// <summary>
    /// Confidence score (0-100) for this action/decision
    /// </summary>
    public int? ConfidenceScore { get; set; }

    /// <summary>
    /// Reasoning/rationale for the action
    /// </summary>
    [StringLength(2000)]
    public string? Reasoning { get; set; }

    /// <summary>
    /// Action status: Pending, Completed, Failed, AwaitingApproval, Approved, Rejected
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Did this action require human approval?
    /// </summary>
    public bool RequiredApproval { get; set; } = false;

    /// <summary>
    /// Approval gate ID (if approval was required)
    /// </summary>
    public Guid? ApprovalGateId { get; set; }

    /// <summary>
    /// Was this action approved by human?
    /// </summary>
    public bool? WasApproved { get; set; }

    /// <summary>
    /// Approved/rejected by (human user ID)
    /// </summary>
    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Approval notes
    /// </summary>
    [StringLength(1000)]
    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Triggered by (parent action ID for chained actions)
    /// </summary>
    public Guid? TriggeredByActionId { get; set; }

    /// <summary>
    /// Immutable timestamp
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Approval Gates

/// <summary>
/// Agent Approval Gate - Defines when human approval is required
/// High-risk actions must go through approval gates
/// </summary>
public class AgentApprovalGate : BaseEntity
{
    public Guid AgentId { get; set; }

    [ForeignKey("AgentId")]
    public virtual AgentDefinition Agent { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string GateCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Action types that trigger this gate (comma-separated)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string TriggerActionTypes { get; set; } = string.Empty;

    /// <summary>
    /// Condition for triggering (JSON expression)
    /// </summary>
    public string? TriggerConditionJson { get; set; }

    /// <summary>
    /// Approver role code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ApproverRoleCode { get; set; } = string.Empty;

    /// <summary>
    /// Approval SLA in hours
    /// </summary>
    public int ApprovalSLAHours { get; set; } = 24;

    /// <summary>
    /// Escalation role code if SLA breached
    /// </summary>
    [StringLength(50)]
    public string? EscalationRoleCode { get; set; }

    /// <summary>
    /// Auto-reject if not approved within X hours (0 = no auto-reject)
    /// </summary>
    public int AutoRejectHours { get; set; } = 0;

    /// <summary>
    /// Minimum confidence score to bypass gate (0 = never bypass)
    /// </summary>
    public int BypassConfidenceThreshold { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Pending Approval - Queue of actions awaiting human approval
/// </summary>
public class PendingApproval : BaseEntity
{
    public Guid TenantId { get; set; }

    public Guid ActionId { get; set; }

    [ForeignKey("ActionId")]
    public virtual AgentAction Action { get; set; } = null!;

    public Guid ApprovalGateId { get; set; }

    [ForeignKey("ApprovalGateId")]
    public virtual AgentApprovalGate ApprovalGate { get; set; } = null!;

    /// <summary>
    /// Approval status: Pending, Approved, Rejected, Expired, Escalated
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Assigned approver (user ID)
    /// </summary>
    [StringLength(100)]
    public string? AssignedApproverId { get; set; }

    [StringLength(255)]
    public string? AssignedApproverName { get; set; }

    /// <summary>
    /// Due date/time for approval
    /// </summary>
    public DateTime DueAt { get; set; }

    /// <summary>
    /// Is overdue?
    /// </summary>
    public bool IsOverdue => DateTime.UtcNow > DueAt && Status == "Pending";

    /// <summary>
    /// Reminder sent?
    /// </summary>
    public bool ReminderSent { get; set; } = false;

    /// <summary>
    /// Escalated?
    /// </summary>
    public bool IsEscalated { get; set; } = false;

    public DateTime? EscalatedAt { get; set; }

    /// <summary>
    /// Decision: Approve, Reject
    /// </summary>
    [StringLength(20)]
    public string? Decision { get; set; }

    /// <summary>
    /// Decision notes
    /// </summary>
    [StringLength(2000)]
    public string? DecisionNotes { get; set; }

    /// <summary>
    /// Decided by
    /// </summary>
    [StringLength(100)]
    public string? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Agent Confidence Scoring

/// <summary>
/// Agent Confidence Score - Records confidence level for agent decisions
/// Low confidence = human review required
/// </summary>
public class AgentConfidenceScore : BaseEntity
{
    public Guid ActionId { get; set; }

    [ForeignKey("ActionId")]
    public virtual AgentAction Action { get; set; } = null!;

    /// <summary>
    /// Overall confidence score (0-100)
    /// </summary>
    public int OverallScore { get; set; } = 0;

    /// <summary>
    /// Confidence level: VeryHigh (90+), High (75-89), Medium (50-74), Low (25-49), VeryLow (<25)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ConfidenceLevel { get; set; } = "Medium";

    /// <summary>
    /// Score breakdown by factor (JSON)
    /// </summary>
    public string? ScoreBreakdownJson { get; set; }

    /// <summary>
    /// Factors that reduced confidence (JSON array)
    /// </summary>
    public string? LowConfidenceFactorsJson { get; set; }

    /// <summary>
    /// Recommended action: AutoApprove, HumanReview, Reject
    /// </summary>
    [StringLength(20)]
    public string RecommendedAction { get; set; } = "HumanReview";

    /// <summary>
    /// Was human review triggered?
    /// </summary>
    public bool HumanReviewTriggered { get; set; } = false;

    /// <summary>
    /// Human review outcome
    /// </summary>
    [StringLength(20)]
    public string? HumanReviewOutcome { get; set; }

    /// <summary>
    /// Human reviewer feedback
    /// </summary>
    [StringLength(1000)]
    public string? HumanReviewerFeedback { get; set; }

    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Agent Segregation of Duties

/// <summary>
/// Agent SoD Rule - Prevents same agent from performing conflicting actions
/// e.g., Agent that collects evidence cannot approve it
/// </summary>
public class AgentSoDRule : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RuleCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Action 1 (conflicting with Action 2)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Action1 { get; set; } = string.Empty;

    /// <summary>
    /// Action 1 agent types (comma-separated)
    /// </summary>
    [StringLength(255)]
    public string? Action1AgentTypes { get; set; }

    /// <summary>
    /// Action 2 (conflicting with Action 1)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Action2 { get; set; } = string.Empty;

    /// <summary>
    /// Action 2 agent types (comma-separated)
    /// </summary>
    [StringLength(255)]
    public string? Action2AgentTypes { get; set; }

    /// <summary>
    /// Risk if both actions performed by same agent
    /// </summary>
    [StringLength(1000)]
    public string? RiskDescription { get; set; }

    /// <summary>
    /// Enforcement: Block, Warn, Log
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Enforcement { get; set; } = "Block";

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Agent SoD Violation - Detected violation of agent segregation of duties
/// </summary>
public class AgentSoDViolation : BaseEntity
{
    public Guid TenantId { get; set; }

    public Guid RuleId { get; set; }

    [ForeignKey("RuleId")]
    public virtual AgentSoDRule Rule { get; set; } = null!;

    public Guid Action1Id { get; set; }

    [ForeignKey("Action1Id")]
    public virtual AgentAction Action1 { get; set; } = null!;

    public Guid Action2Id { get; set; }

    /// <summary>
    /// Violation status: Detected, Blocked, Allowed, Reviewed
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Detected";

    /// <summary>
    /// Was the action blocked?
    /// </summary>
    public bool WasBlocked { get; set; } = true;

    /// <summary>
    /// Override approved by (if allowed despite violation)
    /// </summary>
    [StringLength(100)]
    public string? OverrideApprovedBy { get; set; }

    [StringLength(1000)]
    public string? OverrideReason { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Human-Retained Responsibilities

/// <summary>
/// Human-Retained Responsibility - Defines what humans must always do
/// Accountability is not delegable to agents
/// </summary>
public class HumanRetainedResponsibility : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ResponsibilityCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Category: RiskAcceptance, Attestation, Exception, Interpretation, Governance
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Role code that holds this responsibility
    /// </summary>
    [Required]
    [StringLength(50)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// Why this cannot be delegated to agents
    /// </summary>
    [StringLength(1000)]
    public string? NonDelegableReason { get; set; }

    /// <summary>
    /// Regulatory requirement reference (if any)
    /// </summary>
    [StringLength(255)]
    public string? RegulatoryReference { get; set; }

    /// <summary>
    /// What agents CAN do to support this responsibility
    /// </summary>
    [StringLength(1000)]
    public string? AgentSupportDescription { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Role Transition Plan - Maps current human roles to agent+human model
/// </summary>
public class RoleTransitionPlan : BaseEntity
{
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string PlanCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string CurrentRoleName { get; set; } = string.Empty;

    /// <summary>
    /// Current automation level (0-100%)
    /// </summary>
    public int CurrentAutomationPercent { get; set; } = 0;

    /// <summary>
    /// Target automation level (0-100%)
    /// </summary>
    public int TargetAutomationPercent { get; set; } = 70;

    /// <summary>
    /// Tasks that will be automated (JSON array)
    /// </summary>
    public string? TasksToAutomateJson { get; set; }

    /// <summary>
    /// Tasks that will remain human (JSON array)
    /// </summary>
    public string? TasksToRetainJson { get; set; }

    /// <summary>
    /// Agent(s) that will take over automated tasks
    /// </summary>
    [StringLength(255)]
    public string? AssignedAgentCodes { get; set; }

    /// <summary>
    /// New human role after transition (e.g., "Reviewer" instead of "Tester")
    /// </summary>
    [StringLength(255)]
    public string? NewHumanRole { get; set; }

    /// <summary>
    /// Transition phase: Planning, Pilot, Rollout, Complete
    /// </summary>
    [StringLength(20)]
    public string Phase { get; set; } = "Planning";

    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime? TargetCompletionDate { get; set; }

    /// <summary>
    /// Risk mitigation notes
    /// </summary>
    [StringLength(2000)]
    public string? RiskMitigationNotes { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Standard Agent Types (Seed Data)

/// <summary>
/// Standard agent type definitions for seed data
/// </summary>
public static class StandardAgentTypes
{
    public static readonly Dictionary<string, AgentTypeInfo> Types = new()
    {
        ["EVIDENCE_AGENT"] = new AgentTypeInfo
        {
            Code = "EVIDENCE_AGENT",
            Name = "Evidence Collection Agent",
            Description = "Pulls data from ERP/IAM/SIEM/ITSM, generates evidence packs, validates completeness",
            Capabilities = new[] { "PullData", "GenerateEvidencePack", "ValidateCompleteness", "TagEvidence", "StoreEvidence" },
            ApprovalRequiredActions = new[] { "DeleteEvidence", "ModifyEvidence" },
            AutoApprovalThreshold = 90
        },
        ["MONITORING_AGENT"] = new AgentTypeInfo
        {
            Code = "MONITORING_AGENT",
            Name = "Control Monitoring Agent",
            Description = "Runs KRIs/KPIs, detects breaches, opens tickets and escalates",
            Capabilities = new[] { "RunKRI", "DetectBreach", "CreateTicket", "SendNotification", "Escalate" },
            ApprovalRequiredActions = new[] { "DisableControl", "ModifyThreshold" },
            AutoApprovalThreshold = 85
        },
        ["TESTING_AGENT"] = new AgentTypeInfo
        {
            Code = "TESTING_AGENT",
            Name = "Control Testing Agent",
            Description = "Executes test scripts, records results, proposes pass/fail with rationale",
            Capabilities = new[] { "ExecuteTest", "RecordResult", "ProposeOutcome", "GenerateWorkpaper" },
            ApprovalRequiredActions = new[] { "MarkControlEffective", "MarkControlIneffective" },
            AutoApprovalThreshold = 80
        },
        ["MAPPING_AGENT"] = new AgentTypeInfo
        {
            Code = "MAPPING_AGENT",
            Name = "Framework Mapping Agent",
            Description = "Maps new requirements to canonical controls, flags gaps and overlaps",
            Capabilities = new[] { "SuggestMapping", "IdentifyGap", "IdentifyOverlap", "MaintainCrosswalk" },
            ApprovalRequiredActions = new[] { "ApproveMapping", "DeleteMapping" },
            AutoApprovalThreshold = 75
        },
        ["AUDIT_RESPONSE_AGENT"] = new AgentTypeInfo
        {
            Code = "AUDIT_RESPONSE_AGENT",
            Name = "Audit Response Agent",
            Description = "Assembles audit package, drafts responses, tracks auditor requests",
            Capabilities = new[] { "AssemblePackage", "DraftResponse", "TrackRequest", "LinkEvidence" },
            ApprovalRequiredActions = new[] { "SubmitToAuditor", "CloseAuditRequest" },
            AutoApprovalThreshold = 70
        },
        ["WORKFLOW_AGENT"] = new AgentTypeInfo
        {
            Code = "WORKFLOW_AGENT",
            Name = "Workflow Orchestration Agent",
            Description = "Routes tasks, manages deadlines, sends reminders, handles escalations",
            Capabilities = new[] { "RouteTask", "SendReminder", "Escalate", "UpdateStatus" },
            ApprovalRequiredActions = new[] { "OverrideSLA", "ReassignTask" },
            AutoApprovalThreshold = 85
        }
    };
}

public class AgentTypeInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Capabilities { get; set; } = Array.Empty<string>();
    public string[] ApprovalRequiredActions { get; set; } = Array.Empty<string>();
    public int AutoApprovalThreshold { get; set; } = 80;
}

#endregion
