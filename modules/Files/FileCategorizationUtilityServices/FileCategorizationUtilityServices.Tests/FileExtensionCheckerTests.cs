using System;
using System.IO.Abstractions.TestingHelpers;
using FileCategorizationUtilityServices;
using FluentAssertions;
using Xunit;

namespace FileCategorizationUtilityServices.Tests;

public class FileExtensionCheckerTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly FileExtensionChecker _checker;

    public FileExtensionCheckerTests()
    {
        _fileSystem = new MockFileSystem();
        _checker = new FileExtensionChecker(_fileSystem);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileSystemIsNull()
    {
        // Act
        Action act = () => new FileExtensionChecker(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fileSystem");
    }

    [Fact]
    public void IsConfiguration_ShouldReturnTrue_WhenFileIsJson()
    {
        // Arrange
        var filePath = @"C:\appsettings.json";
        _fileSystem.AddFile(filePath, new MockFileData("{}"));

        // Act
        var result = _checker.IsConfiguration(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("test.cs", true)]
    [InlineData("test.g.cs", true)]
    [InlineData("test.vb", true)]
    [InlineData("test.txt", false)]
    public void IsProgrammingLanguage_ShouldIdentifyCorrectExtensions(string fileName, bool expected)
    {
        // Arrange
        var filePath = @$"C:\{fileName}";
        _fileSystem.AddFile(filePath, new MockFileData("content"));

        // Act
        var result = _checker.IsProgrammingLanguage(filePath);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NeedsToBeReplaced_ShouldReturnTrue_ForSupportedFiles()
    {
        // Arrange
        var projectFile = @"C:\Project.csproj";
        _fileSystem.AddFile(projectFile, new MockFileData("<Project />"));

        // Act
        var result = _checker.NeedsToBeReplaced(projectFile);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsConfiguration_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        // Act
        var result = _checker.IsConfiguration(@"C:\nonexistent.json");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsConfiguration_ShouldThrowArgumentException_WhenFilePathIsEmpty()
    {
        // Act
        Action act = () => _checker.IsConfiguration(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
