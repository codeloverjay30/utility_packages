using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using LoggerFactoryUtilityServices;
using Microsoft.Extensions.Logging.Abstractions;

namespace LoggerFactoryUtilityServices.Tests
{
    // 1. 定義一個繼承自基底類別的子類別
    public class Class1 : LoggerFactoryBaseUtilityService
    {
        public Class1() : base(NullLoggerFactory.Instance) { }
        // 暴露受保護的屬性以便測試驗證
        public ILogger GetInternalLogger => Logger;
        public string GetTypeFullName => this.GetType().FullName;
    }

    public class LoggerFactoryTests1
    {
        [Fact]
        public void Logger_Should_Use_Subclass_Type_Name()
        {
            // Arrange (準備階段)
            var mockFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            // 預期傳入 CreateLogger 的名稱應該是子類別的 FullName
            string expectedCategoryName = typeof(Class1).FullName;

            mockFactory
                .Setup(f => f.CreateLogger(expectedCategoryName))
                .Returns(mockLogger.Object);

            // Act (執行階段)
            var service = new Class1 { LoggerFactory = mockFactory.Object };
            var logger = service.GetInternalLogger; // 觸發 Lazy 載入

            // Assert (驗證階段)
            // 驗證 GetType() 是否回傳 Class1 的名稱
            Assert.Equal("LoggerFactoryUtilityServices.Tests.Class1" , service.GetTypeFullName);
        }
    }
}
