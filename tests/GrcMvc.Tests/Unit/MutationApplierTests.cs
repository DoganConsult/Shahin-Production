using Xunit;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Tests.Unit
{
    /// <summary>
    /// Unit tests for MutationApplier - Tests set/remove/add operations
    /// </summary>
    public class MutationApplierTests
    {
        private readonly MutationApplier _applier;
        private readonly Mock<IDotPathResolver> _pathResolverMock;
        private readonly ILogger<MutationApplier> _logger;

        public MutationApplierTests()
        {
            _pathResolverMock = new Mock<IDotPathResolver>();
            _logger = Mock.Of<ILogger<MutationApplier>>();
            _applier = new MutationApplier(_pathResolverMock.Object, _logger);
        }

        [Fact]
        public async Task ApplyAsync_SetOperation_CallsPathResolver()
        {
            // Arrange
            var resource = new PolicyResourceWrapper
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Metadata = new PolicyResourceMetadata
                {
                    Labels = new Dictionary<string, string>()
                }
            };
            var mutations = new List<PolicyMutation>
            {
                new PolicyMutation { Op = "set", Path = "Title", Value = "New Title" }
            };

            // Act
            await _applier.ApplyAsync(mutations, resource);

            // Assert
            _pathResolverMock.Verify(r => r.Set(resource, "Title", "New Title"), Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_SetMetadataLabels_UpdatesLabels()
        {
            // Arrange
            var resource = new PolicyResourceWrapper
            {
                Id = Guid.NewGuid(),
                Metadata = new PolicyResourceMetadata
                {
                    Labels = new Dictionary<string, string>()
                }
            };
            var mutations = new List<PolicyMutation>
            {
                new PolicyMutation { Op = "set", Path = "metadata.labels.owner", Value = "Team1" }
            };

            // Act
            await _applier.ApplyAsync(mutations, resource);

            // Assert
            Assert.Equal("Team1", resource.Metadata.Labels["owner"]);
        }

        [Fact]
        public async Task ApplyAsync_RemoveOperation_CallsPathResolver()
        {
            // Arrange
            var resource = new PolicyResourceWrapper
            {
                Id = Guid.NewGuid(),
                Metadata = new PolicyResourceMetadata
                {
                    Labels = new Dictionary<string, string> { ["owner"] = "Team1" }
                }
            };
            var mutations = new List<PolicyMutation>
            {
                new PolicyMutation { Op = "remove", Path = "metadata.labels.owner" }
            };

            // Act
            await _applier.ApplyAsync(mutations, resource);

            // Assert
            Assert.False(resource.Metadata.Labels.ContainsKey("owner"));
        }

        [Fact]
        public async Task ApplyAsync_MultipleMutations_AppliesAll()
        {
            // Arrange
            var resource = new PolicyResourceWrapper
            {
                Id = Guid.NewGuid(),
                Metadata = new PolicyResourceMetadata
                {
                    Labels = new Dictionary<string, string>()
                }
            };
            var mutations = new List<PolicyMutation>
            {
                new PolicyMutation { Op = "set", Path = "metadata.labels.owner", Value = "Team1" },
                new PolicyMutation { Op = "set", Path = "metadata.labels.dataClassification", Value = "internal" }
            };

            // Act
            await _applier.ApplyAsync(mutations, resource);

            // Assert
            Assert.Equal("Team1", resource.Metadata.Labels["owner"]);
            Assert.Equal("internal", resource.Metadata.Labels["dataClassification"]);
        }
    }
}
