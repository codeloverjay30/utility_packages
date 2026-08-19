using System;
using System.Management;
using Moq;
using FluentAssertions;
using Xunit;
using EnvironmentUtilityServices;
using FileExplorerUtilityServices;

namespace FileExplorerUtilityServices.Tests;

/// <summary>
/// Contains robust unit tests for <see cref="BitLockerStorageEventListener"/> using FluentAssertions.
/// </summary>
public class BitLockerStorageEventListenerTests
{
    private readonly Mock<IBitLockerShellRefresher> _mockShellRefresher;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;

    public BitLockerStorageEventListenerTests()
    {
        _mockShellRefresher = new Mock<IBitLockerShellRefresher>(MockBehavior.Strict);
        _mockEnvironmentService = new Mock<IEnvironmentService>(MockBehavior.Strict);
    }

    [Fact]
    public void Constructor_WhenShellRefresherIsNull_ShouldThrowArgumentNullExceptionWithCorrectMessage()
    {
        // Arrange
        _mockEnvironmentService.Setup(x => x.IsWindows()).Returns(true);

        // Act
        Action act = () => new BitLockerStorageEventListener(null!, _mockEnvironmentService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .And.ParamName.Should().Be("shellRefresher");
    }

    [Fact]
    public void Constructor_WhenEnvironmentServiceIsNull_ShouldThrowArgumentNullExceptionWithCorrectMessage()
    {
        // Act
        Action act = () => new BitLockerStorageEventListener(_mockShellRefresher.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .And.ParamName.Should().Be("environmentService");
    }

    [Fact]
    public void StartListening_WhenPlatformIsNotWindows_ShouldThrowPlatformNotSupportedException()
    {
        // Arrange
        _mockEnvironmentService.Setup(x => x.IsWindows()).Returns(false);
        var sut = new BitLockerStorageEventListener(_mockShellRefresher.Object, _mockEnvironmentService.Object);

        // Act
        Action act = () => sut.StartListening();

        // Assert
        act.Should().Throw<PlatformNotSupportedException>()
           .WithMessage("This API is only available for Windows");
    }

    [Fact]
    public void StopListening_WhenWatcherNotStarted_ShouldExecuteSuccessfullyWithoutException()
    {
        // Arrange
        var sut = new BitLockerStorageEventListener(_mockShellRefresher.Object, _mockEnvironmentService.Object);

        // Act
        Action act = () => sut.StopListening();

        // Assert
        act.Should().NotThrow();
    }
}