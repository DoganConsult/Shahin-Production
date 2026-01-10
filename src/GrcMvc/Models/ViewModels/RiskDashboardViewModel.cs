using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace GrcMvc.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Risk Manager Dashboard
    /// </summary>
    public class RiskDashboardViewModel
    {
        public RiskStatisticsDto? Statistics { get; set; }
        public RiskPostureSummaryDto? RiskPosture { get; set; }
        public RiskHeatMapDto? HeatMap { get; set; }
        public List<RiskDto> TopRisks { get; set; } = new();
        public List<RiskDto> RecentRisks { get; set; } = new();
        public List<RiskDto> UpcomingReviews { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
