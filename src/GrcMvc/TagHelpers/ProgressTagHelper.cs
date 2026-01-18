using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a progress bar
    /// Usage: <grc-progress percent="75" show-label="true" />
    /// </summary>
    [HtmlTargetElement("grc-progress")]
    public class ProgressTagHelper : TagHelper
    {
        public decimal Percent { get; set; } = 0;
        public string? Color { get; set; } // auto, primary, success, warning, danger, info
        public bool ShowLabel { get; set; } = true;
        public bool Striped { get; set; } = false;
        public bool Animated { get; set; } = false;
        public string? Height { get; set; } = "8px";
        public string? Label { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "grc-progress d-flex align-items-center gap-2");

            var value = Math.Min(Math.Max(Percent, 0), 100);
            var colorClass = GetColorClass(value);
            var stripedClass = Striped ? "progress-bar-striped" : "";
            var animatedClass = Animated ? "progress-bar-animated" : "";
            var labelText = Label ?? $"{value:0}%";

            var html = $@"
                <div class=""progress flex-grow-1"" style=""height: {Height};"">
                    <div class=""progress-bar {colorClass} {stripedClass} {animatedClass}""
                         role=""progressbar""
                         style=""width: {value}%;""
                         aria-valuenow=""{value}""
                         aria-valuemin=""0""
                         aria-valuemax=""100"">
                    </div>
                </div>";

            if (ShowLabel)
            {
                html += $@"<span class=""badge {colorClass}"" style=""min-width: 50px;"">{labelText}</span>";
            }

            output.Content.SetHtmlContent(html);
        }

        private string GetColorClass(decimal value)
        {
            if (!string.IsNullOrEmpty(Color) && Color != "auto")
                return $"bg-{Color}";

            return value switch
            {
                >= 80 => "bg-success",
                >= 60 => "bg-info",
                >= 40 => "bg-warning",
                >= 20 => "bg-orange",
                _ => "bg-danger"
            };
        }
    }
}
