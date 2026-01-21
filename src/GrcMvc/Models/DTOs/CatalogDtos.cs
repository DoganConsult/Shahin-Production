namespace GrcMvc.Models.DTOs;

/// <summary>
/// Regulator catalog DTO
/// </summary>
public class RegulatorCatalogDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string JurisdictionEn { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string RegionType { get; set; } = string.Empty;
    public int FrameworkCount { get; set; }
}

/// <summary>
/// Framework catalog DTO
/// </summary>
public class FrameworkCatalogDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public Guid? RegulatorId { get; set; }
    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public int ControlCount { get; set; }
    public string Domains { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? RetiredDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Versions { get; set; } = new(); // All versions of this framework
}

/// <summary>
/// Control catalog DTO
/// </summary>
public class ControlCatalogDto
{
    public Guid Id { get; set; }
    public string ControlId { get; set; } = string.Empty;
    public Guid FrameworkId { get; set; }
    public string FrameworkCode { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ControlNumber { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string RequirementAr { get; set; } = string.Empty;
    public string RequirementEn { get; set; } = string.Empty;
    public string ControlType { get; set; } = string.Empty;
    public int MaturityLevel { get; set; }
    public string ImplementationGuidanceEn { get; set; } = string.Empty;
    public string EvidenceRequirements { get; set; } = string.Empty; // Comma-separated
    public List<string> EvidenceTypeCodes { get; set; } = new(); // Parsed evidence types
    public string MappingIso27001 { get; set; } = string.Empty;
    public string MappingNistCsf { get; set; } = string.Empty;
    public int DefaultWeight { get; set; } = 1;
    public int MinEvidenceScore { get; set; } = 70;
}

/// <summary>
/// Evidence type catalog DTO
/// </summary>
public class EvidenceTypeCatalogDto
{
    public Guid Id { get; set; }
    public string EvidenceTypeCode { get; set; } = string.Empty;
    public string EvidenceTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DefaultWeight { get; set; } = 1;
}

/// <summary>
/// Dropdown item DTO for UI
/// </summary>
public class DropdownItemDto
{
    public string Value { get; set; } = string.Empty; // Usually ID or Code
    public string Text { get; set; } = string.Empty; // Display text
    public string? TextAr { get; set; } // Arabic text
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Assessment template data (all controls + evidence types for a framework)
/// </summary>
public class AssessmentTemplateDataDto
{
    public Guid FrameworkId { get; set; }
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkVersion { get; set; } = string.Empty;
    public string FrameworkTitle { get; set; } = string.Empty;
    public int TotalControls { get; set; }
    public List<ControlWithEvidenceDto> Controls { get; set; } = new();
}

/// <summary>
/// Control with its required evidence types
/// </summary>
public class ControlWithEvidenceDto
{
    public Guid ControlId { get; set; }
    public string ControlNumber { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string RequirementEn { get; set; } = string.Empty;
    public int DefaultWeight { get; set; } = 1;
    public int MinEvidenceScore { get; set; } = 70;
    public List<EvidenceTypeCatalogDto> RequiredEvidenceTypes { get; set; } = new();
}
