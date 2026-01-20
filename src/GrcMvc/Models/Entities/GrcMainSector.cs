using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Main GRC Sector Catalog - The 18 main sectors for KSA
/// This table stores the main sectors separately from sub-sector mappings
/// </summary>
[Table("GrcMainSectors")]
public class GrcMainSector : BaseEntity
{
    /// <summary>
    /// Sector code (e.g., "BANKING", "HEALTHCARE", "GOVERNMENT")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SectorCode { get; set; } = string.Empty;

    /// <summary>
    /// Sector name in English
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SectorNameEn { get; set; } = string.Empty;

    /// <summary>
    /// Sector name in Arabic
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SectorNameAr { get; set; } = string.Empty;

    /// <summary>
    /// Sector description in English
    /// </summary>
    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Sector description in Arabic
    /// </summary>
    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Primary regulator for this sector (e.g., "SAMA", "NCA", "MOH")
    /// </summary>
    [MaxLength(100)]
    public string? PrimaryRegulator { get; set; }

    /// <summary>
    /// Number of applicable frameworks for this sector
    /// </summary>
    public int FrameworkCount { get; set; } = 0;

    /// <summary>
    /// Total number of controls across all frameworks for this sector
    /// </summary>
    public int TotalControlCount { get; set; } = 0;

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Is this sector active/enabled
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Icon/emoji for UI display
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }

    /// <summary>
    /// Color code for UI display
    /// </summary>
    [MaxLength(20)]
    public string? ColorCode { get; set; }

    // Navigation property - Sub-sectors that map to this main sector
    public virtual ICollection<GrcSubSectorMapping> SubSectors { get; set; } = new List<GrcSubSectorMapping>();
}
