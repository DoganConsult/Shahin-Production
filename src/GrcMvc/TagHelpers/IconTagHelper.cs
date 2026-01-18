using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a Bootstrap icon with optional styling
    /// Usage: <grc-icon name="check" color="success" size="lg" />
    /// </summary>
    [HtmlTargetElement("grc-icon")]
    public class IconTagHelper : TagHelper
    {
        public string Name { get; set; } = "circle";
        public string? Color { get; set; } // primary, secondary, success, danger, warning, info, light, dark, muted
        public string? Size { get; set; } // xs, sm, md, lg, xl, 2x, 3x, 4x, 5x
        public bool Spin { get; set; } = false;
        public string? Title { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "i";

            // Ensure icon name has bi- prefix
            var iconName = Name.StartsWith("bi-") ? Name : $"bi-{Name}";

            var classes = new List<string> { "bi", iconName, "grc-icon" };

            // Add color class
            if (!string.IsNullOrEmpty(Color))
                classes.Add($"text-{Color}");

            // Add size class
            if (!string.IsNullOrEmpty(Size))
            {
                var sizeClass = Size.ToLower() switch
                {
                    "xs" => "fs-6",
                    "sm" => "fs-5",
                    "md" => "fs-4",
                    "lg" => "fs-3",
                    "xl" => "fs-2",
                    "2x" => "fs-1",
                    "3x" => "display-5",
                    "4x" => "display-4",
                    "5x" => "display-3",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(sizeClass))
                    classes.Add(sizeClass);
            }

            // Add spin animation
            if (Spin)
                classes.Add("grc-icon-spin");

            output.Attributes.SetAttribute("class", string.Join(" ", classes));

            if (!string.IsNullOrEmpty(Title))
            {
                output.Attributes.SetAttribute("title", Title);
                output.Attributes.SetAttribute("data-bs-toggle", "tooltip");
            }

            // Self-closing tag
            output.TagMode = TagMode.SelfClosing;
        }
    }
}
