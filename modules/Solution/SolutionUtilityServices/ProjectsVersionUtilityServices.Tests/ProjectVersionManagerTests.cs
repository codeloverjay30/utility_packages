using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;
using ProjectsVersionUtilityServices;

namespace ProjectsVersionUtilityServices.Tests;

/// <summary>
/// Contains defensive unit tests for the <see cref="ProjectVersionManager"/> class,
/// ensuring robust file system operations and proper XML content transformations.
/// </summary>
[TestFixture]
public class ProjectVersionManagerTests
{
    private MockFileSystem _fileSystem = null!;
    private ProjectVersionManager _manager = null!;

    /// <summary>
    /// Initializes the test environment before each execution by isolating the file system.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Enforce strict file system isolation via System.IO.Abstractions to prevent side effects.
        _fileSystem = new MockFileSystem();
        _manager = new ProjectVersionManager(_fileSystem);
    }

    /// <summary>
    /// Verifies that the standard project file's main Version element is successfully updated 
    /// when no explicit package name is provided.
    /// </summary>
    [Test]
    public void UpdateVersion_ShouldUpdateProjectVersionProperty_WhenPackageNameIsNull()
    {
        // Arrange
        const string projectPath = @"C:\workspace\MyProject.csproj";
        const string content = 
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <Version>1.0.0</Version>
  </PropertyGroup>
</Project>";
        _fileSystem.AddFile(projectPath, new MockFileData(content));

        // Act
        _manager.UpdateVersion(projectPath, "2.1.0");

        // Assert
        var updatedContent = _fileSystem.File.ReadAllText(projectPath);
        updatedContent.Should().Contain("<Version>2.1.0</Version>", "because the project root version should be updated");
        updatedContent.Should().NotContain("<Version>1.0.0</Version>", "because the old version must be overwritten");
    }

    /// <summary>
    /// Verifies that Central Package Management (CPM) props files are properly manipulated 
    /// and the correct PackageVersion metadata is targeted.
    /// </summary>
    [Test]
    public void UpdateVersion_ShouldUpdatePackageVersion_WhenPackageNameIsProvided()
    {
        // Arrange
        const string propsPath = @"C:\workspace\Directory.Packages.props";
        const string content = 
@"<Project>
  <ItemGroup>
    <PackageVersion Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>
</Project>";
        _fileSystem.AddFile(propsPath, new MockFileData(content));

        // Act
        _manager.UpdateVersion(propsPath, "13.0.3", "Newtonsoft.Json");

        // Assert
        var updatedContent = _fileSystem.File.ReadAllText(propsPath);
        updatedContent.Should().Contain("Include=\"Newtonsoft.Json\" Version=\"13.0.3\"", "because the specific package dependency version must be updated");
    }

  /// <summary>
  /// Verifies that if a PackageReference lacks version metadata, a VersionOverride attribute 
  /// is appended defensively to ensure local override semantics.
  /// </summary>
  [Test]
  public void UpdateVersion_ShouldAddVersionOverride_WhenPackageExistsButHasNoVersionMetadata()
  {
    // Arrange
    const string projectPath = @"C:\workspace\MyProject.csproj";
    const string content =
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""MyLib"" />
  </ItemGroup>
</Project>";
    _fileSystem.AddFile(projectPath, new MockFileData(content));

    // Act
    _manager.UpdateVersion(projectPath, "5.0.0", "MyLib");

    // Assert
    var updatedContent = _fileSystem.File.ReadAllText(projectPath);
    updatedContent.Should().Contain("VersionOverride", "because a missing standard version requires a VersionOverride attribute injection");
    updatedContent.Should().Contain("5.0.0", "because the VersionOverride attribute should have the new version value");
  }

    /// <summary>
    /// Verifies that the hierarchical directory traversal correctly bubbles up to locate 
    /// configuration files located in parent paths.
    /// </summary>
    [Test]
    public void FindConfigInAncestors_ShouldReturnFilePath_WhenFileExistsInParentDirectory()
    {
        // Arrange
        _fileSystem.AddDirectory(@"C:\workspace\Solution\Project\Sub");
        const string configPath = @"C:\workspace\Solution\Directory.Build.props";
        _fileSystem.AddFile(configPath, new MockFileData(string.Empty));

        // Act
        var result = _manager.FindConfigInAncestors(@"C:\workspace\Solution\Project\Sub", "Directory.Build.props");

        // Assert
        result.Should().Be(configPath, "because the utility must recursively traverse upward until the target file is found");
    }

    /// <summary>
    /// Verifies that if the configuration file does not exist anywhere in the directory chain, 
    /// the method returns null gracefully instead of crashing.
    /// </summary>
    [Test]
    public void FindConfigInAncestors_ShouldReturnNull_WhenFileNotFoundInAnyAncestors()
    {
        // Arrange
        _fileSystem.AddDirectory(@"C:\workspace\OnlyDir");

        // Act
        var result = _manager.FindConfigInAncestors(@"C:\workspace\OnlyDir", "NotExist.txt");

        // Assert
        result.Should().BeNull("because the configuration file does not exist in any ancestor directory");
    }

    /// <summary>
    /// Defensive Test: Ensures that attempting to update a non-existent file path 
    /// throws a strict <see cref="FileNotFoundException"/> with a clear contextual message.
    /// </summary>
    [Test]
    public void UpdateVersion_ShouldThrowFileNotFoundException_WhenTargetFileDoesNotExist()
    {
        // Arrange
        const string nonExistentPath = @"C:\workspace\NonExistentProject.csproj";

        // Act
        Action act = () => _manager.UpdateVersion(nonExistentPath, "1.0.0");

        // Assert
        act.Should().Throw<FileNotFoundException>("because file operations on missing targets must fail immediately to preserve data integrity")
           .WithMessage($"*{nonExistentPath}*");
    }

    /// <summary>
    /// Defensive Test: Ensures that trying to update a file containing malformed XML 
    /// throws a <see cref="XmlException"/> or related parsing exception, preventing corrupt write-backs.
    /// </summary>
    [Test]
    public void UpdateVersion_ShouldThrowException_WhenXmlContentIsMalformed()
    {
        // Arrange
        const string projectPath = @"C:\workspace\CorruptedProject.csproj";
        const string malformedContent = @"<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup><Version>1.0.0</Version></PropertyGroup>"; // Missing closing tag
        _fileSystem.AddFile(projectPath, new MockFileData(malformedContent));

        // Act
        Action act = () => _manager.UpdateVersion(projectPath, "2.0.0");

        // Assert
        act.Should().Throw<System.Xml.XmlException>("because the system must reject corrupt and unparsable XML inputs defensively");
    }
}