using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Workflow Definition - Template for workflow executions
    /// Supports 7 assessment workflows: NCA ECC, SAMA CSF, PDPL PIA, ERM, Evidence Review, Finding Remediation, Policy Review
    /// </summary>
    public class WorkflowDefinition : BaseEntity
    {
        public Guid? TenantId { get; set; } // Multi-tenant: null = global template
        public string WorkflowNumber { get; set; } = string.Empty; // WORKFLOW-001, WORKFLOW-002, etc.
        public string Name { get; set; } = string.Empty; // "NCA ECC Assessment", "SAMA Cyber Assessment", etc.
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty; // "Compliance", "Financial", "Privacy", "Risk", "Evidence", "Audit", "Governance"
        public string WorkflowType { get; set; } = string.Empty; // "Assessment", "Approval", "Remediation"
        public string Framework { get; set; } = string.Empty; // "NCA", "SAMA", "PDPL", "ERM", "Evidence", "Finding", "Policy"
        public string Type { get; set; } = string.Empty; // "Sequential", "Parallel", "Hybrid"

        public string TriggerType { get; set; } = "Manual"; // Manual, Automatic, Scheduled, EventBased
        public string Status { get; set; } = "Active"; // Active, Inactive, Draft
        public bool IsTemplate { get; set; } = true; // True = can instantiate multiple times
        public bool IsActive { get; set; } = true;

        public string Version { get; set; } = "1.0";
        public string DefaultAssignee { get; set; } = string.Empty; // User ID or role
        public string DefaultAssigneeRoleCode { get; set; } = string.Empty; // RoleCode from catalog

        // Workflow metrics
        public int TotalSteps { get; set; } = 0;
        public int EstimatedDays { get; set; } = 0;

        // ===== VISUAL FLOW DIAGRAM (MS Dynamics Style) =====
        public string? BpmnXml { get; set; } // BPMN 2.0 XML for visual workflow
        public string FlowDiagramJson { get; set; } = "{}"; // JSON for visual flow: nodes, edges, positions
        public string MermaidDiagram { get; set; } = string.Empty; // Mermaid.js flowchart syntax

        // Stage-based status format (like MS Dynamics Business Process Flow)
        public string StagesJson { get; set; } = "[]"; // [{stageId, stageName, sequence, status, steps:[]}]
        public int CurrentStageIndex { get; set; } = 0;
        public string StatusFormat { get; set; } = "Stages"; // Stages, Linear, Kanban

        // JSON array of steps with full step definitions
        public string Steps { get; set; } = "[]";
        public string StepsJson { get; set; } = "[]"; // Detailed step definitions

        // JSON variables schema: {"inputVariables":[{"name":"...","type":"...","required":true}]}
        public string VariablesSchema { get; set; } = "{}";

        // Permissions required to execute this workflow
        public string RequiredPermission { get; set; } = string.Empty; // e.g., "Grc.Workflows.Execute"

        // Navigation properties
        public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
    }
}
