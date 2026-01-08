using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for generating reports in various formats
    /// </summary>
    public class ReportGeneratorService : IReportGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportGeneratorService> _logger;

        static ReportGeneratorService()
        {
            // Configure QuestPDF license (Community license for commercial use with revenue < $1M)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public ReportGeneratorService(
            IConfiguration configuration,
            ILogger<ReportGeneratorService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generate a PDF report using QuestPDF
        /// </summary>
        public async Task<byte[]> GeneratePdfAsync(Report report, ReportData data)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var document = Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(11));

                            // Header
                            page.Header()
                                .Height(3, Unit.Centimetre)
                                .Background(Colors.Grey.Lighten4)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(text =>
                                {
                                    text.Span(report.Title).FontSize(20).Bold();
                                    text.Line($"\nReport Number: {report.ReportNumber}");
                                });

                            // Content
                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Column(column =>
                                {
                                    column.Spacing(20);

                                    // Report Information Section
                                    column.Item().Element(ComposeReportInfo);

                                    // Executive Summary
                                    if (!string.IsNullOrEmpty(data.ExecutiveSummary))
                                    {
                                        column.Item().Element(container => ComposeSection(container, "Executive Summary", data.ExecutiveSummary));
                                    }

                                    // Key Findings
                                    if (!string.IsNullOrEmpty(data.KeyFindings))
                                    {
                                        column.Item().Element(container => ComposeSection(container, "Key Findings", data.KeyFindings));
                                    }

                                    // Statistics
                                    if (data.Statistics.Any())
                                    {
                                        column.Item().Element(container => ComposeStatistics(container, data.Statistics));
                                    }

                                    // Data Table
                                    if (data.Items.Any())
                                    {
                                        column.Item().PageBreak();
                                        column.Item().Element(container => ComposeDataTable(container, data.Items, report.Type));
                                    }

                                    // Recommendations
                                    if (!string.IsNullOrEmpty(data.Recommendations))
                                    {
                                        column.Item().Element(container => ComposeSection(container, "Recommendations", data.Recommendations));
                                    }
                                });

                            // Footer
                            page.Footer()
                                .Height(1, Unit.Centimetre)
                                .Background(Colors.Grey.Lighten4)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("Page ");
                                    text.CurrentPageNumber();
                                    text.Span(" of ");
                                    text.TotalPages();
                                    text.Span($" | Generated: {report.GeneratedDate?.ToString("yyyy-MM-dd HH:mm") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")} UTC");
                                });

                            void ComposeReportInfo(IContainer container)
                            {
                                container.Background(Colors.Grey.Lighten5)
                                    .Padding(10)
                                    .Column(column =>
                                    {
                                        column.Spacing(5);
                                        column.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"Report Type: {report.Type}").Bold();
                                            row.RelativeItem().AlignRight().Text($"Status: {report.Status}");
                                        });
                                        column.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"Period: {report.ReportPeriodStart:yyyy-MM-dd} to {report.ReportPeriodEnd:yyyy-MM-dd}");
                                            row.RelativeItem().AlignRight().Text($"Generated By: {report.GeneratedBy}");
                                        });
                                    });
                            }

                            void ComposeSection(IContainer container, string title, string content)
                            {
                                container.Column(column =>
                                {
                                    column.Item().Text(title).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                                    column.Item().PaddingTop(5).Text(content);
                                });
                            }

                            void ComposeStatistics(IContainer container, Dictionary<string, decimal> statistics)
                            {
                                container.Column(column =>
                                {
                                    column.Item().Text("Key Metrics").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                                    column.Item().PaddingTop(10).Grid(grid =>
                                    {
                                        grid.Columns(3);
                                        grid.Spacing(10);

                                        foreach (var stat in statistics)
                                        {
                                            grid.Item().Background(Colors.Blue.Lighten5)
                                                .Padding(10)
                                                .Column(innerColumn =>
                                                {
                                                    innerColumn.Item().Text(stat.Key).FontSize(10);
                                                    innerColumn.Item().Text(stat.Value.ToString("N0")).FontSize(16).Bold();
                                                });
                                        }
                                    });
                                });
                            }

                            void ComposeDataTable(IContainer container, List<dynamic> items, string reportType)
                            {
                                container.Column(column =>
                                {
                                    column.Item().Text($"{reportType} Details").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                                    column.Item().PaddingTop(10).Table(table =>
                                    {
                                        // Define table structure based on report type
                                        DefineTableColumns(table, reportType);

                                        // Add table data
                                        AddTableData(table, items, reportType);
                                    });
                                });
                            }
                        });
                    });

                    return document.GeneratePdf();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                throw new InvalidOperationException("Failed to generate PDF report", ex);
            }
        }

        /// <summary>
        /// Generate an Excel report using ClosedXML
        /// </summary>
        public async Task<byte[]> GenerateExcelAsync(Report report, ReportData data)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook();

                    // Summary worksheet
                    var summarySheet = workbook.Worksheets.Add("Summary");
                    BuildSummarySheet(summarySheet, report, data);

                    // Data worksheet
                    if (data.Items.Any())
                    {
                        var dataSheet = workbook.Worksheets.Add($"{report.Type} Data");
                        BuildDataSheet(dataSheet, data.Items, report.Type);
                    }

                    // Statistics worksheet
                    if (data.Statistics.Any())
                    {
                        var statsSheet = workbook.Worksheets.Add("Statistics");
                        BuildStatisticsSheet(statsSheet, data.Statistics);
                    }

                    // Save to memory stream
                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw new IntegrationException("Excel", "Failed to generate Excel report", ex);
            }
        }

        private void BuildSummarySheet(IXLWorksheet worksheet, Report report, ReportData data)
        {
            // Title
            worksheet.Cell(1, 1).Value = report.Title;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 6).Merge();

            // Report Info
            int row = 3;
            worksheet.Cell(row, 1).Value = "Report Number:";
            worksheet.Cell(row, 2).Value = report.ReportNumber;
            row++;

            worksheet.Cell(row, 1).Value = "Report Type:";
            worksheet.Cell(row, 2).Value = report.Type;
            row++;

            worksheet.Cell(row, 1).Value = "Status:";
            worksheet.Cell(row, 2).Value = report.Status;
            row++;

            worksheet.Cell(row, 1).Value = "Period:";
            worksheet.Cell(row, 2).Value = $"{report.ReportPeriodStart:yyyy-MM-dd} to {report.ReportPeriodEnd:yyyy-MM-dd}";
            row++;

            worksheet.Cell(row, 1).Value = "Generated By:";
            worksheet.Cell(row, 2).Value = report.GeneratedBy;
            row++;

            worksheet.Cell(row, 1).Value = "Generated Date:";
            worksheet.Cell(row, 2).Value = report.GeneratedDate?.ToString("yyyy-MM-dd HH:mm") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
            row += 2;

            // Executive Summary
            if (!string.IsNullOrEmpty(data.ExecutiveSummary))
            {
                worksheet.Cell(row, 1).Value = "Executive Summary";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                row++;
                worksheet.Cell(row, 1).Value = data.ExecutiveSummary;
                worksheet.Range(row, 1, row, 6).Merge();
                row += 2;
            }

            // Key Findings
            if (!string.IsNullOrEmpty(data.KeyFindings))
            {
                worksheet.Cell(row, 1).Value = "Key Findings";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                row++;
                worksheet.Cell(row, 1).Value = data.KeyFindings;
                worksheet.Range(row, 1, row, 6).Merge();
                row += 2;
            }

            // Recommendations
            if (!string.IsNullOrEmpty(data.Recommendations))
            {
                worksheet.Cell(row, 1).Value = "Recommendations";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                row++;
                worksheet.Cell(row, 1).Value = data.Recommendations;
                worksheet.Range(row, 1, row, 6).Merge();
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
        }

        private void BuildDataSheet(IXLWorksheet worksheet, List<dynamic> items, string reportType)
        {
            if (!items.Any())
                return;

            // Get columns from first item
            var firstItem = items.First() as IDictionary<string, object>;
            if (firstItem == null)
                return;

            // Headers
            int col = 1;
            foreach (var key in firstItem.Keys)
            {
                worksheet.Cell(1, col).Value = key;
                worksheet.Cell(1, col).Style.Font.Bold = true;
                worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                col++;
            }

            // Data
            int row = 2;
            foreach (var item in items)
            {
                var dict = item as IDictionary<string, object>;
                if (dict != null)
                {
                    col = 1;
                    foreach (var value in dict.Values)
                    {
                        worksheet.Cell(row, col).Value = value?.ToString() ?? "";
                        col++;
                    }
                    row++;
                }
            }

            // Format as table
            var dataRange = worksheet.Range(1, 1, row - 1, firstItem.Keys.Count);
            var table = dataRange.CreateTable($"{reportType}Data");
            table.Theme = XLTableTheme.TableStyleLight9;

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
        }

        private void BuildStatisticsSheet(IXLWorksheet worksheet, Dictionary<string, decimal> statistics)
        {
            worksheet.Cell(1, 1).Value = "Key Metrics";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;

            int row = 3;
            worksheet.Cell(row, 1).Value = "Metric";
            worksheet.Cell(row, 2).Value = "Value";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;
            foreach (var stat in statistics)
            {
                worksheet.Cell(row, 1).Value = stat.Key;
                worksheet.Cell(row, 2).Value = stat.Value;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
        }

        private void DefineTableColumns(TableDescriptor table, string reportType)
        {
            table.ColumnsDefinition(columns =>
            {
                switch (reportType.ToUpper())
                {
                    case "RISK":
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        break;
                    case "COMPLIANCE":
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        break;
                    case "AUDIT":
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        break;
                    default:
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        break;
                }
            });
        }

        private void AddTableData(TableDescriptor table, List<dynamic> items, string reportType)
        {
            // Header
            table.Header(header =>
            {
                switch (reportType.ToUpper())
                {
                    case "RISK":
                        header.Cell().Background(Colors.Grey.Medium).Text("Risk ID").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Description").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Level").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Status").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Owner").FontColor(Colors.White).Bold();
                        break;
                    case "COMPLIANCE":
                        header.Cell().Background(Colors.Grey.Medium).Text("Requirement").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Description").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Status").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Compliance %").FontColor(Colors.White).Bold();
                        break;
                    case "AUDIT":
                        header.Cell().Background(Colors.Grey.Medium).Text("Finding ID").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Description").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Severity").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Status").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Due Date").FontColor(Colors.White).Bold();
                        break;
                    default:
                        header.Cell().Background(Colors.Grey.Medium).Text("ID").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Description").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Medium).Text("Status").FontColor(Colors.White).Bold();
                        break;
                }
            });

            // Add sample data rows (customize based on actual data structure)
            foreach (var item in items.Take(50)) // Limit to 50 rows for PDF
            {
                // This is a simplified example - adjust based on actual data structure
                table.Cell().BorderBottom(1).Text("ID-001").FontSize(9);
                table.Cell().BorderBottom(1).Text("Sample description").FontSize(9);
                table.Cell().BorderBottom(1).Text("Active").FontSize(9);

                if (reportType.ToUpper() == "RISK")
                {
                    table.Cell().BorderBottom(1).Text("In Progress").FontSize(9);
                    table.Cell().BorderBottom(1).Text("John Doe").FontSize(9);
                }
                else if (reportType.ToUpper() == "AUDIT")
                {
                    table.Cell().BorderBottom(1).Text("Open").FontSize(9);
                    table.Cell().BorderBottom(1).Text("2024-12-31").FontSize(9);
                }
                else if (reportType.ToUpper() == "COMPLIANCE")
                {
                    table.Cell().BorderBottom(1).Text("85%").FontSize(9);
                }
            }
        }
    }
}