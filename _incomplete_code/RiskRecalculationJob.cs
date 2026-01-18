using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for auto-recalculating risk scores when controls change.
    /// Runs hourly
    /// </summary>
    public class RiskRecalculationJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RiskRecalculationJob> _logger;

        private const int CRITICAL_THRESHOLD = 20;
        private const int HIGH_THRESHOLD = 12;

        public RiskRecalculationJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<RiskRecalculationJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task RecalculateResidualRisksAsync()
        {
            _logger.LogInformation("RiskRecalculationJob started at {Time}", DateTime.UtcNow);

            try
            {
                var tenants = await _context.Tenants.AsNoTracking()
                    .Where(t => t.IsActive).Select(t => t.Id).ToListAsync();

                var totalRecalculated = 0;
                var totalAlerts = 0;

                foreach (var tenantId in tenants)
                {
                    var (recalc, alerts) = await ProcessTenantRiskRecalculationAsync(tenantId);
                    totalRecalculated += recalc;
                    totalAlerts += alerts;
                }

                _logger.LogInformation("RiskRecalculationJob completed. Recalculated {Recalc}, alerts {Alerts}", 
                    totalRecalculated, totalAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RiskRecalculationJob failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<(int recalculated, int alerts)> ProcessTenantRiskRecalculationAsync(Guid tenantId)
        {
            var recalculatedCount = 0;
            var alertCount = 0;

            var recentChangeThreshold = DateTime.UtcNow.AddHours(-1);

            // Get risks with control changes or stale calculations
            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && r.IsActive && r.Status != "Closed" && r.Status != "Accepted")
                .Where(r => r.LastRecalculatedAt == null || r.LastRecalculatedAt < DateTime.UtcNow.AddDays(-7))
                .Include(r => r.MitigatingControls).ThenInclude(mc => mc.Control)
                .Take(100)
                .ToListAsync();

            foreach (var risk in risks)
            {
                var previousScore = risk.ResidualRiskScore;
                var previousLevel = risk.ResidualRiskLevel;

                // Calculate control effectiveness
                var controlEffectiveness = await CalculateControlEffectivenessAsync(risk, tenantId);

                // Inherent risk = Likelihood × Impact
                var inherentScore = (risk.Likelihood ?? 3) * (risk.Impact ?? 3);
                var residualScore = (int)Math.Ceiling(inherentScore * (1 - controlEffectiveness));

                risk.InherentRiskScore = inherentScore;
                risk.ResidualRiskScore = residualScore;
                risk.ResidualRiskLevel = residualScore switch
                {
                    >= CRITICAL_THRESHOLD => "Critical",
                    >= HIGH_THRESHOLD => "High",
                    >= 6 => "Medium",
                    _ => "Low"
                };
                risk.ControlEffectiveness = (decimal)controlEffectiveness;
                risk.LastRecalculatedAt = DateTime.UtcNow;
                risk.ModifiedDate = DateTime.UtcNow;
                risk.ModifiedBy = "System:RiskRecalculationJob";
                recalculatedCount++;

                // Alert if risk increased to Critical or High
                if (risk.ResidualRiskScore >= HIGH_THRESHOLD && 
                    (!previousScore.HasValue || risk.ResidualRiskScore > previousScore.Value))
                {
                    var recipientId = risk.OwnerId?.ToString() ?? "risk-manager";
                    var urgency = risk.ResidualRiskLevel == "Critical" ? "Critical" : "High";

                    await _notificationService.SendNotificationAsync(
                        risk.Id, recipientId, "RiskAlert",
                        $"[{urgency.ToUpper()}] Risk Alert: {risk.Title}",
                        $"Risk score has changed.\n\nRisk: {risk.Title}\n" +
                        $"Previous: {previousScore ?? 0} ({previousLevel ?? "N/A"})\n" +
                        $"Current: {risk.ResidualRiskScore} ({risk.ResidualRiskLevel})\n" +
                        $"Effectiveness: {controlEffectiveness:P0}",
                        urgency, tenantId);
                    alertCount++;
                }
            }

            // Update tenant risk profile
            var riskProfile = await _context.TenantRiskProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            if (riskProfile != null)
            {
                var activeRisks = await _context.Risks
                    .Where(r => r.TenantId == tenantId && r.IsActive && r.Status != "Closed")
                    .ToListAsync();

                riskProfile.TotalRiskCount = activeRisks.Count;
                riskProfile.CriticalRiskCount = activeRisks.Count(r => r.ResidualRiskLevel == "Critical");
                riskProfile.HighRiskCount = activeRisks.Count(r => r.ResidualRiskLevel == "High");
                riskProfile.MediumRiskCount = activeRisks.Count(r => r.ResidualRiskLevel == "Medium");
                riskProfile.LowRiskCount = activeRisks.Count(r => r.ResidualRiskLevel == "Low");
                riskProfile.AverageRiskScore = activeRisks.Any() ? (decimal)activeRisks.Average(r => r.ResidualRiskScore ?? 0) : 0;
                riskProfile.LastCalculatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (recalculatedCount, alertCount);
        }

        private async Task<double> CalculateControlEffectivenessAsync(Risk risk, Guid tenantId)
        {
            if (risk.MitigatingControls == null || !risk.MitigatingControls.Any()) return 0.0;

            var totalWeight = 0.0;
            var weightedEffectiveness = 0.0;

            foreach (var mc in risk.MitigatingControls)
            {
                if (mc.Control == null || !mc.Control.IsActive) continue;

                var tenantControl = await _context.TenantControlSets
                    .FirstOrDefaultAsync(tc => tc.CatalogControlId == mc.Control.Id && tc.TenantId == tenantId);

                var implScore = (tenantControl?.ImplementationStatus?.ToUpper()) switch
                {
                    "FULLY_IMPLEMENTED" or "IMPLEMENTED" => 1.0,
                    "PARTIALLY_IMPLEMENTED" => 0.5,
                    "IN_PROGRESS" => 0.3,
                    _ => 0.0
                };

                var compScore = (tenantControl?.ComplianceStatus?.ToUpper()) switch
                {
                    "COMPLIANT" or "EFFECTIVE" => 1.0,
                    "PARTIALLY_COMPLIANT" => 0.6,
                    "EVIDENCE_EXPIRED" => 0.2,
                    _ => 0.5
                };

                var score = (implScore * 0.4) + (compScore * 0.6);
                var weight = mc.EffectivenessWeight ?? 1.0;
                weightedEffectiveness += score * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? weightedEffectiveness / totalWeight : 0.0;
        }
    }

    /// <summary>
    /// KRI (Key Risk Indicator) Monitoring job - checks KRI thresholds and alerts.
    /// Runs every 30 minutes
    /// </summary>
    public class KRIMonitoringJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<KRIMonitoringJob> _logger;

        public KRIMonitoringJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<KRIMonitoringJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task CheckKRIThresholdsAsync()
        {
            _logger.LogInformation("KRIMonitoringJob started at {Time}", DateTime.UtcNow);

            var tenants = await _context.Tenants.AsNoTracking()
                .Where(t => t.IsActive).Select(t => t.Id).ToListAsync();

            var alerts = 0;
            foreach (var tenantId in tenants)
            {
                alerts += await CheckTenantKRIsAsync(tenantId);
            }

            _logger.LogInformation("KRIMonitoringJob completed. Sent {Alerts} alerts", alerts);
        }

        private async Task<int> CheckTenantKRIsAsync(Guid tenantId)
        {
            var alertCount = 0;

            var kris = await _context.KRIDefinitions
                .Where(k => k.TenantId == tenantId && k.IsActive)
                .ToListAsync();

            foreach (var kri in kris)
            {
                var currentValue = await CalculateKRIValueAsync(tenantId, kri);
                var previousValue = kri.CurrentValue;
                
                kri.CurrentValue = currentValue;
                kri.LastCalculatedAt = DateTime.UtcNow;

                // Check thresholds
                var status = "Normal";
                if (kri.CriticalThreshold.HasValue && currentValue >= kri.CriticalThreshold.Value)
                    status = "Critical";
                else if (kri.WarningThreshold.HasValue && currentValue >= kri.WarningThreshold.Value)
                    status = "Warning";

                if (status != kri.Status && status != "Normal" && kri.OwnerId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        kri.Id, kri.OwnerId.Value.ToString(), "KRIAlert",
                        $"[{status.ToUpper()}] KRI Alert: {kri.Name}",
                        $"KRI threshold breached.\n\nKRI: {kri.Name}\nValue: {currentValue:N2}\n" +
                        $"Warning Threshold: {kri.WarningThreshold}\nCritical Threshold: {kri.CriticalThreshold}",
                        status == "Critical" ? "Critical" : "High", tenantId);
                    alertCount++;
                }

                kri.Status = status;
            }

            await _context.SaveChangesAsync();
            return alertCount;
        }

        private async Task<decimal> CalculateKRIValueAsync(Guid tenantId, KRIDefinition kri)
        {
            // Calculate based on KRI formula/type
            return kri.CalculationType switch
            {
                "OverdueTasksCount" => await _context.WorkflowTasks
                    .CountAsync(t => t.TenantId == tenantId && t.Status != "Completed" && t.DueDate < DateTime.UtcNow),
                "OpenCriticalRisks" => await _context.Risks
                    .CountAsync(r => r.TenantId == tenantId && r.IsActive && r.ResidualRiskLevel == "Critical"),
                "ExpiredEvidenceCount" => await _context.EvidencePacks
                    .CountAsync(e => e.TenantId == tenantId && e.Status == "Expired"),
                "ComplianceGapPercent" => await CalculateComplianceGapAsync(tenantId),
                _ => kri.CurrentValue ?? 0
            };
        }

        private async Task<decimal> CalculateComplianceGapAsync(Guid tenantId)
        {
            var total = await _context.TenantControlSets
                .CountAsync(c => c.TenantId == tenantId && c.IsActive && c.ApplicabilityStatus == "Applicable");
            var compliant = await _context.TenantControlSets
                .CountAsync(c => c.TenantId == tenantId && c.IsActive && c.ComplianceStatus == "COMPLIANT");
            return total > 0 ? 100 - ((decimal)compliant / total * 100) : 0;
        }
    }
}
