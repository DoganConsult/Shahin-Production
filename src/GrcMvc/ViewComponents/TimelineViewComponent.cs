using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Renders a vertical timeline
    /// Usage: @await Component.InvokeAsync("Timeline", new { events = events })
    /// </summary>
    public class TimelineViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<TimelineEvent>? events = null, bool compact = false)
        {
            var model = new TimelineModel
            {
                Events = events ?? new List<TimelineEvent>(),
                Compact = compact
            };

            return View(model);
        }
    }

    public class TimelineModel
    {
        public List<TimelineEvent> Events { get; set; } = new();
        public bool Compact { get; set; }
    }

    public class TimelineEvent
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string? TitleAr { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = "bi-circle-fill";
        public string Color { get; set; } = "primary"; // primary, success, warning, danger, info, secondary
        public string? User { get; set; }
        public string? Link { get; set; }
        public bool IsCompleted { get; set; } = true;
        public bool IsCurrent { get; set; } = false;
    }
}
