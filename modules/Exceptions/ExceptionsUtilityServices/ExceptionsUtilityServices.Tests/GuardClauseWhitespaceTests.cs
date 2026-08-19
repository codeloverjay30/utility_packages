using System;
using System.Buffers;
using Xunit;
using FluentAssertions;
using ExceptionsUtilityServices;

namespace ExceptionsUtilityServices.Tests;

/// <summary>
/// Defensive specification tests for string and character-based whitespace guard validation.
/// </summary>
public class GuardClauseWhitespaceTests
{
    #region ----------- String Overload Tests -----------

    [Fact]
    public void ThrowIfNullOrWhitespace_WithNullString_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? target = null;

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(target, "nullStringParam");

        // Assert: FluentAssertions integration testing pattern
        act.Should().Throw<ArgumentNullException>()
           .Where(e => e.ParamName == "nullStringParam")
           .WithMessage("*cannot be null*");
    }

    [Fact]
    public void ThrowIfNullOrWhitespace_WithOnlyWhitespacesString_ShouldThrowArgumentEmptyOrWhitespaceException()
    {
        // Arrange
        string target = "   \r\n \t  ";

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(target, "whitespaceParam");

        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(e => e.ParamName == "whitespaceParam")
           .WithMessage("*cannot be empty or consist only of white-space characters*");
    }

    [Fact]
    public void ThrowIfNullOrWhitespace_WithValidString_ShouldPassWithoutAnyExceptions()
    {
        // Arrange
        string target = "  Architect Core  ";

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(target, "validParam");

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region ----------- ReadOnlySpan Overload Tests -----------

    [Fact]
    public void ThrowIfNullOrWhitespace_WithEmptySpan_ShouldThrowArgumentEmptyOrWhitespaceException()
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<char> emptySpan = ReadOnlySpan<char>.Empty;
            ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(emptySpan, "emptySpanParam");
        };

        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(e => e.ParamName == "emptySpanParam")
           .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void ThrowIfNullOrWhitespace_WithWhitespaceSpan_ShouldThrowException()
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<char> whitespaceSpan = "   ".AsSpan();
            ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(whitespaceSpan, "spacesSpanParam");
        };
        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(e => e.ParamName == "spacesSpanParam")
           .WithMessage("*cannot consist only of white-space characters*");
    }

    #endregion
}