using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;

namespace EnvironmentUtilityServices.Tests;

public class EnvironmentServiceTestsExtensions
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsUncPath_WhenPathIsNullOrEmptyOrWhitespace_ShouldReturnFalse(string? invalidPath)
    {
        // Arrange
        var sut = new EnvironmentService(p => p == OSPlatform.Windows);

        // Act
        var result = sut.IsUncPath(invalidPath!);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(@"\\?\UNC\server\share\file.txt")]
    [InlineData(@"\\?\unc\myserver\myfiles")]
    public void IsUncPath_WhenWin32DeviceNamespaceUnc_ShouldReturnTrue(string uncPath)
    {
        // Arrange
        var sut = new EnvironmentService(p => p == OSPlatform.Windows);

        // Act
        var result = sut.IsUncPath(uncPath);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(@"\\server\share\file.txt")]
    [InlineData(@"//server/share/file.txt")]
    public void IsUncPath_WhenStandardNetworkShare_ShouldReturnTrue(string uncPath)
    {
        // Arrange
        var sut = new EnvironmentService(p => p == OSPlatform.Windows);

        // Act
        var result = sut.IsUncPath(uncPath);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(@"C:\LocalFolder\file.txt")]
    [InlineData(@"/usr/bin/local")]
    [InlineData(@"relative/path/to/file")]
    public void IsUncPath_WhenLocalOrRelativePath_ShouldReturnFalse(string localPath)
    {
        // Arrange
        var sut = new EnvironmentService(p => p == OSPlatform.Windows);

        // Act
        var result = sut.IsUncPath(localPath);

        // Assert
        result.Should().BeFalse();
    }
}