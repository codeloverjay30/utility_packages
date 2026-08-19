using System;
using FluentAssertions;
using Xunit;
using FileExplorerUtilityServices;

namespace FileExplorerUtilityServices.Tests;

/// <summary>
/// Contains unit tests for the <see cref="BitLockerShellRefresher"/> class to ensure robust defensive programming.
/// </summary>
public class BitLockerShellRefresherTests
{
    private readonly BitLockerShellRefresher _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitLockerShellRefresherTests"/> class.
    /// </summary>
    public BitLockerShellRefresherTests()
    {
        // SUT (System Under Test)
        _sut = new BitLockerShellRefresher();
    }

    /// <summary>
    /// Verifies that <see cref="BitLockerShellRefresher.NotifyToRefresh"/> executes without throwing any exceptions
    /// when provided with a valid win32 drive format representation using ReadOnlySpan.
    /// </summary>
    [Theory]
    [InlineData("C:")]
    [InlineData("D:\\")]
    [InlineData("Z:NewVolume")]
    public void NotifyToRefresh_ValidDriveFormat_ShouldExecuteSuccessfullyWithoutException(string drivePath)
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<char> driveSpan = drivePath.AsSpan();
            _sut.NotifyToRefresh(driveSpan);
        };
        // Assert
        // 使用 FluentAssertions 確保即使內部呼叫 Windows 核心 API，在此有效邊界內亦不會引發非預期潰散
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="BitLockerShellRefresher.NotifyToRefresh"/> gracefully bypasses the notification
    /// and does not crash or throw exceptions when given an invalid drive path format or an empty span.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("C")]
    [InlineData(" :")]
    [InlineData("\\\\Server\\Share")]
    public void NotifyToRefresh_InvalidDriveFormatOrEmpty_ShouldBypassGracefullyWithoutException(string invalidDrivePath)
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<char> driveSpan = invalidDrivePath.AsSpan();
            _sut.NotifyToRefresh(driveSpan);
        };

        // Assert
        // 防禦性設計核心：面對不合規輸入，應優雅攔截或無視，絕不可對呼叫端拋出非預期的 Exception
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the internal validation or execution pipeline handles highly volatile, 
    /// memory-aligned empty spans securely without memory corruption or AccessViolationException.
    /// </summary>
    [Fact]
    public void NotifyToRefresh_EmptySpan_ShouldBeSecureAndNotThrow()
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<char> emptySpan = ReadOnlySpan<char>.Empty;
            _sut.NotifyToRefresh(emptySpan);
        };

        // Assert
        act.Should().NotThrow();
    }
}