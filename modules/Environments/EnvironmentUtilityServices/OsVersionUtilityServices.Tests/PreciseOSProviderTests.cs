using Moq;
using FluentAssertions;
using Xunit;
using System.Runtime.InteropServices;
using OsVersionUtilityServices;

namespace OsVersionUtilityServices.Tests;

public class PreciseOSProviderTests
{
    private readonly Mock<IOSVersionResolver> _mockResolver;
    private readonly List<IOSVersionResolver> _resolvers;
    private readonly PreciseOSProvider _sut;

    public PreciseOSProviderTests()
    {
        _mockResolver = new Mock<IOSVersionResolver>(MockBehavior.Strict);
        _resolvers = new List<IOSVersionResolver> { _mockResolver.Object };
        _sut = new PreciseOSProvider(_resolvers);
    }

    /// <summary>
    /// Verifies that GetPreciseVersion throws NotSupportedException when no matching resolver is found.
    /// </summary>
    [Fact]
    public void GetPreciseVersion_ShouldThrowNotSupportedException_WhenNoResolverFound()
    {
        // Arrange
        _mockResolver.Setup(r => r.CanHandle(It.IsAny<OSPlatform>())).Returns(false);

        // Act
        Action act = () => _sut.GetPreciseVersion();

        // Assert
        act.Should().Throw<NotSupportedException>()
           .WithMessage("No resolver found for current OS.");
    }

    /// <summary>
    /// Verifies that GetPreciseVersion returns the correct version when a valid resolver is provided.
    /// </summary>
    [Fact]
    public void GetPreciseVersion_ShouldReturnVersion_WhenResolverExists()
    {
        // Arrange
        var expectedVersion = new Version(10, 0);
        _mockResolver.Setup(r => r.CanHandle(It.IsAny<OSPlatform>())).Returns(true);
        _mockResolver.Setup(r => r.Resolve(It.IsAny<string>())).Returns(expectedVersion);

        // Act
        var result = _sut.GetPreciseVersion();

        // Assert
        result.Should().Be(expectedVersion);
    }
}