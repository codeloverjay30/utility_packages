using FluentAssertions;
using FileCategorizationUtilityServices;
using Xunit;

namespace FileCategorizationUtilityServices.Tests;

public class ExcludedEntriesUtilityServiceTests
{
    private readonly ExcludedEntriesUtilityService _service = new();

    [Theory]
    [InlineData(@"\bin\", true)]
    [InlineData(@"\obj\", true)]
    [InlineData(@"\src\", false)]
    [InlineData(@"\.git\", true)]
    public void IsExcludedPath_ShouldReturnExpectedResult(string path, bool expected)
    {
        // Act
        var result = _service.IsExcludedPath(path);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("bin", true)]
    [InlineData("obj", true)]
    [InlineData(".git", true)]
    [InlineData("Source", false)]
    public void IsExcludedFolderName_ShouldReturnExpectedResult(string folderName, bool expected)
    {
        // Act
        var result = _service.IsExcludedFolderName(folderName);

        // Assert
        result.Should().Be(expected);
    }
}