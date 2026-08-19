using FluentAssertions;
using System.Reflection;
using Xunit;

namespace AssemblyUtilityServices.Tests;

public sealed class AssemblyVersionMatcherTests
{
    [Theory]
    [InlineData(2, 0, 0)]
    [InlineData(0, 9, 0)]
    public void Matchers_WhenInformationalVersionContainsZero_ShouldMatch(
        int major,
        int minor,
        int patch)
    {
        // Arrange
        Assembly assembly = DynamicAssemblyFactory.Create(
            $"{major}.{minor}.{patch}-preview.1+abcdef");

        // Act
        bool majorMatched = assembly.IsMajorVersionMatched(major);
        bool minorMatched = assembly.IsMinorVersionMatched(minor);
        bool patchMatched = assembly.IsPatchVersionMatched(patch);

        // Assert
        majorMatched.Should().BeTrue();
        minorMatched.Should().BeTrue();
        patchMatched.Should().BeTrue();
    }

    [Fact]
    public void IsValidInformationalVersion_WhenVersionContainsPrereleaseAndMetadata_ShouldReturnTrue()
    {
        // Act
        bool result = AssemblyVersionMatcher.IsValidInformationalVersion(
            "2.0.0-preview-1.0.0+abcdef");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidInformationalVersion_WhenValueIsNotAVersion_ShouldReturnFalse()
    {
        // Act
        bool result = AssemblyVersionMatcher.IsValidInformationalVersion(
            "release-version");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMajorVersionMatched_WhenExpectedVersionIsNegative_ShouldThrow()
    {
        // Arrange
        Assembly assembly = DynamicAssemblyFactory.Create("2.0.0");

        // Act
        Action act = () => assembly.IsMajorVersionMatched(-1);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Expected version level must be zero or greater.*");
    }

    [Fact]
    public void IsMajorVersionMatched_WhenInformationalVersionIsMissing_ShouldReturnFalse()
    {
        // Arrange
        Assembly assembly = DynamicAssemblyFactory.Create();

        // Act
        bool result = assembly.IsMajorVersionMatched(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AssemblyIdentityMatchers_WhenVersionMatches_ShouldReturnTrue()
    {
        // Arrange
        Assembly assembly = DynamicAssemblyFactory.Create(
            informationalVersion: "99.99.99",
            assemblyVersion: new Version(3, 4, 5, 6));

        // Act
        bool majorMatched = assembly.IsAssemblyMajorVersionMatched(3);
        bool minorMatched = assembly.IsAssemblyMinorVersionMatched(4);
        bool buildMatched = assembly.IsAssemblyBuildVersionMatched(5);

        // Assert
        majorMatched.Should().BeTrue();
        minorMatched.Should().BeTrue();
        buildMatched.Should().BeTrue();
    }
}
