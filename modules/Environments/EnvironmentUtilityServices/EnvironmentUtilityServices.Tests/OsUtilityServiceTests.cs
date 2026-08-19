using System;
using System.IO.Abstractions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnvironmentUtilityServices.Tests;

public class OsUtilityServiceTests
{
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<IEnvironmentService> _environmentServiceMock;

    public OsUtilityServiceTests()
    {
        // 防禦設定：明確將 Mock 設為嚴格模式或預設值防禦，防止平行時空未設定的導覽屬性噴錯
        _fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
        _environmentServiceMock = new Mock<IEnvironmentService>(MockBehavior.Strict);
    }

    private OsUtilityService CreateSut()
    {
        return new OsUtilityService(_fileSystemMock.Object, _environmentServiceMock.Object);
    }

    [Fact]
    public void Constructor_WhenFileSystemIsNull_ShouldThrowArgumentNullExceptionWithDetails()
    {
        // Act
        Action act = () => _ = new OsUtilityService(null!, _environmentServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("fileSystem");
    }

    [Fact]
    public void Constructor_WhenEnvironmentServiceIsNull_ShouldThrowArgumentNullExceptionWithDetails()
    {
        // Act
        Action act = () => _ = new OsUtilityService(_fileSystemMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .Which.ParamName.Should().Be("environmentService");
    }

    [Fact]
    public void GetComparison_WhenIsWindowsIsTrue_ShouldReturnOrdinalIgnoreCase()
    {
        // Arrange
        _environmentServiceMock.Setup(x => x.IsWindows()).Returns(true);
        var sut = CreateSut();

        // Act
        var result = sut.GetComparison();

        // Assert
        result.Should().Be(StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetComparison_WhenIsWindowsIsFalse_ShouldReturnOrdinal()
    {
        // Arrange
        _environmentServiceMock.Setup(x => x.IsWindows()).Returns(false);
        var sut = CreateSut();

        // Act
        var result = sut.GetComparison();

        // Assert
        result.Should().Be(StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizePath_WhenPathIsInvalid_ShouldThrowArgumentExceptionWithCorrectMessage()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Action act = () => sut.NormalizePath("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Path cannot be null or empty.*")
           .Which.ParamName.Should().Be("path");
    }

    [Fact]
    public void NormalizePath_WhenPathIsValid_ShouldReturnFullPathFromMockedFileSystem()
    {
        // Arrange
        var inputPath = "relative/path/file.txt";
        var expectedFullPath = @"C:\src\relative\path\file.txt";

        // 副作用防禦：明確 Mock 底層導覽屬性 Path，嚴防 NotSupportedException
        var pathMock = new Mock<IPath>();
        pathMock.Setup(p => p.GetFullPath(inputPath)).Returns(expectedFullPath);
        _fileSystemMock.Setup(fs => fs.Path).Returns(pathMock.Object);

        var sut = CreateSut();

        // Act
        var result = sut.NormalizePath(inputPath);

        // Assert
        result.Should().Be(expectedFullPath);
        _fileSystemMock.Verify(fs => fs.Path, Times.Once);
    }
}