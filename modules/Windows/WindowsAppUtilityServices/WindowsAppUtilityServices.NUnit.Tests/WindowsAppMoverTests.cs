using System;
using System.Collections.Generic;
using System.Diagnostics.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CommonModels;
using FluentAssertions; // 導入最高優先級斷言套件
using Moq;
using NUnit.Framework;
using WindowsAppUtilityServices.Diagnostics;

namespace WindowsAppUtilityServices.NUnit.Tests
{
    [TestFixture]
    public class WindowsAppMoverTests
    {
        private Mock<IProcessUtilityService> _mockProcessService;
        private Mock<ICommandRunner> _mockCommandRunner;
        private MockFileSystem _mockFileSystem;
        private WindowsAppMover _mover;
        private AppSettings _singleTestSetting;

        [SetUp]
        public void SetUp()
        {
            // 主動防禦：使用 Strict 嚴格模式，嚴防底層未 Setup 的行為產生平行時空副作用
            _mockProcessService = new Mock<IProcessUtilityService>(MockBehavior.Strict);
            _mockCommandRunner = new Mock<ICommandRunner>();
            _mockFileSystem = new MockFileSystem();

            // 初始化受測對象，注入 Mock 依賴 
            _mover = new WindowsAppMover(
                _mockFileSystem,
                _mockProcessService.Object,
                _mockCommandRunner.Object
            );

            _singleTestSetting = new AppSettings
            {
                ProcessName = "TestApp",
                SourcePath = @"C:\Source\App",
                TargetPath = @"D:\Target\App"
            };
        }

        [Test]
        public void MoveOneApp_Should_Kill_Process_If_Running()
        {
            // Arrange
            // 模擬檔案系統存在 
            _mockFileSystem.AddDirectory(_singleTestSetting.SourcePath);
            
            // 修正 Setup：WindowsAppMover 實際上是呼叫帶有 string 參數的 SafeKillAndExit
            _mockProcessService.Setup(procServ => procServ.SafeKillAndExit(_singleTestSetting.ProcessName));

            // Act
            // 使用 Action 封裝執行步驟，以便符合防禦性測試標準與 FluentAssertions 語法
            Action act = () => _mover.MoveOneApp(_singleTestSetting, MoveDirectoryOptions.Default);

            // Assert
            // 1. 驗證執行過程中是否無噴出任何非預期異常
            act.Should().NotThrow();

            // 2. 驗證受測對象（Mover）是否有克盡職責，將正確的 ProcessName 傳遞給程序控制服務
            _mockProcessService.Verify(procServ => procServ.SafeKillAndExit(_singleTestSetting.ProcessName), Times.Once);
        }

        [Test]
        public void MoveOneApp_Should_Return_Error_If_SourcePath_Missing()
        {
            // Arrange
            _mockProcessService.Setup(procServ => procServ.SafeKillAndExit(_singleTestSetting.ProcessName));

            // Act
            var result = _mover.MoveOneApp(_singleTestSetting, MoveDirectoryOptions.Default);

            // Assert (全面改用 FluentAssertions)
            result.IsSuccess.Should().BeFalse();
            result.Result.Should().Contain("Can't find the process");
        }

        [Test]
        public void MoveOneApp_Should_Execute_Robocopy_And_Mklink()
        {
            // Arrange
            _mockFileSystem.AddDirectory(_singleTestSetting.SourcePath);
            _mockProcessService.Setup(procServ => procServ.SafeKillAndExit(_singleTestSetting.ProcessName));

            // Act
            Action act = () => _mover.MoveOneApp(_singleTestSetting, MoveDirectoryOptions.Default);

            // Assert
            act.Should().NotThrow();
            _mockCommandRunner.Verify(c => c.ExecuteCommand(It.Is<string>(s => s.Contains("robocopy"))), Times.Once);
        }

        [Test]
        public void MoveOneApp_Catch_Exception_Should_Return_Failure_Status()
        {
            // Arrange
            _mockFileSystem.AddDirectory(_singleTestSetting.SourcePath);
            _mockProcessService.Setup(procServ => procServ.SafeKillAndExit(_singleTestSetting.ProcessName));
            
            _mockCommandRunner.Setup(c => c.ExecuteCommand(It.IsAny<string>()))
                              .Throws(new Exception("Disk Full"));

            // Act
            var result = _mover.MoveOneApp(_singleTestSetting, MoveDirectoryOptions.Default);

            // Assert
            result.OverallErrorMessage.Should().Be("Disk Full");
            result.ErrorMessage.Should().Contain("exception ocurred");
        }
    }
}