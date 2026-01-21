using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Risk Heatmap ViewComponent - Interactive probability vs impact matrix
    /// </summary>
    public class RiskHeatmapViewComponent : ViewComponent
    {
        private readonly IRiskService _riskService;
        private readonly ILogger<RiskHeatmapViewComponent> _logger;

        public RiskHeatmapViewComponent(IRiskService riskService, ILogger<RiskHeatmapViewComponent> logger)
        {
            _riskService = riskService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var tenantIdClaim = HttpContext.User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    _logger.LogWarning("No tenant ID found in claims for risk heatmap");
                    return View(new RiskHeatmapViewModel());
                }

                var result = await _riskService.GetHeatMapAsync(tenantId);
                if (!result.IsSuccess || result.Value == null)
                {
                    return View(new RiskHeatmapViewModel());
                }

                var viewModel = new RiskHeatmapViewModel
                {
                    TotalRisks = result.Value.Cells.Sum(c => c.RiskCount),
                    GeneratedAt = result.Value.GeneratedAt,
                    Cells = result.Value.Cells.Select(c => new RiskHeatmapCell
                    {
                        Probability = c.Likelihood,
                        Impact = c.Impact,
                        Count = c.RiskCount
                    }).ToList()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading risk heatmap data");
                return View(new RiskHeatmapViewModel());
            }
        }
    }

    public class RiskHeatmapViewModel
    {
        public List<RiskHeatmapCell> Cells { get; set; } = new();
        public int TotalRisks { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class RiskHeatmapCell
    {
        public int Probability { get; set; }
        public int Impact { get; set; }
        public int Count { get; set; }
        public string SeverityLevel => CalculateSeverity();

        private string CalculateSeverity()
        {
            var score = Probability * Impact;
            return score switch
            {
                >= 20 => "Critical",
                >= 15 => "High",
                >= 10 => "Medium",
                >= 5 => "Low",
                _ => "VeryLow"
            };
        }
    }
}
