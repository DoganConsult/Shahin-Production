namespace GrcMvc.Models.DTOs;

/// <summary>
/// Result of smart onboarding completion
/// </summary>
public class SmartOnboardingResultDto
{
    public Guid TenantId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<GeneratedAssessmentTemplateDto> GeneratedTemplates { get; set; } = new();
    public GeneratedGrcPlanDto? GeneratedPlan { get; set; }
    public OnboardingScopeDto? Scope { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Generated assessment template
/// </summary>
public class GeneratedAssessmentTemplateDto
{
    public string TemplateCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public int EstimatedControls { get; set; }
    public string Priority { get; set; } = "Medium"; // High, Medium, Low
    public string Reason { get; set; } = string.Empty;
    public DateTime? RecommendedStartDate { get; set; }
    public DateTime? RecommendedEndDate { get; set; }
}

/// <summary>
/// Generated comprehensive GRC plan
/// </summary>
public class GeneratedGrcPlanDto
{
    public Guid PlanId { get; set; }
    public Guid TenantId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlanType { get; set; } = "Comprehensive"; // QuickScan, Comprehensive, Remediation
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
    public List<GrcPlanPhaseDto> Phases { get; set; } = new();
    public List<GrcPlanMilestoneDto> Milestones { get; set; } = new();
    public List<string> ApplicableFrameworks { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Phase in GRC plan
/// </summary>
public class GrcPlanPhaseDto
{
    public int PhaseNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Activities { get; set; } = new();
    public List<string> Deliverables { get; set; } = new();
    public string Status { get; set; } = "Planned";
}

/// <summary>
/// Milestone in GRC plan
/// </summary>
public class GrcPlanMilestoneDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string Status { get; set; } = "Planned";
    public List<string> Dependencies { get; set; } = new();
}
