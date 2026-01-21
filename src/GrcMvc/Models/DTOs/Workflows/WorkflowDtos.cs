namespace GrcMvc.Models.DTOs.Workflows
{
    // NOTE: WorkflowInstanceDto and WorkflowTaskDto are defined in Models/DTOs/WorkflowDtos.cs
    // to avoid duplicate class definitions. Use those versions which have more complete properties.

    /// <summary>Workflow Approval DTO</summary>
    public class WorkflowApprovalDto
    {
        public string ApprovalLevel { get; set; }
        public string Decision { get; set; }
        public string Comments { get; set; }
        public DateTime ApprovedAt { get; set; }
        public string ApproversRole { get; set; }
    }

    /// <summary>Generic workflow action</summary>
    public class WorkflowActionDto
    {
        public string Comments { get; set; }
        public string Notes { get; set; }
        public string Details { get; set; }
    }

    /// <summary>Approval submission DTO</summary>
    public class ApprovalSubmissionDto
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; }
    }

    /// <summary>Evidence submission DTO</summary>
    public class EvidenceSubmissionDto
    {
        public string Description { get; set; }
        public List<string> FileUrls { get; set; } = new();
    }

    /// <summary>Test results DTO</summary>
    public class TestResultsDto
    {
        public string Results { get; set; }
        public bool Passed { get; set; }
        public string Findings { get; set; }
    }

    /// <summary>Remediation identification DTO</summary>
    public class RemediationIdentificationDto
    {
        public Guid IssueId { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
    }

    /// <summary>Remediation verification DTO</summary>
    public class RemediationVerificationDto
    {
        public bool IsSuccessful { get; set; }
        public string VerificationNotes { get; set; }
    }

    /// <summary>Policy review schedule DTO</summary>
    public class PolicyReviewScheduleDto
    {
        public DateTime ReviewDate { get; set; }
        public string Reviewer { get; set; }
    }

    /// <summary>Training assignment DTO</summary>
    public class TrainingAssignmentDto
    {
        public Guid EmployeeId { get; set; }
        public Guid TrainingModuleId { get; set; }
        public DateTime DueDate { get; set; }
    }

    /// <summary>Training result DTO</summary>
    public class TrainingResultDto
    {
        public int Score { get; set; }
        public DateTime CompletionDate { get; set; }
        public string Feedback { get; set; }
    }

    /// <summary>Audit report DTO</summary>
    public class AuditReportDto
    {
        public string Report { get; set; }
        public List<string> Findings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>Exception submission DTO</summary>
    public class ExceptionSubmissionDto
    {
        public string Description { get; set; }
        public string Justification { get; set; }
        public string ImpactArea { get; set; }
        public string RequestedDuration { get; set; }
    }

    /// <summary>Workflow dashboard DTO</summary>
    public class WorkflowDashboardDto
    {
        public int ActiveWorkflows { get; set; }
        public int CompletedWorkflows { get; set; }
        public int PendingTasks { get; set; }
        public decimal CompletionRate { get; set; }
        public Dictionary<string, int> WorkflowsByType { get; set; } = new();
    }
}
