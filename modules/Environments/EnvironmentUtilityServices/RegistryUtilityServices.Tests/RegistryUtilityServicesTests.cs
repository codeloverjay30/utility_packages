using Moq;
using FluentAssertions;
using Xunit;
using RegistryUtilityServices;

namespace RegistryUtilityServices.Tests;

public class RegistryServiceTests
{
    private readonly RegistryService _sut;

    public RegistryServiceTests()
    {
        _sut = new RegistryService();
    }

    /// <summary>
    /// Verifies that GetValue returns null when the registry key path is invalid.
    /// </summary>
    [Fact]
    public void GetValue_ShouldReturnNull_WhenKeyPathDoesNotExist()
    {
        // Arrange
        var invalidPath = @"SOFTWARE\Invalid\Path";

        // Act
        var result = _sut.GetValue(invalidPath, "SomeValue");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetValue returns null when the specific value name does not exist.
    /// </summary>
    [Fact]
    public void GetValue_ShouldReturnNull_WhenValueNameDoesNotExist()
    {
        // Arrange
        // Using a path that likely exists but with a non-existent value
        var path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        var nonExistentValue = "ThisValueWillNeverExist_12345";

        // Act
        var result = _sut.GetValue(path, nonExistentValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetValue_ShouldHandleSecurityException_Gracefully()
    {
        // Note: Since RegistryService directly calls Registry.LocalMachine, 
        // to strictly mock this without an abstraction over RegistryKey itself, 
        // we focus on the public contract behavior.

        // Arrange
        var path = @"HKEY_LOCAL_MACHINE\SECURITY"; // Usually restricted

        // Act
        Action act = () => _sut.GetValue(path, "AnyValue");

        // Assert
        // The service is designed to return null on SecurityException per implementation
        act.Should().NotThrow();
        _sut.GetValue(path, "AnyValue").Should().BeNull();
    }
}