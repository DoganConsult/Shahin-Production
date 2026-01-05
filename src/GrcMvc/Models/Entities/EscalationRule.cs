using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Escalation Rule - Define when and how to escalate overdue workflow tasks
    /// Escalation tiers: 2 days → reminder, 5 days → manager, 10 days → director, 15 days → executive
    /// </summary>
    public class EscalationRule : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant

        public string RuleCode { get; set; } = string.Empty; // ESC_STANDARD_01
        public string Name { get; set; } = string.Empty; // "Standard Workflow Escalation"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 100; // Lower = higher priority

        // ===== TRIGGER CONDITIONS =====
        public int DaysOverdueTrigger { get; set; } // 2, 5, 10, 15
        public int HoursOverdueTrigger { get; set; } = 0; // For urgent tasks
        public string TriggerConditionJson { get; set; } = "{}"; // Complex conditions: {"priority": "Critical", "taskType": "Approval"}

        // ===== ESCALATION ACTION =====
        public string Action { get; set; } = string.Empty; // Reminder, EscalateToManager, EscalateToDirector, ExecutiveAlert, AutoReject
        public string EscalateToRoleCode { get; set; } = string.Empty; // Target role for escalation
        public Guid? EscalateToUserId { get; set; } // Specific user (optional)
        public bool ShouldReassign { get; set; } = false;
        public bool ShouldNotifyOriginalAssignee { get; set; } = true;
        public bool ShouldNotifyManager { get; set; } = true;

        // ===== NOTIFICATION CONFIG =====
        public string NotificationConfig { get; set; } = "{}"; // {"channels": ["Email", "SMS", "Browser"], "template": "..."}
        public string EmailTemplateCode { get; set; } = string.Empty;
        public string SmsTemplateCode { get; set; } = string.Empty;

        // ===== SCOPE =====
        public string WorkflowCategory { get; set; } = string.Empty; // Assessment, Approval, Remediation - empty = all
        public string WorkflowDefinitionId { get; set; } = string.Empty; // Specific workflow (optional)
        public string TaskType { get; set; } = string.Empty; // Specific task type (optional)
    }
}
