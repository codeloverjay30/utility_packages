using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using CommonModels;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Options;
using FluentAssertions; // 必須引用以支援 IOptions

namespace WindowsAppUtilityServices.NUnit.Tests;

[TestFixture]
public class WindowsAppsMoverTests
{
    private Mock<IWindowsAppMover> _mockSingleMover;
    private List<AppSettings> _testSettings;
    private Mock<IOptions<List<AppSettings>>> _mockOptions;

    [SetUp]
    public void SetUp()
    {
        _mockSingleMover = new Mock<IWindowsAppMover>();
        _mockOptions = new Mock<IOptions<List<AppSettings>>>();

        _testSettings = new List<AppSettings>
            {
                new AppSettings
                {
                    ProcessName = "TestApp1",
                    SourcePath = @"C:\Source\App1",
                    TargetPath = @"D:\Target\App1"
                },
                new AppSettings
                {
                    ProcessName = "TestApp2",
                    SourcePath = @"C:\Source\App2",
                    TargetPath = @"D:\Target\App2"
                }
            };

        // 模擬 IOptions 的 Value 回傳測試設定檔 
        _mockOptions.Setup(o => o.Value).Returns(_testSettings);
    }

    [Test]
    public void MoveManyApps_Should_Call_MoveOneApp_For_Each_Setting()
    {
        // Arrange
        // 模擬 MoveOneApp 每次執行都回傳成功的狀態
        _mockSingleMover.Setup(m => m.MoveOneApp(It.IsAny<AppSettings>(), It.IsAny<MoveDirectoryOptions>()))
                        .Returns(new StatusJsonModel { IsSuccess = true });

        // 注入 Mock 服務，不再需要傳入 FileSystem 等基礎服務，因為它們已被封裝在單一 Mover 中 
        var mover = new WindowsAppsMover(_mockSingleMover.Object, _mockOptions.Object);

        // Act
        var result = mover.MoveManyApps(MoveDirectoryOptions.Default);

        // Assert
        // 驗證是否真的執行了兩次 (對應 _testSettings 的數量)
        _mockSingleMover.Verify(m => m.MoveOneApp(
            It.IsAny<AppSettings>(),
            It.IsAny<MoveDirectoryOptions>()),
            Times.Exactly(2));

        result.StatusList.Should().HaveCount(2);
        result.IsAllSuccess.Should().BeTrue();

        // Assert.That(result.StatusList.Count, Is.EqualTo(2));
        // Assert.That(result.IsAllSuccess, Is.True);
    }

    [Test]
    public void MoveManyApps_WithEmptySettings_ShouldReturnNoStatus()
    {
        // Arrange
        _mockOptions.Setup(o => o.Value).Returns(new List<AppSettings>());
        var mover = new WindowsAppsMover(_mockSingleMover.Object, _mockOptions.Object);

        // Act
        var result = mover.MoveManyApps();

        // Assert
        result.StatusList.Should().BeEmpty();
        result.HasNoneStatus.Should().BeTrue();
        // Assert.That(result.StatusList, Is.Empty);
        // // 根據您的 CommonModels，如果是空列表應回傳 HasNoneStatus 為 True
        // Assert.That(result.HasNoneStatus, Is.True);
    }
}
    