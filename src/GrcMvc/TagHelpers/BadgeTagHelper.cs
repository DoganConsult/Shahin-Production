using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a styled badge
    /// Usage: <grc-badge type="success" pill="true">Text</grc-badge>
    /// </summary>
    [HtmlTargetElement("grc-badge")]
    public class BadgeTagHelper : TagHelper
    {
        public string Type { get; set; } = "primary"; // primary, secondary, success, danger, warning, info, light, dark
        public bool Pill { get; set; } = false;
        public string? Icon { get; set; }
        public int? Count { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            var pillClass = Pill ? "rounded-pill" : "";
            output.Attributes.SetAttribute("class", $"badge bg-{Type} {pillClass} grc-badge");

            var childContent = await output.GetChildContentAsync();
            var iconHtml = string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"bi {Icon} me-1\"></i>";
            var countHtml = Count.HasValue ? $"<span class=\"ms-1\">({Count})</span>" : "";

            output.Content.SetHtmlContent($"{iconHtml}{childContent.GetContent()}{countHtml}");
        }
    }
}
