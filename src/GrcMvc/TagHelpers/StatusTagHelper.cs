using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a status badge with appropriate styling
    /// Usage: <grc-status value="Active" />
    /// </summary>
    [HtmlTargetElement("grc-status")]
    public class StatusTagHelper : TagHelper
    {
        public string? Value { get; set; }
        public string? Size { get; set; } = "normal"; // small, normal, large

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            var status = Value?.ToLower() ?? "";
            var (bgClass, icon, label) = GetStatusStyle(status);
            var sizeClass = Size?.ToLower() switch
            {
                "small" or "sm" => "badge-sm",
                "large" or "lg" => "badge-lg fs-6",
                _ => ""
            };

            output.Attributes.SetAttribute("class", $"badge {bgClass} {sizeClass} grc-status");
            output.Content.SetHtmlContent($"<i class=\"bi {icon} me-1\"></i>{label}");
        }

        private static (string bgClass, string icon, string label) GetStatusStyle(string status)
        {
            return status switch
            {
                "active" or "approved" or "completed" or "compliant" => ("bg-success", "bi-check-circle-fill", "Active"),
                "pending" or "inprogress" or "in_progress" => ("bg-warning text-dark", "bi-hourglass-split", "Pending"),
                "inactive" or "disabled" or "archived" => ("bg-secondary", "bi-pause-circle", "Inactive"),
                "draft" => ("bg-info", "bi-pencil", "Draft"),
                "rejected" or "failed" or "noncompliant" => ("bg-danger", "bi-x-circle-fill", "Rejected"),
                "expired" or "overdue" => ("bg-danger", "bi-exclamation-triangle-fill", "Expired"),
                "review" or "under_review" or "submitted" => ("bg-primary", "bi-eye", "Under Review"),
                "scheduled" or "planned" => ("bg-info", "bi-calendar-event", "Scheduled"),
                _ => ("bg-light text-dark", "bi-question-circle", status)
            };
        }
    }
}
