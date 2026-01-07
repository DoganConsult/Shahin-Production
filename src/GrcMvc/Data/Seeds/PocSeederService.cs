using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds
{
    /// <summary>
    /// POC Seeder Service - Seeds complete Shahin-AI organization data
    /// Call via API: POST /api/seed/poc-organization
    /// </summary>
    public interface IPocSeederService
    {
        Task<PocSeedResult> SeedPocOrganizationAsync(bool force = false);
        Task<bool> IsPocSeededAsync();
    }

    public class PocSeederService : IPocSeederService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<PocSeederService> _logger;

        public PocSeederService(GrcDbContext context, ILogger<PocSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsPocSeededAsync()
        {
            return await _context.Tenants
                .AnyAsync(t => t.Id == PocOrganizationSeeds.TenantId);
        }

        public async Task<PocSeedResult> SeedPocOrganizationAsync(bool force = false)
        {
            var result = new PocSeedResult();

            try
            {
                // Check if already seeded
                if (await IsPocSeededAsync() && !force)
                {
                    result.Success = false;
                    result.Message = "POC organization already seeded. Use force=true to reseed.";
                    return result;
                }

                // If force, remove existing POC data
                if (force)
                {
                    await RemovePocDataAsync();
                    result.Removed = true;
                }

                _logger.LogInformation("Starting POC organization seeding for Shahin-AI...");

                // Section 1: Tenant
                var tenant = PocOrganizationSeeds.GetPocTenant();
                _context.Tenants.Add(tenant);
                result.SectionsSeeded.Add("1. Tenant");

                // Section 2: Onboarding Wizard
                var wizard = PocOrganizationSeeds.GetPocOnboardingWizard();
                _context.OnboardingWizards.Add(wizard);
                result.SectionsSeeded.Add("2. Onboarding Wizard (12 steps)");

                // Section 3: Organization Profile
                var profile = PocOrganizationSeeds.GetPocOrganizationProfile();
                _context.OrganizationProfiles.Add(profile);
                result.SectionsSeeded.Add("3. Organization Profile");

                // Section 4: Tenant Baselines
                var baselines = PocOrganizationSeeds.GetPocTenantBaselines();
                _context.TenantBaselines.AddRange(baselines);
                result.SectionsSeeded.Add($"4. Tenant Baselines ({baselines.Count})");

                // Section 5: Tenant Packages
                var packages = PocOrganizationSeeds.GetPocTenantPackages();
                _context.TenantPackages.AddRange(packages);
                result.SectionsSeeded.Add($"5. Tenant Packages ({packages.Count})");

                // Section 6: Plan
                var plan = PocOrganizationSeeds.GetPocPlan();
                _context.Plans.Add(plan);
                result.SectionsSeeded.Add("6. GRC Plan");

                // Section 7: Plan Phases
                var phases = PocOrganizationSeeds.GetPocPlanPhases();
                _context.PlanPhases.AddRange(phases);
                result.SectionsSeeded.Add($"7. Plan Phases ({phases.Count})");

                // Section 8: Assessments
                var assessments = PocOrganizationSeeds.GetPocAssessments();
                _context.Assessments.AddRange(assessments);
                result.SectionsSeeded.Add($"8. Assessments ({assessments.Count})");

                // Section 9: Team
                var team = PocOrganizationSeeds.GetPocTeam();
                _context.Teams.Add(team);
                result.SectionsSeeded.Add("9. Team");

                // Section 10: Team Members - Skipped (requires TenantUsers to exist first)
                // var members = PocOrganizationSeeds.GetPocTeamMembers();
                // _context.TeamMembers.AddRange(members);
                result.SectionsSeeded.Add("10. Team Members (skipped - FK constraint)");

                // Section 11: RACI Assignments
                var raci = PocOrganizationSeeds.GetPocRaciAssignments();
                _context.RACIAssignments.AddRange(raci);
                result.SectionsSeeded.Add($"11. RACI Assignments ({raci.Count})");

                // Section 12: Evidence
                var evidence = PocOrganizationSeeds.GetPocEvidence();
                _context.Evidences.AddRange(evidence);
                result.SectionsSeeded.Add($"12. Evidence ({evidence.Count})");

                // Section 13: Workflow Instance
                var workflow = PocOrganizationSeeds.GetPocWorkflowInstance();
                _context.WorkflowInstances.Add(workflow);
                result.SectionsSeeded.Add("13. Workflow Instance");

                // Section 14: Audit Events
                var auditEvents = PocOrganizationSeeds.GetPocAuditEvents();
                _context.AuditEvents.AddRange(auditEvents);
                result.SectionsSeeded.Add($"14. Audit Events ({auditEvents.Count})");

                // Section 15: Policy Decisions
                var policyDecisions = PocOrganizationSeeds.GetPocPolicyDecisions();
                _context.PolicyDecisions.AddRange(policyDecisions);
                result.SectionsSeeded.Add($"15. Policy Decisions ({policyDecisions.Count})");

                // Save all changes
                await _context.SaveChangesAsync();

                result.Success = true;
                result.TenantId = PocOrganizationSeeds.TenantId;
                result.TenantSlug = "shahin-ai";
                result.Message = $"POC organization seeded successfully with {result.SectionsSeeded.Count} sections";

                _logger.LogInformation("POC organization seeding completed: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding POC organization");
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
                result.Error = ex.ToString();
            }

            return result;
        }

        private async Task RemovePocDataAsync()
        {
            var tenantId = PocOrganizationSeeds.TenantId;

            // Remove in reverse dependency order
            await _context.PolicyDecisions.Where(p => p.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.AuditEvents.Where(e => e.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.WorkflowInstances.Where(w => w.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.Evidences.Where(e => e.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.RACIAssignments.Where(r => r.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.TeamMembers.Where(m => m.Team.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.Teams.Where(t => t.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.Assessments.Where(a => a.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.PlanPhases.Where(p => p.Plan.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.Plans.Where(p => p.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.TenantPackages.Where(p => p.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.TenantBaselines.Where(b => b.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.OrganizationProfiles.Where(o => o.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.OnboardingWizards.Where(w => w.TenantId == tenantId).ExecuteDeleteAsync();
            await _context.Tenants.Where(t => t.Id == tenantId).ExecuteDeleteAsync();

            _logger.LogInformation("Removed existing POC data for tenant {TenantId}", tenantId);
        }
    }

    public class PocSeedResult
    {
        public bool Success { get; set; }
        public bool Removed { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; } = string.Empty;
        public System.Collections.Generic.List<string> SectionsSeeded { get; set; } = new();
    }
}
