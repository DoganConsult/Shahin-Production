using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Diagnostic API for troubleshooting Team → Workflow integration issues
    /// Helps identify why workflows are not routing to the correct team/users
    /// </summary>
    [ApiController]
    [Route("api/diagnostics/team-workflow")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public class TeamWorkflowDiagnosticsController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly IWorkflowRoutingService? _routingService;
        private readonly ITenantContextService _tenantContext;

        public TeamWorkflowDiagnosticsController(
            GrcDbContext context,
            ITenantContextService tenantContext,
            IWorkflowRoutingService? routingService = null)
        {
            _context = context;
            _tenantContext = tenantContext;
            _routingService = routingService;
        }

        /// <summary>
        /// Full diagnostic check for a tenant's team-workflow configuration
        /// GET /api/diagnostics/team-workflow/check
        /// </summary>
        [HttpGet("check")]
        public async Task<IActionResult> RunFullDiagnostics()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "No tenant context" });

            var result = new DiagnosticResult
            {
                TenantId = tenantId,
                CheckedAt = DateTime.UtcNow,
                Checks = new List<DiagnosticCheck>()
            };

            // Check 1: Teams exist
            var teams = await _context.Teams
                .Where(t => t.TenantId == tenantId && !t.IsDeleted && t.IsActive)
                .ToListAsync();

            result.Checks.Add(new DiagnosticCheck
            {
                Name = "Teams Exist",
                Status = teams.Any() ? "PASS" : "FAIL",
                Details = $"Found {teams.Count} active teams",
                Recommendation = teams.Any() ? null : "Create at least one team in OrgSetup"
            });

            // Check 2: Fallback team configured
            var fallbackTeam = teams.FirstOrDefault(t => t.IsDefaultFallback);
            result.Checks.Add(new DiagnosticCheck
            {
                Name = "Fallback Team Configured",
                Status = fallbackTeam != null ? "PASS" : "WARN",
                Details = fallbackTeam != null ? $"Fallback: {fallbackTeam.Name}" : "No fallback team set",
                Recommendation = fallbackTeam != null ? null : "Set IsDefaultFallback=true on one team"
            });

            // Check 3: Team members with roles
            var teamMembers = await _context.Set<TeamMember>()
                .Where(tm => tm.TenantId == tenantId && !tm.IsDeleted && tm.IsActive)
                .Include(tm => tm.Team)
                .ToListAsync();

            var teamsWithMembers = teamMembers.Select(tm => tm.TeamId).Distinct().Count();
            result.Checks.Add(new DiagnosticCheck
            {
                Name = "Teams Have Members",
                Status = teamsWithMembers == teams.Count ? "PASS" : (teamsWithMembers > 0 ? "WARN" : "FAIL"),
                Details = $"{teamsWithMembers}/{teams.Count} teams have members ({teamMembers.Count} total members)",
                Recommendation = teamsWithMembers < teams.Count ? "Add members to all teams" : null
            });

            // Check 4: RoleCodes assigned
            var roleCodes = teamMembers.Select(tm => tm.RoleCode).Distinct().ToList();
            result.Checks.Add(new DiagnosticCheck
            {
                Name = "RoleCodes Assigned",
                Status = roleCodes.Any() ? "PASS" : "FAIL",
                Details = roleCodes.Any() ? $"RoleCodes: {string.Join(", ", roleCodes)}" : "No RoleCodes assigned",
                Recommendation = roleCodes.Any() ? null : "Assign RoleCodes to team members (e.g., APPROVER, CONTROL_OWNER)"
            });

            // Check 5: RACI assignments
            var raciAssignments = await _context.Set<RACIAssignment>()
                .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.IsActive)
                .ToListAsync();

            result.Checks.Add(new DiagnosticCheck
            {
                Name = "RACI Assignments Configured",
                Status = raciAssignments.Any() ? "PASS" : "WARN",
                Details = $"Found {raciAssignments.Count} RACI assignments",
                Recommendation = raciAssignments.Any() ? null : "Configure RACI mappings for scopes (ControlFamily, Framework, etc.)"
            });

            // Check 6: Workspace alignment
            var workspaces = await _context.Workspaces
                .Where(w => w.TenantId == tenantId && !w.IsDeleted)
                .ToListAsync();

            var teamsPerWorkspace = teams.GroupBy(t => t.WorkspaceId).ToList();
            var workspacesWithTeams = teamsPerWorkspace.Where(g => g.Key != null).Select(g => g.Key).Distinct().Count();
            var sharedTeams = teams.Count(t => t.WorkspaceId == null || t.IsSharedTeam);

            result.Checks.Add(new DiagnosticCheck
            {
                Name = "Workspace-Team Alignment",
                Status = workspacesWithTeams > 0 || sharedTeams > 0 ? "PASS" : "WARN",
                Details = $"{workspaces.Count} workspaces, {workspacesWithTeams} have teams, {sharedTeams} shared teams",
                Recommendation = workspacesWithTeams == 0 && sharedTeams == 0 ? "Assign teams to workspaces or mark as shared" : null
            });

            // Check 7: Workflow definitions reference roles
            var workflowDefs = await _context.WorkflowDefinitions
                .Where(w => w.TenantId == tenantId || w.TenantId == null)
                .Where(w => !w.IsDeleted)
                .ToListAsync();

            result.Checks.Add(new DiagnosticCheck
            {
                Name = "Workflow Definitions Available",
                Status = workflowDefs.Any() ? "PASS" : "WARN",
                Details = $"Found {workflowDefs.Count} workflow definitions",
                Recommendation = workflowDefs.Any() ? null : "Seed workflow definitions or create custom workflows"
            });

            // Summary
            var failCount = result.Checks.Count(c => c.Status == "FAIL");
            var warnCount = result.Checks.Count(c => c.Status == "WARN");
            result.Summary = failCount == 0 && warnCount == 0
                ? "All checks passed"
                : $"{failCount} failures, {warnCount} warnings";
            result.OverallStatus = failCount > 0 ? "FAIL" : (warnCount > 0 ? "WARN" : "PASS");

            return Ok(result);
        }

        /// <summary>
        /// Check team configuration for a specific workspace
        /// GET /api/diagnostics/team-workflow/workspace/{workspaceId}
        /// </summary>
        [HttpGet("workspace/{workspaceId:guid}")]
        public async Task<IActionResult> CheckWorkspace(Guid workspaceId)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "No tenant context" });

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == workspaceId && w.TenantId == tenantId);

            if (workspace == null)
                return NotFound(new { error = "Workspace not found" });

            // Get teams in this workspace (including shared teams)
            var teams = await _context.Teams
                .Where(t => t.TenantId == tenantId && !t.IsDeleted && t.IsActive)
                .Where(t => t.WorkspaceId == workspaceId || t.WorkspaceId == null || t.IsSharedTeam)
                .Include(t => t.Members.Where(m => !m.IsDeleted && m.IsActive))
                .ToListAsync();

            // Get RACI for this workspace
            var raci = await _context.Set<RACIAssignment>()
                .Where(r => r.TenantId == tenantId && !r.IsDeleted && r.IsActive)
                .Where(r => r.WorkspaceId == workspaceId || r.WorkspaceId == null)
                .Include(r => r.Team)
                .ToListAsync();

            return Ok(new
            {
                workspace = new { workspace.Id, workspace.WorkspaceCode, workspace.Name },
                teams = teams.Select(t => new
                {
                    t.Id,
                    t.TeamCode,
                    t.Name,
                    t.WorkspaceId,
                    t.IsSharedTeam,
                    t.IsDefaultFallback,
                    members = t.Members.Select(m => new
                    {
                        m.UserId,
                        m.RoleCode,
                        m.IsPrimaryForRole,
                        m.CanApprove
                    })
                }),
                raciAssignments = raci.Select(r => new
                {
                    r.ScopeType,
                    r.ScopeId,
                    r.RACI,
                    r.RoleCode,
                    teamName = r.Team?.Name
                }),
                summary = new
                {
                    teamCount = teams.Count,
                    memberCount = teams.Sum(t => t.Members.Count),
                    raciCount = raci.Count,
                    hasFallback = teams.Any(t => t.IsDefaultFallback),
                    roleCodes = teams.SelectMany(t => t.Members).Select(m => m.RoleCode).Distinct().ToList()
                }
            });
        }

        /// <summary>
        /// Test workflow routing resolution for a specific role
        /// GET /api/diagnostics/team-workflow/test-routing?roleCode=APPROVER&scopeType=ControlFamily&scopeId=IAM
        /// </summary>
        [HttpGet("test-routing")]
        public async Task<IActionResult> TestRouting(
            [FromQuery] string roleCode,
            [FromQuery] string? scopeType = null,
            [FromQuery] string? scopeId = null,
            [FromQuery] Guid? teamId = null)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "No tenant context" });

            if (string.IsNullOrEmpty(roleCode))
                return BadRequest(new { error = "roleCode is required" });

            if (_routingService == null)
                return StatusCode(503, new { error = "WorkflowRoutingService not available" });

            var result = await _routingService.ResolveAssigneesAsync(
                tenantId: tenantId,
                roleCode: roleCode,
                scopeType: scopeType,
                scopeId: scopeId,
                recordOwnerTeamId: teamId);

            return Ok(new
            {
                query = new { roleCode, scopeType, scopeId, teamId },
                resolvedAssignees = result.Select(a => new
                {
                    a.UserId,
                    a.UserName,
                    a.Email,
                    a.RoleCode,
                    a.ResolutionSource,
                    a.IsPrimary,
                    a.Priority
                }),
                success = result.Any(),
                message = result.Any()
                    ? $"Resolved {result.Count} assignee(s) via {result.First().ResolutionSource}"
                    : "No assignees found - check RACI, team members, and fallback configuration"
            });
        }

        /// <summary>
        /// List all teams with their members and roles
        /// GET /api/diagnostics/team-workflow/teams
        /// </summary>
        [HttpGet("teams")]
        public async Task<IActionResult> ListTeams()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "No tenant context" });

            var teams = await _context.Teams
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .Include(t => t.Members.Where(m => !m.IsDeleted))
                .Include(t => t.Workspace)
                .OrderBy(t => t.WorkspaceId).ThenBy(t => t.Name)
                .ToListAsync();

            return Ok(teams.Select(t => new
            {
                t.Id,
                t.TeamCode,
                t.Name,
                t.TeamType,
                t.IsActive,
                t.IsDefaultFallback,
                t.IsSharedTeam,
                workspace = t.Workspace != null ? new { t.Workspace.Id, t.Workspace.WorkspaceCode, t.Workspace.Name } : null,
                members = t.Members.Select(m => new
                {
                    m.Id,
                    m.UserId,
                    m.RoleCode,
                    m.IsPrimaryForRole,
                    m.CanApprove,
                    m.CanDelegate,
                    m.IsActive
                }),
                memberCount = t.Members.Count(m => m.IsActive),
                roleCodes = t.Members.Where(m => m.IsActive).Select(m => m.RoleCode).Distinct().ToList()
            }));
        }

        /// <summary>
        /// List all RACI assignments
        /// GET /api/diagnostics/team-workflow/raci
        /// </summary>
        [HttpGet("raci")]
        public async Task<IActionResult> ListRACIAssignments()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "No tenant context" });

            var raci = await _context.Set<RACIAssignment>()
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .Include(r => r.Team)
                .Include(r => r.Workspace)
                .OrderBy(r => r.ScopeType).ThenBy(r => r.ScopeId)
                .ToListAsync();

            return Ok(raci.Select(r => new
            {
                r.Id,
                r.ScopeType,
                r.ScopeId,
                r.RACI,
                r.RoleCode,
                r.Priority,
                r.IsActive,
                team = r.Team != null ? new { r.Team.Id, r.Team.TeamCode, r.Team.Name } : null,
                workspace = r.Workspace != null ? new { r.Workspace.Id, r.Workspace.WorkspaceCode } : null
            }));
        }
    }

    public class DiagnosticResult
    {
        public Guid TenantId { get; set; }
        public DateTime CheckedAt { get; set; }
        public string OverallStatus { get; set; } = "";
        public string Summary { get; set; } = "";
        public List<DiagnosticCheck> Checks { get; set; } = new();
    }

    public class DiagnosticCheck
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = ""; // PASS, WARN, FAIL
        public string Details { get; set; } = "";
        public string? Recommendation { get; set; }
    }
}
