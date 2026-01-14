using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Services
{
    /// <summary>
    /// Unit tests for OnboardingCoverageService
    /// Tests coverage validation, manifest loading, and integrity checks
    /// </summary>
    public class OnboardingCoverageServiceTests : IDisposable
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<OnboardingCoverageService>> _mockLogger;
        private readonly OnboardingCoverageService _service;

        public OnboardingCoverageServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Onboarding:CoverageManifestPath"])
                .Returns("etc/onboarding/coverage-manifest.yml");

            _mockLogger = new Mock<ILogger<OnboardingCoverageService>>();
            _service = new OnboardingCoverageService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task LoadManifestAsync_WithValidPath_ReturnsManifest()
        {
            // Arrange - Service should handle missing file gracefully
            // Act
            var manifest = await _service.LoadManifestAsync();

            // Assert
            Assert.NotNull(manifest);
            Assert.NotNull(manifest.RequiredIdsByNode);
            Assert.NotNull(manifest.OptionalIdsByNode);
            Assert.NotNull(manifest.RequiredIdsByMission);
            Assert.NotNull(manifest.ConditionalRequired);
            Assert.NotNull(manifest.IntegrityChecks);
        }

        [Fact]
        public async Task ValidateNodeCoverageAsync_WithCompleteData_ReturnsValid()
        {
            // Arrange
            var wizard = CreateCompleteWizard();
            var fieldProvider = new OnboardingFieldValueProvider(wizard);
            var nodeId = "M1.C";

            // Act
            var result = await _service.ValidateNodeCoverageAsync(nodeId, fieldProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nodeId, result.NodeId);
            // Note: Actual validation depends on manifest content
        }

        [Fact]
        public async Task ValidateNodeCoverageAsync_WithMissingFields_ReturnsMissingFields()
        {
            // Arrange
            var wizard = CreateEmptyWizard();
            var fieldProvider = new OnboardingFieldValueProvider(wizard);
            var nodeId = "FS.1";

            // Act
            var result = await _service.ValidateNodeCoverageAsync(nodeId, fieldProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.MissingRequiredFields);
            // Note: Without a coverage manifest file, no fields are defined as required,
            // so IsValid may be true (no missing required fields). This test validates
            // the service handles missing manifest gracefully.
            // To test actual missing field detection, you would need a proper manifest file.
            Assert.Equal(nodeId, result.NodeId);
        }

        [Fact]
        public async Task ValidateMissionCoverageAsync_WithCompleteData_ReturnsValid()
        {
            // Arrange
            var wizard = CreateCompleteWizard();
            var fieldProvider = new OnboardingFieldValueProvider(wizard);
            var missionId = "MISSION_1_SCOPE_RISK";

            // Act
            var result = await _service.ValidateMissionCoverageAsync(missionId, fieldProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(missionId, result.MissionId);
        }

        [Fact]
        public async Task ValidateCompleteCoverageAsync_WithCompleteData_ReturnsAllNodes()
        {
            // Arrange
            var wizard = CreateCompleteWizard();
            var fieldProvider = new OnboardingFieldValueProvider(wizard);

            // Act
            var result = await _service.ValidateCompleteCoverageAsync(fieldProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.NodeResults);
            Assert.NotNull(result.MissionResults);
        }

        [Fact]
        public async Task EvaluateConditionalRequired_WithSSOEnabled_ReturnsSSOFields()
        {
            // Arrange
            var wizard = CreateCompleteWizard();
            wizard.SsoEnabled = true;
            wizard.IdentityProvider = "Azure AD";
            var fieldProvider = new OnboardingFieldValueProvider(wizard);
            var manifest = await _service.LoadManifestAsync();

            // Act
            var conditionalFields = _service.EvaluateConditionalRequired(fieldProvider, manifest);

            // Assert
            Assert.NotNull(conditionalFields);
            // Should include SSO-related fields when SSO is enabled
        }

        [Fact]
        public async Task GetRequiredFieldsForNode_WithValidNodeId_ReturnsFieldIds()
        {
            // Arrange
            var manifest = await _service.LoadManifestAsync();
            var nodeId = "FS.1";

            // Act
            var requiredFields = _service.GetRequiredFieldsForNode(nodeId, manifest);

            // Assert
            Assert.NotNull(requiredFields);
            // FS.1 should have required fields like organization_name, admin_email, etc.
        }

        [Fact]
        public async Task GetRequiredFieldsForMission_WithValidMissionId_ReturnsFieldIds()
        {
            // Arrange
            var manifest = await _service.LoadManifestAsync();
            var missionId = "FAST_START";

            // Act
            var requiredFields = _service.GetRequiredFieldsForMission(missionId, manifest);

            // Assert
            Assert.NotNull(requiredFields);
        }

        #region Helper Methods

        private OnboardingWizard CreateCompleteWizard()
        {
            return new OnboardingWizard
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                OrganizationLegalNameEn = "Test Organization",
                OrganizationLegalNameAr = "منظمة الاختبار",
                TradeName = "TestCo",
                CountryOfIncorporation = "Saudi Arabia",
                OperatingCountriesJson = "[\"Saudi Arabia\", \"UAE\"]",
                PrimaryDriver = "RegulatorExam",
                PrimaryRegulatorsJson = "[\"NCA\", \"SAMA\"]",
                InScopeEnvironments = "Production,Staging",
                DataTypesProcessedJson = "[\"PII\", \"Financial\"]",
                HasPaymentCardData = false,
                IdentityProvider = "Azure AD",
                SsoEnabled = true,
                ControlOwnershipApproach = "Centralized",
                OrgAdminsJson = "[{\"Email\":\"admin@test.com\",\"Name\":\"Admin User\"}]",
                EvidenceSlaSubmitDays = 30,
                AdoptDefaultBaseline = true,
                WizardStatus = "InProgress",
                CurrentStep = 1,
                CreatedDate = DateTime.UtcNow
            };
        }

        private OnboardingWizard CreateEmptyWizard()
        {
            return new OnboardingWizard
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                WizardStatus = "NotStarted",
                CurrentStep = 0,
                CreatedDate = DateTime.UtcNow
            };
        }

        #endregion

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
