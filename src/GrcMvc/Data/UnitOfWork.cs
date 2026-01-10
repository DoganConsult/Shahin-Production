using System;
using System.Threading.Tasks;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Storage;

namespace GrcMvc.Data
{
    /// <summary>
    /// Implementation of Unit of Work pattern
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GrcDbContext _context;
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

        // Teams & RACI (Role-based workflow routing)
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

        // STAGE 2: Workflow infrastructure repositories
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

        // User Workspace (Role-based pre-mapping)
        private IGenericRepository<UserWorkspace>? _userWorkspaces;
        private IGenericRepository<UserWorkspaceTask>? _userWorkspaceTasks;
        private IGenericRepository<WorkspaceTemplate>? _workspaceTemplates;

        public UnitOfWork(GrcDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Repository properties with lazy initialization
        public IGenericRepository<Risk> Risks =>
            _risks ??= new GenericRepository<Risk>(_context);

        public IGenericRepository<RiskControlMapping> RiskControlMappings =>
            _riskControlMappings ??= new GenericRepository<RiskControlMapping>(_context);

        public IGenericRepository<RiskAppetiteSetting> RiskAppetiteSettings =>
            _riskAppetiteSettings ??= new GenericRepository<RiskAppetiteSetting>(_context);

        public IGenericRepository<Control> Controls =>
            _controls ??= new GenericRepository<Control>(_context);

        public IGenericRepository<Assessment> Assessments =>
            _assessments ??= new GenericRepository<Assessment>(_context);

        public IGenericRepository<Audit> Audits =>
            _audits ??= new GenericRepository<Audit>(_context);

        public IGenericRepository<AuditFinding> AuditFindings =>
            _auditFindings ??= new GenericRepository<AuditFinding>(_context);

        public IGenericRepository<Evidence> Evidences =>
            _evidences ??= new GenericRepository<Evidence>(_context);

        public IGenericRepository<Policy> Policies =>
            _policies ??= new GenericRepository<Policy>(_context);

        public IGenericRepository<PolicyViolation> PolicyViolations =>
            _policyViolations ??= new GenericRepository<PolicyViolation>(_context);

        public IGenericRepository<Workflow> Workflows =>
            _workflows ??= new GenericRepository<Workflow>(_context);

        public IGenericRepository<WorkflowExecution> WorkflowExecutions =>
            _workflowExecutions ??= new GenericRepository<WorkflowExecution>(_context);

        public IGenericRepository<ActionPlan> ActionPlans =>
            _actionPlans ??= new GenericRepository<ActionPlan>(_context);

        public IGenericRepository<Vendor> Vendors =>
            _vendors ??= new GenericRepository<Vendor>(_context);

        public IGenericRepository<Regulator> Regulators =>
            _regulators ??= new GenericRepository<Regulator>(_context);

        public IGenericRepository<ComplianceEvent> ComplianceEvents =>
            _complianceEvents ??= new GenericRepository<ComplianceEvent>(_context);

        public IGenericRepository<GrcMvc.Models.Entities.Framework> Frameworks =>
            _frameworks ??= new GenericRepository<GrcMvc.Models.Entities.Framework>(_context);

        // Multi-tenant repository properties
        public IGenericRepository<Tenant> Tenants =>
            _tenants ??= new GenericRepository<Tenant>(_context);

        public IGenericRepository<TenantUser> TenantUsers =>
            _tenantUsers ??= new GenericRepository<TenantUser>(_context);

        public IGenericRepository<OwnerTenantCreation> OwnerTenantCreations =>
            _ownerTenantCreations ??= new GenericRepository<OwnerTenantCreation>(_context);

        public IGenericRepository<OrganizationProfile> OrganizationProfiles =>
            _organizationProfiles ??= new GenericRepository<OrganizationProfile>(_context);

        public IGenericRepository<OnboardingWizard> OnboardingWizards =>
            _onboardingWizards ??= new GenericRepository<OnboardingWizard>(_context);

        // Teams & RACI properties
        public IGenericRepository<Team> Teams =>
            _teams ??= new GenericRepository<Team>(_context);

        public IGenericRepository<TeamMember> TeamMembers =>
            _teamMembers ??= new GenericRepository<TeamMember>(_context);

        public IGenericRepository<RACIAssignment> RACIAssignments =>
            _raciAssignments ??= new GenericRepository<RACIAssignment>(_context);

        // Asset inventory property
        public IGenericRepository<Asset> Assets =>
            _assets ??= new GenericRepository<Asset>(_context);

        public IGenericRepository<Ruleset> Rulesets =>
            _rulesets ??= new GenericRepository<Ruleset>(_context);

        public IGenericRepository<Rule> Rules =>
            _rules ??= new GenericRepository<Rule>(_context);

        public IGenericRepository<RuleExecutionLog> RuleExecutionLogs =>
            _ruleExecutionLogs ??= new GenericRepository<RuleExecutionLog>(_context);

        public IGenericRepository<TenantBaseline> TenantBaselines =>
            _tenantBaselines ??= new GenericRepository<TenantBaseline>(_context);

        public IGenericRepository<TenantPackage> TenantPackages =>
            _tenantPackages ??= new GenericRepository<TenantPackage>(_context);

        public IGenericRepository<TenantTemplate> TenantTemplates =>
            _tenantTemplates ??= new GenericRepository<TenantTemplate>(_context);

        public IGenericRepository<Plan> Plans =>
            _plans ??= new GenericRepository<Plan>(_context);

        public IGenericRepository<PlanPhase> PlanPhases =>
            _planPhases ??= new GenericRepository<PlanPhase>(_context);

        public IGenericRepository<AuditEvent> AuditEvents =>
            _auditEvents ??= new GenericRepository<AuditEvent>(_context);

        public IGenericRepository<Report> Reports =>
            _reports ??= new GenericRepository<Report>(_context);

        public IGenericRepository<AssessmentRequirement> AssessmentRequirements =>
            _assessmentRequirements ??= new GenericRepository<AssessmentRequirement>(_context);

        public IGenericRepository<FrameworkControl> FrameworkControls =>
            _frameworkControls ??= new GenericRepository<FrameworkControl>(_context);

        public IGenericRepository<TemplateCatalog> TemplateCatalogs =>
            _templateCatalogs ??= new GenericRepository<TemplateCatalog>(_context);

        // STAGE 2: Workflow infrastructure properties
        public IGenericRepository<WorkflowDefinition> WorkflowDefinitions =>
            _workflowDefinitions ??= new GenericRepository<WorkflowDefinition>(_context);

        public IGenericRepository<WorkflowInstance> WorkflowInstances =>
            _workflowInstances ??= new GenericRepository<WorkflowInstance>(_context);

        public IGenericRepository<WorkflowTask> WorkflowTasks =>
            _workflowTasks ??= new GenericRepository<WorkflowTask>(_context);

        public IGenericRepository<ApprovalChain> ApprovalChains =>
            _approvalChains ??= new GenericRepository<ApprovalChain>(_context);

        public IGenericRepository<ApprovalInstance> ApprovalInstances =>
            _approvalInstances ??= new GenericRepository<ApprovalInstance>(_context);

        public IGenericRepository<EscalationRule> EscalationRules =>
            _escalationRules ??= new GenericRepository<EscalationRule>(_context);

        public IGenericRepository<WorkflowAuditEntry> WorkflowAuditEntries =>
            _workflowAuditEntries ??= new GenericRepository<WorkflowAuditEntry>(_context);

        // User Consent & Legal Documents properties
        public IGenericRepository<UserConsent> UserConsents =>
            _userConsents ??= new GenericRepository<UserConsent>(_context);

        public IGenericRepository<LegalDocument> LegalDocuments =>
            _legalDocuments ??= new GenericRepository<LegalDocument>(_context);

        // Support Agent & Chat properties
        public IGenericRepository<SupportConversation> SupportConversations =>
            _supportConversations ??= new GenericRepository<SupportConversation>(_context);

        public IGenericRepository<SupportMessage> SupportMessages =>
            _supportMessages ??= new GenericRepository<SupportMessage>(_context);

        // User Workspace properties
        public IGenericRepository<UserWorkspace> UserWorkspaces =>
            _userWorkspaces ??= new GenericRepository<UserWorkspace>(_context);

        public IGenericRepository<UserWorkspaceTask> UserWorkspaceTasks =>
            _userWorkspaceTasks ??= new GenericRepository<UserWorkspaceTask>(_context);

        public IGenericRepository<WorkspaceTemplate> WorkspaceTemplates =>
            _workspaceTemplates ??= new GenericRepository<WorkspaceTemplate>(_context);

        public bool HasActiveTransaction => _transaction != null;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress to commit.");
            }

            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress to rollback.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}