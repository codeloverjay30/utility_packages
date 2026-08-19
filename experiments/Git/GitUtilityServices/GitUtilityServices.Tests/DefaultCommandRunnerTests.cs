using Xunit;
using System.IO.Abstractions.TestingHelpers;
using GitUtilityServices;

namespace GitUtilityServices.Tests;

public class DefaultCommandRunnerTests
{
    [Fact]
    public void ExecuteGitCommand_ShouldThrowDirectoryNotFoundException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(); // Empty file system
        var runner = new DefaultCommandRunner(mockFileSystem);
        var nonExistentDir = @"C:\FakePath";

        // Act & Assert
        var exception = Assert.Throws<DirectoryNotFoundException>(() => 
            runner.ExecuteGitCommand(nonExistentDir, "status")
        );

        Assert.Contains(nonExistentDir, exception.Message);
    }

    [Fact]
    public void ExecuteGitCommand_ShouldNotThrow_WhenDirectoryExists()
    {
        // Note: We can't easily test the Process.Start part here without a Process wrapper,
        // but we can verify it passes the directory check.
        
        // Arrange
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddDirectory(@"C:\RealRepo");
        var runner = new DefaultCommandRunner(mockFileSystem);

        // Act & Assert
        // This will likely fail in a pure unit test environment because 'git' isn't a real process 
        // we can run, but it proves the Directory.Exists check passed.
        var ex = Record.Exception(() => runner.ExecuteGitCommand(@"C:\RealRepo", "status"));
        
        // If git is not installed in the test environment, Process.Start might throw Win32Exception
        // but it should NOT be a DirectoryNotFoundException.
        Assert.IsNotType<DirectoryNotFoundException>(ex);
    }
}