using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using FileCategorizationUtilityServices;
using Xunit;

namespace FileCategorizationUtilityServices.Tests;

public class PathExtensionsTests
{
    [Fact]
    public void GetFileExtension_ShouldReturnCorrectExtensionWithMock()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var path = @"C:\data\config.json";

        // Act
        var ext = path.GetFileExtension(fileSystem);

        // Assert
        ext.Should().Be(".json");
    }

    [Fact]
    public void IsOneOf_ShouldReturnTrue_WhenExtensionExistsInSet()
    {
        // Arrange
        var exts = new HashSet<string> { ".cs", ".txt" };
        var target = ".cs";

        // Act
        var result = target.IsOneOf(exts);

        // Assert
        result.Should().BeTrue();
    }
}