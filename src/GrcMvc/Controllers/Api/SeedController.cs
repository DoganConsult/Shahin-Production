using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for seeding catalog, workflow, and framework control data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class SeedController : ControllerBase
    {
        private readonly CatalogSeederService _catalogSeeder;
        private readonly WorkflowDefinitionSeederService _workflowSeeder;
        private readonly FrameworkControlImportService _controlImporter;
        private readonly IPocSeederService _pocSeeder;
        private readonly GrcDbContext _context;
        private readonly ILogger<SeedController> _logger;

        public SeedController(
            CatalogSeederService catalogSeeder,
            WorkflowDefinitionSeederService workflowSeeder,
            FrameworkControlImportService controlImporter,
            IPocSeederService pocSeeder,
            GrcDbContext context,
            ILogger<SeedController> logger)
        {
            _catalogSeeder = catalogSeeder;
            _workflowSeeder = workflowSeeder;
            _controlImporter = controlImporter;
            _pocSeeder = pocSeeder;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seed all catalog data (Roles, Titles, Baselines, Packages, Templates, Evidence Types)
        /// </summary>
        [HttpPost("catalogs")]
        [AllowAnonymous] // For initial setup - should be secured in production
        public async Task<IActionResult> SeedCatalogs()
        {
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
        [AllowAnonymous] // For initial setup - should be secured in production
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
        [AllowAnonymous] // For initial setup - should be secured in production
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

                return Ok(new {
                    message = "All seed data created successfully",
                    catalogs = new[] { "Roles", "Titles", "Baselines", "Packages", "Templates", "EvidenceTypes" },
                    regulators = "92 regulators (62 Saudi, 20 International, 10 Regional)",
                    frameworks = new[] { "NCA-ECC (30 controls)", "SAMA-CSF (11 controls)", "PDPL (10 controls)" },
                    workflows = new[] { "NCA ECC", "SAMA CSF", "PDPL PIA", "ERM", "Evidence Review", "Audit Remediation", "Policy Review" }
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
    }

    /// <summary>
    /// Request model for importing controls from a file path
    /// </summary>
    public class ImportFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
