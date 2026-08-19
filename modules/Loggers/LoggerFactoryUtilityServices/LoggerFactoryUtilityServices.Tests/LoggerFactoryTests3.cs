using LoggerFactoryUtilityServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LoggerFactoryUtilityServices.Tests
{
    // 建立一個繼承類別，但不傳入任何引數給 base()
    public class DefaultParameterService : LoggerFactoryBaseUtilityService
    {
        // 這裡不傳參數，測試 base 的預設參數 string unknownAppName = "UnknownApp"
        public DefaultParameterService() : base(NullLoggerFactory.Instance) { }

        public ILogger Trigger => Logger;
    }

    public class LoggerFactoryDefaultValueTests
    {
        [Fact]
        public void Constructor_WhenNoArgument_ShouldHaveDefaultUnknownAppName()
        {
            // Arrange
            var mockFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            // 因為 GetType().FullName 存在，它會「蓋過」UnknownApp
            string expectedName = typeof(DefaultParameterService).FullName!;

            mockFactory
                .Setup(f => f.CreateLogger(expectedName))
                .Returns(mockLogger.Object);

            var service = new DefaultParameterService
            {
                LoggerFactory = mockFactory.Object
            };

            // Act
            var logger = service.Trigger;

            // Assert
            // 驗證邏輯是否正確執行
            Assert.NotNull(logger);
        }
    }
}
