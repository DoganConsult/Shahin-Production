using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// AI Report Builder - Generate compliance reports with AI summaries
    /// </summary>
    public class AIReportBuilderViewComponent : ViewComponent
    {
        private readonly IClaudeAgentService _claudeService;
        private readonly IReportService _reportService;

        public AIReportBuilderViewComponent(IClaudeAgentService claudeService, IReportService reportService)
        {
            _claudeService = claudeService;
            _reportService = reportService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? reportType = null)
        {
            var model = new AIReportBuilderModel
            {
                ReportType = reportType,
                AiAvailable = await _claudeService.IsAvailableAsync(),
                AvailableReportTypes = new List<ReportTypeOption>
                {
                    new() { Id = "executive_summary", Name = "Executive Summary", Icon = "bi-file-earmark-text", Description = "High-level overview for leadership" },
                    new() { Id = "compliance_status", Name = "Compliance Status", Icon = "bi-clipboard-check", Description = "Framework compliance assessment" },
                    new() { Id = "risk_assessment", Name = "Risk Assessment", Icon = "bi-exclamation-triangle", Description = "Detailed risk analysis report" },
                    new() { Id = "control_effectiveness", Name = "Control Effectiveness", Icon = "bi-shield-check", Description = "Control testing results" },
                    new() { Id = "audit_findings", Name = "Audit Findings", Icon = "bi-search", Description = "Audit results and recommendations" },
                    new() { Id = "gap_analysis", Name = "Gap Analysis", Icon = "bi-bar-chart", Description = "Compliance gaps and remediation" },
                    new() { Id = "trend_analysis", Name = "Trend Analysis", Icon = "bi-graph-up", Description = "Historical trends and projections" }
                }
            };

            return View(model);
        }
    }

    public class AIReportBuilderModel
    {
        public string ReportType { get; set; }
        public bool AiAvailable { get; set; }
        public List<ReportTypeOption> AvailableReportTypes { get; set; } = new();
        public DateTime ReportPeriodStart { get; set; } = DateTime.Today.AddMonths(-3);
        public DateTime ReportPeriodEnd { get; set; } = DateTime.Today;
    }

    public class ReportTypeOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }
}
