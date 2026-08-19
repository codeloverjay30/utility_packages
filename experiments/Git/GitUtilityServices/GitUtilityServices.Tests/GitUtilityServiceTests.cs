using Moq;
using Xunit.v3;
using System.IO.Abstractions.TestingHelpers;
using GitUtilityServices;
using Xunit;

namespace GitUtilityServices.Tests;

public class GitUtilityServiceTests
{
    [Fact]
    public void CheckModules_ShouldCallExecuteGitCommand_ForEverySubdirectory()
    {
        // Arrange
        var mockCommandRunner = new Mock<ICommandRunner>();
        
        // Setup a mock file system with some directories
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"C:\projects\repo\ProjectA\readme.txt", new MockFileData("content") },
            { @"C:\projects\repo\ProjectB\readme.txt", new MockFileData("content") }
        });

        var service = new GitUtilityService(mockCommandRunner.Object, mockFileSystem);
        var rootPath = @"C:\projects\repo";

        // Act
        service.CheckModules(rootPath);

        // Assert
        // Verify ExecuteGitCommand was called exactly once for each subdirectory
        mockCommandRunner.Verify(x => 
            x.ExecuteGitCommand(@"C:\projects\repo\ProjectA", "status -s"), Times.Once);
            
        mockCommandRunner.Verify(x => 
            x.ExecuteGitCommand(@"C:\projects\repo\ProjectB", "status -s"), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCommandRunnerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GitUtilityService(null!));
    }
}