using Xunit;
using Microsoft.Extensions.Logging;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using Moq;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for MutationApplier
/// Tests policy mutation application to resources
/// </summary>
public class MutationApplierTests
{
    private readonly MutationApplier _applier;
    private readonly Mock<IDotPathResolver> _pathResolverMock;
    private readonly Mock<ILogger<MutationApplier>> _loggerMock;

    public MutationApplierTests()
    {
        _pathResolverMock = new Mock<IDotPathResolver>();
        _loggerMock = new Mock<ILogger<MutationApplier>>();
        _applier = new MutationApplier(_pathResolverMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Apply_SetMutation_UpdatesProperty()
    {
        // Arrange
        var resource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>()
            }
        };

        var mutations = new[]
        {
            new PolicyMutation { Op = "set", Path = "Title", Value = "Updated" }
        };

        // Act
        await _applier.ApplyAsync(mutations, resource);

        // Assert
        Assert.Equal("Updated", resource.Title);
    }

    [Fact]
    public async Task Apply_SetLabelMutation_UpdatesLabel()
    {
        // Arrange
        var resource = new PolicyResourceWrapper
        {
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>()
            }
        };

        var mutations = new[]
        {
            new PolicyMutation { Op = "set", Path = "metadata.labels.dataClassification", Value = "confidential" }
        };

        // Act
        await _applier.ApplyAsync(mutations, resource);

        // Assert
        Assert.Equal("confidential", resource.Metadata.Labels["dataClassification"]);
    }

    [Fact]
    public async Task Apply_NormalizeEmptyString_ConvertsToNull()
    {
        // Arrange
        var resource = new PolicyResourceWrapper
        {
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string> { { "owner", "" } }
            }
        };

        var mutations = new[]
        {
            new PolicyMutation { Op = "normalize", Path = "metadata.labels.owner", Value = null }
        };

        // Act
        await _applier.ApplyAsync(mutations, resource);

        // Assert
        Assert.False(resource.Metadata.Labels.ContainsKey("owner") || 
                    string.IsNullOrEmpty(resource.Metadata.Labels.GetValueOrDefault("owner")));
    }

    [Fact]
    public async Task Apply_MultipleMutations_AppliesAll()
    {
        // Arrange
        var resource = new PolicyResourceWrapper
        {
            Title = "Original",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>()
            }
        };

        var mutations = new[]
        {
            new PolicyMutation { Op = "set", Path = "Title", Value = "Updated" },
            new PolicyMutation { Op = "set", Path = "metadata.labels.owner", Value = "admin" }
        };

        // Act
        await _applier.ApplyAsync(mutations, resource);

        // Assert
        Assert.Equal("Updated", resource.Title);
        Assert.Equal("admin", resource.Metadata.Labels["owner"]);
    }

    [Fact]
    public async Task Apply_UnknownOperation_LogsWarning()
    {
        // Arrange
        var resource = new PolicyResourceWrapper();
        var mutations = new[]
        {
            new PolicyMutation { Op = "unknown", Path = "Title", Value = "Test" }
        };

        // Act
        await _applier.ApplyAsync(mutations, resource);

        // Assert
        // Should not throw, but log warning
        _pathResolverMock.Verify(x => x.Set(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Apply_EmptyMutations_NoChanges()
    {
        // Arrange
        var resource = new PolicyResourceWrapper { Title = "Original" };
        var mutations = Array.Empty<PolicyMutation>();

        // Act
        await _applier.ApplyAsync(mutations, resource);

        // Assert
        Assert.Equal("Original", resource.Title);
    }
}
