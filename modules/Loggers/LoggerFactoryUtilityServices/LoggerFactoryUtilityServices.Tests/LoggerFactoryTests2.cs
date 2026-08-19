using LoggerFactoryUtilityServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LoggerFactoryUtilityServices.Tests
{
    // 1. 建立一個專門用於測試的子類別，繼承自你的基底類別
    public class TestService : LoggerFactoryBaseUtilityService
    {
        // 暴露受保護的 Logger 屬性給測試專案使用
        public ILogger GetExposedLogger => Logger;

        // 預設建構子會自動呼叫 base()
        public TestService() : base(NullLoggerFactory.Instance,"TestApp") { }
    }

    public class LoggerFactoryBaseUtilityServiceTests2
    {
        [Fact]
        public void Logger_Initialization_ShouldUse_Current_Type_Name()
        {
            // Arrange (準備)
            var mockFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            // 預期傳入 CreateLogger 的字串應該是 TestService 的完整名稱
            string expectedCategoryName = typeof(TestService).FullName!;

            // 設定 Mock Factory：當傳入正確名稱時，回傳 Mock Logger
            mockFactory
                .Setup(f => f.CreateLogger(It.Is<string>(s => s == expectedCategoryName)))
                .Returns(mockLogger.Object);

            // 使用 C# 11 的目標類型 new 與物件初始化項 (針對 required 屬性)
            var service = new TestService
            {
                LoggerFactory = mockFactory.Object
            };

            // Act (執行)
            // 在存取屬性前，IsLoggerCreated 應該為 false (因為是 Lazy)
            bool beforeAccess = service.GetType()
                .GetProperty("IsLoggerCreated" , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(service) as bool? ?? false;

            var logger = service.GetExposedLogger; // 觸發 Lazy<ILogger>

            // Assert (驗證)
            Assert.NotNull(logger);

        }
    }
}
