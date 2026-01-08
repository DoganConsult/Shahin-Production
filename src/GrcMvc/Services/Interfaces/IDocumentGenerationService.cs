using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Interface for document generation service
/// Generates Word, PDF, and Excel documents from templates
/// </summary>
public interface IDocumentGenerationService
{
    /// <summary>
    /// Generate a Word document (.docx) from a template
    /// </summary>
    Task<byte[]> GenerateWordDocumentAsync(DocumentTemplate template, string language = "en");

    /// <summary>
    /// Generate a PDF document from a template
    /// </summary>
    Task<byte[]> GeneratePdfDocumentAsync(DocumentTemplate template, string language = "en");

    /// <summary>
    /// Generate an Excel document (.xlsx) from a template
    /// </summary>
    Task<byte[]> GenerateExcelDocumentAsync(DocumentTemplate template, string language = "en");
}
