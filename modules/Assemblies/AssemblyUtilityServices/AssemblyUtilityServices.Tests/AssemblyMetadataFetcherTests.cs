using FluentAssertions;
using System.Reflection;
using Xunit;

namespace AssemblyUtilityServices.Tests;

public sealed class AssemblyMetadataFetcherTests
{
    [Fact]
    public void GetInformationalVersion_WhenAttributeExists_ShouldReturnExactValue()
    {
        // Arrange
        const string expected = "2.0.0-preview.1+abcdef";
        Assembly assembly = DynamicAssemblyFactory.Create(expected);

        // Act
        string result = assembly.GetInformationalVersion();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetInformationalVersion_WhenAttributeIsMissing_ShouldThrow()
    {
        // Arrange
        Assembly assembly = DynamicAssemblyFactory.Create();

        // Act
        Action act = () => assembly.GetInformationalVersion();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*does not declare an informational version*");
    }

    [Fact]
    public void TryGetInformationalVersion_WhenAttributeIsMissing_ShouldReturnFalseAndNull()
    {
        // Arrange
        Assembly assembly = DynamicAssemblyFactory.Create();

        // Act
        bool result = assembly.TryGetInformationalVersion(out string? version);

        // Assert
        result.Should().BeFalse();
        version.Should().BeNull();
    }

    [Fact]
    public void GetAssemblyVersion_WhenVersionExists_ShouldReturnIdentityVersion()
    {
        // Arrange
        var expected = new Version(3, 4, 5, 6);
        Assembly assembly = DynamicAssemblyFactory.Create(
            informationalVersion: "99.0.0",
            assemblyVersion: expected);

        // Act
        Version result = assembly.GetAssemblyVersion();

        // Assert
        result.Should().Be(expected);
    }
}
