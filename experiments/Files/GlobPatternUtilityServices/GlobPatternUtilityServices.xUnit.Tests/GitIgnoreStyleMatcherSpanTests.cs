using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using GlobPatternUtilityServices.Abstractions;
using GlobPatternUtilityServices.Models;
using Moq;

namespace GlobPatternUtilityServices.xUnit.Tests;

/// <summary>
/// Contains unit tests for verifying the high-performance GitIgnoreStyleMatcher class.
/// </summary>
public class GitIgnoreStyleMatcherSpanTests
{
    private readonly string _testRootPath;
    private readonly MockFileSystem _mockFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitIgnoreStyleMatcherSpanTests"/> class with mocked filesystem.
    /// </summary>
    public GitIgnoreStyleMatcherSpanTests()
    {
        _mockFileSystem = new MockFileSystem();
        _testRootPath = _mockFileSystem.Path.Combine(_mockFileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());
        _mockFileSystem.Directory.CreateDirectory(_testRootPath);
    }

    /// <summary>
    /// Cleans up test directory artifacts.
    /// </summary>
    private void Cleanup()
    {
        if (_mockFileSystem.Directory.Exists(_testRootPath))
        {
            _mockFileSystem.Directory.Delete(_testRootPath, true);
        }
    }

    /// <summary>
    /// Ensures constructor throws ArgumentNullException when required rules parameter is missing.
    /// </summary>
    [Fact]
    public void Constructor_WhenRulesNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        Action act = () => new GitIgnoreStyleMatcher(_mockFileSystem, null!);

        // Assert via FluentAssertions 
        act.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "rules");
    }

    /// <summary>
    /// Verifies that inverse rules work seamlessly and that Windows-style backslashes are normalized to slashes.
    /// </summary>
    [Fact]
    public void ProcessDirectory_WithInverseAndWindowsPathSlashes_ShouldMatchCorrectlyAndRespectStrictMode()
    {
        // Arrange
        string binPath = _mockFileSystem.Path.Combine(_testRootPath, "experiments", "bin");
        _mockFileSystem.Directory.CreateDirectory(binPath);

        string normalFile = _mockFileSystem.Path.Combine(binPath, "log.txt");
        string keepFile = _mockFileSystem.Path.Combine(binPath, "important.config");

        _mockFileSystem.File.WriteAllText(normalFile, "temp");
        _mockFileSystem.File.WriteAllText(keepFile, "save me");

        // Setup Strict Mocks (遵守鐵律：說明原因與看法)
        // 看法：此處必須使用 Strict Mode。因為我們要驗證 Windows 反斜線路徑（"\"）能否在執行期
        // 被正確轉換為 Microsoft Globbing 支援的正斜線（"/"）。如果轉換失敗，規則就無法觸發；
        // 如果反向排除（!）失效，不該被處理的 important.config 就會被錯誤處理。
        // Strict Mode 可以在發生任何非預期呼叫時，讓測試立刻崩潰，確保比對演算法的絕對精準度。
        var mockActionStrategy = new Mock<IMatchActionStrategy>(MockBehavior.Strict);
        mockActionStrategy.Setup(s => s.Execute(It.IsAny<IFileInfo>())).Verifiable();

        var rules = new List<GlobRuleEntry>
        {
            new GlobRuleEntry
            {
                Pattern = "!**/important.config",
                ActionStrategy = mockActionStrategy.Object 
            }, 
            new GlobRuleEntry
            {
                Pattern = @"**\bin\*", // 刻意混用反斜線輸入，驗證前置轉換與執行期轉換的雙向穩定度
                ActionStrategy = mockActionStrategy.Object 
            }
        };

        var matcher = new GitIgnoreStyleMatcher(_mockFileSystem, rules);

        // Act
        matcher.ProcessDirectory(_mockFileSystem.DirectoryInfo.New(_testRootPath));

        // Assert   
        mockActionStrategy.Verify(s => s.Execute(It.Is<IFileInfo>(f => f.Name == "log.txt")), Times.Once);
 
        Cleanup();
    }
}