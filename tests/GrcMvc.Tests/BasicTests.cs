using Xunit;

namespace GrcMvc.Tests;

public class BasicTests
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var expected = 42;
        var actual = 42;

        // Act & Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void String_Concatenation_ShouldWork()
    {
        // Arrange
        var first = "Hello";
        var second = "World";

        // Act
        var result = $"{first} {second}";

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Theory]
    [InlineData(2, 4)]
    [InlineData(3, 9)]
    [InlineData(4, 16)]
    public void Square_ShouldCalculateCorrectly(int input, int expected)
    {
        // Act
        var actual = input * input;

        // Assert
        Assert.Equal(expected, actual);
    }
}
