using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Dtos;

/// <summary>
/// DTO for report list display
/// </summary>
public class ReportListItemDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Risk, Compliance, Audit, Dashboard
    public string Status { get; set; } = "Draft"; // Draft, Generated, Delivered, Archived
    public string Scope { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public int PageCount { get; set; }
}

/// <summary>
/// DTO for report detail view
/// </summary>
public class ReportDetailDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    
    // Scope & Filters
    public string Scope { get; set; } = string.Empty;
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
    public List<string> IncludedEntities { get; set; } = new();
    
    // Content
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string KeyFindings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public int TotalFindingsCount { get; set; }
    public int CriticalFindingsCount { get; set; }
    
    // Metadata
    public DateTime GeneratedDate { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string DeliveredTo { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
    public int PageCount { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new report
/// </summary>
public class ReportCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
    public List<string> IncludedEntities { get; set; } = new();
    public string IncludeExecutiveSummary { get; set; } = "yes";
    public string IncludeFindingsDetail { get; set; } = "yes";
    public string IncludeRecommendations { get; set; } = "yes";
}

/// <summary>
/// DTO for editing a report
/// </summary>
public class ReportEditDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty; // Read-only
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string KeyFindings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string DeliveredTo { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
}

/// <summary>
/// DTO for dashboard summary metrics
/// </summary>
public class DashboardMetricsDto
{
    // Workflow Metrics
    public int TotalWorkflows { get; set; }
    public int ActiveWorkflows { get; set; }
    public int CompletedWorkflows { get; set; }
    public int OverdueWorkflows { get; set; }
    
    // Assessment Metrics
    public int TotalAssessments { get; set; }
    public int InProgressAssessments { get; set; }
    public int CompletedAssessments { get; set; }
    public decimal AverageAssessmentScore { get; set; }
    
    // Audit Metrics
    public int TotalAudits { get; set; }
    public int OpenAudits { get; set; }
    public int CompletedAudits { get; set; }
    public int TotalAuditFindings { get; set; }
    public int CriticalFindings { get; set; }
    
    // Risk Metrics
    public int TotalRisks { get; set; }
    public int HighRisks { get; set; }
    public int CriticalRisks { get; set; }
    public decimal AverageResidualScore { get; set; }
    
    // Control Metrics
    public int TotalControls { get; set; }
    public int EffectiveControls { get; set; }
    public int InefficientControls { get; set; }
    public decimal ControlEffectivenessPct { get; set; }
    
    // Approval Metrics
    public int PendingApprovals { get; set; }
    public int ApprovedToday { get; set; }
    
    // Policy Metrics
    public int TotalPolicies { get; set; }
    public int PolicyViolations { get; set; }
    
    // Timeline
    public DateTime LastUpdated { get; set; }
}
