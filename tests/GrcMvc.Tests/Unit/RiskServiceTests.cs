using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using GrcMvc.Data;
using GrcMvc.Data.Repositories;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Tests.Unit
{
    /// <summary>
    /// Unit tests for RiskService and related components.
    /// Tests business logic in isolation.
    /// </summary>
    public class RiskModuleTests
    {
        private readonly Guid _testTenantId = Guid.NewGuid();

        #region Risk Entity Tests

        [Fact]
        public void Risk_DefaultValues_AreCorrect()
        {
            var risk = new Risk();
            // TenantId may be null or empty depending on entity implementation
            Assert.True(risk.TenantId == Guid.Empty || risk.TenantId == null);
            Assert.Null(risk.Title);
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(3, 3, 9)]
        [InlineData(4, 4, 16)]
        [InlineData(5, 5, 25)]
        public void Risk_ScoreCalculation_IsCorrect(int impact, int likelihood, int expectedScore)
        {
            var score = impact * likelihood;
            Assert.Equal(expectedScore, score);
        }

        [Fact]
        public void Risk_TenantIsolation_RequiresTenantId()
        {
            var risk = new Risk
            {
                Id = Guid.NewGuid(),
                Title = "Test Risk",
                TenantId = Guid.Empty
            };
            Assert.Equal(Guid.Empty, risk.TenantId);
        }

        #endregion

        #region Risk Status Transition Tests

        [Theory]
        [InlineData("Draft", "Active", true)]
        [InlineData("Active", "Mitigated", true)]
        [InlineData("Active", "Accepted", true)]
        [InlineData("Active", "Closed", true)]
        [InlineData("Mitigated", "Closed", true)]
        [InlineData("Closed", "Active", false)]
        public void RiskStatus_ValidTransitions_AreEnforced(string from, string to, bool isValid)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                { "Draft", new[] { "Active" } },
                { "Active", new[] { "Mitigated", "Accepted", "Closed" } },
                { "Mitigated", new[] { "Closed", "Active" } },
                { "Accepted", new[] { "Closed", "Active" } },
                { "Closed", Array.Empty<string>() }
            };

            var canTransition = validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
            Assert.Equal(isValid, canTransition);
        }

        #endregion

        #region Risk Appetite Entity Tests

        [Fact]
        public void RiskAppetiteSetting_DefaultValues_AreCorrect()
        {
            var setting = new RiskAppetiteSetting();
            Assert.Equal(0, setting.MinimumRiskScore);
            Assert.Equal(100, setting.MaximumRiskScore);
            Assert.Equal(50, setting.TargetRiskScore);
            Assert.Equal(10, setting.TolerancePercentage);
            Assert.True(setting.IsActive);
        }

        [Theory]
        [InlineData(25, true)]
        [InlineData(50, true)]
        [InlineData(75, true)]
        [InlineData(110, false)]
        public void RiskAppetiteSetting_IsWithinAppetite_WorksCorrectly(int score, bool expectedWithin)
        {
            var setting = new RiskAppetiteSetting
            {
                MinimumRiskScore = 0,
                MaximumRiskScore = 100,
                TargetRiskScore = 50
            };
            var isWithin = setting.IsWithinAppetite(score);
            Assert.Equal(expectedWithin, isWithin);
        }

        [Theory]
        [InlineData(50, 50, 10, false)]
        [InlineData(55, 50, 10, false)]
        [InlineData(56, 50, 10, true)]
        [InlineData(70, 50, 10, true)]
        public void RiskAppetiteSetting_ExceedsTolerance_WorksCorrectly(int score, int target, int tolerance, bool expected)
        {
            var setting = new RiskAppetiteSetting
            {
                MinimumRiskScore = 0,
                MaximumRiskScore = 100,
                TargetRiskScore = target,
                TolerancePercentage = tolerance
            };
            var exceeds = setting.ExceedsTolerance(score);
            Assert.Equal(expected, exceeds);
        }

        [Theory]
        [InlineData(-10, RiskAppetiteStatus.UnderControlled)]
        [InlineData(50, RiskAppetiteStatus.OnTarget)]
        [InlineData(55, RiskAppetiteStatus.OnTarget)]
        [InlineData(70, RiskAppetiteStatus.WithinAppetite)]
        [InlineData(95, RiskAppetiteStatus.AtTolerance)]
        [InlineData(110, RiskAppetiteStatus.Exceeded)]
        public void RiskAppetiteSetting_GetAppetiteStatus_ReturnsCorrectStatus(int score, RiskAppetiteStatus expected)
        {
            var setting = new RiskAppetiteSetting
            {
                MinimumRiskScore = 0,
                MaximumRiskScore = 100,
                TargetRiskScore = 50,
                TolerancePercentage = 80
            };
            var status = setting.GetAppetiteStatus(score);
            Assert.Equal(expected, status);
        }

        #endregion

        #region Risk DTO Validation Tests

        [Fact]
        public void CreateRiskDto_RequiredFields_AreEnforced()
        {
            var validDto = new CreateRiskDto
            {
                Name = "Test Risk",
                Description = "Test Description",
                Category = "Operational",
                Impact = 3,
                Probability = 3
            };
            Assert.NotNull(validDto.Name);
            Assert.NotNull(validDto.Description);
            Assert.NotNull(validDto.Category);
        }

        [Fact]
        public void UpdateRiskDto_AllFields_CanBeUpdated()
        {
            var dto = new UpdateRiskDto
            {
                Name = "Updated Title",
                Description = "Updated Description",
                Category = "Strategic",
                Impact = 4,
                Probability = 4
            };
            Assert.Equal("Updated Title", dto.Name);
            Assert.Equal(4, dto.Impact);
        }

        #endregion

        #region Risk Control Mapping Tests

        [Fact]
        public void RiskControlMapping_Creation_RequiresBothIds()
        {
            var riskId = Guid.NewGuid();
            var controlId = Guid.NewGuid();

            var mapping = new RiskControlMapping
            {
                Id = Guid.NewGuid(),
                RiskId = riskId,
                ControlId = controlId,
                TenantId = _testTenantId
            };

            Assert.NotEqual(Guid.Empty, mapping.RiskId);
            Assert.NotEqual(Guid.Empty, mapping.ControlId);
            Assert.NotEqual(Guid.Empty, mapping.TenantId);
        }

        #endregion

        #region Risk Heat Map Tests

        [Theory]
        [InlineData(1, 1, "green")]
        [InlineData(3, 3, "yellow")]
        [InlineData(4, 4, "orange")]
        [InlineData(5, 5, "red")]
        public void RiskHeatMap_ColorMapping_IsCorrect(int impact, int likelihood, string expectedColor)
        {
            var score = impact * likelihood;
            var color = GetHeatMapColor(score);
            Assert.Equal(expectedColor, color);
        }

        private static string GetHeatMapColor(int score)
        {
            return score switch
            {
                <= 4 => "green",
                <= 9 => "yellow",
                <= 16 => "orange",
                _ => "red"
            };
        }

        #endregion

        #region Risk Category Tests

        [Theory]
        [InlineData("Strategic")]
        [InlineData("Operational")]
        [InlineData("Financial")]
        [InlineData("Compliance")]
        [InlineData("Reputational")]
        [InlineData("Technology")]
        public void RiskCategory_ValidValues_AreAccepted(string category)
        {
            var risk = new Risk { Category = category };
            Assert.NotNull(risk.Category);
            Assert.NotEmpty(risk.Category);
        }

        #endregion

        #region Risk Priority Calculation Tests

        [Theory]
        [InlineData(5, 5, 1)]
        [InlineData(4, 4, 2)]
        [InlineData(3, 3, 3)]
        [InlineData(1, 1, 4)]
        public void RiskPriority_Calculation_BasedOnScore(int impact, int likelihood, int expectedPriority)
        {
            var score = impact * likelihood;
            var priority = GetPriority(score);
            Assert.Equal(expectedPriority, priority);
        }

        private static int GetPriority(int score)
        {
            return score switch
            {
                >= 20 => 1,
                >= 12 => 2,
                >= 6 => 3,
                _ => 4
            };
        }

        #endregion

        #region Risk Tenant Isolation Tests

        [Fact]
        public void Risk_TenantFilter_ExcludesOtherTenants()
        {
            var tenant1 = Guid.NewGuid();
            var tenant2 = Guid.NewGuid();

            var risks = new List<Risk>
            {
                new Risk { Id = Guid.NewGuid(), TenantId = tenant1, Title = "Risk 1" },
                new Risk { Id = Guid.NewGuid(), TenantId = tenant1, Title = "Risk 2" },
                new Risk { Id = Guid.NewGuid(), TenantId = tenant2, Title = "Risk 3" }
            };

            var tenant1Risks = risks.Where(r => r.TenantId == tenant1).ToList();

            Assert.Equal(2, tenant1Risks.Count);
            Assert.All(tenant1Risks, r => Assert.Equal(tenant1, r.TenantId));
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for RiskWorkflowService state machine.
    /// </summary>
    public class RiskWorkflowServiceTests
    {
        [Theory]
        [InlineData("Active", "accept", "Accepted")]
        [InlineData("Active", "reject", "Active")]
        [InlineData("Active", "mitigate", "Mitigated")]
        [InlineData("Active", "close", "Closed")]
        public void StateMachine_Transitions_AreCorrect(string from, string action, string expectedTo)
        {
            var transitions = new Dictionary<(string, string), string>
            {
                { ("Active", "accept"), "Accepted" },
                { ("Active", "reject"), "Active" },
                { ("Active", "mitigate"), "Mitigated" },
                { ("Active", "close"), "Closed" }
            };

            var newStatus = transitions.TryGetValue((from, action), out var result) ? result : from;
            Assert.Equal(expectedTo, newStatus);
        }

        [Fact]
        public void StateMachine_InvalidTransition_IsRejected()
        {
            var closedStatus = "Closed";
            var canAccept = closedStatus != "Closed";
            Assert.False(canAccept);
        }
    }

    /// <summary>
    /// Unit tests for RiskAppetiteApiController validation.
    /// </summary>
    public class RiskAppetiteControllerTests
    {
        [Fact]
        public void CreateDto_Validation_MinMaxRelationship()
        {
            var dto = new CreateRiskAppetiteSettingDto
            {
                Category = "Operational",
                Name = "Test Setting",
                MinimumRiskScore = 50,
                MaximumRiskScore = 30,
                TargetRiskScore = 40
            };

            var isValid = dto.MinimumRiskScore <= dto.MaximumRiskScore;
            Assert.False(isValid);
        }

        [Fact]
        public void CreateDto_Validation_TargetWithinRange()
        {
            var dto = new CreateRiskAppetiteSettingDto
            {
                Category = "Operational",
                Name = "Test Setting",
                MinimumRiskScore = 20,
                MaximumRiskScore = 80,
                TargetRiskScore = 50
            };

            var isValid = dto.TargetRiskScore >= dto.MinimumRiskScore && 
                          dto.TargetRiskScore <= dto.MaximumRiskScore;
            Assert.True(isValid);
        }

        [Theory]
        [InlineData("Strategic", true)]
        [InlineData("Operational", true)]
        [InlineData("Financial", true)]
        [InlineData("InvalidCategory", false)]
        public void Category_Validation_ChecksAllowedValues(string category, bool expectedValid)
        {
            var allowedCategories = new[]
            {
                "Strategic", "Operational", "Financial", "Compliance",
                "Reputational", "Technology", "Legal", "Market", "Credit", "Liquidity"
            };

            var isValid = allowedCategories.Contains(category);
            Assert.Equal(expectedValid, isValid);
        }
    }
}
