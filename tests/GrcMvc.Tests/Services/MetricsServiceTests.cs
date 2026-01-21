using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using GrcMvc.Configuration;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using System.Diagnostics;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Tests for MetricsService to verify tracking and statistics
/// </summary>
public class MetricsServiceTests
{
    private readonly ILogger<MetricsService> _logger;
    private readonly MetricsService _metricsService;
    
    public MetricsServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<MetricsService>();
        _metricsService = new MetricsService(_logger);
    }
    
    [Fact]
    public async Task TrackMethodCall_TracksCorrectly()
    {
        // Act
        _metricsService.TrackMethodCall("GetUser", "Enhanced", true, 50);
        _metricsService.TrackMethodCall("GetUser", "Legacy", true, 100);
        
        // Get stats
        var stats = await _metricsService.GetStatisticsAsync(
            DateTime.UtcNow.AddMinutes(-5), 
            DateTime.UtcNow.AddMinutes(5));
        
        // Assert
        Assert.Equal(2, stats.TotalCalls);
        Assert.Equal(1, stats.EnhancedCalls);
        Assert.Equal(1, stats.LegacyCalls);
    }
    
    [Fact]
    public async Task GetStatisticsAsync_CalculatesSuccessRateCorrectly()
    {
        // Act
        _metricsService.TrackMethodCall("Test", "Enhanced", true, 10);
        _metricsService.TrackMethodCall("Test", "Enhanced", true, 10);
        _metricsService.TrackMethodCall("Test", "Enhanced", false, 10);
        _metricsService.TrackMethodCall("Test", "Legacy", true, 10);
        
        var stats = await _metricsService.GetStatisticsAsync(
            DateTime.UtcNow.AddMinutes(-5), 
            DateTime.UtcNow.AddMinutes(5));
        
        // Assert
        Assert.Equal(66.67, stats.EnhancedSuccessRate, 2);
        Assert.Equal(100.0, stats.LegacySuccessRate, 2);
    }
    
    [Fact]
    public async Task GetStatisticsAsync_CalculatesAverageDurationCorrectly()
    {
        // Act
        _metricsService.TrackMethodCall("Test", "Enhanced", true, 50);
        _metricsService.TrackMethodCall("Test", "Enhanced", true, 100);
        _metricsService.TrackMethodCall("Test", "Legacy", true, 200);
        
        var stats = await _metricsService.GetStatisticsAsync(
            DateTime.UtcNow.AddMinutes(-5), 
            DateTime.UtcNow.AddMinutes(5));
        
        // Assert
        Assert.Equal(75.0, stats.EnhancedAverageDurationMs);
        Assert.Equal(200.0, stats.LegacyAverageDurationMs);
    }
    
    [Fact]
    public async Task TrackConsistencyCheck_TracksFailuresCorrectly()
    {
        // Act
        _metricsService.TrackConsistencyCheck("GetUser", true, "Match");
        _metricsService.TrackConsistencyCheck("GetUser", false, "Mismatch");
        _metricsService.TrackConsistencyCheck("GetUser", true, "Match");
        
        var stats = await _metricsService.GetStatisticsAsync(
            DateTime.UtcNow.AddMinutes(-5), 
            DateTime.UtcNow.AddMinutes(5));
        
        // Assert
        Assert.Equal(3, stats.ConsistencyChecks);
        Assert.Equal(1, stats.ConsistencyFailures);
    }
    
    [Fact]
    public async Task GetStatisticsAsync_FiltersByDateRange()
    {
        // Act
        _metricsService.TrackMethodCall("OldCall", "Legacy", true, 10);
        await Task.Delay(100);
        var midpoint = DateTime.UtcNow;
        await Task.Delay(100);
        _metricsService.TrackMethodCall("NewCall", "Enhanced", true, 10);
        
        var statsAfterMidpoint = await _metricsService.GetStatisticsAsync(
            midpoint, 
            DateTime.UtcNow.AddMinutes(5));
        
        // Assert - Only "NewCall" should be counted
        Assert.Equal(1, statsAfterMidpoint.TotalCalls);
        Assert.Equal(1, statsAfterMidpoint.EnhancedCalls);
    }
    
    [Fact]
    public async Task CallsByMethod_TracksDifferentMethods()
    {
        // Act
        _metricsService.TrackMethodCall("GetUser", "Enhanced", true, 10);
        _metricsService.TrackMethodCall("GetUser", "Legacy", true, 10);
        _metricsService.TrackMethodCall("ResetPassword", "Enhanced", true, 20);
        
        var stats = await _metricsService.GetStatisticsAsync(
            DateTime.UtcNow.AddMinutes(-5), 
            DateTime.UtcNow.AddMinutes(5));
        
        // Assert
        Assert.Equal(2, stats.CallsByMethod["GetUser"]);
        Assert.Equal(1, stats.CallsByMethod["ResetPassword"]);
    }
}
