using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders an alert box
    /// Usage: <grc-alert type="warning" dismissible="true">Message</grc-alert>
    /// </summary>
    [HtmlTargetElement("grc-alert")]
    public class AlertTagHelper : TagHelper
    {
        public string Type { get; set; } = "info"; // primary, secondary, success, danger, warning, info, light, dark
        public bool Dismissible { get; set; } = true;
        public string? Icon { get; set; }
        public string? Title { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            var dismissClass = Dismissible ? "alert-dismissible fade show" : "";
            output.Attributes.SetAttribute("class", $"alert alert-{Type} {dismissClass} grc-alert");
            output.Attributes.SetAttribute("role", "alert");

            var childContent = await output.GetChildContentAsync();
            var iconHtml = GetIconHtml();
            var titleHtml = string.IsNullOrEmpty(Title) ? "" : $"<strong class=\"alert-heading\">{Title}</strong><br/>";
            var dismissBtn = Dismissible
                ? "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>"
                : "";

            output.Content.SetHtmlContent($@"
                <div class=""d-flex gap-2"">
                    {iconHtml}
                    <div class=""flex-grow-1"">
                        {titleHtml}
                        {childContent.GetContent()}
                    </div>
                </div>
                {dismissBtn}");
        }

        private string GetIconHtml()
        {
            var icon = Icon ?? Type switch
            {
                "success" => "bi-check-circle-fill",
                "danger" or "error" => "bi-exclamation-octagon-fill",
                "warning" => "bi-exclamation-triangle-fill",
                "info" => "bi-info-circle-fill",
                "primary" => "bi-bell-fill",
                _ => ""
            };

            return string.IsNullOrEmpty(icon) ? "" : $"<i class=\"bi {icon} fs-5\"></i>";
        }
    }
}
