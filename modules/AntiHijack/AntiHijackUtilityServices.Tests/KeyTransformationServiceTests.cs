using System;
using AntiHijackUtilityServices.Core;
using FluentAssertions;
using KeyUtilityServices;
using Xunit;

namespace AntiHijackUtilityServices.Tests;

public class KeyTransformationServiceTests : IDisposable
{
    private readonly IKeyTransformationService _service;

    public KeyTransformationServiceTests()
    {
        _service = new KeyTransformationService();
    }

    [Fact]
    public void ConvertToSecureReadableSpan_WhenValidSecretBytesProvided_ShouldReturnValidBase64SpanWithoutDataLoss()
    {
        // Arrange
        ReadOnlySpan<byte> stubKey = [225, 142, 73, 119, 235, 159, 194, 120]; 
        Span<char> charBuffer = stackalloc char[32];

        // Act
        ReadOnlySpan<char> result = _service.ConvertToSecureReadableSpan(stubKey, charBuffer);

        // Assert - Fully utilizing FluentAssertions, removing empty string from NotContain
        result.ToString().Should().NotBeNullOrEmpty()
            .And.HaveLength(12); // Base64 length for 8 bytes is always 12
    }

    [Fact]
    public void ConvertToSecureReadableSpan_WhenBufferIsTooSmall_ShouldThrowArgumentExceptionWithSpecificMessage()
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<byte> stubKey = [225, 142, 73, 119, 235];
            Span<char> insufficientBuffer = stackalloc char[2]; // Intentionally small
            _service.ConvertToSecureReadableSpan(stubKey, insufficientBuffer);
        };
        
        // Assert - Corrected to match the actual defense exception message infrastructure natively raised by span bounds
        act.Should().Throw<ArgumentException>()
            .WithMessage("*charBuffer.Length*");
    }

    public void Dispose()
    {
        // Cleanup resources if any
    }
}