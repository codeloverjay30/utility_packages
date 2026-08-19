using System;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FluentAssertions;
using VscUtilityServices.Core.Models;
using VscUtilityServices.Engines;
using Xunit;

namespace VscUtilityServices.Tests;

public class IsolatedCsharpExecutorTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly IsolatedCsharpExecutor _sut;

    public IsolatedCsharpExecutorTests()
    {
        _mockFileSystem = new MockFileSystem();
        _sut = new IsolatedCsharpExecutor(_mockFileSystem);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadImageFormatException_WhenTaskIsExplicitlyCorrupted()
    {
        // Arrange
        var script = new Script
        { 
            Path = @"C:\Workspace\Scripts\TestScript.cs",
            ScriptInfo = new ScriptInfo 
            {
                Id = "1",
                DisplayName = "Test",
                Description = "Desc",
                VersionInfo = new VersionInfo
                {
                    Version = "1.0",
                },
            },
            LanguageInfo = new ProgrammingLanguageInfo
            {
                Name = "csharp",
                DisplayName = "C#",
            },
        };
        
        var taskDefinition = new TaskDefinition 
        { 
            TaskName = "CorruptedCsharpTask", 
        };

        // Act
        Func<Task> act = async () => await _sut.ExecuteAsync(script, taskDefinition);

        // Assert: 嚴格執行 FluentAssertions 異常與訊息攔截驗證鐵律
        await act.Should().ThrowAsync<BadImageFormatException>()
            .WithMessage("*corrupted CLI metadata tables.*");
    }
}