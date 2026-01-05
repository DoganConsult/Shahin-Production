using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    // Assessment DTOs
    public class AssessmentDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public int? ComplianceScore { get; set; }
        public int Score { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public string Results { get; set; } = string.Empty;
        public Guid? RiskId { get; set; }
        public Guid? ControlId { get; set; }
    }

    public class CreateAssessmentDto
    {
        public string AssessmentNumber { get; set; } = string.Empty;
        public string AssessmentCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public Guid? RiskId { get; set; }
        public Guid? ControlId { get; set; }
    }

    public class UpdateAssessmentDto : CreateAssessmentDto
    {
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public int? ComplianceScore { get; set; }
        public string Results { get; set; } = string.Empty;
        public string Findings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
    }

    // Audit DTOs
    public class AuditDto
    {
        public Guid Id { get; set; }
        public string AuditNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LeadAuditor { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
        public string Methodology { get; set; } = string.Empty;
        public string ReportSummary { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int FindingsCount { get; set; }
    }

    public class CreateAuditDto
    {
        public string AuditNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
        public string Methodology { get; set; } = string.Empty;
        public string ReportSummary { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public string LeadAuditor { get; set; } = string.Empty;
        public string AuditTeam { get; set; } = string.Empty;
    }

    public class UpdateAuditDto : CreateAuditDto
    {
        public new DateTime StartDate { get; set; }
        public new DateTime EndDate { get; set; }
        public new string Status { get; set; } = string.Empty;
        public new string Criteria { get; set; } = string.Empty;
        public new string Methodology { get; set; } = string.Empty;
        public new string ReportSummary { get; set; } = string.Empty;
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string RiskRating { get; set; } = string.Empty;
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string KeyFindings { get; set; } = string.Empty;
        public string ManagementResponse { get; set; } = string.Empty;
    }

    public class AuditFindingDto
    {
        public Guid Id { get; set; }
        public string FindingNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid AuditId { get; set; }
    }

    public class CreateAuditFindingDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public Guid AuditId { get; set; }
        public string FindingNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RootCause { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string ResponsibleParty { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
    }

    // Policy DTOs
    public class PolicyDto
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Procedures { get; set; } = string.Empty;
        public string References { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime NextReviewDate { get; set; }
        public int ViolationsCount { get; set; }
    }

    public class CreatePolicyDto
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Procedures { get; set; } = string.Empty;
        public string References { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class UpdatePolicyDto : CreatePolicyDto
    {
        public new DateTime? ExpirationDate { get; set; }
        public new DateTime? ReviewDate { get; set; }
        public new string Status { get; set; } = string.Empty;
        public new string Requirements { get; set; } = string.Empty;
        public new string Procedures { get; set; } = string.Empty;
        public new string References { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
    }

    // Evidence DTOs
    public class EvidenceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EvidenceType { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CollectionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateEvidenceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EvidenceType { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CollectionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateEvidenceDto : CreateEvidenceDto
    {
        public new DateTime? ExpirationDate { get; set; }
        public new string Status { get; set; } = string.Empty;
        public new string Notes { get; set; } = string.Empty;
    }

    // Workflow DTOs (continued)
    public class WorkflowDto
    {
        public Guid Id { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string InitiatedBy { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Steps { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
        public string Notifications { get; set; } = string.Empty;
        public int ExecutionsCount { get; set; }
    }

    public class CreateWorkflowDto
    {
        public string WorkflowNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string InitiatedBy { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Steps { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
        public string Notifications { get; set; } = string.Empty;
    }

    public class UpdateWorkflowDto : CreateWorkflowDto
    {
        public new DateTime? DueDate { get; set; }
        public new string Status { get; set; } = string.Empty;
        public new string Priority { get; set; } = string.Empty;
    }

    public class WorkflowExecutionDto
    {
        public Guid Id { get; set; }
        public string ExecutionNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? Duration { get; set; }
        public string InitiatedBy { get; set; } = string.Empty;
    }

    public class CreateWorkflowExecutionDto
    {
        public string ExecutionNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string InitiatedBy { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? Duration { get; set; }
    }

    public class ControlStatisticsDto
    {
        public int TotalControls { get; set; }
        public int EffectiveControls { get; set; }
        public int IneffectiveControls { get; set; }
        public int TestedControls { get; set; }
        public double AverageEffectiveness { get; set; }
        public double EffectivenessRate { get; set; }
        public Dictionary<string, int> ControlsByType { get; set; } = new();
        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
    }

    // Statistics DTOs
    public class AssessmentStatisticsDto
    {
        public int TotalAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public int InProgressAssessments { get; set; }
        public int PendingAssessments { get; set; }
        public int OverdueAssessments { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public Dictionary<string, int> AssessmentsByType { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }

    public class AuditStatisticsDto
    {
        public int TotalAudits { get; set; }
        public int PlannedAudits { get; set; }
        public int CompletedAudits { get; set; }
        public int InProgressAudits { get; set; }
        public int OverdueAudits { get; set; }
        public int TotalFindings { get; set; }
        public int CriticalFindings { get; set; }
        public int HighFindings { get; set; }
        public int MediumFindings { get; set; }
        public int LowFindings { get; set; }
        public double CompletionRate { get; set; }
        public Dictionary<string, int> AuditsByType { get; set; } = new();
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }

    public class EvidenceStatisticsDto
    {
        public int TotalEvidences { get; set; }
        public int ActiveEvidences { get; set; }
        public int ExpiredEvidences { get; set; }
        public int ArchivedEvidences { get; set; }
        public int ExpiringSoon { get; set; }
        public Dictionary<string, int> EvidencesByType { get; set; } = new();
        public Dictionary<string, int> ClassificationDistribution { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }

    public class PolicyStatisticsDto
    {
        public int TotalPolicies { get; set; }
        public int PublishedPolicies { get; set; }
        public int DraftPolicies { get; set; }
        public int ArchivedPolicies { get; set; }
        public int ExpiringSoon { get; set; }
        public int TotalViolations { get; set; }
        public int OpenViolations { get; set; }
        public int ResolvedViolations { get; set; }
        public int ActivePolicies { get; set; }
        public int ApprovedPolicies { get; set; }
        public int ExpiredPolicies { get; set; }
        public double ComplianceRate { get; set; }
        public Dictionary<string, int> PoliciesByCategory { get; set; } = new();
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }

    public class WorkflowStatisticsDto
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int CompletedWorkflows { get; set; }
        public int SuspendedWorkflows { get; set; }
        public int OverdueWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int InProgressExecutions { get; set; }
        public int CompletedExecutions { get; set; }
        public double AverageExecutionTime { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<string, int> WorkflowsByCategory { get; set; } = new();
        public Dictionary<string, int> PriorityDistribution { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> WorkflowsByType { get; set; } = new();
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
    }

    public class PolicyViolationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResolutionPlan { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public Guid PolicyId { get; set; }
    }

    public class CreatePolicyViolationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResolutionPlan { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public Guid PolicyId { get; set; }
    }

    // Authentication DTOs
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class AuthTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } = 3600;
        public AuthUserDto User { get; set; } = new();
    }

    public class AuthUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class UserRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    // Additional DTOs for API responses
    public class EvidenceListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string LinkedItemId { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string FileSize { get; set; } = string.Empty;
    }

    public class ApprovalListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    public class ApprovalReviewDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string ApprovalType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public List<ApprovalCommentDto> Comments { get; set; } = new();
        public List<ApprovalHistoryDto> History { get; set; } = new();
    }

    public class ApprovalCommentDto
    {
        public Guid Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class ApprovalHistoryDto
    {
        public string Action { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class InboxTaskDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string AssignedByName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public List<TaskCommentDto> Comments { get; set; } = new();
        public List<TaskAttachmentDto> Attachments { get; set; } = new();
    }

    public class TaskCommentDto
    {
        public Guid Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class TaskAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }

    // Account Management DTOs
    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }

    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class PasswordResetResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
    }
}
