using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Enhanced service for generating and managing GRC reports with PDF/Excel generation (Fixed version)
    /// </summary>
    public class EnhancedReportServiceFixed : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditEventService _auditService;
        private readonly ICurrentUserService _currentUser;
        private readonly IReportGenerator _reportGenerator;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<EnhancedReportServiceFixed> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public EnhancedReportServiceFixed(
            IUnitOfWork unitOfWork,
            IAuditEventService auditService,
            ICurrentUserService currentUser,
            IReportGenerator reportGenerator,
            IFileStorageService fileStorage,
            ILogger<EnhancedReportServiceFixed> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _currentUser = currentUser;
            _reportGenerator = reportGenerator;
            _fileStorage = fileStorage;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        /// <summary>
        /// Generate a compliance report with PDF output
        /// </summary>
        public async Task<(string reportId, string filePath)> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var tenantId = _currentUser.GetTenantId();
                var userName = _currentUser.GetUserName();

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("COMP"),
                    Title = $"Compliance Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    Type = "Compliance",
                    Status = "Generating",
                    Scope = "All compliance requirements",
                    ReportPeriodStart = startDate,
                    ReportPeriodEnd = endDate,
                    GeneratedBy = userName,
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                // Query compliance data
                var risks = await _unitOfWork.Risks
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .Where(r => r.CreatedDate >= startDate && r.CreatedDate <= endDate)
                    .ToListAsync();

                var audits = await _unitOfWork.Audits
                    .Query()
                    .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                    .Where(a => a.PlannedStartDate >= startDate && a.PlannedStartDate <= endDate)
                    .ToListAsync();

                var policies = await _unitOfWork.Policies
                    .Query()
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .ToListAsync();

                var controls = await _unitOfWork.Controls
                    .Query()
                    .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                    .ToListAsync();

                // Generate report content
                report.ExecutiveSummary = GenerateComplianceExecutiveSummary(risks, audits, policies, controls);
                report.KeyFindings = GenerateComplianceKeyFindings(risks, audits, controls);
                report.Recommendations = GenerateComplianceRecommendations(risks, audits, controls);
                report.TotalFindingsCount = risks.Count + audits.Count;
                report.CriticalFindingsCount = risks.Count(r => r.RiskLevel == "Critical") + audits.Count(a => a.Status == "Critical");

                // Prepare data for PDF generation
                var reportData = PrepareComplianceReportData(risks, audits, policies, controls);

                // Generate PDF
                var pdfContent = await _reportGenerator.GeneratePdfAsync(report, reportData);

                // Save PDF file
                var fileName = $"{report.ReportNumber}.pdf";
                var filePath = await _fileStorage.SaveFileAsync(pdfContent, fileName, "application/pdf");

                // Update report with file information
                report.FileUrl = await _fileStorage.GetFileUrlAsync(filePath);
                report.FilePath = filePath;
                report.FileName = fileName;
                report.ContentType = "application/pdf";
                report.FileSize = pdfContent.Length;
                report.PageCount = CalculatePageCount(pdfContent.Length);
                report.Status = "Generated";

                // Enforce policy before saving report (reports may contain sensitive data)
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Report",
                    resource: report,
                    dataClassification: "confidential",
                    owner: userName);

                // Save report to database
                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: userName,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber, report.FileUrl }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Compliance report generated: {report.Id} at {filePath}");

                return (reportId: report.Id.ToString(), filePath: report.FileUrl ?? filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                throw;
            }
        }

        /// <summary>
        /// Generate a risk report with PDF output
        /// </summary>
        public async Task<(string reportId, string filePath)> GenerateRiskReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var tenantId = _currentUser.GetTenantId();
                var userName = _currentUser.GetUserName();

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("RISK"),
                    Title = $"Risk Summary Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    Type = "Risk",
                    Status = "Generating",
                    Scope = "All identified risks",
                    ReportPeriodStart = startDate,
                    ReportPeriodEnd = endDate,
                    GeneratedBy = userName,
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                var risks = await _unitOfWork.Risks
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .Where(r => (r.IdentifiedDate ?? r.CreatedDate) >= startDate && (r.IdentifiedDate ?? r.CreatedDate) <= endDate)
                    .ToListAsync();

                report.ExecutiveSummary = GenerateRiskExecutiveSummary(risks);
                report.KeyFindings = GenerateRiskKeyFindings(risks);
                report.Recommendations = GenerateRiskRecommendations(risks);
                report.TotalFindingsCount = risks.Count;
                report.CriticalFindingsCount = risks.Count(r => r.RiskLevel == "Critical" || r.RiskLevel == "High");

                // Prepare data for PDF generation
                var reportData = PrepareRiskReportData(risks);

                // Generate PDF
                var pdfContent = await _reportGenerator.GeneratePdfAsync(report, reportData);

                // Save PDF file
                var fileName = $"{report.ReportNumber}.pdf";
                var filePath = await _fileStorage.SaveFileAsync(pdfContent, fileName, "application/pdf");

                // Update report with file information
                report.FileUrl = await _fileStorage.GetFileUrlAsync(filePath);
                report.FilePath = filePath;
                report.FileName = fileName;
                report.ContentType = "application/pdf";
                report.FileSize = pdfContent.Length;
                report.PageCount = CalculatePageCount(pdfContent.Length);
                report.Status = "Generated";

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: userName,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber, report.FileUrl }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Risk report generated: {report.Id} at {filePath}");

                return (reportId: report.Id.ToString(), filePath: report.FileUrl ?? filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating risk report");
                throw;
            }
        }

        /// <summary>
        /// Generate an audit report with PDF output
        /// </summary>
        public async Task<(string reportId, string filePath)> GenerateAuditReportAsync(Guid auditId)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(auditId);
                if (audit == null)
                {
                    throw new InvalidOperationException($"Audit '{auditId}' not found.");
                }

                var tenantId = audit.TenantId ?? _currentUser.GetTenantId();
                var userName = _currentUser.GetUserName();

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("AUDIT"),
                    Title = $"Audit Report - {audit.Title}",
                    Type = "Audit",
                    Status = "Generating",
                    Scope = audit.Scope ?? "Audit findings",
                    ReportPeriodStart = audit.PlannedStartDate,
                    ReportPeriodEnd = audit.ActualEndDate ?? audit.PlannedEndDate,
                    GeneratedBy = userName,
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                var findings = await _unitOfWork.AuditFindings
                    .Query()
                    .Where(f => f.AuditId == auditId && !f.IsDeleted)
                    .ToListAsync();

                report.ExecutiveSummary = GenerateAuditExecutiveSummary(audit, findings);
                report.KeyFindings = GenerateAuditKeyFindings(findings);
                report.Recommendations = GenerateAuditRecommendations(findings);
                report.TotalFindingsCount = findings.Count;
                report.CriticalFindingsCount = findings.Count(f => f.Severity == "Critical" || f.Severity == "High");

                // Prepare data for PDF generation
                var reportData = PrepareAuditReportData(audit, findings);

                // Generate PDF
                var pdfContent = await _reportGenerator.GeneratePdfAsync(report, reportData);

                // Save PDF file
                var fileName = $"{report.ReportNumber}.pdf";
                var filePath = await _fileStorage.SaveFileAsync(pdfContent, fileName, "application/pdf");

                // Update report with file information
                report.FileUrl = await _fileStorage.GetFileUrlAsync(filePath);
                report.FilePath = filePath;
                report.FileName = fileName;
                report.ContentType = "application/pdf";
                report.FileSize = pdfContent.Length;
                report.PageCount = CalculatePageCount(pdfContent.Length);
                report.Status = "Generated";

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: userName,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber, AuditId = auditId, report.FileUrl }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Audit report generated: {report.Id} at {filePath}");

                return (reportId: report.Id.ToString(), filePath: report.FileUrl ?? filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit report");
                throw;
            }
        }

        /// <summary>
        /// Generate a control assessment report with PDF output
        /// </summary>
        public async Task<(string reportId, string filePath)> GenerateControlReportAsync(Guid controlId)
        {
            try
            {
                var control = await _unitOfWork.Controls.GetByIdAsync(controlId);
                if (control == null)
                {
                    throw new InvalidOperationException($"Control '{controlId}' not found.");
                }

                var tenantId = control.TenantId ?? _currentUser.GetTenantId();
                var userName = _currentUser.GetUserName();

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("CTRL"),
                    Title = $"Control Assessment Report - {control.Name}",
                    Type = "Control",
                    Status = "Generating",
                    Scope = $"Control: {control.ControlId}",
                    ReportPeriodStart = DateTime.UtcNow.AddMonths(-1),
                    ReportPeriodEnd = DateTime.UtcNow,
                    GeneratedBy = userName,
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                report.ExecutiveSummary = GenerateControlExecutiveSummary(control);
                report.KeyFindings = $"Control Effectiveness: {GetEffectivenessText(control.Effectiveness)}";
                report.Recommendations = GenerateControlRecommendations(control);
                report.TotalFindingsCount = 1;
                report.CriticalFindingsCount = control.Effectiveness <= 2 ? 1 : 0;

                // Prepare data for PDF generation
                var reportData = PrepareControlReportData(control);

                // Generate PDF
                var pdfContent = await _reportGenerator.GeneratePdfAsync(report, reportData);

                // Save PDF file
                var fileName = $"{report.ReportNumber}.pdf";
                var filePath = await _fileStorage.SaveFileAsync(pdfContent, fileName, "application/pdf");

                // Update report with file information
                report.FileUrl = await _fileStorage.GetFileUrlAsync(filePath);
                report.FilePath = filePath;
                report.FileName = fileName;
                report.ContentType = "application/pdf";
                report.FileSize = pdfContent.Length;
                report.PageCount = CalculatePageCount(pdfContent.Length);
                report.Status = "Generated";

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: userName,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber, ControlId = controlId, report.FileUrl }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Control report generated: {report.Id} at {filePath}");

                return (reportId: report.Id.ToString(), filePath: report.FileUrl ?? filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating control report");
                throw;
            }
        }

        /// <summary>
        /// Update an existing report
        /// </summary>
        public async Task<Report> UpdateReportAsync(Guid reportId, ReportEditDto dto)
        {
            try
            {
                var report = await _unitOfWork.Reports.GetByIdAsync(reportId);

                if (report == null || report.IsDeleted)
                {
                    throw new InvalidOperationException($"Report {reportId} not found");
                }

                // Verify tenant access
                if (report.TenantId != _currentUser.GetTenantId())
                {
                    throw new UnauthorizedAccessException("Access denied to this report");
                }

                // Update properties
                report.Title = dto.Title;
                report.Description = dto.Description;
                report.ExecutiveSummary = dto.ExecutiveSummary;
                report.KeyFindings = dto.KeyFindings;
                report.Recommendations = dto.Recommendations;
                report.DeliveredTo = dto.DeliveredTo;
                report.DeliveryDate = dto.DeliveryDate;
                report.Status = dto.Status;
                report.ModifiedDate = DateTime.UtcNow;
                report.ModifiedBy = _currentUser.GetUserName();

                await _unitOfWork.Reports.UpdateAsync(report);
                await _unitOfWork.SaveChangesAsync();

                // Audit log
                await _auditService.LogEventAsync(
                    tenantId: report.TenantId,
                    eventType: "ReportUpdated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Update",
                    actor: _currentUser.GetUserName(),
                    payloadJson: JsonSerializer.Serialize(dto),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Report {reportId} updated by {_currentUser.GetUserName()}");

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", reportId);
                throw;
            }
        }

        /// <summary>
        /// Delete a report (soft delete)
        /// </summary>
        public async Task<bool> DeleteReportAsync(Guid reportId)
        {
            try
            {
                var report = await _unitOfWork.Reports.GetByIdAsync(reportId);

                if (report == null || report.IsDeleted)
                {
                    return false;
                }

                // Verify tenant access
                if (report.TenantId != _currentUser.GetTenantId())
                {
                    throw new UnauthorizedAccessException("Access denied to this report");
                }

                // Soft delete - BaseEntity has IsDeleted, DeletedDate might not exist
                report.IsDeleted = true;
                report.ModifiedDate = DateTime.UtcNow;
                report.ModifiedBy = _currentUser.GetUserName();

                await _unitOfWork.Reports.UpdateAsync(report);
                await _unitOfWork.SaveChangesAsync();

                // Audit log
                await _auditService.LogEventAsync(
                    tenantId: report.TenantId,
                    eventType: "ReportDeleted",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Delete",
                    actor: _currentUser.GetUserName(),
                    payloadJson: JsonSerializer.Serialize(new { reportId }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Report {reportId} deleted by {_currentUser.GetUserName()}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", reportId);
                throw;
            }
        }

        /// <summary>
        /// Generate executive summary
        /// </summary>
        public async Task<object> GenerateExecutiveSummaryAsync()
        {
            try
            {
                var tenantId = _currentUser.GetTenantId();

                var risks = await _unitOfWork.Risks
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                var controls = await _unitOfWork.Controls
                    .Query()
                    .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                    .ToListAsync();

                var audits = await _unitOfWork.Audits
                    .Query()
                    .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                    .ToListAsync();

                var criticalRisks = risks.Count(r => r.RiskLevel == "Critical");
                var highRisks = risks.Count(r => r.RiskLevel == "High");
                var effectiveControls = controls.Count(c => c.Effectiveness >= 4);
                var controlsCompliant = effectiveControls;
                var controlsNonCompliant = controls.Count - effectiveControls;

                // Calculate compliance score (simplified)
                var complianceScore = controls.Count > 0
                    ? Math.Round((double)effectiveControls / controls.Count * 100, 1)
                    : 100.0;

                var riskLevel = criticalRisks > 0 ? "High" : highRisks > 5 ? "Moderate" : "Low";

                return new
                {
                    complianceScore,
                    riskLevel,
                    controlsCompliant,
                    controlsNonCompliant,
                    criticalRisks,
                    highRisks,
                    generatedDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating executive summary");
                throw;
            }
        }

        /// <summary>
        /// Get report by ID
        /// </summary>
        public async Task<object> GetReportAsync(string reportId)
        {
            try
            {
                if (!Guid.TryParse(reportId, out var id))
                {
                    throw new ArgumentException("Invalid report ID format.");
                }

                var report = await _unitOfWork.Reports.GetByIdAsync(id);
                if (report == null || report.IsDeleted)
                {
                    throw new InvalidOperationException($"Report '{reportId}' not found.");
                }

                // Verify tenant access
                if (report.TenantId != _currentUser.GetTenantId())
                {
                    throw new UnauthorizedAccessException("Access denied to this report");
                }

                return new
                {
                    reportId = report.Id.ToString(),
                    reportNumber = report.ReportNumber,
                    title = report.Title,
                    type = report.Type,
                    status = report.Status,
                    generatedDate = report.GeneratedDate,
                    pages = report.PageCount,
                    fileSize = report.FileSize.HasValue ? $"{report.FileSize / 1024.0 / 1024.0:F2} MB" : "N/A",
                    fileUrl = report.FileUrl,
                    filePath = report.FilePath,
                    fileName = report.FileName,
                    contentType = report.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report");
                throw;
            }
        }

        /// <summary>
        /// List all generated reports
        /// </summary>
        public async Task<List<object>> ListReportsAsync()
        {
            try
            {
                var tenantId = _currentUser.GetTenantId();

                var reports = await _unitOfWork.Reports
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .OrderByDescending(r => r.GeneratedDate ?? r.CreatedDate)
                    .Take(100)
                    .ToListAsync();

                return reports.Select(r => new
                {
                    reportId = r.Id.ToString(),
                    reportNumber = r.ReportNumber,
                    title = r.Title,
                    type = r.Type,
                    status = r.Status,
                    generatedDate = r.GeneratedDate ?? r.CreatedDate
                }).Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing reports");
                throw;
            }
        }

        // Helper methods for report content generation

        private ReportData PrepareComplianceReportData(List<Risk> risks, List<Audit> audits, List<Policy> policies, List<Control> controls)
        {
            var data = new ReportData
            {
                ExecutiveSummary = GenerateComplianceExecutiveSummary(risks, audits, policies, controls),
                KeyFindings = GenerateComplianceKeyFindings(risks, audits, controls),
                Recommendations = GenerateComplianceRecommendations(risks, audits, controls),
                Statistics = new Dictionary<string, decimal>
                {
                    { "Total Risks", risks.Count },
                    { "Critical Risks", risks.Count(r => r.RiskLevel == "Critical") },
                    { "Total Audits", audits.Count },
                    { "Open Audits", audits.Count(a => a.Status != "Completed") },
                    { "Total Policies", policies.Count },
                    { "Effective Controls", controls.Count(c => c.Effectiveness >= 4) }
                },
                Items = new List<dynamic>()
            };

            // Add compliance items
            foreach (var policy in policies.Take(20))
            {
                dynamic item = new ExpandoObject();
                item.PolicyNumber = policy.PolicyNumber;
                item.Title = policy.Title;
                item.Status = policy.Status;
                item.EffectiveDate = policy.EffectiveDate;
                data.Items.Add(item);
            }

            return data;
        }

        private ReportData PrepareRiskReportData(List<Risk> risks)
        {
            var data = new ReportData
            {
                ExecutiveSummary = GenerateRiskExecutiveSummary(risks),
                KeyFindings = GenerateRiskKeyFindings(risks),
                Recommendations = GenerateRiskRecommendations(risks),
                Statistics = new Dictionary<string, decimal>
                {
                    { "Total Risks", risks.Count },
                    { "Critical", risks.Count(r => r.RiskLevel == "Critical") },
                    { "High", risks.Count(r => r.RiskLevel == "High") },
                    { "Medium", risks.Count(r => r.RiskLevel == "Medium") },
                    { "Low", risks.Count(r => r.RiskLevel == "Low") },
                    { "Unmitigated", risks.Count(r => r.Status != "Mitigated") }
                },
                Items = new List<dynamic>()
            };

            // Add risk items using the actual Risk entity properties
            foreach (var risk in risks)
            {
                dynamic item = new ExpandoObject();
                item.RiskNumber = risk.RiskNumber ?? $"RISK-{risk.Id.ToString().Substring(0, 8)}";
                item.Title = risk.DisplayTitle;
                item.Level = risk.RiskLevel;
                item.Status = risk.Status;
                item.Owner = risk.Owner;
                item.InherentScore = risk.InherentRisk;
                item.ResidualScore = risk.ResidualRisk;
                data.Items.Add(item);
            }

            return data;
        }

        private ReportData PrepareAuditReportData(Audit audit, List<AuditFinding> findings)
        {
            var data = new ReportData
            {
                ExecutiveSummary = GenerateAuditExecutiveSummary(audit, findings),
                KeyFindings = GenerateAuditKeyFindings(findings),
                Recommendations = GenerateAuditRecommendations(findings),
                Statistics = new Dictionary<string, decimal>
                {
                    { "Total Findings", findings.Count },
                    { "Critical", findings.Count(f => f.Severity == "Critical") },
                    { "High", findings.Count(f => f.Severity == "High") },
                    { "Medium", findings.Count(f => f.Severity == "Medium") },
                    { "Low", findings.Count(f => f.Severity == "Low") },
                    { "Unresolved", findings.Count(f => f.Status != "Resolved") }
                },
                Items = new List<dynamic>()
            };

            // Add finding items
            foreach (var finding in findings)
            {
                dynamic item = new ExpandoObject();
                item.FindingNumber = $"FIND-{finding.Id.ToString().Substring(0, 8)}";
                item.Title = finding.Title;
                item.Severity = finding.Severity;
                item.Status = finding.Status;
                item.DueDate = finding.TargetDate;
                data.Items.Add(item);
            }

            return data;
        }

        private ReportData PrepareControlReportData(Control control)
        {
            var data = new ReportData
            {
                ExecutiveSummary = GenerateControlExecutiveSummary(control),
                KeyFindings = $"Control Effectiveness: {GetEffectivenessText(control.Effectiveness)}",
                Recommendations = GenerateControlRecommendations(control),
                Statistics = new Dictionary<string, decimal>
                {
                    { "Effectiveness Score", control.Effectiveness },
                    { "Test Frequency (days)", 30 }, // Default test frequency
                    { "Last Test Days Ago", control.LastTestDate.HasValue ? (decimal)(DateTime.UtcNow - control.LastTestDate.Value).Days : 0 }
                },
                Items = new List<dynamic>()
            };

            // Add control details
            dynamic item = new ExpandoObject();
            item.ControlId = control.ControlId;
            item.Name = control.Name;
            item.Type = control.Type;
            item.Category = control.Category;
            item.Status = control.Status;
            item.Effectiveness = GetEffectivenessText(control.Effectiveness);
            data.Items.Add(item);

            return data;
        }

        private string GenerateComplianceExecutiveSummary(List<Risk> risks, List<Audit> audits, List<Policy> policies, List<Control> controls)
        {
            var effectiveControls = controls.Count(c => c.Effectiveness >= 4);
            var complianceRate = controls.Count > 0 ? (effectiveControls * 100.0 / controls.Count) : 100.0;

            return $"Compliance Summary: The organization maintains {policies.Count} active policies with {complianceRate:F1}% control effectiveness. " +
                   $"Current assessment identifies {risks.Count} risks with {risks.Count(r => r.RiskLevel == "Critical")} critical items requiring immediate attention. " +
                   $"{audits.Count} audits have been conducted with {audits.Count(a => a.Status == "Completed")} completed successfully.";
        }

        private string GenerateComplianceKeyFindings(List<Risk> risks, List<Audit> audits, List<Control> controls)
        {
            var criticalRisks = risks.Count(r => r.RiskLevel == "Critical");
            var openAudits = audits.Count(a => a.Status != "Completed");
            var ineffectiveControls = controls.Count(c => c.Effectiveness < 3);

            return $"Key Findings: {criticalRisks} critical risks identified, {openAudits} audits pending completion, " +
                   $"{ineffectiveControls} controls require improvement to meet compliance standards.";
        }

        private string GenerateComplianceRecommendations(List<Risk> risks, List<Audit> audits, List<Control> controls)
        {
            var recommendations = new List<string>();

            if (risks.Any(r => r.RiskLevel == "Critical"))
                recommendations.Add("Prioritize mitigation of critical risks");

            if (audits.Any(a => a.Status != "Completed"))
                recommendations.Add("Complete pending audit activities");

            if (controls.Any(c => c.Effectiveness < 3))
                recommendations.Add("Enhance control effectiveness through additional testing and refinement");

            if (!recommendations.Any())
                recommendations.Add("Maintain current compliance posture with regular monitoring");

            return "Recommendations: " + string.Join("; ", recommendations) + ".";
        }

        private string GenerateRiskExecutiveSummary(List<Risk> risks)
        {
            var criticalCount = risks.Count(r => r.RiskLevel == "Critical");
            var highCount = risks.Count(r => r.RiskLevel == "High");
            var mitigatedCount = risks.Count(r => r.Status == "Mitigated");

            return $"Risk Summary: Portfolio contains {risks.Count} identified risks with {criticalCount} critical and {highCount} high priority items. " +
                   $"{mitigatedCount} risks have been successfully mitigated. Average residual risk score across the portfolio is " +
                   $"{risks.Where(r => r.ResidualRisk > 0).DefaultIfEmpty().Average(r => r?.ResidualRisk ?? 0):F1}.";
        }

        private string GenerateRiskKeyFindings(List<Risk> risks)
        {
            var unmitigated = risks.Count(r => r.Status != "Mitigated");
            var highImpact = risks.Count(r => r.InherentRisk >= 15);

            return $"Key Findings: {unmitigated} risks require active mitigation efforts. {highImpact} risks have high inherent scores requiring continuous monitoring.";
        }

        private string GenerateRiskRecommendations(List<Risk> risks)
        {
            var recommendations = new List<string>();

            if (risks.Any(r => r.RiskLevel == "Critical" && r.Status != "Mitigated"))
                recommendations.Add("Implement immediate mitigation plans for critical risks");

            if (risks.Any(r => string.IsNullOrEmpty(r.Owner)))
                recommendations.Add("Assign risk owners to ensure accountability");

            if (risks.Any(r => r.ResidualRisk > 15))
                recommendations.Add("Review and enhance controls for high residual risk items");

            return "Recommendations: " + string.Join("; ", recommendations) + ".";
        }

        private string GenerateAuditExecutiveSummary(Audit audit, List<AuditFinding> findings)
        {
            var criticalFindings = findings.Count(f => f.Severity == "Critical");
            var resolvedFindings = findings.Count(f => f.Status == "Resolved");

            return $"Audit Summary: The audit '{audit.Title}' identified {findings.Count} findings with {criticalFindings} critical items. " +
                   $"{resolvedFindings} findings have been resolved. Audit scope covered {audit.Scope}.";
        }

        private string GenerateAuditKeyFindings(List<AuditFinding> findings)
        {
            var unresolvedCritical = findings.Count(f => f.Severity == "Critical" && f.Status != "Resolved");
            var overdueFindings = findings.Count(f => f.TargetDate < DateTime.UtcNow && f.Status != "Resolved");

            return $"Key Findings: {findings.Count} total findings identified. {unresolvedCritical} critical findings require immediate remediation. " +
                   $"{overdueFindings} findings are past their due date.";
        }

        private string GenerateAuditRecommendations(List<AuditFinding> findings)
        {
            var recommendations = new List<string>();

            if (findings.Any(f => f.Severity == "Critical" && f.Status != "Resolved"))
                recommendations.Add("Prioritize resolution of critical findings");

            if (findings.Any(f => f.TargetDate < DateTime.UtcNow && f.Status != "Resolved"))
                recommendations.Add("Address overdue remediation items immediately");

            recommendations.Add("Implement corrective action plans with clear timelines");

            return "Recommendations: " + string.Join("; ", recommendations) + ".";
        }

        private string GenerateControlExecutiveSummary(Control control)
        {
            var effectiveness = GetEffectivenessText(control.Effectiveness);
            var testStatus = control.LastTestDate.HasValue
                ? $"last tested on {control.LastTestDate.Value:yyyy-MM-dd}"
                : "pending initial testing";

            return $"Control Assessment: Control '{control.Name}' ({control.ControlId}) is assessed as {effectiveness}. " +
                   $"Control type: {control.Type}, Category: {control.Category}, Status: {control.Status}. Control was {testStatus}.";
        }

        private string GenerateControlRecommendations(Control control)
        {
            if (control.Effectiveness <= 2)
            {
                return "Recommendations: Immediate review required. Redesign control activities, enhance documentation, " +
                       "increase testing frequency, and implement compensating controls until effectiveness improves.";
            }
            else if (control.Effectiveness == 3)
            {
                return "Recommendations: Continue monitoring control effectiveness. Consider enhancements to achieve full effectiveness. " +
                       "Maintain regular testing schedule and update control documentation.";
            }

            return "Recommendations: Maintain current control design and operation. Continue regular testing and monitoring. " +
                   "Document any changes in business processes that may impact control effectiveness.";
        }

        private string GetEffectivenessText(int effectiveness)
        {
            return effectiveness switch
            {
                1 => "Ineffective",
                2 => "Partially Effective",
                3 => "Largely Effective",
                4 => "Effective",
                5 => "Highly Effective",
                _ => "Not Assessed"
            };
        }

        private async Task<string> GenerateReportNumberAsync(string prefix)
        {
            var year = DateTime.UtcNow.Year;
            var reportsThisYear = await _unitOfWork.Reports
                .Query()
                .Where(r => r.ReportNumber.StartsWith($"{prefix}-{year}"))
                .CountAsync();

            var sequence = reportsThisYear + 1;
            return $"{prefix}-{year}-{sequence:D3}";
        }

        private int CalculatePageCount(long fileSize)
        {
            // Estimate pages based on file size (rough approximation)
            // Average PDF page is about 30-50KB
            var estimatedPages = Math.Max(1, (int)(fileSize / 40000));
            return estimatedPages;
        }

        /// <summary>
        /// Update an existing report
        /// </summary>
        public async Task<object> UpdateReportAsync(string reportId, UpdateReportDto dto)
        {
            if (!Guid.TryParse(reportId, out var id))
                throw new ArgumentException("Invalid report ID");

            var report = await _unitOfWork.Reports
                .Query()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (report == null)
                throw new InvalidOperationException("Report not found");

            if (!string.IsNullOrEmpty(dto.Title))
                report.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description))
                report.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.ExecutiveSummary))
                report.ExecutiveSummary = dto.ExecutiveSummary;
            if (!string.IsNullOrEmpty(dto.KeyFindings))
                report.KeyFindings = dto.KeyFindings;
            if (!string.IsNullOrEmpty(dto.Recommendations))
                report.Recommendations = dto.Recommendations;
            if (!string.IsNullOrEmpty(dto.Status))
                report.Status = dto.Status;

            report.ModifiedDate = DateTime.UtcNow;
            report.ModifiedBy = _currentUser.GetUserName();

            await _unitOfWork.SaveChangesAsync();

            return new { id = report.Id, reportNumber = report.ReportNumber, title = report.Title, status = report.Status };
        }

        /// <summary>
        /// Soft delete a report
        /// </summary>
        public async Task<bool> DeleteReportAsync(string reportId)
        {
            if (!Guid.TryParse(reportId, out var id))
                return false;

            var report = await _unitOfWork.Reports
                .Query()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (report == null)
                return false;

            report.IsDeleted = true;
            report.ModifiedDate = DateTime.UtcNow;
            report.ModifiedBy = _currentUser.GetUserName();

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Mark report as delivered
        /// </summary>
        public async Task<object> DeliverReportAsync(string reportId, string deliveredTo, string? notes = null)
        {
            if (!Guid.TryParse(reportId, out var id))
                throw new ArgumentException("Invalid report ID");

            var report = await _unitOfWork.Reports
                .Query()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (report == null)
                throw new InvalidOperationException("Report not found");

            report.Status = "Delivered";
            report.DeliveredTo = deliveredTo;
            report.DeliveryDate = DateTime.UtcNow;
            report.ModifiedDate = DateTime.UtcNow;
            report.ModifiedBy = _currentUser.GetUserName();

            await _unitOfWork.SaveChangesAsync();

            return new { id = report.Id, reportNumber = report.ReportNumber, status = report.Status, deliveredTo, deliveryDate = report.DeliveryDate };
        }

        /// <summary>
        /// Get report file for download
        /// </summary>
        public async Task<(byte[] fileData, string fileName, string contentType)?> DownloadReportAsync(string reportId, string format = "pdf")
        {
            if (!Guid.TryParse(reportId, out var id))
                return null;

            var report = await _unitOfWork.Reports
                .Query()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (report == null)
                return null;

            var content = JsonSerializer.SerializeToUtf8Bytes(new
            {
                report.ReportNumber,
                report.Title,
                report.Type,
                report.Status,
                report.ExecutiveSummary,
                Findings = report.KeyFindings,
                Recommendations = report.Recommendations,
                report.GeneratedDate,
                report.GeneratedBy
            }, new JsonSerializerOptions { WriteIndented = true });

            var fileName = $"{report.ReportNumber}.{format}";
            var contentType = format.ToLower() == "pdf" ? "application/pdf" : "application/json";

            return (content, fileName, contentType);
        }
    }
}