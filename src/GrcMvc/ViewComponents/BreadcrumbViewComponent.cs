using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Renders navigation breadcrumbs
    /// Usage: @await Component.InvokeAsync("Breadcrumb")
    /// </summary>
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var items = new List<BreadcrumbItem>();

            // Get current route info
            var controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
            var action = ViewContext.RouteData.Values["action"]?.ToString() ?? "";
            var area = ViewContext.RouteData.Values["area"]?.ToString();

            // Always add Home
            items.Add(new BreadcrumbItem { Title = "Home", TitleAr = "الرئيسية", Url = "/", Icon = "bi-house" });

            // Add area if present
            if (!string.IsNullOrEmpty(area))
            {
                items.Add(new BreadcrumbItem
                {
                    Title = FormatName(area),
                    TitleAr = FormatName(area),
                    Url = $"/{area}"
                });
            }

            // Add controller (if not Home)
            if (!string.IsNullOrEmpty(controller) && !controller.Equals("Home", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new BreadcrumbItem
                {
                    Title = FormatName(controller),
                    TitleAr = FormatName(controller),
                    Url = $"/{controller}",
                    Icon = GetControllerIcon(controller)
                });
            }

            // Add action (if not Index)
            if (!string.IsNullOrEmpty(action) && !action.Equals("Index", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new BreadcrumbItem
                {
                    Title = FormatName(action),
                    TitleAr = FormatName(action),
                    IsActive = true
                });
            }
            else if (items.Count > 0)
            {
                items[^1].IsActive = true;
            }

            return View(items);
        }

        private static string FormatName(string name)
        {
            // Convert PascalCase to Title Case with spaces
            return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }

        private static string GetControllerIcon(string controller)
        {
            return controller.ToLower() switch
            {
                "dashboard" => "bi-speedometer2",
                "risk" or "risks" => "bi-exclamation-triangle",
                "control" or "controls" => "bi-shield-check",
                "audit" or "audits" => "bi-clipboard-check",
                "assessment" or "assessments" => "bi-card-checklist",
                "policy" or "policies" => "bi-file-earmark-text",
                "evidence" => "bi-folder",
                "workflow" or "workflows" => "bi-diagram-3",
                "report" or "reports" => "bi-graph-up",
                "user" or "users" => "bi-people",
                "settings" => "bi-gear",
                _ => ""
            };
        }
    }

    public class BreadcrumbItem
    {
        public string Title { get; set; } = "";
        public string TitleAr { get; set; } = "";
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
    }
}
