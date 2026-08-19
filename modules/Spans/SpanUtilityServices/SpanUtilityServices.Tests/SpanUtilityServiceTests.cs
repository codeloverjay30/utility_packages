using System;
using System.Buffers;
using Xunit;
using FluentAssertions;

namespace SpanUtilityServices.Tests;

public class SpanUtilityServiceTests
{
    private readonly ISpanUtilityService _sut;

    public SpanUtilityServiceTests()
    {
        _sut = new SpanUtilityService();
    }

    [Theory]
    [InlineData(typeof(Span<int>), true)]
    [InlineData(typeof(ReadOnlySpan<byte>), true)]
    [InlineData(typeof(Memory<int>), false)]
    public void IsContinuousSpan_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsContinuousSpan(type);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(Memory<char>), true)]
    [InlineData(typeof(ReadOnlyMemory<double>), true)]
    [InlineData(typeof(Span<char>), false)]
    public void IsMemoryBlock_ShouldEvaluateCorrectly(Type type, bool expected)
    {
        // Act
        bool result = _sut.IsMemoryBlock(type);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsBufferControlInterface_WithCustomWriterImplementingInterface_ShouldReturnTrue()
    {
        // Arrange
        Type mockWriter = typeof(CustomBufferWriterMock<byte>);

        // Act
        bool result = _sut.IsBufferControlInterface(mockWriter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ServiceMethods_WithNullTypeArgument_ShouldThrowArgumentNullExceptionWithPreciseMessage()
    {
        // Arrange
        Type? invalidInput = null;

        // Act
        Action act = () => _sut.IsContinuousSpan(invalidInput!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .Where(p=>p.ParamName == "type")
           .And.Message.Should().Contain("The structural type snapshot evaluated cannot be null reference.");
    }

    // High-performance test double infrastructure mock
    private class CustomBufferWriterMock<T> : IBufferWriter<T>
    {
        public void Advance(int count) => throw new NotImplementedException();
        public Memory<T> GetMemory(int sizeHint = 0) => throw new NotImplementedException();
        public Span<T> GetSpan(int sizeHint = 0) => throw new NotImplementedException();
    }
}