using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Smart onboarding service that auto-generates assessment templates and GRC plans
/// after onboarding completion, aligned with KSA regulations
/// </summary>
public interface ISmartOnboardingService
{
    /// <summary>
    /// Complete onboarding and auto-generate assessment templates and GRC plan
    /// </summary>
    Task<SmartOnboardingResultDto> CompleteSmartOnboardingAsync(Guid tenantId, string userId);

    /// <summary>
    /// Generate assessment templates based on organization profile and KSA frameworks
    /// </summary>
    Task<List<GeneratedAssessmentTemplateDto>> GenerateAssessmentTemplatesAsync(Guid tenantId);

    /// <summary>
    /// Generate comprehensive GRC plan aligned with KSA regulations
    /// </summary>
    Task<GeneratedGrcPlanDto> GenerateGrcPlanAsync(Guid tenantId, string userId);

    /// <summary>
    /// Auto-generate actual Assessment entities from templates
    /// </summary>
    Task<List<Assessment>> GenerateAssessmentsFromTemplatesAsync(
        Guid tenantId,
        Guid planId,
        List<GeneratedAssessmentTemplateDto> templates,
        string createdBy);

    /// <summary>
    /// Pre-map workspaces for team members after onboarding
    /// </summary>
    Task<List<UserWorkspace>> SetupTeamWorkspacesAsync(
        Guid tenantId,
        List<TeamMemberDto> teamMembers,
        List<Guid> assessmentIds,
        string createdBy);

    /// <summary>
    /// Complete full smart onboarding with assessments and workspace setup
    /// </summary>
    Task<FullOnboardingResultDto> CompleteFullSmartOnboardingAsync(
        Guid tenantId,
        string userId,
        List<TeamMemberDto>? teamMembers = null);
}

/// <summary>
/// Full onboarding result including assessments and workspaces
/// </summary>
public class FullOnboardingResultDto
{
    public Guid TenantId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    // Scope
    public OnboardingScopeDto? Scope { get; set; }

    // Plan
    public GeneratedGrcPlanDto? GeneratedPlan { get; set; }

    // Templates
    public List<GeneratedAssessmentTemplateDto> GeneratedTemplates { get; set; } = new();

    // Actual Assessments
    public List<AssessmentSummaryDto> GeneratedAssessments { get; set; } = new();

    // Team Workspaces
    public List<WorkspaceSummaryDto> TeamWorkspaces { get; set; } = new();

    // Statistics
    public int TotalControls { get; set; }
    public int TotalTasks { get; set; }
    public DateTime EstimatedCompletionDate { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class AssessmentSummaryDto
{
    public Guid Id { get; set; }
    public string AssessmentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public int ControlCount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Draft";
}

public class WorkspaceSummaryDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int AssignedTasks { get; set; }
    public string DefaultLandingPage { get; set; } = "/Dashboard";
}
