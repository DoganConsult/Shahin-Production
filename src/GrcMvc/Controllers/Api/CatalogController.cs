using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for Global Catalogs - Layer 1 (Platform)
    /// Provides endpoints for Regulators, Frameworks, Controls, and other catalog entities
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class CatalogController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<CatalogController> _logger;
        private readonly IWebHostEnvironment _environment;

        public CatalogController(
            GrcDbContext context,
            ILogger<CatalogController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        #region Regulators

        /// <summary>
        /// Get all regulators
        /// </summary>
        [HttpGet("regulators")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegulators(
            [FromQuery] string? region = null,
            [FromQuery] string? category = null,
            [FromQuery] string? sector = null)
        {
            try
            {
                var query = _context.RegulatorCatalogs.AsQueryable();

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(r => r.RegionType == region);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(r => r.Category == category);

                if (!string.IsNullOrEmpty(sector))
                    query = query.Where(r => r.Sector == sector);

                var regulators = await query
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.DisplayOrder)
                    .Select(r => new
                    {
                        r.Id,
                        r.Code,
                        r.NameEn,
                        r.NameAr,
                        r.JurisdictionEn,
                        r.Website,
                        r.Category,
                        r.Sector,
                        r.RegionType,
                        r.Established,
                        FrameworkCount = r.Frameworks.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total = regulators.Count,
                    regulators
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regulators");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get regulator by code
        /// </summary>
        [HttpGet("regulators/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegulatorByCode(string code)
        {
            try
            {
                var regulator = await _context.RegulatorCatalogs
                    .Include(r => r.Frameworks.Where(f => f.IsActive))
                    .FirstOrDefaultAsync(r => r.Code == code && r.IsActive);

                if (regulator == null)
                    return NotFound(new { error = $"Regulator '{code}' not found" });

                return Ok(new
                {
                    regulator.Id,
                    regulator.Code,
                    regulator.NameEn,
                    regulator.NameAr,
                    regulator.JurisdictionEn,
                    regulator.Website,
                    regulator.Category,
                    regulator.Sector,
                    regulator.RegionType,
                    regulator.Established,
                    Frameworks = regulator.Frameworks.Select(f => new
                    {
                        f.Id,
                        f.Code,
                        f.Version,
                        f.TitleEn,
                        f.TitleAr,
                        f.Category,
                        f.IsMandatory,
                        f.ControlCount
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regulator by code");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get regulator statistics by region
        /// </summary>
        [HttpGet("regulators/statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegulatorStatistics()
        {
            try
            {
                var stats = await _context.RegulatorCatalogs
                    .Where(r => r.IsActive)
                    .GroupBy(r => r.RegionType)
                    .Select(g => new
                    {
                        Region = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var categoryStats = await _context.RegulatorCatalogs
                    .Where(r => r.IsActive)
                    .GroupBy(r => r.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total = await _context.RegulatorCatalogs.CountAsync(r => r.IsActive),
                    byRegion = stats,
                    byCategory = categoryStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regulator statistics");
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Frameworks

        /// <summary>
        /// Get all frameworks
        /// </summary>
        [HttpGet("frameworks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFrameworks(
            [FromQuery] string? regulator = null,
            [FromQuery] string? category = null,
            [FromQuery] bool? mandatory = null)
        {
            try
            {
                var query = _context.FrameworkCatalogs
                    .Include(f => f.Regulator)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(regulator))
                    query = query.Where(f => f.Regulator != null && f.Regulator.Code == regulator);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(f => f.Category == category);

                if (mandatory.HasValue)
                    query = query.Where(f => f.IsMandatory == mandatory.Value);

                var frameworks = await query
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new
                    {
                        f.Id,
                        f.Code,
                        f.Version,
                        f.TitleEn,
                        f.TitleAr,
                        RegulatorCode = f.Regulator != null ? f.Regulator.Code : null,
                        RegulatorName = f.Regulator != null ? f.Regulator.NameEn : null,
                        f.Category,
                        f.IsMandatory,
                        f.ControlCount,
                        f.Domains,
                        f.Status
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total = frameworks.Count,
                    frameworks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting frameworks");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get framework by code
        /// </summary>
        [HttpGet("frameworks/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFrameworkByCode(string code, [FromQuery] string? version = null)
        {
            try
            {
                var query = _context.FrameworkCatalogs
                    .Include(f => f.Regulator)
                    .Include(f => f.Controls.Where(c => c.IsActive))
                    .Where(f => f.Code == code && f.IsActive);

                if (!string.IsNullOrEmpty(version))
                    query = query.Where(f => f.Version == version);

                var framework = await query.FirstOrDefaultAsync();

                if (framework == null)
                    return NotFound(new { error = $"Framework '{code}' not found" });

                return Ok(new
                {
                    framework.Id,
                    framework.Code,
                    framework.Version,
                    framework.TitleEn,
                    framework.TitleAr,
                    framework.DescriptionEn,
                    framework.DescriptionAr,
                    RegulatorCode = framework.Regulator?.Code,
                    RegulatorName = framework.Regulator?.NameEn,
                    framework.Category,
                    framework.IsMandatory,
                    framework.ControlCount,
                    framework.Domains,
                    framework.EffectiveDate,
                    framework.Status,
                    Controls = framework.Controls.Select(c => new
                    {
                        c.Id,
                        c.ControlId,
                        c.ControlNumber,
                        c.Domain,
                        c.TitleEn,
                        c.ControlType,
                        c.MaturityLevel
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting framework by code");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get framework statistics
        /// </summary>
        [HttpGet("frameworks/statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFrameworkStatistics()
        {
            try
            {
                var byCategoryStats = await _context.FrameworkCatalogs
                    .Where(f => f.IsActive)
                    .GroupBy(f => f.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        TotalControls = g.Sum(f => f.ControlCount)
                    })
                    .ToListAsync();

                var byRegulatorStats = await _context.FrameworkCatalogs
                    .Where(f => f.IsActive && f.Regulator != null)
                    .GroupBy(f => f.Regulator!.Code)
                    .Select(g => new
                    {
                        Regulator = g.Key,
                        Count = g.Count(),
                        TotalControls = g.Sum(f => f.ControlCount)
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(20)
                    .ToListAsync();

                var mandatoryCount = await _context.FrameworkCatalogs.CountAsync(f => f.IsActive && f.IsMandatory);
                var voluntaryCount = await _context.FrameworkCatalogs.CountAsync(f => f.IsActive && !f.IsMandatory);

                return Ok(new
                {
                    total = await _context.FrameworkCatalogs.CountAsync(f => f.IsActive),
                    mandatory = mandatoryCount,
                    voluntary = voluntaryCount,
                    totalControls = await _context.FrameworkCatalogs.Where(f => f.IsActive).SumAsync(f => f.ControlCount),
                    byCategory = byCategoryStats,
                    topRegulators = byRegulatorStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting framework statistics");
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Controls

        /// <summary>
        /// Get controls by framework
        /// </summary>
        [HttpGet("controls")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControls(
            [FromQuery] string? framework = null,
            [FromQuery] string? domain = null,
            [FromQuery] string? controlType = null,
            [FromQuery] int? maturityLevel = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.ControlCatalogs
                    .Include(c => c.Framework)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(framework))
                    query = query.Where(c => c.Framework.Code == framework);

                if (!string.IsNullOrEmpty(domain))
                    query = query.Where(c => c.Domain.Contains(domain));

                if (!string.IsNullOrEmpty(controlType))
                    query = query.Where(c => c.ControlType == controlType);

                if (maturityLevel.HasValue)
                    query = query.Where(c => c.MaturityLevel == maturityLevel.Value);

                var total = await query.CountAsync(c => c.IsActive);

                var controls = await query
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.ControlId,
                        c.ControlNumber,
                        FrameworkCode = c.Framework.Code,
                        c.Domain,
                        c.Subdomain,
                        c.TitleEn,
                        c.TitleAr,
                        c.ControlType,
                        c.MaturityLevel,
                        c.MappingIso27001,
                        c.MappingNistCsf
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    controls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controls");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get control by ID
        /// </summary>
        [HttpGet("controls/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControlById(Guid id)
        {
            try
            {
                var control = await _context.ControlCatalogs
                    .Include(c => c.Framework)
                    .ThenInclude(f => f.Regulator)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (control == null)
                    return NotFound(new { error = "Control not found" });

                return Ok(new
                {
                    control.Id,
                    control.ControlId,
                    control.ControlNumber,
                    control.Version,
                    FrameworkCode = control.Framework.Code,
                    FrameworkTitle = control.Framework.TitleEn,
                    RegulatorCode = control.Framework.Regulator?.Code,
                    control.Domain,
                    control.Subdomain,
                    control.TitleEn,
                    control.TitleAr,
                    control.RequirementEn,
                    control.RequirementAr,
                    control.ControlType,
                    control.MaturityLevel,
                    control.ImplementationGuidanceEn,
                    control.EvidenceRequirements,
                    control.MappingIso27001,
                    control.MappingNistCsf,
                    control.MappingOther,
                    control.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting control by ID");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Search controls
        /// </summary>
        [HttpGet("controls/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchControls([FromQuery] string q, [FromQuery] int limit = 50)
        {
            if (string.IsNullOrEmpty(q))
                return BadRequest(new { error = "Search query 'q' is required" });

            try
            {
                var searchTerm = q.ToLower();
                var controls = await _context.ControlCatalogs
                    .Include(c => c.Framework)
                    .Where(c => c.IsActive &&
                        (c.ControlNumber.ToLower().Contains(searchTerm) ||
                         c.TitleEn.ToLower().Contains(searchTerm) ||
                         c.TitleAr.Contains(q) ||
                         c.Domain.ToLower().Contains(searchTerm) ||
                         c.RequirementEn.ToLower().Contains(searchTerm)))
                    .Take(limit)
                    .Select(c => new
                    {
                        c.Id,
                        c.ControlId,
                        c.ControlNumber,
                        FrameworkCode = c.Framework.Code,
                        c.Domain,
                        c.TitleEn,
                        c.TitleAr,
                        c.ControlType,
                        c.MaturityLevel
                    })
                    .ToListAsync();

                return Ok(new
                {
                    query = q,
                    count = controls.Count,
                    controls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching controls");
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Import

        /// <summary>
        /// Import regulators from CSV file
        /// </summary>
        [HttpPost("regulators/import")]
        [AllowAnonymous]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> ImportRegulators(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var imported = 0;
                var updated = 0;
                var errors = new List<string>();

                foreach (var line in lines.Skip(1)) // Skip header
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var fields = ParseCsvLine(line);
                    if (fields.Length < 8)
                    {
                        errors.Add($"Invalid line: {line.Substring(0, Math.Min(50, line.Length))}...");
                        continue;
                    }

                    var code = fields[0].Trim();
                    var existing = await _context.RegulatorCatalogs.FirstOrDefaultAsync(r => r.Code == code);

                    if (existing != null)
                    {
                        existing.NameAr = fields[1].Trim();
                        existing.NameEn = fields[2].Trim();
                        existing.JurisdictionEn = fields[3].Trim();
                        existing.Website = fields[4].Trim();
                        existing.Category = fields[5].Trim();
                        existing.Sector = fields[6].Trim();
                        existing.Established = int.TryParse(fields[7].Trim(), out var year) ? year : null;
                        existing.ModifiedDate = DateTime.UtcNow;
                        updated++;
                    }
                    else
                    {
                        _context.RegulatorCatalogs.Add(new RegulatorCatalog
                        {
                            Id = Guid.NewGuid(),
                            Code = code,
                            NameAr = fields[1].Trim(),
                            NameEn = fields[2].Trim(),
                            JurisdictionEn = fields[3].Trim(),
                            Website = fields[4].Trim(),
                            Category = fields[5].Trim(),
                            Sector = fields[6].Trim(),
                            Established = int.TryParse(fields[7].Trim(), out var yr) ? yr : null,
                            RegionType = DetermineRegionType(code),
                            IsActive = true,
                            DisplayOrder = imported + 1,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = "API Import"
                        });
                        imported++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    imported,
                    updated,
                    errors = errors.Count > 0 ? errors : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing regulators");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Import frameworks from CSV file
        /// </summary>
        [HttpPost("frameworks/import")]
        [AllowAnonymous]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> ImportFrameworks(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var regulatorLookup = await _context.RegulatorCatalogs
                    .ToDictionaryAsync(r => r.Code, r => r.Id);

                var imported = 0;
                var updated = 0;
                var errors = new List<string>();

                foreach (var line in lines.Skip(1)) // Skip header
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var fields = ParseCsvLine(line);
                    if (fields.Length < 9)
                    {
                        errors.Add($"Invalid line: {line.Substring(0, Math.Min(50, line.Length))}...");
                        continue;
                    }

                    var code = fields[0].Trim();
                    var version = fields[1].Trim();
                    var existing = await _context.FrameworkCatalogs
                        .FirstOrDefaultAsync(f => f.Code == code && f.Version == version);

                    var regulatorCode = fields[4].Trim();
                    Guid? regulatorId = regulatorLookup.TryGetValue(regulatorCode, out var id) ? id : null;

                    if (existing != null)
                    {
                        existing.TitleEn = fields[2].Trim();
                        existing.TitleAr = fields[3].Trim();
                        existing.RegulatorId = regulatorId;
                        existing.Category = fields[5].Trim();
                        existing.IsMandatory = bool.TryParse(fields[6].Trim(), out var mandatory) && mandatory;
                        existing.ControlCount = int.TryParse(fields[7].Trim(), out var controls) ? controls : 0;
                        existing.Domains = fields[8].Trim();
                        existing.ModifiedDate = DateTime.UtcNow;
                        updated++;
                    }
                    else
                    {
                        _context.FrameworkCatalogs.Add(new FrameworkCatalog
                        {
                            Id = Guid.NewGuid(),
                            Code = code,
                            Version = version,
                            TitleEn = fields[2].Trim(),
                            TitleAr = fields[3].Trim(),
                            RegulatorId = regulatorId,
                            Category = fields[5].Trim(),
                            IsMandatory = bool.TryParse(fields[6].Trim(), out var mand) && mand,
                            ControlCount = int.TryParse(fields[7].Trim(), out var ctrl) ? ctrl : 0,
                            Domains = fields[8].Trim(),
                            Status = "Active",
                            IsActive = true,
                            DisplayOrder = imported + 1,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = "API Import"
                        });
                        imported++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    imported,
                    updated,
                    errors = errors.Count > 0 ? errors : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing frameworks");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed all catalogs from CSV files in the SeedData folder
        /// </summary>
        [HttpPost("seed")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedCatalogs()
        {
            try
            {
                var seedDataPath = Path.Combine(_environment.ContentRootPath, "Data", "SeedData");
                var seeder = new CatalogCsvSeeder(_context, _logger, seedDataPath);
                await seeder.SeedAllCatalogsAsync();

                return Ok(new
                {
                    success = true,
                    message = "Catalog seeding completed",
                    regulators = await _context.RegulatorCatalogs.CountAsync(),
                    frameworks = await _context.FrameworkCatalogs.CountAsync(),
                    controls = await _context.ControlCatalogs.CountAsync()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding catalogs");
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get overall catalog statistics
        /// </summary>
        [HttpGet("statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = new
                {
                    regulators = new
                    {
                        total = await _context.RegulatorCatalogs.CountAsync(r => r.IsActive),
                        saudi = await _context.RegulatorCatalogs.CountAsync(r => r.IsActive && r.RegionType == "saudi"),
                        international = await _context.RegulatorCatalogs.CountAsync(r => r.IsActive && r.RegionType == "international"),
                        regional = await _context.RegulatorCatalogs.CountAsync(r => r.IsActive && r.RegionType == "regional")
                    },
                    frameworks = new
                    {
                        total = await _context.FrameworkCatalogs.CountAsync(f => f.IsActive),
                        mandatory = await _context.FrameworkCatalogs.CountAsync(f => f.IsActive && f.IsMandatory),
                        voluntary = await _context.FrameworkCatalogs.CountAsync(f => f.IsActive && !f.IsMandatory),
                        totalControls = await _context.FrameworkCatalogs.Where(f => f.IsActive).SumAsync(f => f.ControlCount)
                    },
                    controls = new
                    {
                        total = await _context.ControlCatalogs.CountAsync(c => c.IsActive)
                    },
                    roles = await _context.RoleCatalogs.CountAsync(r => r.IsActive),
                    titles = await _context.TitleCatalogs.CountAsync(t => t.IsActive),
                    baselines = await _context.BaselineCatalogs.CountAsync(b => b.IsActive),
                    packages = await _context.PackageCatalogs.CountAsync(p => p.IsActive),
                    templates = await _context.TemplateCatalogs.CountAsync(t => t.IsActive),
                    evidenceTypes = await _context.EvidenceTypeCatalogs.CountAsync(e => e.IsActive)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting catalog statistics");
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Helpers

        private static string DetermineRegionType(string code)
        {
            var internationalCodes = new HashSet<string>
            {
                "ISO", "IEC", "NIST", "PCI-SSC", "AICPA", "ISACA", "EU-GDPR", "HHS-OCR",
                "HITRUST", "CIS", "OWASP", "CSA", "SWIFT", "BASEL", "FATF", "IIA",
                "COSO", "ITIL", "PMI", "TOGAF", "NCSC-UK", "ENISA"
            };

            var regionalCodes = new HashSet<string>
            {
                "CBUAE", "ADGM", "DIFC", "CBB", "CBO", "QCB", "CBK", "NESA"
            };

            if (internationalCodes.Contains(code)) return "international";
            if (regionalCodes.Contains(code)) return "regional";
            return "saudi";
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var currentField = new System.Text.StringBuilder();

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }

        #endregion
    }
}
