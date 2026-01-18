using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Enums
{
    /// <summary>
    /// Unified workflow instance status - single source of truth.
    /// All workflow-related code must use these values.
    /// </summary>
    public enum WorkflowInstanceStatus
    {
        /// <summary>Workflow created but not yet started</summary>
        Pending = 0,
        
        /// <summary>Workflow is actively being processed</summary>
        InProgress = 1,
        
        /// <summary>Workflow is awaiting approval decision</summary>
        InApproval = 2,
        
        /// <summary>Workflow completed successfully</summary>
        Completed = 3,
        
        /// <summary>Workflow was rejected</summary>
        Rejected = 4,
        
        /// <summary>Workflow is temporarily suspended</summary>
        Suspended = 5,
        
        /// <summary>Workflow was cancelled by user</summary>
        Cancelled = 6,
        
        /// <summary>Workflow failed due to error</summary>
        Failed = 7
    }

    /// <summary>
    /// Unified workflow task status - single source of truth.
    /// All workflow task code must use these values.
    /// </summary>
    public enum WorkflowTaskStatus
    {
        /// <summary>Task created but not started</summary>
        Pending = 0,
        
        /// <summary>Task is being worked on</summary>
        InProgress = 1,
        
        /// <summary>Task completed/approved successfully</summary>
        Approved = 2,
        
        /// <summary>Task was rejected</summary>
        Rejected = 3,
        
        /// <summary>Task was skipped (e.g., conditional path)</summary>
        Skipped = 4,
        
        /// <summary>Task was cancelled</summary>
        Cancelled = 5
    }

    /// <summary>
    /// Workflow execution status for tracking BPMN execution state.
    /// </summary>
    public enum WorkflowExecutionStatus
    {
        /// <summary>Execution is running</summary>
        Running = 0,
        
        /// <summary>Execution completed successfully</summary>
        Completed = 1,
        
        /// <summary>Execution failed with error</summary>
        Failed = 2,
        
        /// <summary>Execution was cancelled</summary>
        Cancelled = 3
    }

    /// <summary>
    /// Evidence verification workflow status.
    /// </summary>
    public enum EvidenceVerificationStatus
    {
        /// <summary>Initial draft state</summary>
        Draft = 0,
        
        /// <summary>Pending review</summary>
        Pending = 1,
        
        /// <summary>Under review by approver</summary>
        UnderReview = 2,
        
        /// <summary>Verified/approved</summary>
        Verified = 3,
        
        /// <summary>Rejected - needs revision</summary>
        Rejected = 4,
        
        /// <summary>Archived after verification</summary>
        Archived = 5
    }

    /// <summary>
    /// Risk workflow status.
    /// </summary>
    public enum RiskWorkflowStatus
    {
        /// <summary>Risk identified but not yet assessed</summary>
        Identified = 0,
        
        /// <summary>Risk is being assessed</summary>
        UnderAssessment = 1,
        
        /// <summary>Risk assessed, pending decision</summary>
        PendingDecision = 2,
        
        /// <summary>Risk has been accepted</summary>
        Accepted = 3,
        
        /// <summary>Risk requires mitigation</summary>
        RequiresMitigation = 4,
        
        /// <summary>Risk has been mitigated</summary>
        Mitigated = 5,
        
        /// <summary>Risk is being monitored</summary>
        Monitoring = 6,
        
        /// <summary>Risk has been closed</summary>
        Closed = 7
    }

    /// <summary>
    /// State machine for workflow instance transitions.
    /// Defines valid state transitions to prevent invalid state changes.
    /// </summary>
    public static class WorkflowStateMachine
    {
        private static readonly Dictionary<WorkflowInstanceStatus, HashSet<WorkflowInstanceStatus>> ValidTransitions =
            new()
            {
                { WorkflowInstanceStatus.Pending, new HashSet<WorkflowInstanceStatus> 
                    { WorkflowInstanceStatus.InProgress, WorkflowInstanceStatus.InApproval, 
                      WorkflowInstanceStatus.Rejected, WorkflowInstanceStatus.Cancelled,
                      WorkflowInstanceStatus.Completed } },
                    
                { WorkflowInstanceStatus.InProgress, new HashSet<WorkflowInstanceStatus> 
                    { WorkflowInstanceStatus.InApproval, WorkflowInstanceStatus.Completed, 
                      WorkflowInstanceStatus.Rejected, WorkflowInstanceStatus.Suspended, 
                      WorkflowInstanceStatus.Cancelled, WorkflowInstanceStatus.Failed } },
                      
                { WorkflowInstanceStatus.InApproval, new HashSet<WorkflowInstanceStatus> 
                    { WorkflowInstanceStatus.Completed, WorkflowInstanceStatus.Rejected, 
                      WorkflowInstanceStatus.InProgress, WorkflowInstanceStatus.Cancelled } },
                      
                { WorkflowInstanceStatus.Suspended, new HashSet<WorkflowInstanceStatus> 
                    { WorkflowInstanceStatus.InProgress, WorkflowInstanceStatus.Cancelled } },
                    
                { WorkflowInstanceStatus.Rejected, new HashSet<WorkflowInstanceStatus> 
                    { WorkflowInstanceStatus.InProgress } }, // Allow resubmission
                    
                { WorkflowInstanceStatus.Completed, new HashSet<WorkflowInstanceStatus>() }, // Terminal state
                { WorkflowInstanceStatus.Cancelled, new HashSet<WorkflowInstanceStatus>() }, // Terminal state
                { WorkflowInstanceStatus.Failed, new HashSet<WorkflowInstanceStatus> 
                    { WorkflowInstanceStatus.InProgress } } // Allow retry
            };

        private static readonly Dictionary<WorkflowTaskStatus, HashSet<WorkflowTaskStatus>> ValidTaskTransitions =
            new()
            {
                { WorkflowTaskStatus.Pending, new HashSet<WorkflowTaskStatus> 
                    { WorkflowTaskStatus.InProgress, WorkflowTaskStatus.Skipped, WorkflowTaskStatus.Cancelled } },
                    
                { WorkflowTaskStatus.InProgress, new HashSet<WorkflowTaskStatus> 
                    { WorkflowTaskStatus.Approved, WorkflowTaskStatus.Rejected, WorkflowTaskStatus.Cancelled } },
                    
                { WorkflowTaskStatus.Rejected, new HashSet<WorkflowTaskStatus> 
                    { WorkflowTaskStatus.InProgress } }, // Allow retry
                    
                { WorkflowTaskStatus.Approved, new HashSet<WorkflowTaskStatus>() }, // Terminal state
                { WorkflowTaskStatus.Skipped, new HashSet<WorkflowTaskStatus>() }, // Terminal state
                { WorkflowTaskStatus.Cancelled, new HashSet<WorkflowTaskStatus>() } // Terminal state
            };

        /// <summary>
        /// Check if a workflow instance state transition is valid.
        /// </summary>
        public static bool CanTransition(WorkflowInstanceStatus from, WorkflowInstanceStatus to)
        {
            if (from == to) return true; // No-op is always valid
            return ValidTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
        }

        /// <summary>
        /// Check if a workflow task state transition is valid.
        /// </summary>
        public static bool CanTransitionTask(WorkflowTaskStatus from, WorkflowTaskStatus to)
        {
            if (from == to) return true; // No-op is always valid
            return ValidTaskTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
        }

        /// <summary>
        /// Get valid target states for a workflow instance.
        /// </summary>
        public static IReadOnlySet<WorkflowInstanceStatus> GetValidTransitions(WorkflowInstanceStatus from)
        {
            return ValidTransitions.TryGetValue(from, out var targets) 
                ? targets 
                : new HashSet<WorkflowInstanceStatus>();
        }

        /// <summary>
        /// Get valid target states for a workflow task.
        /// </summary>
        public static IReadOnlySet<WorkflowTaskStatus> GetValidTaskTransitions(WorkflowTaskStatus from)
        {
            return ValidTaskTransitions.TryGetValue(from, out var targets) 
                ? targets 
                : new HashSet<WorkflowTaskStatus>();
        }

        /// <summary>
        /// Check if a workflow instance status is terminal (no further transitions allowed).
        /// </summary>
        public static bool IsTerminal(WorkflowInstanceStatus status)
        {
            return status == WorkflowInstanceStatus.Completed || 
                   status == WorkflowInstanceStatus.Cancelled;
        }

        /// <summary>
        /// Check if a workflow task status is terminal.
        /// </summary>
        public static bool IsTerminalTask(WorkflowTaskStatus status)
        {
            return status == WorkflowTaskStatus.Approved || 
                   status == WorkflowTaskStatus.Skipped ||
                   status == WorkflowTaskStatus.Cancelled;
        }
    }

    /// <summary>
    /// State machine for evidence verification workflow.
    /// </summary>
    public static class EvidenceStateMachine
    {
        private static readonly Dictionary<EvidenceVerificationStatus, HashSet<EvidenceVerificationStatus>> ValidTransitions =
            new()
            {
                { EvidenceVerificationStatus.Draft, new HashSet<EvidenceVerificationStatus> 
                    { EvidenceVerificationStatus.Pending } },
                    
                { EvidenceVerificationStatus.Pending, new HashSet<EvidenceVerificationStatus> 
                    { EvidenceVerificationStatus.UnderReview } },
                    
                { EvidenceVerificationStatus.UnderReview, new HashSet<EvidenceVerificationStatus> 
                    { EvidenceVerificationStatus.Verified, EvidenceVerificationStatus.Rejected } },
                    
                { EvidenceVerificationStatus.Rejected, new HashSet<EvidenceVerificationStatus> 
                    { EvidenceVerificationStatus.Pending } }, // Allow resubmission
                    
                { EvidenceVerificationStatus.Verified, new HashSet<EvidenceVerificationStatus> 
                    { EvidenceVerificationStatus.Archived } },
                    
                { EvidenceVerificationStatus.Archived, new HashSet<EvidenceVerificationStatus>() } // Terminal
            };

        public static bool CanTransition(EvidenceVerificationStatus from, EvidenceVerificationStatus to)
        {
            if (from == to) return true;
            return ValidTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
        }

        public static IReadOnlySet<EvidenceVerificationStatus> GetValidTransitions(EvidenceVerificationStatus from)
        {
            return ValidTransitions.TryGetValue(from, out var targets) 
                ? targets 
                : new HashSet<EvidenceVerificationStatus>();
        }
    }

    /// <summary>
    /// State machine for risk workflow.
    /// </summary>
    public static class RiskStateMachine
    {
        private static readonly Dictionary<RiskWorkflowStatus, HashSet<RiskWorkflowStatus>> ValidTransitions =
            new()
            {
                { RiskWorkflowStatus.Identified, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.UnderAssessment } },
                    
                { RiskWorkflowStatus.UnderAssessment, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.PendingDecision } },
                    
                { RiskWorkflowStatus.PendingDecision, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.Accepted, RiskWorkflowStatus.RequiresMitigation } },
                    
                { RiskWorkflowStatus.Accepted, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.Monitoring, RiskWorkflowStatus.Closed } },
                    
                { RiskWorkflowStatus.RequiresMitigation, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.Mitigated } },
                    
                { RiskWorkflowStatus.Mitigated, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.Monitoring, RiskWorkflowStatus.Closed } },
                    
                { RiskWorkflowStatus.Monitoring, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.Closed, RiskWorkflowStatus.RequiresMitigation } }, // Can re-enter mitigation
                    
                { RiskWorkflowStatus.Closed, new HashSet<RiskWorkflowStatus> 
                    { RiskWorkflowStatus.Identified } } // Can reopen
            };

        public static bool CanTransition(RiskWorkflowStatus from, RiskWorkflowStatus to)
        {
            if (from == to) return true;
            return ValidTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
        }

        public static IReadOnlySet<RiskWorkflowStatus> GetValidTransitions(RiskWorkflowStatus from)
        {
            return ValidTransitions.TryGetValue(from, out var targets) 
                ? targets 
                : new HashSet<RiskWorkflowStatus>();
        }
    }

    /// <summary>
    /// Extension methods for converting between enums and string values.
    /// These provide backward compatibility with existing string-based status fields.
    /// </summary>
    public static class WorkflowEnumExtensions
    {
        /// <summary>
        /// Convert string status to WorkflowInstanceStatus enum.
        /// </summary>
        public static WorkflowInstanceStatus ToInstanceStatus(this string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "pending" => WorkflowInstanceStatus.Pending,
                "inprogress" or "in progress" or "active" or "running" => WorkflowInstanceStatus.InProgress,
                "inapproval" or "in approval" => WorkflowInstanceStatus.InApproval,
                "completed" => WorkflowInstanceStatus.Completed,
                "rejected" => WorkflowInstanceStatus.Rejected,
                "suspended" => WorkflowInstanceStatus.Suspended,
                "cancelled" or "canceled" => WorkflowInstanceStatus.Cancelled,
                "failed" => WorkflowInstanceStatus.Failed,
                _ => WorkflowInstanceStatus.Pending
            };
        }

        /// <summary>
        /// Convert WorkflowInstanceStatus enum to string for database storage.
        /// </summary>
        public static string ToStatusString(this WorkflowInstanceStatus status)
        {
            return status switch
            {
                WorkflowInstanceStatus.Pending => "Pending",
                WorkflowInstanceStatus.InProgress => "InProgress",
                WorkflowInstanceStatus.InApproval => "InApproval",
                WorkflowInstanceStatus.Completed => "Completed",
                WorkflowInstanceStatus.Rejected => "Rejected",
                WorkflowInstanceStatus.Suspended => "Suspended",
                WorkflowInstanceStatus.Cancelled => "Cancelled",
                WorkflowInstanceStatus.Failed => "Failed",
                _ => "Pending"
            };
        }

        /// <summary>
        /// Convert string status to WorkflowTaskStatus enum.
        /// </summary>
        public static WorkflowTaskStatus ToTaskStatus(this string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "pending" => WorkflowTaskStatus.Pending,
                "inprogress" or "in progress" => WorkflowTaskStatus.InProgress,
                "approved" or "completed" => WorkflowTaskStatus.Approved,
                "rejected" => WorkflowTaskStatus.Rejected,
                "skipped" => WorkflowTaskStatus.Skipped,
                "cancelled" or "canceled" => WorkflowTaskStatus.Cancelled,
                _ => WorkflowTaskStatus.Pending
            };
        }

        /// <summary>
        /// Convert WorkflowTaskStatus enum to string for database storage.
        /// </summary>
        public static string ToStatusString(this WorkflowTaskStatus status)
        {
            return status switch
            {
                WorkflowTaskStatus.Pending => "Pending",
                WorkflowTaskStatus.InProgress => "InProgress",
                WorkflowTaskStatus.Approved => "Approved",
                WorkflowTaskStatus.Rejected => "Rejected",
                WorkflowTaskStatus.Skipped => "Skipped",
                WorkflowTaskStatus.Cancelled => "Cancelled",
                _ => "Pending"
            };
        }

        /// <summary>
        /// Convert string to EvidenceVerificationStatus enum.
        /// </summary>
        public static EvidenceVerificationStatus ToEvidenceStatus(this string status)
        {
            return status?.ToLowerInvariant().Replace(" ", "") switch
            {
                "draft" => EvidenceVerificationStatus.Draft,
                "pending" => EvidenceVerificationStatus.Pending,
                "underreview" => EvidenceVerificationStatus.UnderReview,
                "verified" => EvidenceVerificationStatus.Verified,
                "rejected" => EvidenceVerificationStatus.Rejected,
                "archived" => EvidenceVerificationStatus.Archived,
                _ => EvidenceVerificationStatus.Draft
            };
        }

        /// <summary>
        /// Convert EvidenceVerificationStatus enum to string for database storage.
        /// </summary>
        public static string ToStatusString(this EvidenceVerificationStatus status)
        {
            return status switch
            {
                EvidenceVerificationStatus.Draft => "Draft",
                EvidenceVerificationStatus.Pending => "Pending",
                EvidenceVerificationStatus.UnderReview => "Under Review",
                EvidenceVerificationStatus.Verified => "Verified",
                EvidenceVerificationStatus.Rejected => "Rejected",
                EvidenceVerificationStatus.Archived => "Archived",
                _ => "Draft"
            };
        }

        /// <summary>
        /// Convert string to RiskWorkflowStatus enum.
        /// </summary>
        public static RiskWorkflowStatus ToRiskStatus(this string? status)
        {
            return status?.ToLowerInvariant().Replace(" ", "").Replace("_", "") switch
            {
                "identified" or "new" or "open" => RiskWorkflowStatus.Identified,
                "underassessment" or "assessing" => RiskWorkflowStatus.UnderAssessment,
                "pendingdecision" or "pending" => RiskWorkflowStatus.PendingDecision,
                "accepted" => RiskWorkflowStatus.Accepted,
                "requiresmitigation" or "mitigation" or "mitigationrequired" => RiskWorkflowStatus.RequiresMitigation,
                "mitigated" => RiskWorkflowStatus.Mitigated,
                "monitoring" or "monitored" => RiskWorkflowStatus.Monitoring,
                "closed" => RiskWorkflowStatus.Closed,
                _ => RiskWorkflowStatus.Identified
            };
        }

        /// <summary>
        /// Convert RiskWorkflowStatus enum to string for database storage.
        /// </summary>
        public static string ToStatusString(this RiskWorkflowStatus status)
        {
            return status switch
            {
                RiskWorkflowStatus.Identified => "Identified",
                RiskWorkflowStatus.UnderAssessment => "Under Assessment",
                RiskWorkflowStatus.PendingDecision => "Pending Decision",
                RiskWorkflowStatus.Accepted => "Accepted",
                RiskWorkflowStatus.RequiresMitigation => "Requires Mitigation",
                RiskWorkflowStatus.Mitigated => "Mitigated",
                RiskWorkflowStatus.Monitoring => "Monitoring",
                RiskWorkflowStatus.Closed => "Closed",
                _ => "Identified"
            };
        }
    }
}
