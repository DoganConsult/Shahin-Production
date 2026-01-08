using System;
using System.Threading.Tasks;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;

namespace GrcMvc.Data
{
    /// <summary>
    /// Unit of Work pattern for managing transactions across repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IGenericRepository<Risk> Risks { get; }
        IGenericRepository<RiskControlMapping> RiskControlMappings { get; }
        IGenericRepository<Control> Controls { get; }
        IGenericRepository<Assessment> Assessments { get; }
        IGenericRepository<Audit> Audits { get; }
        IGenericRepository<AuditFinding> AuditFindings { get; }
        IGenericRepository<Evidence> Evidences { get; }
        IGenericRepository<Policy> Policies { get; }
        IGenericRepository<PolicyViolation> PolicyViolations { get; }
        IGenericRepository<Workflow> Workflows { get; }
        IGenericRepository<WorkflowExecution> WorkflowExecutions { get; }
        IGenericRepository<ActionPlan> ActionPlans { get; }
        IGenericRepository<Vendor> Vendors { get; }
        IGenericRepository<Regulator> Regulators { get; }
        IGenericRepository<ComplianceEvent> ComplianceEvents { get; }
        IGenericRepository<GrcMvc.Models.Entities.Framework> Frameworks { get; }

        // Multi-tenant & onboarding repositories
        IGenericRepository<Tenant> Tenants { get; }
        IGenericRepository<TenantUser> TenantUsers { get; }
        IGenericRepository<OwnerTenantCreation> OwnerTenantCreations { get; }
        IGenericRepository<OrganizationProfile> OrganizationProfiles { get; }
        IGenericRepository<OnboardingWizard> OnboardingWizards { get; }

        // Teams & RACI (Role-based workflow routing)
        IGenericRepository<Team> Teams { get; }
        IGenericRepository<TeamMember> TeamMembers { get; }
        IGenericRepository<RACIAssignment> RACIAssignments { get; }

        // Asset inventory (for recognition & scoping)
        IGenericRepository<Asset> Assets { get; }
        IGenericRepository<Ruleset> Rulesets { get; }
        IGenericRepository<Rule> Rules { get; }
        IGenericRepository<RuleExecutionLog> RuleExecutionLogs { get; }
        IGenericRepository<TenantBaseline> TenantBaselines { get; }
        IGenericRepository<TenantPackage> TenantPackages { get; }
        IGenericRepository<TenantTemplate> TenantTemplates { get; }
        IGenericRepository<Plan> Plans { get; }
        IGenericRepository<PlanPhase> PlanPhases { get; }
        IGenericRepository<AuditEvent> AuditEvents { get; }
        IGenericRepository<Report> Reports { get; }
        IGenericRepository<AssessmentRequirement> AssessmentRequirements { get; }
        IGenericRepository<FrameworkControl> FrameworkControls { get; }
        IGenericRepository<TemplateCatalog> TemplateCatalogs { get; }

        // STAGE 2: Workflow infrastructure repositories
        IGenericRepository<WorkflowDefinition> WorkflowDefinitions { get; }
        IGenericRepository<WorkflowInstance> WorkflowInstances { get; }
        IGenericRepository<WorkflowTask> WorkflowTasks { get; }
        IGenericRepository<ApprovalChain> ApprovalChains { get; }
        IGenericRepository<ApprovalInstance> ApprovalInstances { get; }
        IGenericRepository<EscalationRule> EscalationRules { get; }
        IGenericRepository<WorkflowAuditEntry> WorkflowAuditEntries { get; }

        // User Consent & Legal Documents
        IGenericRepository<UserConsent> UserConsents { get; }
        IGenericRepository<LegalDocument> LegalDocuments { get; }

        // Support Agent & Chat
        IGenericRepository<SupportConversation> SupportConversations { get; }
        IGenericRepository<SupportMessage> SupportMessages { get; }

        // User Workspace (Role-based pre-mapping)
        IGenericRepository<UserWorkspace> UserWorkspaces { get; }
        IGenericRepository<UserWorkspaceTask> UserWorkspaceTasks { get; }
        IGenericRepository<WorkspaceTemplate> WorkspaceTemplates { get; }

        // Transaction management
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        bool HasActiveTransaction { get; }
    }
}