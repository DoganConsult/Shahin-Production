using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Smart onboarding service that auto-generates assessment templates and comprehensive GRC plans
/// aligned with KSA regulations after onboarding completion
/// </summary>
public class SmartOnboardingService : ISmartOnboardingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOnboardingService _onboardingService;
    private readonly IAssessmentService _assessmentService;
    private readonly IPlanService _planService;
    private readonly IFrameworkService _frameworkService;
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<SmartOnboardingService> _logger;

    public SmartOnboardingService(
        IUnitOfWork unitOfWork,
        IOnboardingService onboardingService,
        IAssessmentService assessmentService,
        IPlanService planService,
        IFrameworkService frameworkService,
        IWorkspaceService workspaceService,
        ILogger<SmartOnboardingService> logger)
    {
        _unitOfWork = unitOfWork;
        _onboardingService = onboardingService;
        _assessmentService = assessmentService;
        _planService = planService;
        _frameworkService = frameworkService;
        _workspaceService = workspaceService;
        _logger = logger;
    }

    /// <summary>
    /// Complete onboarding and auto-generate assessment templates and GRC plan
    /// </summary>
    public async Task<SmartOnboardingResultDto> CompleteSmartOnboardingAsync(Guid tenantId, string userId)
    {
        try
        {
            _logger.LogInformation($"Starting smart onboarding completion for tenant {tenantId}");

            // Step 1: Complete standard onboarding
            var executionLog = await _onboardingService.CompleteOnboardingAsync(tenantId, userId);
            _logger.LogInformation($"Standard onboarding completed, execution log: {executionLog?.Id}");

            // Step 2: Get organization profile
            var profile = await _unitOfWork.OrganizationProfiles
                .Query()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId);

            if (profile == null)
            {
                throw new EntityNotFoundException("OrganizationProfile", $"tenant:{tenantId}");
            }

            // Step 3: Get derived scope
            var scope = await _onboardingService.GetDerivedScopeAsync(tenantId);

            // Step 3.5: Provision default team and RACI assignments
            var defaultTeam = await ProvisionDefaultTeamAndRACIAsync(tenantId, userId);
            _logger.LogInformation($"Default team provisioned: {defaultTeam.Id}");

            // Step 4: Generate assessment templates based on profile and KSA frameworks
            var templates = await GenerateAssessmentTemplatesAsync(tenantId);
            _logger.LogInformation($"Generated {templates.Count} assessment templates");

            // Step 5: Generate comprehensive GRC plan
            var plan = await GenerateGrcPlanAsync(tenantId, userId);
            _logger.LogInformation($"Generated GRC plan: {plan.PlanName}");

            return new SmartOnboardingResultDto
            {
                TenantId = tenantId,
                Success = true,
                Message = $"Smart onboarding completed successfully. Generated {templates.Count} assessment templates and comprehensive GRC plan.",
                GeneratedTemplates = templates,
                GeneratedPlan = plan,
                Scope = scope,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing smart onboarding for tenant {tenantId}");
            throw;
        }
    }

    /// <summary>
    /// Generate assessment templates based on organization profile and KSA frameworks
    /// </summary>
    public async Task<List<GeneratedAssessmentTemplateDto>> GenerateAssessmentTemplatesAsync(Guid tenantId)
    {
        var templates = new List<GeneratedAssessmentTemplateDto>();

        try
        {
            // Get organization profile
            var profile = await _unitOfWork.OrganizationProfiles
                .Query()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId);

            if (profile == null)
            {
                _logger.LogWarning($"Organization profile not found for tenant {tenantId}");
                return templates;
            }

            // Determine applicable KSA frameworks based on profile
            var applicableFrameworks = DetermineApplicableKsaFrameworks(profile);

            _logger.LogInformation($"Determined {applicableFrameworks.Count} applicable KSA frameworks for tenant {tenantId}");

            // Generate templates for each applicable framework
            foreach (var framework in applicableFrameworks)
            {
                var template = new GeneratedAssessmentTemplateDto
                {
                    TemplateCode = $"{framework.Code}_ASSESSMENT_{DateTime.UtcNow:yyyyMMdd}",
                    Name = $"{framework.Name} Compliance Assessment",
                    Description = GenerateAssessmentDescription(framework, profile),
                    FrameworkCode = framework.Code,
                    FrameworkName = framework.Name,
                    EstimatedControls = framework.EstimatedControls,
                    Priority = DeterminePriority(framework, profile),
                    Reason = GenerateTemplateReason(framework, profile),
                    RecommendedStartDate = DateTime.UtcNow.AddDays(7), // Start in 1 week
                    RecommendedEndDate = DateTime.UtcNow.AddDays(90) // Complete in 3 months
                };

                templates.Add(template);
            }

            // Sort by priority (High first)
            templates = templates.OrderByDescending(t => t.Priority == "High")
                                 .ThenByDescending(t => t.Priority == "Medium")
                                 .ToList();

            _logger.LogInformation($"Generated {templates.Count} assessment templates for tenant {tenantId}");
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating assessment templates for tenant {tenantId}");
            throw;
        }
    }

    /// <summary>
    /// Generate comprehensive GRC plan aligned with KSA regulations
    /// </summary>
    public async Task<GeneratedGrcPlanDto> GenerateGrcPlanAsync(Guid tenantId, string userId)
    {
        try
        {
            // Get organization profile
            var profile = await _unitOfWork.OrganizationProfiles
                .Query()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId);

            if (profile == null)
            {
                throw new EntityNotFoundException("OrganizationProfile", $"tenant:{tenantId}");
            }

            // Get applicable frameworks
            var applicableFrameworks = DetermineApplicableKsaFrameworks(profile);
            var frameworkNames = applicableFrameworks.Select(f => f.Name).ToList();

            // Generate plan phases based on organization maturity and frameworks
            var phases = GeneratePlanPhases(profile, applicableFrameworks);

            // Generate milestones
            var milestones = GeneratePlanMilestones(phases);

            // Calculate timeline
            var startDate = DateTime.UtcNow.AddDays(7);
            var endDate = CalculatePlanEndDate(profile, applicableFrameworks.Count);

            // Create plan
            var planDto = new CreatePlanDto
            {
                TenantId = tenantId,
                PlanCode = $"GRC_PLAN_{tenantId.ToString()[..8].ToUpper()}_{DateTime.UtcNow:yyyyMMdd}",
                Name = $"Comprehensive GRC Plan - {profile.OrganizationType}",
                Description = GeneratePlanDescription(profile, applicableFrameworks),
                PlanType = DeterminePlanType(profile),
                StartDate = startDate,
                TargetEndDate = endDate
            };

            var plan = await _planService.CreatePlanAsync(planDto, userId);

            return new GeneratedGrcPlanDto
            {
                PlanId = plan.Id,
                TenantId = tenantId,
                PlanName = plan.Name,
                Description = plan.Description,
                PlanType = planDto.PlanType,
                StartDate = startDate,
                TargetEndDate = endDate,
                Phases = phases,
                Milestones = milestones,
                ApplicableFrameworks = frameworkNames,
                Metadata = new Dictionary<string, object>
                {
                    ["OrganizationType"] = profile.OrganizationType,
                    ["Sector"] = profile.Sector,
                    ["Country"] = profile.Country,
                    ["ComplianceMaturity"] = profile.ComplianceMaturity,
                    ["GeneratedAt"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating GRC plan for tenant {tenantId}");
            throw;
        }
    }

    #region Helper Methods

    /// <summary>
    /// Determine applicable KSA frameworks based on organization profile
    /// </summary>
    private List<KsaFrameworkInfo> DetermineApplicableKsaFrameworks(OrganizationProfile profile)
    {
        var frameworks = new List<KsaFrameworkInfo>();

        // Always include based on country
        if (profile.Country == "SA" || profile.Country == "KSA")
        {
            // Core KSA frameworks
            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "PDPL",
                Name = "Personal Data Protection Law (PDPL)",
                EstimatedControls = 45,
                Category = "Data Privacy",
                Mandatory = true
            });

            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "NCA-ECC",
                Name = "NCA Essential Cybersecurity Controls",
                EstimatedControls = 114,
                Category = "Cybersecurity",
                Mandatory = profile.IsCriticalInfrastructure ||
                           profile.Sector?.Contains("Critical", StringComparison.OrdinalIgnoreCase) == true ||
                           profile.Sector?.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase) == true
            });
        }

        // Sector-specific frameworks
        if (profile.Sector.Contains("Banking") || profile.Sector.Contains("Financial"))
        {
            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "SAMA-CSF",
                Name = "SAMA Cybersecurity Framework",
                EstimatedControls = 98,
                Category = "Cybersecurity",
                Mandatory = true
            });

            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "SAMA-AML",
                Name = "SAMA Anti-Money Laundering",
                EstimatedControls = 67,
                Category = "Financial Compliance",
                Mandatory = true
            });
        }

        if (profile.Sector.Contains("Healthcare") || profile.Sector.Contains("Medical"))
        {
            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "SFDA",
                Name = "SFDA Medical Device Regulations",
                EstimatedControls = 89,
                Category = "Healthcare",
                Mandatory = true
            });
        }

        // Size-based frameworks
        if (profile.OrganizationSize == "Large" || profile.OrganizationSize == "Enterprise")
        {
            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "ISO27001",
                Name = "ISO 27001 Information Security",
                EstimatedControls = 114,
                Category = "Information Security",
                Mandatory = false
            });
        }

        // Maturity-based frameworks
        if (profile.ComplianceMaturity == "Advanced" || profile.ComplianceMaturity == "Mature")
        {
            frameworks.Add(new KsaFrameworkInfo
            {
                Code = "ISO22301",
                Name = "ISO 22301 Business Continuity",
                EstimatedControls = 103,
                Category = "Business Continuity",
                Mandatory = false
            });
        }

        return frameworks;
    }

    /// <summary>
    /// Generate assessment description
    /// </summary>
    private string GenerateAssessmentDescription(KsaFrameworkInfo framework, OrganizationProfile profile)
    {
        return $"Comprehensive compliance assessment for {framework.Name} tailored for {profile.OrganizationType} " +
               $"organization in {profile.Sector} sector. This assessment evaluates adherence to {framework.EstimatedControls} " +
               $"controls across {framework.Category} domain.";
    }

    /// <summary>
    /// Determine template priority
    /// </summary>
    private string DeterminePriority(KsaFrameworkInfo framework, OrganizationProfile profile)
    {
        if (framework.Mandatory) return "High";
        if (profile.ComplianceMaturity == "Beginner") return "High";
        if (framework.Category == "Cybersecurity" || framework.Category == "Data Privacy") return "High";
        return "Medium";
    }

    /// <summary>
    /// Generate template reason
    /// </summary>
    private string GenerateTemplateReason(KsaFrameworkInfo framework, OrganizationProfile profile)
    {
        if (framework.Mandatory)
        {
            return $"Mandatory compliance requirement for {profile.Sector} sector organizations in KSA.";
        }
        return $"Recommended based on organization profile: {profile.OrganizationType}, {profile.OrganizationSize} size, {profile.ComplianceMaturity} maturity.";
    }

    /// <summary>
    /// Generate plan phases
    /// </summary>
    private List<GrcPlanPhaseDto> GeneratePlanPhases(OrganizationProfile profile, List<KsaFrameworkInfo> frameworks)
    {
        var phases = new List<GrcPlanPhaseDto>();
        var startDate = DateTime.UtcNow.AddDays(7);

        // Phase 1: Assessment & Gap Analysis (Weeks 1-4)
        phases.Add(new GrcPlanPhaseDto
        {
            PhaseNumber = 1,
            Name = "Assessment & Gap Analysis",
            Description = "Conduct comprehensive assessments across all applicable frameworks and identify compliance gaps",
            StartDate = startDate,
            EndDate = startDate.AddDays(28),
            Activities = new List<string>
            {
                "Complete framework assessments",
                "Perform gap analysis",
                "Document current state",
                "Identify compliance gaps",
                "Prioritize remediation areas"
            },
            Deliverables = new List<string>
            {
                "Assessment reports",
                "Gap analysis document",
                "Compliance status dashboard",
                "Risk register"
            },
            Status = "Planned"
        });

        // Phase 2: Remediation Planning (Weeks 5-8)
        phases.Add(new GrcPlanPhaseDto
        {
            PhaseNumber = 2,
            Name = "Remediation Planning",
            Description = "Develop detailed remediation plans and action items for identified gaps",
            StartDate = startDate.AddDays(28),
            EndDate = startDate.AddDays(56),
            Activities = new List<string>
            {
                "Create action plans",
                "Assign responsibilities",
                "Set timelines",
                "Allocate resources",
                "Define success criteria"
            },
            Deliverables = new List<string>
            {
                "Remediation action plans",
                "Resource allocation plan",
                "Timeline and milestones",
                "Budget estimates"
            },
            Status = "Planned"
        });

        // Phase 3: Implementation (Weeks 9-24)
        phases.Add(new GrcPlanPhaseDto
        {
            PhaseNumber = 3,
            Name = "Implementation & Remediation",
            Description = "Execute remediation activities and implement required controls",
            StartDate = startDate.AddDays(56),
            EndDate = startDate.AddDays(168),
            Activities = new List<string>
            {
                "Implement controls",
                "Execute remediation activities",
                "Monitor progress",
                "Update documentation",
                "Conduct training"
            },
            Deliverables = new List<string>
            {
                "Implemented controls",
                "Updated policies and procedures",
                "Training records",
                "Progress reports"
            },
            Status = "Planned"
        });

        // Phase 4: Validation & Certification (Weeks 25-36)
        phases.Add(new GrcPlanPhaseDto
        {
            PhaseNumber = 4,
            Name = "Validation & Continuous Compliance",
            Description = "Validate implementation, conduct audits, and establish continuous compliance monitoring",
            StartDate = startDate.AddDays(168),
            EndDate = startDate.AddDays(252),
            Activities = new List<string>
            {
                "Internal audits",
                "Control testing",
                "Evidence collection",
                "Compliance reporting",
                "Continuous monitoring setup"
            },
            Deliverables = new List<string>
            {
                "Audit reports",
                "Compliance certificates",
                "Evidence repository",
                "Monitoring dashboard"
            },
            Status = "Planned"
        });

        return phases;
    }

    /// <summary>
    /// Generate plan milestones
    /// </summary>
    private List<GrcPlanMilestoneDto> GeneratePlanMilestones(List<GrcPlanPhaseDto> phases)
    {
        return new List<GrcPlanMilestoneDto>
        {
            new GrcPlanMilestoneDto
            {
                Name = "Assessment Complete",
                Description = "All framework assessments completed and gap analysis finalized",
                TargetDate = phases[0].EndDate,
                Status = "Planned",
                Dependencies = new List<string> { "Phase 1 completion" }
            },
            new GrcPlanMilestoneDto
            {
                Name = "Remediation Plans Approved",
                Description = "All remediation action plans reviewed and approved",
                TargetDate = phases[1].EndDate,
                Status = "Planned",
                Dependencies = new List<string> { "Phase 2 completion" }
            },
            new GrcPlanMilestoneDto
            {
                Name = "50% Controls Implemented",
                Description = "Half of identified controls successfully implemented",
                TargetDate = phases[2].StartDate.AddDays(56),
                Status = "Planned",
                Dependencies = new List<string> { "Phase 3 start" }
            },
            new GrcPlanMilestoneDto
            {
                Name = "All Controls Implemented",
                Description = "All critical controls implemented and tested",
                TargetDate = phases[2].EndDate,
                Status = "Planned",
                Dependencies = new List<string> { "Phase 3 completion" }
            },
            new GrcPlanMilestoneDto
            {
                Name = "Compliance Validated",
                Description = "Internal audits completed and compliance validated",
                TargetDate = phases[3].EndDate,
                Status = "Planned",
                Dependencies = new List<string> { "Phase 4 completion" }
            }
        };
    }

    /// <summary>
    /// Calculate plan end date
    /// </summary>
    private DateTime CalculatePlanEndDate(OrganizationProfile profile, int frameworkCount)
    {
        var baseDays = 180; // 6 months base
        var frameworkDays = frameworkCount * 30; // 1 month per framework
        var maturityMultiplier = profile.ComplianceMaturity switch
        {
            "Beginner" => 1.5,
            "Intermediate" => 1.2,
            "Advanced" => 1.0,
            "Mature" => 0.8,
            _ => 1.0
        };

        var totalDays = (int)((baseDays + frameworkDays) * maturityMultiplier);
        return DateTime.UtcNow.AddDays(7 + totalDays);
    }

    /// <summary>
    /// Generate plan description
    /// </summary>
    private string GeneratePlanDescription(OrganizationProfile profile, List<KsaFrameworkInfo> frameworks)
    {
        return $"Comprehensive GRC implementation plan for {profile.OrganizationType} organization " +
               $"in {profile.Sector} sector. This plan covers {frameworks.Count} regulatory frameworks " +
               $"and includes assessment, remediation, implementation, and validation phases to achieve " +
               $"full compliance with KSA regulations.";
    }

    /// <summary>
    /// Determine plan type
    /// </summary>
    private string DeterminePlanType(OrganizationProfile profile)
    {
        if (profile.ComplianceMaturity == "Beginner")
            return "Comprehensive";
        if (profile.ComplianceMaturity == "Intermediate")
            return "Comprehensive";
        return "Remediation";
    }

    #endregion

    #region New Methods - Auto-generate Assessments and Workspaces

    /// <summary>
    /// Auto-generate actual Assessment entities from templates
    /// </summary>
    public async Task<List<Assessment>> GenerateAssessmentsFromTemplatesAsync(
        Guid tenantId,
        Guid planId,
        List<GeneratedAssessmentTemplateDto> templates,
        string createdBy)
    {
        var assessments = new List<Assessment>();

        try
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(planId);
            if (plan == null)
                throw new EntityNotFoundException("Plan", planId);

            foreach (var template in templates)
            {
                var assessment = new Assessment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PlanId = planId,
                    AssessmentCode = $"ASMT-{template.FrameworkCode}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                    Name = template.Name,
                    Type = "Compliance",
                    Status = "Draft",
                    ScheduledDate = template.RecommendedStartDate,
                    DueDate = template.RecommendedEndDate,
                    TemplateCode = template.TemplateCode,
                    FrameworkCode = template.FrameworkCode,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                await _unitOfWork.Assessments.AddAsync(assessment);
                assessments.Add(assessment);

                // Generate assessment requirements from framework controls
                await GenerateRequirementsForAssessmentAsync(assessment, template.FrameworkCode, createdBy);

                _logger.LogInformation("Created assessment {Code} for framework {Framework}",
                    assessment.AssessmentCode, template.FrameworkCode);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Generated {Count} assessments for tenant {TenantId}", assessments.Count, tenantId);

            return assessments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating assessments from templates");
            throw;
        }
    }

    /// <summary>
    /// Generate assessment requirements from framework controls
    /// </summary>
    private async Task GenerateRequirementsForAssessmentAsync(Assessment assessment, string frameworkCode, string createdBy)
    {
        // Get controls for this framework
        var controls = await _unitOfWork.FrameworkControls
            .Query()
            .Where(c => c.FrameworkCode == frameworkCode && !c.IsDeleted)
            .OrderBy(c => c.ControlNumber)
            .Take(500)
            .ToListAsync();

        foreach (var control in controls)
        {
            var requirement = new AssessmentRequirement
            {
                Id = Guid.NewGuid(),
                AssessmentId = assessment.Id,
                TenantId = assessment.TenantId,
                ControlNumber = control.ControlNumber,
                ControlTitle = control.TitleEn,
                ControlTitleAr = control.TitleAr,
                RequirementText = control.RequirementEn,
                RequirementTextAr = control.RequirementAr,
                Domain = control.Domain,
                ControlType = control.ControlType ?? "Technical",
                MaturityLevel = control.MaturityLevel.ToString(),
                Status = "NotStarted",
                EvidenceStatus = "Pending",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _unitOfWork.AssessmentRequirements.AddAsync(requirement);
        }

        _logger.LogInformation("Generated {Count} requirements for assessment {Code}",
            controls.Count, assessment.AssessmentCode);
    }

    /// <summary>
    /// Pre-map workspaces for team members after onboarding
    /// </summary>
    public async Task<List<UserWorkspace>> SetupTeamWorkspacesAsync(
        Guid tenantId,
        List<TeamMemberDto> teamMembers,
        List<Guid> assessmentIds,
        string createdBy)
    {
        return await _workspaceService.CreateTeamWorkspacesAsync(tenantId, teamMembers, assessmentIds, createdBy);
    }

    /// <summary>
    /// Complete full smart onboarding with assessments and workspace setup
    /// </summary>
    public async Task<FullOnboardingResultDto> CompleteFullSmartOnboardingAsync(
        Guid tenantId,
        string userId,
        List<TeamMemberDto>? teamMembers = null)
    {
        try
        {
            _logger.LogInformation("Starting full smart onboarding for tenant {TenantId}", tenantId);

            // Step 1: Complete standard smart onboarding
            var basicResult = await CompleteSmartOnboardingAsync(tenantId, userId);

            // Step 2: Generate actual assessments from templates
            var assessments = await GenerateAssessmentsFromTemplatesAsync(
                tenantId,
                basicResult.GeneratedPlan?.PlanId ?? Guid.Empty,
                basicResult.GeneratedTemplates,
                userId);

            var assessmentIds = assessments.Select(a => a.Id).ToList();

            // Step 3: Get control counts
            var totalControls = await _unitOfWork.AssessmentRequirements
                .Query()
                .CountAsync(r => assessmentIds.Contains(r.AssessmentId) && !r.IsDeleted);

            // Step 4: Setup team workspaces
            var workspaces = new List<UserWorkspace>();
            var totalTasks = 0;

            // If no team members provided, create default workspace for admin
            if (teamMembers == null || !teamMembers.Any())
            {
                teamMembers = new List<TeamMemberDto>
                {
                    new TeamMemberDto
                    {
                        UserId = userId,
                        Name = "Admin",
                        RoleCode = "GRC_MANAGER",
                        AssignedFrameworks = basicResult.GeneratedTemplates.Select(t => t.FrameworkCode).ToList()
                    }
                };
            }

            workspaces = await SetupTeamWorkspacesAsync(tenantId, teamMembers, assessmentIds, userId);

            // Count total tasks
            foreach (var workspace in workspaces)
            {
                var taskCount = await _unitOfWork.UserWorkspaceTasks
                    .Query()
                    .CountAsync(t => t.WorkspaceId == workspace.Id && !t.IsDeleted);
                totalTasks += taskCount;
            }

            // Build result
            var result = new FullOnboardingResultDto
            {
                TenantId = tenantId,
                Success = true,
                Message = $"Full smart onboarding completed. Generated {assessments.Count} assessments with {totalControls} controls, and {workspaces.Count} team workspaces with {totalTasks} pre-mapped tasks.",
                Scope = basicResult.Scope,
                GeneratedPlan = basicResult.GeneratedPlan,
                GeneratedTemplates = basicResult.GeneratedTemplates,
                GeneratedAssessments = assessments.Select(a => new AssessmentSummaryDto
                {
                    Id = a.Id,
                    AssessmentCode = a.AssessmentCode,
                    Name = a.Name,
                    FrameworkCode = a.FrameworkCode,
                    DueDate = a.DueDate,
                    Status = a.Status
                }).ToList(),
                TeamWorkspaces = workspaces.Select(w => new WorkspaceSummaryDto
                {
                    Id = w.Id,
                    UserId = w.UserId.ToString(),
                    RoleCode = w.RoleCode,
                    RoleName = w.RoleName,
                    DefaultLandingPage = w.DefaultLandingPage
                }).ToList(),
                TotalControls = totalControls,
                TotalTasks = totalTasks,
                EstimatedCompletionDate = basicResult.GeneratedPlan?.TargetEndDate ?? DateTime.UtcNow.AddMonths(6),
                CompletedAt = DateTime.UtcNow
            };

            // Update workspace task counts
            foreach (var ws in result.TeamWorkspaces)
            {
                var workspace = workspaces.First(w => w.Id == ws.Id);
                ws.AssignedTasks = await _unitOfWork.UserWorkspaceTasks
                    .Query()
                    .CountAsync(t => t.WorkspaceId == workspace.Id && !t.IsDeleted);
            }

            _logger.LogInformation("Full smart onboarding completed for tenant {TenantId}: {Assessments} assessments, {Controls} controls, {Workspaces} workspaces, {Tasks} tasks",
                tenantId, assessments.Count, totalControls, workspaces.Count, totalTasks);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in full smart onboarding for tenant {TenantId}", tenantId);
            throw;
        }
    }

    #endregion

    #region Auto-Team and RACI Provisioning

    /// <summary>
    /// Create default GRC team and RACI assignments for a tenant during onboarding
    /// </summary>
    public async Task<Team> ProvisionDefaultTeamAndRACIAsync(Guid tenantId, string createdBy)
    {
        try
        {
            _logger.LogInformation("Provisioning default team and RACI for tenant {TenantId}", tenantId);

            // Check if default team already exists
            var existingTeam = await _unitOfWork.Teams
                .Query()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsDefaultFallback && !t.IsDeleted);

            if (existingTeam != null)
            {
                _logger.LogInformation("Default team already exists for tenant {TenantId}", tenantId);
                return existingTeam;
            }

            // Create default GRC team
            var team = new Team
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TeamCode = "GRC-DEFAULT",
                Name = "GRC Team",
                NameAr = "فريق الحوكمة والمخاطر والامتثال",
                Purpose = "Default GRC team for compliance, risk, and governance activities",
                Description = "Auto-provisioned default team for handling GRC workflows and tasks",
                TeamType = "Governance",
                BusinessUnit = "Compliance",
                IsDefaultFallback = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _unitOfWork.Teams.AddAsync(team);

            // Create default RACI assignments for common scopes
            var raciScopes = new[]
            {
                ("ControlFamily", "Cybersecurity", "R"),
                ("ControlFamily", "DataProtection", "R"),
                ("ControlFamily", "AccessControl", "R"),
                ("ControlFamily", "RiskManagement", "R"),
                ("Framework", "NCA-ECC", "R"),
                ("Framework", "PDPL", "R"),
                ("Framework", "SAMA-CSF", "R"),
                ("Assessment", "Default", "R"),
                ("Evidence", "Default", "R")
            };

            foreach (var (scopeType, scopeId, raci) in raciScopes)
            {
                var raciAssignment = new RACIAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TeamId = team.Id,
                    ScopeType = scopeType,
                    ScopeId = scopeId,
                    RACI = raci,
                    RoleCode = "COMPLIANCE_OFFICER",
                    Priority = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                await _unitOfWork.RACIAssignments.AddAsync(raciAssignment);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created default team {TeamId} with {RaciCount} RACI assignments for tenant {TenantId}",
                team.Id, raciScopes.Length, tenantId);

            return team;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error provisioning default team and RACI for tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <summary>
    /// Add team member with role during onboarding
    /// </summary>
    public async Task<TeamMember> AddTeamMemberAsync(
        Guid tenantId,
        Guid teamId,
        Guid userId,
        string roleCode,
        bool isPrimary,
        string createdBy)
    {
        try
        {
            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TeamId = teamId,
                UserId = userId,
                RoleCode = roleCode,
                IsPrimaryForRole = isPrimary,
                CanApprove = roleCode is "COMPLIANCE_OFFICER" or "GRC_MANAGER" or "APPROVER",
                CanDelegate = roleCode is "COMPLIANCE_OFFICER" or "GRC_MANAGER",
                JoinedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _unitOfWork.TeamMembers.AddAsync(teamMember);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Added team member {UserId} with role {RoleCode} to team {TeamId}",
                userId, roleCode, teamId);

            return teamMember;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team member");
            throw;
        }
    }

    #endregion
}

/// <summary>
/// KSA Framework information
/// </summary>
internal class KsaFrameworkInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int EstimatedControls { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
