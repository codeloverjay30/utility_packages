using System;
using AntiHijackUtilityServices;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// Evaluates fault-isolation boundaries within the <see cref="AntiHijackService"/> verification pipeline.
/// </summary>
public class AntiHijackServiceFaultIsolationTests
{
    /// <summary>
    /// Validates that any unexpected runtime error thrown from downstream crypto dependencies is gracefully contained, 
    /// forcing the validation context to reject the request safely without raising fatal application crashes.
    /// </summary>
    [Fact]
    public void ValidateRequest_WhenCryptoServiceThrowsUnexpectedException_ShouldCatchGracefullyAndReturnFalse()
    {
        // Arrange
        var mockTime = Substitute.For<TimeUtilityServices.ITimeService>();
        var mockAuth = Substitute.For<AuthenticationUtilityServices.IAuthenticationService>();

        // Inject a manual stub designed to bypass dynamic proxy limitations on ref structs
        var faultInjectableCrypto = new FaultInjectableEncryptoService();
        faultInjectableCrypto.SetupExceptionToThrow(new OutOfMemoryException("Simulated catastrophic memory corruption."));

        var sut = new AntiHijackService(mockTime, faultInjectableCrypto, mockAuth);
        byte[] tight32ByteKey = new byte[32];

        // Act
        bool result = false;
        Action act = () =>
        {
            ReadOnlySpan<char> dummyPayload = "ValidPayloadShell".AsSpan();
            result = sut.ValidateRequest(dummyPayload, tight32ByteKey);
        };

        // Assert
        act.Should().NotThrow("Because the perimeter defender must implement strict fault isolation loops.");
        result.Should().BeFalse("Because an unhandled downstream failure must translate into an unverified structural state.");
    }
}