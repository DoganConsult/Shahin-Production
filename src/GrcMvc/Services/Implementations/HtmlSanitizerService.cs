using Ganss.Xss;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// HTML Sanitization Service using Ganss.Xss (HtmlSanitizer) to prevent XSS attacks
/// SECURITY: Sanitizes user-generated content before rendering in views
/// </summary>
public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _defaultSanitizer;
    private readonly ILogger<HtmlSanitizerService> _logger;

    public HtmlSanitizerService(ILogger<HtmlSanitizerService> logger)
    {
        _logger = logger;
        _defaultSanitizer = CreateDefaultSanitizer();
    }

    /// <summary>
    /// Create default HTML sanitizer with safe tag/attribute whitelist
    /// </summary>
    private static HtmlSanitizer CreateDefaultSanitizer()
    {
        // HtmlSanitizer has safe defaults, but we customize for our use case
        var sanitizer = new HtmlSanitizer();
        
        // Clear defaults and only allow safe formatting tags
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("br");
        sanitizer.AllowedTags.Add("strong");
        sanitizer.AllowedTags.Add("em");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("ul");
        sanitizer.AllowedTags.Add("ol");
        sanitizer.AllowedTags.Add("li");
        sanitizer.AllowedTags.Add("h1");
        sanitizer.AllowedTags.Add("h2");
        sanitizer.AllowedTags.Add("h3");
        sanitizer.AllowedTags.Add("h4");
        sanitizer.AllowedTags.Add("h5");
        sanitizer.AllowedTags.Add("h6");
        sanitizer.AllowedTags.Add("blockquote");
        sanitizer.AllowedTags.Add("a");
        sanitizer.AllowedTags.Add("span");
        sanitizer.AllowedTags.Add("div");
        
        // Allow safe attributes only
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("title");
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("id");
        
        // Allow safe URL schemes for links
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");
        
        // Allow safe CSS properties (limited set)
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedCssProperties.Add("color");
        sanitizer.AllowedCssProperties.Add("background-color");
        sanitizer.AllowedCssProperties.Add("font-size");
        sanitizer.AllowedCssProperties.Add("font-weight");
        sanitizer.AllowedCssProperties.Add("text-align");
        
        // Note: Dangerous elements (script, iframe, object, embed, etc.) and
        // event handlers (onclick, onerror, etc.) are automatically removed
        
        return sanitizer;
    }

    public string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        try
        {
            var sanitized = _defaultSanitizer.Sanitize(html);
            return sanitized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sanitize HTML content, returning encoded version");
            // Fallback: return HTML-encoded version if sanitization fails
            return System.Net.WebUtility.HtmlEncode(html);
        }
    }

    public string SanitizeHtml(string? html, IEnumerable<string> allowedTags)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        try
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            foreach (var tag in allowedTags)
            {
                sanitizer.AllowedTags.Add(tag.ToLowerInvariant());
            }
            
            // Allow basic safe attributes
            sanitizer.AllowedAttributes.Add("href");
            sanitizer.AllowedAttributes.Add("title");
            sanitizer.AllowedAttributes.Add("class");
            
            var sanitized = sanitizer.Sanitize(html);
            return sanitized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sanitize HTML with custom tags, returning encoded version");
            return System.Net.WebUtility.HtmlEncode(html);
        }
    }
}
