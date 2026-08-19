using Microsoft.Extensions.Logging;
using Moq;
using ProgressBarUtilityServices;
using System.Timers;
using Xunit;

public class LoggerTrackerTests
{
    [Fact]
    public void Update_ShouldLogEveryTenPercent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var tracker = new LoggerTracker("TestTask" , loggerMock.Object);

        // Act
        tracker.Update(0.11); // 11% -> 應該觸發 10% 的 Log
        tracker.Update(0.12); // 12% -> 同區間，不應重複觸發
        tracker.Update(0.25); // 25% -> 應該觸發 20% 的 Log

        // Assert
        // 驗證 LogInformation 是否被呼叫了 2 次 (針對 10% 和 20%)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information ,
                It.IsAny<EventId>() ,
                It.Is<It.IsAnyType>((v , t) => v.ToString().Contains("Progress")) ,
                null ,
                It.IsAny<Func<It.IsAnyType , Exception , string>>()) ,
            Times.Exactly(2));
    }
}
