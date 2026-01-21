using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using System.Security.Claims;

namespace GrcMvc.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time KPI updates
    /// Provides live dashboard metrics to connected clients
    /// </summary>
    [Authorize]
    public class KpiHub : Hub
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<KpiHub> _logger;

        public KpiHub(GrcDbContext context, ILogger<KpiHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        private Guid? GetTenantId()
        {
            var tenantClaim = Context.User?.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }

        public override async Task OnConnectedAsync()
        {
            var tenantId = GetTenantId();
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("KpiHub: User {UserId} connected from tenant {TenantId}", userId, tenantId);

            // Add to tenant-specific group for targeted updates
            if (tenantId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            }

            // Send initial KPI data
            await SendKpiUpdate();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var tenantId = GetTenantId();
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("KpiHub: User {UserId} disconnected", userId);

            if (tenantId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client requests a KPI refresh
        /// </summary>
        public async Task RequestKpiUpdate()
        {
            await SendKpiUpdate();
        }

        /// <summary>
        /// Send KPI data to the requesting client
        /// </summary>
        private async Task SendKpiUpdate()
        {
            try
            {
                var tenantId = GetTenantId();
                var kpis = await CalculateKpis(tenantId);
                await Clients.Caller.SendAsync("ReceiveKpiUpdate", kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending KPI update");
            }
        }

        /// <summary>
        /// Broadcast KPI update to all clients in a tenant group
        /// Called by background services when data changes
        /// </summary>
        public async Task BroadcastToTenant(Guid tenantId)
        {
            try
            {
                var kpis = await CalculateKpis(tenantId);
                await Clients.Group($"tenant_{tenantId}").SendAsync("ReceiveKpiUpdate", kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting KPI update to tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Subscribe to specific KPI category updates
        /// </summary>
        public async Task SubscribeToCategory(string category)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"kpi_{category}");
            _logger.LogInformation("Client {ConnectionId} subscribed to KPI category: {Category}",
                Context.ConnectionId, category);
        }

        /// <summary>
        /// Unsubscribe from specific KPI category updates
        /// </summary>
        public async Task UnsubscribeFromCategory(string category)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"kpi_{category}");
        }

        private async Task<KpiDataPacket> CalculateKpis(Guid? tenantId)
        {
            var now = DateTime.UtcNow;

            // Compliance KPIs
            var totalControls = await _context.Controls
                .CountAsync(c => !c.IsDeleted && (tenantId == null || c.TenantId == tenantId));
            var effectiveControls = await _context.Controls
                .CountAsync(c => !c.IsDeleted && (c.Status == "Effective" || c.Status == "Active") &&
                           (tenantId == null || c.TenantId == tenantId));
            var complianceScore = totalControls > 0
                ? Math.Round((decimal)effectiveControls / totalControls * 100, 1)
                : 0;

            // Risk KPIs
            var openRisks = await _context.Risks
                .CountAsync(r => !r.IsDeleted && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId));
            var criticalRisks = await _context.Risks
                .CountAsync(r => !r.IsDeleted && r.Status != "Closed" &&
                           (r.Probability ?? 0) * (r.Impact ?? 0) >= 16 &&
                           (tenantId == null || r.TenantId == tenantId));
            var avgRiskScore = await _context.Risks
                .Where(r => !r.IsDeleted && r.Status != "Closed" && (tenantId == null || r.TenantId == tenantId))
                .AverageAsync(r => (double?)((r.Probability ?? 0) * (r.Impact ?? 0))) ?? 0;

            // Assessment KPIs
            var pendingAssessments = await _context.Assessments
                .CountAsync(a => !a.IsDeleted && a.Status == "InProgress" && (tenantId == null || a.TenantId == tenantId));
            var overdueAssessments = await _context.Assessments
                .CountAsync(a => !a.IsDeleted && a.EndDate < now && a.Status != "Completed" &&
                           (tenantId == null || a.TenantId == tenantId));

            // Audit KPIs
            var openFindings = await _context.AuditFindings
                .CountAsync(f => !f.IsDeleted && f.Status != "Closed" && (tenantId == null || f.TenantId == tenantId));
            var activeAudits = await _context.Audits
                .CountAsync(a => !a.IsDeleted && a.Status == "InProgress" && (tenantId == null || a.TenantId == tenantId));

            // Evidence KPIs
            var recentEvidence = await _context.Evidence
                .CountAsync(e => !e.IsDeleted && e.CollectionDate >= now.AddDays(-30) &&
                           (tenantId == null || e.TenantId == tenantId));
            var expiringEvidence = await _context.Evidence
                .CountAsync(e => !e.IsDeleted && e.ExpirationDate <= now.AddDays(30) && e.ExpirationDate > now &&
                           (tenantId == null || e.TenantId == tenantId));

            // Workflow KPIs
            var overdueActions = await _context.WorkflowTasks
                .CountAsync(t => !t.IsDeleted && t.DueDate < now && t.Status != "Completed" &&
                           (tenantId == null || t.TenantId == tenantId));
            var pendingApprovals = await _context.WorkflowTasks
                .CountAsync(t => !t.IsDeleted && t.TaskType == "Approval" && t.Status == "Pending" &&
                           (tenantId == null || t.TenantId == tenantId));

            return new KpiDataPacket
            {
                Timestamp = now,
                Compliance = new ComplianceKpis
                {
                    Score = complianceScore,
                    TotalControls = totalControls,
                    EffectiveControls = effectiveControls,
                    Trend = 0 // Calculate from historical data
                },
                Risk = new RiskKpis
                {
                    OpenCount = openRisks,
                    CriticalCount = criticalRisks,
                    AverageScore = Math.Round((decimal)avgRiskScore, 1),
                    Trend = 0
                },
                Assessment = new AssessmentKpis
                {
                    PendingCount = pendingAssessments,
                    OverdueCount = overdueAssessments
                },
                Audit = new AuditKpis
                {
                    OpenFindings = openFindings,
                    ActiveAudits = activeAudits
                },
                Evidence = new EvidenceKpis
                {
                    RecentCount = recentEvidence,
                    ExpiringCount = expiringEvidence
                },
                Workflow = new WorkflowKpis
                {
                    OverdueActions = overdueActions,
                    PendingApprovals = pendingApprovals
                }
            };
        }
    }

    #region KPI Data Models

    public class KpiDataPacket
    {
        public DateTime Timestamp { get; set; }
        public ComplianceKpis Compliance { get; set; } = new();
        public RiskKpis Risk { get; set; } = new();
        public AssessmentKpis Assessment { get; set; } = new();
        public AuditKpis Audit { get; set; } = new();
        public EvidenceKpis Evidence { get; set; } = new();
        public WorkflowKpis Workflow { get; set; } = new();
    }

    public class ComplianceKpis
    {
        public decimal Score { get; set; }
        public int TotalControls { get; set; }
        public int EffectiveControls { get; set; }
        public decimal Trend { get; set; }
    }

    public class RiskKpis
    {
        public int OpenCount { get; set; }
        public int CriticalCount { get; set; }
        public decimal AverageScore { get; set; }
        public decimal Trend { get; set; }
    }

    public class AssessmentKpis
    {
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class AuditKpis
    {
        public int OpenFindings { get; set; }
        public int ActiveAudits { get; set; }
    }

    public class EvidenceKpis
    {
        public int RecentCount { get; set; }
        public int ExpiringCount { get; set; }
    }

    public class WorkflowKpis
    {
        public int OverdueActions { get; set; }
        public int PendingApprovals { get; set; }
    }

    #endregion

    /// <summary>
    /// Service to broadcast KPI updates from background jobs
    /// </summary>
    public interface IKpiNotificationService
    {
        Task NotifyKpiUpdate(Guid tenantId);
        Task NotifyAllTenants();
    }

    public class KpiNotificationService : IKpiNotificationService
    {
        private readonly IHubContext<KpiHub> _hubContext;
        private readonly GrcDbContext _context;
        private readonly ILogger<KpiNotificationService> _logger;

        public KpiNotificationService(
            IHubContext<KpiHub> hubContext,
            GrcDbContext context,
            ILogger<KpiNotificationService> logger)
        {
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
        }

        public async Task NotifyKpiUpdate(Guid tenantId)
        {
            try
            {
                // Simplified KPI calculation for broadcast
                var kpis = new
                {
                    timestamp = DateTime.UtcNow,
                    complianceScore = 0, // Calculate
                    openRisks = await _context.Risks.CountAsync(r => !r.IsDeleted && r.Status != "Closed" && r.TenantId == tenantId)
                };

                await _hubContext.Clients.Group($"tenant_{tenantId}").SendAsync("ReceiveKpiUpdate", kpis);
                _logger.LogDebug("Broadcasted KPI update to tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying KPI update for tenant {TenantId}", tenantId);
            }
        }

        public async Task NotifyAllTenants()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveKpiRefreshSignal");
                _logger.LogDebug("Sent KPI refresh signal to all clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying all tenants");
            }
        }
    }
}
