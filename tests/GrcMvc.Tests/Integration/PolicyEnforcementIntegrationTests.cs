using Xunit;
using GrcMvc.Application.Policy;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Tests.Integration
{
    /// <summary>
    /// Integration tests for Policy Enforcement
    /// Tests real policy evaluation scenarios
    /// </summary>
    public class PolicyEnforcementIntegrationTests
    {
        private readonly PolicyEnforcementHelper _helper;
        private readonly Mock<IPolicyEnforcer> _enforcerMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IHostEnvironment> _environmentMock;
        private readonly ILogger<PolicyEnforcementHelper> _logger;

        public PolicyEnforcementIntegrationTests()
        {
            _enforcerMock = new Mock<IPolicyEnforcer>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _currentUserMock.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
            _currentUserMock.Setup(u => u.GetTenantId()).Returns(Guid.NewGuid());
            _currentUserMock.Setup(u => u.GetRoles()).Returns(new List<string> { "User" });
            _currentUserMock.Setup(u => u.GetUserName()).Returns("testuser");
            
            _environmentMock = new Mock<IHostEnvironment>();
            _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
            
            _logger = Mock.Of<ILogger<PolicyEnforcementHelper>>();
            _helper = new PolicyEnforcementHelper(
                _enforcerMock.Object,
                _currentUserMock.Object,
                _environmentMock.Object,
                _logger);
        }

        [Fact]
        public async Task EnforceCreateAsync_EvidenceWithoutDataClassification_ThrowsPolicyViolation()
        {
            // Arrange
            var evidence = new Evidence
            {
                Id = Guid.NewGuid(),
                Title = "Test Evidence",
                DataClassification = null // Missing classification
            };

            _enforcerMock.Setup(e => e.EnforceAsync(
                It.Is<PolicyContext>(ctx => 
                    ctx.Action == "create" && 
                    ctx.ResourceType == "Evidence"),
                default))
                .ThrowsAsync(new PolicyViolationException(
                    "Missing/invalid metadata.labels.dataClassification",
                    "REQUIRE_DATA_CLASSIFICATION",
                    "Set metadata.labels.dataClassification to one of the allowed values."));

            // Act & Assert
            await Assert.ThrowsAsync<PolicyViolationException>(async () =>
                await _helper.EnforceCreateAsync("Evidence", evidence));
        }

        [Fact]
        public async Task EnforceCreateAsync_EvidenceWithValidClassification_Allows()
        {
            // Arrange
            var evidence = new Evidence
            {
                Id = Guid.NewGuid(),
                Title = "Test Evidence",
                DataClassification = "internal",
                Owner = "Team1"
            };

            _enforcerMock.Setup(e => e.EnforceAsync(
                It.IsAny<PolicyContext>(),
                default))
                .Returns(Task.CompletedTask);

            // Act
            await _helper.EnforceCreateAsync("Evidence", evidence, 
                dataClassification: "internal", 
                owner: "Team1");

            // Assert - No exception thrown
            _enforcerMock.Verify(e => e.EnforceAsync(
                It.Is<PolicyContext>(ctx => ctx.Action == "create"),
                default), Times.Once);
        }

        [Fact]
        public async Task EnforceUpdateAsync_RiskWithValidData_Allows()
        {
            // Arrange
            var risk = new Risk
            {
                Id = Guid.NewGuid(),
                Name = "Test Risk",
                DataClassification = "confidential",
                Owner = "RiskTeam"
            };

            _enforcerMock.Setup(e => e.EnforceAsync(
                It.IsAny<PolicyContext>(),
                default))
                .Returns(Task.CompletedTask);

            // Act
            await _helper.EnforceUpdateAsync("Risk", risk,
                dataClassification: "confidential",
                owner: "RiskTeam");

            // Assert - No exception thrown
            _enforcerMock.Verify(e => e.EnforceAsync(
                It.Is<PolicyContext>(ctx => ctx.Action == "update"),
                default), Times.Once);
        }

        [Fact]
        public async Task EnforceSubmitAsync_AssessmentWithoutOwner_ThrowsPolicyViolation()
        {
            // Arrange
            var assessment = new Assessment
            {
                Id = Guid.NewGuid(),
                Name = "Test Assessment",
                Owner = null // Missing owner
            };

            _enforcerMock.Setup(e => e.EnforceAsync(
                It.Is<PolicyContext>(ctx => ctx.Action == "submit"),
                default))
                .ThrowsAsync(new PolicyViolationException(
                    "Missing/invalid metadata.labels.owner",
                    "REQUIRE_OWNER",
                    "Set metadata.labels.owner to a team or individual identifier."));

            // Act & Assert
            await Assert.ThrowsAsync<PolicyViolationException>(async () =>
                await _helper.EnforceSubmitAsync("Assessment", assessment));
        }
    }
}
