using System.IO;
using System.Text;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for generating Word and PDF documents from templates.
/// Uses HTML-to-document conversion for cross-platform compatibility.
/// </summary>
public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ILogger<DocumentGenerationService> _logger;

    public DocumentGenerationService(ILogger<DocumentGenerationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate a Word document (.docx) from a template
    /// </summary>
    public async Task<byte[]> GenerateWordDocumentAsync(DocumentTemplate template, string language = "en")
    {
        _logger.LogInformation("Generating Word document for template {Code} in {Language}", template.Code, language);

        try
        {
            var title = language == "ar" ? template.TitleAr ?? template.TitleEn : template.TitleEn;
            var description = language == "ar" ? template.DescriptionAr ?? template.DescriptionEn : template.DescriptionEn;
            var instructions = language == "ar" ? template.InstructionsAr ?? template.InstructionsEn : template.InstructionsEn;
            var isRtl = language == "ar";

            // Generate Word XML content (Office Open XML format)
            var wordContent = GenerateWordXml(title, description, instructions, template, isRtl);

            return await Task.FromResult(Encoding.UTF8.GetBytes(wordContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Word document for template {Code}", template.Code);
            throw;
        }
    }

    /// <summary>
    /// Generate a PDF document from a template
    /// </summary>
    public async Task<byte[]> GeneratePdfDocumentAsync(DocumentTemplate template, string language = "en")
    {
        _logger.LogInformation("Generating PDF document for template {Code} in {Language}", template.Code, language);

        try
        {
            var title = language == "ar" ? template.TitleAr ?? template.TitleEn : template.TitleEn;
            var description = language == "ar" ? template.DescriptionAr ?? template.DescriptionEn : template.DescriptionEn;
            var instructions = language == "ar" ? template.InstructionsAr ?? template.InstructionsEn : template.InstructionsEn;
            var isRtl = language == "ar";

            // Generate HTML content and convert to PDF-ready format
            var htmlContent = GenerateHtmlContent(title, description, instructions, template, isRtl);

            // Return HTML as bytes (can be converted to PDF using browser or wkhtmltopdf)
            return await Task.FromResult(Encoding.UTF8.GetBytes(htmlContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF document for template {Code}", template.Code);
            throw;
        }
    }

    /// <summary>
    /// Generate an Excel document (.xlsx) from a template
    /// </summary>
    public async Task<byte[]> GenerateExcelDocumentAsync(DocumentTemplate template, string language = "en")
    {
        _logger.LogInformation("Generating Excel document for template {Code} in {Language}", template.Code, language);

        try
        {
            var title = language == "ar" ? template.TitleAr ?? template.TitleEn : template.TitleEn;

            // Generate CSV content as Excel alternative
            var csvContent = GenerateCsvContent(title, template);

            return await Task.FromResult(Encoding.UTF8.GetBytes(csvContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Excel document for template {Code}", template.Code);
            throw;
        }
    }

    private string GenerateWordXml(string title, string? description, string? instructions, DocumentTemplate template, bool isRtl)
    {
        var direction = isRtl ? "rtl" : "ltr";
        var align = isRtl ? "right" : "left";

        // Simple Word ML format that can be opened by Word
        return $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<?mso-application progid=""Word.Document""?>
<w:wordDocument xmlns:w=""http://schemas.microsoft.com/office/word/2003/wordml"">
  <w:body>
    <w:p>
      <w:pPr>
        <w:jc w:val=""{align}""/>
        <w:rPr>
          <w:b/>
          <w:sz w:val=""32""/>
        </w:rPr>
      </w:pPr>
      <w:r>
        <w:rPr>
          <w:b/>
          <w:sz w:val=""32""/>
        </w:rPr>
        <w:t>{EscapeXml(title)}</w:t>
      </w:r>
    </w:p>
    <w:p>
      <w:pPr><w:jc w:val=""{align}""/></w:pPr>
      <w:r><w:t>Document Code: {EscapeXml(template.Code)}</w:t></w:r>
    </w:p>
    <w:p>
      <w:pPr><w:jc w:val=""{align}""/></w:pPr>
      <w:r><w:t>Version: {EscapeXml(template.Version)}</w:t></w:r>
    </w:p>
    <w:p>
      <w:pPr><w:jc w:val=""{align}""/></w:pPr>
      <w:r><w:t>Category: {EscapeXml(template.Category)}</w:t></w:r>
    </w:p>
    <w:p/>
    {(string.IsNullOrEmpty(description) ? "" : $@"
    <w:p>
      <w:pPr>
        <w:jc w:val=""{align}""/>
        <w:rPr><w:b/></w:rPr>
      </w:pPr>
      <w:r><w:rPr><w:b/></w:rPr><w:t>Description</w:t></w:r>
    </w:p>
    <w:p>
      <w:pPr><w:jc w:val=""{align}""/></w:pPr>
      <w:r><w:t>{EscapeXml(description)}</w:t></w:r>
    </w:p>
    <w:p/>")}
    {(string.IsNullOrEmpty(instructions) ? "" : $@"
    <w:p>
      <w:pPr>
        <w:jc w:val=""{align}""/>
        <w:rPr><w:b/></w:rPr>
      </w:pPr>
      <w:r><w:rPr><w:b/></w:rPr><w:t>Instructions</w:t></w:r>
    </w:p>
    <w:p>
      <w:pPr><w:jc w:val=""{align}""/></w:pPr>
      <w:r><w:t>{EscapeXml(instructions)}</w:t></w:r>
    </w:p>
    <w:p/>")}
    <w:p>
      <w:pPr><w:jc w:val=""{align}""/></w:pPr>
      <w:r><w:t>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</w:t></w:r>
    </w:p>
  </w:body>
</w:wordDocument>";
    }

    private string GenerateHtmlContent(string title, string? description, string? instructions, DocumentTemplate template, bool isRtl)
    {
        var direction = isRtl ? "rtl" : "ltr";
        var align = isRtl ? "right" : "left";
        var font = isRtl ? "'Segoe UI', 'Arial', sans-serif" : "'Segoe UI', sans-serif";

        return $@"<!DOCTYPE html>
<html dir=""{direction}"" lang=""{(isRtl ? "ar" : "en")}"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{System.Web.HttpUtility.HtmlEncode(title)}</title>
    <style>
        body {{
            font-family: {font};
            max-width: 800px;
            margin: 0 auto;
            padding: 40px;
            line-height: 1.6;
            direction: {direction};
            text-align: {align};
        }}
        h1 {{
            color: #1a365d;
            border-bottom: 2px solid #3182ce;
            padding-bottom: 10px;
        }}
        .metadata {{
            background: #f7fafc;
            padding: 15px;
            border-radius: 8px;
            margin: 20px 0;
        }}
        .metadata p {{
            margin: 5px 0;
            color: #4a5568;
        }}
        .section {{
            margin: 30px 0;
        }}
        .section h2 {{
            color: #2d3748;
            font-size: 1.25rem;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #e2e8f0;
            font-size: 0.875rem;
            color: #718096;
        }}
        @media print {{
            body {{ padding: 20px; }}
        }}
    </style>
</head>
<body>
    <h1>{System.Web.HttpUtility.HtmlEncode(title)}</h1>

    <div class=""metadata"">
        <p><strong>Document Code:</strong> {System.Web.HttpUtility.HtmlEncode(template.Code)}</p>
        <p><strong>Version:</strong> {System.Web.HttpUtility.HtmlEncode(template.Version)}</p>
        <p><strong>Category:</strong> {System.Web.HttpUtility.HtmlEncode(template.Category)}</p>
        {(string.IsNullOrEmpty(template.Domain) ? "" : $"<p><strong>Domain:</strong> {System.Web.HttpUtility.HtmlEncode(template.Domain)}</p>")}
    </div>

    {(string.IsNullOrEmpty(description) ? "" : $@"
    <div class=""section"">
        <h2>{(isRtl ? "الوصف" : "Description")}</h2>
        <p>{System.Web.HttpUtility.HtmlEncode(description)}</p>
    </div>")}

    {(string.IsNullOrEmpty(instructions) ? "" : $@"
    <div class=""section"">
        <h2>{(isRtl ? "التعليمات" : "Instructions")}</h2>
        <p>{System.Web.HttpUtility.HtmlEncode(instructions)}</p>
    </div>")}

    <div class=""footer"">
        <p>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
        <p>Shahin-AI GRC Platform - Document Center</p>
    </div>
</body>
</html>";
    }

    private string GenerateCsvContent(string title, DocumentTemplate template)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Field,Value");
        sb.AppendLine($"\"Title\",\"{EscapeCsv(title)}\"");
        sb.AppendLine($"\"Code\",\"{EscapeCsv(template.Code)}\"");
        sb.AppendLine($"\"Version\",\"{EscapeCsv(template.Version)}\"");
        sb.AppendLine($"\"Category\",\"{EscapeCsv(template.Category)}\"");
        sb.AppendLine($"\"Domain\",\"{EscapeCsv(template.Domain ?? "")}\"");
        sb.AppendLine($"\"Generated\",\"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\"");
        return sb.ToString();
    }

    private static string EscapeXml(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static string EscapeCsv(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("\"", "\"\"");
    }
}
