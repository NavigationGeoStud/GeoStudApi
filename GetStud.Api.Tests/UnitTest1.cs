namespace GetStud.Api.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Arrange
        var expected = "Hello World";

        // Act
        var actual = "Hello World";

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Math_Addition_ShouldWork()
    {
        // Arrange
        var a = 5;
        var b = 3;

        // Act
        var result = a + b;

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void String_IsNotNull_ShouldPass()
    {
        // Arrange
        var text = "Test string";

        // Act & Assert
        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }
}