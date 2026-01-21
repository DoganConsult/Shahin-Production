using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Permission Matrix - Role-Permission assignment grid with bulk editing
    /// Displays all roles and their permission assignments in a matrix format
    /// </summary>
    public class PermissionMatrixViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new PermissionMatrixModel
            {
                PermissionColumns = new List<string>
                {
                    "Risk_View", "Risk_Create", "Risk_Edit", "Risk_Delete", "Risk_Approve",
                    "Control_View", "Control_Create", "Control_Edit", "Control_Delete", "Control_Test",
                    "Policy_View", "Policy_Create", "Policy_Edit", "Policy_Delete", "Policy_Publish"
                },
                Roles = new List<RolePermissionRow>
                {
                    new() { RoleId = "admin", RoleName = "Administrator", Permissions = GetAdminPermissions() },
                    new() { RoleId = "compliance", RoleName = "Compliance Officer", Permissions = GetCompliancePermissions() },
                    new() { RoleId = "risk-manager", RoleName = "Risk Manager", Permissions = GetRiskManagerPermissions() },
                    new() { RoleId = "auditor", RoleName = "Auditor", Permissions = GetAuditorPermissions() },
                    new() { RoleId = "viewer", RoleName = "Read-Only User", Permissions = GetViewerPermissions() }
                }
            };

            return View(model);
        }

        private Dictionary<string, bool> GetAdminPermissions()
        {
            return new Dictionary<string, bool>
            {
                ["Risk_View"] = true, ["Risk_Create"] = true, ["Risk_Edit"] = true, ["Risk_Delete"] = true, ["Risk_Approve"] = true,
                ["Control_View"] = true, ["Control_Create"] = true, ["Control_Edit"] = true, ["Control_Delete"] = true, ["Control_Test"] = true,
                ["Policy_View"] = true, ["Policy_Create"] = true, ["Policy_Edit"] = true, ["Policy_Delete"] = true, ["Policy_Publish"] = true
            };
        }

        private Dictionary<string, bool> GetCompliancePermissions()
        {
            return new Dictionary<string, bool>
            {
                ["Risk_View"] = true, ["Risk_Create"] = true, ["Risk_Edit"] = true, ["Risk_Delete"] = false, ["Risk_Approve"] = true,
                ["Control_View"] = true, ["Control_Create"] = true, ["Control_Edit"] = true, ["Control_Delete"] = false, ["Control_Test"] = true,
                ["Policy_View"] = true, ["Policy_Create"] = true, ["Policy_Edit"] = true, ["Policy_Delete"] = false, ["Policy_Publish"] = true
            };
        }

        private Dictionary<string, bool> GetRiskManagerPermissions()
        {
            return new Dictionary<string, bool>
            {
                ["Risk_View"] = true, ["Risk_Create"] = true, ["Risk_Edit"] = true, ["Risk_Delete"] = false, ["Risk_Approve"] = false,
                ["Control_View"] = true, ["Control_Create"] = false, ["Control_Edit"] = false, ["Control_Delete"] = false, ["Control_Test"] = false,
                ["Policy_View"] = true, ["Policy_Create"] = false, ["Policy_Edit"] = false, ["Policy_Delete"] = false, ["Policy_Publish"] = false
            };
        }

        private Dictionary<string, bool> GetAuditorPermissions()
        {
            return new Dictionary<string, bool>
            {
                ["Risk_View"] = true, ["Risk_Create"] = false, ["Risk_Edit"] = false, ["Risk_Delete"] = false, ["Risk_Approve"] = false,
                ["Control_View"] = true, ["Control_Create"] = false, ["Control_Edit"] = false, ["Control_Delete"] = false, ["Control_Test"] = true,
                ["Policy_View"] = true, ["Policy_Create"] = false, ["Policy_Edit"] = false, ["Policy_Delete"] = false, ["Policy_Publish"] = false
            };
        }

        private Dictionary<string, bool> GetViewerPermissions()
        {
            return new Dictionary<string, bool>
            {
                ["Risk_View"] = true, ["Risk_Create"] = false, ["Risk_Edit"] = false, ["Risk_Delete"] = false, ["Risk_Approve"] = false,
                ["Control_View"] = true, ["Control_Create"] = false, ["Control_Edit"] = false, ["Control_Delete"] = false, ["Control_Test"] = false,
                ["Policy_View"] = true, ["Policy_Create"] = false, ["Policy_Edit"] = false, ["Policy_Delete"] = false, ["Policy_Publish"] = false
            };
        }
    }

    public class PermissionMatrixModel
    {
        public List<string> PermissionColumns { get; set; } = new();
        public List<RolePermissionRow> Roles { get; set; } = new();
    }

    public class RolePermissionRow
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public Dictionary<string, bool> Permissions { get; set; } = new();
    }
}
