using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Implementations;

namespace Grc.Application.Contracts;

/// <summary>
/// ABP Application Contracts Module
/// Registers application service interfaces and implementations
/// </summary>
[DependsOn(typeof(AbpAuthorizationModule))]
public class GrcApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // ══════════════════════════════════════════════════════════════
        // Endpoint Management Services
        // ══════════════════════════════════════════════════════════════
        
        // Endpoint Discovery Service - Discovers all API endpoints via reflection
        services.AddScoped<IEndpointDiscoveryService, EndpointDiscoveryService>();

        // Endpoint Monitoring Service - Monitors endpoint health, usage, and performance
        services.AddScoped<IEndpointMonitoringService, EndpointMonitoringService>();
    }
}
