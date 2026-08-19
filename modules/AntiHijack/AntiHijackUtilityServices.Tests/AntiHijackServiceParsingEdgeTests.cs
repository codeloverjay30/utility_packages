using System;
using AntiHijackUtilityServices;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// Explores extreme data-fuzzing and formatting layout edge cases for the non-allocating parser.
/// </summary>
public class AntiHijackServiceParsingEdgeTests
{
    /// <summary>
    /// Confirms that if the inner payload successfully contains structurally valid boundaries but the inner 
    /// value is malicious or unparsable as an integer epoch ticker, the runtime cleanly aborts request context.
    /// </summary>
    [Fact]
    public void ValidateRequest_WhenTimestampIsMalformedAlphaCharacters_ShouldReturnFalse()
    {
        // Arrange
        var mockTime = NSubstitute.Substitute.For<TimeUtilityServices.ITimeService>();
        var fakeCrypto = new FakeEncryptoService();
        var stubAuth = new StubAuthenticationService();

        // Formulate corrupted inner layout containing alphabetical ticks
        fakeCrypto.SetupDecryptionResult("Timestamp=InvalidAlphaTicks;UserId=999;");
        
        var sut = new AntiHijackService(mockTime, fakeCrypto, stubAuth);
        ReadOnlySpan<char> dummyPayload = "PayloadShell".AsSpan();
        byte[] tight32ByteKey = new byte[32];

        // Act
        bool result = sut.ValidateRequest(dummyPayload, tight32ByteKey);

        // Assert
        result.Should().BeFalse("Because a non-numeric timestamp structure represents an invalid telemetry layout.");
    }

    /// <summary>
    /// Validates that when the "Timestamp=" token sits precisely at the trailing edge of the decrypted payload string 
    /// without any subsequent trailing characters, the non-allocating slice mechanics do not trigger critical indexing exceptions.
    /// </summary>
    [Fact]
    public void ValidateRequest_WhenTimestampIsAtTheAbsoluteEndOfTheString_ShouldHandleGracefullyAndReturnFalse()
    {
        // Arrange
        var mockTime = NSubstitute.Substitute.For<TimeUtilityServices.ITimeService>();
        var fakeCrypto = new FakeEncryptoService();
        var stubAuth = new StubAuthenticationService();

        // Deep defense test: Layout terminates exactly at the parameter key token edge
        fakeCrypto.SetupDecryptionResult("UserId=123;Timestamp=");
        
        var sut = new AntiHijackService(mockTime, fakeCrypto, stubAuth);
        byte[] tight32ByteKey = new byte[32];

        // Act
        bool result = false;
        Action act = () => {
            ReadOnlySpan<char> dummyPayload = "EdgePayloadShell".AsSpan();
            result = sut.ValidateRequest(dummyPayload, tight32ByteKey); 
        };

        // Assert
        act.Should().NotThrow("Because the internal memory slice calculation must pre-emptively guard against range out of bound anomalies.");
        result.Should().BeFalse("Because an empty trailing timestamp represents an unparsable epoch layout.");
    }

    /// <summary>
    /// Verifies system immunity against parameter pollution style exploits where a malicious payload contains 
    /// multiple "Timestamp=" tokens to bypass standard first-match IndexOf tracking structures.
    /// </summary>
    [Fact]
    public void ValidateRequest_WhenPayloadContainsDuplicateTimestampTokens_ShouldEvaluateFirstTokenOrRejectSafely()
    {
        // Arrange
        var mockTime = NSubstitute.Substitute.For<TimeUtilityServices.ITimeService>();
        var fakeCrypto = new FakeEncryptoService();
        var stubAuth = new StubAuthenticationService();

        // Malicious layout injection: The first index points to an expired/invalid value, the second points to a fake current one
        fakeCrypto.SetupDecryptionResult("Timestamp=9999;Timestamp=1000000;UserId=456;");
        mockTime.GetCurrentStopWatch().Returns(1000000L); // Current synchronized server clock ticks

        var sut = new AntiHijackService(mockTime, fakeCrypto, stubAuth);
        ReadOnlySpan<char> dummyPayload = "FuzzedPayload".AsSpan();
        byte[] tight32ByteKey = new byte[32];

        // Act
        bool result = sut.ValidateRequest(dummyPayload, tight32ByteKey);

        // Assert
        result.Should().BeFalse("Because duplicate parameter blocks indicate structural tampering or a malicious replay obfuscation attempt.");
    }

    /// <summary>
    /// Assures that when a numeric value is present but completely exceeds the memory threshold layout limits of a 64-bit signed integer, 
    /// the system gracefully shortcuts without cascading into primitive system panics.
    /// </summary>
    [Fact]
    public void ValidateRequest_WhenTimestampNumericalValueOverflowsInt64_ShouldReturnFalseCleanly()
    {
        // Arrange
        var mockTime = NSubstitute.Substitute.For<TimeUtilityServices.ITimeService>();
        var fakeCrypto = new FakeEncryptoService();
        var stubAuth = new StubAuthenticationService();

        // 999999999999999999999999999999 exceeds Int64.MaxValue radically
        fakeCrypto.SetupDecryptionResult("Timestamp=999999999999999999999999999999;UserId=789;");
        
        var sut = new AntiHijackService(mockTime, fakeCrypto, stubAuth);
        ReadOnlySpan<char> dummyPayload = "OverflowPayload".AsSpan();
        byte[] tight32ByteKey = new byte[32];

        // Act
        bool result = sut.ValidateRequest(dummyPayload, tight32ByteKey);

        // Assert
        result.Should().BeFalse("Because a data value that overflows Int64 limits fails structural long.TryParse validation boundaries.");
    }
}