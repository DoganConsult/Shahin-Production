using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AUTHORITATIVE Tenant Onboarding Provisioner
    /// Creates ONE workspace per tenant (idempotent), assessment template, GRC plan
    /// </summary>
    public class TenantOnboardingProvisioner : ITenantOnboardingProvisioner
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<TenantOnboardingProvisioner> _logger;

        public TenantOnboardingProvisioner(
            GrcDbContext context,
            ILogger<TenantOnboardingProvisioner> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// IDEMPOTENT: Ensure exactly ONE default workspace exists per tenant
        /// </summary>
        public async Task<Guid> EnsureDefaultWorkspaceAsync(Guid tenantId, string workspaceName, string createdBy)
        {
            // Check if tenant already has a default workspace
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            // IDEMPOTENCY: If DefaultWorkspaceId already set, return it
            if (tenant.DefaultWorkspaceId.HasValue)
            {
                var existingWorkspace = await _context.Workspaces
                    .FirstOrDefaultAsync(w => w.Id == tenant.DefaultWorkspaceId.Value && !w.IsDeleted);

                if (existingWorkspace != null)
                {
                    _logger.LogInformation("Tenant {TenantId} already has default workspace {WorkspaceId}",
                        tenantId, existingWorkspace.Id);
                    return existingWorkspace.Id;
                }
            }

            // Check for any existing default workspace (fallback)
            var defaultWorkspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.IsDefault && !w.IsDeleted);

            if (defaultWorkspace != null)
            {
                // Link tenant to existing workspace
                tenant.DefaultWorkspaceId = defaultWorkspace.Id;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Linked tenant {TenantId} to existing workspace {WorkspaceId}",
                    tenantId, defaultWorkspace.Id);
                return defaultWorkspace.Id;
            }

            // CREATE ONE DEFAULT WORKSPACE (only reaches here if none exists)
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkspaceCode = "DEFAULT",
                Name = workspaceName,
                NameAr = workspaceName,
                WorkspaceType = "Market",
                JurisdictionCode = "SA",
                DefaultLanguage = "ar",
                IsDefault = true,
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Workspaces.Add(workspace);

            // Update tenant with workspace reference
            tenant.DefaultWorkspaceId = workspace.Id;
            tenant.OnboardingStatus = "IN_PROGRESS";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created default workspace {WorkspaceId} for tenant {TenantId}",
                workspace.Id, tenantId);

            return workspace.Id;
        }

        /// <summary>
        /// Create 100-question baseline assessment template (stored as TenantTemplate)
        /// </summary>
        public async Task<Guid> CreateAssessmentTemplateAsync(Guid tenantId, string baselineCode, string createdBy)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            // IDEMPOTENCY: If AssessmentTemplateId already set, return it
            if (tenant.AssessmentTemplateId.HasValue)
            {
                _logger.LogInformation("Tenant {TenantId} already has assessment template {TemplateId}",
                    tenantId, tenant.AssessmentTemplateId.Value);
                return tenant.AssessmentTemplateId.Value;
            }

            // Create 100Q baseline template as TenantTemplate
            var templateCode = $"BASE_{baselineCode}_{DateTime.UtcNow:yyyyMMdd}";
            var template = new TenantTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TemplateCode = templateCode,
                TemplateName = $"{baselineCode} Baseline Assessment (100Q)",
                Applicability = "MANDATORY",
                DerivedAt = DateTime.UtcNow,
                ReasonJson = JsonSerializer.Serialize(new[] { new { ruleCode = "ONBOARDING", reason = "Auto-generated baseline from onboarding wizard" } }),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.TenantTemplates.Add(template);

            // Update tenant reference
            tenant.AssessmentTemplateId = template.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created assessment template {TemplateCode} for tenant {TenantId}",
                templateCode, tenantId);

            return template.Id;
        }

        /// <summary>
        /// Create initial GRC plan
        /// </summary>
        public async Task<Guid> CreateGrcPlanAsync(Guid tenantId, string planName, string createdBy)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                throw new EntityNotFoundException("Tenant", tenantId);

            // IDEMPOTENCY: If GrcPlanId already set, return it
            if (tenant.GrcPlanId.HasValue)
            {
                _logger.LogInformation("Tenant {TenantId} already has GRC plan {PlanId}",
                    tenantId, tenant.GrcPlanId.Value);
                return tenant.GrcPlanId.Value;
            }

            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PlanCode = $"PLAN-{DateTime.UtcNow:yyyyMMdd}-001",
                Name = planName,
                Description = "Auto-generated initial compliance plan from onboarding",
                PlanType = "Full",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                TargetEndDate = DateTime.UtcNow.AddDays(90),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Plans.Add(plan);

            // Update tenant reference
            tenant.GrcPlanId = plan.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created GRC plan {PlanId} for tenant {TenantId}",
                plan.Id, tenantId);

            return plan.Id;
        }

        /// <summary>
        /// Create initial assessments from template
        /// </summary>
        public async Task<int> CreateInitialAssessmentsAsync(Guid tenantId, Guid templateId, Guid planId, string createdBy)
        {
            // Get template code
            var template = await _context.TenantTemplates.FirstOrDefaultAsync(t => t.Id == templateId);
            var templateCode = template?.TemplateCode ?? "BASE_100Q";

            // Create one initial assessment
            var assessment = new Assessment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TemplateCode = templateCode,
                PlanId = planId,
                AssessmentCode = $"ASSESS-{DateTime.UtcNow:yyyyMMdd}-001",
                Name = "Initial Compliance Assessment",
                Type = "Compliance",
                Status = "Draft",
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created initial assessment {AssessmentId} for tenant {TenantId}",
                assessment.Id, tenantId);

            return 1;
        }

        /// <summary>
        /// Activate default workflows
        /// </summary>
        public async Task ActivateDefaultWorkflowsAsync(Guid tenantId, Guid workspaceId, string createdBy)
        {
            // Check for existing workflow instances
            var hasWorkflows = await _context.WorkflowInstances
                .AnyAsync(w => w.TenantId == tenantId && !w.IsDeleted);

            if (hasWorkflows)
            {
                _logger.LogInformation("Tenant {TenantId} already has workflows", tenantId);
                return;
            }

            // Get active workflow definitions (first 3)
            var defaultDefinitions = await _context.WorkflowDefinitions
                .Where(d => d.IsActive && !d.IsDeleted)
                .Take(3)
                .ToListAsync();

            foreach (var definition in defaultDefinitions)
            {
                var instance = new WorkflowInstance
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowDefinitionId = definition.Id,
                    Status = "Active",
                    StartedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };
                _context.WorkflowInstances.Add(instance);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Activated {Count} default workflows for tenant {TenantId}",
                defaultDefinitions.Count, tenantId);
        }

        /// <summary>
        /// Setup workspace features (role landing configs)
        /// </summary>
        public async Task SetupWorkspaceFeaturesAsync(Guid tenantId, Guid workspaceId, string createdBy)
        {
            // Check if role configs already exist
            var hasRoleConfigs = await _context.Set<RoleLandingConfig>()
                .AnyAsync(r => r.TenantId == tenantId && r.WorkspaceId == workspaceId);

            if (hasRoleConfigs)
            {
                _logger.LogInformation("Tenant {TenantId} already has role landing configs", tenantId);
                return;
            }

            // Create default role landing configs
            var roleConfigs = new[]
            {
                CreateRoleLandingConfig(tenantId, workspaceId, "TENANT_ADMIN", "/admin/dashboard", createdBy),
                CreateRoleLandingConfig(tenantId, workspaceId, "COMPLIANCE_OFFICER", "/compliance/dashboard", createdBy),
                CreateRoleLandingConfig(tenantId, workspaceId, "RISK_MANAGER", "/risk/dashboard", createdBy),
                CreateRoleLandingConfig(tenantId, workspaceId, "CONTROL_OWNER", "/tasks", createdBy),
                CreateRoleLandingConfig(tenantId, workspaceId, "AUDITOR", "/assessments", createdBy),
                CreateRoleLandingConfig(tenantId, workspaceId, "VIEWER", "/reports", createdBy)
            };

            foreach (var config in roleConfigs)
            {
                _context.Add(config);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} role landing configs for tenant {TenantId}",
                roleConfigs.Length, tenantId);
        }

        private RoleLandingConfig CreateRoleLandingConfig(
            Guid tenantId, Guid workspaceId, string roleCode, string landingPage, string createdBy)
        {
            return new RoleLandingConfig
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkspaceId = workspaceId,
                RoleCode = roleCode,
                DefaultLandingPage = landingPage,
                WidgetsJson = GetDefaultWidgetsForRole(roleCode),
                QuickActionsJson = GetDefaultQuickActionsForRole(roleCode),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        private string GetDefaultWidgetsForRole(string roleCode) => roleCode switch
        {
            "TENANT_ADMIN" => JsonSerializer.Serialize(new[] { "user_stats", "compliance_overview", "recent_activity" }),
            "COMPLIANCE_OFFICER" => JsonSerializer.Serialize(new[] { "compliance_score", "pending_tasks", "assessment_progress" }),
            "RISK_MANAGER" => JsonSerializer.Serialize(new[] { "risk_heat_map", "open_risks", "mitigation_status" }),
            "CONTROL_OWNER" => JsonSerializer.Serialize(new[] { "my_tasks", "upcoming_deadlines", "evidence_status" }),
            "AUDITOR" => JsonSerializer.Serialize(new[] { "assessment_queue", "findings", "evidence_review" }),
            _ => JsonSerializer.Serialize(new[] { "dashboard_summary" })
        };

        private string GetDefaultQuickActionsForRole(string roleCode) => roleCode switch
        {
            "TENANT_ADMIN" => JsonSerializer.Serialize(new[] { "invite_user", "view_reports", "manage_settings" }),
            "COMPLIANCE_OFFICER" => JsonSerializer.Serialize(new[] { "start_assessment", "review_evidence", "generate_report" }),
            "RISK_MANAGER" => JsonSerializer.Serialize(new[] { "log_risk", "update_mitigation", "view_heat_map" }),
            "CONTROL_OWNER" => JsonSerializer.Serialize(new[] { "submit_evidence", "complete_task", "request_extension" }),
            "AUDITOR" => JsonSerializer.Serialize(new[] { "review_assessment", "add_finding", "export_audit_pack" }),
            _ => JsonSerializer.Serialize(new[] { "view_dashboard" })
        };

        /// <summary>
        /// Run complete onboarding provisioning
        /// </summary>
        public async Task<OnboardingProvisioningResult> ProvisionTenantAsync(Guid tenantId, string createdBy)
        {
            var result = new OnboardingProvisioningResult { Success = true };

            try
            {
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
                if (tenant == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Tenant {tenantId} not found");
                    return result;
                }

                var workspaceName = tenant.OrganizationName ?? "Default Workspace";

                // 1. Ensure default workspace (idempotent)
                result.WorkspaceId = await EnsureDefaultWorkspaceAsync(tenantId, workspaceName, createdBy);

                // 2. Create assessment template (idempotent)
                result.AssessmentTemplateId = await CreateAssessmentTemplateAsync(tenantId, "100Q", createdBy);

                // 3. Create GRC plan (idempotent)
                result.GrcPlanId = await CreateGrcPlanAsync(tenantId, $"{workspaceName} - Compliance Plan", createdBy);

                // 4. Create initial assessments
                result.AssessmentsCreated = await CreateInitialAssessmentsAsync(
                    tenantId, result.AssessmentTemplateId.Value, result.GrcPlanId.Value, createdBy);

                // 5. Activate workflows
                await ActivateDefaultWorkflowsAsync(tenantId, result.WorkspaceId.Value, createdBy);

                // 6. Setup workspace features
                await SetupWorkspaceFeaturesAsync(tenantId, result.WorkspaceId.Value, createdBy);

                // 7. Auto-provision evidence requirements based on sector
                result.EvidenceRequirementsCreated = await ProvisionEvidenceRequirementsAsync(tenantId, createdBy);

                // Update tenant status
                tenant.OnboardingStatus = "COMPLETED";
                tenant.OnboardingCompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Onboarding provisioning completed for tenant {TenantId} with {EvidenceCount} evidence requirements", 
                    tenantId, result.EvidenceRequirementsCreated);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                _logger.LogError(ex, "Onboarding provisioning failed for tenant {TenantId}", tenantId);
            }

            return result;
        }

        /// <summary>
        /// Auto-provision evidence requirements based on tenant's organization profile sector
        /// </summary>
        private async Task<int> ProvisionEvidenceRequirementsAsync(Guid tenantId, string createdBy)
        {
            try
            {
                // Get organization profile to determine sector
                var profile = await _context.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);
                if (profile == null || string.IsNullOrEmpty(profile.Sector))
                {
                    _logger.LogWarning("No organization profile or sector found for tenant {TenantId}, skipping evidence provisioning", tenantId);
                    return 0;
                }

                // Get default workspace
                var workspace = await _context.Workspaces
                    .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.IsDefault && !w.IsDeleted);

                // Get sector-framework mappings
                var sectorMappings = await _context.SectorFrameworkIndexes
                    .Where(s => s.SectorCode.ToUpper() == profile.Sector.ToUpper() && s.IsActive && s.IsMandatory)
                    .OrderBy(s => s.Priority)
                    .ToListAsync();

                if (sectorMappings.Count == 0)
                {
                    _logger.LogInformation("No sector framework mappings found for sector {Sector}", profile.Sector);
                    return 0;
                }

                // Get existing requirements to avoid duplicates
                var existingKeys = await _context.TenantEvidenceRequirements
                    .Where(r => r.TenantId == tenantId)
                    .Select(r => $"{r.FrameworkCode}|{r.ControlNumber}|{r.EvidenceTypeCode}")
                    .ToListAsync();
                var existingSet = existingKeys.ToHashSet();

                var newRequirements = new List<TenantEvidenceRequirement>();

                foreach (var mapping in sectorMappings)
                {
                    // Get controls for this framework
                    var controls = await _context.FrameworkControls
                        .Where(c => c.FrameworkCode == mapping.FrameworkCode && !string.IsNullOrEmpty(c.EvidenceRequirements))
                        .Select(c => new { c.ControlNumber, c.EvidenceRequirements })
                        .Take(50) // Limit to top 50 controls per framework for initial provisioning
                        .ToListAsync();

                    foreach (var control in controls)
                    {
                        var evidenceTypes = control.EvidenceRequirements.Split('|')
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .Select(e => e.Trim())
                            .Distinct();

                        foreach (var evidenceType in evidenceTypes)
                        {
                            var key = $"{mapping.FrameworkCode}|{control.ControlNumber}|{evidenceType}";
                            if (existingSet.Contains(key))
                                continue;

                            // Get scoring criteria
                            var criteria = await _context.EvidenceScoringCriteria
                                .FirstOrDefaultAsync(c => c.EvidenceTypeName.Contains(evidenceType) && c.IsActive);

                            var requirement = new TenantEvidenceRequirement
                            {
                                Id = Guid.NewGuid(),
                                TenantId = tenantId,
                                EvidenceTypeCode = evidenceType.Replace(" ", "_").ToUpper(),
                                EvidenceTypeName = evidenceType,
                                FrameworkCode = mapping.FrameworkCode,
                                ControlNumber = control.ControlNumber,
                                MinimumScore = criteria?.MinimumScore ?? 70,
                                CollectionFrequency = criteria?.CollectionFrequency ?? "Annual",
                                DefaultValidityDays = criteria?.DefaultValidityDays ?? 365,
                                Status = "NotStarted",
                                DueDate = DateTime.UtcNow.AddDays(90), // Default 90 days
                                WorkspaceId = workspace?.Id,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = createdBy
                            };

                            newRequirements.Add(requirement);
                            existingSet.Add(key);
                        }
                    }
                }

                if (newRequirements.Count > 0)
                {
                    await _context.TenantEvidenceRequirements.AddRangeAsync(newRequirements);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Provisioned {Count} evidence requirements for tenant {TenantId} in sector {Sector}",
                    newRequirements.Count, tenantId, profile.Sector);

                return newRequirements.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning evidence requirements for tenant {TenantId}", tenantId);
                return 0;
            }
        }

        /// <summary>
        /// Check if provisioning is complete
        /// </summary>
        public async Task<bool> IsProvisioningCompleteAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null) return false;

            return tenant.OnboardingStatus == "COMPLETED" &&
                   tenant.DefaultWorkspaceId.HasValue &&
                   tenant.AssessmentTemplateId.HasValue &&
                   tenant.GrcPlanId.HasValue;
        }
    }
}
