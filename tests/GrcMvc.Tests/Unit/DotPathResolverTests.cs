using Xunit;
using GrcMvc.Application.Policy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace GrcMvc.Tests.Unit
{
    /// <summary>
    /// Unit tests for DotPathResolver - Tests path resolution and condition operations
    /// </summary>
    public class DotPathResolverTests
    {
        private readonly DotPathResolver _resolver;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DotPathResolver> _logger;

        public DotPathResolverTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _logger = Mock.Of<ILogger<DotPathResolver>>();
            _resolver = new DotPathResolver(_cache, _logger);
        }

        [Fact]
        public void Resolve_SimplePath_ReturnsValue()
        {
            // Arrange
            var obj = new { Name = "Test" };
            var path = "Name";

            // Act
            var result = _resolver.Resolve(obj, path);

            // Assert
            Assert.Equal("Test", result);
        }

        [Fact]
        public void Resolve_NestedPath_ReturnsValue()
        {
            // Arrange
            var obj = new { Metadata = new { Labels = new { Owner = "Team1" } } };
            var path = "Metadata.Labels.Owner";

            // Act
            var result = _resolver.Resolve(obj, path);

            // Assert
            Assert.Equal("Team1", result);
        }

        [Fact]
        public void Resolve_MissingPath_ReturnsNull()
        {
            // Arrange
            var obj = new { Name = "Test" };
            var path = "Missing";

            // Act
            var result = _resolver.Resolve(obj, path);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_DictionaryPath_ReturnsValue()
        {
            // Arrange
            var obj = new Dictionary<string, object>
            {
                ["Metadata"] = new Dictionary<string, object>
                {
                    ["Labels"] = new Dictionary<string, string>
                    {
                        ["DataClassification"] = "confidential"
                    }
                }
            };
            var path = "Metadata.Labels.DataClassification";

            // Act
            var result = _resolver.Resolve(obj, path);

            // Assert
            Assert.Equal("confidential", result);
        }

        [Fact]
        public void Resolve_ArrayIndex_ReturnsValue()
        {
            // Arrange
            var obj = new { Items = new[] { "Item1", "Item2" } };
            var path = "Items[0]";

            // Act
            var result = _resolver.Resolve(obj, path);

            // Assert
            Assert.Equal("Item1", result);
        }

        [Fact]
        public void Exists_WithValidPath_ReturnsTrue()
        {
            // Arrange
            var obj = new { Name = "Test" };

            // Act
            var result = _resolver.Exists(obj, "Name");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Exists_WithInvalidPath_ReturnsFalse()
        {
            // Arrange
            var obj = new { Name = "Test" };

            // Act
            var result = _resolver.Exists(obj, "Missing");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Set_WithValidPath_SetsValue()
        {
            // Arrange
            var obj = new TestClass { Name = "Old" };

            // Act
            _resolver.Set(obj, "Name", "New");

            // Assert
            Assert.Equal("New", obj.Name);
        }

        [Fact]
        public void Remove_WithValidPath_SetsToNull()
        {
            // Arrange
            var obj = new TestClass { Name = "Test" };

            // Act
            _resolver.Remove(obj, "Name");

            // Assert
            Assert.Null(obj.Name);
        }

        private class TestClass
        {
            public string? Name { get; set; }
        }
    }
}
