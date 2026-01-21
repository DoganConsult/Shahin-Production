using System;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for generating reports in various formats (PDF, Excel, etc.)
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Generate a PDF report
        /// </summary>
        /// <param name="report">Report entity with metadata</param>
        /// <param name="data">Report data to include</param>
        /// <returns>PDF file as byte array</returns>
        Task<byte[]> GeneratePdfAsync(Report report, ReportData data);

        /// <summary>
        /// Generate an Excel report
        /// </summary>
        /// <param name="report">Report entity with metadata</param>
        /// <param name="data">Report data to include</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> GenerateExcelAsync(Report report, ReportData data);
    }

    /// <summary>
    /// Container for report data
    /// </summary>
    public class ReportData
    {
        public List<dynamic> Items { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public Dictionary<string, decimal> Statistics { get; set; } = new();
        public List<ChartData> Charts { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string KeyFindings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
    }

    /// <summary>
    /// Chart data for report visualizations
    /// </summary>
    public class ChartData
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "bar"; // bar, line, pie, etc.
        public List<string> Labels { get; set; } = new();
        public List<ChartDataset> Datasets { get; set; } = new();
    }

    /// <summary>
    /// Dataset for chart
    /// </summary>
    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
    }
}