using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for generating GRC reports
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generate a compliance report
        /// </summary>
        Task<(string reportId, string filePath)> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generate a risk report
        /// </summary>
        Task<(string reportId, string filePath)> GenerateRiskReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generate an audit report
        /// </summary>
        Task<(string reportId, string filePath)> GenerateAuditReportAsync(Guid auditId);

        /// <summary>
        /// Generate a control assessment report
        /// </summary>
        Task<(string reportId, string filePath)> GenerateControlReportAsync(Guid controlId);

        /// <summary>
        /// Generate executive summary
        /// </summary>
        Task<object> GenerateExecutiveSummaryAsync();

        /// <summary>
        /// Get report by ID
        /// </summary>
        Task<object> GetReportAsync(string reportId);

        /// <summary>
        /// List all generated reports
        /// </summary>
        Task<List<object>> ListReportsAsync();

        /// <summary>
        /// Update an existing report
        /// </summary>
        Task<object> UpdateReportAsync(string reportId, UpdateReportDto dto);

        /// <summary>
        /// Soft delete a report
        /// </summary>
        Task<bool> DeleteReportAsync(string reportId);

        /// <summary>
        /// Mark report as delivered
        /// </summary>
        Task<object> DeliverReportAsync(string reportId, string deliveredTo, string? notes = null);

        /// <summary>
        /// Get report file for download
        /// </summary>
        Task<(byte[] fileData, string fileName, string contentType)?> DownloadReportAsync(string reportId, string format = "pdf");
    }

    public class UpdateReportDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ExecutiveSummary { get; set; }
        public string? KeyFindings { get; set; }
        public string? Recommendations { get; set; }
        public string? Status { get; set; }
    }
}
