using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Moq;
using GrcMvc.Configuration;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.Entities;
using System.Threading.Tasks;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Integration tests for UserManagementFacade feature flag routing
/// </summary>
public class UserManagementFacadeTests
{
    private readonly Mock<IPlatformAdminService> _mockLegacyService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ISecurePasswordGenerator> _mockPasswordGenerator;
    private readonly Mock<IMetricsService> _mockMetrics;
    private readonly ILogger<UserManagementFacade> _logger;
    
    public UserManagementFacadeTests()
    {
        _mockLegacyService = new Mock<IPlatformAdminService>();
        _mockUserManager = MockUserManager();
        _mockPasswordGenerator = new Mock<ISecurePasswordGenerator>();
        _mockMetrics = new Mock<IMetricsService>();
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UserManagementFacade>();
    }
    
    [Fact]
    public async Task GetUserAsync_WithFlagOff_UsesLegacyService()
    {
        // Arrange
        var options = Options.Create(new GrcFeatureOptions
        {
            UseSecurePasswordGeneration = false,
            CanaryPercentage = 0
        });
        
        var mockAdmin = new Models.Entities.PlatformAdmin
        {
            UserId = "user123",
            ContactEmail = "test@example.com",
            DisplayName = "Test User",
            Status = "Active"
        };
        
        _mockLegacyService.Setup(s => s.GetByUserIdAsync("user123"))
            .ReturnsAsync(mockAdmin);
        
        var facade = new UserManagementFacade(
            _mockLegacyService.Object,
            _mockUserManager.Object,
            _mockPasswordGenerator.Object,
            options,
            _mockMetrics.Object,
            _logger);
        
        // Act
        var result = await facade.GetUserAsync("user123");
        
        // Assert
        Assert.Equal("user123", result.Id);
        _mockLegacyService.Verify(s => s.GetByUserIdAsync("user123"), Times.Once);
        _mockMetrics.Verify(m => m.TrackMethodCall(
            "GetUser", 
            "Legacy", 
            true, 
            It.IsAny<long>()), Times.Once);
    }
    
    [Fact]
    public async Task GetUserAsync_WithFlagOn_UsesEnhancedService()
    {
        // Arrange
        var options = Options.Create(new GrcFeatureOptions
        {
            UseSecurePasswordGeneration = true,
            CanaryPercentage = 0
        });
        
        var mockUser = new ApplicationUser
        {
            Id = "user123",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync("user123"))
            .ReturnsAsync(mockUser);
        _mockUserManager.Setup(m => m.GetRolesAsync(mockUser))
            .ReturnsAsync(new List<string> { "Admin" });
        
        var facade = new UserManagementFacade(
            _mockLegacyService.Object,
            _mockUserManager.Object,
            _mockPasswordGenerator.Object,
            options,
            _mockMetrics.Object,
            _logger);
        
        // Act
        var result = await facade.GetUserAsync("user123");
        
        // Assert
        Assert.Equal("user123", result.Id);
        _mockUserManager.Verify(m => m.FindByIdAsync("user123"), Times.Once);
        _mockMetrics.Verify(m => m.TrackMethodCall(
            "GetUser", 
            "Enhanced", 
            true, 
            It.IsAny<long>()), Times.Once);
    }
    
    [Theory]
    [InlineData(5, "user5", true)]   // hash % 100 = 5, within 5%
    [InlineData(10, "user95", false)] // hash % 100 = 95, outside 10%
    [InlineData(50, "user25", true)]  // hash % 100 = 25, within 50%
    public async Task CanaryDeployment_RoutesBasedOnPercentage(
        int canaryPercentage, 
        string userId, 
        bool shouldUseEnhanced)
    {
        // Arrange
        var options = Options.Create(new GrcFeatureOptions
        {
            UseSecurePasswordGeneration = false,
            CanaryPercentage = canaryPercentage
        });
        
        var facade = new UserManagementFacade(
            _mockLegacyService.Object,
            _mockUserManager.Object,
            _mockPasswordGenerator.Object,
            options,
            _mockMetrics.Object,
            _logger);
        
        // Setup mocks
        _mockLegacyService.Setup(s => s.GetByUserIdAsync(userId))
            .ReturnsAsync(new Models.Entities.PlatformAdmin
            {
                UserId = userId,
                ContactEmail = "test@example.com",
                Status = "Active"
            });
        
        _mockUserManager.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            });
        
        // Act
        await facade.GetUserAsync(userId);
        
        // Assert - Verify which service was used
        var expectedImplementation = shouldUseEnhanced ? "Enhanced" : "Legacy";
        _mockMetrics.Verify(m => m.TrackMethodCall(
            "GetUser",
            expectedImplementation,
            true,
            It.IsAny<long>()), Times.Once);
    }
    
    [Fact]
    public async Task ResetPasswordAsync_WithSecureFlag_UsesSecurePasswordGenerator()
    {
        // Arrange
        var options = Options.Create(new GrcFeatureOptions
        {
            UseSecurePasswordGeneration = true
        });
        
        var mockUser = new ApplicationUser
        {
            Id = "user123",
            Email = "test@example.com"
        };
        
        _mockUserManager.Setup(m => m.FindByIdAsync("user123"))
            .ReturnsAsync(mockUser);
        _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(mockUser))
            .ReturnsAsync("reset-token");
        _mockUserManager.Setup(m => m.ResetPasswordAsync(mockUser, "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(m => m.UpdateAsync(mockUser))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockPasswordGenerator.Setup(g => g.GeneratePassword())
            .Returns("SecureP@ssw0rd123!");
        
        var facade = new UserManagementFacade(
            _mockLegacyService.Object,
            _mockUserManager.Object,
            _mockPasswordGenerator.Object,
            options,
            _mockMetrics.Object,
            _logger);
        
        // Act
        var result = await facade.ResetPasswordAsync("admin123", "user123", "ignored");
        
        // Assert
        Assert.True(result);
        _mockPasswordGenerator.Verify(g => g.GeneratePassword(), Times.Once);
        _mockMetrics.Verify(m => m.TrackMethodCall(
            "ResetPassword",
            "Enhanced",
            true,
            It.IsAny<long>()), Times.Once);
    }
    
    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }
}
