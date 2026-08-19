using System;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnvironmentUtilityServices.Tests;

public class PlatformServiceTests
{
    private readonly Mock<IEnvironmentService> _environmentServiceMock;
    private readonly Mock<IOsUtilityService> _osUtilityServiceMock;

    public PlatformServiceTests()
    {
        // 防禦設定：全面採用嚴格模式，任何未經 Setup 的非預期呼叫將立即噴出 Exception，嚴防隱蔽的平行時空副作用
        _environmentServiceMock = new Mock<IEnvironmentService>(MockBehavior.Strict);
        _osUtilityServiceMock = new Mock<IOsUtilityService>(MockBehavior.Strict);
    }

    private PlatformService CreateSut()
    {
        return new PlatformService(_environmentServiceMock.Object, _osUtilityServiceMock.Object);
    }

    [Fact]
    public void Constructor_WhenEnvironmentServiceIsNull_ShouldThrowArgumentNullExceptionWithCorrectMessage()
    {
        // Act
        Action act = () => _ = new PlatformService(null!, _osUtilityServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*Value cannot be null.*")
           .Which.ParamName.Should().Be("env");
    }

    [Fact]
    public void Constructor_WhenOsUtilityServiceIsNull_ShouldThrowArgumentNullExceptionWithCorrectMessage()
    {
        // Act
        Action act = () => _ = new PlatformService(_environmentServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*Value cannot be null.*")
           .Which.ParamName.Should().Be("util");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsWindows_WhenCalled_ShouldDelegateToEnvironmentService(bool expected)
    {
        // Arrange
        _environmentServiceMock.Setup(x => x.IsWindows()).Returns(expected);
        var sut = CreateSut();

        // Act
        var result = sut.IsWindows();

        // Assert
        result.Should().Be(expected);
        _environmentServiceMock.Verify(x => x.IsWindows(), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsLinux_WhenCalled_ShouldDelegateToEnvironmentService(bool expected)
    {
        // Arrange
        _environmentServiceMock.Setup(x => x.IsLinux()).Returns(expected);
        var sut = CreateSut();

        // Act
        var result = sut.IsLinux();

        // Assert
        result.Should().Be(expected);
        _environmentServiceMock.Verify(x => x.IsLinux(), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMacOS_WhenCalled_ShouldDelegateToEnvironmentService(bool expected)
    {
        // Arrange
        _environmentServiceMock.Setup(x => x.IsMacOS()).Returns(expected);
        var sut = CreateSut();

        // Act
        var result = sut.IsMacOS();

        // Assert
        result.Should().Be(expected);
        _environmentServiceMock.Verify(x => x.IsMacOS(), Times.Once);
    }

    [Theory]
    [InlineData(@"\\server\share", true)]
    [InlineData(@"C:\local", false)]
    public void IsUncPath_WhenCalled_ShouldDelegateToEnvironmentService(string testPath, bool expected)
    {
        // Arrange
        _environmentServiceMock.Setup(x => x.IsUncPath(testPath)).Returns(expected);
        var sut = CreateSut();

        // Act
        var result = sut.IsUncPath(testPath);

        // Assert
        result.Should().Be(expected);
        _environmentServiceMock.Verify(x => x.IsUncPath(testPath), Times.Once);
    }

    [Theory]
    [InlineData(StringComparison.OrdinalIgnoreCase)]
    [InlineData(StringComparison.Ordinal)]
    public void GetComparison_WhenCalled_ShouldDelegateToOsUtilityService(StringComparison expected)
    {
        // Arrange
        _osUtilityServiceMock.Setup(x => x.GetComparison()).Returns(expected);
        var sut = CreateSut();

        // Act
        var result = sut.GetComparison();

        // Assert
        result.Should().Be(expected);
        _osUtilityServiceMock.Verify(x => x.GetComparison(), Times.Once);
    }

    [Fact]
    public void NormalizePath_WhenCalled_ShouldDelegateToOsUtilityService()
    {
        // Arrange
        var inputPath = "some/raw/path";
        var expectedPath = @"C:\some\raw\path";
        _osUtilityServiceMock.Setup(x => x.NormalizePath(inputPath)).Returns(expectedPath);
        var sut = CreateSut();

        // Act
        var result = sut.NormalizePath(inputPath);

        // Assert
        result.Should().Be(expectedPath);
        _osUtilityServiceMock.Verify(x => x.NormalizePath(inputPath), Times.Once);
    }
}