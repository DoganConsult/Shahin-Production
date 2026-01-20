using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Unit tests for OnboardingWizardService - 12-section wizard flow
/// Tests progressive save, validation, coverage, and completion logic
/// </summary>
public class OnboardingWizardServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRulesEngineService> _mockRulesEngine;
    private readonly Mock<IAuditEventService> _mockAuditService;
    private readonly Mock<IOnboardingCoverageService> _mockCoverageService;
    private readonly Mock<IFieldRegistryService> _mockFieldRegistryService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<OnboardingWizardService>> _mockLogger;
    private readonly Mock<IGenericRepository<OnboardingWizard>> _mockWizardRepo;
    private readonly Mock<IGenericRepository<TenantBaseline>> _mockBaselineRepo;
    private readonly Mock<IGenericRepository<TenantPackage>> _mockPackageRepo;
    private readonly Mock<IGenericRepository<TenantTemplate>> _mockTemplateRepo;
    private readonly OnboardingWizardService _service;

    public OnboardingWizardServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRulesEngine = new Mock<IRulesEngineService>();
        _mockAuditService = new Mock<IAuditEventService>();
        _mockCoverageService = new Mock<IOnboardingCoverageService>();
        _mockFieldRegistryService = new Mock<IFieldRegistryService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OnboardingWizardService>>();

        _mockWizardRepo = new Mock<IRepository<OnboardingWizard>>();
        _mockBaselineRepo = new Mock<IRepository<TenantBaseline>>();
        _mockPackageRepo = new Mock<IRepository<TenantPackage>>();
        _mockTemplateRepo = new Mock<IRepository<TenantTemplate>>();

        _mockUnitOfWork.Setup(u => u.OnboardingWizards).Returns(_mockWizardRepo.Object);
        _mockUnitOfWork.Setup(u => u.TenantBaselines).Returns(_mockBaselineRepo.Object);
        _mockUnitOfWork.Setup(u => u.TenantPackages).Returns(_mockPackageRepo.Object);
        _mockUnitOfWork.Setup(u => u.TenantTemplates).Returns(_mockTemplateRepo.Object);

        // Setup coverage service with default results
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

        _mockCoverageService.Setup(c => c.ValidateCompleteCoverageAsync(It.IsAny<IFieldValueProvider>()))
            .ReturnsAsync(new CompleteCoverageResult
            {
                NodeResults = new Dictionary<string, NodeCoverageResult>(),
                MissionResults = new Dictionary<string, MissionCoverageResult>()
            });

        _mockConfiguration.Setup(c => c.GetValue<bool>("Onboarding:EnableCoverageValidation", true)).Returns(false);
        _mockConfiguration.Setup(c => c.GetValue<bool>("Onboarding:ValidateOnSectionSave", true)).Returns(false);

        _service = new OnboardingWizardService(
            _mockUnitOfWork.Object,
            _mockRulesEngine.Object,
            _mockAuditService.Object,
            _mockCoverageService.Object,
            _mockFieldRegistryService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    #region StartWizardAsync Tests

    [Fact]
    public async Task StartWizardAsync_CreatesNewWizard()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = "test-user";
        OnboardingWizard? addedWizard = null;

        SetupEmptyWizardQuery(tenantId);
        _mockWizardRepo.Setup(r => r.AddAsync(It.IsAny<OnboardingWizard>()))
            .Callback<OnboardingWizard>(w => addedWizard = w)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.StartWizardAsync(tenantId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("InProgress", result.WizardStatus);
        Assert.Equal(1, result.CurrentStep);
        Assert.NotNull(result.StartedAt);
    }

    [Fact]
    public async Task StartWizardAsync_WithExistingWizard_ReturnsExisting()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var existingWizard = CreateTestWizard(tenantId);

        SetupWizardQuery(tenantId, existingWizard);

        // Act
        var result = await _service.StartWizardAsync(tenantId, "test-user");

        // Assert
        Assert.Equal(existingWizard.Id, result.Id);
        _mockWizardRepo.Verify(r => r.AddAsync(It.IsAny<OnboardingWizard>()), Times.Never);
    }

    [Fact]
    public async Task StartWizardAsync_LogsAuditEvent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = "test-user";

        SetupEmptyWizardQuery(tenantId);
        _mockWizardRepo.Setup(r => r.AddAsync(It.IsAny<OnboardingWizard>())).Returns(Task.CompletedTask);

        // Act
        await _service.StartWizardAsync(tenantId, userId);

        // Assert
        _mockAuditService.Verify(
            a => a.LogEventAsync(
                tenantId,
                "OnboardingWizardStarted",
                "OnboardingWizard",
                It.IsAny<string>(),
                "Create",
                userId,
                It.IsAny<string>()),
            Times.Once);
    }

    #endregion

    #region Section Save Tests

    [Fact]
    public async Task SaveSectionAAsync_SavesOrganizationIdentity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        SetupWizardQuery(tenantId, wizard);

        var sectionA = new SectionA_OrganizationIdentity
        {
            LegalNameEn = "Test Company",
            LegalNameAr = "شركة اختبار",
            TradeName = "TestCo",
            CountryOfIncorporation = "Saudi Arabia",
            OperatingCountries = new List<string> { "Saudi Arabia", "UAE" },
            PrimaryHqLocation = "Riyadh",
            Timezone = "Arabia Standard Time",
            PrimaryLanguage = "English",
            CorporateEmailDomains = new List<string> { "test.com" },
            OrgType = "Corporation",
            Sectors = new List<string> { "Finance" },
            BusinessLines = new List<string> { "Banking" }
        };

        // Act
        var result = await _service.SaveSectionAAsync(tenantId, sectionA, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("A", result.Section);
        Assert.Equal("Test Company", wizard.OrganizationLegalNameEn);
        Assert.Equal("شركة اختبار", wizard.OrganizationLegalNameAr);
        Assert.Equal("TestCo", wizard.TradeName);
        Assert.Contains("Saudi Arabia", wizard.OperatingCountriesJson);
    }

    [Fact]
    public async Task SaveSectionAAsync_WhenComplete_MarksSectionComplete()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        SetupWizardQuery(tenantId, wizard);

        var sectionA = CreateCompleteSectionA();

        // Act
        var result = await _service.SaveSectionAAsync(tenantId, sectionA, "test-user");

        // Assert
        Assert.True(result.SectionComplete);
        Assert.Contains("\"A\"", wizard.CompletedSectionsJson);
    }

    [Fact]
    public async Task SaveSectionAAsync_UpdatesProgress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[]";
        SetupWizardQuery(tenantId, wizard);

        var sectionA = CreateCompleteSectionA();

        // Act
        await _service.SaveSectionAAsync(tenantId, sectionA, "test-user");

        // Assert
        Assert.Equal(8, wizard.ProgressPercent); // 1/12 * 100 = 8.33%
    }

    [Fact]
    public async Task SaveSectionDAsync_SavesScopeDefinition()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        SetupWizardQuery(tenantId, wizard);

        var sectionD = new SectionD_ScopeDefinition
        {
            InScopeLegalEntities = new List<LegalEntityScope>
            {
                new LegalEntityScope { EntityName = "Main Company", JurisdictionCode = "SA" }
            },
            InScopeBusinessUnits = new List<BusinessUnitScope>
            {
                new BusinessUnitScope { UnitName = "IT Department" }
            },
            InScopeSystems = new List<SystemScope>
            {
                new SystemScope { SystemName = "Core Banking", CriticalityTier = "Critical" }
            },
            InScopeProcesses = new List<string> { "Account Opening", "KYC" },
            InScopeEnvironments = new List<string> { "Production", "Staging" },
            InScopeLocations = new List<LocationScope>
            {
                new LocationScope { LocationName = "Riyadh HQ" }
            }
        };

        // Act
        var result = await _service.SaveSectionDAsync(tenantId, sectionD, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("D", result.Section);
        Assert.Contains("Main Company", wizard.InScopeLegalEntitiesJson);
        Assert.Contains("IT Department", wizard.InScopeBusinessUnitsJson);
        Assert.Contains("Core Banking", wizard.InScopeSystemsJson);
        Assert.Equal("Production,Staging", wizard.InScopeEnvironments);
    }

    [Fact]
    public async Task SaveSectionFAsync_SavesTechnologyLandscape()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        SetupWizardQuery(tenantId, wizard);

        var sectionF = new SectionF_TechnologyLandscape
        {
            IdentityProvider = "Azure AD",
            SsoEnabled = true,
            ScimEnabled = true,
            ItsmPlatform = "ServiceNow",
            EvidenceRepository = "SharePoint",
            SiemPlatform = "Sentinel",
            VulnerabilityManagement = "Tenable",
            EdrPlatform = "Defender",
            CloudProviders = new List<string> { "Azure", "AWS" },
            ErpPlatform = "SAP",
            CiCdTools = new List<string> { "GitHub", "Azure DevOps" }
        };

        // Act
        var result = await _service.SaveSectionFAsync(tenantId, sectionF, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Azure AD", wizard.IdentityProvider);
        Assert.True(wizard.SsoEnabled);
        Assert.Equal("ServiceNow", wizard.ItsmPlatform);
        Assert.Contains("Azure", wizard.CloudProvidersJson);
    }

    #endregion

    #region GetProgressAsync Tests

    [Fact]
    public async Task GetProgressAsync_WithNotStartedWizard_ReturnsNotStarted()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetupEmptyWizardQuery(tenantId);

        // Act
        var result = await _service.GetProgressAsync(tenantId);

        // Assert
        Assert.Equal("NotStarted", result.WizardStatus);
        Assert.Equal(0, result.CurrentStep);
        Assert.Equal(0, result.ProgressPercent);
        Assert.False(result.CanComplete);
    }

    [Fact]
    public async Task GetProgressAsync_WithSomeSectionsComplete_CalculatesProgress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\"]";
        wizard.ProgressPercent = 25;
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.GetProgressAsync(tenantId);

        // Assert
        Assert.Equal("InProgress", result.WizardStatus);
        Assert.Equal(12, result.TotalSteps);
        Assert.Equal(25, result.ProgressPercent);
    }

    [Fact]
    public async Task GetProgressAsync_WithRequiredSectionsComplete_CanComplete()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\",\"F\",\"H\",\"I\"]"; // All 6 required
        wizard.ProgressPercent = 50;
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.GetProgressAsync(tenantId);

        // Assert
        Assert.True(result.CanComplete);
    }

    [Fact]
    public async Task GetProgressAsync_ReturnsAllSectionProgress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"B\"]";
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.GetProgressAsync(tenantId);

        // Assert
        Assert.Equal(12, result.Sections.Count);
        Assert.True(result.Sections["A"].IsComplete);
        Assert.True(result.Sections["B"].IsComplete);
        Assert.False(result.Sections["C"].IsComplete);
        Assert.True(result.Sections["A"].IsRequired);
        Assert.False(result.Sections["B"].IsRequired);
    }

    #endregion

    #region ValidateWizardAsync Tests

    [Fact]
    public async Task ValidateWizardAsync_WithAllRequiredComplete_IsValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\",\"F\",\"H\",\"I\"]";
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.ValidateWizardAsync(tenantId, minimalOnly: true);

        // Assert
        Assert.True(result.IsValid);
        Assert.True(result.CanComplete);
        Assert.Empty(result.MissingRequiredFields);
    }

    [Fact]
    public async Task ValidateWizardAsync_WithMissingRequired_IsNotValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\"]"; // Missing E, F, H, I
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.ValidateWizardAsync(tenantId, minimalOnly: true);

        // Assert
        Assert.False(result.IsValid);
        Assert.False(result.CanComplete);
        Assert.Contains("Section E incomplete", result.MissingRequiredFields);
        Assert.Contains("Section F incomplete", result.MissingRequiredFields);
        Assert.Contains("Section H incomplete", result.MissingRequiredFields);
        Assert.Contains("Section I incomplete", result.MissingRequiredFields);
    }

    [Fact]
    public async Task ValidateWizardAsync_FullValidation_RequiresAllSections()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\",\"F\",\"H\",\"I\"]"; // Only required
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.ValidateWizardAsync(tenantId, minimalOnly: false);

        // Assert
        Assert.False(result.IsValid); // Not valid because optional sections missing
        Assert.True(result.CanComplete); // But can complete with minimal
        Assert.NotEmpty(result.Warnings); // Warnings for optional sections
    }

    [Fact]
    public async Task ValidateWizardAsync_WithNoWizard_ReturnsInvalid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetupEmptyWizardQuery(tenantId);

        // Act
        var result = await _service.ValidateWizardAsync(tenantId);

        // Assert
        Assert.False(result.IsValid);
        Assert.False(result.CanComplete);
        Assert.Contains("Wizard not started", result.MissingRequiredFields);
    }

    #endregion

    #region CompleteWizardAsync Tests

    [Fact]
    public async Task CompleteWizardAsync_WithValidWizard_CompletesSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\",\"F\",\"H\",\"I\"]";
        SetupWizardQuery(tenantId, wizard);
        SetupEmptyDerivedScope(tenantId);

        // Act
        var result = await _service.CompleteWizardAsync(tenantId, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Completed", wizard.WizardStatus);
        Assert.Equal(100, wizard.ProgressPercent);
        Assert.NotNull(wizard.CompletedAt);
        Assert.Equal("test-user", wizard.CompletedByUserId);
    }

    [Fact]
    public async Task CompleteWizardAsync_TriggersScopeDerivation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\",\"F\",\"H\",\"I\"]";
        SetupWizardQuery(tenantId, wizard);
        SetupEmptyDerivedScope(tenantId);

        // Act
        await _service.CompleteWizardAsync(tenantId, "test-user");

        // Assert
        _mockRulesEngine.Verify(
            r => r.DeriveAndPersistScopeAsync(tenantId, "test-user"),
            Times.Once);
    }

    [Fact]
    public async Task CompleteWizardAsync_LogsAuditEvent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\",\"D\",\"E\",\"F\",\"H\",\"I\"]";
        SetupWizardQuery(tenantId, wizard);
        SetupEmptyDerivedScope(tenantId);

        // Act
        await _service.CompleteWizardAsync(tenantId, "test-user");

        // Assert
        _mockAuditService.Verify(
            a => a.LogEventAsync(
                tenantId,
                "OnboardingWizardCompleted",
                "OnboardingWizard",
                It.IsAny<string>(),
                "Complete",
                "test-user",
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteWizardAsync_WithIncompleteRequiredSections_ReturnsFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[\"A\"]"; // Missing required sections
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.CompleteWizardAsync(tenantId, "test-user");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("required sections incomplete", result.Message.ToLower());
        Assert.NotEmpty(result.Errors);
    }

    #endregion

    #region SaveMinimalOnboardingAsync Tests

    [Fact]
    public async Task SaveMinimalOnboardingAsync_SavesAllRequiredSections()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        SetupWizardQuery(tenantId, wizard);

        var minimal = CreateMinimalOnboardingDto();

        // Act
        var result = await _service.SaveMinimalOnboardingAsync(tenantId, minimal, "test-user");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Minimal", result.Section);

        // Verify all 6 required sections are marked complete
        Assert.Contains("\"A\"", wizard.CompletedSectionsJson);
        Assert.Contains("\"D\"", wizard.CompletedSectionsJson);
        Assert.Contains("\"E\"", wizard.CompletedSectionsJson);
        Assert.Contains("\"F\"", wizard.CompletedSectionsJson);
        Assert.Contains("\"H\"", wizard.CompletedSectionsJson);
        Assert.Contains("\"I\"", wizard.CompletedSectionsJson);
    }

    [Fact]
    public async Task SaveMinimalOnboardingAsync_UpdatesProgress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.CompletedSectionsJson = "[]";
        SetupWizardQuery(tenantId, wizard);

        var minimal = CreateMinimalOnboardingDto();

        // Act
        await _service.SaveMinimalOnboardingAsync(tenantId, minimal, "test-user");

        // Assert
        Assert.Equal(50, wizard.ProgressPercent); // 6/12 * 100 = 50%
    }

    #endregion

    #region GetWizardStateAsync Tests

    [Fact]
    public async Task GetWizardStateAsync_WithNoWizard_ReturnsNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        SetupEmptyWizardQuery(tenantId);

        // Act
        var result = await _service.GetWizardStateAsync(tenantId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWizardStateAsync_ReturnsMappedState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wizard = CreateTestWizard(tenantId);
        wizard.OrganizationLegalNameEn = "Test Company";
        wizard.CompletedSectionsJson = "[\"A\"]";
        wizard.WizardStatus = "InProgress";
        wizard.CurrentStep = 2;
        SetupWizardQuery(tenantId, wizard);

        // Act
        var result = await _service.GetWizardStateAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("InProgress", result.WizardStatus);
        Assert.Equal(2, result.CurrentStep);
        Assert.Equal("Test Company", result.OrganizationIdentity.LegalNameEn);
        Assert.True(result.SectionCompleted["A"]);
        Assert.False(result.SectionCompleted["B"]);
    }

    #endregion

    #region GetDerivedScopeAsync Tests

    [Fact]
    public async Task GetDerivedScopeAsync_ReturnsBaselinesPackagesTemplates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var baselines = new List<TenantBaseline>
        {
            new TenantBaseline { TenantId = tenantId, BaselineCode = "ISO27001", IsDeleted = false }
        };
        var packages = new List<TenantPackage>
        {
            new TenantPackage { TenantId = tenantId, PackageCode = "SAMA-PKG", IsDeleted = false }
        };
        var templates = new List<TenantTemplate>
        {
            new TenantTemplate { TenantId = tenantId, TemplateCode = "BANKING-TPL", IsDeleted = false }
        };

        SetupDerivedScope(tenantId, baselines, packages, templates);

        // Act
        var result = await _service.GetDerivedScopeAsync(tenantId);

        // Assert
        Assert.Equal(tenantId, result.TenantId);
        Assert.Single(result.ApplicableBaselines);
        Assert.Equal("ISO27001", result.ApplicableBaselines[0].BaselineCode);
        Assert.Single(result.ApplicablePackages);
        Assert.Equal("SAMA-PKG", result.ApplicablePackages[0].PackageCode);
        Assert.Single(result.ApplicableTemplates);
        Assert.Equal("BANKING-TPL", result.ApplicableTemplates[0].TemplateCode);
    }

    #endregion

    #region Helper Methods

    private OnboardingWizard CreateTestWizard(Guid tenantId)
    {
        return new OnboardingWizard
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WizardStatus = "InProgress",
            CurrentStep = 1,
            ProgressPercent = 0,
            CompletedSectionsJson = "[]",
            StartedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test-user"
        };
    }

    private SectionA_OrganizationIdentity CreateCompleteSectionA()
    {
        return new SectionA_OrganizationIdentity
        {
            LegalNameEn = "Test Company",
            LegalNameAr = "شركة اختبار",
            TradeName = "TestCo",
            CountryOfIncorporation = "Saudi Arabia",
            OperatingCountries = new List<string> { "Saudi Arabia" },
            PrimaryHqLocation = "Riyadh",
            Timezone = "Arabia Standard Time",
            PrimaryLanguage = "English",
            CorporateEmailDomains = new List<string> { "test.com" },
            DomainVerificationMethod = "DNS",
            OrgType = "Corporation",
            Sectors = new List<string> { "Finance" },
            BusinessLines = new List<string> { "Banking" },
            HasDataResidencyRequirement = false
        };
    }

    private MinimalOnboardingDto CreateMinimalOnboardingDto()
    {
        return new MinimalOnboardingDto
        {
            // Section A
            LegalNameEn = "Test Company",
            LegalNameAr = "شركة اختبار",
            TradeName = "TestCo",
            CountryOfIncorporation = "Saudi Arabia",
            OperatingCountries = new List<string> { "Saudi Arabia" },
            PrimaryHqLocation = "Riyadh",
            Timezone = "Arabia Standard Time",
            PrimaryLanguage = "English",
            CorporateEmailDomains = new List<string> { "test.com" },
            DomainVerificationMethod = "DNS",

            // Section D
            InScopeLegalEntities = new List<LegalEntityScope>
            {
                new LegalEntityScope { EntityName = "Main Company" }
            },
            InScopeBusinessUnits = new List<BusinessUnitScope>(),
            InScopeSystems = new List<SystemScope>(),
            InScopeProcesses = new List<string>(),
            InScopeEnvironments = new List<string> { "Production" },
            InScopeLocations = new List<LocationScope>(),
            CriticalityTiers = new Dictionary<string, string>(),
            ImportantBusinessServices = new List<ImportantBusinessService>(),

            // Section E
            DataTypesProcessed = new List<string> { "PII" },
            HasPaymentCardData = false,
            HasCrossBorderTransfers = false,
            CustomerVolumeTier = "Tier1",

            // Section F
            IdentityProvider = "Azure AD",
            SsoEnabled = true,
            ScimEnabled = false,
            ItsmPlatform = "ServiceNow",
            EvidenceRepository = "SharePoint",
            SiemPlatform = "Sentinel",
            VulnerabilityManagement = "Tenable",
            EdrPlatform = "Defender",
            CloudProviders = new List<string> { "Azure" },
            ErpPlatform = "SAP",

            // Section H
            OrgAdmins = new List<OrgAdmin>
            {
                new OrgAdmin { Email = "admin@test.com", Name = "Admin" }
            },
            CreateTeamsNow = false,
            Teams = new List<WizardTeamDefinition>(),
            TeamMembers = new List<TeamMemberDefinition>(),
            RoleCatalog = new List<string> { "ControlOwner" },
            RaciMappingNeeded = false,

            // Section I
            EvidenceFrequencyByDomain = new Dictionary<string, string>(),
            AccessReviewFrequency = "Quarterly",
            VulnerabilityReviewFrequency = "Monthly",
            BackupReviewFrequency = "Monthly",
            RestoreTestCadence = "Quarterly",
            DrExerciseCadence = "Annual",
            IncidentTabletopCadence = "SemiAnnual",
            EvidenceSubmitSlaDays = 5,
            RemediationSlaDays = new Dictionary<string, int>
            {
                ["Critical"] = 7,
                ["High"] = 14,
                ["Medium"] = 30,
                ["Low"] = 90
            },
            ExceptionExpiryDays = 90
        };
    }

    private void SetupWizardQuery(Guid tenantId, OnboardingWizard wizard)
    {
        var queryable = new List<OnboardingWizard> { wizard }.AsQueryable();
        var mockDbSet = new Mock<DbSet<OnboardingWizard>>();
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<OnboardingWizard>(queryable.Provider));
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<OnboardingWizard>>().Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<OnboardingWizard>(queryable.GetEnumerator()));

        _mockWizardRepo.Setup(r => r.Query()).Returns(mockDbSet.Object);
    }

    private void SetupEmptyWizardQuery(Guid tenantId)
    {
        var emptyQueryable = new List<OnboardingWizard>().AsQueryable();
        var mockDbSet = new Mock<DbSet<OnboardingWizard>>();
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<OnboardingWizard>(emptyQueryable.Provider));
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.Expression).Returns(emptyQueryable.Expression);
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.ElementType).Returns(emptyQueryable.ElementType);
        mockDbSet.As<IQueryable<OnboardingWizard>>().Setup(m => m.GetEnumerator()).Returns(emptyQueryable.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<OnboardingWizard>>().Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<OnboardingWizard>(emptyQueryable.GetEnumerator()));

        _mockWizardRepo.Setup(r => r.Query()).Returns(mockDbSet.Object);
    }

    private void SetupEmptyDerivedScope(Guid tenantId)
    {
        SetupDerivedScope(tenantId,
            new List<TenantBaseline>(),
            new List<TenantPackage>(),
            new List<TenantTemplate>());
    }

    private void SetupDerivedScope(Guid tenantId,
        List<TenantBaseline> baselines,
        List<TenantPackage> packages,
        List<TenantTemplate> templates)
    {
        var baselineQuery = baselines.AsQueryable();
        var mockBaselineDbSet = new Mock<DbSet<TenantBaseline>>();
        mockBaselineDbSet.As<IQueryable<TenantBaseline>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TenantBaseline>(baselineQuery.Provider));
        mockBaselineDbSet.As<IQueryable<TenantBaseline>>().Setup(m => m.Expression).Returns(baselineQuery.Expression);
        mockBaselineDbSet.As<IQueryable<TenantBaseline>>().Setup(m => m.ElementType).Returns(baselineQuery.ElementType);
        mockBaselineDbSet.As<IQueryable<TenantBaseline>>().Setup(m => m.GetEnumerator()).Returns(baselineQuery.GetEnumerator());
        mockBaselineDbSet.As<IAsyncEnumerable<TenantBaseline>>().Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<TenantBaseline>(baselineQuery.GetEnumerator()));
        _mockBaselineRepo.Setup(r => r.Query()).Returns(mockBaselineDbSet.Object);

        var packageQuery = packages.AsQueryable();
        var mockPackageDbSet = new Mock<DbSet<TenantPackage>>();
        mockPackageDbSet.As<IQueryable<TenantPackage>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TenantPackage>(packageQuery.Provider));
        mockPackageDbSet.As<IQueryable<TenantPackage>>().Setup(m => m.Expression).Returns(packageQuery.Expression);
        mockPackageDbSet.As<IQueryable<TenantPackage>>().Setup(m => m.ElementType).Returns(packageQuery.ElementType);
        mockPackageDbSet.As<IQueryable<TenantPackage>>().Setup(m => m.GetEnumerator()).Returns(packageQuery.GetEnumerator());
        mockPackageDbSet.As<IAsyncEnumerable<TenantPackage>>().Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<TenantPackage>(packageQuery.GetEnumerator()));
        _mockPackageRepo.Setup(r => r.Query()).Returns(mockPackageDbSet.Object);

        var templateQuery = templates.AsQueryable();
        var mockTemplateDbSet = new Mock<DbSet<TenantTemplate>>();
        mockTemplateDbSet.As<IQueryable<TenantTemplate>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TenantTemplate>(templateQuery.Provider));
        mockTemplateDbSet.As<IQueryable<TenantTemplate>>().Setup(m => m.Expression).Returns(templateQuery.Expression);
        mockTemplateDbSet.As<IQueryable<TenantTemplate>>().Setup(m => m.ElementType).Returns(templateQuery.ElementType);
        mockTemplateDbSet.As<IQueryable<TenantTemplate>>().Setup(m => m.GetEnumerator()).Returns(templateQuery.GetEnumerator());
        mockTemplateDbSet.As<IAsyncEnumerable<TenantTemplate>>().Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<TenantTemplate>(templateQuery.GetEnumerator()));
        _mockTemplateRepo.Setup(r => r.Query()).Returns(mockTemplateDbSet.Object);
    }

    #endregion
}

#region Async Test Helpers

internal class TestAsyncQueryProvider<TEntity> : IQueryProvider, IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }
}

#endregion
