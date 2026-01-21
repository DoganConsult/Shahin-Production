namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for sanitizing HTML content to prevent XSS attacks
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>
    /// Sanitize HTML content, allowing only safe HTML tags and attributes
    /// </summary>
    /// <param name="html">Raw HTML content that may contain unsafe elements</param>
    /// <returns>Sanitized HTML safe for display</returns>
    string SanitizeHtml(string? html);

    /// <summary>
    /// Sanitize HTML content with custom allowed tags
    /// </summary>
    /// <param name="html">Raw HTML content</param>
    /// <param name="allowedTags">Custom list of allowed HTML tags</param>
    /// <returns>Sanitized HTML safe for display</returns>
    string SanitizeHtml(string? html, IEnumerable<string> allowedTags);
}
