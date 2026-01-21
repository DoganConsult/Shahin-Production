using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Resources;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ReportController(
            IReportService reportService,
            ILogger<ReportController> logger,
            IStringLocalizer<SharedResource> localizer)
        {
            _reportService = reportService;
            _logger = logger;
            _localizer = localizer;
        }

        /// <summary>
        /// List all reports with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ReportListItemDto>>> GetReports(
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var reports = await _reportService.ListReportsAsync();
                var reportDtos = reports.Cast<dynamic>()
                    .Select(r => new ReportListItemDto
                    {
                        Id = Guid.Parse(r.reportId.ToString()),
                        ReportNumber = r.reportNumber?.ToString() ?? "",
                        Title = r.title?.ToString() ?? "",
                        Type = r.type?.ToString() ?? "",
                        Status = r.status?.ToString() ?? "Draft",
                        GeneratedDate = r.generatedDate != null ? (DateTime)r.generatedDate : DateTime.UtcNow,
                        GeneratedBy = "System",
                        PageCount = 0
                    })
                    .ToList();

                // Apply filters
                if (!string.IsNullOrEmpty(type))
                {
                    reportDtos = reportDtos.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(status))
                {
                    reportDtos = reportDtos.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (startDate.HasValue)
                {
                    reportDtos = reportDtos.Where(r => r.GeneratedDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    reportDtos = reportDtos.Where(r => r.GeneratedDate <= endDate.Value).ToList();
                }

                // Apply pagination
                var totalCount = reportDtos.Count;
                var pagedResults = reportDtos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    data = pagedResults,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing reports");
                return StatusCode(500, new { error = _localizer["Error_ServerError"] });
            }
        }

        /// <summary>
        /// Get report details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDetailDto>> GetReport(string id)
        {
            try
            {
                var report = await _reportService.GetReportAsync(id);
                if (report == null)
                {
                    return NotFound(new { error = _localizer["Error_NotFound"] });
                }

                // Convert dynamic object to DTO
                var reportObj = report as dynamic;
                var dto = new ReportDetailDto
                {
                    Id = Guid.Parse(reportObj.reportId.ToString()),
                    ReportNumber = reportObj.reportNumber?.ToString() ?? "",
                    Title = reportObj.title?.ToString() ?? "",
                    Type = reportObj.type?.ToString() ?? "",
                    Status = reportObj.status?.ToString() ?? "Draft",
                    GeneratedDate = reportObj.generatedDate != null ? (DateTime)reportObj.generatedDate : DateTime.UtcNow,
                    GeneratedBy = "System",
                    PageCount = reportObj.pages != null ? (int)reportObj.pages : 0,
                    FileUrl = reportObj.fileUrl?.ToString() ?? ""
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report {ReportId}", id);
                return StatusCode(500, new { error = "Failed to retrieve report" });
            }
        }

        /// <summary>
        /// Create/generate a new report
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReportDetailDto>> CreateReport([FromBody] ReportCreateDto dto)
        {
            try
            {
                (string reportId, string filePath) result;

                switch (dto.Type.ToUpper())
                {
                    case "COMPLIANCE":
                        result = await _reportService.GenerateComplianceReportAsync(
                            dto.ReportPeriodStart,
                            dto.ReportPeriodEnd);
                        break;

                    case "RISK":
                        result = await _reportService.GenerateRiskReportAsync(
                            dto.ReportPeriodStart,
                            dto.ReportPeriodEnd);
                        break;

                    default:
                        return BadRequest(new { error = _localizer["Error_BadRequest"] });
                }

                var report = await _reportService.GetReportAsync(result.reportId);
                var reportObj = report as dynamic;

                var reportDto = new ReportDetailDto
                {
                    Id = Guid.Parse(result.reportId),
                    ReportNumber = reportObj?.reportNumber?.ToString() ?? "",
                    Title = dto.Title,
                    Type = dto.Type,
                    Description = dto.Description,
                    Status = "Generated",
                    Scope = dto.Scope,
                    ReportPeriodStart = dto.ReportPeriodStart,
                    ReportPeriodEnd = dto.ReportPeriodEnd,
                    GeneratedDate = DateTime.UtcNow,
                    GeneratedBy = "System",
                    FileUrl = result.filePath
                };

                return CreatedAtAction(nameof(GetReport), new { id = result.reportId }, reportDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report");
                return StatusCode(500, new { error = "Failed to create report" });
            }
        }

        /// <summary>
        /// Update report (edit summary, findings, recommendations)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ReportDetailDto>> UpdateReport(string id, [FromBody] ReportEditDto dto)
        {
            try
            {
                var updateDto = new UpdateReportDto
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    ExecutiveSummary = dto.ExecutiveSummary,
                    KeyFindings = dto.KeyFindings,
                    Recommendations = dto.Recommendations,
                    Status = dto.Status
                };

                var result = await _reportService.UpdateReportAsync(id, updateDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return StatusCode(500, new { error = "Failed to update report" });
            }
        }

        /// <summary>
        /// Delete/archive a report
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReport(string id)
        {
            try
            {
                var result = await _reportService.DeleteReportAsync(id);
                if (!result)
                {
                    return NotFound(new { error = _localizer["Error_NotFound"] });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                return StatusCode(500, new { error = "Failed to delete report" });
            }
        }

        /// <summary>
        /// Download report file (PDF/Excel)
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadReport(string id, [FromQuery] string format = "pdf")
        {
            try
            {
                var result = await _reportService.DownloadReportAsync(id, format);
                if (result == null)
                {
                    return NotFound(new { error = _localizer["Error_NotFound"] });
                }

                var (fileData, fileName, contentType) = result.Value;
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading report {ReportId}", id);
                return StatusCode(500, new { error = "Failed to download report" });
            }
        }

        /// <summary>
        /// Mark report as delivered
        /// </summary>
        [HttpPost("{id}/deliver")]
        public async Task<ActionResult> DeliverReport(string id, [FromBody] DeliverReportDto dto)
        {
            try
            {
                var result = await _reportService.DeliverReportAsync(id, dto.DeliveredTo, dto.Notes);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = "The requested resource was not found." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering report {ReportId}", id);
                return StatusCode(500, new { error = "Failed to deliver report" });
            }
        }

        /// <summary>
        /// Get report templates
        /// </summary>
        [HttpGet("templates")]
        public ActionResult<List<object>> GetTemplates()
        {
            var templates = new List<object>
            {
                new { type = "Risk", name = "Risk Summary Report", description = "Comprehensive risk register report" },
                new { type = "Compliance", name = "Compliance Status Report", description = "Current compliance status" },
                new { type = "Audit", name = "Audit Findings Report", description = "Detailed audit findings" },
                new { type = "Control", name = "Control Effectiveness Report", description = "Control assessment results" },
                new { type = "Executive", name = "Executive Summary", description = "High-level executive dashboard" }
            };

            return Ok(templates);
        }

        /// <summary>
        /// Quick generate report by type
        /// </summary>
        [HttpPost("generate/{type}")]
        public async Task<ActionResult<ReportDetailDto>> QuickGenerate(string type, [FromBody] QuickGenerateReportDto dto)
        {
            try
            {
                (string reportId, string filePath) result;

                switch (type.ToUpper())
                {
                    case "COMPLIANCE":
                        result = await _reportService.GenerateComplianceReportAsync(
                            dto.StartDate,
                            dto.EndDate);
                        break;

                    case "RISK":
                        result = await _reportService.GenerateRiskReportAsync(
                            dto.StartDate,
                            dto.EndDate);
                        break;

                    default:
                        return BadRequest(new { error = $"Unsupported report type: {type}" });
                }

                var report = await _reportService.GetReportAsync(result.reportId);
                var reportObj = report as dynamic;

                var reportDto = new ReportDetailDto
                {
                    Id = Guid.Parse(result.reportId),
                    ReportNumber = reportObj?.reportNumber?.ToString() ?? "",
                    Title = $"{type} Report",
                    Type = type,
                    Status = "Generated",
                    GeneratedDate = DateTime.UtcNow,
                    GeneratedBy = "System",
                    FileUrl = result.filePath
                };

                return Ok(reportDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report type {Type}", type);
                return StatusCode(500, new { error = "Failed to generate report" });
            }
        }
    }

    public class DeliverReportDto
    {
        public string DeliveredTo { get; set; } = string.Empty;
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class QuickGenerateReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
