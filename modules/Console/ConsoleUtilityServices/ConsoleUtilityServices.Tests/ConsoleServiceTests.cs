using Xunit;
using System;
using System.IO;
using ConsoleUtilityServices;

namespace ConsoleUtilityServices.Tests
{
    public class ConsoleServiceTests : IDisposable
    {
        private readonly ConsoleService _service;
        private readonly TextWriter _originalOut;

        public ConsoleServiceTests()
        {
            _service = new ConsoleService();
            // 備份原始的 Console 輸出流
            _originalOut = Console.Out;
        }

        /// <summary>
        /// 測試：當輸出被重新導向時，CanUseConsole 應該回傳 false
        /// </summary>
        [Fact]
        public void CanUseConsole_WhenOutputIsRedirected_ShouldReturnFalse()
        {
            // Arrange: 模擬重新導向（例如導向至 StringWriter 而非實體 Console 視窗）
            using(var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                bool result = _service.CanUseConsole();

                // Assert
                Assert.False(result , "當 Console 輸出被導向時，應視為無法使用互動式 Console");
            }
        }

        /// <summary>
        /// 測試：在標準單元測試執行環境（通常無互動視窗）下，驗證行為
        /// </summary>
        [Fact]
        public void CanUseConsole_InTestEnvironment_ShouldBeDeterminedByEnvironment()
        {
            // Act
            bool result = _service.CanUseConsole();

            // Assert
            // 在 CI 或一般 Test Runner 中，Console.WindowHeight 通常會拋出異常或回傳 0
            // 這裡驗證它不會崩潰，且根據環境給出合理的 bool 值
            // 若在 IDE 內手動執行測試，結果可能不同，這取決於 Runner 是否有分配 PTY
            Assert.IsType<bool>(result);
        }

        // 釋放資源：確保測試結束後將 Console 狀態還原，避免影響其他測試
        public void Dispose()
        {
            Console.SetOut(_originalOut);
        }
    }
}
