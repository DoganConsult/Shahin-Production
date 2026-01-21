using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// PHASE 9: Dashboard Service Interface
    /// Provides compliance dashboards, reporting, and analytics
    /// </summary>
    public interface IDashboardService
    {
        #region Executive Dashboard

        /// <summary>
        /// Get executive summary dashboard
        /// </summary>
        Task<ExecutiveDashboard> GetExecutiveDashboardAsync(Guid tenantId);

        #endregion

        #region Compliance Dashboard

        /// <summary>
        /// Get compliance overview by framework/baseline
        /// </summary>
        Task<ComplianceDashboard> GetComplianceDashboardAsync(Guid tenantId);

        /// <summary>
        /// Get compliance score by package
        /// </summary>
        Task<List<PackageComplianceScore>> GetComplianceByPackageAsync(Guid tenantId);

        /// <summary>
        /// Get compliance trend over time
        /// </summary>
        Task<List<ComplianceTrendPoint>> GetComplianceTrendAsync(Guid tenantId, int months = 12);

        #endregion

        #region Plan Progress

        /// <summary>
        /// Get plan progress dashboard
        /// </summary>
        Task<PlanProgressDashboard> GetPlanProgressAsync(Guid tenantId, Guid? planId = null);

        /// <summary>
        /// Get assessment progress by plan
        /// </summary>
        Task<List<AssessmentProgress>> GetAssessmentProgressAsync(Guid planId);

        #endregion

        #region Task Dashboard

        /// <summary>
        /// Get task overview dashboard
        /// </summary>
        Task<TaskDashboard> GetTaskDashboardAsync(Guid tenantId);

        /// <summary>
        /// Get overdue tasks
        /// </summary>
        Task<List<OverdueTask>> GetOverdueTasksAsync(Guid tenantId, int limit = 20);

        /// <summary>
        /// Get upcoming tasks
        /// </summary>
        Task<List<UpcomingTask>> GetUpcomingTasksAsync(Guid tenantId, int days = 7, int limit = 20);

        #endregion

        #region Risk Dashboard

        /// <summary>
        /// Get risk overview dashboard
        /// </summary>
        Task<RiskDashboard> GetRiskDashboardAsync(Guid tenantId);

        #endregion

        #region Top Actions

        /// <summary>
        /// Get top 10 next actions
        /// </summary>
        Task<List<NextAction>> GetTopNextActionsAsync(Guid tenantId, int limit = 10);

        #endregion

        #region Drill-Down

        /// <summary>
        /// Drill down: Package → Assessments
        /// </summary>
        Task<PackageDrillDown> DrillDownPackageAsync(Guid tenantId, string packageCode);

        /// <summary>
        /// Drill down: Assessment → Requirements
        /// </summary>
        Task<AssessmentDrillDown> DrillDownAssessmentAsync(Guid assessmentId);

        /// <summary>
        /// Drill down: Requirement → Evidence
        /// </summary>
        Task<RequirementDrillDown> DrillDownRequirementAsync(Guid requirementId);

        #endregion

        #region Audit Pack

        /// <summary>
        /// Generate audit pack export data
        /// </summary>
        Task<AuditPackData> GenerateAuditPackAsync(Guid tenantId, Guid? assessmentId = null);

        #endregion
    }

    #region Dashboard DTOs

    public class ExecutiveDashboard
    {
        public decimal OverallComplianceScore { get; set; }
        public int TotalRequirements { get; set; }
        public int CompliantRequirements { get; set; }
        public int PartialRequirements { get; set; }
        public int NonCompliantRequirements { get; set; }
        public int NotStartedRequirements { get; set; }
        public int ActivePlans { get; set; }
        public int CompletedPlans { get; set; }
        public int OpenTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighRisks { get; set; }
        public int MediumRisks { get; set; }
        public int LowRisks { get; set; }
        public List<ComplianceByBaseline> ComplianceByBaseline { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class ComplianceByBaseline
    {
        public string BaselineCode { get; set; } = string.Empty;
        public string BaselineName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
    }

    public class ComplianceDashboard
    {
        public decimal OverallScore { get; set; }
        public List<FrameworkCompliance> ByFramework { get; set; } = new();
        public List<DomainCompliance> ByDomain { get; set; } = new();
        public List<StatusBreakdown> ByStatus { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class FrameworkCompliance
    {
        public string FrameworkCode { get; set; } = string.Empty;
        public string FrameworkName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int TotalControls { get; set; }
        public int Compliant { get; set; }
        public int Partial { get; set; }
        public int NonCompliant { get; set; }
        public int NotStarted { get; set; }
    }

    public class DomainCompliance
    {
        public string Domain { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int TotalControls { get; set; }
        public int Compliant { get; set; }
    }

    public class StatusBreakdown
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PackageComplianceScore
    {
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int TotalRequirements { get; set; }
        public int Compliant { get; set; }
        public int Partial { get; set; }
        public int NonCompliant { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ComplianceTrendPoint
    {
        public DateTime Date { get; set; }
        public decimal Score { get; set; }
        public int TotalRequirements { get; set; }
        public int Compliant { get; set; }
    }

    public class PlanProgressDashboard
    {
        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int CompletedPlans { get; set; }
        public decimal OverallProgress { get; set; }
        public List<PlanSummary> Plans { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class PlanSummary
    {
        public Guid PlanId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Progress { get; set; }
        public int TotalPhases { get; set; }
        public int CompletedPhases { get; set; }
        public int TotalAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetEndDate { get; set; }
    }

    public class AssessmentProgress
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Progress { get; set; }
        public decimal Score { get; set; }
        public int TotalRequirements { get; set; }
        public int CompletedRequirements { get; set; }
    }

    public class TaskDashboard
    {
        public int TotalTasks { get; set; }
        public int OpenTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int DueThisWeek { get; set; }
        public List<TasksByRole> ByRole { get; set; } = new();
        public List<TasksByPriority> ByPriority { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class TasksByRole
    {
        public string RoleCode { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Overdue { get; set; }
    }

    public class TasksByPriority
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class OverdueTask
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string AssignedToRole { get; set; } = string.Empty;
        public string AssignedToUser { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
    }

    public class UpcomingTask
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string AssignedToRole { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    public class RiskDashboard
    {
        public int TotalRisks { get; set; }
        public int HighRisks { get; set; }
        public int MediumRisks { get; set; }
        public int LowRisks { get; set; }
        public int OpenRisks { get; set; }
        public int MitigatedRisks { get; set; }
        public List<RiskByCategory> ByCategory { get; set; } = new();
        public List<TopRisk> TopRisks { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class RiskByCategory
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
    }

    public class TopRisk
    {
        public Guid RiskId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Severity { get; set; }
        public int Likelihood { get; set; }
        public int RiskScore { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class NextAction
    {
        public int Priority { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
    }

    public class PackageDrillDown
    {
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public List<AssessmentSummary> Assessments { get; set; } = new();
    }

    public class AssessmentSummary
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalRequirements { get; set; }
        public int Compliant { get; set; }
    }

    public class AssessmentDrillDown
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public List<RequirementSummary> Requirements { get; set; } = new();
    }

    public class RequirementSummary
    {
        public Guid RequirementId { get; set; }
        public string ControlNumber { get; set; } = string.Empty;
        public string ControlTitle { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int EvidenceCount { get; set; }
    }

    public class RequirementDrillDown
    {
        public Guid RequirementId { get; set; }
        public string ControlNumber { get; set; } = string.Empty;
        public string ControlTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public List<EvidenceSummary> Evidences { get; set; } = new();
    }

    public class EvidenceSummary
    {
        public Guid EvidenceId { get; set; }
        public string EvidenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Score { get; set; }
        public DateTime CollectionDate { get; set; }
    }

    public class AuditPackData
    {
        public Guid TenantId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public ExecutiveDashboard ExecutiveSummary { get; set; } = new();
        public List<AssessmentAuditData> Assessments { get; set; } = new();
        public List<EvidenceAuditData> Evidences { get; set; } = new();
    }

    public class AssessmentAuditData
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public List<RequirementAuditData> Requirements { get; set; } = new();
    }

    public class RequirementAuditData
    {
        public string ControlNumber { get; set; } = string.Empty;
        public string ControlTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string EvidenceIds { get; set; } = string.Empty;
    }

    public class EvidenceAuditData
    {
        public Guid EvidenceId { get; set; }
        public string EvidenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Score { get; set; }
        public DateTime CollectionDate { get; set; }
        public string CollectedBy { get; set; } = string.Empty;
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
    }

    #endregion
}
