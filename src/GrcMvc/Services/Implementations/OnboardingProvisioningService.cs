using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Onboarding Provisioning Service
    /// Auto-creates default teams, RACI assignments, and role mappings during onboarding
    /// Ensures workflow routing has required entities to function
    /// </summary>
    public interface IOnboardingProvisioningService
    {
        /// <summary>
        /// Provision default GRC teams for a tenant based on profile and wizard data
        /// </summary>
        Task<ProvisioningResult> ProvisionDefaultTeamsAsync(Guid tenantId, string userId);

        /// <summary>
        /// Provision RACI assignments for default control families
        /// </summary>
        Task<ProvisioningResult> ProvisionDefaultRACIAsync(Guid tenantId, string userId);

        /// <summary>
        /// Full provisioning: teams + RACI + role profiles
        /// Called after onboarding wizard completion
        /// </summary>
        Task<ProvisioningResult> ProvisionAllAsync(Guid tenantId, string userId);

        /// <summary>
        /// Check if provisioning is needed (no teams/RACI exist)
        /// </summary>
        Task<bool> IsProvisioningNeededAsync(Guid tenantId);
    }

    public class ProvisioningResult
    {
        public bool Success { get; set; }
        public int TeamsCreated { get; set; }
        public int RACIAssignmentsCreated { get; set; }
        public int UsersAssigned { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class OnboardingProvisioningService : IOnboardingProvisioningService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<OnboardingProvisioningService> _logger;

        // Default team templates for GRC operations
        private static readonly List<TeamTemplate> DefaultTeams = new()
        {
            new TeamTemplate
            {
                TeamCode = "GRC-CORE",
                Name = "GRC Core Team",
                NameAr = "فريق الحوكمة الأساسي",
                Purpose = "Core GRC operations team - manages frameworks, assessments, and compliance",
                TeamType = "Governance",
                IsDefaultFallback = true,
                DefaultRoles = new[] { "CONTROL_OWNER", "ASSESSOR", "APPROVER" }
            },
            new TeamTemplate
            {
                TeamCode = "SEC-OPS",
                Name = "Security Operations",
                NameAr = "عمليات الأمن",
                Purpose = "Security controls, vulnerability management, incident response",
                TeamType = "Operational",
                IsDefaultFallback = false,
                DefaultRoles = new[] { "CONTROL_OWNER", "EVIDENCE_CUSTODIAN" }
            },
            new TeamTemplate
            {
                TeamCode = "IT-OPS",
                Name = "IT Operations",
                NameAr = "عمليات تقنية المعلومات",
                Purpose = "IT infrastructure, change management, backup/DR",
                TeamType = "Operational",
                IsDefaultFallback = false,
                DefaultRoles = new[] { "CONTROL_OWNER", "EVIDENCE_CUSTODIAN" }
            },
            new TeamTemplate
            {
                TeamCode = "RISK-MGT",
                Name = "Risk Management",
                NameAr = "إدارة المخاطر",
                Purpose = "Risk assessment, risk treatment, risk monitoring",
                TeamType = "Governance",
                IsDefaultFallback = false,
                DefaultRoles = new[] { "ASSESSOR", "APPROVER" }
            },
            new TeamTemplate
            {
                TeamCode = "INT-AUDIT",
                Name = "Internal Audit",
                NameAr = "المراجعة الداخلية",
                Purpose = "Independent assurance, audit testing, findings management",
                TeamType = "Governance",
                IsDefaultFallback = false,
                DefaultRoles = new[] { "ASSESSOR", "VIEWER" }
            }
        };

        // Default RACI templates for control families
        private static readonly List<RACITemplate> DefaultRACITemplates = new()
        {
            // Access Control
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "IAM", TeamCode = "SEC-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "IAM", TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },

            // Network Security
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "NETWORK", TeamCode = "SEC-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "NETWORK", TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },

            // Change Management
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "CHANGE", TeamCode = "IT-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "CHANGE", TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },

            // Business Continuity
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "BCM", TeamCode = "IT-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "BCM", TeamCode = "RISK-MGT", RACI = "A", RoleCode = "APPROVER" },

            // Risk Management
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "RISK", TeamCode = "RISK-MGT", RACI = "R", RoleCode = "ASSESSOR" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "RISK", TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },

            // Vendor Management
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "VENDOR", TeamCode = "GRC-CORE", RACI = "R", RoleCode = "CONTROL_OWNER" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "VENDOR", TeamCode = "RISK-MGT", RACI = "C", RoleCode = "ASSESSOR" },

            // Data Protection
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "DATA", TeamCode = "SEC-OPS", RACI = "R", RoleCode = "CONTROL_OWNER" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "DATA", TeamCode = "GRC-CORE", RACI = "A", RoleCode = "APPROVER" },

            // Audit
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "AUDIT", TeamCode = "INT-AUDIT", RACI = "R", RoleCode = "ASSESSOR" },
            new RACITemplate { ScopeType = "ControlFamily", ScopeId = "AUDIT", TeamCode = "GRC-CORE", RACI = "I", RoleCode = "VIEWER" }
        };

        public OnboardingProvisioningService(GrcDbContext context, ILogger<OnboardingProvisioningService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsProvisioningNeededAsync(Guid tenantId)
        {
            var hasTeams = await _context.Teams.AnyAsync(t => t.TenantId == tenantId && !t.IsDeleted);
            var hasRaci = await _context.RACIAssignments.AnyAsync(r => r.TenantId == tenantId && !r.IsDeleted);

            return !hasTeams || !hasRaci;
        }

        public async Task<ProvisioningResult> ProvisionDefaultTeamsAsync(Guid tenantId, string userId)
        {
            var result = new ProvisioningResult { Success = true };

            try
            {
                // Check if teams already exist
                var existingTeams = await _context.Teams
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .Select(t => t.TeamCode)
                    .ToListAsync();

                foreach (var template in DefaultTeams)
                {
                    if (existingTeams.Contains(template.TeamCode))
                    {
                        result.Warnings.Add($"Team {template.TeamCode} already exists, skipping");
                        continue;
                    }

                    var team = new Team
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        TeamCode = template.TeamCode,
                        Name = template.Name,
                        NameAr = template.NameAr,
                        Purpose = template.Purpose,
                        TeamType = template.TeamType,
                        IsDefaultFallback = template.IsDefaultFallback,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.Teams.Add(team);
                    result.TeamsCreated++;

                    _logger.LogInformation("Created team {TeamCode} for tenant {TenantId}", template.TeamCode, tenantId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Provisioned {Count} default teams for tenant {TenantId}",
                    result.TeamsCreated, tenantId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                _logger.LogError(ex, "Error provisioning teams for tenant {TenantId}", tenantId);
            }

            return result;
        }

        public async Task<ProvisioningResult> ProvisionDefaultRACIAsync(Guid tenantId, string userId)
        {
            var result = new ProvisioningResult { Success = true };

            try
            {
                // Get existing teams for mapping
                var teams = await _context.Teams
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .ToDictionaryAsync(t => t.TeamCode, t => t.Id);

                if (!teams.Any())
                {
                    result.Warnings.Add("No teams found. Run ProvisionDefaultTeamsAsync first.");
                    return result;
                }

                // Check existing RACI assignments
                var existingRaci = await _context.RACIAssignments
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .Select(r => $"{r.ScopeType}:{r.ScopeId}:{r.RACI}")
                    .ToListAsync();

                foreach (var template in DefaultRACITemplates)
                {
                    if (!teams.TryGetValue(template.TeamCode, out var teamId))
                    {
                        result.Warnings.Add($"Team {template.TeamCode} not found, skipping RACI for {template.ScopeId}");
                        continue;
                    }

                    var key = $"{template.ScopeType}:{template.ScopeId}:{template.RACI}";
                    if (existingRaci.Contains(key))
                    {
                        continue; // Already exists
                    }

                    var raci = new RACIAssignment
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        ScopeType = template.ScopeType,
                        ScopeId = template.ScopeId,
                        TeamId = teamId,
                        RACI = template.RACI,
                        RoleCode = template.RoleCode,
                        Priority = template.RACI == "R" ? 1 : template.RACI == "A" ? 2 : 5,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.RACIAssignments.Add(raci);
                    result.RACIAssignmentsCreated++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Provisioned {Count} RACI assignments for tenant {TenantId}",
                    result.RACIAssignmentsCreated, tenantId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                _logger.LogError(ex, "Error provisioning RACI for tenant {TenantId}", tenantId);
            }

            return result;
        }

        public async Task<ProvisioningResult> ProvisionAllAsync(Guid tenantId, string userId)
        {
            var result = new ProvisioningResult { Success = true };

            // Step 1: Provision teams
            var teamsResult = await ProvisionDefaultTeamsAsync(tenantId, userId);
            result.TeamsCreated = teamsResult.TeamsCreated;
            result.Errors.AddRange(teamsResult.Errors);
            result.Warnings.AddRange(teamsResult.Warnings);

            if (!teamsResult.Success)
            {
                result.Success = false;
                return result;
            }

            // Step 2: Provision RACI
            var raciResult = await ProvisionDefaultRACIAsync(tenantId, userId);
            result.RACIAssignmentsCreated = raciResult.RACIAssignmentsCreated;
            result.Errors.AddRange(raciResult.Errors);
            result.Warnings.AddRange(raciResult.Warnings);

            if (!raciResult.Success)
            {
                result.Success = false;
            }

            // Step 3: Assign org admin to GRC-CORE team if available
            await AssignOrgAdminToFallbackTeamAsync(tenantId, userId, result);

            _logger.LogInformation("Full provisioning completed for tenant {TenantId}: {Teams} teams, {RACI} RACI assignments",
                tenantId, result.TeamsCreated, result.RACIAssignmentsCreated);

            return result;
        }

        private async Task AssignOrgAdminToFallbackTeamAsync(Guid tenantId, string userId, ProvisioningResult result)
        {
            try
            {
                // Find fallback team
                var fallbackTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsDefaultFallback && !t.IsDeleted);

                if (fallbackTeam == null) return;

                // Find org admin (current user or first admin)
                var adminUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(u => u.TenantId == tenantId &&
                        (u.RoleCode == "ADMIN" || u.RoleCode == "ORG_ADMIN") &&
                        u.Status == "Active" && !u.IsDeleted);

                if (adminUser == null) return;

                // Check if already member
                var isMember = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == fallbackTeam.Id && tm.UserId == adminUser.Id && !tm.IsDeleted);

                if (isMember) return;

                // Add as team member with APPROVER role
                var teamMember = new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TeamId = fallbackTeam.Id,
                    UserId = adminUser.Id,
                    RoleCode = "APPROVER",
                    IsPrimaryForRole = true,
                    CanApprove = true,
                    CanDelegate = true,
                    IsActive = true,
                    JoinedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.TeamMembers.Add(teamMember);
                await _context.SaveChangesAsync();
                result.UsersAssigned++;

                _logger.LogInformation("Assigned org admin {UserId} to fallback team {TeamId}", adminUser.Id, fallbackTeam.Id);
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Could not assign org admin to fallback team: {ex.Message}");
                _logger.LogWarning(ex, "Error assigning org admin to fallback team");
            }
        }

        // Template classes
        private class TeamTemplate
        {
            public string TeamCode { get; set; } = "";
            public string Name { get; set; } = "";
            public string NameAr { get; set; } = "";
            public string Purpose { get; set; } = "";
            public string TeamType { get; set; } = "Operational";
            public bool IsDefaultFallback { get; set; }
            public string[] DefaultRoles { get; set; } = Array.Empty<string>();
        }

        private class RACITemplate
        {
            public string ScopeType { get; set; } = "";
            public string ScopeId { get; set; } = "";
            public string TeamCode { get; set; } = "";
            public string RACI { get; set; } = "R";
            public string RoleCode { get; set; } = "";
        }
    }
}
