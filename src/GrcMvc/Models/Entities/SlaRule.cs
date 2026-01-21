using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// SLA Rule - Define Service Level Agreements for tasks and workflows
    /// Tracks response time, resolution time, and compliance with SLA targets
    /// </summary>
    public class SlaRule : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant

        public string RuleCode { get; set; } = string.Empty; // SLA_STANDARD_01
        public string Name { get; set; } = string.Empty; // "Standard Task SLA"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 100;

        // ===== SLA TARGETS =====
        public int ResponseTimeHours { get; set; } = 24; // Time to first response
        public int ResolutionTimeHours { get; set; } = 72; // Time to complete
        public int WarningThresholdPercent { get; set; } = 75; // Warn at 75% of SLA time
        public int CriticalThresholdPercent { get; set; } = 90; // Critical at 90%

        // ===== BUSINESS HOURS =====
        public bool UseBusinessHoursOnly { get; set; } = true;
        public string BusinessHoursJson { get; set; } = "{\"start\": \"08:00\", \"end\": \"17:00\", \"timezone\": \"Asia/Riyadh\"}";
        public string ExcludedDaysJson { get; set; } = "[\"Friday\", \"Saturday\"]"; // Weekend days
        public bool ExcludeHolidays { get; set; } = true;

        // ===== SCOPE =====
        public string WorkflowCategory { get; set; } = string.Empty; // Assessment, Approval, Remediation
        public string TaskPriority { get; set; } = string.Empty; // Critical, High, Medium, Low - empty = all
        public string TaskType { get; set; } = string.Empty; // Specific task type
        public string ApplicableRolesJson { get; set; } = "[]"; // Roles this SLA applies to

        // ===== BREACH ACTIONS =====
        public string OnWarningAction { get; set; } = "Notify"; // Notify, Escalate
        public string OnBreachAction { get; set; } = "Escalate"; // Notify, Escalate, AutoReassign
        public string BreachEscalationRuleCode { get; set; } = string.Empty; // Link to escalation rule
        public bool AutoExtendOnHoliday { get; set; } = true;

        // ===== METRICS =====
        public bool TrackFirstResponseTime { get; set; } = true;
        public bool TrackResolutionTime { get; set; } = true;
        public bool IncludeInReporting { get; set; } = true;
    }
}
