using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Trigger Rule - Define event-based triggers for automated actions
    /// Can be evaluated by AI agent to check input and trigger workflows
    /// For future implementation: Agent-based evaluation
    /// </summary>
    public class TriggerRule : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant

        public string RuleCode { get; set; } = string.Empty; // TRG_EVIDENCE_SUBMIT_01
        public string Name { get; set; } = string.Empty; // "Evidence Submission Trigger"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 100;

        // ===== TRIGGER EVENT =====
        public string TriggerEvent { get; set; } = string.Empty; // OnEvidenceSubmit, OnTaskComplete, OnStatusChange, OnThreshold, Scheduled
        public string EntityType { get; set; } = string.Empty; // Evidence, Task, Assessment, Requirement
        public string EventConditionJson { get; set; } = "{}"; // {"status": "Submitted", "evidenceType": "Policy"}

        // ===== CONDITION EVALUATION =====
        public string ConditionType { get; set; } = "Simple"; // Simple, Complex, AgentEvaluated
        public string ConditionExpression { get; set; } = string.Empty; // Simple expression or JSON
        public string AgentPrompt { get; set; } = string.Empty; // Prompt for AI agent evaluation (future)
        public bool RequiresAgentEvaluation { get; set; } = false; // Flag for agent-based evaluation

        // ===== ACTIONS =====
        public string ActionType { get; set; } = string.Empty; // StartWorkflow, SendNotification, UpdateStatus, CreateTask, CallWebhook
        public string ActionConfigJson { get; set; } = "{}"; // Action-specific configuration
        public string WorkflowDefinitionId { get; set; } = string.Empty; // Workflow to start (if applicable)
        public string NotificationTemplateCode { get; set; } = string.Empty; // Notification template
        public string WebhookUrl { get; set; } = string.Empty; // External webhook

        // ===== SCHEDULING (for scheduled triggers) =====
        public string CronExpression { get; set; } = string.Empty; // Cron for scheduled triggers
        public DateTime? NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }

        // ===== EXECUTION CONTROL =====
        public bool RunOnce { get; set; } = false; // Only trigger once per entity
        public int MaxExecutionsPerDay { get; set; } = 0; // 0 = unlimited
        public int CooldownMinutes { get; set; } = 0; // Minimum time between executions
        public bool IsAsync { get; set; } = true; // Execute asynchronously

        // ===== AUDIT =====
        public int ExecutionCount { get; set; } = 0;
        public int SuccessCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public DateTime? LastSuccessAt { get; set; }
        public string LastErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trigger Execution Log - Track all trigger executions
    /// </summary>
    public class TriggerExecutionLog : BaseEntity
    {
        public Guid? TenantId { get; set; }
        public Guid TriggerRuleId { get; set; }

        public string TriggerEvent { get; set; } = string.Empty;
        public Guid? EntityId { get; set; } // Entity that triggered
        public string EntityType { get; set; } = string.Empty;

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty; // Success, Failed, Skipped
        public string ResultJson { get; set; } = "{}"; // Execution result
        public string ErrorMessage { get; set; } = string.Empty;

        // Agent evaluation results (future)
        public bool WasAgentEvaluated { get; set; } = false;
        public string AgentResponseJson { get; set; } = "{}";
        public double? AgentConfidenceScore { get; set; }

        public int ExecutionTimeMs { get; set; } = 0;

        // Navigation
        public virtual TriggerRule TriggerRule { get; set; } = null!;
    }
}
