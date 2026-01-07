using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using GrcMvc.Configuration;

namespace GrcMvc.Tests.Configuration;

/// <summary>
/// Tests for GrcFeatureOptions to verify feature flag behavior
/// </summary>
public class GrcFeatureOptionsTests
{
    [Fact]
    public void GrcFeatureOptions_DefaultValues_AreAllOff()
    {
        // Arrange & Act
        var options = new GrcFeatureOptions();
        
        // Assert - All flags should default to false
        Assert.False(options.UseSecurePasswordGeneration);
        Assert.False(options.UseSessionBasedClaims);
        Assert.False(options.UseEnhancedAuditLogging);
        Assert.False(options.UseDeterministicTenantResolution);
        Assert.False(options.DisableDemoLogin);
        Assert.Equal(0, options.CanaryPercentage);
        Assert.False(options.VerifyConsistency);
        Assert.True(options.LogFeatureFlagDecisions); // Only this one is true
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(50)]
    [InlineData(100)]
    public void CanaryPercentage_AcceptsValidValues(int percentage)
    {
        // Arrange & Act
        var options = new GrcFeatureOptions
        {
            CanaryPercentage = percentage
        };
        
        // Assert
        Assert.Equal(percentage, options.CanaryPercentage);
        Assert.InRange(options.CanaryPercentage, 0, 100);
    }
    
    [Fact]
    public void FeatureFlagNames_MatchConfigurationSection()
    {
        // Arrange
        var sectionName = GrcFeatureOptions.SectionName;
        
        // Assert
        Assert.Equal("GrcFeatureFlags", sectionName);
    }
}
