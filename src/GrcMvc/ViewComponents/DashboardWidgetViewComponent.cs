using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Renders a dashboard widget/card with stats
    /// Usage: @await Component.InvokeAsync("DashboardWidget", new { title = "Risks", value = 42, icon = "bi-exclamation-triangle", color = "danger" })
    /// </summary>
    public class DashboardWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            string title,
            object value,
            string? icon = null,
            string color = "primary",
            string? subtitle = null,
            decimal? change = null,
            string? changeLabel = null,
            string? link = null,
            string? linkText = null)
        {
            var model = new DashboardWidgetModel
            {
                Title = title,
                Value = value?.ToString() ?? "0",
                Icon = icon ?? "bi-graph-up",
                Color = color,
                Subtitle = subtitle,
                Change = change,
                ChangeLabel = changeLabel,
                Link = link,
                LinkText = linkText ?? "View Details"
            };

            return View(model);
        }
    }

    public class DashboardWidgetModel
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "0";
        public string Icon { get; set; } = "bi-graph-up";
        public string Color { get; set; } = "primary";
        public string? Subtitle { get; set; }
        public decimal? Change { get; set; }
        public string? ChangeLabel { get; set; }
        public string? Link { get; set; }
        public string? LinkText { get; set; }
    }
}
