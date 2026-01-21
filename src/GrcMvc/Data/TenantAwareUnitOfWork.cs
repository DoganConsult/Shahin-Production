using System;
using System.Threading.Tasks;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GrcMvc.Data
{
    /// <summary>
    /// Tenant-aware Unit of Work implementation.
    /// Uses IDbContextFactory to create tenant-isolated database contexts.
    /// 
    /// ABP Best Practice: Factory pattern for multi-tenant database isolation.
    /// Each tenant gets their own database connection based on ITenantContextService.
    /// </summary>
    public class TenantAwareUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IDbContextFactory<GrcDbContext> _contextFactory;
        private GrcDbContext? _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        // Lazy-loaded repositories
        private IGenericRepository<Risk>? _risks;
        private IGenericRepository<RiskControlMapping>? _riskControlMappings;
        private IGenericRepository<RiskAppetiteSetting>? _riskAppetiteSettings;
        private IGenericRepository<Control>? _controls;
        private IGenericRepository<Assessment>? _assessments;
        private IGenericRepository<Audit>? _audits;
        private IGenericRepository<AuditFinding>? _auditFindings;
        private IGenericRepository<Evidence>? _evidences;
        private IGenericRepository<Policy>? _policies;
        private IGenericRepository<PolicyViolation>? _policyViolations;
        private IGenericRepository<Workflow>? _workflows;
        private IGenericRepository<WorkflowExecution>? _workflowExecutions;
        private IGenericRepository<ActionPlan>? _actionPlans;
        private IGenericRepository<Vendor>? _vendors;
        private IGenericRepository<Regulator>? _regulators;
        private IGenericRepository<ComplianceEvent>? _complianceEvents;
        private IGenericRepository<GrcMvc.Models.Entities.Framework>? _frameworks;

        // Multi-tenant repositories
        private IGenericRepository<Tenant>? _tenants;
        private IGenericRepository<TenantUser>? _tenantUsers;
        private IGenericRepository<OwnerTenantCreation>? _ownerTenantCreations;
        private IGenericRepository<OrganizationProfile>? _organizationProfiles;
        private IGenericRepository<OnboardingWizard>? _onboardingWizards;

        // Teams & RACI
        private IGenericRepository<Team>? _teams;
        private IGenericRepository<TeamMember>? _teamMembers;
        private IGenericRepository<RACIAssignment>? _raciAssignments;

        // Asset inventory
        private IGenericRepository<Asset>? _assets;

        private IGenericRepository<Ruleset>? _rulesets;
        private IGenericRepository<Rule>? _rules;
        private IGenericRepository<RuleExecutionLog>? _ruleExecutionLogs;
        private IGenericRepository<TenantBaseline>? _tenantBaselines;
        private IGenericRepository<TenantPackage>? _tenantPackages;
        private IGenericRepository<TenantTemplate>? _tenantTemplates;
        private IGenericRepository<Plan>? _plans;
        private IGenericRepository<PlanPhase>? _planPhases;
        private IGenericRepository<AuditEvent>? _auditEvents;
        private IGenericRepository<Report>? _reports;
        private IGenericRepository<AssessmentRequirement>? _assessmentRequirements;
        private IGenericRepository<FrameworkControl>? _frameworkControls;
        private IGenericRepository<TemplateCatalog>? _templateCatalogs;

        // Workflow infrastructure
        private IGenericRepository<WorkflowDefinition>? _workflowDefinitions;
        private IGenericRepository<WorkflowInstance>? _workflowInstances;
        private IGenericRepository<WorkflowTask>? _workflowTasks;
        private IGenericRepository<ApprovalChain>? _approvalChains;
        private IGenericRepository<ApprovalInstance>? _approvalInstances;
        private IGenericRepository<EscalationRule>? _escalationRules;
        private IGenericRepository<WorkflowAuditEntry>? _workflowAuditEntries;

        // User Consent & Legal Documents
        private IGenericRepository<UserConsent>? _userConsents;
        private IGenericRepository<LegalDocument>? _legalDocuments;

        // Support Agent & Chat
        private IGenericRepository<SupportConversation>? _supportConversations;
        private IGenericRepository<SupportMessage>? _supportMessages;

        // User Workspace
        private IGenericRepository<UserWorkspace>? _userWorkspaces;
        private IGenericRepository<UserWorkspaceTask>? _userWorkspaceTasks;
        private IGenericRepository<WorkspaceTemplate>? _workspaceTemplates;

        public TenantAwareUnitOfWork(IDbContextFactory<GrcDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        /// Gets the tenant-specific DbContext, creating it lazily on first access.
        /// This ensures each request gets the correct tenant's database.
        /// </summary>
        private GrcDbContext Context => _context ??= _contextFactory.CreateDbContext();

        // Repository properties with lazy initialization using tenant-aware context
        public IGenericRepository<Risk> Risks =>
            _risks ??= new GenericRepository<Risk>(Context);

        public IGenericRepository<RiskControlMapping> RiskControlMappings =>
            _riskControlMappings ??= new GenericRepository<RiskControlMapping>(Context);

        public IGenericRepository<RiskAppetiteSetting> RiskAppetiteSettings =>
            _riskAppetiteSettings ??= new GenericRepository<RiskAppetiteSetting>(Context);

        public IGenericRepository<Control> Controls =>
            _controls ??= new GenericRepository<Control>(Context);

        public IGenericRepository<Assessment> Assessments =>
            _assessments ??= new GenericRepository<Assessment>(Context);

        public IGenericRepository<Audit> Audits =>
            _audits ??= new GenericRepository<Audit>(Context);

        public IGenericRepository<AuditFinding> AuditFindings =>
            _auditFindings ??= new GenericRepository<AuditFinding>(Context);

        public IGenericRepository<Evidence> Evidences =>
            _evidences ??= new GenericRepository<Evidence>(Context);

        public IGenericRepository<Policy> Policies =>
            _policies ??= new GenericRepository<Policy>(Context);

        public IGenericRepository<PolicyViolation> PolicyViolations =>
            _policyViolations ??= new GenericRepository<PolicyViolation>(Context);

        public IGenericRepository<Workflow> Workflows =>
            _workflows ??= new GenericRepository<Workflow>(Context);

        public IGenericRepository<WorkflowExecution> WorkflowExecutions =>
            _workflowExecutions ??= new GenericRepository<WorkflowExecution>(Context);

        public IGenericRepository<ActionPlan> ActionPlans =>
            _actionPlans ??= new GenericRepository<ActionPlan>(Context);

        public IGenericRepository<Vendor> Vendors =>
            _vendors ??= new GenericRepository<Vendor>(Context);

        public IGenericRepository<Regulator> Regulators =>
            _regulators ??= new GenericRepository<Regulator>(Context);

        public IGenericRepository<ComplianceEvent> ComplianceEvents =>
            _complianceEvents ??= new GenericRepository<ComplianceEvent>(Context);

        public IGenericRepository<GrcMvc.Models.Entities.Framework> Frameworks =>
            _frameworks ??= new GenericRepository<GrcMvc.Models.Entities.Framework>(Context);

        public IGenericRepository<Tenant> Tenants =>
            _tenants ??= new GenericRepository<Tenant>(Context);

        public IGenericRepository<TenantUser> TenantUsers =>
            _tenantUsers ??= new GenericRepository<TenantUser>(Context);

        public IGenericRepository<OwnerTenantCreation> OwnerTenantCreations =>
            _ownerTenantCreations ??= new GenericRepository<OwnerTenantCreation>(Context);

        public IGenericRepository<OrganizationProfile> OrganizationProfiles =>
            _organizationProfiles ??= new GenericRepository<OrganizationProfile>(Context);

        public IGenericRepository<OnboardingWizard> OnboardingWizards =>
            _onboardingWizards ??= new GenericRepository<OnboardingWizard>(Context);

        public IGenericRepository<Team> Teams =>
            _teams ??= new GenericRepository<Team>(Context);

        public IGenericRepository<TeamMember> TeamMembers =>
            _teamMembers ??= new GenericRepository<TeamMember>(Context);

        public IGenericRepository<RACIAssignment> RACIAssignments =>
            _raciAssignments ??= new GenericRepository<RACIAssignment>(Context);

        public IGenericRepository<Asset> Assets =>
            _assets ??= new GenericRepository<Asset>(Context);

        public IGenericRepository<Ruleset> Rulesets =>
            _rulesets ??= new GenericRepository<Ruleset>(Context);

        public IGenericRepository<Rule> Rules =>
            _rules ??= new GenericRepository<Rule>(Context);

        public IGenericRepository<RuleExecutionLog> RuleExecutionLogs =>
            _ruleExecutionLogs ??= new GenericRepository<RuleExecutionLog>(Context);

        public IGenericRepository<TenantBaseline> TenantBaselines =>
            _tenantBaselines ??= new GenericRepository<TenantBaseline>(Context);

        public IGenericRepository<TenantPackage> TenantPackages =>
            _tenantPackages ??= new GenericRepository<TenantPackage>(Context);

        public IGenericRepository<TenantTemplate> TenantTemplates =>
            _tenantTemplates ??= new GenericRepository<TenantTemplate>(Context);

        public IGenericRepository<Plan> Plans =>
            _plans ??= new GenericRepository<Plan>(Context);

        public IGenericRepository<PlanPhase> PlanPhases =>
            _planPhases ??= new GenericRepository<PlanPhase>(Context);

        public IGenericRepository<AuditEvent> AuditEvents =>
            _auditEvents ??= new GenericRepository<AuditEvent>(Context);

        public IGenericRepository<Report> Reports =>
            _reports ??= new GenericRepository<Report>(Context);

        public IGenericRepository<AssessmentRequirement> AssessmentRequirements =>
            _assessmentRequirements ??= new GenericRepository<AssessmentRequirement>(Context);

        public IGenericRepository<FrameworkControl> FrameworkControls =>
            _frameworkControls ??= new GenericRepository<FrameworkControl>(Context);

        public IGenericRepository<TemplateCatalog> TemplateCatalogs =>
            _templateCatalogs ??= new GenericRepository<TemplateCatalog>(Context);

        public IGenericRepository<WorkflowDefinition> WorkflowDefinitions =>
            _workflowDefinitions ??= new GenericRepository<WorkflowDefinition>(Context);

        public IGenericRepository<WorkflowInstance> WorkflowInstances =>
            _workflowInstances ??= new GenericRepository<WorkflowInstance>(Context);

        public IGenericRepository<WorkflowTask> WorkflowTasks =>
            _workflowTasks ??= new GenericRepository<WorkflowTask>(Context);

        public IGenericRepository<ApprovalChain> ApprovalChains =>
            _approvalChains ??= new GenericRepository<ApprovalChain>(Context);

        public IGenericRepository<ApprovalInstance> ApprovalInstances =>
            _approvalInstances ??= new GenericRepository<ApprovalInstance>(Context);

        public IGenericRepository<EscalationRule> EscalationRules =>
            _escalationRules ??= new GenericRepository<EscalationRule>(Context);

        public IGenericRepository<WorkflowAuditEntry> WorkflowAuditEntries =>
            _workflowAuditEntries ??= new GenericRepository<WorkflowAuditEntry>(Context);

        public IGenericRepository<UserConsent> UserConsents =>
            _userConsents ??= new GenericRepository<UserConsent>(Context);

        public IGenericRepository<LegalDocument> LegalDocuments =>
            _legalDocuments ??= new GenericRepository<LegalDocument>(Context);

        public IGenericRepository<SupportConversation> SupportConversations =>
            _supportConversations ??= new GenericRepository<SupportConversation>(Context);

        public IGenericRepository<SupportMessage> SupportMessages =>
            _supportMessages ??= new GenericRepository<SupportMessage>(Context);

        public IGenericRepository<UserWorkspace> UserWorkspaces =>
            _userWorkspaces ??= new GenericRepository<UserWorkspace>(Context);

        public IGenericRepository<UserWorkspaceTask> UserWorkspaceTasks =>
            _userWorkspaceTasks ??= new GenericRepository<UserWorkspaceTask>(Context);

        public IGenericRepository<WorkspaceTemplate> WorkspaceTemplates =>
            _workspaceTemplates ??= new GenericRepository<WorkspaceTemplate>(Context);

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await Context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public bool HasActiveTransaction => _transaction != null;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context?.Dispose();
                _disposed = true;
            }
        }
    }
}
