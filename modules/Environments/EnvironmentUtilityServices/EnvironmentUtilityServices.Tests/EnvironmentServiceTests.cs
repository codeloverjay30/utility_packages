using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;

namespace EnvironmentUtilityServices.Tests;

public class EnvironmentServiceTests
{
    [Fact]
    public void Constructor_WhenOsCheckIsNull_ShouldThrowArgumentNullExceptionWithCorrectMessage()
    {
        // Arrange & Act
        Action act = () => _ = new EnvironmentService(null!);

        // Assert (鐵律：必須使用 FluentAssertions 攔截並驗證真實 Exception Message)
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Value cannot be null.*")
           .Which.ParamName.Should().Be("osCheck");
    }

    [Fact]
    public void IsWindows_WhenPlatformIsWindows_ShouldReturnTrueAndOthersFalse()
    {
        // Arrange
        Func<OSPlatform, bool> mockOsCheck = platform => platform == OSPlatform.Windows;
        var sut = new EnvironmentService(mockOsCheck);

        // Act & Assert (鐵律：全數採用 FluentAssertions 斷言)
        sut.IsWindows().Should().BeTrue();
        sut.IsLinux().Should().BeFalse();
        sut.IsMacOS().Should().BeFalse();
    }

    [Fact]
    public void IsLinux_WhenPlatformIsLinux_ShouldReturnTrueAndOthersFalse()
    {
        // Arrange
        Func<OSPlatform, bool> mockOsCheck = platform => platform == OSPlatform.Linux;
        var sut = new EnvironmentService(mockOsCheck);

        // Act & Assert
        sut.IsLinux().Should().BeTrue();
        sut.IsWindows().Should().BeFalse();
        sut.IsMacOS().Should().BeFalse();
    }

    [Fact]
    public void IsMacOS_WhenPlatformIsMacOS_ShouldReturnTrueAndOthersFalse()
    {
        // Arrange
        Func<OSPlatform, bool> mockOsCheck = platform => platform == OSPlatform.OSX;
        var sut = new EnvironmentService(mockOsCheck);

        // Act & Assert
        sut.IsMacOS().Should().BeTrue();
        sut.IsWindows().Should().BeFalse();
        sut.IsLinux().Should().BeFalse();
    }
}