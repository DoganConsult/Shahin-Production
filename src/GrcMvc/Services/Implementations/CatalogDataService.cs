using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for querying catalog data (regulators, frameworks, controls, evidence types)
/// Provides dropdown data with sector/company-type filtering
/// Supports all 92+ regulators, 163+ frameworks, 57K+ controls
/// </summary>
public class CatalogDataService : ICatalogDataService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<CatalogDataService> _logger;
    private readonly IMemoryCache _cache;
    private const int CacheExpirationMinutes = 30;

    public CatalogDataService(
        GrcDbContext context,
        ILogger<CatalogDataService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Get all regulators with optional filtering
    /// </summary>
    public async Task<List<RegulatorCatalogDto>> GetRegulatorsAsync(
        string? sector = null,
        string? country = null,
        string? regionType = null,
        bool activeOnly = true)
    {
        var cacheKey = $"regulators_{sector}_{country}_{regionType}_{activeOnly}";

        if (_cache.TryGetValue(cacheKey, out List<RegulatorCatalogDto>? cached))
        {
            return cached ?? new List<RegulatorCatalogDto>();
        }

        try
        {
            var query = _context.RegulatorCatalogs.AsQueryable();

            if (activeOnly)
                query = query.Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(sector))
                query = query.Where(r => r.Sector == sector || r.Sector == "all");

            if (!string.IsNullOrEmpty(country))
            {
                if (country == "SA" || country == "KSA")
                    query = query.Where(r => r.RegionType == "saudi");
                else
                    query = query.Where(r => r.RegionType == "international" || r.RegionType == "regional");
            }

            if (!string.IsNullOrEmpty(regionType))
                query = query.Where(r => r.RegionType == regionType);

            var regulators = await query
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.NameEn)
                .Select(r => new RegulatorCatalogDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    NameAr = r.NameAr,
                    NameEn = r.NameEn,
                    JurisdictionEn = r.JurisdictionEn,
                    Website = r.Website,
                    Category = r.Category,
                    Sector = r.Sector,
                    RegionType = r.RegionType,
                    FrameworkCount = r.Frameworks.Count(f => !f.IsDeleted)
                })
                .ToListAsync();

            _cache.Set(cacheKey, regulators, TimeSpan.FromMinutes(CacheExpirationMinutes));
            _logger.LogDebug("Retrieved {Count} regulators", regulators.Count);

            return regulators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regulators");
            throw;
        }
    }

    /// <summary>
    /// Get all frameworks with optional filtering
    /// Supports multiple versions per framework
    /// </summary>
    public async Task<List<FrameworkCatalogDto>> GetFrameworksAsync(
        Guid? regulatorId = null,
        string? sector = null,
        string? companyType = null,
        string? category = null,
        string? version = null,
        bool mandatoryOnly = false,
        bool activeOnly = true)
    {
        var cacheKey = $"frameworks_{regulatorId}_{sector}_{companyType}_{category}_{version}_{mandatoryOnly}_{activeOnly}";

        if (_cache.TryGetValue(cacheKey, out List<FrameworkCatalogDto>? cached))
        {
            return cached ?? new List<FrameworkCatalogDto>();
        }

        try
        {
            var query = _context.FrameworkCatalogs.AsQueryable();

            if (activeOnly)
                query = query.Where(f => f.IsActive && f.Status == "Active");

            if (regulatorId.HasValue)
                query = query.Where(f => f.RegulatorId == regulatorId);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(f => f.Category == category);

            if (mandatoryOnly)
                query = query.Where(f => f.IsMandatory);

            if (!string.IsNullOrEmpty(version))
                query = query.Where(f => f.Version == version);

            // Sector filtering - frameworks applicable to sector
            if (!string.IsNullOrEmpty(sector))
            {
                // Get regulator sectors that match
                var regulatorCodes = await _context.RegulatorCatalogs
                    .Where(r => (r.Sector == sector || r.Sector == "all") && r.IsActive)
                    .Select(r => r.Code)
                    .ToListAsync();

                // Framework codes typically start with regulator code
                query = query.Where(f => regulatorCodes.Any(rc => f.Code.StartsWith(rc)));
            }

            var frameworks = await query
                .Include(f => f.Regulator)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.TitleEn)
                .ToListAsync();

            // Group by framework code to get all versions
            var frameworkGroups = frameworks
                .GroupBy(f => f.Code)
                .ToList();

            var result = new List<FrameworkCatalogDto>();

            foreach (var group in frameworkGroups)
            {
                var latest = group.OrderByDescending(f => f.Version).FirstOrDefault();
                if (latest == null)
                    continue; // Skip empty groups
                var allVersions = group.Select(f => f.Version).OrderByDescending(v => v).ToList();

                result.Add(new FrameworkCatalogDto
                {
                    Id = latest.Id,
                    Code = latest.Code,
                    Version = latest.Version,
                    TitleEn = latest.TitleEn,
                    TitleAr = latest.TitleAr,
                    DescriptionEn = latest.DescriptionEn,
                    DescriptionAr = latest.DescriptionAr,
                    RegulatorId = latest.RegulatorId,
                    RegulatorCode = latest.Regulator?.Code ?? string.Empty,
                    RegulatorName = latest.Regulator?.NameEn ?? string.Empty,
                    Category = latest.Category,
                    IsMandatory = latest.IsMandatory,
                    ControlCount = latest.ControlCount,
                    Domains = latest.Domains,
                    EffectiveDate = latest.EffectiveDate,
                    RetiredDate = latest.RetiredDate,
                    Status = latest.Status,
                    Versions = allVersions
                });
            }

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            _logger.LogDebug("Retrieved {Count} frameworks", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving frameworks");
            throw;
        }
    }

    /// <summary>
    /// Get all controls for a framework (with version support)
    /// </summary>
    public async Task<List<ControlCatalogDto>> GetControlsAsync(
        Guid frameworkId,
        string? version = null,
        string? domain = null,
        bool activeOnly = true)
    {
        var cacheKey = $"controls_{frameworkId}_{version}_{domain}_{activeOnly}";

        if (_cache.TryGetValue(cacheKey, out List<ControlCatalogDto>? cached))
        {
            return cached ?? new List<ControlCatalogDto>();
        }

        try
        {
            var framework = await _context.FrameworkCatalogs
                .FirstOrDefaultAsync(f => f.Id == frameworkId);

            if (framework == null)
                throw CatalogException.NotFound("Framework", frameworkId);

            var query = _context.ControlCatalogs
                .Where(c => c.FrameworkId == frameworkId);

            if (activeOnly)
                query = query.Where(c => c.IsActive && c.Status == "Active");

            if (!string.IsNullOrEmpty(version))
                query = query.Where(c => c.Version == version);
            else
                query = query.Where(c => c.Version == framework.Version);

            if (!string.IsNullOrEmpty(domain))
                query = query.Where(c => c.Domain == domain);

            var controls = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.ControlNumber)
                .Select(c => new ControlCatalogDto
                {
                    Id = c.Id,
                    ControlId = c.ControlId,
                    FrameworkId = c.FrameworkId,
                    FrameworkCode = framework.Code,
                    Version = c.Version,
                    ControlNumber = c.ControlNumber,
                    Domain = c.Domain,
                    Subdomain = c.Subdomain,
                    TitleAr = c.TitleAr,
                    TitleEn = c.TitleEn,
                    RequirementAr = c.RequirementAr,
                    RequirementEn = c.RequirementEn,
                    ControlType = c.ControlType,
                    MaturityLevel = c.MaturityLevel,
                    ImplementationGuidanceEn = c.ImplementationGuidanceEn,
                    EvidenceRequirements = c.EvidenceRequirements,
                    EvidenceTypeCodes = !string.IsNullOrEmpty(c.EvidenceRequirements)
                        ? c.EvidenceRequirements.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .ToList()
                        : new List<string>(),
                    MappingIso27001 = c.MappingIso27001,
                    MappingNistCsf = c.MappingNistCsf,
                    DefaultWeight = 1, // Can be configured per control
                    MinEvidenceScore = 70 // Can be configured per control
                })
                .ToListAsync();

            _cache.Set(cacheKey, controls, TimeSpan.FromMinutes(CacheExpirationMinutes));
            _logger.LogDebug("Retrieved {Count} controls for framework {FrameworkId}", controls.Count, frameworkId);

            return controls;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving controls for framework {FrameworkId}", frameworkId);
            throw;
        }
    }

    /// <summary>
    /// Get evidence types required for a control
    /// </summary>
    public async Task<List<EvidenceTypeCatalogDto>> GetEvidenceTypesAsync(
        Guid controlId,
        bool activeOnly = true)
    {
        try
        {
            var control = await _context.ControlCatalogs
                .FirstOrDefaultAsync(c => c.Id == controlId);

            if (control == null)
                return new List<EvidenceTypeCatalogDto>();

            if (string.IsNullOrEmpty(control.EvidenceRequirements))
                return new List<EvidenceTypeCatalogDto>();

            var evidenceTypeCodes = control.EvidenceRequirements
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .ToList();

            var query = _context.EvidenceTypeCatalogs
                .Where(e => evidenceTypeCodes.Contains(e.EvidenceTypeCode));

            if (activeOnly)
                query = query.Where(e => e.IsActive);

            var evidenceTypes = await query
                .OrderBy(e => e.DisplayOrder)
                .Select(e => new EvidenceTypeCatalogDto
                {
                    Id = e.Id,
                    EvidenceTypeCode = e.EvidenceTypeCode,
                    EvidenceTypeName = e.EvidenceTypeName,
                    Description = e.Description,
                    Category = e.Category,
                    IsRequired = true, // Required for this control
                    DefaultWeight = 1
                })
                .ToListAsync();

            return evidenceTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evidence types for control {ControlId}", controlId);
            throw;
        }
    }

    /// <summary>
    /// Get evidence types by framework
    /// </summary>
    public async Task<List<EvidenceTypeCatalogDto>> GetEvidenceTypesByFrameworkAsync(
        Guid frameworkId,
        string? version = null,
        bool activeOnly = true)
    {
        try
        {
            var controls = await GetControlsAsync(frameworkId, version, null, activeOnly);
            var allEvidenceTypeCodes = controls
                .SelectMany(c => c.EvidenceTypeCodes)
                .Distinct()
                .ToList();

            if (!allEvidenceTypeCodes.Any())
                return new List<EvidenceTypeCatalogDto>();

            var query = _context.EvidenceTypeCatalogs
                .Where(e => allEvidenceTypeCodes.Contains(e.EvidenceTypeCode));

            if (activeOnly)
                query = query.Where(e => e.IsActive);

            var evidenceTypes = await query
                .OrderBy(e => e.DisplayOrder)
                .Select(e => new EvidenceTypeCatalogDto
                {
                    Id = e.Id,
                    EvidenceTypeCode = e.EvidenceTypeCode,
                    EvidenceTypeName = e.EvidenceTypeName,
                    Description = e.Description,
                    Category = e.Category,
                    IsRequired = false,
                    DefaultWeight = 1
                })
                .ToListAsync();

            return evidenceTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evidence types for framework {FrameworkId}", frameworkId);
            throw;
        }
    }

    /// <summary>
    /// Get dropdown data for a specific catalog type
    /// Optimized for UI dropdown population
    /// </summary>
    public async Task<List<DropdownItemDto>> GetDropdownDataAsync(
        string catalogType,
        Dictionary<string, object>? filters = null,
        string? searchTerm = null,
        int? limit = null)
    {
        try
        {
            return catalogType.ToLower() switch
            {
                "regulator" => await GetRegulatorDropdownAsync(filters, searchTerm, limit),
                "framework" => await GetFrameworkDropdownAsync(filters, searchTerm, limit),
                "control" => await GetControlDropdownAsync(filters, searchTerm, limit),
                "evidencetype" => await GetEvidenceTypeDropdownAsync(filters, searchTerm, limit),
                _ => throw new ArgumentException($"Unknown catalog type: {catalogType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dropdown data for {CatalogType}", catalogType);
            throw;
        }
    }

    private async Task<List<DropdownItemDto>> GetRegulatorDropdownAsync(
        Dictionary<string, object>? filters,
        string? searchTerm,
        int? limit)
    {
        var query = _context.RegulatorCatalogs
            .Where(r => r.IsActive);

        if (filters != null)
        {
            if (filters.ContainsKey("sector"))
                query = query.Where(r => r.Sector == filters["sector"].ToString() || r.Sector == "all");

            if (filters.ContainsKey("regionType"))
                query = query.Where(r => r.RegionType == filters["regionType"].ToString());
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(r =>
                r.NameEn.Contains(searchTerm) ||
                r.NameAr.Contains(searchTerm) ||
                r.Code.Contains(searchTerm));
        }

        var regulators = await query
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.NameEn)
            .Select(r => new DropdownItemDto
            {
                Value = r.Id.ToString(),
                Text = r.NameEn,
                TextAr = r.NameAr,
                Description = r.JurisdictionEn,
                IsActive = r.IsActive,
                Metadata = new Dictionary<string, object>
                {
                    { "Code", r.Code },
                    { "Category", r.Category },
                    { "Sector", r.Sector }
                }
            })
            .ToListAsync();

        if (limit.HasValue)
            regulators = regulators.Take(limit.Value).ToList();

        return regulators;
    }

    private async Task<List<DropdownItemDto>> GetFrameworkDropdownAsync(
        Dictionary<string, object>? filters,
        string? searchTerm,
        int? limit)
    {
            IQueryable<FrameworkCatalog> query = _context.FrameworkCatalogs
                .Where(f => f.IsActive && f.Status == "Active")
                .Include(f => f.Regulator);

            if (filters != null)
            {
                if (filters.ContainsKey("regulatorId") && filters["regulatorId"] is Guid regulatorId)
                    query = query.Where(f => f.RegulatorId == regulatorId);

                if (filters.ContainsKey("category"))
                    query = query.Where(f => f.Category == filters["category"].ToString());

                if (filters.ContainsKey("mandatory") && (bool)filters["mandatory"])
                    query = query.Where(f => f.IsMandatory);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f =>
                    f.TitleEn.Contains(searchTerm) ||
                    f.TitleAr.Contains(searchTerm) ||
                    f.Code.Contains(searchTerm));
            }

        var frameworksList = await query
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.TitleEn)
            .ToListAsync();

        var frameworks = frameworksList
            .Select(f => new DropdownItemDto
            {
                Value = f.Id.ToString(),
                Text = $"{f.Code} {f.Version} - {f.TitleEn}",
                TextAr = f.TitleAr,
                Description = f.DescriptionEn,
                IsActive = f.IsActive,
                Metadata = new Dictionary<string, object>
                {
                    { "Code", f.Code },
                    { "Version", f.Version },
                    { "Category", f.Category },
                    { "IsMandatory", f.IsMandatory },
                    { "ControlCount", f.ControlCount }
                }
            })
            .ToList();

        if (limit.HasValue)
            frameworks = frameworks.Take(limit.Value).ToList();

        return frameworks;
    }

    private async Task<List<DropdownItemDto>> GetControlDropdownAsync(
        Dictionary<string, object>? filters,
        string? searchTerm,
        int? limit)
    {
            IQueryable<ControlCatalog> query = _context.ControlCatalogs
                .Where(c => c.IsActive && c.Status == "Active")
                .Include(c => c.Framework);

            if (filters != null)
            {
                if (filters.ContainsKey("frameworkId") && filters["frameworkId"] is Guid frameworkId)
                    query = query.Where(c => c.FrameworkId == frameworkId);

                if (filters.ContainsKey("domain"))
                    query = query.Where(c => c.Domain == filters["domain"].ToString());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.TitleEn.Contains(searchTerm) ||
                    c.TitleAr.Contains(searchTerm) ||
                    c.ControlNumber.Contains(searchTerm));
            }

        var controlsList = await query
            .OrderBy(c => c.Framework.Code)
            .ThenBy(c => c.ControlNumber)
            .ToListAsync();

        var controls = controlsList
            .Select(c => new DropdownItemDto
            {
                Value = c.Id.ToString(),
                Text = $"{c.Framework.Code}-{c.ControlNumber} - {c.TitleEn}",
                TextAr = c.TitleAr,
                Description = c.RequirementEn,
                IsActive = c.IsActive,
                Metadata = new Dictionary<string, object>
                {
                    { "ControlId", c.ControlId },
                    { "ControlNumber", c.ControlNumber },
                    { "Domain", c.Domain },
                    { "ControlType", c.ControlType }
                }
            })
            .ToList();

        if (limit.HasValue)
            controls = controls.Take(limit.Value).ToList();

        return controls;
    }

    private async Task<List<DropdownItemDto>> GetEvidenceTypeDropdownAsync(
        Dictionary<string, object>? filters,
        string? searchTerm,
        int? limit)
    {
        var query = _context.EvidenceTypeCatalogs
            .Where(e => e.IsActive);

        if (filters != null)
        {
            if (filters.ContainsKey("category"))
                query = query.Where(e => e.Category == filters["category"].ToString());
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(e =>
                e.EvidenceTypeName.Contains(searchTerm) ||
                e.EvidenceTypeCode.Contains(searchTerm));
        }

        var evidenceTypes = await query
            .OrderBy(e => e.DisplayOrder)
            .ThenBy(e => e.EvidenceTypeName)
            .Select(e => new DropdownItemDto
            {
                Value = e.Id.ToString(),
                Text = e.EvidenceTypeName,
                Description = e.Description,
                IsActive = e.IsActive,
                Metadata = new Dictionary<string, object>
                {
                    { "Code", e.EvidenceTypeCode },
                    { "Category", e.Category },
                    { "AllowedFileTypes", e.AllowedFileTypes }
                }
            })
            .ToListAsync();

        if (limit.HasValue)
            evidenceTypes = evidenceTypes.Take(limit.Value).ToList();

        return evidenceTypes;
    }

    /// <summary>
    /// Get frameworks applicable to organization profile
    /// Considers sector, company type, size, critical infrastructure status
    /// </summary>
    public async Task<List<FrameworkCatalogDto>> GetApplicableFrameworksAsync(
        string sector,
        string companyType,
        string organizationSize,
        bool isCriticalInfrastructure,
        string country = "SA")
    {
        try
        {
            // Get regulators first based on country
            var regulators = await GetRegulatorsAsync(
                sector: sector,
                country: country,
                activeOnly: true);

            var regulatorIds = regulators.Select(r => r.Id).ToList();

            var allFrameworks = await _context.FrameworkCatalogs
                .Where(f => f.IsActive && f.Status == "Active" &&
                           (regulatorIds.Contains(f.RegulatorId ?? Guid.Empty)))
                .Include(f => f.Regulator)
                .Select(f => new FrameworkCatalogDto
                {
                    Id = f.Id,
                    Code = f.Code,
                    Version = f.Version,
                    TitleEn = f.TitleEn,
                    TitleAr = f.TitleAr,
                    DescriptionEn = f.DescriptionEn,
                    DescriptionAr = f.DescriptionAr,
                    RegulatorId = f.RegulatorId,
                    RegulatorCode = f.Regulator != null ? f.Regulator.Code : string.Empty,
                    RegulatorName = f.Regulator != null ? f.Regulator.NameEn : string.Empty,
                    Category = f.Category,
                    IsMandatory = f.IsMandatory,
                    ControlCount = f.ControlCount,
                    Domains = f.Domains,
                    EffectiveDate = f.EffectiveDate,
                    RetiredDate = f.RetiredDate,
                    Status = f.Status,
                    Versions = new List<string> { f.Version }
                })
                .ToListAsync();

            var applicable = new List<FrameworkCatalogDto>();

            // Always include country-specific mandatory frameworks
            if (country == "SA" || country == "KSA")
            {
                // PDPL is always mandatory for KSA
                var pdpl = allFrameworks.FirstOrDefault(f => f.Code == "PDPL");
                if (pdpl != null)
                    applicable.Add(pdpl);

                // NCA-ECC for critical infrastructure
                if (isCriticalInfrastructure)
                {
                    var ncaEcc = allFrameworks.FirstOrDefault(f => f.Code == "NCA-ECC");
                    if (ncaEcc != null)
                        applicable.Add(ncaEcc);
                }
            }

            // Sector-specific frameworks
            if (sector.Contains("Banking") || sector.Contains("Financial"))
            {
                var samaFrameworks = allFrameworks.Where(f => f.RegulatorCode == "SAMA").ToList();
                applicable.AddRange(samaFrameworks);
            }

            if (sector.Contains("Healthcare") || sector.Contains("Medical"))
            {
                var sfdaframeworks = allFrameworks.Where(f => f.RegulatorCode == "SFDA").ToList();
                applicable.AddRange(sfdaframeworks);
            }

            // Size-based frameworks
            if (organizationSize == "Large" || organizationSize == "Enterprise")
            {
                var isoFrameworks = allFrameworks.Where(f =>
                    f.Code.StartsWith("ISO") || f.RegulatorCode == "ISO").ToList();
                applicable.AddRange(isoFrameworks);
            }

            // Remove duplicates
            return applicable
                .GroupBy(f => f.Code)
                .Select(g => g.First())
                .OrderBy(f => f.Code)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applicable frameworks");
            throw;
        }
    }

    /// <summary>
    /// Get all controls and evidence types for assessment template generation
    /// </summary>
    public async Task<AssessmentTemplateDataDto> GetAssessmentTemplateDataAsync(
        Guid frameworkId,
        string version)
    {
        try
        {
            var framework = await _context.FrameworkCatalogs
                .FirstOrDefaultAsync(f => f.Id == frameworkId);

            if (framework == null)
                throw CatalogException.NotFound("Framework", frameworkId);

            var controls = await GetControlsAsync(frameworkId, version, null, true);

            var controlsWithEvidence = new List<ControlWithEvidenceDto>();

            foreach (var control in controls)
            {
                var evidenceTypes = await GetEvidenceTypesAsync(control.Id, true);

                controlsWithEvidence.Add(new ControlWithEvidenceDto
                {
                    ControlId = control.Id,
                    ControlNumber = control.ControlNumber,
                    TitleEn = control.TitleEn,
                    TitleAr = control.TitleAr,
                    Domain = control.Domain,
                    RequirementEn = control.RequirementEn,
                    DefaultWeight = control.DefaultWeight,
                    MinEvidenceScore = control.MinEvidenceScore,
                    RequiredEvidenceTypes = evidenceTypes
                });
            }

            return new AssessmentTemplateDataDto
            {
                FrameworkId = frameworkId,
                FrameworkCode = framework.Code,
                FrameworkVersion = version,
                FrameworkTitle = framework.TitleEn,
                TotalControls = controlsWithEvidence.Count,
                Controls = controlsWithEvidence
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assessment template data for framework {FrameworkId}", frameworkId);
            throw;
        }
    }

    /// <summary>
    /// Get framework versions
    /// </summary>
    public async Task<List<string>> GetFrameworkVersionsAsync(string frameworkCode)
    {
        try
        {
            var versions = await _context.FrameworkCatalogs
                .Where(f => f.Code == frameworkCode && f.IsActive && f.Status == "Active")
                .Select(f => f.Version)
                .Distinct()
                .OrderByDescending(v => v)
                .ToListAsync();

            return versions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for framework {FrameworkCode}", frameworkCode);
            throw;
        }
    }
}
