using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GrcMvc.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time dashboard updates
    /// Pushes live updates to connected clients when data changes
    /// </summary>
    [Authorize]
    public class DashboardHub : Hub
    {
        private readonly ILogger<DashboardHub> _logger;

        public DashboardHub(ILogger<DashboardHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantId = Context.User?.FindFirst("TenantId")?.Value;

            if (!string.IsNullOrEmpty(tenantId))
            {
                // Join tenant-specific group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
                _logger.LogInformation("User {UserId} connected to dashboard hub for tenant {TenantId}",
                    userId, tenantId);
            }

            // Join user-specific group for personal notifications
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} disconnected from dashboard hub", userId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to specific dashboard updates
        /// </summary>
        public async Task SubscribeToDashboard(string dashboardType)
        {
            var tenantId = Context.User?.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                var group = $"dashboard_{tenantId}_{dashboardType}";
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                _logger.LogDebug("Client subscribed to {Group}", group);
            }
        }

        /// <summary>
        /// Unsubscribe from specific dashboard updates
        /// </summary>
        public async Task UnsubscribeFromDashboard(string dashboardType)
        {
            var tenantId = Context.User?.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                var group = $"dashboard_{tenantId}_{dashboardType}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            }
        }

        /// <summary>
        /// Request immediate dashboard refresh
        /// </summary>
        public async Task RequestDashboardRefresh(string dashboardType)
        {
            var tenantId = Context.User?.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                // Notify the server to refresh and broadcast
                await Clients.Caller.SendAsync("DashboardRefreshStarted", dashboardType);
            }
        }
    }

    /// <summary>
    /// Service to push dashboard updates to connected clients
    /// </summary>
    public interface IDashboardHubService
    {
        Task NotifyDashboardUpdateAsync(Guid tenantId, string dashboardType, object data);
        Task NotifyWidgetUpdateAsync(Guid tenantId, string widgetType, object data);
        Task NotifyUserTaskUpdateAsync(string userId, object taskData);
        Task NotifyTenantAlertAsync(Guid tenantId, string alertType, string message);
        Task NotifyComplianceScoreChangeAsync(Guid tenantId, string frameworkCode, decimal oldScore, decimal newScore);
        Task NotifyRiskLevelChangeAsync(Guid tenantId, Guid riskId, string riskName, string newLevel);
    }

    /// <summary>
    /// Implementation of dashboard hub service
    /// </summary>
    public class DashboardHubService : IDashboardHubService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<DashboardHubService> _logger;

        public DashboardHubService(
            IHubContext<DashboardHub> hubContext,
            ILogger<DashboardHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyDashboardUpdateAsync(Guid tenantId, string dashboardType, object data)
        {
            var group = $"dashboard_{tenantId}_{dashboardType}";
            await _hubContext.Clients.Group(group).SendAsync("DashboardUpdated", new
            {
                DashboardType = dashboardType,
                Data = data,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogDebug("Pushed {DashboardType} update to tenant {TenantId}", dashboardType, tenantId);
        }

        public async Task NotifyWidgetUpdateAsync(Guid tenantId, string widgetType, object data)
        {
            var tenantGroup = $"tenant_{tenantId}";
            await _hubContext.Clients.Group(tenantGroup).SendAsync("WidgetUpdated", new
            {
                WidgetType = widgetType,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyUserTaskUpdateAsync(string userId, object taskData)
        {
            var userGroup = $"user_{userId}";
            await _hubContext.Clients.Group(userGroup).SendAsync("TaskUpdated", taskData);
        }

        public async Task NotifyTenantAlertAsync(Guid tenantId, string alertType, string message)
        {
            var tenantGroup = $"tenant_{tenantId}";
            await _hubContext.Clients.Group(tenantGroup).SendAsync("Alert", new
            {
                AlertType = alertType,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyComplianceScoreChangeAsync(Guid tenantId, string frameworkCode, decimal oldScore, decimal newScore)
        {
            var tenantGroup = $"tenant_{tenantId}";
            await _hubContext.Clients.Group(tenantGroup).SendAsync("ComplianceScoreChanged", new
            {
                FrameworkCode = frameworkCode,
                OldScore = oldScore,
                NewScore = newScore,
                Delta = newScore - oldScore,
                Direction = newScore > oldScore ? "up" : "down",
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Compliance score changed for {Framework}: {OldScore} → {NewScore}",
                frameworkCode, oldScore, newScore);
        }

        public async Task NotifyRiskLevelChangeAsync(Guid tenantId, Guid riskId, string riskName, string newLevel)
        {
            var tenantGroup = $"tenant_{tenantId}";
            await _hubContext.Clients.Group(tenantGroup).SendAsync("RiskLevelChanged", new
            {
                RiskId = riskId,
                RiskName = riskName,
                NewLevel = newLevel,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
