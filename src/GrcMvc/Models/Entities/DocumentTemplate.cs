using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Document template for the Document Center
/// قالب مستند لمركز المستندات
/// </summary>
public class DocumentTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string TitleEn { get; set; } = string.Empty;

    [StringLength(200)]
    public string? TitleAr { get; set; }

    [StringLength(2000)]
    public string? DescriptionEn { get; set; }

    [StringLength(2000)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Template category: Policy, Procedure, Form, Checklist, Report, Agreement
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// GRC domain: Governance, Risk, Compliance, Audit, Security, Privacy
    /// </summary>
    [StringLength(50)]
    public string? Domain { get; set; }

    /// <summary>
    /// Related framework codes (comma-separated): NCA-ECC, PDPL, SAMA-CSF
    /// </summary>
    [StringLength(500)]
    public string? FrameworkCodes { get; set; }

    /// <summary>
    /// Template version
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// File format: docx, xlsx, pdf
    /// </summary>
    [Required]
    [StringLength(10)]
    public string FileFormat { get; set; } = "docx";

    /// <summary>
    /// Path to English template file
    /// </summary>
    [StringLength(500)]
    public string? FilePathEn { get; set; }

    /// <summary>
    /// Path to Arabic template file
    /// </summary>
    [StringLength(500)]
    public string? FilePathAr { get; set; }

    /// <summary>
    /// Template sections/structure as JSON
    /// </summary>
    public string? SectionsJson { get; set; }

    /// <summary>
    /// Required fields in the template as JSON
    /// </summary>
    public string? FieldsJson { get; set; }

    /// <summary>
    /// Usage instructions
    /// </summary>
    public string? InstructionsEn { get; set; }

    public string? InstructionsAr { get; set; }

    /// <summary>
    /// Tags for search (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Priority for display order
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Download count
    /// </summary>
    public int DownloadCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsBilingual { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Document template category enumeration
/// </summary>
public static class DocumentTemplateCategories
{
    public const string Policy = "Policy";
    public const string Procedure = "Procedure";
    public const string Form = "Form";
    public const string Checklist = "Checklist";
    public const string Report = "Report";
    public const string Agreement = "Agreement";
    public const string Certificate = "Certificate";
    public const string Guide = "Guide";
}

/// <summary>
/// GRC domain enumeration
/// </summary>
public static class GrcDomains
{
    public const string Governance = "Governance";
    public const string Risk = "Risk";
    public const string Compliance = "Compliance";
    public const string Audit = "Audit";
    public const string Security = "Security";
    public const string Privacy = "Privacy";
    public const string Operations = "Operations";
    public const string Training = "Training";
}
