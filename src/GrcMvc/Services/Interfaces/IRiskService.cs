using GrcMvc.Common;
using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IRiskService
    {
        // ===== CRUD Operations =====
        Task<Result<RiskDto>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<RiskDto>>> GetAllAsync();
        Task<Result<RiskDto>> CreateAsync(CreateRiskDto dto);
        Task<Result<RiskDto>> UpdateAsync(Guid id, UpdateRiskDto dto);
        Task<Result> DeleteAsync(Guid id);
        Task<Result<IEnumerable<RiskDto>>> GetByStatusAsync(string status);
        Task<Result<IEnumerable<RiskDto>>> GetByCategoryAsync(string category);
        Task<Result<RiskStatisticsDto>> GetStatisticsAsync();
        Task<Result> AcceptAsync(Guid id);

        // ===== Risk Scoring (NEW) =====
        /// <summary>
        /// Calculate risk score based on likelihood and impact
        /// </summary>
        Task<Result<RiskScoreResultDto>> CalculateRiskScoreAsync(Guid riskId);

        /// <summary>
        /// Calculate residual risk after controls
        /// </summary>
        Task<Result<RiskScoreResultDto>> CalculateResidualRiskAsync(Guid riskId);

        /// <summary>
        /// Get risk score history for trending
        /// </summary>
        Task<Result<List<RiskScoreHistoryDto>>> GetScoreHistoryAsync(Guid riskId, int months = 12);

        // ===== Risk-Assessment Linkage (NEW) =====
        /// <summary>
        /// Link risk to an assessment finding
        /// </summary>
        Task<Result<RiskDto>> LinkToAssessmentAsync(Guid riskId, Guid assessmentId, string? findingReference = null);

        /// <summary>
        /// Get risks identified from a specific assessment
        /// </summary>
        Task<Result<List<RiskDto>>> GetRisksByAssessmentAsync(Guid assessmentId);

        /// <summary>
        /// Auto-generate risks from assessment findings
        /// </summary>
        Task<Result<List<RiskDto>>> GenerateRisksFromAssessmentAsync(Guid assessmentId, Guid tenantId);

        // ===== Risk-Control Mapping (NEW) =====
        /// <summary>
        /// Link a control to mitigate a risk
        /// </summary>
        Task<Result<RiskControlMappingDto>> LinkControlAsync(Guid riskId, Guid controlId, int expectedEffectiveness);

        /// <summary>
        /// Get all controls linked to a risk
        /// </summary>
        Task<Result<List<RiskControlMappingDto>>> GetLinkedControlsAsync(Guid riskId);

        /// <summary>
        /// Calculate control effectiveness for a risk
        /// </summary>
        Task<Result<decimal>> CalculateControlEffectivenessAsync(Guid riskId);

        // ===== Risk Posture (NEW) =====
        /// <summary>
        /// Get overall risk posture for a tenant
        /// </summary>
        Task<Result<RiskPostureSummaryDto>> GetRiskPostureAsync(Guid tenantId);

        /// <summary>
        /// Get risk heat map data
        /// </summary>
        Task<Result<RiskHeatMapDto>> GetHeatMapAsync(Guid tenantId);
    }

    // ===== Risk DTOs =====

    public class RiskScoreResultDto
    {
        public Guid RiskId { get; set; }
        public int Likelihood { get; set; }
        public int Impact { get; set; }
        public int InherentRisk { get; set; }
        public int ResidualRisk { get; set; }
        public string RiskLevel { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string RiskLevelAr { get; set; } = string.Empty;
        public decimal ControlEffectiveness { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RiskScoreHistoryDto
    {
        public DateTime Date { get; set; }
        public int InherentRisk { get; set; }
        public int ResidualRisk { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class RiskControlMappingDto
    {
        public Guid Id { get; set; }
        public Guid RiskId { get; set; }
        public Guid ControlId { get; set; }
        public string ControlCode { get; set; } = string.Empty;
        public string ControlName { get; set; } = string.Empty;
        public int ExpectedEffectiveness { get; set; }
        public int ActualEffectiveness { get; set; }
        public string MappingStrength { get; set; } = string.Empty; // Strong, Moderate, Weak
        public DateTime MappedAt { get; set; }
    }

    public class RiskPostureSummaryDto
    {
        public Guid TenantId { get; set; }
        public int TotalRisks { get; set; }
        public int CriticalRisks { get; set; }
        public int HighRisks { get; set; }
        public int MediumRisks { get; set; }
        public int LowRisks { get; set; }
        public int AcceptedRisks { get; set; }
        public int MitigatedRisks { get; set; }
        public int OpenRisks { get; set; }
        public decimal OverallRiskScore { get; set; }
        public string RiskTrend { get; set; } = string.Empty; // Improving, Stable, Deteriorating
        public decimal AverageControlEffectiveness { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RiskHeatMapDto
    {
        public Guid TenantId { get; set; }
        public List<HeatMapCellDto> Cells { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class HeatMapCellDto
    {
        public int Likelihood { get; set; } // 1-5
        public int Impact { get; set; } // 1-5
        public int RiskCount { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> RiskNames { get; set; } = new();
    }
}