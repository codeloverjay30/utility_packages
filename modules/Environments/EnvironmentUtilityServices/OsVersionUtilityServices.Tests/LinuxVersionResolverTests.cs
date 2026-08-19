using System.IO.Abstractions;
using FluentAssertions;
using Moq;

namespace OsVersionUtilityServices.Tests;

public class LinuxVersionResolverTests
{

    public void Resolve_WhenOsReleaseMissing_ShouldReturnEnvironmentVersion()
    {
        // Arrange
        var mockFs = new Mock<IFileSystem>(MockBehavior.Strict);
        mockFs.Setup(fs => fs.File.Exists("/etc/os-release")).Returns(false);
        var resolver = new LinuxVersionResolver(mockFs.Object);

        // Act
        var action = () => resolver.Resolve("Linux");

        // Assert
        action.Should().NotThrow();
        resolver.Resolve("Linux").Should().Be(Environment.OSVersion.Version);
    }
}
