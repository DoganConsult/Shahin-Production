using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for Framework Controls - Import, Query, Statistics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class FrameworkControlsController : ControllerBase
    {
        private readonly FrameworkControlImportService _importService;
        private readonly GrcDbContext _context;
        private readonly ILogger<FrameworkControlsController> _logger;

        public FrameworkControlsController(
            FrameworkControlImportService importService,
            GrcDbContext context,
            ILogger<FrameworkControlsController> logger)
        {
            _importService = importService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Import controls from uploaded CSV file
        /// </summary>
        [HttpPost("import")]
        [AllowAnonymous]
        [RequestSizeLimit(100_000_000)] // 100MB limit for large CSV files
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var result = await _importService.ImportFromStreamAsync(reader);

                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing controls");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Import controls from file path on server
        /// </summary>
        [HttpPost("import-from-path")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportFromPath([FromBody] ImportPathRequest request)
        {
            if (string.IsNullOrEmpty(request?.FilePath))
            {
                return BadRequest(new { error = "File path is required" });
            }

            try
            {
                var result = await _importService.ImportFromFileAsync(request.FilePath);

                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing controls from path");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get statistics about imported controls
        /// </summary>
        [HttpGet("statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _importService.GetStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get controls by framework code
        /// </summary>
        [HttpGet("by-framework/{frameworkCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByFramework(string frameworkCode, [FromQuery] string? version = null)
        {
            try
            {
                var controls = await _importService.GetControlsByFrameworkAsync(frameworkCode, version);
                return Ok(new {
                    framework = frameworkCode,
                    version = version,
                    count = controls.Count,
                    controls = controls.Select(c => new {
                        c.Id,
                        c.ControlNumber,
                        c.Domain,
                        c.TitleEn,
                        c.ControlType,
                        c.MaturityLevel,
                        c.MappingIso27001,
                        c.MappingNist
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controls by framework");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Search controls by keyword
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 50)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest(new { error = "Search query 'q' is required" });
            }

            try
            {
                var controls = await _importService.SearchControlsAsync(q, limit);
                return Ok(new {
                    query = q,
                    count = controls.Count,
                    controls = controls.Select(c => new {
                        c.Id,
                        c.FrameworkCode,
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
                _logger.LogError(ex, "Error searching controls");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// Get control by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var control = await _context.FrameworkControls.FindAsync(id);
                if (control == null)
                {
                    return NotFound(new { error = "Control not found" });
                }
                return Ok(control);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting control by ID");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }

        /// <summary>
        /// List all unique frameworks
        /// </summary>
        [HttpGet("frameworks")]
        [AllowAnonymous]
        public async Task<IActionResult> ListFrameworks()
        {
            try
            {
                var frameworks = await _context.FrameworkControls
                    .GroupBy(c => new { c.FrameworkCode, c.Version })
                    .Select(g => new {
                        FrameworkCode = g.Key.FrameworkCode,
                        Version = g.Key.Version,
                        ControlCount = g.Count()
                    })
                    .OrderBy(f => f.FrameworkCode)
                    .ThenBy(f => f.Version)
                    .ToListAsync();

                return Ok(new {
                    totalFrameworks = frameworks.Count,
                    frameworks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing frameworks");
                return BadRequest(new { error = "An error occurred processing your request." });
            }
        }
    }

    public class ImportPathRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
