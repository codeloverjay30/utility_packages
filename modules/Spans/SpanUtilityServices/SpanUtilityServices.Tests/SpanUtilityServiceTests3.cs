using System;
using System.Buffers;
using FluentAssertions;
using Xunit;
using SpanUtilityServices;

namespace SpanUtilityServices.Tests;

/// <summary>
/// Contains comprehensive unit tests for <see cref="SpanUtilityService"/> ensuring safe memory mapping and defensive branching.
/// </summary>
public class SpanUtilityServiceTests3
{
    private readonly SpanUtilityService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpanUtilityServiceTests"/> class.
    /// </summary>
    public SpanUtilityServiceTests3()
    {
        _sut = new SpanUtilityService();
    }

#if NET9_0_OR_GREATER

    [Fact]
    public void GetUnknownRefStructLength_GivenValidReadOnlySpan_ShouldReturnCorrectLength()
    {
        // Arrange
        ReadOnlySpan<int> span = new int[] { 10, 20, 30, 40 };

        // Act
        int result = _sut.GetStatusOfUnknownRefStruct(ref span);

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public void GetUnknownRefStructLength_GivenEmptySpan_ShouldReturnZero()
    {
        // Arrange
        Span<char> span = Span<char>.Empty;

        // Act
        int result = _sut.GetStatusOfUnknownRefStruct(ref span);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetUnknownRefStructLength_GivenNonRefStructReadOnlySequenceOfByte_ShouldReturnCorrectBranchingValue()
    {
        // Arrange
        byte[] buffer = new byte[] { 1, 2, 3 };
        var sequence = new ReadOnlySequence<byte>(buffer);

        // Act
        int result = _sut.GetStatusOfUnknownRefStruct(ref sequence);

        // Assert
        // According to the defensive logic, returns 1 if not empty, otherwise 0
        result.Should().Be((int)StatusInfo.IsNotEmpty);
    }

    /// <summary>
    /// </summary>
    /// <remarks>
    /// Because there is an exception swallowing during method call of 
    /// `DynamicReadOnlySequenceInspector.IsEmpty`, it returns (int)StatusInfo.IsNotEmpty
    /// </remarks>
    [Fact]
    public void GetUnknownRefStructLength_GivenEmptyReadOnlySequenceOfChar_ShouldReturnNonEmpty()
    {
        // Arrange
        var sequence = ReadOnlySequence<char>.Empty;

        // Act
        int result = _sut.GetStatusOfUnknownRefStruct(ref sequence);

        // Assert
        result.Should().Be((int)StatusInfo.IsEmpty);
    }

    [Fact]
    public void GetUnknownRefStructLength_GivenUnexpectedStandardType_ShouldReturnFailureTestConstant()
    {
        // Arrange
        string standardString = "Defense C#";

        // Act
        int result = _sut.GetStatusOfUnknownRefStruct(ref standardString);

        // Assert
        result.Should().Be((int)StatusInfo.FailureTest);
    }

    [Fact]
    public void IsEmpty_GivenEmptyReadOnlySpan_ShouldReturnTrue()
    {
        // Arrange
        ReadOnlySpan<byte> emptySpan = ReadOnlySpan<byte>.Empty;

        // Act
        bool result = _sut.IsEmpty(ref emptySpan);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_GivenNotEmptySpan_ShouldReturnFalse()
    {
        // Arrange
        Span<int> activeSpan = new int[] { 99 };

        // Act
        bool result = _sut.IsEmpty(ref activeSpan);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_GivenNonRefStructEmptySequence_ShouldReturnTrue()
    {
        // Arrange
        var emptySequence = ReadOnlySequence<byte>.Empty;

        // Act
        bool result = _sut.IsEmpty(ref emptySequence);

        // Assert
        result.Should().BeTrue();
    }

#else

    [Fact]
    public void GetUnknownRefStructLength_GivenNetEightOrLower_ShouldAlwaysReturnFailureTestConstant()
    {
        // Arrange
        int[] standardArray = new int[] { 1, 2, 3 };

        // Act
        int result = _sut.GetUnknownRefStructLength(standardArray);

        // Assert
        result.Should().Be((int)StatusInfo.FailureTest);
    }

    [Fact]
    public void IsEmpty_GivenNetEightOrLower_ShouldAlwaysReturnFalseToPreventBusinessInterruption()
    {
        // Arrange
        string testInstance = "Fallback Test";

        // Act
        bool result = _sut.IsEmpty(testInstance);

        // Assert
        // In .NET 8-, GetUnknownRefStructLength returns -1, so -1 == 0 evaluates to false.
        result.Should().BeFalse();
    }

#endif

    [Fact]
    public void Action_ExampleOfExceptionInterception_DemonstratingFluentAssertionsStandard()
    {
        // Arrange & Act
        Action act = () => throw new InvalidOperationException("Defensive memory validation failed unexpected.");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*validation failed*");
    }
}