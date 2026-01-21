using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing Workspaces (sub-scopes within a Tenant).
    /// Handles workspace creation, membership management, and routing configuration.
    /// </summary>
    public interface IWorkspaceManagementService
    {
        // Workspace CRUD
        Task<Workspace> CreateWorkspaceAsync(CreateWorkspaceRequest request);
        Task<Workspace?> GetWorkspaceAsync(Guid workspaceId);
        Task<Workspace?> GetWorkspaceByCodeAsync(Guid tenantId, string workspaceCode);
        Task<IReadOnlyList<Workspace>> GetTenantWorkspacesAsync(Guid tenantId);
        Task<Workspace> UpdateWorkspaceAsync(Guid workspaceId, UpdateWorkspaceRequest request);
        Task SetDefaultWorkspaceAsync(Guid tenantId, Guid workspaceId);

        // Membership management
        Task<WorkspaceMembership> AddMemberAsync(Guid workspaceId, AddWorkspaceMemberRequest request);
        Task<IReadOnlyList<WorkspaceMembership>> GetMembersAsync(Guid workspaceId);
        Task RemoveMemberAsync(Guid workspaceId, string userId);
        Task UpdateMemberRolesAsync(Guid workspaceId, string userId, List<string> roles);
        Task SetPrimaryWorkspaceAsync(Guid tenantId, string userId, Guid workspaceId);

        // Control suite management
        Task<WorkspaceControl> AddControlToWorkspaceAsync(Guid workspaceId, Guid controlId, string? overlaySource = null);
        Task<IReadOnlyList<WorkspaceControl>> GetWorkspaceControlsAsync(Guid workspaceId);
        Task RemoveControlFromWorkspaceAsync(Guid workspaceId, Guid controlId);

        // Approval gates
        Task<WorkspaceApprovalGate> CreateApprovalGateAsync(Guid workspaceId, CreateApprovalGateRequest request);
        Task<IReadOnlyList<WorkspaceApprovalGate>> GetApprovalGatesAsync(Guid workspaceId);
        Task AddApproverToGateAsync(Guid gateId, AddApproverRequest request);

        // Routing resolution
        Task<IReadOnlyList<string>> ResolveAssigneesAsync(Guid workspaceId, string roleCode, Guid? teamId = null);
    }

    public class CreateWorkspaceRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "TenantId is required")]
        public Guid TenantId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "WorkspaceCode is required")]
        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 2, ErrorMessage = "WorkspaceCode must be 2-50 characters")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "WorkspaceCode must be uppercase alphanumeric with underscores/hyphens")]
        public string WorkspaceCode { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Name is required")]
        [System.ComponentModel.DataAnnotations.StringLength(255, MinimumLength = 2, ErrorMessage = "Name must be 2-255 characters")]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? NameAr { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(Market|BusinessUnit|Entity|Environment)$", ErrorMessage = "WorkspaceType must be Market, BusinessUnit, Entity, or Environment")]
        public string WorkspaceType { get; set; } = "Market";

        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? JurisdictionCode { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string DefaultLanguage { get; set; } = "ar";

        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string? Timezone { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;
        public List<string>? Regulators { get; set; }
        public List<string>? Overlays { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "CreatedBy is required")]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Validates the request and returns validation errors
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            if (TenantId == Guid.Empty) errors.Add("TenantId is required");
            if (string.IsNullOrWhiteSpace(WorkspaceCode)) errors.Add("WorkspaceCode is required");
            if (string.IsNullOrWhiteSpace(Name)) errors.Add("Name is required");
            if (string.IsNullOrWhiteSpace(CreatedBy)) errors.Add("CreatedBy is required");
            return errors;
        }
    }

    public class UpdateWorkspaceRequest
    {
        public string? Name { get; set; }
        public string? NameAr { get; set; }
        public string? Description { get; set; }
        public string? DefaultLanguage { get; set; }
        public string? Timezone { get; set; }
        public string? Status { get; set; }
        public List<string>? Regulators { get; set; }
        public List<string>? Overlays { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }

    public class AddWorkspaceMemberRequest
    {
        public Guid TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<string> WorkspaceRoles { get; set; } = new();
        public bool IsPrimary { get; set; } = false;
        public bool IsWorkspaceAdmin { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CreateApprovalGateRequest
    {
        public Guid TenantId { get; set; }
        public string GateCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string ScopeType { get; set; } = "Global";
        public string? ScopeValue { get; set; }
        public int MinApprovals { get; set; } = 1;
        public int SlaDays { get; set; } = 3;
        public int EscalationDays { get; set; } = 2;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class AddApproverRequest
    {
        public Guid TenantId { get; set; }
        public Guid WorkspaceId { get; set; }
        public string ApproverType { get; set; } = "Role"; // User, Role, Team
        public string ApproverReference { get; set; } = string.Empty;
        public int ApprovalOrder { get; set; } = 0;
        public bool IsMandatory { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
