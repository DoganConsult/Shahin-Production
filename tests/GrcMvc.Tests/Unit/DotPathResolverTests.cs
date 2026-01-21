using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using GrcMvc.Application.Policy;
using Moq;

namespace GrcMvc.Tests.Unit;

/// <summary>
/// Unit tests for DotPathResolver
/// Tests path resolution for policy evaluation
/// </summary>
public class DotPathResolverTests
{
    private readonly DotPathResolver _resolver;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<DotPathResolver>> _loggerMock;

    public DotPathResolverTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<DotPathResolver>>();
        _resolver = new DotPathResolver(_cache, _loggerMock.Object);
    }

    [Fact]
    public void Resolve_SimpleProperty_ReturnsValue()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123 };

        // Act
        var result = _resolver.Resolve(obj, "Name");

        // Assert
        Assert.Equal("Test", result);
    }

    [Fact]
    public void Resolve_NestedProperty_ReturnsValue()
    {
        // Arrange
        var obj = new { User = new { Name = "John", Email = "john@example.com" } };

        // Act
        var result = _resolver.Resolve(obj, "User.Name");

        // Assert
        Assert.Equal("John", result);
    }

    [Fact]
    public void Resolve_DeepNestedProperty_ReturnsValue()
    {
        // Arrange
        var obj = new { A = new { B = new { C = new { Value = "Deep" } } } };

        // Act
        var result = _resolver.Resolve(obj, "A.B.C.Value");

        // Assert
        Assert.Equal("Deep", result);
    }

    [Fact]
    public void Resolve_NonExistentProperty_ReturnsNull()
    {
        // Arrange
        var obj = new { Name = "Test" };

        // Act
        var result = _resolver.Resolve(obj, "NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_EmptyPath_ReturnsObject()
    {
        // Arrange
        var obj = new { Name = "Test" };

        // Act
        var result = _resolver.Resolve(obj, "");

        // Assert
        Assert.Equal(obj, result);
    }

    [Fact]
    public void Resolve_NullObject_ReturnsNull()
    {
        // Act
        var result = _resolver.Resolve(null, "Name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_ArrayIndex_ReturnsElement()
    {
        // Arrange
        var obj = new { Items = new[] { "First", "Second", "Third" } };

        // Act
        var result = _resolver.Resolve(obj, "Items[0]");

        // Assert
        Assert.Equal("First", result);
    }

    [Fact]
    public void Resolve_DictionaryKey_ReturnsValue()
    {
        // Arrange
        var obj = new { Metadata = new Dictionary<string, string> { { "Key", "Value" } } };

        // Act
        var result = _resolver.Resolve(obj, "Metadata.Key");

        // Assert
        Assert.Equal("Value", result);
    }
}
