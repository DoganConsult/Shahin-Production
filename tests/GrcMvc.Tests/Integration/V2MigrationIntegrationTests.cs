using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;

namespace GrcMvc.Tests.Integration;

/// <summary>
/// End-to-end integration tests for V2 migration
/// </summary>
public class V2MigrationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public V2MigrationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task V2Dashboard_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/platform-admin/v2/dashboard");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("V2 (Facade)", content);
    }
    
    [Fact]
    public async Task MigrationMetrics_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/platform-admin/migration-metrics");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Migration Metrics", content);
    }
    
    [Fact]
    public async Task MigrationMetricsApi_ReturnsJson()
    {
        // Act
        var response = await _client.GetAsync("/platform-admin/migration-metrics/api/stats?days=1");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", json);
    }
    
    [Fact]
    public async Task AccountLoginV2_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/account/v2/login");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Enhanced Security", content);
    }
    
    [Fact]
    public async Task TenantLoginV2_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/account/v2/tenant-login");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Session-Based Claims", content);
    }
}
