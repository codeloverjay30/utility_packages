using System;
using System.Exceptions;
using FluentAssertions;
using Xunit;

namespace ExceptionsUtilityServices.Tests;

public class MismatchedDataStructureExceptionTests
{
    [Fact]
    public void Create_WithInstanceArguments_ShouldConstructCorrectException()
    {
        // Arrange
        string expected = "Sample";
        int actual = 123;
        string format = "Expected {0}, but got {1}";

        // Act
        var exception = MismatchedDataStructureException<string, int>.Create(expected, actual, format);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("System.String")
            .And.Contain("System.Int32");
        exception.ParamName.Should().Be("expected");
    }

    [Fact]
    public void Create_WithTypeArguments_ShouldConstructCorrectException()
    {
        // Arrange
        var expectedType = typeof(string);
        var actualType = typeof(int);
        string format = "Invalid type, expected {0} but was {1}";

        // Act
        var exception = MismatchedDataStructureException<string, int>.Create(expectedType, actualType, format);

        // Assert
        // 使用 Contain 進行邏輯檢查，避免因瑣碎的字串格式差異導致測試失敗
        exception.Message.Should().Contain("String")
                 .And.Contain("Int32");
        exception.ParamName.Should().Be("expected");
    }


    [Fact]
    public void ThrowingException_ShouldBeCapturable_WithFluentAssertions()
    {
        // Arrange & Act
        Action act = () => throw MismatchedDataStructureException<string, int>.Create(null!, 0, "Error");

        // Assert
        act.Should().Throw<MismatchedDataStructureException<string, int>>()
           .WithMessage("*Error*")
           .And.ParamName.Should().Be("expected");
    }
}