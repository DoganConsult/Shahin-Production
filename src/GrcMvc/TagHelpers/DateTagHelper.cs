using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace GrcMvc.TagHelpers
{
    /// <summary>
    /// Renders a formatted date with various display options
    /// Usage: <grc-date value="@Model.Date" format="relative" />
    /// </summary>
    [HtmlTargetElement("grc-date")]
    public class DateTagHelper : TagHelper
    {
        public DateTime? Value { get; set; }
        public string Format { get; set; } = "relative"; // relative, short, long, full, custom
        public string? CustomFormat { get; set; }
        public bool ShowTime { get; set; } = false;
        public bool ShowIcon { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", "grc-date");

            if (!Value.HasValue)
            {
                output.Content.SetHtmlContent("<span class=\"text-muted\">—</span>");
                return;
            }

            var culture = CultureInfo.CurrentUICulture;
            var isRtl = culture.TextInfo.IsRightToLeft;
            var date = Value.Value;
            var now = DateTime.UtcNow;
            var tooltip = date.ToString("F", culture);

            output.Attributes.SetAttribute("title", tooltip);
            output.Attributes.SetAttribute("data-timestamp", date.ToString("o"));

            var iconHtml = ShowIcon ? "<i class=\"bi bi-calendar3 me-1\"></i>" : "";
            var displayText = GetDisplayText(date, now, culture, isRtl);

            output.Content.SetHtmlContent($"{iconHtml}{displayText}");
        }

        private string GetDisplayText(DateTime date, DateTime now, CultureInfo culture, bool isRtl)
        {
            return Format.ToLower() switch
            {
                "relative" => GetRelativeTime(date, now, isRtl),
                "short" => date.ToString("d", culture),
                "long" => date.ToString("D", culture),
                "full" => date.ToString("F", culture),
                "custom" when !string.IsNullOrEmpty(CustomFormat) => date.ToString(CustomFormat, culture),
                _ => ShowTime ? date.ToString("g", culture) : date.ToString("d", culture)
            };
        }

        private static string GetRelativeTime(DateTime date, DateTime now, bool isRtl)
        {
            var diff = now - date;
            var isFuture = diff.TotalSeconds < 0;
            diff = isFuture ? -diff : diff;

            if (diff.TotalMinutes < 1)
                return isRtl ? "الآن" : "Just now";
            if (diff.TotalHours < 1)
            {
                var mins = (int)diff.TotalMinutes;
                return isRtl
                    ? (isFuture ? $"بعد {mins} دقيقة" : $"منذ {mins} دقيقة")
                    : (isFuture ? $"in {mins}m" : $"{mins}m ago");
            }
            if (diff.TotalDays < 1)
            {
                var hours = (int)diff.TotalHours;
                return isRtl
                    ? (isFuture ? $"بعد {hours} ساعة" : $"منذ {hours} ساعة")
                    : (isFuture ? $"in {hours}h" : $"{hours}h ago");
            }
            if (diff.TotalDays < 7)
            {
                var days = (int)diff.TotalDays;
                return isRtl
                    ? (isFuture ? $"بعد {days} يوم" : $"منذ {days} يوم")
                    : (isFuture ? $"in {days}d" : $"{days}d ago");
            }

            return date.ToString("d MMM yyyy", CultureInfo.CurrentUICulture);
        }
    }
}
