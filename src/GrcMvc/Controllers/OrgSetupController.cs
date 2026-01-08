using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Organization Setup Controller
    /// Manages post-onboarding organization configuration
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class OrgSetupController : Controller
    {
        private readonly GrcDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOnboardingProvisioningService _provisioningService;
        private readonly ILogger<OrgSetupController> _logger;

        public OrgSetupController(
            GrcDbContext context,
            ICurrentUserService currentUserService,
            IOnboardingProvisioningService provisioningService,
            ILogger<OrgSetupController> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _provisioningService = provisioningService;
            _logger = logger;
        }

        /// <summary>
        /// Organization Setup Dashboard - shows setup progress and quick actions
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var tenantId = _currentUserService.GetTenantId();

            var model = new OrgSetupDashboardDto
            {
                TenantId = tenantId,
                Organization = await GetOrganizationSummaryAsync(tenantId),
                Teams = await GetTeamsSummaryAsync(tenantId),
                Users = await GetUsersSummaryAsync(tenantId),
                SetupProgress = await CalculateSetupProgressAsync(tenantId)
            };

            return View(model);
        }

        /// <summary>
        /// Teams Management Page
        /// </summary>
        [HttpGet("Teams")]
        public async Task<IActionResult> Teams()
        {
            var tenantId = _currentUserService.GetTenantId();

            var teams = await _context.Teams
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .Include(t => t.Members.Where(m => !m.IsDeleted))
                .OrderBy(t => t.Name)
                .ToListAsync();

            var model = new TeamsManagementDto
            {
                TenantId = tenantId,
                Teams = teams.Select(t => new TeamDto
                {
                    Id = t.Id,
                    TeamCode = t.TeamCode,
                    Name = t.Name,
                    NameAr = t.NameAr,
                    TeamType = t.TeamType,
                    Purpose = t.Purpose,
                    IsDefaultFallback = t.IsDefaultFallback,
                    IsActive = t.IsActive,
                    MemberCount = t.Members.Count(m => m.IsActive)
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Create Team (GET)
        /// </summary>
        [HttpGet("Teams/Create")]
        public IActionResult CreateTeam()
        {
            return View(new CreateTeamDto());
        }

        /// <summary>
        /// Create Team (POST)
        /// </summary>
        [HttpPost("Teams/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var tenantId = _currentUserService.GetTenantId();
            var userId = _currentUserService.GetUserName();

            var team = new Team
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TeamCode = dto.TeamCode,
                Name = dto.Name,
                NameAr = dto.NameAr,
                Purpose = dto.Purpose,
                Description = dto.Description,
                TeamType = dto.TeamType,
                BusinessUnit = dto.BusinessUnit,
                IsDefaultFallback = dto.IsDefaultFallback,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Team '{team.Name}' created successfully.";
            return RedirectToAction(nameof(Teams));
        }

        /// <summary>
        /// Edit Team (GET)
        /// </summary>
        [HttpGet("Teams/Edit/{id:guid}")]
        public async Task<IActionResult> EditTeam(Guid id)
        {
            var tenantId = _currentUserService.GetTenantId();
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && !t.IsDeleted);

            if (team == null)
                return NotFound();

            var dto = new CreateTeamDto
            {
                Id = team.Id,
                TeamCode = team.TeamCode,
                Name = team.Name,
                NameAr = team.NameAr,
                Purpose = team.Purpose,
                Description = team.Description,
                TeamType = team.TeamType,
                BusinessUnit = team.BusinessUnit,
                IsDefaultFallback = team.IsDefaultFallback
            };

            return View(dto);
        }

        /// <summary>
        /// Edit Team (POST)
        /// </summary>
        [HttpPost("Teams/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeam(Guid id, CreateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var tenantId = _currentUserService.GetTenantId();
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && !t.IsDeleted);

            if (team == null)
                return NotFound();

            team.TeamCode = dto.TeamCode;
            team.Name = dto.Name;
            team.NameAr = dto.NameAr;
            team.Purpose = dto.Purpose;
            team.Description = dto.Description;
            team.TeamType = dto.TeamType;
            team.BusinessUnit = dto.BusinessUnit;
            team.IsDefaultFallback = dto.IsDefaultFallback;
            team.ModifiedDate = DateTime.UtcNow;
            team.ModifiedBy = _currentUserService.GetUserName();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Team '{team.Name}' updated successfully.";
            return RedirectToAction(nameof(Teams));
        }

        /// <summary>
        /// Team Members Management
        /// </summary>
        [HttpGet("Teams/{teamId:guid}/Members")]
        public async Task<IActionResult> TeamMembers(Guid teamId)
        {
            var tenantId = _currentUserService.GetTenantId();

            var team = await _context.Teams
                .Include(t => t.Members.Where(m => !m.IsDeleted))
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.Id == teamId && t.TenantId == tenantId && !t.IsDeleted);

            if (team == null)
                return NotFound();

            var availableUsers = await _context.TenantUsers
                .Include(u => u.User)
                .Where(u => u.TenantId == tenantId && u.Status == "Active" && !u.IsDeleted)
                .ToListAsync();

            var model = new TeamMembersManagementDto
            {
                TeamId = teamId,
                TeamName = team.Name,
                Members = team.Members.Select(m => new OrgTeamMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = m.User?.User?.UserName ?? "Unknown",
                    Email = m.User?.User?.Email ?? "",
                    RoleCode = m.RoleCode,
                    IsPrimaryForRole = m.IsPrimaryForRole,
                    CanApprove = m.CanApprove,
                    CanDelegate = m.CanDelegate,
                    IsActive = m.IsActive,
                    JoinedDate = m.JoinedDate
                }).ToList(),
                AvailableUsers = availableUsers.Select(u => new UserOptionDto
                {
                    Id = u.Id,
                    Email = u.User?.Email ?? "",
                    Name = $"{u.User?.FirstName} {u.User?.LastName}".Trim()
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Add Team Member (POST)
        /// </summary>
        [HttpPost("Teams/{teamId:guid}/Members/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeamMember(Guid teamId, AddTeamMemberDto dto)
        {
            var tenantId = _currentUserService.GetTenantId();

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId && t.TenantId == tenantId && !t.IsDeleted);

            if (team == null)
                return NotFound();

            // Check if user is already a member
            var existingMember = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == dto.UserId && !m.IsDeleted);

            if (existingMember != null)
            {
                TempData["ErrorMessage"] = "User is already a member of this team.";
                return RedirectToAction(nameof(TeamMembers), new { teamId });
            }

            var member = new TeamMember
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TeamId = teamId,
                UserId = dto.UserId,
                RoleCode = dto.RoleCode,
                IsPrimaryForRole = dto.IsPrimaryForRole,
                CanApprove = dto.CanApprove,
                CanDelegate = dto.CanDelegate,
                IsActive = true,
                JoinedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _currentUserService.GetUserName()
            };

            _context.TeamMembers.Add(member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Team member added successfully.";
            return RedirectToAction(nameof(TeamMembers), new { teamId });
        }

        /// <summary>
        /// Remove Team Member
        /// </summary>
        [HttpPost("Teams/{teamId:guid}/Members/{memberId:guid}/Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTeamMember(Guid teamId, Guid memberId)
        {
            var tenantId = _currentUserService.GetTenantId();

            var member = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.Id == memberId && m.TeamId == teamId && !m.IsDeleted);

            if (member == null)
                return NotFound();

            member.IsDeleted = true;
            member.IsActive = false;
            member.LeftDate = DateTime.UtcNow;
            member.ModifiedDate = DateTime.UtcNow;
            member.ModifiedBy = _currentUserService.GetUserName();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Team member removed successfully.";
            return RedirectToAction(nameof(TeamMembers), new { teamId });
        }

        /// <summary>
        /// Users Management Page
        /// </summary>
        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var tenantId = _currentUserService.GetTenantId();

            var users = await _context.TenantUsers
                .Include(u => u.User)
                .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .OrderBy(u => u.User.Email)
                .ToListAsync();

            var model = new UsersManagementDto
            {
                TenantId = tenantId,
                Users = users.Select(u => new TenantUserDto
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    Email = u.User?.Email ?? "",
                    FirstName = u.User?.FirstName ?? "",
                    LastName = u.User?.LastName ?? "",
                    RoleCode = u.RoleCode,
                    TitleCode = u.TitleCode,
                    Department = u.User?.Department ?? "",
                    Status = u.Status,
                    IsActive = u.Status == "Active"
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// RACI Assignments Page
        /// </summary>
        [HttpGet("RACI")]
        public async Task<IActionResult> RACI()
        {
            var tenantId = _currentUserService.GetTenantId();

            var assignments = await _context.RACIAssignments
                .Include(r => r.Team)
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .OrderBy(r => r.ScopeType)
                .ThenBy(r => r.ScopeId)
                .ToListAsync();

            var teams = await _context.Teams
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .ToListAsync();

            var model = new RACIManagementDto
            {
                TenantId = tenantId,
                Assignments = assignments.Select(a => new RACIAssignmentDto
                {
                    Id = a.Id,
                    ScopeType = a.ScopeType,
                    ScopeId = a.ScopeId,
                    TeamId = a.TeamId,
                    TeamName = a.Team?.Name ?? "",
                    RACI = a.RACI,
                    RoleCode = a.RoleCode,
                    Priority = a.Priority,
                    IsActive = a.IsActive
                }).ToList(),
                AvailableTeams = teams.Select(t => new TeamOptionDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Provision default teams and RACI (if not already done)
        /// </summary>
        [HttpPost("ProvisionDefaults")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProvisionDefaults()
        {
            var tenantId = _currentUserService.GetTenantId();
            var userId = _currentUserService.GetUserName();

            var result = await _provisioningService.ProvisionAllAsync(tenantId, userId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Provisioned {result.TeamsCreated} teams and {result.RACIAssignmentsCreated} RACI assignments.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Provisioning failed: {string.Join("; ", result.Errors)}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<OrganizationSummaryDto> GetOrganizationSummaryAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            var profile = await _context.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);

            return new OrganizationSummaryDto
            {
                TenantId = tenantId,
                OrganizationName = tenant?.OrganizationName ?? "Unknown",
                LegalEntityName = profile?.LegalEntityName ?? "",
                Sector = profile?.Sector ?? "",
                Country = profile?.Country ?? "SA",
                OnboardingStatus = profile?.OnboardingStatus ?? "NotStarted",
                OnboardingProgress = profile?.OnboardingProgressPercent ?? 0
            };
        }

        private async Task<TeamsSummaryDto> GetTeamsSummaryAsync(Guid tenantId)
        {
            var teams = await _context.Teams
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .ToListAsync();

            var memberCount = await _context.TeamMembers
                .Where(m => m.TenantId == tenantId && !m.IsDeleted && m.IsActive)
                .CountAsync();

            return new TeamsSummaryDto
            {
                TotalTeams = teams.Count,
                ActiveTeams = teams.Count(t => t.IsActive),
                TotalMembers = memberCount,
                HasDefaultTeam = teams.Any(t => t.IsDefaultFallback)
            };
        }

        private async Task<UsersSummaryDto> GetUsersSummaryAsync(Guid tenantId)
        {
            var users = await _context.TenantUsers
                .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .ToListAsync();

            return new UsersSummaryDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.Status == "Active"),
                PendingInvitations = users.Count(u => u.Status == "Pending")
            };
        }

        private async Task<SetupProgressDto> CalculateSetupProgressAsync(Guid tenantId)
        {
            var progress = new SetupProgressDto();

            // Check organization profile
            var profile = await _context.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            progress.OrganizationProfileComplete = profile != null && !string.IsNullOrEmpty(profile.LegalEntityName);

            // Check teams
            var hasTeams = await _context.Teams.AnyAsync(t => t.TenantId == tenantId && !t.IsDeleted);
            progress.TeamsConfigured = hasTeams;

            // Check RACI
            var hasRaci = await _context.RACIAssignments.AnyAsync(r => r.TenantId == tenantId && !r.IsDeleted);
            progress.RACIConfigured = hasRaci;

            // Check users
            var hasActiveUsers = await _context.TenantUsers.AnyAsync(u => u.TenantId == tenantId && u.Status == "Active" && !u.IsDeleted);
            progress.UsersInvited = hasActiveUsers;

            // Calculate overall percentage
            int completed = 0;
            if (progress.OrganizationProfileComplete) completed++;
            if (progress.TeamsConfigured) completed++;
            if (progress.RACIConfigured) completed++;
            if (progress.UsersInvited) completed++;
            progress.OverallPercent = (completed * 100) / 4;

            return progress;
        }
    }
}
