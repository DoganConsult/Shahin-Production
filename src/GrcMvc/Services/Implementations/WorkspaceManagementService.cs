using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Service for managing Workspaces (sub-scopes within a Tenant).
    /// Implements the "Workspace inside Tenant" model for multi-market banks.
    /// </summary>
    public class WorkspaceManagementService : IWorkspaceManagementService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkspaceManagementService> _logger;
        private readonly IAuditEventService? _auditService;
        private readonly ITenantContextService? _tenantContext;

        public WorkspaceManagementService(
            GrcDbContext context,
            ILogger<WorkspaceManagementService> logger,
            IAuditEventService? auditService = null,
            ITenantContextService? tenantContext = null)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
            _tenantContext = tenantContext;
        }

        #region Workspace CRUD

        public async Task<Workspace> CreateWorkspaceAsync(CreateWorkspaceRequest request)
        {
            // Validate request
            var validationErrors = request.Validate();
            if (validationErrors.Any())
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
            }

            // Check for duplicate code
            var existing = await _context.Workspaces
                .IgnoreQueryFilters() // Include soft-deleted
                .FirstOrDefaultAsync(w => w.TenantId == request.TenantId
                                       && w.WorkspaceCode == request.WorkspaceCode);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    // Reactivate soft-deleted workspace
                    existing.IsDeleted = false;
                    existing.Name = request.Name;
                    existing.NameAr = request.NameAr;
                    existing.Status = "Active";
                    existing.ModifiedDate = DateTime.UtcNow;
                    existing.ModifiedBy = request.CreatedBy;
                    await _context.SaveChangesAsync();
                    return existing;
                }
                throw new EntityExistsException("Workspace", "Code", request.WorkspaceCode);
            }

            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                WorkspaceCode = request.WorkspaceCode,
                Name = request.Name,
                NameAr = request.NameAr,
                WorkspaceType = request.WorkspaceType,
                JurisdictionCode = request.JurisdictionCode,
                DefaultLanguage = request.DefaultLanguage,
                Timezone = request.Timezone,
                Description = request.Description,
                IsDefault = request.IsDefault,
                RegulatorsJson = request.Regulators != null ? JsonSerializer.Serialize(request.Regulators) : null,
                OverlaysJson = request.Overlays != null ? JsonSerializer.Serialize(request.Overlays) : null,
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            // If this is set as default, unset other defaults
            if (request.IsDefault)
            {
                var existingDefaults = await _context.Workspaces
                    .Where(w => w.TenantId == request.TenantId && w.IsDefault && !w.IsDeleted)
                    .ToListAsync();
                foreach (var def in existingDefaults)
                {
                    def.IsDefault = false;
                }
            }

            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();

            // Add creator as workspace member if user context is available
            if (_tenantContext != null && !string.IsNullOrEmpty(_tenantContext.GetCurrentUserId()))
            {
                var userId = _tenantContext.GetCurrentUserId();
                var membership = new WorkspaceMembership
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    WorkspaceId = workspace.Id,
                    UserId = userId,
                    IsPrimary = request.IsDefault, // Primary if this is the default workspace
                    Status = "Active",
                    WorkspaceRolesJson = JsonSerializer.Serialize(new List<string> { "WorkspaceAdmin" }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };
                _context.Set<WorkspaceMembership>().Add(membership);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added creator {UserId} as member of workspace {WorkspaceCode}",
                    userId, workspace.WorkspaceCode);
            }

            _logger.LogInformation("Created workspace {WorkspaceCode} in tenant {TenantId}",
                workspace.WorkspaceCode, workspace.TenantId);

            // Audit log
            if (_auditService != null)
            {
                await _auditService.LogEventAsync(
                    tenantId: request.TenantId,
                    eventType: "WorkspaceCreated",
                    affectedEntityType: "Workspace",
                    affectedEntityId: workspace.Id.ToString(),
                    action: "Create",
                    actor: request.CreatedBy,
                    payloadJson: JsonSerializer.Serialize(new { workspace.WorkspaceCode, workspace.Name, workspace.WorkspaceType })
                );
            }

            return workspace;
        }

        public async Task<Workspace?> GetWorkspaceAsync(Guid workspaceId)
        {
            return await _context.Workspaces
                .Include(w => w.Memberships)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
        }

        public async Task<Workspace?> GetWorkspaceByCodeAsync(Guid tenantId, string workspaceCode)
        {
            return await _context.Workspaces
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.WorkspaceCode == workspaceCode);
        }

        public async Task<IReadOnlyList<Workspace>> GetTenantWorkspacesAsync(Guid tenantId)
        {
            return await _context.Workspaces
                .Where(w => w.TenantId == tenantId && w.Status == "Active")
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Workspace> UpdateWorkspaceAsync(Guid workspaceId, UpdateWorkspaceRequest request)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                throw new EntityNotFoundException("Workspace", workspaceId);

            if (!string.IsNullOrEmpty(request.Name))
                workspace.Name = request.Name;
            if (request.NameAr != null)
                workspace.NameAr = request.NameAr;
            if (!string.IsNullOrEmpty(request.Description))
                workspace.Description = request.Description;
            if (!string.IsNullOrEmpty(request.DefaultLanguage))
                workspace.DefaultLanguage = request.DefaultLanguage;
            if (!string.IsNullOrEmpty(request.Timezone))
                workspace.Timezone = request.Timezone;
            if (!string.IsNullOrEmpty(request.Status))
                workspace.Status = request.Status;
            if (request.Regulators != null)
                workspace.RegulatorsJson = JsonSerializer.Serialize(request.Regulators);
            if (request.Overlays != null)
                workspace.OverlaysJson = JsonSerializer.Serialize(request.Overlays);

            workspace.ModifiedDate = DateTime.UtcNow;
            workspace.ModifiedBy = request.ModifiedBy;

            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task SetDefaultWorkspaceAsync(Guid tenantId, Guid workspaceId)
        {
            // Unset all existing defaults for this tenant
            var existingDefaults = await _context.Workspaces
                .Where(w => w.TenantId == tenantId && w.IsDefault)
                .ToListAsync();
            foreach (var def in existingDefaults)
            {
                def.IsDefault = false;
            }

            // Set new default
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                throw new EntityNotFoundException("Workspace", workspaceId);

            workspace.IsDefault = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set workspace {WorkspaceId} as default for tenant {TenantId}",
                workspaceId, tenantId);
        }

        #endregion

        #region Membership Management

        public async Task<WorkspaceMembership> AddMemberAsync(Guid workspaceId, AddWorkspaceMemberRequest request)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                throw new EntityNotFoundException("Workspace", workspaceId);

            // Check for existing membership
            var existing = await _context.WorkspaceMemberships
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == request.UserId && !m.IsDeleted);

            if (existing != null)
            {
                // Update existing membership
                existing.WorkspaceRolesJson = JsonSerializer.Serialize(request.WorkspaceRoles);
                existing.IsPrimary = request.IsPrimary;
                existing.IsWorkspaceAdmin = request.IsWorkspaceAdmin;
                existing.ModifiedDate = DateTime.UtcNow;
                existing.ModifiedBy = request.CreatedBy;
                await _context.SaveChangesAsync();
                return existing;
            }

            var membership = new WorkspaceMembership
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                WorkspaceId = workspaceId,
                UserId = request.UserId,
                WorkspaceRolesJson = JsonSerializer.Serialize(request.WorkspaceRoles),
                IsPrimary = request.IsPrimary,
                IsWorkspaceAdmin = request.IsWorkspaceAdmin,
                Status = "Active",
                JoinedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            _context.WorkspaceMemberships.Add(membership);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added member {UserId} to workspace {WorkspaceId}",
                request.UserId, workspaceId);

            return membership;
        }

        public async Task<IReadOnlyList<WorkspaceMembership>> GetMembersAsync(Guid workspaceId)
        {
            return await _context.WorkspaceMemberships
                .Where(m => m.WorkspaceId == workspaceId && m.Status == "Active")
                .Include(m => m.User)
                .ToListAsync();
        }

        public async Task RemoveMemberAsync(Guid workspaceId, string userId)
        {
            var membership = await _context.WorkspaceMemberships
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && !m.IsDeleted);

            if (membership != null)
            {
                membership.Status = "Disabled";
                membership.IsDeleted = true;
                membership.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed member {UserId} from workspace {WorkspaceId}",
                    userId, workspaceId);
            }
        }

        public async Task UpdateMemberRolesAsync(Guid workspaceId, string userId, List<string> roles)
        {
            var membership = await _context.WorkspaceMemberships
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && !m.IsDeleted);

            if (membership == null)
                throw new EntityNotFoundException("WorkspaceMembership", $"{userId}:{workspaceId}");

            membership.WorkspaceRolesJson = JsonSerializer.Serialize(roles);
            membership.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task SetPrimaryWorkspaceAsync(Guid tenantId, string userId, Guid workspaceId)
        {
            // Unset existing primary
            var existingPrimary = await _context.WorkspaceMemberships
                .Where(m => m.TenantId == tenantId && m.UserId == userId && m.IsPrimary && !m.IsDeleted)
                .ToListAsync();
            foreach (var p in existingPrimary)
            {
                p.IsPrimary = false;
            }

            // Set new primary
            var membership = await _context.WorkspaceMemberships
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && !m.IsDeleted);

            if (membership != null)
            {
                membership.IsPrimary = true;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Control Suite Management

        public async Task<WorkspaceControl> AddControlToWorkspaceAsync(Guid workspaceId, Guid controlId, string? overlaySource = null)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                throw new EntityNotFoundException("Workspace", workspaceId);

            // Check for existing
            var existing = await _context.WorkspaceControls
                .FirstOrDefaultAsync(wc => wc.WorkspaceId == workspaceId && wc.ControlId == controlId && !wc.IsDeleted);

            if (existing != null)
                return existing;

            var workspaceControl = new WorkspaceControl
            {
                Id = Guid.NewGuid(),
                TenantId = workspace.TenantId,
                WorkspaceId = workspaceId,
                ControlId = controlId,
                Status = "Active",
                OverlaySource = overlaySource,
                CreatedDate = DateTime.UtcNow
            };

            _context.WorkspaceControls.Add(workspaceControl);
            await _context.SaveChangesAsync();

            return workspaceControl;
        }

        public async Task<IReadOnlyList<WorkspaceControl>> GetWorkspaceControlsAsync(Guid workspaceId)
        {
            return await _context.WorkspaceControls
                .Where(wc => wc.WorkspaceId == workspaceId && wc.Status == "Active")
                .Include(wc => wc.Control)
                .ToListAsync();
        }

        public async Task RemoveControlFromWorkspaceAsync(Guid workspaceId, Guid controlId)
        {
            var workspaceControl = await _context.WorkspaceControls
                .FirstOrDefaultAsync(wc => wc.WorkspaceId == workspaceId && wc.ControlId == controlId && !wc.IsDeleted);

            if (workspaceControl != null)
            {
                workspaceControl.Status = "Inactive";
                workspaceControl.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Approval Gates

        public async Task<WorkspaceApprovalGate> CreateApprovalGateAsync(Guid workspaceId, CreateApprovalGateRequest request)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                throw new EntityNotFoundException("Workspace", workspaceId);

            var gate = new WorkspaceApprovalGate
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                WorkspaceId = workspaceId,
                GateCode = request.GateCode,
                Name = request.Name,
                NameAr = request.NameAr,
                ScopeType = request.ScopeType,
                ScopeValue = request.ScopeValue,
                MinApprovals = request.MinApprovals,
                SlaDays = request.SlaDays,
                EscalationDays = request.EscalationDays,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            _context.WorkspaceApprovalGates.Add(gate);
            await _context.SaveChangesAsync();

            return gate;
        }

        public async Task<IReadOnlyList<WorkspaceApprovalGate>> GetApprovalGatesAsync(Guid workspaceId)
        {
            return await _context.WorkspaceApprovalGates
                .Where(g => g.WorkspaceId == workspaceId && g.IsActive && !g.IsDeleted)
                .Include(g => g.Approvers)
                .ToListAsync();
        }

        public async Task AddApproverToGateAsync(Guid gateId, AddApproverRequest request)
        {
            var gate = await _context.WorkspaceApprovalGates.FindAsync(gateId);
            if (gate == null)
                throw new EntityNotFoundException("ApprovalGate", gateId);

            var approver = new WorkspaceApprovalGateApprover
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                WorkspaceId = request.WorkspaceId,
                GateId = gateId,
                ApproverType = request.ApproverType,
                ApproverReference = request.ApproverReference,
                ApprovalOrder = request.ApprovalOrder,
                IsMandatory = request.IsMandatory,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            _context.WorkspaceApprovalGateApprovers.Add(approver);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Routing Resolution

        public async Task<IReadOnlyList<string>> ResolveAssigneesAsync(Guid workspaceId, string roleCode, Guid? teamId = null)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
                return Array.Empty<string>();

            // Query team members within this workspace with the specified role
            var query = _context.TeamMembers
                .Where(tm => tm.TenantId == workspace.TenantId
                          && tm.WorkspaceId == workspaceId
                          && tm.RoleCode == roleCode
                          && tm.IsActive
                          && !tm.IsDeleted);

            if (teamId.HasValue)
            {
                query = query.Where(tm => tm.TeamId == teamId.Value);
            }

            var userIds = await query
                .Select(tm => tm.UserId.ToString())
                .ToListAsync();

            // If no workspace-scoped members found, try shared teams (WorkspaceId = null)
            if (!userIds.Any())
            {
                userIds = await _context.TeamMembers
                    .Where(tm => tm.TenantId == workspace.TenantId
                              && tm.WorkspaceId == null // Shared team
                              && tm.RoleCode == roleCode
                              && tm.IsActive
                              && !tm.IsDeleted)
                    .Select(tm => tm.UserId.ToString())
                    .ToListAsync();
            }

            return userIds.AsReadOnly();
        }

        #endregion
    }
}
