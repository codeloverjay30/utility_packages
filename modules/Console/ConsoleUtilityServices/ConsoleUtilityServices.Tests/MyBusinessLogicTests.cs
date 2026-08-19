using Moq;
using System.Timers;
using Xunit;

namespace ConsoleUtilityServices.Tests
{


    public class MyBusinessLogicTests
    {
        [Fact]
        public void Process_ShouldNotPrint_WhenConsoleIsUnavailable()
        {
            // Arrange: 模擬一個「無法使用 Console」的環境
            var mockConsole = new Mock<IConsoleService>();
            mockConsole.Setup(m => m.CanUseConsole()).Returns(false);

            // 假設你有一個處理邏輯需要用到 ConsoleService
            var processor = new DataProcessor(mockConsole.Object);

            // Act
            processor.Execute();

            // Assert: 驗證 WriteLine 從未被呼叫（因為環境不允許）
            mockConsole.Verify(m => m.WriteLine(It.IsAny<string>()) , Times.Never());
        }
    }

    public class DataProcessor
    {
        private readonly IConsoleService _consoleService;

        public DataProcessor(
            IConsoleService consoleService
        )
        {
            this._consoleService = consoleService;
        }

        public void Execute()
        {
            if (_consoleService.CanUseConsole())
            {
                _consoleService.WriteLine("Hello World");
            }
        }
    }
}
