using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// View component for workspace switcher dropdown in header
    /// </summary>
    public class WorkspaceSwitcherViewComponent : ViewComponent
    {
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly IWorkspaceManagementService? _workspaceService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<RequestLocalizationOptions> _locOptions;

        public WorkspaceSwitcherViewComponent(
            IWorkspaceContextService? workspaceContext,
            IWorkspaceManagementService? workspaceService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<RequestLocalizationOptions> locOptions)
        {
            _workspaceContext = workspaceContext;
            _workspaceService = workspaceService;
            _httpContextAccessor = httpContextAccessor;
            _locOptions = locOptions;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new WorkspaceSwitcherModel
            {
                CurrentWorkspaceId = Guid.Empty,
                CurrentWorkspaceName = "",
                Workspaces = new List<WorkspaceItem>()
            };

            if (User?.Identity?.IsAuthenticated != true || _workspaceContext == null || _workspaceService == null)
            {
                return View(model);
            }

            try
            {
                var requestCulture = _httpContextAccessor.HttpContext?.Features.Get<IRequestCultureFeature>();
                var currentCulture = requestCulture?.RequestCulture?.UICulture?.Name ?? "ar";
                var isRtl = currentCulture == "ar";

                model.CurrentWorkspaceId = _workspaceContext.GetCurrentWorkspaceId();

                if (model.CurrentWorkspaceId != Guid.Empty)
                {
                    var workspace = await _workspaceService.GetWorkspaceAsync(model.CurrentWorkspaceId);
                    if (workspace != null)
                    {
                        model.CurrentWorkspaceName = isRtl && !string.IsNullOrEmpty(workspace.NameAr)
                            ? workspace.NameAr
                            : workspace.Name;
                    }
                }

                // Get all user workspaces
                var tenantId = _workspaceContext.GetCurrentTenantId();
                if (tenantId != Guid.Empty)
                {
                    var workspaces = await _workspaceService.GetTenantWorkspacesAsync(tenantId);
                    model.Workspaces = workspaces.Select(w => new WorkspaceItem
                    {
                        Id = w.Id,
                        Name = isRtl && !string.IsNullOrEmpty(w.NameAr) ? w.NameAr : w.Name,
                        Code = w.WorkspaceCode,
                        IsCurrent = w.Id == model.CurrentWorkspaceId
                    }).ToList();
                }

                model.IsRtl = isRtl;
            }
            catch
            {
                // Silently fail if workspace context not available
            }

            return View(model);
        }
    }

    public class WorkspaceSwitcherModel
    {
        public Guid CurrentWorkspaceId { get; set; }
        public string CurrentWorkspaceName { get; set; } = "";
        public List<WorkspaceItem> Workspaces { get; set; } = new();
        public bool IsRtl { get; set; }
    }

    public class WorkspaceItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public bool IsCurrent { get; set; }
    }
}
