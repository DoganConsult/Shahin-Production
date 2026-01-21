using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.DTOs
{
    // ============================================================================
    // ORGANIZATION SETUP DTOs
    // ============================================================================

    public class OrgSetupDashboardDto
    {
        public Guid TenantId { get; set; }
        public OrganizationSummaryDto Organization { get; set; } = new();
        public TeamsSummaryDto Teams { get; set; } = new();
        public UsersSummaryDto Users { get; set; } = new();
        public SetupProgressDto SetupProgress { get; set; } = new();
    }

    public class OrganizationSummaryDto
    {
        public Guid TenantId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string LegalEntityName { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Country { get; set; } = "SA";
        public string OnboardingStatus { get; set; } = "NotStarted";
        public int OnboardingProgress { get; set; }
    }

    public class TeamsSummaryDto
    {
        public int TotalTeams { get; set; }
        public int ActiveTeams { get; set; }
        public int TotalMembers { get; set; }
        public bool HasDefaultTeam { get; set; }
    }

    public class UsersSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingInvitations { get; set; }
    }

    public class SetupProgressDto
    {
        public bool OrganizationProfileComplete { get; set; }
        public bool TeamsConfigured { get; set; }
        public bool RACIConfigured { get; set; }
        public bool UsersInvited { get; set; }
        public int OverallPercent { get; set; }
    }

    // ============================================================================
    // TEAMS MANAGEMENT DTOs
    // ============================================================================

    public class TeamsManagementDto
    {
        public Guid TenantId { get; set; }
        public List<TeamDto> Teams { get; set; } = new();
    }

    public class TeamDto
    {
        public Guid Id { get; set; }
        public string TeamCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public bool IsDefaultFallback { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
    }

    public class CreateTeamDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Team code is required")]
        [MaxLength(50)]
        public string TeamCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Team name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string TeamType { get; set; } = "Operational";

        [MaxLength(100)]
        public string BusinessUnit { get; set; } = string.Empty;

        public bool IsDefaultFallback { get; set; }
    }

    // ============================================================================
    // TEAM MEMBERS DTOs
    // ============================================================================

    public class TeamMembersManagementDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public List<OrgTeamMemberDto> Members { get; set; } = new();
        public List<UserOptionDto> AvailableUsers { get; set; } = new();
    }

    public class OrgTeamMemberDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public bool IsPrimaryForRole { get; set; }
        public bool CanApprove { get; set; }
        public bool CanDelegate { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinedDate { get; set; }
    }

    public class AddTeamMemberDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string RoleCode { get; set; } = "CONTROL_OWNER";

        public bool IsPrimaryForRole { get; set; }
        public bool CanApprove { get; set; }
        public bool CanDelegate { get; set; }
    }

    public class UserOptionDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // ============================================================================
    // USERS MANAGEMENT DTOs
    // ============================================================================

    public class UsersManagementDto
    {
        public Guid TenantId { get; set; }
        public List<TenantUserDto> Users { get; set; } = new();
    }

    public class TenantUserDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string TitleCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ============================================================================
    // RACI MANAGEMENT DTOs
    // ============================================================================

    public class RACIManagementDto
    {
        public Guid TenantId { get; set; }
        public List<RACIAssignmentDto> Assignments { get; set; } = new();
        public List<TeamOptionDto> AvailableTeams { get; set; } = new();
    }

    public class RACIAssignmentDto
    {
        public Guid Id { get; set; }
        public string ScopeType { get; set; } = string.Empty;
        public string ScopeId { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string RACI { get; set; } = "R";
        public string RoleCode { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }

    public class TeamOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
