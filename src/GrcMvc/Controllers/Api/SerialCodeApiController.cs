using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Serial Code API Controller
/// Provides REST endpoints for serial code generation, validation, and management.
/// </summary>
[ApiController]
[Route("api/v1/serial-codes")]
[Authorize]
public class SerialCodeApiController : ControllerBase
{
    private readonly ISerialCodeService _serialCodeService;
    private readonly ILogger<SerialCodeApiController> _logger;

    public SerialCodeApiController(
        ISerialCodeService serialCodeService,
        ILogger<SerialCodeApiController> logger)
    {
        _serialCodeService = serialCodeService;
        _logger = logger;
    }

    // =========================================================================
    // GENERATION
    // =========================================================================

    /// <summary>
    /// Generate a new serial code
    /// </summary>
    /// <param name="request">Serial code generation request</param>
    /// <returns>Generated serial code result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SerialCodeResult), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Generate([FromBody] GenerateSerialCodeRequest request)
    {
        try
        {
            var result = await _serialCodeService.GenerateAsync(new SerialCodeRequest
            {
                EntityType = request.EntityType,
                TenantCode = request.TenantCode,
                EntityId = request.EntityId,
                Stage = request.Stage,
                Year = request.Year,
                Metadata = request.Metadata,
                CreatedBy = User.Identity?.Name ?? "System"
            });

            return CreatedAtAction(nameof(GetByCode), new { code = result.Code }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid serial code generation request");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Generate multiple serial codes in a batch
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(List<SerialCodeResult>), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> GenerateBatch([FromBody] List<GenerateSerialCodeRequest> requests)
    {
        try
        {
            var serialRequests = requests.ConvertAll(r => new SerialCodeRequest
            {
                EntityType = r.EntityType,
                TenantCode = r.TenantCode,
                EntityId = r.EntityId,
                Stage = r.Stage,
                Year = r.Year,
                Metadata = r.Metadata,
                CreatedBy = User.Identity?.Name ?? "System"
            });

            var results = await _serialCodeService.GenerateBatchAsync(serialRequests);
            return StatusCode(201, results);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid batch serial code generation request");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    // =========================================================================
    // VALIDATION
    // =========================================================================

    /// <summary>
    /// Validate a serial code format
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SerialCodeValidationResult), 200)]
    public IActionResult Validate([FromBody] ValidateSerialCodeRequest request)
    {
        var result = _serialCodeService.Validate(request.Code);
        return Ok(result);
    }

    /// <summary>
    /// Parse a serial code into its components
    /// </summary>
    [HttpGet("parse/{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ParsedSerialCode), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public IActionResult Parse(string code)
    {
        try
        {
            var parsed = _serialCodeService.Parse(code);
            return Ok(parsed);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Serial Code",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    // =========================================================================
    // LOOKUP
    // =========================================================================

    /// <summary>
    /// Get serial code record by code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(SerialCodeRecord), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var record = await _serialCodeService.GetByCodeAsync(code);
        if (record == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Serial code not found: {code}",
                Status = 404
            });
        }

        return Ok(record);
    }

    /// <summary>
    /// Check if a serial code exists
    /// </summary>
    [HttpHead("{code}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Exists(string code)
    {
        var exists = await _serialCodeService.ExistsAsync(code);
        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Get version history for a serial code
    /// </summary>
    [HttpGet("{code}/history")]
    [ProducesResponseType(typeof(List<SerialCodeVersion>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> GetHistory(string code)
    {
        try
        {
            var history = await _serialCodeService.GetHistoryAsync(code);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Serial Code",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Get serial code by entity reference
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(SerialCodeRecord), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByEntity(string entityType, Guid entityId)
    {
        var record = await _serialCodeService.GetByEntityAsync(entityType, entityId);
        if (record == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"No serial code found for {entityType} with ID {entityId}",
                Status = 404
            });
        }

        return Ok(record);
    }

    // =========================================================================
    // VERSIONING
    // =========================================================================

    /// <summary>
    /// Create a new version of an existing serial code
    /// </summary>
    [HttpPost("{code}/versions")]
    [ProducesResponseType(typeof(SerialCodeResult), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateNewVersion(string code, [FromBody] CreateVersionRequest? request = null)
    {
        try
        {
            var result = await _serialCodeService.CreateNewVersionAsync(code, request?.ChangeReason);
            return CreatedAtAction(nameof(GetByCode), new { code = result.Code }, result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = 404
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Operation Failed",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Get the latest version code
    /// </summary>
    [HttpGet("{code}/latest")]
    [ProducesResponseType(typeof(LatestVersionResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> GetLatestVersion(string code)
    {
        try
        {
            var latestCode = await _serialCodeService.GetLatestVersionAsync(code);
            return Ok(new LatestVersionResponse { Code = latestCode });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Serial Code",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    // =========================================================================
    // SEARCH
    // =========================================================================

    /// <summary>
    /// Search serial codes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SerialCodeSearchResult), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] string? prefix = null,
        [FromQuery] string? tenant = null,
        [FromQuery] int? stage = null,
        [FromQuery] int? year = null,
        [FromQuery] string? status = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0)
    {
        var criteria = new SerialCodeSearchCriteria
        {
            Prefix = prefix,
            TenantCode = tenant,
            Stage = stage,
            Year = year,
            Status = status,
            EntityType = entityType,
            Limit = Math.Min(limit, 500), // Cap at 500
            Offset = offset
        };

        var result = await _serialCodeService.SearchAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get serial codes by stage
    /// </summary>
    [HttpGet("stage/{stage:int}")]
    [ProducesResponseType(typeof(List<SerialCodeRecord>), 200)]
    public async Task<IActionResult> GetByStage(int stage, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var records = await _serialCodeService.GetByStageAsync(stage, Math.Min(limit, 500), offset);
        return Ok(records);
    }

    // =========================================================================
    // RESERVATION
    // =========================================================================

    /// <summary>
    /// Reserve a serial code for later use
    /// </summary>
    [HttpPost("reserve")]
    [ProducesResponseType(typeof(SerialCodeReservationResult), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Reserve([FromBody] ReserveSerialCodeRequest request)
    {
        try
        {
            var ttl = request.ExpiresInSeconds.HasValue
                ? TimeSpan.FromSeconds(request.ExpiresInSeconds.Value)
                : TimeSpan.FromMinutes(5);

            var result = await _serialCodeService.ReserveAsync(new SerialCodeRequest
            {
                EntityType = request.EntityType,
                TenantCode = request.TenantCode,
                Stage = request.Stage,
                CreatedBy = User.Identity?.Name ?? "System"
            }, ttl);

            return StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Confirm a reservation
    /// </summary>
    [HttpPost("reserve/{reservationId}/confirm")]
    [ProducesResponseType(typeof(SerialCodeResult), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ConfirmReservation(string reservationId, [FromBody] ConfirmReservationRequest request)
    {
        try
        {
            var result = await _serialCodeService.ConfirmReservationAsync(reservationId, request.EntityId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = 404
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Operation Failed",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    [HttpDelete("reserve/{reservationId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelReservation(string reservationId)
    {
        try
        {
            await _serialCodeService.CancelReservationAsync(reservationId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = 404
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Operation Failed",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    // =========================================================================
    // ADMINISTRATION
    // =========================================================================

    /// <summary>
    /// Void a serial code
    /// </summary>
    [HttpPost("{code}/void")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Void(string code, [FromBody] VoidSerialCodeRequest request)
    {
        try
        {
            await _serialCodeService.VoidAsync(code, request.Reason);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = 404
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Operation Failed",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Get traceability report for a serial code
    /// </summary>
    [HttpGet("{code}/traceability")]
    [ProducesResponseType(typeof(SerialCodeTraceabilityReport), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTraceabilityReport(string code)
    {
        try
        {
            var report = await _serialCodeService.GetTraceabilityReportAsync(code);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = 404
            });
        }
    }

    /// <summary>
    /// Get the next sequence number (peek only)
    /// </summary>
    [HttpGet("next-sequence")]
    [ProducesResponseType(typeof(NextSequenceResponse), 200)]
    public async Task<IActionResult> GetNextSequence(
        [FromQuery] string prefix,
        [FromQuery] string tenant,
        [FromQuery] int stage,
        [FromQuery] int? year = null)
    {
        var nextYear = year ?? DateTime.UtcNow.Year;
        var nextSequence = await _serialCodeService.GetNextSequenceAsync(prefix, tenant, stage, nextYear);

        return Ok(new NextSequenceResponse
        {
            Prefix = prefix,
            TenantCode = tenant,
            Stage = stage,
            Year = nextYear,
            NextSequence = nextSequence
        });
    }
}

// =========================================================================
// REQUEST/RESPONSE DTOs
// =========================================================================

public class GenerateSerialCodeRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public int? Stage { get; set; }
    public int? Year { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ValidateSerialCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public class CreateVersionRequest
{
    public string? ChangeReason { get; set; }
}

public class LatestVersionResponse
{
    public string Code { get; set; } = string.Empty;
}

public class ReserveSerialCodeRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public int? Stage { get; set; }
    public int? ExpiresInSeconds { get; set; }
}

public class ConfirmReservationRequest
{
    public Guid EntityId { get; set; }
}

public class VoidSerialCodeRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class NextSequenceResponse
{
    public string Prefix { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public int Stage { get; set; }
    public int Year { get; set; }
    public int NextSequence { get; set; }
}
