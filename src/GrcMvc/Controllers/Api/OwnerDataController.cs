using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Owner Dashboard CRUD operations
/// Manages Regulators, Frameworks, Controls with full CRUD + Export
/// </summary>
[Route("api/owner")]
[ApiController]
[Authorize(Roles = "PlatformAdmin,Owner")]
public class OwnerDataController : ControllerBase
{
    private readonly GrcDbContext _context;
    private readonly ILogger<OwnerDataController> _logger;

    public OwnerDataController(GrcDbContext context, ILogger<OwnerDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Regulators CRUD

    /// <summary>
    /// Get all regulators with search/filter
    /// Uses RegulatorCatalog (seeded regulatory authorities)
    /// </summary>
    [HttpGet("regulators")]
    public async Task<IActionResult> GetRegulators(
        [FromQuery] string? search = null,
        [FromQuery] string? regionType = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.RegulatorCatalogs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(r => 
                r.NameEn.ToLower().Contains(search) || 
                r.NameAr.Contains(search) || 
                r.Code.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(regionType))
            query = query.Where(r => r.RegionType == regionType);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(r => r.Category == category);

        var totalCount = await query.CountAsync();
        var regulators = await query
            .OrderBy(r => r.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new {
                r.Id,
                regulatorCode = r.Code,
                name = r.NameEn,
                nameAr = r.NameAr,
                description = r.JurisdictionEn,
                jurisdiction = r.RegionType == "saudi" ? "Saudi Arabia" : 
                              r.RegionType == "international" ? "International" : "Regional",
                type = r.Category,
                r.Website,
                contactEmail = "",
                status = "Active",
                isPrimary = r.DisplayOrder <= 10,
                r.CreatedDate
            })
            .ToListAsync();

        return Ok(new {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            data = regulators
        });
    }

    /// <summary>
    /// Get single regulator by ID
    /// </summary>
    [HttpGet("regulators/{id}")]
    public async Task<IActionResult> GetRegulator(Guid id)
    {
        var r = await _context.RegulatorCatalogs.FindAsync(id);
        if (r == null)
            return NotFound(new { error = "Regulator not found" });

        return Ok(new {
            r.Id,
            regulatorCode = r.Code,
            name = r.NameEn,
            nameAr = r.NameAr,
            description = r.JurisdictionEn,
            jurisdiction = r.RegionType == "saudi" ? "Saudi Arabia" : 
                          r.RegionType == "international" ? "International" : "Regional",
            type = r.Category,
            r.Website,
            r.Category,
            r.Sector,
            r.RegionType,
            r.Established,
            r.DisplayOrder,
            r.CreatedDate
        });
    }

    /// <summary>
    /// Create new regulator
    /// </summary>
    [HttpPost("regulators")]
    public async Task<IActionResult> CreateRegulator([FromBody] RegulatorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check for duplicate code
        if (await _context.RegulatorCatalogs.AnyAsync(r => r.Code == dto.RegulatorCode))
            return BadRequest(new { error = $"Regulator code '{dto.RegulatorCode}' already exists" });

        var regulator = new RegulatorCatalog
        {
            Id = Guid.NewGuid(),
            Code = dto.RegulatorCode,
            NameEn = dto.Name,
            NameAr = dto.NameAr ?? "",
            JurisdictionEn = dto.Description ?? "",
            Website = dto.Website ?? "",
            Category = dto.Type ?? "government",
            Sector = "all",
            RegionType = dto.Jurisdiction?.Contains("International") == true ? "international" : "saudi",
            DisplayOrder = 100,
            CreatedDate = DateTime.UtcNow
        };

        _context.RegulatorCatalogs.Add(regulator);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created regulator: {Code} - {Name}", regulator.Code, regulator.NameEn);
        return CreatedAtAction(nameof(GetRegulator), new { id = regulator.Id }, regulator);
    }

    /// <summary>
    /// Update regulator
    /// </summary>
    [HttpPut("regulators/{id}")]
    public async Task<IActionResult> UpdateRegulator(Guid id, [FromBody] RegulatorDto dto)
    {
        var regulator = await _context.RegulatorCatalogs.FindAsync(id);
        if (regulator == null)
            return NotFound(new { error = "Regulator not found" });

        // Check for duplicate code (excluding self)
        if (await _context.RegulatorCatalogs.AnyAsync(r => r.Code == dto.RegulatorCode && r.Id != id))
            return BadRequest(new { error = $"Regulator code '{dto.RegulatorCode}' already exists" });

        regulator.Code = dto.RegulatorCode;
        regulator.NameEn = dto.Name;
        regulator.NameAr = dto.NameAr ?? regulator.NameAr;
        regulator.JurisdictionEn = dto.Description ?? regulator.JurisdictionEn;
        regulator.Website = dto.Website ?? regulator.Website;
        regulator.Category = dto.Type ?? regulator.Category;
        regulator.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated regulator: {Code} - {Name}", regulator.Code, regulator.NameEn);
        return Ok(regulator);
    }

    /// <summary>
    /// Delete regulator
    /// </summary>
    [HttpDelete("regulators/{id}")]
    public async Task<IActionResult> DeleteRegulator(Guid id)
    {
        var regulator = await _context.RegulatorCatalogs.FindAsync(id);
        if (regulator == null)
            return NotFound(new { error = "Regulator not found" });

        _context.RegulatorCatalogs.Remove(regulator);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted regulator: {Code} - {Name}", regulator.Code, regulator.NameEn);
        return Ok(new { message = "Regulator deleted successfully" });
    }

    /// <summary>
    /// Export regulators to CSV
    /// </summary>
    [HttpGet("regulators/export")]
    public async Task<IActionResult> ExportRegulators([FromQuery] string? search = null)
    {
        var query = _context.RegulatorCatalogs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(r => r.NameEn.ToLower().Contains(search) || r.Code.ToLower().Contains(search));
        }

        var regulators = await query.OrderBy(r => r.DisplayOrder).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Code,Name,Name (Arabic),Jurisdiction,Category,Sector,Website,Region,Established");
        foreach (var r in regulators)
        {
            csv.AppendLine($"\"{r.Code}\",\"{r.NameEn}\",\"{r.NameAr}\",\"{r.JurisdictionEn}\",\"{r.Category}\",\"{r.Sector}\",\"{r.Website}\",\"{r.RegionType}\",\"{r.Established}\"");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"regulators_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    #endregion

    #region Frameworks CRUD

    /// <summary>
    /// Get all frameworks with search/filter
    /// </summary>
    [HttpGet("frameworks")]
    public async Task<IActionResult> GetFrameworks(
        [FromQuery] string? search = null,
        [FromQuery] Guid? regulatorId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.FrameworkCatalogs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(f => 
                f.TitleEn.ToLower().Contains(search) || 
                f.TitleAr.Contains(search) || 
                f.Code.ToLower().Contains(search));
        }

        if (regulatorId.HasValue)
            query = query.Where(f => f.RegulatorId == regulatorId.Value);

        var totalCount = await query.CountAsync();
        
        // Get control counts
        var frameworkCodes = await query.Select(f => f.Code).ToListAsync();
        var controlCounts = await _context.FrameworkControls
            .Where(c => frameworkCodes.Contains(c.FrameworkCode))
            .GroupBy(c => c.FrameworkCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Code, x => x.Count);

        var frameworks = await query
            .Include(f => f.Regulator)
            .OrderBy(f => f.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = frameworks.Select(f => new {
            f.Id,
            f.Code,
            name = f.TitleEn,
            nameAr = f.TitleAr,
            f.Version,
            f.DescriptionEn,
            f.DescriptionAr,
            f.RegulatorId,
            regulatorCode = f.Regulator?.Code ?? "",
            jurisdiction = f.Regulator?.JurisdictionEn ?? "Saudi Arabia",
            f.Category,
            f.IsActive,
            controlCount = controlCounts.GetValueOrDefault(f.Code, 0),
            f.CreatedDate
        }).ToList();

        return Ok(new {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            data = result
        });
    }

    /// <summary>
    /// Get single framework by ID
    /// </summary>
    [HttpGet("frameworks/{id}")]
    public async Task<IActionResult> GetFramework(Guid id)
    {
        var framework = await _context.FrameworkCatalogs.Include(f => f.Regulator).FirstOrDefaultAsync(f => f.Id == id);
        if (framework == null)
            return NotFound(new { error = "Framework not found" });

        var controlCount = await _context.FrameworkControls.CountAsync(c => c.FrameworkCode == framework.Code);

        return Ok(new {
            framework.Id,
            framework.Code,
            name = framework.TitleEn,
            nameAr = framework.TitleAr,
            framework.Version,
            framework.DescriptionEn,
            framework.DescriptionAr,
            framework.RegulatorId,
            regulatorCode = framework.Regulator?.Code ?? "",
            jurisdiction = framework.Regulator?.JurisdictionEn ?? "Saudi Arabia",
            framework.Category,
            framework.IsActive,
            controlCount
        });
    }

    /// <summary>
    /// Create new framework
    /// </summary>
    [HttpPost("frameworks")]
    public async Task<IActionResult> CreateFramework([FromBody] FrameworkDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _context.FrameworkCatalogs.AnyAsync(f => f.Code == dto.Code))
            return BadRequest(new { error = $"Framework code '{dto.Code}' already exists" });

        var framework = new FrameworkCatalog
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            TitleEn = dto.Name,
            TitleAr = dto.NameAr ?? "",
            Version = dto.Version ?? "1.0",
            DescriptionEn = dto.DescriptionEn ?? "",
            DescriptionAr = dto.DescriptionAr ?? "",
            RegulatorId = dto.RegulatorId,
            Category = dto.Category ?? "Regulatory",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.FrameworkCatalogs.Add(framework);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created framework: {Code} - {Name}", framework.Code, framework.TitleEn);
        return CreatedAtAction(nameof(GetFramework), new { id = framework.Id }, framework);
    }

    /// <summary>
    /// Update framework
    /// </summary>
    [HttpPut("frameworks/{id}")]
    public async Task<IActionResult> UpdateFramework(Guid id, [FromBody] FrameworkDto dto)
    {
        var framework = await _context.FrameworkCatalogs.FindAsync(id);
        if (framework == null)
            return NotFound(new { error = "Framework not found" });

        if (await _context.FrameworkCatalogs.AnyAsync(f => f.Code == dto.Code && f.Id != id))
            return BadRequest(new { error = $"Framework code '{dto.Code}' already exists" });

        framework.Code = dto.Code;
        framework.TitleEn = dto.Name;
        framework.TitleAr = dto.NameAr ?? framework.TitleAr;
        framework.Version = dto.Version ?? framework.Version;
        framework.DescriptionEn = dto.DescriptionEn ?? framework.DescriptionEn;
        framework.DescriptionAr = dto.DescriptionAr ?? framework.DescriptionAr;
        framework.RegulatorId = dto.RegulatorId;
        framework.Category = dto.Category ?? framework.Category;
        framework.IsActive = dto.IsActive;
        framework.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated framework: {Code} - {Name}", framework.Code, framework.TitleEn);
        return Ok(framework);
    }

    /// <summary>
    /// Delete framework
    /// </summary>
    [HttpDelete("frameworks/{id}")]
    public async Task<IActionResult> DeleteFramework(Guid id)
    {
        var framework = await _context.FrameworkCatalogs.FindAsync(id);
        if (framework == null)
            return NotFound(new { error = "Framework not found" });

        // Check if controls exist
        var controlCount = await _context.FrameworkControls.CountAsync(c => c.FrameworkCode == framework.Code);
        if (controlCount > 0)
            return BadRequest(new { error = $"Cannot delete framework with {controlCount} controls. Delete controls first." });

        _context.FrameworkCatalogs.Remove(framework);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted framework: {Code} - {Name}", framework.Code, framework.TitleEn);
        return Ok(new { message = "Framework deleted successfully" });
    }

    /// <summary>
    /// Export frameworks to CSV
    /// </summary>
    [HttpGet("frameworks/export")]
    public async Task<IActionResult> ExportFrameworks([FromQuery] string? search = null)
    {
        var query = _context.FrameworkCatalogs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(f => f.TitleEn.ToLower().Contains(search) || f.Code.ToLower().Contains(search));
        }

        var frameworks = await query.Include(f => f.Regulator).OrderBy(f => f.Code).ToListAsync();
        var controlCounts = await _context.FrameworkControls
            .GroupBy(c => c.FrameworkCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Code, x => x.Count);

        var csv = new StringBuilder();
        csv.AppendLine("Code,Name,Name (Arabic),Version,Category,Regulator,Controls,Active");
        foreach (var f in frameworks)
        {
            csv.AppendLine($"\"{f.Code}\",\"{f.TitleEn}\",\"{f.TitleAr}\",\"{f.Version}\",\"{f.Category}\",\"{f.Regulator?.Code ?? ""}\",\"{controlCounts.GetValueOrDefault(f.Code, 0)}\",\"{f.IsActive}\"");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"frameworks_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    #endregion

    #region Controls CRUD

    /// <summary>
    /// Get all controls with search/filter
    /// </summary>
    [HttpGet("controls")]
    public async Task<IActionResult> GetControls(
        [FromQuery] string? search = null,
        [FromQuery] string? frameworkCode = null,
        [FromQuery] string? domain = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.FrameworkControls.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(c => 
                c.TitleEn.ToLower().Contains(search) || 
                c.TitleAr.Contains(search) || 
                c.ControlNumber.ToLower().Contains(search) ||
                c.RequirementEn.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(frameworkCode))
            query = query.Where(c => c.FrameworkCode == frameworkCode);

        if (!string.IsNullOrEmpty(domain))
            query = query.Where(c => c.Domain.Contains(domain));

        var totalCount = await query.CountAsync();
        var controls = await query
            .OrderBy(c => c.FrameworkCode)
            .ThenBy(c => c.ControlNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new {
                c.Id,
                c.ControlNumber,
                c.FrameworkCode,
                c.Version,
                c.Domain,
                c.TitleEn,
                c.TitleAr,
                c.ControlType,
                c.MaturityLevel,
                c.CreatedDate
            })
            .ToListAsync();

        // Get unique domains for filter
        var domains = await _context.FrameworkControls
            .Select(c => c.Domain)
            .Distinct()
            .Where(d => !string.IsNullOrEmpty(d))
            .OrderBy(d => d)
            .ToListAsync();

        return Ok(new {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            domains,
            data = controls
        });
    }

    /// <summary>
    /// Get single control by ID with full details
    /// </summary>
    [HttpGet("controls/{id}")]
    public async Task<IActionResult> GetControl(Guid id)
    {
        var control = await _context.FrameworkControls.FindAsync(id);
        if (control == null)
            return NotFound(new { error = "Control not found" });

        return Ok(new {
            control.Id,
            control.ControlNumber,
            control.FrameworkCode,
            control.Version,
            control.Domain,
            control.TitleEn,
            control.TitleAr,
            control.RequirementEn,
            control.RequirementAr,
            control.ControlType,
            control.MaturityLevel,
            control.ImplementationGuidanceEn,
            control.EvidenceRequirements,
            control.MappingIso27001,
            control.MappingNist,
            control.CreatedDate,
            control.ModifiedDate
        });
    }

    /// <summary>
    /// Create new control
    /// </summary>
    [HttpPost("controls")]
    public async Task<IActionResult> CreateControl([FromBody] ControlDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check for duplicate
        if (await _context.FrameworkControls.AnyAsync(c => c.FrameworkCode == dto.FrameworkCode && c.ControlNumber == dto.ControlNumber))
            return BadRequest(new { error = $"Control '{dto.ControlNumber}' already exists in framework '{dto.FrameworkCode}'" });

        var control = new FrameworkControl
        {
            Id = Guid.NewGuid(),
            FrameworkCode = dto.FrameworkCode,
            ControlNumber = dto.ControlNumber,
            Version = dto.Version ?? "1.0",
            Domain = dto.Domain ?? "",
            TitleEn = dto.TitleEn,
            TitleAr = dto.TitleAr ?? "",
            RequirementEn = dto.RequirementEn ?? "",
            RequirementAr = dto.RequirementAr ?? "",
            ControlType = dto.ControlType ?? "Preventive",
            MaturityLevel = dto.MaturityLevel,
            ImplementationGuidanceEn = dto.ImplementationGuidanceEn ?? "",
            EvidenceRequirements = dto.EvidenceRequirements ?? "",
            MappingIso27001 = dto.MappingIso27001 ?? "",
            MappingNist = dto.MappingNist ?? "",
            CreatedDate = DateTime.UtcNow
        };

        _context.FrameworkControls.Add(control);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created control: {Framework} - {Number}", control.FrameworkCode, control.ControlNumber);
        return CreatedAtAction(nameof(GetControl), new { id = control.Id }, control);
    }

    /// <summary>
    /// Update control
    /// </summary>
    [HttpPut("controls/{id}")]
    public async Task<IActionResult> UpdateControl(Guid id, [FromBody] ControlDto dto)
    {
        var control = await _context.FrameworkControls.FindAsync(id);
        if (control == null)
            return NotFound(new { error = "Control not found" });

        // Check for duplicate (excluding self)
        if (await _context.FrameworkControls.AnyAsync(c => c.FrameworkCode == dto.FrameworkCode && c.ControlNumber == dto.ControlNumber && c.Id != id))
            return BadRequest(new { error = $"Control '{dto.ControlNumber}' already exists in framework '{dto.FrameworkCode}'" });

        control.FrameworkCode = dto.FrameworkCode;
        control.ControlNumber = dto.ControlNumber;
        control.Version = dto.Version ?? control.Version;
        control.Domain = dto.Domain ?? control.Domain;
        control.TitleEn = dto.TitleEn;
        control.TitleAr = dto.TitleAr ?? control.TitleAr;
        control.RequirementEn = dto.RequirementEn ?? control.RequirementEn;
        control.RequirementAr = dto.RequirementAr ?? control.RequirementAr;
        control.ControlType = dto.ControlType ?? control.ControlType;
        control.MaturityLevel = dto.MaturityLevel;
        control.ImplementationGuidanceEn = dto.ImplementationGuidanceEn ?? control.ImplementationGuidanceEn;
        control.EvidenceRequirements = dto.EvidenceRequirements ?? control.EvidenceRequirements;
        control.MappingIso27001 = dto.MappingIso27001 ?? control.MappingIso27001;
        control.MappingNist = dto.MappingNist ?? control.MappingNist;
        control.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated control: {Framework} - {Number}", control.FrameworkCode, control.ControlNumber);
        return Ok(control);
    }

    /// <summary>
    /// Delete control
    /// </summary>
    [HttpDelete("controls/{id}")]
    public async Task<IActionResult> DeleteControl(Guid id)
    {
        var control = await _context.FrameworkControls.FindAsync(id);
        if (control == null)
            return NotFound(new { error = "Control not found" });

        _context.FrameworkControls.Remove(control);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted control: {Framework} - {Number}", control.FrameworkCode, control.ControlNumber);
        return Ok(new { message = "Control deleted successfully" });
    }

    /// <summary>
    /// Export controls to CSV
    /// </summary>
    [HttpGet("controls/export")]
    public async Task<IActionResult> ExportControls([FromQuery] string? frameworkCode = null, [FromQuery] string? search = null)
    {
        var query = _context.FrameworkControls.AsQueryable();

        if (!string.IsNullOrEmpty(frameworkCode))
            query = query.Where(c => c.FrameworkCode == frameworkCode);

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(c => c.TitleEn.ToLower().Contains(search) || c.ControlNumber.ToLower().Contains(search));
        }

        var controls = await query.OrderBy(c => c.FrameworkCode).ThenBy(c => c.ControlNumber).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Framework,Control Number,Domain,Title (EN),Title (AR),Type,Maturity,Requirement (EN),ISO 27001 Mapping,NIST Mapping");
        foreach (var c in controls)
        {
            csv.AppendLine($"\"{c.FrameworkCode}\",\"{c.ControlNumber}\",\"{c.Domain}\",\"{EscapeCsv(c.TitleEn)}\",\"{EscapeCsv(c.TitleAr)}\",\"{c.ControlType}\",\"{c.MaturityLevel}\",\"{EscapeCsv(c.RequirementEn)}\",\"{c.MappingIso27001}\",\"{c.MappingNist}\"");
        }

        var filename = string.IsNullOrEmpty(frameworkCode) 
            ? $"all_controls_{DateTime.UtcNow:yyyyMMdd}.csv" 
            : $"controls_{frameworkCode}_{DateTime.UtcNow:yyyyMMdd}.csv";

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
    }

    #endregion

    #region GOSI Sectors Export

    /// <summary>
    /// Export GOSI sector mappings to CSV
    /// </summary>
    [HttpGet("gosi-sectors/export")]
    public async Task<IActionResult> ExportGosiSectors([FromQuery] string? mainSector = null)
    {
        var query = _context.GrcSubSectorMappings.AsQueryable();

        if (!string.IsNullOrEmpty(mainSector))
            query = query.Where(m => m.MainSectorCode == mainSector.ToUpper());

        var mappings = await query.OrderBy(m => m.MainSectorCode).ThenBy(m => m.DisplayOrder).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("ISIC Section,GOSI Code,Sub-Sector (EN),Sub-Sector (AR),Main Sector,Main Sector (EN),Main Sector (AR),Primary Regulator,Notes");
        foreach (var m in mappings)
        {
            csv.AppendLine($"\"{m.IsicSection}\",\"{m.GosiCode}\",\"{EscapeCsv(m.SubSectorNameEn)}\",\"{EscapeCsv(m.SubSectorNameAr)}\",\"{m.MainSectorCode}\",\"{m.MainSectorNameEn}\",\"{m.MainSectorNameAr}\",\"{m.PrimaryRegulator}\",\"{EscapeCsv(m.RegulatoryNotes ?? "")}\"");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"gosi_sectors_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    #endregion

    #region Helpers

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
    }

    #endregion
}

#region DTOs

public class RegulatorDto
{
    public string RegulatorCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Type { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsPrimary { get; set; }
}

public class FrameworkDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Version { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public Guid? RegulatorId { get; set; }
    public string? RegulatorCode { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ControlDto
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string ControlNumber { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? Domain { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string? RequirementEn { get; set; }
    public string? RequirementAr { get; set; }
    public string? ControlType { get; set; }
    public int MaturityLevel { get; set; } = 1;
    public string? ImplementationGuidanceEn { get; set; }
    public string? EvidenceRequirements { get; set; }
    public string? MappingIso27001 { get; set; }
    public string? MappingNist { get; set; }
}

#endregion
