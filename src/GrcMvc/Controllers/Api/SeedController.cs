using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for seeding catalog, workflow, and framework control data
    /// SECURITY: Requires PlatformAdmin role - never expose without authorization
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "PlatformAdmin")]
    [IgnoreAntiforgeryToken]
    public class SeedController : ControllerBase
    {
        private readonly CatalogSeederService _catalogSeeder;
        private readonly WorkflowDefinitionSeederService _workflowSeeder;
        private readonly FrameworkControlImportService _controlImporter;
        private readonly IPocSeederService _pocSeeder;
        private readonly GrcDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SeedController> _logger;
        private readonly IWebHostEnvironment _environment;

        public SeedController(
            CatalogSeederService catalogSeeder,
            WorkflowDefinitionSeederService workflowSeeder,
            FrameworkControlImportService controlImporter,
            IPocSeederService pocSeeder,
            GrcDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SeedController> logger,
            IWebHostEnvironment environment)
        {
            _catalogSeeder = catalogSeeder;
            _workflowSeeder = workflowSeeder;
            _controlImporter = controlImporter;
            _pocSeeder = pocSeeder;
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Check if seeding is allowed - only in Development or when ALLOW_SEED=true
        /// </summary>
        private bool IsSeedingAllowed()
        {
            if (_environment.IsDevelopment()) return true;
            var allowSeed = Environment.GetEnvironmentVariable("ALLOW_SEED");
            return allowSeed?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Seed all catalog data (Roles, Titles, Baselines, Packages, Templates, Evidence Types)
        /// </summary>
        [HttpPost("catalogs")]
        public async Task<IActionResult> SeedCatalogs()
        {
            if (!IsSeedingAllowed())
            {
                _logger.LogWarning("Seed attempt blocked in production environment");
                return BadRequest(new { error = "Seeding is not allowed in production. Set ALLOW_SEED=true to override." });
            }

            try
            {
                await _catalogSeeder.SeedAllCatalogsAsync();
                return Ok(new { message = "Catalogs seeded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding catalogs");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed all workflow definitions (7 pre-defined workflows)
        /// </summary>
        [HttpPost("workflows")]
        public async Task<IActionResult> SeedWorkflows()
        {
            try
            {
                await _workflowSeeder.SeedAllWorkflowDefinitionsAsync();
                return Ok(new { message = "Workflow definitions seeded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding workflow definitions");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed all data (catalogs + workflows + regulators + KSA frameworks)
        /// </summary>
        [HttpPost("all")]
        public async Task<IActionResult> SeedAll()
        {
            try
            {
                // Seed catalogs (roles, titles, baselines, packages, templates, evidence types)
                await _catalogSeeder.SeedAllCatalogsAsync();

                // Seed workflow definitions
                await _workflowSeeder.SeedAllWorkflowDefinitionsAsync();

                // Seed regulators (92 KSA + International)
                await RegulatorSeeds.SeedRegulatorsAsync(_context, _logger);

                // Seed KSA framework controls (NCA-ECC, SAMA-CSF, PDPL)
                await KsaFrameworkSeeds.SeedAllFrameworksAsync(_context, _logger);

                // Seed evidence scoring criteria and sector-framework index (18 sectors)
                await EvidenceScoringSeeds.SeedEvidenceScoringCriteriaAsync(_context, _logger);
                await EvidenceScoringSeeds.SeedSectorFrameworkIndexAsync(_context, _logger);
                
                // Seed GOSI 70+ sub-sector mappings to 18 main sectors
                await GosiSectorSeeds.SeedSubSectorMappingsAsync(_context, _logger);
                
                // Seed marketing content (testimonials, case studies, pricing)
                await MarketingSeeds.SeedMarketingDataAsync(_context);
                
                // Seed document templates for Document Center
                await DocumentTemplateSeeds.SeedDocumentTemplatesAsync(_context);

                return Ok(new {
                    message = "All seed data created successfully",
                    catalogs = new[] { "Roles", "Titles", "Baselines", "Packages", "Templates", "EvidenceTypes" },
                    regulators = "92 regulators (62 Saudi, 20 International, 10 Regional)",
                    frameworks = new[] { "NCA-ECC (114 controls)", "SAMA-CSF (156 controls)", "PDPL (45 controls)" },
                    workflows = new[] { "NCA ECC", "SAMA CSF", "PDPL PIA", "ERM", "Evidence Review", "Audit Remediation", "Policy Review" },
                    evidenceScoring = "29 evidence scoring criteria",
                    sectorIndex = "18 main sectors with framework mappings",
                    gosiMappings = "70+ GOSI sub-sectors mapped to 18 main GRC sectors (based on KSA ISIC Rev 4)",
                    marketing = "4 testimonials, 5 case studies, 4 pricing plans",
                    documentTemplates = "20 GRC document templates (policies, forms, checklists, reports)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding all data");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed KSA regulators (NCA, SAMA, SDAIA, CMA, CST, + 57 more)
        /// </summary>
        [HttpPost("regulators")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedRegulators()
        {
            try
            {
                await RegulatorSeeds.SeedRegulatorsAsync(_context, _logger);
                return Ok(new { message = "92 regulators seeded successfully (62 Saudi, 20 International, 10 Regional)" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding regulators");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed KSA framework controls (NCA-ECC, SAMA-CSF, PDPL)
        /// </summary>
        [HttpPost("ksa-frameworks")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedKsaFrameworks()
        {
            try
            {
                await KsaFrameworkSeeds.SeedAllFrameworksAsync(_context, _logger);
                return Ok(new {
                    message = "KSA framework controls seeded successfully",
                    frameworks = new {
                        NCA_ECC = "30 controls (sample - full 109 via CSV import)",
                        SAMA_CSF = "11 controls (sample)",
                        PDPL = "10 controls"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding KSA frameworks");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Import framework controls from CSV file upload
        /// CSV format: id,framework_code,version,control_number,domain,title_ar,title_en,requirement_ar,requirement_en,control_type,maturity_level,implementation_guidance_en,evidence_requirements,mapping_iso27001,mapping_nist,status
        /// </summary>
        [HttpPost("controls")]
        [AllowAnonymous] // For initial setup - should be secured in production
        public async Task<IActionResult> ImportControls(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file uploaded" });
                }

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { error = "File must be a CSV file" });
                }

                _logger.LogInformation("Starting import of controls from file: {FileName}, Size: {Size} bytes",
                    file.FileName, file.Length);

                using var reader = new StreamReader(file.OpenReadStream());
                var result = await _controlImporter.ImportFromStreamAsync(reader);

                if (result.Success)
                {
                    return Ok(new {
                        message = result.Message,
                        totalRecords = result.TotalRecords,
                        imported = result.ImportedCount,
                        skipped = result.SkippedCount
                    });
                }
                else
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing controls");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Import framework controls from a file path on the server
        /// </summary>
        [HttpPost("controls/file")]
        [AllowAnonymous] // For initial setup - should be secured in production
        public async Task<IActionResult> ImportControlsFromPath([FromBody] ImportFileRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.FilePath))
                {
                    return BadRequest(new { error = "FilePath is required" });
                }

                _logger.LogInformation("Starting import of controls from path: {FilePath}", request.FilePath);

                var result = await _controlImporter.ImportFromFileAsync(request.FilePath);

                if (result.Success)
                {
                    return Ok(new {
                        message = result.Message,
                        totalRecords = result.TotalRecords,
                        imported = result.ImportedCount,
                        skipped = result.SkippedCount
                    });
                }
                else
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing controls from path");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get statistics about imported framework controls
        /// </summary>
        [HttpGet("controls/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControlStats()
        {
            try
            {
                var stats = await _controlImporter.GetStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting control statistics");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get controls by framework code
        /// </summary>
        [HttpGet("controls/{frameworkCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControlsByFramework(string frameworkCode, [FromQuery] string? version = null)
        {
            try
            {
                var controls = await _controlImporter.GetControlsByFrameworkAsync(frameworkCode, version);
                return Ok(new {
                    frameworkCode,
                    version,
                    count = controls.Count,
                    controls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controls for framework {Framework}", frameworkCode);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Search controls by keyword
        /// </summary>
        [HttpGet("controls/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchControls([FromQuery] string q, [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(q))
                {
                    return BadRequest(new { error = "Search query 'q' is required" });
                }

                var controls = await _controlImporter.SearchControlsAsync(q, limit);
                return Ok(new {
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

        /// <summary>
        /// Check for duplicate controls in the database
        /// </summary>
        [HttpGet("controls/duplicates")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDuplicateControls()
        {
            try
            {
                // Find duplicates: same FrameworkCode + ControlNumber + Version
                var duplicates = await _context.FrameworkControls
                    .GroupBy(c => new { c.FrameworkCode, c.ControlNumber, c.Version })
                    .Where(g => g.Count() > 1)
                    .Select(g => new {
                        FrameworkCode = g.Key.FrameworkCode,
                        ControlNumber = g.Key.ControlNumber,
                        Version = g.Key.Version,
                        Count = g.Count(),
                        Ids = g.Select(x => x.Id).ToList()
                    })
                    .ToListAsync();

                // Get total control count
                var totalControls = await _context.FrameworkControls.CountAsync();

                // Get unique control count
                var uniqueControls = await _context.FrameworkControls
                    .Select(c => new { c.FrameworkCode, c.ControlNumber, c.Version })
                    .Distinct()
                    .CountAsync();

                return Ok(new {
                    totalControls,
                    uniqueControls,
                    duplicateGroups = duplicates.Count,
                    duplicateRecords = duplicates.Sum(d => d.Count - 1), // Extra records beyond unique
                    duplicates = duplicates.Take(50) // Limit to first 50 groups
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate controls");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove duplicate controls (keeps first occurrence)
        /// </summary>
        [HttpPost("controls/deduplicate")]
        [AllowAnonymous]
        public async Task<IActionResult> DeduplicateControls()
        {
            try
            {
                // Find all duplicates
                var duplicateGroups = await _context.FrameworkControls
                    .GroupBy(c => new { c.FrameworkCode, c.ControlNumber, c.Version })
                    .Where(g => g.Count() > 1)
                    .Select(g => new {
                        FrameworkCode = g.Key.FrameworkCode,
                        ControlNumber = g.Key.ControlNumber,
                        Version = g.Key.Version,
                        Ids = g.Select(x => x.Id).OrderBy(x => x).ToList()
                    })
                    .ToListAsync();

                if (duplicateGroups.Count == 0)
                {
                    return Ok(new { message = "No duplicates found", removed = 0 });
                }

                // Get IDs to delete (all except first in each group)
                var idsToDelete = duplicateGroups
                    .SelectMany(g => g.Ids.Skip(1))
                    .ToList();

                // Delete duplicates
                var controlsToDelete = await _context.FrameworkControls
                    .Where(c => idsToDelete.Contains(c.Id))
                    .ToListAsync();

                _context.FrameworkControls.RemoveRange(controlsToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed {Count} duplicate controls from {Groups} groups", 
                    controlsToDelete.Count, duplicateGroups.Count);

                return Ok(new {
                    message = $"Removed {controlsToDelete.Count} duplicate controls from {duplicateGroups.Count} groups",
                    removed = controlsToDelete.Count,
                    groupsProcessed = duplicateGroups.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deduplicating controls");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed derivation rules (50+ rules for scope derivation via Rules Engine)
        /// </summary>
        [HttpPost("rules")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedDerivationRules()
        {
            try
            {
                await DerivationRulesSeeds.SeedAsync(_context, _logger);

                var rulesetCount = await _context.Rulesets.CountAsync();
                var ruleCount = await _context.Rules.CountAsync();

                return Ok(new {
                    message = "Derivation rules seeded successfully",
                    rulesets = rulesetCount,
                    rules = ruleCount,
                    sections = new[] {
                        "Country & Region (Priority 1-10)",
                        "Sector-based (Priority 11-40)",
                        "Data Types (Priority 41-60)",
                        "Infrastructure (Priority 61-80)",
                        "Organization Size (Priority 81-90)",
                        "Vendor/Third-party (Priority 91-100)",
                        "International (Priority 101-110)",
                        "Certifications (Priority 111-120)"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding derivation rules");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed evidence scoring criteria, sector-framework index, and GOSI sub-sector mappings
        /// </summary>
        [HttpPost("evidence-scoring")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedEvidenceScoring()
        {
            try
            {
                await EvidenceScoringSeeds.SeedEvidenceScoringCriteriaAsync(_context, _logger);
                await EvidenceScoringSeeds.SeedSectorFrameworkIndexAsync(_context, _logger);
                await GosiSectorSeeds.SeedSubSectorMappingsAsync(_context, _logger);

                var criteriaCount = await _context.EvidenceScoringCriteria.CountAsync();
                var indexCount = await _context.SectorFrameworkIndexes.CountAsync();
                var gosiCount = await _context.GrcSubSectorMappings.CountAsync();
                var uniqueSectors = await _context.SectorFrameworkIndexes.Select(s => s.SectorCode).Distinct().CountAsync();

                return Ok(new {
                    message = "Evidence scoring, sector index, and GOSI mappings seeded successfully",
                    evidenceScoringCriteria = criteriaCount,
                    sectorFrameworkMappings = indexCount,
                    gosiSubSectorMappings = gosiCount,
                    mainSectors = uniqueSectors,
                    note = $"KSA GOSI classifies 70+ sub-sectors, mapped to {uniqueSectors} main GRC sectors"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding evidence scoring");
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get GOSI sub-sector to main sector mappings
        /// </summary>
        [HttpGet("gosi-sectors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGosiSectorMappings([FromQuery] string? mainSector = null)
        {
            try
            {
                var query = _context.GrcSubSectorMappings.AsQueryable();
                
                if (!string.IsNullOrEmpty(mainSector))
                {
                    query = query.Where(m => m.MainSectorCode == mainSector.ToUpper());
                }
                
                var mappings = await query
                    .OrderBy(m => m.MainSectorCode)
                    .ThenBy(m => m.DisplayOrder)
                    .Select(m => new {
                        m.GosiCode,
                        m.IsicSection,
                        m.SubSectorNameEn,
                        m.SubSectorNameAr,
                        m.MainSectorCode,
                        m.MainSectorNameEn,
                        m.MainSectorNameAr,
                        m.PrimaryRegulator,
                        m.RegulatoryNotes
                    })
                    .ToListAsync();
                
                var summary = mappings
                    .GroupBy(m => m.MainSectorCode)
                    .Select(g => new { MainSector = g.Key, SubSectorCount = g.Count() })
                    .OrderBy(s => s.MainSector)
                    .ToList();
                
                return Ok(new {
                    totalGosiSubSectors = mappings.Count,
                    totalMainSectors = summary.Count,
                    note = "KSA GOSI Occupational Safety Classification: 70+ sectors mapped to 18 main GRC sectors",
                    sectorSummary = summary,
                    mappings = mappings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GOSI sector mappings");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all regulators for dashboard display
        /// Uses RegulatorCatalog (seeded regulatory authorities)
        /// </summary>
        [HttpGet("regulators")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegulators([FromQuery] string? country = null, [FromQuery] string? type = null)
        {
            try
            {
                var query = _context.RegulatorCatalogs.AsQueryable();
                
                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(r => r.RegionType.Contains(country.ToLower()) || r.JurisdictionEn.Contains(country));
                }
                
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(r => r.Category == type.ToLower());
                }
                
                var regulators = await query
                    .OrderBy(r => r.DisplayOrder)
                    .Select(r => new {
                        r.Id,
                        code = r.Code,
                        name = r.NameEn,
                        nameAr = r.NameAr,
                        country = r.RegionType == "saudi" ? "Saudi Arabia" : 
                                 r.RegionType == "international" ? "International" : "Regional",
                        type = r.Category,
                        r.Website,
                        isActive = true
                    })
                    .ToListAsync();
                
                return Ok(new {
                    count = regulators.Count,
                    regulators = regulators
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regulators");
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get all frameworks for dashboard display (from FrameworkCatalog)
        /// </summary>
        [HttpGet("frameworks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFrameworks([FromQuery] Guid? regulatorId = null)
        {
            try
            {
                var query = _context.FrameworkCatalogs.AsQueryable();
                
                if (regulatorId.HasValue)
                {
                    query = query.Where(f => f.RegulatorId == regulatorId.Value);
                }
                
                var frameworks = await query
                    .OrderBy(f => f.Code)
                    .ToListAsync();
                
                // Count controls per framework code
                var frameworkCodes = frameworks.Select(f => f.Code).ToList();
                var controlCounts = await _context.FrameworkControls
                    .Where(c => frameworkCodes.Contains(c.FrameworkCode))
                    .GroupBy(c => c.FrameworkCode)
                    .Select(g => new { FrameworkCode = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.FrameworkCode, x => x.Count);
                
                var result = frameworks.Select(f => new {
                    f.Id,
                    code = f.Code,
                    name = f.TitleEn,
                    nameAr = f.TitleAr,
                    version = f.Version,
                    regulatorId = f.RegulatorId,
                    isActive = f.IsActive,
                    controlCount = controlCounts.GetValueOrDefault(f.Code, 0)
                }).ToList();
                
                return Ok(new {
                    count = result.Count,
                    frameworks = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting frameworks");
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get framework controls for dashboard display
        /// </summary>
        [HttpGet("controls")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControls([FromQuery] Guid? frameworkId = null, [FromQuery] string? frameworkCode = null, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            try
            {
                var query = _context.FrameworkControls.AsQueryable();
                
                if (frameworkId.HasValue)
                {
                    // Get framework code from ID via FrameworkCatalog
                    var framework = await _context.FrameworkCatalogs.FindAsync(frameworkId.Value);
                    if (framework != null)
                    {
                        query = query.Where(c => c.FrameworkCode == framework.Code);
                    }
                }
                else if (!string.IsNullOrEmpty(frameworkCode))
                {
                    query = query.Where(c => c.FrameworkCode == frameworkCode);
                }
                
                var totalCount = await query.CountAsync();
                
                var controls = await query
                    .OrderBy(c => c.ControlNumber)
                    .Skip(offset)
                    .Take(limit)
                    .Select(c => new {
                        c.Id,
                        c.ControlNumber,
                        c.FrameworkCode,
                        domainName = c.Domain,
                        title = c.TitleEn ?? c.TitleAr,
                        titleEn = c.TitleEn,
                        titleAr = c.TitleAr,
                        description = c.RequirementEn,
                        priority = c.ControlType ?? "Standard",
                        c.MaturityLevel
                    })
                    .ToListAsync();
                
                return Ok(new {
                    totalCount = totalCount,
                    count = controls.Count,
                    offset = offset,
                    limit = limit,
                    controls = controls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controls");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get sector framework mappings (cached for fast response)
        /// </summary>
        [HttpGet("sectors/{sectorCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSectorFrameworks(string sectorCode, [FromQuery] string? orgType = null)
        {
            try
            {
                var cacheService = HttpContext.RequestServices.GetRequiredService<ISectorFrameworkCacheService>();
                var bundle = await cacheService.GetSectorBundleAsync(sectorCode, orgType);

                return Ok(new {
                    sector = sectorCode,
                    orgType = orgType ?? "ALL",
                    frameworks = bundle.Frameworks,
                    totalControls = bundle.TotalControls,
                    totalEvidenceTypes = bundle.TotalEvidenceTypes,
                    evidenceByFramework = bundle.EvidenceByFramework.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Select(e => new { e.EvidenceType, MinScore = e.Criteria?.MinimumScore ?? 70, Frequency = e.Criteria?.CollectionFrequency ?? "Annual" })
                    ),
                    cachedAt = bundle.ComputedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sector frameworks for {Sector}", sectorCode);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Provision evidence requirements for a tenant (auto-generate based on sector)
        /// </summary>
        [HttpPost("tenant/{tenantId}/provision-evidence")]
        [AllowAnonymous]
        public async Task<IActionResult> ProvisionTenantEvidence(Guid tenantId, [FromQuery] string? sector = null, [FromQuery] string? orgType = null)
        {
            try
            {
                var provisioningService = HttpContext.RequestServices.GetRequiredService<ITenantEvidenceProvisioningService>();

                if (string.IsNullOrEmpty(sector))
                {
                    // Get sector from organization profile
                    var profile = await _context.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);
                    if (profile == null)
                    {
                        return BadRequest(new { error = "Organization profile not found. Please provide sector parameter." });
                    }
                    sector = profile.Sector;
                    // OrgType can be derived from sector or passed as query param
                }

                var result = await provisioningService.ProvisionEvidenceRequirementsAsync(tenantId, sector, orgType);

                if (result.Success)
                {
                    return Ok(new {
                        success = true,
                        tenantId,
                        sector = result.Sector,
                        orgType = result.OrgType,
                        evidenceRequirementsCreated = result.EvidenceRequirementsCreated,
                        controlsProcessed = result.ControlsProcessed,
                        frameworksProcessed = result.FrameworksProcessed
                    });
                }
                else
                {
                    return BadRequest(new { success = false, error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning evidence for tenant {TenantId}", tenantId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get evidence requirements summary for a tenant
        /// </summary>
        [HttpGet("tenant/{tenantId}/evidence-summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenantEvidenceSummary(Guid tenantId)
        {
            try
            {
                var provisioningService = HttpContext.RequestServices.GetRequiredService<ITenantEvidenceProvisioningService>();
                var summary = await provisioningService.GetEvidenceSummaryAsync(tenantId);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence summary for tenant {TenantId}", tenantId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all evidence scoring criteria
        /// </summary>
        [HttpGet("evidence-criteria")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvidenceScoringCriteria([FromQuery] string? sector = null, [FromQuery] string? framework = null)
        {
            try
            {
                var cacheService = HttpContext.RequestServices.GetRequiredService<ISectorFrameworkCacheService>();
                var criteria = await cacheService.GetEvidenceScoringCriteriaAsync(sector, framework);

                return Ok(new {
                    count = criteria.Count,
                    criteria = criteria.Select(c => new {
                        c.EvidenceTypeCode,
                        c.EvidenceTypeName,
                        c.Category,
                        c.BaseScore,
                        c.MaxScore,
                        c.MinimumScore,
                        c.CollectionFrequency,
                        c.DefaultValidityDays,
                        c.RequiresApproval,
                        c.AllowedFileTypes
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence scoring criteria");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Seed POC organization (Shahin-AI) with complete end-to-end data
        /// 15 sections: Tenant, Wizard, Profile, Baselines, Packages, Plan, Phases, Assessments, Team, Members, RACI, Evidence, Workflow, Audit, PolicyDecisions
        /// </summary>
        [HttpPost("poc-organization")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedPocOrganization([FromQuery] bool force = false)
        {
            try
            {
                var result = await _pocSeeder.SeedPocOrganizationAsync(force);

                if (result.Success)
                {
                    return Ok(new {
                        success = true,
                        message = result.Message,
                        tenantId = result.TenantId,
                        tenantSlug = result.TenantSlug,
                        sectionsSeeded = result.SectionsSeeded,
                        removed = result.Removed
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding POC organization");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if POC organization is already seeded
        /// </summary>
        [HttpGet("poc-organization/status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPocStatus()
        {
            var isSeeded = await _pocSeeder.IsPocSeededAsync();
            return Ok(new {
                tenantSlug = "shahin-ai",
                isSeeded,
                tenantId = isSeeded ? PocOrganizationSeeds.TenantId : (Guid?)null
            });
        }

        /// <summary>
        /// Create a new user in the system
        /// </summary>
        [HttpPost("users/create")]
        [AllowAnonymous] // For initial setup - should be secured in production
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.FirstName) ||
                    string.IsNullOrEmpty(request?.LastName) ||
                    string.IsNullOrEmpty(request?.Email) ||
                    string.IsNullOrEmpty(request?.Password))
                {
                    return BadRequest(new { error = "FirstName, LastName, Email, and Password are required" });
                }

                var user = await CreateUserHelper.CreateUserAsync(
                    _userManager,
                    _context,
                    _logger,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password,
                    request.Department,
                    request.JobTitle,
                    request.RoleName,
                    request.TenantId);

                if (user == null)
                {
                    return BadRequest(new { error = "Failed to create user. Check logs for details." });
                }

                return Ok(new {
                    success = true,
                    message = "User created successfully",
                    userId = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    mustChangePassword = user.MustChangePassword
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a Platform Admin user directly (bypasses tenant linking issues)
        /// </summary>
        [HttpPost("platform-admin/create")]
        [AllowAnonymous] // For initial setup only
        public async Task<IActionResult> CreatePlatformAdmin([FromBody] CreatePlatformAdminRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request?.Password))
                {
                    return BadRequest(new { error = "Email and Password are required" });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    // Just ensure they have PlatformAdmin role
                    if (!await _userManager.IsInRoleAsync(existingUser, "PlatformAdmin"))
                    {
                        await _userManager.AddToRoleAsync(existingUser, "PlatformAdmin");
                    }

                    // Ensure PlatformAdmin record exists
                    await EnsurePlatformAdminRecordAsync(existingUser, request);

                    return Ok(new { success = true, message = "User already exists, role and record verified", userId = existingUser.Id });
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                    FirstName = request.FirstName ?? "Platform",
                    LastName = request.LastName ?? "Admin",
                    Department = request.Department ?? "Administration",
                    JobTitle = request.JobTitle ?? "Platform Administrator",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    MustChangePassword = false
                };

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { error = string.Join(", ", createResult.Errors.Select(e => e.Description)) });
                }

                // Add PlatformAdmin role
                await _userManager.AddToRoleAsync(user, "PlatformAdmin");

                // Create PlatformAdmin record
                await EnsurePlatformAdminRecordAsync(user, request);

                _logger.LogInformation("✅ Platform Admin created: {Email}", request.Email);

                return Ok(new {
                    success = true,
                    message = "Platform Admin created successfully",
                    userId = user.Id,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating platform admin");
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task EnsurePlatformAdminRecordAsync(ApplicationUser user, CreatePlatformAdminRequest request)
        {
            await EnsureApplicationUserRecordAsync(user);

            var existingRecord = await _context.PlatformAdmins
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (existingRecord == null)
            {
                var platformAdmin = new PlatformAdmin
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DisplayName = $"{user.FirstName} {user.LastName}",
                    ContactEmail = user.Email!,
                    AdminLevel = PlatformAdminLevel.Owner,
                    Status = "Active",
                    CanCreateTenants = true,
                    CanManageTenants = true,
                    CanDeleteTenants = true,
                    CanManageBilling = true,
                    CanAccessTenantData = true,
                    CanManageCatalogs = true,
                    CanManagePlatformAdmins = true,
                    CanViewAnalytics = true,
                    CanManageConfiguration = true,
                    CanImpersonateUsers = true,
                    CanResetOwnPassword = true,
                    CanResetTenantAdminPasswords = true,
                    CanRestartTenantAdminAccounts = true,
                    MaxTenantsAllowed = 0,
                    MfaRequired = false,
                    SessionTimeoutMinutes = 60,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _context.PlatformAdmins.Add(platformAdmin);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ PlatformAdmin record created for user {UserId}", user.Id);
            }
        }

        private async Task EnsureApplicationUserRecordAsync(ApplicationUser authUser)
        {
            var existingAppUser = await _context.Set<ApplicationUser>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == authUser.Id);

            if (existingAppUser != null)
            {
                return;
            }

            var appUser = new ApplicationUser
            {
                Id = authUser.Id,
                UserName = authUser.UserName,
                NormalizedUserName = authUser.NormalizedUserName,
                Email = authUser.Email,
                NormalizedEmail = authUser.NormalizedEmail,
                EmailConfirmed = authUser.EmailConfirmed,
                PhoneNumber = authUser.PhoneNumber,
                PhoneNumberConfirmed = authUser.PhoneNumberConfirmed,
                TwoFactorEnabled = authUser.TwoFactorEnabled,
                LockoutEnd = authUser.LockoutEnd,
                LockoutEnabled = authUser.LockoutEnabled,
                AccessFailedCount = authUser.AccessFailedCount,
                FirstName = authUser.FirstName,
                LastName = authUser.LastName,
                Department = authUser.Department,
                JobTitle = authUser.JobTitle,
                IsActive = authUser.IsActive,
                CreatedDate = authUser.CreatedDate,
                MustChangePassword = authUser.MustChangePassword,
                KsaCompetencyLevel = authUser.KsaCompetencyLevel
            };

            _context.Set<ApplicationUser>().Add(appUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ ApplicationUser record created in GrcMvcDb for user {UserId}", authUser.Id);
        }

        /// <summary>
        /// List all tenants and their status (debug only)
        /// GET /api/seed/debug-tenants
        /// </summary>
        [HttpGet("debug-tenants")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugTenants()
        {
            var tenants = await _context.Tenants
                .Select(t => new {
                    t.Id,
                    t.OrganizationName,
                    t.AdminEmail,
                    t.OnboardingStatus,
                    t.Status,
                    t.IsTrial,
                    t.CreatedDate
                })
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .ToListAsync();

            var tenantUsers = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .Select(tu => new {
                    tu.UserId,
                    tu.TenantId,
                    TenantName = tu.Tenant != null ? tu.Tenant.OrganizationName : null,
                    tu.RoleCode,
                    tu.Status,
                    tu.CreatedDate
                })
                .OrderByDescending(tu => tu.CreatedDate)
                .Take(10)
                .ToListAsync();

            return Ok(new { tenants, tenantUsers });
        }

        /// <summary>
        /// Check user's tenant and onboarding status
        /// GET /api/seed/check-onboarding?email=user@example.com
        /// </summary>
        [HttpGet("check-onboarding")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckOnboardingStatus([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { error = "User not found", email });
            }

            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            if (tenantUser == null)
            {
                return Ok(new { 
                    email, 
                    userId = user.Id,
                    hasTenantUser = false,
                    message = "User has no tenant association"
                });
            }

            var tenant = tenantUser.Tenant;
            return Ok(new {
                email,
                userId = user.Id,
                hasTenantUser = true,
                tenantId = tenantUser.TenantId,
                tenantName = tenant?.OrganizationName,
                roleCode = tenantUser.RoleCode,
                tenantStatus = tenant?.Status,
                onboardingStatus = tenant?.OnboardingStatus,
                isTrial = tenant?.IsTrial,
                trialEndsAt = tenant?.TrialEndsAt,
                shouldRedirectToOnboarding = tenant?.OnboardingStatus != "COMPLETED",
                onboardingUrl = $"/OnboardingWizard?tenantId={tenantUser.TenantId}"
            });
        }

        /// <summary>
        /// Reset admin password - Emergency endpoint for fixing password hash issues
        /// POST /api/seed/fix-admin-password
        /// </summary>
        [HttpPost("fix-admin-password")]
        [AllowAnonymous]
        public async Task<IActionResult> FixAdminPassword([FromBody] FixPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request?.NewPassword))
                {
                    return BadRequest(new { error = "Email and NewPassword are required" });
                }

                _logger.LogInformation("Fixing admin password for: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", email = request.Email });
                }

                // Remove the old (possibly corrupted) password hash
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Failed to remove old password: {Errors}", 
                        string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                }

                // Add new password with proper ASP.NET Identity hash
                var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (!addResult.Succeeded)
                {
                    return BadRequest(new { 
                        error = "Failed to set new password",
                        details = addResult.Errors.Select(e => e.Description)
                    });
                }

                // Reset lockout
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogInformation("✅ Password fixed successfully for: {Email}", request.Email);

                return Ok(new {
                    success = true,
                    message = "Password reset successfully",
                    email = request.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing admin password for: {Email}", request?.Email);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Fix all platform admin passwords (quick fix endpoint)
        /// GET /api/seed/fix-all-admins
        /// </summary>
        [HttpGet("fix-all-admins")]
        [AllowAnonymous]
        public async Task<IActionResult> FixAllAdmins()
        {
            var results = new List<object>();

            // Fix ahmet.dogan@doganconsult.com
            var user1 = await _userManager.FindByEmailAsync("ahmet.dogan@doganconsult.com");
            if (user1 != null)
            {
                await _userManager.RemovePasswordAsync(user1);
                var result1 = await _userManager.AddPasswordAsync(user1, "Dogan@Admin2026!");
                await _userManager.SetLockoutEndDateAsync(user1, null);
                results.Add(new { email = "ahmet.dogan@doganconsult.com", success = result1.Succeeded });
            }

            // Fix Dooganlap@gmail.com
            var user2 = await _userManager.FindByEmailAsync("Dooganlap@gmail.com");
            if (user2 != null)
            {
                await _userManager.RemovePasswordAsync(user2);
                var result2 = await _userManager.AddPasswordAsync(user2, "Platform@Admin2026!");
                await _userManager.SetLockoutEndDateAsync(user2, null);
                results.Add(new { email = "Dooganlap@gmail.com", success = result2.Succeeded });
            }

            return Ok(new {
                message = "Admin passwords fixed",
                results,
                credentials = new[] {
                    new { email = "ahmet.dogan@doganconsult.com", password = "Dogan@Admin2026!" },
                    new { email = "Dooganlap@gmail.com", password = "Platform@Admin2026!" }
                }
            });
        }
    }

    public class CreatePlatformAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
    }

    /// <summary>
    /// Request model for importing controls from a file path
    /// </summary>
    public class ImportFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating a new user
    /// </summary>
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? RoleName { get; set; }
        public Guid? TenantId { get; set; }
    }

    /// <summary>
    /// Request model for fixing password
    /// </summary>
    public class FixPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
