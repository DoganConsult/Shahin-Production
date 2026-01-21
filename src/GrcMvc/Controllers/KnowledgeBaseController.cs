using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Knowledge Base Controller - Team guidance, role guides, and workflow documentation
/// قاعدة المعرفة - دليل الفريق والأدوار وسير العمل
/// </summary>
[Authorize]
[Route("[controller]")]
public class KnowledgeBaseController : Controller
{
    private readonly GrcDbContext _context;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(GrcDbContext context, ILogger<KnowledgeBaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Knowledge Base Home
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        var model = new KnowledgeBaseViewModel
        {
            Categories = GetCategories(),
            QuickLinks = GetQuickLinks(),
            RecentArticles = GetRecentArticles()
        };
        return View(model);
    }

    /// <summary>
    /// Role-specific guide
    /// </summary>
    [HttpGet("Role/{roleCode}")]
    public async Task<IActionResult> RoleGuide(string roleCode)
    {
        var role = await _context.RoleCatalogs.FirstOrDefaultAsync(r => r.RoleCode == roleCode);
        if (role == null)
        {
            TempData["Error"] = $"Role '{roleCode}' not found";
            return RedirectToAction("Index");
        }

        var model = new RoleGuideViewModel
        {
            RoleCode = role.RoleCode,
            RoleName = role.RoleName,
            RoleNameAr = role.RoleName, // RoleCatalog doesn't have RoleNameAr
            Description = role.Description ?? "",
            Responsibilities = new List<string>(), // Simplified - no ResponsibilitiesJson in RoleCatalog
            WorkflowParticipation = new List<string>(), // Simplified
            Permissions = GetRolePermissions(roleCode),
            RelatedGuides = GetRelatedGuides(roleCode)
        };

        return View(model);
    }

    /// <summary>
    /// Workflow guide
    /// </summary>
    [HttpGet("Workflow/{workflowCode}")]
    public async Task<IActionResult> WorkflowGuide(string workflowCode)
    {
        var workflow = await _context.WorkflowDefinitions.FirstOrDefaultAsync(w => w.WorkflowNumber == workflowCode);
        if (workflow == null)
        {
            TempData["Error"] = $"Workflow '{workflowCode}' not found";
            return RedirectToAction("Index");
        }

        var model = new WorkflowGuideViewModel
        {
            WorkflowCode = workflow.WorkflowNumber,
            WorkflowName = workflow.Name,
            Description = workflow.Description ?? "",
            Category = workflow.Category ?? "",
            Steps = ParseWorkflowSteps(workflow.StepsJson),
            EstimatedDuration = workflow.EstimatedDays * 24, // Convert days to hours
            Participants = GetWorkflowParticipants(workflowCode)
        };

        return View(model);
    }

    /// <summary>
    /// Control Ownership Guide
    /// </summary>
    [HttpGet("ControlOwnership")]
    public IActionResult ControlOwnership()
    {
        var model = new ControlOwnershipGuideViewModel
        {
            Principles = GetOwnershipPrinciples(),
            RACIMatrix = GetRACIExamples(),
            BestPractices = GetOwnershipBestPractices()
        };
        return View(model);
    }

    /// <summary>
    /// Evidence Collection Guide
    /// </summary>
    [HttpGet("EvidenceCollection")]
    public IActionResult EvidenceCollection()
    {
        var model = new EvidenceGuideViewModel
        {
            EvidenceTypes = GetEvidenceTypes(),
            CollectionProcess = GetCollectionSteps(),
            QualityCriteria = GetQualityCriteria(),
            CommonMistakes = GetCommonMistakes()
        };
        return View(model);
    }

    /// <summary>
    /// Getting Started Guide
    /// </summary>
    [HttpGet("GettingStarted")]
    public IActionResult GettingStarted()
    {
        var model = new GettingStartedViewModel
        {
            Steps = GetOnboardingSteps(),
            FAQs = GetCommonFAQs()
        };
        return View(model);
    }

    /// <summary>
    /// Search knowledge base
    /// </summary>
    [HttpGet("Search")]
    public IActionResult Search(string q)
    {
        var results = SearchKnowledgeBase(q);
        ViewBag.Query = q;
        return View(results);
    }
    
    /// <summary>
    /// Sector Mapping - GOSI 70+ sub-sectors to 18 main GRC sectors
    /// </summary>
    [HttpGet("SectorMapping")]
    public IActionResult SectorMapping()
    {
        return View();
    }

    #region Private Helpers

    private List<KBCategory> GetCategories() => new()
    {
        new KBCategory
        {
            Name = "Getting Started",
            NameAr = "البداية",
            Icon = "fas fa-rocket",
            Description = "New to the platform? Start here",
            ArticleCount = 5,
            Link = "/KnowledgeBase/GettingStarted"
        },
        new KBCategory
        {
            Name = "Role Guides",
            NameAr = "أدلة الأدوار",
            Icon = "fas fa-user-tag",
            Description = "Responsibilities and workflows per role",
            ArticleCount = 15,
            Link = "/RoleProfile/Roles"
        },
        new KBCategory
        {
            Name = "Workflow Guides",
            NameAr = "أدلة سير العمل",
            Icon = "fas fa-project-diagram",
            Description = "Step-by-step workflow documentation",
            ArticleCount = 7,
            Link = "/Workflow"
        },
        new KBCategory
        {
            Name = "Control Ownership",
            NameAr = "ملكية الضوابط",
            Icon = "fas fa-clipboard-check",
            Description = "How to own and manage controls",
            ArticleCount = 3,
            Link = "/KnowledgeBase/ControlOwnership"
        },
        new KBCategory
        {
            Name = "Evidence Collection",
            NameAr = "جمع الأدلة",
            Icon = "fas fa-file-alt",
            Description = "Evidence requirements and best practices",
            ArticleCount = 8,
            Link = "/KnowledgeBase/EvidenceCollection"
        },
        new KBCategory
        {
            Name = "Compliance Calendar",
            NameAr = "تقويم الامتثال",
            Icon = "fas fa-calendar-alt",
            Description = "Key dates and deadlines",
            ArticleCount = 2,
            Link = "/ComplianceCalendar"
        }
    };

    private List<QuickLink> GetQuickLinks() => new()
    {
        new QuickLink { Title = "Submit Evidence", Icon = "fas fa-upload", Url = "/Evidence/Submit" },
        new QuickLink { Title = "My Tasks", Icon = "fas fa-tasks", Url = "/Inbox" },
        new QuickLink { Title = "Request Support", Icon = "fas fa-headset", Url = "/Help/Contact" },
        new QuickLink { Title = "View Calendar", Icon = "fas fa-calendar", Url = "/ComplianceCalendar" }
    };

    private List<KBArticle> GetRecentArticles() => new()
    {
        new KBArticle { Title = "How to Complete Control Assessments", Category = "Workflows", ReadTime = 5 },
        new KBArticle { Title = "Evidence Naming Conventions", Category = "Evidence", ReadTime = 3 },
        new KBArticle { Title = "Understanding Risk Ratings", Category = "Risk Management", ReadTime = 4 },
        new KBArticle { Title = "Requesting Exceptions", Category = "Workflows", ReadTime = 3 }
    };

    private List<string> GetRolePermissions(string roleCode) => roleCode switch
    {
        "CRO" => new() { "Grc.Risks.*", "Grc.Dashboard", "Grc.Reports.*", "Grc.Assessments.Approve" },
        "CISO" => new() { "Grc.Controls.*", "Grc.Evidence.*", "Grc.Audits.*", "Grc.Policies.*" },
        "CCO" => new() { "Grc.Assessments.*", "Grc.Frameworks.*", "Grc.Policies.Approve", "Grc.Reports.*" },
        "DPO" => new() { "Grc.Evidence.*", "Grc.Policies.*", "Grc.Assessments.*" },
        _ => new() { "Grc.Dashboard", "Grc.Evidence.View" }
    };

    private List<RelatedGuide> GetRelatedGuides(string roleCode) => new()
    {
        new RelatedGuide { Title = "Control Assessment Workflow", Url = "/KnowledgeBase/Workflow/CTRL_ASSESS" },
        new RelatedGuide { Title = "Evidence Submission", Url = "/KnowledgeBase/EvidenceCollection" },
        new RelatedGuide { Title = "Exception Handling", Url = "/KnowledgeBase/Workflow/EXCEPTION" }
    };

    private List<WorkflowStep> ParseWorkflowSteps(string? stepsJson)
    {
        if (string.IsNullOrEmpty(stepsJson)) return new();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<WorkflowStep>>(stepsJson) ?? new();
        }
        catch
        {
            return new();
        }
    }

    private List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }

    private List<WorkflowParticipant> GetWorkflowParticipants(string workflowCode) => new()
    {
        new WorkflowParticipant { Role = "Control Owner", Responsibility = "Initiates and provides implementation evidence" },
        new WorkflowParticipant { Role = "Reviewer", Responsibility = "Validates evidence and assessment results" },
        new WorkflowParticipant { Role = "Approver", Responsibility = "Final approval authority" }
    };

    private List<OwnershipPrinciple> GetOwnershipPrinciples() => new()
    {
        new OwnershipPrinciple { Title = "Single Accountability", Description = "Each control has ONE designated owner responsible for its effectiveness" },
        new OwnershipPrinciple { Title = "Domain Expertise", Description = "Owners should have relevant expertise in the control's domain" },
        new OwnershipPrinciple { Title = "Authority to Act", Description = "Owners must have authority to implement and modify controls" },
        new OwnershipPrinciple { Title = "Regular Review", Description = "Owners must review control effectiveness at defined intervals" }
    };

    private List<RACIExample> GetRACIExamples() => new()
    {
        new RACIExample { Activity = "Control Implementation", Responsible = "Control Owner", Accountable = "CISO", Consulted = "Risk Manager", Informed = "Audit" },
        new RACIExample { Activity = "Evidence Collection", Responsible = "Evidence Collector", Accountable = "Control Owner", Consulted = "Compliance", Informed = "Auditor" },
        new RACIExample { Activity = "Exception Approval", Responsible = "Risk Manager", Accountable = "CRO", Consulted = "Legal", Informed = "Audit, Board" }
    };

    private List<string> GetOwnershipBestPractices() => new()
    {
        "Document all control changes with business justification",
        "Maintain evidence of control effectiveness continuously",
        "Escalate issues early - don't wait for audit findings",
        "Coordinate with related control owners for dependencies",
        "Review control adequacy when business processes change"
    };

    private List<EvidenceType> GetEvidenceTypes() => new()
    {
        new EvidenceType { Name = "Configuration Screenshot", Examples = new[] { "Firewall rules", "Access settings" }, Frequency = "Quarterly" },
        new EvidenceType { Name = "System Report", Examples = new[] { "Access review logs", "Patch status" }, Frequency = "Monthly" },
        new EvidenceType { Name = "Policy Document", Examples = new[] { "Security policy", "Procedure docs" }, Frequency = "Annual" },
        new EvidenceType { Name = "Training Records", Examples = new[] { "Attendance", "Completion certificates" }, Frequency = "Annual" }
    };

    private List<ProcessStep> GetCollectionSteps() => new()
    {
        new ProcessStep { Order = 1, Title = "Identify Required Evidence", Description = "Check control requirements for evidence type" },
        new ProcessStep { Order = 2, Title = "Collect from Source", Description = "Export/screenshot from source system" },
        new ProcessStep { Order = 3, Title = "Apply Naming Convention", Description = "Use standard naming: {TenantId}-{ControlId}-{Date}" },
        new ProcessStep { Order = 4, Title = "Upload to System", Description = "Upload via Evidence > Submit" },
        new ProcessStep { Order = 5, Title = "Link to Controls", Description = "Associate with applicable controls" }
    };

    private List<QualityCriterion> GetQualityCriteria() => new()
    {
        new QualityCriterion { Criterion = "Completeness", Description = "Evidence covers the full control scope" },
        new QualityCriterion { Criterion = "Currency", Description = "Evidence is from the assessment period" },
        new QualityCriterion { Criterion = "Authenticity", Description = "Evidence is unaltered from source" },
        new QualityCriterion { Criterion = "Relevance", Description = "Evidence directly supports control effectiveness" }
    };

    private List<string> GetCommonMistakes() => new()
    {
        "Submitting outdated evidence (check dates!)",
        "Missing key information in screenshots (crop too tight)",
        "Using wrong naming convention",
        "Not linking to correct controls",
        "Submitting duplicate evidence for same period"
    };

    private List<OnboardingStep> GetOnboardingSteps() => new()
    {
        new OnboardingStep { Order = 1, Title = "Complete Organization Profile", Description = "Answer 96 questions about your organization" },
        new OnboardingStep { Order = 2, Title = "Review Derived Scope", Description = "Verify the automatically derived frameworks and controls" },
        new OnboardingStep { Order = 3, Title = "Set Up Team", Description = "Invite team members and assign roles" },
        new OnboardingStep { Order = 4, Title = "Assign Control Owners", Description = "Map controls to responsible owners" },
        new OnboardingStep { Order = 5, Title = "Start First Assessment", Description = "Begin your baseline assessment" }
    };

    private List<FAQ> GetCommonFAQs() => new()
    {
        new FAQ { Question = "How do I reset my password?", Answer = "Go to Account > Change Password or use Forgot Password on login" },
        new FAQ { Question = "How do I invite team members?", Answer = "Go to Team > Invite User or ask your Tenant Admin" },
        new FAQ { Question = "Where do I see my pending tasks?", Answer = "Your Inbox shows all pending workflow items" }
    };

    private List<KBSearchResult> SearchKnowledgeBase(string query)
    {
        // Simplified search - in production would use full-text search
        if (string.IsNullOrWhiteSpace(query)) return new();

        return new()
        {
            new KBSearchResult { Title = "Evidence Collection Guide", Snippet = "Learn how to collect and submit evidence...", Url = "/KnowledgeBase/EvidenceCollection" },
            new KBSearchResult { Title = "Control Owner Responsibilities", Snippet = "Control owners are responsible for...", Url = "/KnowledgeBase/ControlOwnership" }
        };
    }

    #endregion
}

#region View Models

public class KnowledgeBaseViewModel
{
    public List<KBCategory> Categories { get; set; } = new();
    public List<QuickLink> QuickLinks { get; set; } = new();
    public List<KBArticle> RecentArticles { get; set; } = new();
}

public class KBCategory
{
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public int ArticleCount { get; set; }
    public string Link { get; set; } = "";
}

public class QuickLink
{
    public string Title { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Url { get; set; } = "";
}

public class KBArticle
{
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public int ReadTime { get; set; }
}

public class RoleGuideViewModel
{
    public string RoleCode { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string RoleNameAr { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Responsibilities { get; set; } = new();
    public List<string> WorkflowParticipation { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public List<RelatedGuide> RelatedGuides { get; set; } = new();
}

public class RelatedGuide
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
}

public class WorkflowGuideViewModel
{
    public string WorkflowCode { get; set; } = "";
    public string WorkflowName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public List<WorkflowStep> Steps { get; set; } = new();
    public int EstimatedDuration { get; set; }
    public List<WorkflowParticipant> Participants { get; set; } = new();
}

public class WorkflowStep
{
    public int Order { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? AssigneeRole { get; set; }
}

public class WorkflowParticipant
{
    public string Role { get; set; } = "";
    public string Responsibility { get; set; } = "";
}

public class ControlOwnershipGuideViewModel
{
    public List<OwnershipPrinciple> Principles { get; set; } = new();
    public List<RACIExample> RACIMatrix { get; set; } = new();
    public List<string> BestPractices { get; set; } = new();
}

public class OwnershipPrinciple
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class RACIExample
{
    public string Activity { get; set; } = "";
    public string Responsible { get; set; } = "";
    public string Accountable { get; set; } = "";
    public string Consulted { get; set; } = "";
    public string Informed { get; set; } = "";
}

public class EvidenceGuideViewModel
{
    public List<EvidenceType> EvidenceTypes { get; set; } = new();
    public List<ProcessStep> CollectionProcess { get; set; } = new();
    public List<QualityCriterion> QualityCriteria { get; set; } = new();
    public List<string> CommonMistakes { get; set; } = new();
}

public class EvidenceType
{
    public string Name { get; set; } = "";
    public string[] Examples { get; set; } = Array.Empty<string>();
    public string Frequency { get; set; } = "";
}

public class ProcessStep
{
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class QualityCriterion
{
    public string Criterion { get; set; } = "";
    public string Description { get; set; } = "";
}

public class GettingStartedViewModel
{
    public List<OnboardingStep> Steps { get; set; } = new();
    public List<FAQ> FAQs { get; set; } = new();
}

public class OnboardingStep
{
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class FAQ
{
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

public class KBSearchResult
{
    public string Title { get; set; } = "";
    public string Snippet { get; set; } = "";
    public string Url { get; set; } = "";
}

#endregion
