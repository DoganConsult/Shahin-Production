using System;

namespace GrcMvc.Configuration;

/// <summary>
/// Centralized risk scoring configuration
/// Single source of truth for risk level thresholds
/// </summary>
public static class RiskScoringConfiguration
{
    /// <summary>
    /// Risk score thresholds (based on Probability × Impact matrix)
    /// Standard 5x5 matrix: scores range from 1-25
    /// </summary>
    public static class Thresholds
    {
        public const int CriticalMin = 20;  // 20-25: Critical (red)
        public const int HighMin = 12;      // 12-19: High (orange)
        public const int MediumMin = 6;     // 6-11: Medium (yellow)
        public const int LowMin = 1;        // 1-5: Low (green)
    }

    /// <summary>
    /// Get risk level from score
    /// </summary>
    public static string GetRiskLevel(int score)
    {
        return score switch
        {
            >= Thresholds.CriticalMin => "Critical",
            >= Thresholds.HighMin => "High",
            >= Thresholds.MediumMin => "Medium",
            _ => "Low"
        };
    }

    /// <summary>
    /// Get risk level from probability and impact
    /// </summary>
    public static string GetRiskLevel(int probability, int impact)
    {
        return GetRiskLevel(probability * impact);
    }

    /// <summary>
    /// Calculate inherent risk score
    /// Inherent = Probability × Impact (before controls)
    /// </summary>
    public static int CalculateInherentRisk(int probability, int impact)
    {
        return Math.Clamp(probability * impact, 1, 25);
    }

    /// <summary>
    /// Calculate residual risk score
    /// Residual = Inherent × (1 - ControlEffectiveness%)
    /// </summary>
    public static int CalculateResidualRisk(int inherentRisk, decimal controlEffectiveness)
    {
        if (controlEffectiveness < 0 || controlEffectiveness > 100)
            throw new ArgumentOutOfRangeException(nameof(controlEffectiveness), "Control effectiveness must be between 0 and 100");

        var reduction = inherentRisk * (controlEffectiveness / 100);
        var residual = (int)Math.Round(inherentRisk - reduction);
        return Math.Clamp(residual, 1, 25);
    }

    /// <summary>
    /// Validate that residual risk is not greater than inherent risk
    /// </summary>
    public static bool ValidateResidualRisk(int inherentRisk, int residualRisk)
    {
        return residualRisk <= inherentRisk && residualRisk >= 1;
    }

    /// <summary>
    /// Get color for risk level (for UI)
    /// </summary>
    public static string GetRiskColor(string riskLevel)
    {
        return riskLevel.ToLower() switch
        {
            "critical" => "#dc3545", // Red
            "high" => "#fd7e14",     // Orange
            "medium" => "#ffc107",   // Yellow
            "low" => "#28a745",      // Green
            _ => "#6c757d"           // Gray
        };
    }

    /// <summary>
    /// Get CSS class for risk level
    /// </summary>
    public static string GetRiskCssClass(string riskLevel)
    {
        return riskLevel.ToLower() switch
        {
            "critical" => "badge-danger",
            "high" => "badge-warning",
            "medium" => "badge-info",
            "low" => "badge-success",
            _ => "badge-secondary"
        };
    }

    /// <summary>
    /// Valid risk statuses for state machine
    /// </summary>
    public static class Statuses
    {
        public const string Identified = "Identified";
        public const string Assessed = "Assessed";
        public const string PendingAcceptance = "Pending Acceptance";
        public const string Accepted = "Accepted";
        public const string RequiresMitigation = "Requires Mitigation";
        public const string Mitigated = "Mitigated";
        public const string Monitoring = "Monitoring";
        public const string Closed = "Closed";
        public const string Active = "Active"; // Legacy compatibility

        public static readonly string[] AllStatuses = 
        {
            Identified, Assessed, PendingAcceptance, Accepted,
            RequiresMitigation, Mitigated, Monitoring, Closed, Active
        };
    }
}
