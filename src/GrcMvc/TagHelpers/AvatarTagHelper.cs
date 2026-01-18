using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a user avatar with initials or image
    /// Usage: <grc-avatar name="John Doe" email="john@example.com" size="md" />
    /// </summary>
    [HtmlTargetElement("grc-avatar")]
    public class AvatarTagHelper : TagHelper
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string Size { get; set; } = "md"; // xs, sm, md, lg, xl
        public bool ShowTooltip { get; set; } = true;
        public string? Status { get; set; } // online, offline, away, busy

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            var sizeClass = GetSizeClass();
            var sizePx = GetSizePx();
            output.Attributes.SetAttribute("class", $"grc-avatar {sizeClass} position-relative d-inline-flex");

            if (ShowTooltip && !string.IsNullOrEmpty(Name))
            {
                output.Attributes.SetAttribute("data-bs-toggle", "tooltip");
                output.Attributes.SetAttribute("title", Name);
            }

            var avatarHtml = !string.IsNullOrEmpty(ImageUrl)
                ? GetImageAvatar(sizePx)
                : GetInitialsAvatar(sizePx);

            var statusHtml = GetStatusIndicator();

            output.Content.SetHtmlContent(avatarHtml + statusHtml);
        }

        private string GetSizeClass() => Size.ToLower() switch
        {
            "xs" => "avatar-xs",
            "sm" => "avatar-sm",
            "lg" => "avatar-lg",
            "xl" => "avatar-xl",
            _ => "avatar-md"
        };

        private int GetSizePx() => Size.ToLower() switch
        {
            "xs" => 24,
            "sm" => 32,
            "lg" => 48,
            "xl" => 64,
            _ => 40
        };

        private string GetImageAvatar(int size)
        {
            return $@"<img src=""{ImageUrl}""
                          alt=""{Name ?? "Avatar"}""
                          class=""rounded-circle""
                          style=""width: {size}px; height: {size}px; object-fit: cover;"" />";
        }

        private string GetInitialsAvatar(int size)
        {
            var initials = GetInitials();
            var bgColor = GetBackgroundColor();
            var fontSize = size / 2.5;

            return $@"<div class=""rounded-circle d-flex align-items-center justify-content-center text-white fw-semibold""
                          style=""width: {size}px; height: {size}px; background-color: {bgColor}; font-size: {fontSize}px;"">
                        {initials}
                      </div>";
        }

        private string GetInitials()
        {
            if (string.IsNullOrEmpty(Name))
                return Email?.Substring(0, 1).ToUpper() ?? "?";

            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return Name.Substring(0, Math.Min(2, Name.Length)).ToUpper();
        }

        private string GetBackgroundColor()
        {
            var text = Name ?? Email ?? "default";
            var hash = text.GetHashCode();
            var colors = new[] { "#6366f1", "#8b5cf6", "#ec4899", "#f43f5e", "#f97316", "#eab308", "#22c55e", "#14b8a6", "#06b6d4", "#3b82f6" };
            return colors[Math.Abs(hash) % colors.Length];
        }

        private string GetStatusIndicator()
        {
            if (string.IsNullOrEmpty(Status)) return "";

            var color = Status.ToLower() switch
            {
                "online" => "bg-success",
                "away" => "bg-warning",
                "busy" => "bg-danger",
                _ => "bg-secondary"
            };

            return $@"<span class=""position-absolute bottom-0 end-0 {color} border border-white rounded-circle""
                          style=""width: 12px; height: 12px;""></span>";
        }
    }
}
