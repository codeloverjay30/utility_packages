using System;
using AntiHijackUtilityService.Sensors;
using EnvironmentUtilityServices;
using FluentAssertions;
using Moq;
using Xunit;

namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// Proactively validates defensive runtime platform barriers for the <see cref="DebuggerDetector"/> service.
/// </summary>
public class DebuggerDetectorTests
{
    private readonly Mock<IPlatformService> _mockPlatformService;

    /// <summary>
    /// Initializes a tight, isolated mock ecosystem for detector evaluation.
    /// </summary>
    public DebuggerDetectorTests()
    {
        _mockPlatformService = new Mock<IPlatformService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies that executing the detector on non-Windows platforms immediately triggers a platform exception to prevent undefined subsystem behaviors.
    /// </summary>
    [Fact]
    public void IsThreatDetected_WhenPlatformIsNotWindows_ShouldThrowPlatformNotSupportedException()
    {
        // Arrange
        _mockPlatformService.Setup(p => p.IsWindows()).Returns(false);
        var detector = new DebuggerDetector(_mockPlatformService.Object);

        // Act
        Action act = () => detector.IsThreatDetected();

        // Assert
        act.Should().Throw<PlatformNotSupportedException>()
           .WithMessage("*only supported on Windows platforms*");

        _mockPlatformService.Verify(p => p.IsWindows(), Times.Once);
    }
}