using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Dtos;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for generating and managing GRC reports
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IUnitOfWork unitOfWork,
            IAuditEventService auditService,
            ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a compliance report
        /// </summary>
        public async Task<(string reportId, string filePath)> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // TODO: Get tenant ID from current context
                var tenantId = Guid.Empty; // Placeholder - should come from current user context

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("COMP"),
                    Title = $"Compliance Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    Type = "Compliance",
                    Status = "Draft",
                    Scope = "All compliance requirements",
                    ReportPeriodStart = startDate,
                    ReportPeriodEnd = endDate,
                    GeneratedBy = "System", // TODO: Get from current user
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    CorrelationId = Guid.NewGuid().ToString()
                };

                // Query compliance data
                var risks = await _unitOfWork.Risks
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                var audits = await _unitOfWork.Audits
                    .Query()
                    .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                    .ToListAsync();

                var policies = await _unitOfWork.Policies
                    .Query()
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .ToListAsync();

                // Generate report content
                report.ExecutiveSummary = GenerateComplianceExecutiveSummary(risks, audits, policies);
                report.KeyFindings = GenerateComplianceKeyFindings(risks, audits);
                report.Recommendations = GenerateComplianceRecommendations(risks, audits);
                report.TotalFindingsCount = risks.Count + audits.Count;
                report.CriticalFindingsCount = risks.Count(r => r.RiskLevel == "Critical") + audits.Count(a => a.Status == "Critical");

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: report.GeneratedBy,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Compliance report generated: {report.Id}");

                return (reportId: report.Id.ToString(), filePath: $"/reports/{report.ReportNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                throw;
            }
        }

        /// <summary>
        /// Generate a risk report
        /// </summary>
        public async Task<(string reportId, string filePath)> GenerateRiskReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var tenantId = Guid.Empty; // TODO: Get from current user context

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("RISK"),
                    Title = $"Risk Summary Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    Type = "Risk",
                    Status = "Draft",
                    Scope = "All identified risks",
                    ReportPeriodStart = startDate,
                    ReportPeriodEnd = endDate,
                    GeneratedBy = "System",
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    CorrelationId = Guid.NewGuid().ToString()
                };

                var risks = await _unitOfWork.Risks
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .ToListAsync();

                report.ExecutiveSummary = GenerateRiskExecutiveSummary(risks);
                report.KeyFindings = GenerateRiskKeyFindings(risks);
                report.Recommendations = GenerateRiskRecommendations(risks);
                report.TotalFindingsCount = risks.Count;
                report.CriticalFindingsCount = risks.Count(r => r.RiskLevel == "Critical" || r.RiskLevel == "High");

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: report.GeneratedBy,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Risk report generated: {report.Id}");

                return (reportId: report.Id.ToString(), filePath: $"/reports/{report.ReportNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating risk report");
                throw;
            }
        }

        /// <summary>
        /// Generate an audit report
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

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = audit.TenantId ?? Guid.Empty,
                    ReportNumber = await GenerateReportNumberAsync("AUDIT"),
                    Title = $"Audit Report - {audit.Title}",
                    Type = "Audit",
                    Status = "Draft",
                    Scope = audit.Scope ?? "Audit findings",
                    ReportPeriodStart = audit.PlannedStartDate,
                    ReportPeriodEnd = audit.ActualEndDate ?? audit.PlannedEndDate,
                    GeneratedBy = "System",
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
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

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogEventAsync(
                    tenantId: audit.TenantId ?? Guid.Empty,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: report.GeneratedBy,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber, AuditId = auditId }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Audit report generated: {report.Id}");

                return (reportId: report.Id.ToString(), filePath: $"/reports/{report.ReportNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit report");
                throw;
            }
        }

        /// <summary>
        /// Generate a control assessment report
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

                var tenantId = control.TenantId.HasValue ? control.TenantId.Value : Guid.Empty;

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = await GenerateReportNumberAsync("CTRL"),
                    Title = $"Control Assessment Report - {control.Name}",
                    Type = "Control",
                    Status = "Draft",
                    Scope = $"Control: {control.ControlId}",
                    ReportPeriodStart = DateTime.UtcNow.AddMonths(-1),
                    ReportPeriodEnd = DateTime.UtcNow,
                    GeneratedBy = "System",
                    GeneratedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    CorrelationId = Guid.NewGuid().ToString()
                };

                report.ExecutiveSummary = GenerateControlExecutiveSummary(control);
                report.KeyFindings = $"Control Effectiveness: {GetEffectivenessText(control.Effectiveness)}";
                report.Recommendations = GenerateControlRecommendations(control);
                report.TotalFindingsCount = 1;
                report.CriticalFindingsCount = control.Effectiveness <= 2 ? 1 : 0;

                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "ReportGenerated",
                    affectedEntityType: "Report",
                    affectedEntityId: report.Id.ToString(),
                    action: "Generate",
                    actor: report.GeneratedBy,
                    payloadJson: JsonSerializer.Serialize(new { report.Type, report.ReportNumber, ControlId = controlId }),
                    correlationId: report.CorrelationId
                );

                _logger.LogInformation($"Control report generated: {report.Id}");

                return (reportId: report.Id.ToString(), filePath: $"/reports/{report.ReportNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating control report");
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
                var tenantId = Guid.Empty; // TODO: Get from current user context

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
                    fileUrl = report.FileUrl
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
                var tenantId = Guid.Empty; // TODO: Get from current user context

                var reports = await _unitOfWork.Reports
                    .Query()
                    .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                    .OrderByDescending(r => r.GeneratedDate ?? r.CreatedDate)
                    .Take(100)
                    .ToListAsync();

                return reports.Select(r => new
                {
                    reportId = r.Id.ToString(),
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

        private string GenerateComplianceExecutiveSummary(List<Risk> risks, List<Audit> audits, List<Policy> policies)
        {
            return $"Compliance Summary: {policies.Count} policies, {risks.Count} risks identified, {audits.Count} audits conducted.";
        }

        private string GenerateComplianceKeyFindings(List<Risk> risks, List<Audit> audits)
        {
            var criticalRisks = risks.Count(r => r.RiskLevel == "Critical");
            var openAudits = audits.Count(a => a.Status != "Completed");
            return $"Key Findings: {criticalRisks} critical risks, {openAudits} open audits requiring attention.";
        }

        private string GenerateComplianceRecommendations(List<Risk> risks, List<Audit> audits)
        {
            return "Recommendations: Address critical risks immediately, complete pending audits, and update compliance policies.";
        }

        private string GenerateRiskExecutiveSummary(List<Risk> risks)
        {
            return $"Risk Summary: {risks.Count} total risks identified. {risks.Count(r => r.RiskLevel == "Critical")} critical, {risks.Count(r => r.RiskLevel == "High")} high priority.";
        }

        private string GenerateRiskKeyFindings(List<Risk> risks)
        {
            var unmitigated = risks.Count(r => r.Status != "Mitigated");
            return $"Key Findings: {unmitigated} risks require mitigation actions.";
        }

        private string GenerateRiskRecommendations(List<Risk> risks)
        {
            return "Recommendations: Develop mitigation plans for high and critical risks, assign owners, and track progress.";
        }

        private string GenerateAuditExecutiveSummary(Audit audit, List<AuditFinding> findings)
        {
            return $"Audit Summary: {findings.Count} findings identified in audit '{audit.Title}'. {findings.Count(f => f.Severity == "Critical")} critical findings.";
        }

        private string GenerateAuditKeyFindings(List<AuditFinding> findings)
        {
            return $"Key Findings: {findings.Count} total findings. {findings.Count(f => f.Status != "Resolved")} findings require remediation.";
        }

        private string GenerateAuditRecommendations(List<AuditFinding> findings)
        {
            return "Recommendations: Address all critical findings, develop action plans, and implement corrective measures.";
        }

        private string GenerateControlExecutiveSummary(Control control)
        {
            return $"Control Assessment: Control '{control.Name}' ({control.ControlId}) effectiveness: {GetEffectivenessText(control.Effectiveness)}.";
        }

        private string GenerateControlRecommendations(Control control)
        {
            if (control.Effectiveness <= 2)
            {
                return "Recommendations: Review control design, enhance control activities, and reassess effectiveness.";
            }
            return "Recommendations: Continue monitoring control effectiveness and update as needed.";
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
    }
}
