using System;
using AuthenticationUtilityServices;
using FluentAssertions;
using Xunit;

namespace AuthenticationUtilityServices.Tests;

public class AuthenticationServiceTests
{
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _sut = new AuthenticationService();
    }

    [Fact]
    public void VerifySignature_WhenPayloadIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        ReadOnlySpan<char> rawPayload = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> expectedSignature = "valid_signature".AsSpan();

        // Act
        bool result = _sut.VerifySignature(rawPayload, expectedSignature);

        // Assert - 遵循鐵律：一律使用 FluentAssertions
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifySignature_WhenSignaturesMatch_ShouldReturnTrue()
    {
        // Arrange
        string secret = "secure_payload_data";
        string signature = "secure_payload_data";

        // Act
        bool result = _sut.VerifySignature(secret.AsSpan(), signature.AsSpan());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifySignature_WhenSignaturesDoNotMatch_ShouldReturnFalse()
    {
        // Arrange
        string secret = "secure_payload_data";
        string maliciousSignature = "attack_signature_data";

        // Act
        bool result = _sut.VerifySignature(secret.AsSpan(), maliciousSignature.AsSpan());

        // Assert
        result.Should().BeFalse();
    }
}