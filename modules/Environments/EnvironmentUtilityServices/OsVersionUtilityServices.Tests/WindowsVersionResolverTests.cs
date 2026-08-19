using FluentAssertions;
using Moq;
using RegistryUtilityServices;

namespace OsVersionUtilityServices.Tests;

public class WindowsVersionResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnCorrectVersion_WhenRegistryIsValid()
    {
        var registry = new Mock<IRegistryService>();
        registry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns("22631");
        // Arrange
        var resolver = new WindowsVersionResolver(registry.Object);

        // Act
        var version = resolver.Resolve("Microsoft Windows 10.0.22631");

        // Assert
        version.Should().NotBeNull();
        version.Major.Should().BeGreaterOrEqualTo(6); // Windows NT kernel base
    }

    [Fact]
    public void Resolve_ShouldReturnFallbackVersion_WhenRegistryReturnsNull()
    {
        // Arrange
        var mockRegistry = new Mock<IRegistryService>();
        mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns((string)null);

        var resolver = new WindowsVersionResolver(mockRegistry.Object);

        // Act
        var result = resolver.Resolve("Windows");

        // Assert
        result.Should().Be(Environment.OSVersion.Version);
    }


    [Fact]
    public void Resolve_ShouldThrowException_WhenRegistryAccessThrows()
    {
        // Arrange
        var mockRegistry = new Mock<IRegistryService>();
        mockRegistry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new System.Security.SecurityException("Access Denied"));

        var resolver = new WindowsVersionResolver(mockRegistry.Object);

        // Act
        Action act = () => resolver.Resolve("Windows");

        // Assert
        act.Should().Throw<System.Security.SecurityException>()
           .WithMessage("Access Denied");
    }

}
