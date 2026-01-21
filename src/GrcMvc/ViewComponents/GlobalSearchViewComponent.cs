using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Global Faceted Search - Search across all GRC entities with filters
    /// </summary>
    public class GlobalSearchViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string placeholder = "Search risks, controls, policies...")
        {
            var model = new GlobalSearchModel
            {
                Placeholder = placeholder,
                SearchCategories = new List<SearchCategory>
                {
                    new() { Id = "all", Name = "All", Icon = "bi-search" },
                    new() { Id = "risks", Name = "Risks", Icon = "bi-exclamation-triangle", Color = "#ef4444" },
                    new() { Id = "controls", Name = "Controls", Icon = "bi-shield-check", Color = "#3b82f6" },
                    new() { Id = "policies", Name = "Policies", Icon = "bi-file-text", Color = "#8b5cf6" },
                    new() { Id = "assessments", Name = "Assessments", Icon = "bi-clipboard-check", Color = "#10b981" },
                    new() { Id = "evidence", Name = "Evidence", Icon = "bi-folder", Color = "#f59e0b" },
                    new() { Id = "audits", Name = "Audits", Icon = "bi-search", Color = "#06b6d4" }
                },
                Filters = new List<SearchFilter>
                {
                    new() { Id = "status", Name = "Status", Options = new[] { "Active", "Pending", "Closed", "Draft" } },
                    new() { Id = "owner", Name = "Owner", Options = new[] { "Me", "My Team", "Unassigned" } },
                    new() { Id = "severity", Name = "Severity", Options = new[] { "Critical", "High", "Medium", "Low" } },
                    new() { Id = "date", Name = "Date Range", Options = new[] { "Today", "This Week", "This Month", "This Year" } }
                }
            };

            return View(model);
        }
    }

    public class GlobalSearchModel
    {
        public string Placeholder { get; set; }
        public List<SearchCategory> SearchCategories { get; set; } = new();
        public List<SearchFilter> Filters { get; set; } = new();
    }

    public class SearchCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }

    public class SearchFilter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string[] Options { get; set; }
    }
}
