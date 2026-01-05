using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for managing role-based user workspaces
/// </summary>
public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkspaceService> _logger;

    // Default workspace configurations per role
    private static readonly Dictionary<string, RoleWorkspaceConfig> RoleConfigs = new()
    {
        ["COMPLIANCE_OFFICER"] = new RoleWorkspaceConfig
        {
            RoleName = "Compliance Officer",
            RoleNameAr = "مسؤول الامتثال",
            DefaultLandingPage = "/Dashboard",
            DashboardWidgets = new[] { "ComplianceScore", "PendingTasks", "FrameworkProgress", "RecentActivity" },
            QuickActions = new[] { "StartAssessment", "ReviewEvidence", "GenerateReport", "ViewGaps" },
            AssignableTaskTypes = new[] { "Assessment", "Review", "Approval", "Report" }
        },
        ["CONTROL_OWNER"] = new RoleWorkspaceConfig
        {
            RoleName = "Control Owner",
            RoleNameAr = "مالك الضابط",
            DefaultLandingPage = "/Controls",
            DashboardWidgets = new[] { "MyControls", "PendingEvidence", "ControlStatus", "UpcomingDeadlines" },
            QuickActions = new[] { "UploadEvidence", "UpdateControlStatus", "RequestExtension", "ViewMyControls" },
            AssignableTaskTypes = new[] { "Evidence", "ControlUpdate", "Remediation" }
        },
        ["RISK_MANAGER"] = new RoleWorkspaceConfig
        {
            RoleName = "Risk Manager",
            RoleNameAr = "مدير المخاطر",
            DefaultLandingPage = "/Risks",
            DashboardWidgets = new[] { "RiskHeatmap", "TopRisks", "RiskTrends", "MitigationProgress" },
            QuickActions = new[] { "RegisterRisk", "AssessRisk", "ReviewMitigation", "RiskReport" },
            AssignableTaskTypes = new[] { "RiskAssessment", "Mitigation", "Review" }
        },
        ["DPO"] = new RoleWorkspaceConfig
        {
            RoleName = "Data Protection Officer",
            RoleNameAr = "مسؤول حماية البيانات",
            DefaultLandingPage = "/PDPL",
            DashboardWidgets = new[] { "PDPLCompliance", "PIAStatus", "DataSubjectRequests", "BreachAlerts" },
            QuickActions = new[] { "StartPIA", "ReviewConsent", "DataMapping", "BreachNotification" },
            AssignableTaskTypes = new[] { "PIA", "ConsentReview", "DataMapping", "BreachResponse" }
        },
        ["SECURITY_OFFICER"] = new RoleWorkspaceConfig
        {
            RoleName = "Security Officer",
            RoleNameAr = "مسؤول الأمن",
            DefaultLandingPage = "/Security",
            DashboardWidgets = new[] { "SecurityScore", "VulnerabilityStatus", "IncidentAlerts", "AccessReview" },
            QuickActions = new[] { "ReviewIncident", "AccessReview", "VulnerabilityScan", "SecurityReport" },
            AssignableTaskTypes = new[] { "SecurityAssessment", "IncidentResponse", "AccessReview" }
        },
        ["AUDITOR"] = new RoleWorkspaceConfig
        {
            RoleName = "Internal Auditor",
            RoleNameAr = "المدقق الداخلي",
            DefaultLandingPage = "/Audits",
            DashboardWidgets = new[] { "AuditSchedule", "OpenFindings", "AuditProgress", "RemediationTracking" },
            QuickActions = new[] { "StartAudit", "AddFinding", "ReviewRemediation", "AuditReport" },
            AssignableTaskTypes = new[] { "Audit", "Finding", "RemediationReview" }
        },
        ["GRC_MANAGER"] = new RoleWorkspaceConfig
        {
            RoleName = "GRC Manager",
            RoleNameAr = "مدير الحوكمة والمخاطر والامتثال",
            DefaultLandingPage = "/Dashboard",
            DashboardWidgets = new[] { "ExecutiveSummary", "AllFrameworks", "TeamProgress", "KeyMetrics" },
            QuickActions = new[] { "ViewDashboard", "AssignTasks", "GenerateExecutiveReport", "TeamManagement" },
            AssignableTaskTypes = new[] { "All" }
        }
    };

    public WorkspaceService(IUnitOfWork unitOfWork, ILogger<WorkspaceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserWorkspace> CreateWorkspaceAsync(
        Guid tenantId, string userId, string roleCode, string createdBy)
    {
        // Check if workspace already exists
        var existing = await _unitOfWork.UserWorkspaces
            .Query()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.TenantId == tenantId && !w.IsDeleted);

        if (existing != null)
        {
            _logger.LogInformation("Workspace already exists for user {UserId}", userId);
            return existing;
        }

        // Get role configuration
        var config = RoleConfigs.GetValueOrDefault(roleCode) ?? RoleConfigs["COMPLIANCE_OFFICER"];

        var workspace = new UserWorkspace
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            RoleCode = roleCode,
            RoleName = config.RoleName,
            RoleNameAr = config.RoleNameAr,
            DefaultLandingPage = config.DefaultLandingPage,
            DashboardWidgetsJson = JsonSerializer.Serialize(config.DashboardWidgets),
            QuickActionsJson = JsonSerializer.Serialize(config.QuickActions),
            WorkspaceConfigJson = JsonSerializer.Serialize(new
            {
                config.AssignableTaskTypes,
                Theme = "default",
                Language = "ar",
                NotificationsEnabled = true
            }),
            IsConfigured = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.UserWorkspaces.AddAsync(workspace);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created workspace for user {UserId} with role {RoleCode}", userId, roleCode);
        return workspace;
    }

    public async Task<UserWorkspace?> GetUserWorkspaceAsync(string userId)
    {
        return await _unitOfWork.UserWorkspaces
            .Query()
            .Include(w => w.Tasks.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
    }

    public async Task<List<UserWorkspaceTask>> PreMapTasksAsync(
        Guid workspaceId, Guid tenantId, string roleCode, List<Guid> assessmentIds, string createdBy)
    {
        var tasks = new List<UserWorkspaceTask>();
        var config = RoleConfigs.GetValueOrDefault(roleCode) ?? RoleConfigs["COMPLIANCE_OFFICER"];

        // Get assessments
        var assessments = await _unitOfWork.Assessments
            .Query()
            .Where(a => assessmentIds.Contains(a.Id) && !a.IsDeleted)
            .ToListAsync();

        var displayOrder = 0;

        foreach (var assessment in assessments)
        {
            // Create assessment task
            var assessmentTask = new UserWorkspaceTask
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                TenantId = tenantId,
                Title = $"Complete {assessment.Name}",
                TitleAr = $"إكمال تقييم {assessment.Name}",
                Description = $"Review and complete the {assessment.Name} assessment",
                TaskType = "Assessment",
                RelatedEntityId = assessment.Id,
                RelatedEntityType = "Assessment",
                ActionUrl = $"/Assessments/Details/{assessment.Id}",
                Priority = 1,
                DueDate = assessment.DueDate,
                Status = "Pending",
                FrameworkCode = assessment.FrameworkCode,
                EstimatedHours = 8,
                DisplayOrder = displayOrder++,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            tasks.Add(assessmentTask);

            // Create evidence collection task for control owners
            if (roleCode == "CONTROL_OWNER" || config.AssignableTaskTypes.Contains("Evidence"))
            {
                var evidenceTask = new UserWorkspaceTask
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    TenantId = tenantId,
                    Title = $"Collect Evidence for {assessment.Name}",
                    TitleAr = $"جمع الأدلة لـ {assessment.Name}",
                    Description = $"Upload supporting evidence for controls in {assessment.Name}",
                    TaskType = "Evidence",
                    RelatedEntityId = assessment.Id,
                    RelatedEntityType = "Assessment",
                    ActionUrl = $"/Evidence/Upload?assessmentId={assessment.Id}",
                    Priority = 2,
                    DueDate = assessment.DueDate?.AddDays(-7), // Due 1 week before assessment
                    Status = "Pending",
                    FrameworkCode = assessment.FrameworkCode,
                    EstimatedHours = 16,
                    DisplayOrder = displayOrder++,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                tasks.Add(evidenceTask);
            }

            // Create review task for compliance officers and managers
            if (roleCode == "COMPLIANCE_OFFICER" || roleCode == "GRC_MANAGER" ||
                config.AssignableTaskTypes.Contains("Review"))
            {
                var reviewTask = new UserWorkspaceTask
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    TenantId = tenantId,
                    Title = $"Review {assessment.Name} Results",
                    TitleAr = $"مراجعة نتائج {assessment.Name}",
                    Description = $"Review and approve assessment results for {assessment.Name}",
                    TaskType = "Review",
                    RelatedEntityId = assessment.Id,
                    RelatedEntityType = "Assessment",
                    ActionUrl = $"/Assessments/Review/{assessment.Id}",
                    Priority = 2,
                    DueDate = assessment.DueDate?.AddDays(3), // Due 3 days after assessment
                    Status = "Pending",
                    FrameworkCode = assessment.FrameworkCode,
                    EstimatedHours = 4,
                    DisplayOrder = displayOrder++,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                tasks.Add(reviewTask);
            }
        }

        // Add initial onboarding tasks
        tasks.Add(new UserWorkspaceTask
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            TenantId = tenantId,
            Title = "Complete Profile Setup",
            TitleAr = "إكمال إعداد الملف الشخصي",
            Description = "Update your profile information and notification preferences",
            TaskType = "Setup",
            ActionUrl = "/Profile",
            Priority = 1,
            DueDate = DateTime.UtcNow.AddDays(3),
            Status = "Pending",
            EstimatedHours = 1,
            DisplayOrder = 0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        });

        tasks.Add(new UserWorkspaceTask
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            TenantId = tenantId,
            Title = "Review GRC Training Materials",
            TitleAr = "مراجعة مواد التدريب على الحوكمة والمخاطر والامتثال",
            Description = "Complete the introductory training for the GRC system",
            TaskType = "Training",
            ActionUrl = "/Training",
            Priority = 2,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = "Pending",
            EstimatedHours = 2,
            DisplayOrder = 1,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        });

        foreach (var task in tasks)
        {
            await _unitOfWork.UserWorkspaceTasks.AddAsync(task);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Pre-mapped {Count} tasks for workspace {WorkspaceId}", tasks.Count, workspaceId);

        return tasks;
    }

    public async Task<WorkspaceTemplate?> GetWorkspaceTemplateAsync(string roleCode)
    {
        return await _unitOfWork.WorkspaceTemplates
            .Query()
            .FirstOrDefaultAsync(t => t.RoleCode == roleCode && t.IsActive && t.IsDefault && !t.IsDeleted);
    }

    public async Task<IEnumerable<UserWorkspaceTask>> GetUserTasksAsync(string userId, string? status = null)
    {
        var workspace = await GetUserWorkspaceAsync(userId);
        if (workspace == null) return Enumerable.Empty<UserWorkspaceTask>();

        var query = _unitOfWork.UserWorkspaceTasks
            .Query()
            .Where(t => t.WorkspaceId == workspace.Id && !t.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        return await query.OrderBy(t => t.Priority).ThenBy(t => t.DueDate).ToListAsync();
    }

    public async Task<UserWorkspaceTask> UpdateTaskStatusAsync(Guid taskId, string status, string modifiedBy)
    {
        var task = await _unitOfWork.UserWorkspaceTasks.GetByIdAsync(taskId);
        if (task == null)
            throw new InvalidOperationException($"Task {taskId} not found");

        task.Status = status;
        task.ModifiedDate = DateTime.UtcNow;
        task.ModifiedBy = modifiedBy;

        await _unitOfWork.SaveChangesAsync();
        return task;
    }

    public async Task<List<UserWorkspace>> CreateTeamWorkspacesAsync(
        Guid tenantId, List<TeamMemberDto> teamMembers, List<Guid> assessmentIds, string createdBy)
    {
        var workspaces = new List<UserWorkspace>();

        foreach (var member in teamMembers)
        {
            var workspace = await CreateWorkspaceAsync(tenantId, member.UserId, member.RoleCode, createdBy);

            // Pre-map tasks for this user
            await PreMapTasksAsync(workspace.Id, tenantId, member.RoleCode, assessmentIds, createdBy);

            // Update assigned frameworks
            if (member.AssignedFrameworks?.Any() == true)
            {
                workspace.AssignedFrameworks = string.Join(",", member.AssignedFrameworks);
                workspace.AssignedAssessmentIds = JsonSerializer.Serialize(assessmentIds);
                await _unitOfWork.SaveChangesAsync();
            }

            workspaces.Add(workspace);
        }

        _logger.LogInformation("Created {Count} team workspaces for tenant {TenantId}", workspaces.Count, tenantId);
        return workspaces;
    }
}

/// <summary>
/// Internal configuration class for role-based workspace setup
/// </summary>
internal class RoleWorkspaceConfig
{
    public string RoleName { get; set; } = string.Empty;
    public string RoleNameAr { get; set; } = string.Empty;
    public string DefaultLandingPage { get; set; } = "/Dashboard";
    public string[] DashboardWidgets { get; set; } = Array.Empty<string>();
    public string[] QuickActions { get; set; } = Array.Empty<string>();
    public string[] AssignableTaskTypes { get; set; } = Array.Empty<string>();
}
