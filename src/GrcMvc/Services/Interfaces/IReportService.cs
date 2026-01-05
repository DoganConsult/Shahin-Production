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
    }
}
