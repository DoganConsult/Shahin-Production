using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.BackgroundJobs;

/// <summary>
/// Integration Health Monitor Background Job - Monitors integration connector health
/// Runs every 15 minutes via Hangfire recurring job
/// </summary>
public class IntegrationHealthMonitorJob
{
    private readonly GrcDbContext _context;
    private readonly IIntegrationAgentService _integrationAgent;
    private readonly ILogger<IntegrationHealthMonitorJob> _logger;

    public IntegrationHealthMonitorJob(
        GrcDbContext context,
        IIntegrationAgentService integrationAgent,
        ILogger<IntegrationHealthMonitorJob> logger)
    {
        _context = context;
        _integrationAgent = integrationAgent;
        _logger = logger;
    }

    public async Task MonitorAllIntegrationsAsync()
    {
        _logger.LogInformation("[IntegrationHealthMonitorJob] Starting integration health monitoring...");

        try
        {
            var activeConnectors = await _context.Set<IntegrationConnector>()
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync();

            _logger.LogInformation("[IntegrationHealthMonitorJob] Monitoring {Count} active connectors", activeConnectors.Count);

            int healthyCount = 0;
            int degradedCount = 0;
            int failedCount = 0;

            foreach (var connector in activeConnectors)
            {
                try
                {
                    // Update last health check timestamp
                    connector.LastHealthCheck = DateTime.UtcNow;

                    // Get health status from Integration Agent (AI-powered analysis)
                    var healthResult = await _integrationAgent.MonitorIntegrationHealthAsync(connector.Id);

                    // Create health metric record
                    var metric = new IntegrationHealthMetric
                    {
                        Id = Guid.NewGuid(),
                        ConnectorId = connector.Id,
                        MetricType = "HealthScore",
                        Value = healthResult.HealthScore,
                        Unit = "Percentage",
                        PeriodStart = DateTime.UtcNow.AddMinutes(-15),
                        PeriodEnd = DateTime.UtcNow,
                        AlertThreshold = 70,
                        IsBreaching = healthResult.HealthScore < 70,
                        RecordedAt = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    _context.Set<IntegrationHealthMetric>().Add(metric);

                    // Update connector health status
                    if (healthResult.HealthStatus == "Healthy")
                    {
                        connector.ConnectionStatus = "Connected";
                        healthyCount++;
                    }
                    else if (healthResult.HealthStatus == "Degraded")
                    {
                        connector.ConnectionStatus = "Disconnected";
                        degradedCount++;
                    }
                    else
                    {
                        connector.ConnectionStatus = "Error";
                        failedCount++;
                    }

                    connector.ModifiedDate = DateTime.UtcNow;

                    _logger.LogInformation("[IntegrationHealthMonitorJob] Connector {ConnectorCode}: {HealthStatus} (score: {HealthScore})",
                        connector.ConnectorCode, healthResult.HealthStatus, healthResult.HealthScore);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[IntegrationHealthMonitorJob] Error monitoring connector: {ConnectorCode}",
                        connector.ConnectorCode);

                    connector.ConnectionStatus = "Error";
                    connector.ErrorCount++;
                    connector.ModifiedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "[IntegrationHealthMonitorJob] Health monitoring completed - Healthy: {Healthy}, Degraded: {Degraded}, Failed: {Failed}",
                healthyCount, degradedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[IntegrationHealthMonitorJob] Error during health monitoring");
            throw;
        }
    }
}
