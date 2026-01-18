using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a styled card component
    /// Usage: <grc-card title="Title" icon="bi-star" color="primary">Content</grc-card>
    /// </summary>
    [HtmlTargetElement("grc-card")]
    public class CardTagHelper : TagHelper
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; } = "light"; // primary, success, warning, danger, info, light, dark
        public bool Collapsible { get; set; } = false;
        public bool Collapsed { get; set; } = false;
        public string? Footer { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"card grc-card border-{Color} shadow-sm");

            var childContent = await output.GetChildContentAsync();
            var collapseId = Guid.NewGuid().ToString("N").Substring(0, 8);

            var headerHtml = "";
            if (!string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Icon))
            {
                var collapseAttr = Collapsible
                    ? $"data-bs-toggle=\"collapse\" data-bs-target=\"#collapse-{collapseId}\" aria-expanded=\"{(!Collapsed).ToString().ToLower()}\" style=\"cursor:pointer;\""
                    : "";

                headerHtml = $@"
                    <div class=""card-header bg-{Color} bg-opacity-10 d-flex align-items-center justify-content-between"" {collapseAttr}>
                        <div class=""d-flex align-items-center gap-2"">
                            {(string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"bi {Icon} text-{Color}\"></i>")}
                            <div>
                                <h6 class=""card-title mb-0"">{Title}</h6>
                                {(string.IsNullOrEmpty(Subtitle) ? "" : $"<small class=\"text-muted\">{Subtitle}</small>")}
                            </div>
                        </div>
                        {(Collapsible ? $"<i class=\"bi bi-chevron-{(Collapsed ? "down" : "up")} collapse-icon\"></i>" : "")}
                    </div>";
            }

            var bodyClass = Collapsible ? $"collapse {(Collapsed ? "" : "show")}" : "";
            var bodyHtml = $@"
                <div class=""{bodyClass}"" id=""collapse-{collapseId}"">
                    <div class=""card-body"">
                        {childContent.GetContent()}
                    </div>
                    {(string.IsNullOrEmpty(Footer) ? "" : $"<div class=\"card-footer text-muted\">{Footer}</div>")}
                </div>";

            output.Content.SetHtmlContent(headerHtml + bodyHtml);
        }
    }
}
