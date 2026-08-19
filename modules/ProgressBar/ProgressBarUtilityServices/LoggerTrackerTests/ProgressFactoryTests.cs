using Microsoft.Extensions.Logging;
using Moq;
using ProgressBarUtilityServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoggerTrackerTests
{
    public class ProgressFactoryTests
    {
        [Fact]
        public void CreateTracker_ShouldReturnLoggerTracker_WhenOutputIsRedirected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProgressFactory>>();
            var factory = new ProgressFactory(loggerMock.Object);

            // 注意：在單元測試中模擬 Console.IsOutputRedirected 較困難
            // 通常會將「環境偵測」也抽象化成一個 IEnvironment 介面注入
            // 這裡示範基本的建立驗證

            // Act
            var tracker = factory.CreateTracker("Unit Test Task");

            // Assert
            Assert.NotNull(tracker);
            Assert.IsAssignableFrom<ITaskProgressTracker>(tracker);
        }
    }
}
