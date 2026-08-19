using System;
using System.Buffers;
using FluentAssertions;
using Xunit;

namespace SpanUtilityServices.Tests;

/// <summary>
/// Contains high-performance architectural unit tests for the <see cref="SpanUtilityService"/> verification logic.
/// </summary>
public class SpanUtilityServiceTests2
{
    private readonly ISpanUtilityService _sut;

    public SpanUtilityServiceTests2()
    {
        _sut = new SpanUtilityService();
    }

    [Fact]
    public void GuardAgainstNull_ShouldThrowArgumentNullException_WhenTypeIsNull()
    {
        // Arrange
        Action act = () => _sut.IsContinuousSpan(null!);

        // Act & Assert
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*The structural type snapshot evaluated cannot be null reference.*");
    }

    [Theory]
    [InlineData(typeof(Span<int>), true)]
    [InlineData(typeof(ReadOnlySpan<byte>), true)]
    [InlineData(typeof(Memory<int>), false)]
    [InlineData(typeof(int[]), false)]
    public void IsContinuousSpan_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsContinuousSpan(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(Memory<int>), true)]
    [InlineData(typeof(ReadOnlyMemory<byte>), true)]
    [InlineData(typeof(Span<int>), false)]
    [InlineData(typeof(StandardClass), false)]
    public void IsMemoryBlock_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsMemoryBlock(type);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsMultiDimensionalMemory_ShouldReturnFalse_ForStandardTypes()
    {
        // Act & Assert
        _sut.IsMultiDimensionalMemory(typeof(Memory<int>)).Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(FakeMemoryManager), true)]
    [InlineData(typeof(ReadOnlySequenceSegment<byte>), true)]
    [InlineData(typeof(BufferSegment), true)]
    [InlineData(typeof(StandardClass), false)]
    public void IsMemoryManagerOrSegment_ShouldIdentifyCorrectTypesAndTextMatches(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsMemoryManagerOrSegment(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(ReadOnlySequence<byte>), true)]
    [InlineData(typeof(ReadOnlySpan<byte>), false)]
    public void IsDiscontinuousSequence_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsDiscontinuousSequence(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(SequenceReader<byte>), true)]
    [InlineData(typeof(StandardClass), false)]
    public void IsSequenceReader_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsSequenceReader(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(IBufferWriter<byte>), true)]
    [InlineData(typeof(CustomBufferWriter), true)]
    [InlineData(typeof(IMemoryOwner<byte>), true)]
    [InlineData(typeof(StandardClass), false)]
    public void IsBufferControlInterface_ShouldScanHierarchyCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsBufferControlInterface(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(ArrayPool<byte>), true)]
    [InlineData(typeof(StandardClass), false)]
    public void IsPoolInfrastructure_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsPoolInfrastructure(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(Span<byte>), true)]
    [InlineData(typeof(ReadOnlySpan<char>), true)]
    [InlineData(typeof(string), false)]
    public void IsRefStruct_ShouldDetectByRefLikeAttribute(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsRefStruct(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(StringRentedBuffer), true)]
    [InlineData(typeof(ValueStringBuilder), true)]
    [InlineData(typeof(StandardClass), false)]
    public void IsHighPerformanceStringDefense_ShouldMatchCopyPastedTemplatesByName(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsHighPerformanceStringDefense(type);

        // Assert
        result.Should().Be(expected);
    }

    #if NET9_0_OR_GREATER

    [Fact]
    public void GetUnknownRefStructLength_ShouldExtractLength_FromSpanAndCustomRefStructViaPointerMapping()
    {
        // Arrange
        Span<byte> testSpan = new byte[42];
        var customRefStruct = new CustomRefStruct(99);
        Span<byte> emptySpan = Span<byte>.Empty;

        // Act
        int spanLength = _sut.GetStatusOfUnknownRefStruct(ref testSpan);
        int customLength = _sut.GetStatusOfUnknownRefStruct(ref customRefStruct);
        bool isSpanEmpty = _sut.IsEmpty(ref testSpan);
        bool isZeroSpanEmpty = _sut.IsEmpty(ref emptySpan);

        // Assert
        spanLength.Should().Be(42);
        customLength.Should().Be(99);
        isSpanEmpty.Should().BeFalse();
        isZeroSpanEmpty.Should().BeTrue();
    }

    #else

    [Fact]
    public void GetUnknownRefStructLength_ShouldReturnFailureTest_WhenTargetIsLessThanNet9()
    {
        // Arrange
        int[] testArray = new int[5];

        // Act
        int length = _sut.GetUnknownRefStructLength(ref testArray);
        bool isEmpty = _sut.IsEmpty(ref testArray);

        // Assert
        length.Should().Be((int)StatusInfo.FailureTest);
        isEmpty.Should().BeFalse(); // Because FAILURE_TEST (-1) == 0 is false
    }

    #endif
}