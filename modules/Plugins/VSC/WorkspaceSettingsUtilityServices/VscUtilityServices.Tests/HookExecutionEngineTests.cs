using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WorkspaceUtility.Core.Services;
using Xunit;

namespace WorkspaceUtility.Tests;

/// <summary>
/// Comprehensive defense testing suite validating the transactional lifecycles of the <see cref="HookExecutionEngine"/>.
/// </summary>
public class HookExecutionEngineTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly Mock<ITaskDispatcher> _mockTaskDispatcher;
    private readonly HookExecutionEngine _sut;

    /// <summary>
    /// Initializes a sterile testing environment avoiding cross-talk or Moq dynamic defaults side-effects.
    /// </summary>
    public HookExecutionEngineTests()
    {
        _mockFileSystem = new MockFileSystem();
        // Strict behavior enforces exact predictability criteria across execution boundaries.
        _mockTaskDispatcher = new Mock<ITaskDispatcher>(MockBehavior.Strict);
        _sut = new HookExecutionEngine(_mockFileSystem, _mockTaskDispatcher.Object);
    }

    [Fact]
    public async Task ProcessHookConfigurationAsync_ShouldThrowFileNotFoundException_WhenConfigPathDoesNotExist()
    {
        // Arrange
        string nonExistentPath = @"C:\Inaccessible\.vscode\settings.json5";

        // Act
        Func<Task> act = async () => await _sut.ProcessHookConfigurationAsync(nonExistentPath, @"C:\Workspace", "vsc-workspace-onentered");

        // Assert: Enforcing pure FluentAssertions instead of native xUnit Assert.Throws
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*could not be located.*");
    }

    [Fact]
    public async Task ProcessHookConfigurationAsync_ShouldSuccessfullyWithFallbackVersion_WhenVersionMetadataIsMissing()
    {
        // Arrange
        string configPath = @"C:\Workspace\.vscode\settings.json5";
        string workspacePath = @"C:\Workspace";
        
        string jsonWithoutVersion = """
            {
                "hooks": [
                    {
                        "target": {
                            "name": "fallback-test",
                            "on": ["vsc-workspace-onentered"],
                            "tasks": [
                                {
                                    "task-name": "MethodWithNoVersion",
                                    "programming-languages": "python"
                                }
                            ]
                        }
                    }
                ]
            }
            """;
            
        _mockFileSystem.AddFile(configPath, new MockFileData(jsonWithoutVersion));

        // Mitigation of Strict Mock exceptions: Precisely matching the fallback signature sequence
        _mockTaskDispatcher.Setup(d => d.ExecuteTaskDefensivelyAsync(
            workspacePath,
            "MethodWithNoVersion",
            "python",
            "0.0"
        )).Returns(Task.CompletedTask).Verifiable();

        // Act
        Func<Task> act = async () => await _sut.ProcessHookConfigurationAsync(configPath, workspacePath, "vsc-workspace-onentered");

        // Assert
        await act.Should().NotThrowAsync();
        _mockTaskDispatcher.Verify(d => d.ExecuteTaskDefensivelyAsync(workspacePath, "MethodWithNoVersion", "python", "0.0"), Times.Once);
    }

    [Fact]
    public async Task ProcessHookConfigurationAsync_ShouldThrowJsonException_WhenPayloadIsMalformed()
    {
        // Arrange
        string configPath = @"C:\Workspace\.vscode\settings.json5";
        string malformedJson5 = "{ 'hooks': [ { 'target': { 'name': 'broken' "; 
        _mockFileSystem.AddFile(configPath, new MockFileData(malformedJson5));

        // Act
        Func<Task> act = async () => await _sut.ProcessHookConfigurationAsync(configPath, @"C:\Workspace", "vsc-workspace-onentered");

        // Assert
        await act.Should().ThrowAsync<JsonException>()
            .WithMessage("*Failed to safely deserialize the JSON5 workspace hook payload*");
    }
}