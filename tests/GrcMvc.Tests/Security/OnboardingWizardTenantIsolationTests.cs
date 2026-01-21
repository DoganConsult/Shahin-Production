using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Security;

/// <summary>
/// Security tests for onboarding wizard tenant isolation.
/// Verifies that users from one tenant cannot access or modify another tenant's wizard data.
/// These tests are critical for multi-tenant data security.
/// </summary>
[Trait("Category", "Security")]
[Trait("Category", "TenantIsolation")]
public class OnboardingWizardTenantIsolationTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRulesEngineService> _mockRulesEngine;
    private readonly Mock<IAuditEventService> _mockAuditService;
    private readonly Mock<IOnboardingCoverageService> _mockCoverageService;
    private readonly Mock<IFieldRegistryService> _mockFieldRegistryService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<OnboardingWizardService>> _mockLogger;
    private readonly Mock<IGenericRepository<OnboardingWizard>> _mockWizardRepo;
    private readonly OnboardingWizardService _service;

    // Test tenant IDs
    private readonly Guid _tenantAId = Guid.NewGuid();
    private readonly Guid _tenantBId = Guid.NewGuid();
    private readonly string _userAId = "user-tenant-a";
    private readonly string _userBId = "user-tenant-b";

    public OnboardingWizardTenantIsolationTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRulesEngine = new Mock<IRulesEngineService>();
        _mockAuditService = new Mock<IAuditEventService>();
        _mockCoverageService = new Mock<IOnboardingCoverageService>();
        _mockFieldRegistryService = new Mock<IFieldRegistryService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OnboardingWizardService>>();
        _mockWizardRepo = new Mock<IGenericRepository<OnboardingWizard>>();

        _mockUnitOfWork.Setup(u => u.OnboardingWizards).Returns(_mockWizardRepo.Object);

        // Setup default coverage service responses
        SetupDefaultCoverageService();

        _mockConfiguration.Setup(c => c.GetValue<bool>("Onboarding:EnableCoverageValidation", true)).Returns(false);

        _service = new OnboardingWizardService(
            _mockUnitOfWork.Object,
            _mockRulesEngine.Object,
            _mockAuditService.Object,
            _mockCoverageService.Object,
            _mockFieldRegistryService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region Read Isolation Tests

    /// <summary>
    /// Verifies that GetWizardStateAsync returns null when querying for a different tenant's wizard.
    /// This is the primary data isolation check.
    /// </summary>
    [Fact]
    public async Task GetWizardStateAsync_ReturnsNull_WhenTenantIdDoesNotMatch()
    {
        // Arrange: Create wizard for Tenant A
        var wizardA = CreateTestWizard(_tenantAId);
        SetupWizardQuery(_tenantAId, wizardA);

        // Act: Try to access with Tenant B's ID
        var result = await _service.GetWizardStateAsync(_tenantBId);

        // Assert: Should return null (wizard doesn't exist for Tenant B)
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetProgressAsync returns default progress for non-existent tenant.
    /// </summary>
    [Fact]
    public async Task GetProgressAsync_ReturnsNotStarted_WhenTenantHasNoWizard()
    {
        // Arrange: Setup empty query for Tenant B
        SetupEmptyWizardQuery(_tenantBId);

        // Act
        var result = await _service.GetProgressAsync(_tenantBId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NotStarted", result.WizardStatus);
        Assert.Equal(0, result.CurrentStep);
        Assert.Equal(0, result.ProgressPercent);
    }

    /// <summary>
    /// Verifies that wizards are isolated by TenantId in queries.
    /// </summary>
    [Fact]
    public async Task GetWizardStateAsync_OnlyReturnsWizardForMatchingTenant()
    {
        // Arrange: Create wizard for Tenant A
        var wizardA = CreateTestWizard(_tenantAId);
        wizardA.OrganizationLegalNameEn = "Tenant A Organization";

        // Setup: Only return wizard when queried with Tenant A's ID
        SetupWizardQuery(_tenantAId, wizardA);
        SetupEmptyWizardQuery(_tenantBId);

        // Act: Query with Tenant A's ID
        var resultA = await _service.GetWizardStateAsync(_tenantAId);

        // Act: Query with Tenant B's ID
        var resultB = await _service.GetWizardStateAsync(_tenantBId);

        // Assert
        Assert.NotNull(resultA);
        Assert.Null(resultB);
    }

    #endregion

    #region Write Isolation Tests

    /// <summary>
    /// Verifies that SaveSectionAAsync only modifies wizard for the specified tenant.
    /// </summary>
    [Fact]
    public async Task SaveSectionAAsync_DoesNotModifyOtherTenantWizard()
    {
        // Arrange: Create wizards for both tenants
        var wizardA = CreateTestWizard(_tenantAId);
        var wizardB = CreateTestWizard(_tenantBId);
        wizardB.OrganizationLegalNameEn = "Original Tenant B Name";

        SetupWizardQuery(_tenantAId, wizardA);
        SetupWizardQuery(_tenantBId, wizardB);

        var sectionData = new SectionA_OrganizationIdentity
        {
            LegalNameEn = "Modified Name by Tenant A",
            CountryOfIncorporation = "SA",
            PrimaryHqLocation = "Riyadh",
            Timezone = "Asia/Riyadh",
            PrimaryLanguage = "English",
            OrgType = "enterprise",
            Sectors = new List<string> { "Technology" },
            CorporateEmailDomains = new List<string> { "test.com" }
        };

        // Act: Tenant A saves section A
        await _service.SaveSectionAAsync(_tenantAId, sectionData, _userAId);

        // Assert: Tenant B's wizard should remain unchanged
        Assert.Equal("Original Tenant B Name", wizardB.OrganizationLegalNameEn);
    }

    /// <summary>
    /// Verifies that CompleteWizardAsync cannot be called on another tenant's wizard.
    /// </summary>
    [Fact]
    public async Task CompleteWizardAsync_FailsForNonExistentTenant()
    {
        // Arrange: Create wizard only for Tenant A
        var wizardA = CreateTestWizard(_tenantAId);
        SetupWizardQuery(_tenantAId, wizardA);
        SetupEmptyWizardQuery(_tenantBId);

        // Act: Try to complete Tenant B's wizard (which doesn't exist)
        var result = await _service.CompleteWizardAsync(_tenantBId, _userBId);

        // Assert: Should fail because wizard doesn't exist
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Derived Scope Isolation Tests

    /// <summary>
    /// Verifies that GetDerivedScopeAsync only returns scope for the correct tenant.
    /// </summary>
    [Fact]
    public async Task GetDerivedScopeAsync_ReturnsNull_WhenWizardDoesNotExist()
    {
        // Arrange: Create wizard only for Tenant A
        var wizardA = CreateTestWizard(_tenantAId);
        SetupWizardQuery(_tenantAId, wizardA);
        SetupEmptyWizardQuery(_tenantBId);

        // Act: Try to get scope for Tenant B
        var result = await _service.GetDerivedScopeAsync(_tenantBId);

        // Assert: Should return null or empty scope
        Assert.True(result == null ||
                    (result.ApplicableBaselines.Count == 0 &&
                     result.ApplicablePackages.Count == 0 &&
                     result.ApplicableTemplates.Count == 0));
    }

    #endregion

    #region Concurrent Access Tests

    /// <summary>
    /// Verifies that concurrent saves from different tenants don't interfere.
    /// </summary>
    [Fact]
    public async Task ConcurrentSaves_FromDifferentTenants_AreIsolated()
    {
        // Arrange
        var wizardA = CreateTestWizard(_tenantAId);
        var wizardB = CreateTestWizard(_tenantBId);

        SetupWizardQuery(_tenantAId, wizardA);
        SetupWizardQuery(_tenantBId, wizardB);

        var sectionDataA = new SectionA_OrganizationIdentity
        {
            LegalNameEn = "Tenant A Name",
            CountryOfIncorporation = "SA",
            PrimaryHqLocation = "Riyadh",
            Timezone = "Asia/Riyadh",
            PrimaryLanguage = "English",
            OrgType = "enterprise",
            Sectors = new List<string> { "Banking" },
            CorporateEmailDomains = new List<string> { "tenanta.com" }
        };

        var sectionDataB = new SectionA_OrganizationIdentity
        {
            LegalNameEn = "Tenant B Name",
            CountryOfIncorporation = "AE",
            PrimaryHqLocation = "Dubai",
            Timezone = "Asia/Dubai",
            PrimaryLanguage = "Arabic",
            OrgType = "sme",
            Sectors = new List<string> { "Healthcare" },
            CorporateEmailDomains = new List<string> { "tenantb.com" }
        };

        // Act: Concurrent saves
        var taskA = _service.SaveSectionAAsync(_tenantAId, sectionDataA, _userAId);
        var taskB = _service.SaveSectionAAsync(_tenantBId, sectionDataB, _userBId);

        await Task.WhenAll(taskA, taskB);

        // Assert: Each wizard should have its own data
        Assert.Equal("Tenant A Name", wizardA.OrganizationLegalNameEn);
        Assert.Equal("Tenant B Name", wizardB.OrganizationLegalNameEn);
        Assert.Equal("SA", wizardA.CountryOfIncorporation);
        Assert.Equal("AE", wizardB.CountryOfIncorporation);
    }

    #endregion

    #region Validation Isolation Tests

    /// <summary>
    /// Verifies that ValidateWizardAsync only validates the correct tenant's wizard.
    /// </summary>
    [Fact]
    public async Task ValidateWizardAsync_OnlyValidatesSpecifiedTenant()
    {
        // Arrange: Create complete wizard for Tenant A, empty for Tenant B
        var wizardA = CreateTestWizard(_tenantAId);
        wizardA.WizardStatus = "Completed";
        SetupWizardQuery(_tenantAId, wizardA);
        SetupEmptyWizardQuery(_tenantBId);

        // Act
        var resultA = await _service.ValidateWizardAsync(_tenantAId);
        var resultB = await _service.ValidateWizardAsync(_tenantBId);

        // Assert: Different validation results for different tenants
        Assert.NotNull(resultA);
        Assert.NotNull(resultB);
        // Tenant A has wizard, Tenant B doesn't
        Assert.NotEqual(resultA.CompletedSections, resultB.CompletedSections);
    }

    #endregion

    #region Helper Methods

    private void SetupDefaultCoverageService()
    {
        _mockCoverageService.Setup(c => c.LoadManifestAsync())
            .ReturnsAsync(new CoverageManifest
            {
                RequiredIdsByNode = new Dictionary<string, List<string>>(),
                OptionalIdsByNode = new Dictionary<string, List<string>>(),
                RequiredIdsByMission = new Dictionary<string, List<string>>(),
                ConditionalRequired = new List<ConditionalRequirement>(),
                IntegrityChecks = new List<IntegrityCheck>()
            });

        _mockCoverageService.Setup(c => c.ValidateNodeCoverageAsync(It.IsAny<string>(), It.IsAny<IFieldValueProvider>()))
            .ReturnsAsync(new NodeCoverageResult
            {
                NodeId = "test",
                IsValid = true,
                RequiredFields = new List<string>(),
                MissingRequiredFields = new List<string>(),
                PresentRequiredFields = new List<string>()
            });
    }

    private OnboardingWizard CreateTestWizard(Guid tenantId)
    {
        return new OnboardingWizard
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WizardStatus = "InProgress",
            CurrentStep = 1,
            StartedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            OrganizationLegalNameEn = $"Test Org {tenantId.ToString().Substring(0, 8)}",
            CountryOfIncorporation = "SA",
            DefaultTimezone = "Asia/Riyadh"
        };
    }

    private void SetupWizardQuery(Guid tenantId, OnboardingWizard wizard)
    {
        _mockWizardRepo.Setup(r => r.GetFirstOrDefaultAsync(
                It.Is<System.Linq.Expressions.Expression<Func<OnboardingWizard, bool>>>(
                    expr => ExpressionMatchesTenant(expr, tenantId)),
                It.IsAny<Func<IQueryable<OnboardingWizard>, IQueryable<OnboardingWizard>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(wizard);
    }

    private void SetupEmptyWizardQuery(Guid tenantId)
    {
        _mockWizardRepo.Setup(r => r.GetFirstOrDefaultAsync(
                It.Is<System.Linq.Expressions.Expression<Func<OnboardingWizard, bool>>>(
                    expr => ExpressionMatchesTenant(expr, tenantId)),
                It.IsAny<Func<IQueryable<OnboardingWizard>, IQueryable<OnboardingWizard>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((OnboardingWizard?)null);
    }

    private bool ExpressionMatchesTenant(
        System.Linq.Expressions.Expression<Func<OnboardingWizard, bool>> expr,
        Guid tenantId)
    {
        // Simple check - compile and test with a sample wizard
        var compiled = expr.Compile();
        var testWizard = new OnboardingWizard { TenantId = tenantId };
        return compiled(testWizard);
    }

    #endregion
}
