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
        private readonly ILogger<SeedController> _logger;

        public SeedController(
            CatalogSeederService catalogSeeder,
            WorkflowDefinitionSeederService workflowSeeder,
            FrameworkControlImportService controlImporter,
            ILogger<SeedController> logger)
        {
            _catalogSeeder = catalogSeeder;
            _workflowSeeder = workflowSeeder;
            _controlImporter = controlImporter;
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
        /// Seed all data (catalogs + workflows)
        /// </summary>
        [HttpPost("all")]
        [AllowAnonymous] // For initial setup - should be secured in production
        public async Task<IActionResult> SeedAll()
        {
            try
            {
                await _catalogSeeder.SeedAllCatalogsAsync();
                await _workflowSeeder.SeedAllWorkflowDefinitionsAsync();
                return Ok(new {
                    message = "All seed data created successfully",
                    catalogs = new[] { "Roles", "Titles", "Baselines", "Packages", "Templates", "EvidenceTypes" },
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
    }

    /// <summary>
    /// Request model for importing controls from a file path
    /// </summary>
    public class ImportFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
